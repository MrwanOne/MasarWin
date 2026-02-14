namespace Masar.Application.DTOs;

public class AcademicTermDto
{
    public int TermId { get; set; }
    public int Year { get; set; }
    public int Semester { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
