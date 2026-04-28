using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

class Program
{
    static void Main()
    {
        string connStr = "User Id=MASAR;Password=masar;Data Source=192.168.56.101:1521/freepdb1";
        using var conn = new OracleConnection(connStr);
        try
        {
            conn.Open();
            Console.WriteLine("Connected!");
            
            string[] scripts = new string[] {
                @"CREATE OR REPLACE FUNCTION FN_CALC_FINAL_SCORE(
    p_supervisor_score IN NUMBER,
    p_committee_score  IN NUMBER
) RETURN NUMBER AS
BEGIN
    RETURN ROUND((p_supervisor_score + p_committee_score) / 2, 2);
END FN_CALC_FINAL_SCORE;",

                @"CREATE OR REPLACE FUNCTION FN_GET_SUPERVISOR_PROJECT_COUNT(
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
END FN_GET_SUPERVISOR_PROJECT_COUNT;",

                @"CREATE OR REPLACE FUNCTION FN_SUPERVISOR_IS_COMMITTEE_MEMBER(
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
END FN_SUPERVISOR_IS_COMMITTEE_MEMBER;",

                @"CREATE OR REPLACE FUNCTION FN_GET_STUDENT_COUNT_BY_DEPT(
    p_department_id IN NUMBER
) RETURN NUMBER AS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count
    FROM ""student""
    WHERE ""department_id"" = p_department_id
      AND ""is_deleted""    = 0;
    RETURN v_count;
END FN_GET_STUDENT_COUNT_BY_DEPT;",

                @"CREATE OR REPLACE FUNCTION FN_STUDENT_HAS_TEAM(
    p_student_id IN NUMBER
) RETURN NUMBER AS
    v_team_id NUMBER;
BEGIN
    SELECT ""team_id"" INTO v_team_id
    FROM ""student""
    WHERE ""student_id"" = p_student_id
      AND ""is_deleted"" = 0;
    RETURN CASE WHEN v_team_id IS NOT NULL THEN 1 ELSE 0 END;
EXCEPTION
    WHEN NO_DATA_FOUND THEN RETURN 0;
END FN_STUDENT_HAS_TEAM;",

                @"CREATE OR REPLACE PROCEDURE SP_ACCEPT_PROJECT(
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
END SP_ACCEPT_PROJECT;",

                @"CREATE OR REPLACE PROCEDURE SP_SAVE_EVALUATION(
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

    -- تحديث المشروع المرتبط بالفريق 3 إلى حالة منتهي
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
END SP_SAVE_EVALUATION;",

                @"CREATE OR REPLACE PROCEDURE SP_ADD_STUDENT(
    p_student_number  IN  ""student"".""student_number""%TYPE,
    p_full_name       IN  ""student"".""full_name""%TYPE,
    p_gender          IN  ""student"".""gender""%TYPE,
    p_email           IN  ""student"".""email""%TYPE,
    p_phone           IN  ""student"".""phone""%TYPE,
    p_gpa             IN  ""student"".""gpa""%TYPE,
    p_level           IN  ""student"".""level""%TYPE,
    p_status          IN  ""student"".""status""%TYPE,
    p_department_id   IN  ""student"".""department_id""%TYPE,
    p_enrollment_year IN  ""student"".""enrollment_year""%TYPE,
    p_created_by      IN  NUMBER,
    p_result_code     OUT NUMBER,
    p_result_msg      OUT VARCHAR2,
    p_new_student_id  OUT NUMBER
) AS
    v_dept_count  NUMBER;
    v_num_count   NUMBER;
    v_email_count NUMBER;
