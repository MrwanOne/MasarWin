using System;
using System.Collections.Generic;
using Masar.Domain.Common;

namespace Masar.Domain.Entities;

/// <summary>
/// الفصل الدراسي / السنة الأكاديمية
/// </summary>
public class AcademicTerm : BaseEntity
{
    public int TermId { get; set; }
    public int Year { get; set; } // 2025, 2026
    public int Semester { get; set; } // 1 = الأول, 2 = الثاني
    public string NameAr { get; set; } = string.Empty; // "الفصل الأول 1446"
    public string NameEn { get; set; } = string.Empty; // "First Semester 2025"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Committee> Committees { get; set; } = new List<Committee>();
}
