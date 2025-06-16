namespace Project.DTOs;

public class ProjectDetailsDto
{
    public int ProjectId { get; set; }
    public string Objective { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public ArtifactDto Artifact { get; set; }
    public InstitutionDto Institution { get; set; }
}