BEGIN
    -- التحقق من وجود القسم
    SELECT COUNT(*) INTO v_dept_count
    FROM ""department""
    WHERE ""department_id"" = p_department_id AND ""is_deleted"" = 0;

    IF v_dept_count = 0 THEN
        p_result_code    := 1;
        p_result_msg     := 'القسم المحدد غير موجود';
        p_new_student_id := NULL;
        RETURN;
    END IF;

    -- التحقق من تفرّد رقم الطالب
    SELECT COUNT(*) INTO v_num_count
    FROM ""student""
    WHERE ""student_number"" = p_student_number AND ""is_deleted"" = 0;

    IF v_num_count > 0 THEN
        p_result_code    := 1;
        p_result_msg     := 'رقم الطالب ' || p_student_number || ' مستخدم مسبقاً';
        p_new_student_id := NULL;
        RETURN;
    END IF;

    -- التحقق من تفرّد البريد الإلكتروني (اختياري)
    IF p_email IS NOT NULL THEN
        SELECT COUNT(*) INTO v_email_count
        FROM ""student""
        WHERE ""email"" = p_email AND ""is_deleted"" = 0;

        IF v_email_count > 0 THEN
            p_result_code    := 1;
            p_result_msg     := 'البريد الإلكتروني ' || p_email || ' مستخدم مسبقاً';
            p_new_student_id := NULL;
            RETURN;
        END IF;
    END IF;

    -- إدراج الطالب
    INSERT INTO ""student"" (
        ""student_number"", ""full_name"", ""gender"", ""email"", ""phone"",
        ""gpa"", ""level"", ""status"", ""department_id"", ""enrollment_year"",
        ""created_at"", ""created_by_user_id"", ""is_deleted""
    ) VALUES (
        p_student_number, p_full_name, p_gender, p_email, p_phone,
        p_gpa, p_level, p_status, p_department_id, p_enrollment_year,
        SYSTIMESTAMP, p_created_by, 0
    ) RETURNING ""student_id"" INTO p_new_student_id;

    COMMIT;
    p_result_code := 0;
    p_result_msg  := 'تم إضافة الطالب بنجاح برقم: ' || p_new_student_id;

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result_code    := 1;
        p_result_msg     := 'خطأ غير متوقع: ' || SQLERRM;
        p_new_student_id := NULL;
END SP_ADD_STUDENT;",

                @"CREATE OR REPLACE PROCEDURE SP_UPDATE_STUDENT(
    p_student_id      IN  ""student"".""student_id""%TYPE,
    p_student_number  IN  ""student"".""student_number""%TYPE,
    p_full_name       IN  ""student"".""full_name""%TYPE,
    p_gender          IN  ""student"".""gender""%TYPE,
    p_email           IN  ""student"".""email""%TYPE,
    p_phone           IN  ""student"".""phone""%TYPE,
    p_gpa             IN  ""student"".""gpa""%TYPE,
    p_level           IN  ""student"".""level""%TYPE,
    p_status          IN  ""student"".""status""%TYPE,
    p_department_id   IN  ""student"".""department_id""%TYPE,
    p_enrollment_year IN  ""student"".""enrollment_year""%TYPE,
    p_updated_by      IN  NUMBER,
    p_result_code     OUT NUMBER,
    p_result_msg      OUT VARCHAR2
) AS
    v_student_count NUMBER;
    v_dept_count    NUMBER;
    v_num_count     NUMBER;
    v_email_count   NUMBER;
