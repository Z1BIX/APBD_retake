using Project.DTOs;

namespace Project.Services;

public interface IDbService
{
    Task<ProjectDetailsDto?> GetProjectByIdAsync(int id);
    Task AddArtifactWithProjectAsync(CreateArtifactWithProjectDto dto);
}