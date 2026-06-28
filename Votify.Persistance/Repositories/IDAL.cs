using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Persistance.Repositories
{
    public interface IDAL
    {
        // Métodos asíncronos recomendados
        Task InsertAsync<T>(T entity) where T : class;
        Task<IEnumerable<T>> GetAllAsync<T>() where T : class;
        Task<T?> GetByIdAsync<T>(object id) where T : class;
        Task<bool> ExistsAsync<T>(object id) where T : class;
        Task<IEnumerable<T>> GetWhereAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
        Task CommitAsync();
        
        // Acceso IQueryable para consultas complejas (como SelectMany, Include, etc) sin acoplar DbContext
        IQueryable<T> Query<T>() where T : class;

        // Métodos síncronos legados (para no romper servicios no actualizados aún)
        void Insert<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        IEnumerable<T> GetAll<T>() where T : class;
        T GetById<T>(IComparable id) where T : class;
        bool Exists<T>(IComparable id) where T : class;
        void Clear<T>() where T : class;
        IEnumerable<T> GetWhere<T>(Expression<Func<T, bool>> predicate) where T : class;
        void Commit();
        void Rollback();
        void RemoveAllData();
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}