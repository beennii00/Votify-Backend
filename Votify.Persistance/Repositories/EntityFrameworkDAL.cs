using Microsoft.EntityFrameworkCore;
using Persistance.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Persistance.Repositories
{
    public class EntityFrameworkDAL : IDAL
    {
        private readonly AppDbContext _dbContext;

        public EntityFrameworkDAL(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // --- MÉTODOS ASÍNCRONOS AÑADIDOS PARA REFACTORIZAR "SYNC-OVER-ASYNC" ---
        public IQueryable<T> Query<T>() where T : class
        {
            return _dbContext.Set<T>();
        }

        public async Task InsertAsync<T>(T entity) where T : class
        {
            await _dbContext.Set<T>().AddAsync(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync<T>(object id) where T : class
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<bool> ExistsAsync<T>(object id) where T : class
        {
            return await _dbContext.Set<T>().FindAsync(id) != null;
        }

        public async Task<IEnumerable<T>> GetWhereAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await _dbContext.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task CommitAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
        // ------------------------------------------------------------------------

        public void Insert<T>(T entity) where T : class
        {
            _dbContext.Set<T>().Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _dbContext.Set<T>().Remove(entity);
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            return _dbContext.Set<T>();
        }

        public T GetById<T>(IComparable id) where T : class
        {
            return _dbContext.Set<T>().Find(id);
        }

        public bool Exists<T>(IComparable id) where T : class
        {
            return _dbContext.Set<T>().Find(id) != null;
        }

        public void Clear<T>() where T : class
        {
            _dbContext.Set<T>().RemoveRange(_dbContext.Set<T>());
        }

        public IEnumerable<T> GetWhere<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return _dbContext.Set<T>().Where(predicate).AsEnumerable();
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        public void Rollback()
        {
            foreach (var entry in _dbContext.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }
        }

        public void RemoveAllData()
        {
            foreach (var entity in _dbContext.Model.GetEntityTypes())
            {
                var dbSet = _dbContext.Set<object>();
                _dbContext.RemoveRange(dbSet);
            }
            _dbContext.SaveChanges();
        }

        public void BeginTransaction()
        {
            _dbContext.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _dbContext.Database.CurrentTransaction?.Commit();
        }

        public void RollbackTransaction()
        {
            _dbContext.Database.CurrentTransaction?.Rollback();
        }
    }
}