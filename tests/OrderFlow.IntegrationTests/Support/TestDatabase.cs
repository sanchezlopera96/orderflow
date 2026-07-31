using Npgsql;

namespace OrderFlow.IntegrationTests.Support;

/// <summary>Crea una base de datos nueva y única en el contenedor, para aislar cada prueba.</summary>
public static class TestDatabase
{
    public static async Task<string> CreateFreshAsync(string adminConnectionString)
    {
        var databaseName = $"of_{Guid.NewGuid():N}";

        await using (var admin = new NpgsqlConnection(adminConnectionString))
        {
            await admin.OpenAsync();
            await using var command = admin.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        return new NpgsqlConnectionStringBuilder(adminConnectionString) { Database = databaseName }.ConnectionString;
    }
}
