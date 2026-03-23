using Auth.Models;
using ELearningProject.Contarcts;
using ELearningProject.Contracts;
using ELearningProject.Data;
using ELearningProject.Models;
using System.Collections.Concurrent;

namespace ELearningProject.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UniversitySystemAuthContext _dbContext;
        private readonly ConcurrentDictionary<string, object> _Repositories;

        public UnitOfWork(UniversitySystemAuthContext applicationDb) {
            _dbContext = applicationDb;
            _Repositories = new ConcurrentDictionary<string, object>();
        } 
        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class, IBaseEntity
        {
            var type = typeof(TEntity).Name;

            return (IGenericRepository<TEntity>)_Repositories.GetOrAdd(type, t => 
            {
                var repositoryType = typeof(GenericRepository<>);
                return Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _dbContext)!;
            });
        }

        public async Task<int> SaveChangesAsync()
        
            => await _dbContext.SaveChangesAsync();

    }
}
