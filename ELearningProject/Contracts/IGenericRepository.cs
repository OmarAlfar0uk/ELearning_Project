using System.Linq.Expressions;
using Auth.Models;
using ELearningProject.Contracts;

namespace ELearningProject.Contarcts
{
    public interface IGenericRepository<TEntity> where TEntity : class, IBaseEntity
    {
        Task CreateAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        IQueryable<TEntity> GetAll(bool trackChanges = false);
        IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> expression, bool trackChanges = false);
        Task<TEntity?> GetByIdAsync(Guid id);

        
    }
}
