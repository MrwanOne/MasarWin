namespace Masar.Infrastructure.Seed;

/// <summary>
/// نصوص SQL لإنشاء Functions وStored Procedures وTriggers في Oracle.
/// يُستدعى مرة واحدة عند بدء التطبيق عبر DatabaseProceduresInitializer.
/// </summary>
internal static class SqlScripts
{
    // ═══════════════════════════════════════════════════════════════
    // FUNCTIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// تحسب الدرجة النهائية كمتوسط درجة المشرف ودرجة اللجنة.
    /// تُستخدم داخل SP_SAVE_EVALUATION وTRG_CALC_FINAL_SCORE.
    /// </summary>
    public const string FN_CALC_FINAL_SCORE = @"
CREATE OR REPLACE FUNCTION FN_CALC_FINAL_SCORE(
    p_supervisor_score IN NUMBER,
    p_committee_score  IN NUMBER
) RETURN NUMBER AS
BEGIN
    RETURN ROUND((p_supervisor_score + p_committee_score) / 2, 2);
END FN_CALC_FINAL_SCORE";

    /// <summary>
    /// تُرجع عدد المشاريع النشطة التي يشرف عليها الدكتور.
    /// تستبدل منطق EnsureSupervisionCapacity في ProjectService الذي كان يجلب كل المشاريع.
    /// </summary>
    public const string FN_GET_SUPERVISOR_PROJECT_COUNT = @"
CREATE OR REPLACE FUNCTION FN_GET_SUPERVISOR_PROJECT_COUNT(
    p_supervisor_id IN NUMBER
) RETURN NUMBER AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM ""project""
    WHERE ""supervisor_id"" = p_supervisor_id
      AND ""is_deleted""    = 0
      AND ""status""        NOT IN (3, 4); -- تجاهل المكتملة (3) والمرفوضة (4)
    RETURN v_count;
END FN_GET_SUPERVISOR_PROJECT_COUNT";

    /// <summary>
    /// تتحقق إن كان دكتور معين عضواً في لجنة معينة.
    /// تُرجع 1 إذا كان عضواً (تعارض)، 0 إذا لم يكن.
    /// </summary>
    public const string FN_SUPERVISOR_IS_COMMITTEE_MEMBER = @"
CREATE OR REPLACE FUNCTION FN_SUPERVISOR_IS_COMMITTEE_MEMBER(
    p_doctor_id    IN NUMBER,
    p_committee_id IN NUMBER
) RETURN NUMBER AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM ""committee_member""
    WHERE ""doctor_id""    = p_doctor_id
      AND ""committee_id"" = p_committee_id;
    RETURN CASE WHEN v_count > 0 THEN 1 ELSE 0 END;
END FN_SUPERVISOR_IS_COMMITTEE_MEMBER";

    // ═══════════════════════════════════════════════════════════════
    // STORED PROCEDURES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// تقبل مشروعاً وتعيّن مشرفاً وتسجل السجل في ProjectStatusHistory.
    /// تستبدل 4 استعلامات منفصلة في ProjectService.AcceptAsync.
    /// p_result_code: 0=نجاح، 1=خطأ.
    /// </summary>
    public const string SP_ACCEPT_PROJECT = @"
CREATE OR REPLACE PROCEDURE SP_ACCEPT_PROJECT(
    p_project_id     IN  ""project"".""project_id""%TYPE,
    p_supervisor_id  IN  ""project"".""supervisor_id""%TYPE,
    p_changed_by     IN  NUMBER,
    p_result_code    OUT NUMBER,
    p_result_msg     OUT VARCHAR2
) AS
    v_old_status  NUMBER;
    v_dept_id     NUMBER;
    v_sup_dept    NUMBER;
    v_sup_count   NUMBER;
    v_max_sup     NUMBER;
BEGIN
    -- جلب بيانات المشروع
    BEGIN
        SELECT ""status"", ""department_id""
        INTO   v_old_status, v_dept_id
        FROM   ""project""
        WHERE  ""project_id"" = p_project_id AND ""is_deleted"" = 0;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            p_result_code := 1;
            p_result_msg  := 'المشروع غير موجود';
            RETURN;
    END;

    -- التحقق: المشروع في حالة مقترح (0)؟
    IF v_old_status != 0 THEN
        p_result_code := 1;
        p_result_msg  := 'لا يمكن قبول مشروع ليس في حالة مقترح';
        RETURN;
    END IF;

    -- التحقق من المشرف
    IF p_supervisor_id IS NOT NULL AND p_supervisor_id > 0 THEN
        BEGIN
            SELECT ""department_id"", ""max_supervision_count""
            INTO   v_sup_dept, v_max_sup
            FROM   ""doctor""
            WHERE  ""doctor_id"" = p_supervisor_id AND ""is_deleted"" = 0;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                p_result_code := 1;
                p_result_msg  := 'المشرف غير موجود';
                RETURN;
        END;

        -- التحقق: المشرف في نفس القسم؟
        IF v_sup_dept != v_dept_id THEN
            p_result_code := 1;
            p_result_msg  := 'المشرف لا ينتمي لقسم المشروع';
            RETURN;
        END IF;

        -- التحقق: لم تجاوز الطاقة الاستيعابية للمشرف؟
        IF v_max_sup > 0 THEN
            v_sup_count := FN_GET_SUPERVISOR_PROJECT_COUNT(p_supervisor_id);
            IF v_sup_count >= v_max_sup THEN
                p_result_code := 1;
                p_result_msg  := 'المشرف وصل للحد الأقصى للإشراف (' || v_max_sup || ' مشاريع)';
                RETURN;
            END IF;
        END IF;
    END IF;

