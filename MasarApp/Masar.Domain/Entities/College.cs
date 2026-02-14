using System;
using System.Collections.Generic;

using Masar.Domain.Common;

namespace Masar.Domain.Entities;

public class College : BaseEntity
{
    public int CollegeId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public ICollection<Department> Departments { get; set; } = new List<Department>();
}
