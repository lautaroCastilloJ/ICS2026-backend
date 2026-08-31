using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dsw2025Tpi.Data.Repositories;

public class EfRepository<T> : IRepository<T> where T : EntityBase
{
    private readonly Dsw2025TpiContext _context;

    public EfRepository(Dsw2025TpiContext context)
    {
        _context = context;
    }

    // Los metodos de escritura NO confirman: solo registran el cambio en el
    // contexto. La confirmacion es responsabilidad del caso de uso, a traves de
    // IUnitOfWork.SaveChangesAsync(). Asi una operacion que toca varias entidades
    // se guarda entera o no se guarda.

    public async Task<T> Add(T entity)
    {
        await _context.AddAsync(entity);
        return entity;
    }

    public Task<T> Update(T entity)
    {
        _context.Update(entity);
        return Task.FromResult(entity);
    }

    public Task<T> Delete(T entity)
    {
        _context.Remove(entity);
        return Task.FromResult(entity);
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