    -- تحديث المشروع
    UPDATE ""project""
    SET ""status""        = 1,
        ""supervisor_id"" = p_supervisor_id,
        ""approved_at""   = SYSTIMESTAMP
    WHERE ""project_id"" = p_project_id;

    -- تسجيل السجل في ProjectStatusHistory
    INSERT INTO ""project_status_history""
        (""project_id"", ""old_status"", ""new_status"", ""changed_by_user_id"", ""change_reason"", ""changed_at"")
    VALUES
        (p_project_id, v_old_status, 1, p_changed_by, 'تم القبول وتعيين المشرف', SYSTIMESTAMP);

    COMMIT;
    p_result_code := 0;
    p_result_msg  := 'تم قبول المشروع بنجاح';

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result_code := 1;
        p_result_msg  := 'خطأ غير متوقع: ' || SQLERRM;
END SP_ACCEPT_PROJECT";

    /// <summary>
    /// تحفظ تقييم المناقشة وتحسب الدرجة النهائية عبر FN_CALC_FINAL_SCORE،
    /// ثم تحدّث حالة المشروع المرتبط إلى "منتهي" (3) تلقائياً.
    /// تستبدل منطق SaveEvaluationAsync في DiscussionService.
    /// </summary>
    public const string SP_SAVE_EVALUATION = @"
CREATE OR REPLACE PROCEDURE SP_SAVE_EVALUATION(
    p_discussion_id    IN  NUMBER,
    p_supervisor_score IN  NUMBER,
    p_committee_score  IN  NUMBER,
    p_report_text      IN  VARCHAR2,
    p_result_code      OUT NUMBER,
    p_result_msg       OUT VARCHAR2
) AS
    v_team_id  NUMBER;
    v_final    NUMBER;
BEGIN
    -- التحقق من وجود المناقشة
    BEGIN
        SELECT ""team_id"" INTO v_team_id
        FROM   ""discussion""
        WHERE  ""discussion_id"" = p_discussion_id AND ""is_deleted"" = 0;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            p_result_code := 1;
            p_result_msg  := 'المناقشة غير موجودة';
            RETURN;
    END;

    -- التحقق من صحة الدرجات
    IF p_supervisor_score < 0 OR p_supervisor_score > 100 THEN
        p_result_code := 1;
        p_result_msg  := 'درجة المشرف يجب أن تكون بين 0 و 100';
        RETURN;
    END IF;
    IF p_committee_score < 0 OR p_committee_score > 100 THEN
        p_result_code := 1;
        p_result_msg  := 'درجة اللجنة يجب أن تكون بين 0 و 100';
        RETURN;
    END IF;

    -- حساب الدرجة النهائية عبر الـ Function المخصصة
    v_final := FN_CALC_FINAL_SCORE(p_supervisor_score, p_committee_score);

    -- تحديث المناقشة
    UPDATE ""discussion""
    SET ""supervisor_score"" = p_supervisor_score,
        ""committee_score""  = p_committee_score,
        ""final_score""      = v_final,
        ""report_text""      = p_report_text
    WHERE ""discussion_id"" = p_discussion_id;

    -- تحديث المشروع المرتبط بالفريق إلى حالة "منتهي" (3)
    UPDATE ""project""
    SET ""status""          = 3,
        ""completion_rate"" = 100
    WHERE ""team_id"" = v_team_id AND ""is_deleted"" = 0;

    COMMIT;
    p_result_code := 0;
    p_result_msg  := 'تم تسجيل التقييم، الدرجة النهائية: ' || v_final;

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result_code := 1;
        p_result_msg  := 'خطأ غير متوقع: ' || SQLERRM;
END SP_SAVE_EVALUATION";

    // ═══════════════════════════════════════════════════════════════
    // TRIGGERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// يمنع إضافة دكتور كعضو في لجنة إذا كان هو المشرف على مشروع
    /// تناقشه هذه اللجنة — يمنع تعارض الأدوار.
    /// </summary>
    public const string TRG_NO_SUPERVISOR_IN_OWN_COMMITTEE = @"
CREATE OR REPLACE TRIGGER TRG_NO_SUPERVISOR_IN_OWN_COMMITTEE
BEFORE INSERT ON ""committee_member""
FOR EACH ROW
DECLARE
    v_conflict_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_conflict_count
    FROM ""project""    p
    JOIN ""team""       t ON p.""team_id""      = t.""team_id""
    JOIN ""discussion"" d ON d.""committee_id"" = :NEW.""committee_id""
                        AND d.""team_id""      = t.""team_id""
    WHERE p.""supervisor_id"" = :NEW.""doctor_id""
      AND p.""is_deleted""    = 0;

    IF v_conflict_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20001,
            'لا يمكن للمشرف أن يكون عضواً في لجنة تناقش أحد مشاريعه');
    END IF;
END TRG_NO_SUPERVISOR_IN_OWN_COMMITTEE";

    /// <summary>
    /// يحسب FinalScore تلقائياً عند INSERT/UPDATE على جدول discussion
    /// مما يضمن صحة الحقل دائماً بصرف النظر عن مصدر التحديث.
    /// </summary>
    public const string TRG_CALC_FINAL_SCORE = @"
CREATE OR REPLACE TRIGGER TRG_CALC_FINAL_SCORE
BEFORE INSERT OR UPDATE OF ""supervisor_score"", ""committee_score"" ON ""discussion""
FOR EACH ROW
BEGIN
    :NEW.""final_score"" := FN_CALC_FINAL_SCORE(:NEW.""supervisor_score"", :NEW.""committee_score"");
END TRG_CALC_FINAL_SCORE";
}