BEGIN
    -- التحقق من وجود الطالب
    SELECT COUNT(*) INTO v_student_count
    FROM ""student""
    WHERE ""student_id"" = p_student_id AND ""is_deleted"" = 0;

    IF v_student_count = 0 THEN
        p_result_code := 1;
        p_result_msg  := 'الطالب غير موجود';
        RETURN;
    END IF;

    -- التحقق من وجود القسم
    SELECT COUNT(*) INTO v_dept_count
    FROM ""department""
    WHERE ""department_id"" = p_department_id AND ""is_deleted"" = 0;

    IF v_dept_count = 0 THEN
        p_result_code := 1;
        p_result_msg  := 'القسم المحدد غير موجود';
        RETURN;
    END IF;

    -- التحقق من تفرّد رقم الطالب (باستثناء نفس الطالب)
    SELECT COUNT(*) INTO v_num_count
    FROM ""student""
    WHERE ""student_number"" = p_student_number
      AND ""student_id""     != p_student_id
      AND ""is_deleted""     = 0;

    IF v_num_count > 0 THEN
        p_result_code := 1;
        p_result_msg  := 'رقم الطالب ' || p_student_number || ' مستخدم من طالب آخر';
        RETURN;
    END IF;

    -- التحقق من تفرّد البريد الإلكتروني (باستثناء نفس الطالب)
    IF p_email IS NOT NULL THEN
        SELECT COUNT(*) INTO v_email_count
        FROM ""student""
        WHERE ""email""      = p_email
          AND ""student_id"" != p_student_id
          AND ""is_deleted"" = 0;

        IF v_email_count > 0 THEN
            p_result_code := 1;
            p_result_msg  := 'البريد الإلكتروني ' || p_email || ' مستخدم من طالب آخر';
            RETURN;
        END IF;
    END IF;

    -- تحديث بيانات الطالب
    UPDATE ""student"" SET
        ""student_number""  = p_student_number,
        ""full_name""        = p_full_name,
        ""gender""           = p_gender,
        ""email""            = p_email,
        ""phone""            = p_phone,
        ""gpa""              = p_gpa,
        ""level""            = p_level,
        ""status""           = p_status,
        ""department_id""    = p_department_id,
        ""enrollment_year""  = p_enrollment_year,
        ""updated_at""       = SYSTIMESTAMP,
        ""updated_by_user_id"" = p_updated_by
    WHERE ""student_id"" = p_student_id;

    COMMIT;
    p_result_code := 0;
    p_result_msg  := 'تم تعديل بيانات الطالب بنجاح';

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result_code := 1;
        p_result_msg  := 'خطأ غير متوقع: ' || SQLERRM;
END SP_UPDATE_STUDENT;",

                @"CREATE OR REPLACE TRIGGER TRG_NO_SUPERVISOR_IN_OWN_COMMITTEE
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
END TRG_NO_SUPERVISOR_IN_OWN_COMMITTEE;",

                @"CREATE OR REPLACE TRIGGER TRG_CALC_FINAL_SCORE
BEFORE INSERT OR UPDATE OF ""supervisor_score"", ""committee_score"" ON ""discussion""
FOR EACH ROW
BEGIN
    :NEW.""final_score"" := FN_CALC_FINAL_SCORE(:NEW.""supervisor_score"", :NEW.""committee_score"");
END TRG_CALC_FINAL_SCORE;",

                @"CREATE OR REPLACE PROCEDURE SP_DELETE_STUDENT(
    p_student_id  IN  ""student"".""student_id""%TYPE,
    p_deleted_by  IN  NUMBER,
    p_result_code OUT NUMBER,
    p_result_msg  OUT VARCHAR2
) AS
    v_student_count NUMBER;
    v_has_team      NUMBER;
BEGIN
    -- التحقق من وجود الطالب
    SELECT COUNT(*) INTO v_student_count
    FROM ""student""
    WHERE ""student_id"" = p_student_id AND ""is_deleted"" = 0;

    IF v_student_count = 0 THEN
        p_result_code := 1;
        p_result_msg  := 'الطالب غير موجود أو محذوف مسبقاً';
        RETURN;
    END IF;

    -- التحقق: هل الطالب في فريق نشط؟
    v_has_team := FN_STUDENT_HAS_TEAM(p_student_id);

    IF v_has_team = 1 THEN
        p_result_code := 1;
        p_result_msg  := 'لا يمكن حذف الطالب لأنه منتسب لفريق نشط. يرجى إزالته من الفريق أولاً';
        RETURN;
    END IF;

    -- حذف منطقي
    UPDATE ""student""
    SET ""is_deleted""        = 1,
        ""updated_at""        = SYSTIMESTAMP,
        ""updated_by_user_id"" = p_deleted_by
    WHERE ""student_id"" = p_student_id;

    COMMIT;
    p_result_code := 0;
    p_result_msg  := 'تم حذف الطالب بنجاح';

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result_code := 1;
        p_result_msg  := 'خطأ غير متوقع: ' || SQLERRM;
