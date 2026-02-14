namespace Masar.Application.DTOs;

public class EvaluationCriteriaDto
{
    public int CriteriaId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
}
