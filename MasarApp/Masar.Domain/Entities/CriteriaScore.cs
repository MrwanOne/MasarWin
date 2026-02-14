namespace Masar.Domain.Entities;

/// <summary>
/// درجة طالب معين في معيار معين
/// </summary>
public class CriteriaScore
{
    public int ScoreId { get; set; }
    public int EvaluationId { get; set; }
    public StudentEvaluation? Evaluation { get; set; }
    public int CriteriaId { get; set; }
    public EvaluationCriteria? Criteria { get; set; }
    public decimal Score { get; set; }
    public string Comments { get; set; } = string.Empty;
}
