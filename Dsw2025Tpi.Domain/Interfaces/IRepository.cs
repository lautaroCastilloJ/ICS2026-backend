using Dsw2025Tpi.Domain.Entities;
using System.Linq.Expressions;

namespace Dsw2025Tpi.Domain.Interfaces;

public interface IRepository<T> where T : EntityBase
{
    Task<T> Add(T entity);
    Task<T> Update(T entity);
    Task<T> Delete(T entity);
    Task<T?> GetById(Guid id, params string[] include);
    Task<T?> First(Expression<Func<T, bool>> predicate, params string[] include);
    Task<IEnumerable<T>?> GetAll(params string[] include);
    IQueryable<T> GetAllQueryable(params string[] include);
    Task<IEnumerable<T>?> GetFiltered(Expression<Func<T, bool>> predicate, params string[] include);
}
