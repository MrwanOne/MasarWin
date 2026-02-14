namespace Masar.Application.DTOs;

public class StudentEvaluationDto
{
    public int EvaluationId { get; set; }
    public int DiscussionId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public decimal TotalScore { get; set; }
    public decimal ContributionPercentage { get; set; }
    public string GeneralFeedback { get; set; } = string.Empty;
    public string StrengthPoints { get; set; } = string.Empty;
    public string ImprovementAreas { get; set; } = string.Empty;
    public DateTime EvaluatedAt { get; set; }
    public int? EvaluatedByUserId { get; set; }
    public string EvaluatedByName { get; set; } = string.Empty;
    public List<CriteriaScoreDto> CriteriaScores { get; set; } = new();
}

public class CriteriaScoreDto
{
    public int ScoreId { get; set; }
    public int EvaluationId { get; set; }
    public int CriteriaId { get; set; }
    public string CriteriaNameAr { get; set; } = string.Empty;
    public string CriteriaNameEn { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public decimal Score { get; set; }
    public string Comments { get; set; } = string.Empty;
}
