namespace Masar.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalProjects      { get; set; }
    public int ProposedProjects   { get; set; }
    public int ApprovedProjects   { get; set; }
    public int InProgressProjects { get; set; }
    public int CompletedProjects  { get; set; }
    public int RejectedProjects   { get; set; }
    public int TotalStudents      { get; set; }
    public int TotalTeams         { get; set; }
    public int TotalCommittees    { get; set; }
    public int TotalDoctors       { get; set; }
}

