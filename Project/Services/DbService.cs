using Microsoft.Data.SqlClient;
using Project.DTOs;

namespace Project.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }

    public async Task<ProjectDetailsDto?> GetProjectByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT p.ProjectId, p.Objective, p.StartDate, p.EndDate,
                             a.ArtifactId, a.Name AS ArtifactName, a.OriginDate, a.InstitutionId,
                             i.Name AS InstitutionName, i.FoundedYear
                      FROM Preservation_Project p
                      JOIN Artifact a ON p.ArtifactId = a.ArtifactId
                      JOIN Institution i ON a.InstitutionId = i.InstitutionId
                      WHERE p.ProjectId = @ProjectId";

        using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@ProjectId", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ProjectDetailsDto
            {
                ProjectId = reader.GetInt32(0),
                Objective = reader.GetString(1),
                StartDate = reader.GetDateTime(2),
                EndDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                Artifact = new ArtifactDto
                {
                    ArtifactId = reader.GetInt32(4),
                    Name = reader.GetString(5),
                    OriginDate = reader.GetDateTime(6),
                    InstitutionId = reader.GetInt32(7)
                },
                Institution = new InstitutionDto
                {
                    InstitutionId = reader.GetInt32(7),
                    Name = reader.GetString(8),
                    FoundedYear = reader.GetInt32(9)
                }
            };
        }

        return null;
    }

    public async System.Threading.Tasks.Task AddArtifactWithProjectAsync(CreateArtifactWithProjectDto dto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var cmd1 = new SqlCommand(@"INSERT INTO Artifact (ArtifactId, Name, OriginDate, InstitutionId)
                                        VALUES (@ArtifactId, @Name, @OriginDate, @InstitutionId)", connection, transaction);

            cmd1.Parameters.AddWithValue("@ArtifactId", dto.Artifact.ArtifactId);
            cmd1.Parameters.AddWithValue("@Name", dto.Artifact.Name);
            cmd1.Parameters.AddWithValue("@OriginDate", dto.Artifact.OriginDate);
            cmd1.Parameters.AddWithValue("@InstitutionId", dto.Artifact.InstitutionId);
            await cmd1.ExecuteNonQueryAsync();

            var cmd2 = new SqlCommand(@"INSERT INTO Preservation_Project (ProjectId, ArtifactId, StartDate, EndDate, Objective)
                                        VALUES (@ProjectId, @ArtifactId, @StartDate, @EndDate, @Objective)", connection, transaction);

            cmd2.Parameters.AddWithValue("@ProjectId", dto.Project.ProjectId);
            cmd2.Parameters.AddWithValue("@ArtifactId", dto.Artifact.ArtifactId);
            cmd2.Parameters.AddWithValue("@StartDate", dto.Project.StartDate);
            cmd2.Parameters.AddWithValue("@EndDate", (object?)dto.Project.EndDate ?? DBNull.Value);
            cmd2.Parameters.AddWithValue("@Objective", dto.Project.Objective);
            await cmd2.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}