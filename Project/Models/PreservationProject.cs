namespace Project.Models;

public class PreservationProject
{
    public int ProjectId { get; set; }
    public int ArtifactId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Objective { get; set; }
}