END SP_DELETE_STUDENT;",

                @"CREATE OR REPLACE PROCEDURE SP_ASSIGN_STUDENT_TO_TEAM(
    p_student_id  IN  ""student"".""student_id""%TYPE,
    p_team_id     IN  ""student"".""team_id""%TYPE,   -- NULL لإلغاء الانتساب
    p_updated_by  IN  NUMBER,
    p_result_code OUT NUMBER,
    p_result_msg  OUT VARCHAR2
) AS
    v_student_dept  NUMBER;
    v_team_dept     NUMBER;
    v_has_team      NUMBER;
    v_student_count NUMBER;
    v_team_count    NUMBER;
BEGIN
    -- التحقق من وجود الطالب
    SELECT COUNT(*), MAX(""department_id"")
    INTO v_student_count, v_student_dept
    FROM ""student""
    WHERE ""student_id"" = p_student_id AND ""is_deleted"" = 0;

    IF v_student_count = 0 THEN
        p_result_code := 1;
        p_result_msg  := 'الطالب غير موجود';
        RETURN;
    END IF;

    -- إذا كان p_team_id = NULL → إلغاء الانتساب فقط
    IF p_team_id IS NULL THEN
        UPDATE ""student""
        SET ""team_id""           = NULL,
            ""updated_at""        = SYSTIMESTAMP,
            ""updated_by_user_id"" = p_updated_by
        WHERE ""student_id"" = p_student_id;

        COMMIT;
        p_result_code := 0;
        p_result_msg  := 'تم إلغاء انتساب الطالب من الفريق بنجاح';
        RETURN;
    END IF;

    -- التحقق من وجود الفريق
    SELECT COUNT(*), MAX(""department_id"")
    INTO v_team_count, v_team_dept
    FROM ""team""
    WHERE ""team_id"" = p_team_id AND ""is_deleted"" = 0;

    IF v_team_count = 0 THEN
        p_result_code := 1;
        p_result_msg  := 'الفريق المحدد غير موجود';
        RETURN;
    END IF;

    -- التحقق: الطالب والفريق في نفس القسم؟
    IF v_student_dept != v_team_dept THEN
        p_result_code := 1;
        p_result_msg  := 'الطالب والفريق لا ينتميان لنفس القسم';
        RETURN;
    END IF;

    -- التحقق: هل الطالب منتسب لفريق آخر؟
    v_has_team := FN_STUDENT_HAS_TEAM(p_student_id);

    IF v_has_team = 1 THEN
        p_result_code := 1;
        p_result_msg  := 'الطالب منتسب لفريق آخر بالفعل. يرجى إلغاء انتسابه أولاً';
        RETURN;
    END IF;

    -- إجراء الانتساب
    UPDATE ""student""
    SET ""team_id""           = p_team_id,
        ""updated_at""        = SYSTIMESTAMP,
        ""updated_by_user_id"" = p_updated_by
    WHERE ""student_id"" = p_student_id;

    COMMIT;
    p_result_code := 0;
    p_result_msg  := 'تم انتساب الطالب للفريق بنجاح';

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result_code := 1;
        p_result_msg  := 'خطأ غير متوقع: ' || SQLERRM;
END SP_ASSIGN_STUDENT_TO_TEAM;"
            };

            foreach(var sql in scripts) {
                try {
                    using var exec = conn.CreateCommand();
                    exec.CommandText = sql;
                    exec.ExecuteNonQuery();
                    Console.WriteLine("Executed successfully.");
                } catch(Exception e) {
                    Console.WriteLine("Error executing: " + e.Message);
                }
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT NAME, TYPE, LINE, POSITION, TEXT FROM USER_ERRORS ORDER BY NAME, LINE";
            using var reader = cmd.ExecuteReader();
            bool hasErrors = false;
            while(reader.Read())
            {
                hasErrors = true;
                Console.WriteLine($"{reader["TYPE"]} {reader["NAME"]} at {reader["LINE"]}:{reader["POSITION"]} - {reader["TEXT"]}");
            }
            if(!hasErrors) Console.WriteLine("No errors found in USER_ERRORS!");
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
