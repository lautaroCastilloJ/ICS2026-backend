using Dsw2025Tpi.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dsw2025Tpi.Data.Persistence;

/// <summary>
/// Implementacion de la unidad de trabajo sobre EF Core.
///
/// Reemplaza al TransactionScope anterior: ambos contextos comparten la misma
/// DbConnection (ver ServiceCollectionExtensions.AddDomainServices), de modo que
/// pueden enlistarse en la misma transaccion local de SQL Server sin promover a
/// una transaccion distribuida (MSDTC).
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly Dsw2025TpiContext _appDb;
    private readonly AuthenticateContext _authDb;

    public UnitOfWork(Dsw2025TpiContext appDb, AuthenticateContext authDb)
    {
        _appDb = appDb;
        _authDb = authDb;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _appDb.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        // Si ya hay una transaccion en curso, se participa de ella en lugar de
        // abrir una anidada: el commit queda a cargo de quien la inicio.
        if (_appDb.Database.CurrentTransaction is not null)
        {
            await action(cancellationToken);
            await _authDb.SaveChangesAsync(cancellationToken);
            await _appDb.SaveChangesAsync(cancellationToken);
            return;
        }

        await using var transaction = await _appDb.Database.BeginTransactionAsync(cancellationToken);

        // El contexto de Identity se enlista en la misma transaccion. Requiere que
        // ambos contextos compartan la conexion; si no la compartieran, esta linea
        // lanzaria InvalidOperationException en vez de escalar silenciosamente.
        await _authDb.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);

        try
        {
            await action(cancellationToken);

            await _authDb.SaveChangesAsync(cancellationToken);
            await _appDb.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
