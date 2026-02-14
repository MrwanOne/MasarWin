using Masar.Domain.Common;

namespace Masar.Domain.Entities;

/// <summary>
/// تقييم طالب فردي في مناقشة
/// </summary>
public class StudentEvaluation : BaseEntity
{
    public int EvaluationId { get; set; }
    public int DiscussionId { get; set; }
    public Discussion? Discussion { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public decimal TotalScore { get; set; }
    public decimal ContributionPercentage { get; set; } // نسبة مساهمة الطالب
    public string GeneralFeedback { get; set; } = string.Empty;
    public string StrengthPoints { get; set; } = string.Empty; // نقاط القوة
    public string ImprovementAreas { get; set; } = string.Empty; // نقاط التحسين

    public ICollection<CriteriaScore> CriteriaScores { get; set; } = new List<CriteriaScore>();
}
