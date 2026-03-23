using ELearningProject.Contracts;

namespace ELearningProject.Contarcts
{
    public interface IUnitOfWork
    {
        public Task <int> SaveChangesAsync();

        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class, IBaseEntity;
    }
}
