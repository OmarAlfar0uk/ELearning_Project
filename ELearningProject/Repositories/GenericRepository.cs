using Microsoft.EntityFrameworkCore;
using ELearningProject.Contarcts;
using ELearningProject.Data;
using ELearningProject.Models;
using System.Linq.Expressions;
using ELearningProject.Contracts;

namespace ELearningProject.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IBaseEntity
    {
        private readonly UniversitySystemAuthContext _dbContext;

        public GenericRepository(UniversitySystemAuthContext dbContext)
        {
           _dbContext = dbContext;
        }

        public async Task CreateAsync(TEntity entity)
            => await _dbContext.Set<TEntity>().AddAsync(entity);

        public void Delete(TEntity entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;
                _dbContext.Set<TEntity>().Update(entity);
        }

        public void Update(TEntity entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _dbContext.Set<TEntity>().Update(entity);
        }

        public IQueryable<TEntity> GetAll(bool trackChanges = false)
        {
            var query =     _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .AsQueryable();

            return trackChanges ? query : query.AsNoTracking();
        }

        public IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> expression, bool trackChanges = false)
        {
            var query = _dbContext.Set<TEntity>()
                .Where(expression)
                .Where(e => !e.IsDeleted)
                .AsQueryable();

            return trackChanges ? query : query.AsNoTracking();
        }

        public async Task<TEntity?> GetByIdAsync(Guid id)
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(id);
            return entity is not null && !entity.IsDeleted ? entity : null;
        }
    }
}
