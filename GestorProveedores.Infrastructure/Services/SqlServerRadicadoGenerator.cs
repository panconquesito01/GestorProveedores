using System.Data;
using GestorProveedores.Business.Ports;
using GestorProveedores.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestorProveedores.Infrastructure.Services;

internal sealed class SqlServerRadicadoGenerator(GestorProveedoresDbContext dbContext) : IRadicadoGenerator
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR dbo.RadicadoSeq";

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var sequence = Convert.ToInt32(result);

            return $"SOL-{DateTime.Now:yyyyMMddHHmmss}-{sequence:0000}";
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}