using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dsw2025Tpi.Data.Repositories;

public class EfRepository<T> : IRepository<T> where T : EntityBase
{
    private readonly Dsw2025TpiDbContext _context;

    public EfRepository(Dsw2025TpiDbContext context)
    {
        _context = context;
    }

    // Los metodos de escritura confirman con SaveChangesAsync, que guarda TODOS
    // los cambios pendientes del change tracker (no solo los de esta entidad) en
    // una unica transaccion implicita.

    public async Task<T> Add(T entity)
    {
        await _context.AddAsync(entity);
        await SaveChangesAsync();
        return entity;
    }

    public async Task<T> Update(T entity)
    {
        _context.Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public async Task<T> Delete(T entity)
    {
        _context.Remove(entity);
        await SaveChangesAsync();
        return entity;
    }

    public async Task<T?> GetById(Guid id, params string[] include)
    {
        return await Include(_context.Set<T>(), include)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<T?> First(Expression<Func<T, bool>> predicate, params string[] include)
    {
        return await Include(_context.Set<T>(), include)
            .FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<T>?> GetAll(params string[] include)
    {
        return await Include(_context.Set<T>(), include)
            .ToListAsync();
    }

    public async Task<IEnumerable<T>?> GetFiltered(Expression<Func<T, bool>> predicate, params string[] include)
    {
        return await Include(_context.Set<T>(), include)
            .Where(predicate)
            .ToListAsync();
    }

    public IQueryable<T> GetAllQueryable(params string[] include)
    {
        return Include(_context.Set<T>(), include);
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            // Una transaccion incluye la orden y TODOS los descuentos trackeados.
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // EF revierte la transaccion. No reintentar una compra con los
            // valores obsoletos ni dejar cambios pendientes en este contexto.
            _context.ChangeTracker.Clear();
            throw new ConcurrentUpdateException(ex);
        }
    }

    private static IQueryable<T> Include(IQueryable<T> query, string[] includes)
    {
        foreach (var include in includes)
        {
            // Soportar múltiples includes separados por coma
            var includeList = include.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i));

            foreach (var inc in includeList)
            {
                query = query.Include(inc);
            }
        }
        return query;
    }
}
