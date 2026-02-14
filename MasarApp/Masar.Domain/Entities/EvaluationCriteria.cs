using System.Collections.Generic;
using Masar.Domain.Common;

namespace Masar.Domain.Entities;

/// <summary>
/// معايير تقييم مشاريع التخرج
/// </summary>
public class EvaluationCriteria : BaseEntity
{
    public int CriteriaId { get; set; }
    public string NameAr { get; set; } = string.Empty; // "التقرير الكتابي"
    public string NameEn { get; set; } = string.Empty; // "Written Report"
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public decimal MaxScore { get; set; } // 25
    public decimal Weight { get; set; } // 0.25
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int? DepartmentId { get; set; } // null = عام لكل الأقسام
    public Department? Department { get; set; }

    public ICollection<CriteriaScore> Scores { get; set; } = new List<CriteriaScore>();
}
