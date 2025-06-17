using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.EntityFramework
{
    public class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly DbContext _context;

        public EfRepository(DbContext context)
        {
            _context = context;
        }

        protected virtual DbSet<T> Entities => _context.Set<T>();

        public IQueryable<T> GetAll => Entities;

        public IQueryable<T> GetAllNoTracking => Entities.AsNoTracking();

        public bool Any(Expression<Func<T, bool>> expression)
        {
            return Entities.Any(expression);
        }

        public int Commit()
        {
            try
            {
                return _context.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                GetFullErrorTextAndRollbackEntityChanges(exception);
                return -1;
            }
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var ali = ex;
                //GetFullErrorTextAndRollbackEntityChanges(exception);
                return -1;
            }
        }

        public int Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Entities.Remove(entity);
            return Commit();
        }

        public int DeleteBulk(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            Entities.RemoveRange(entities);
            return Commit();
        }

        public T Get(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
        {
            var query = Entities.Where(expression);
            return includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty)).FirstOrDefault();
        }

        public T GetById(int Id)
        {
            return Entities.Find(Id);
        }

        public IQueryable<T> GetMany(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
        {
            var query = Entities.Where(expression);
            return includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public int InsertBulk(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            Entities.AddRange(entities);
            return Commit();
        }

        public IEnumerable<T> InsertBulkWithoutCommit(List<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            Entities.AddRange(entities);
            return entities;
        }

        public T InsertWithoutCommit(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Entities.Add(entity);
            return entity;
        }

        public int Remove(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                Entities.Remove(entity);
                return 1;
            }
            catch
            {
                return -1;
            }
        }

        public int Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            foreach (var changeEntity in _context.ChangeTracker.Entries()
                    .Where(x => x.Entity is BaseEntity && x.State == EntityState.Modified)
                    .Select(x => x.Entity)
                    .Cast<BaseEntity>())
            {
                entity.UpdatedTime = DateTime.Now;
            }

            Entities.Update(entity);
            return Commit();
        }

        public int UpdateWithoutCommit(T entity)
        {
            if (entity != null)
                try
                {
                    foreach (var changeEntity in _context.ChangeTracker.Entries()
                      .Where(x => x.Entity is BaseEntity && x.State == EntityState.Modified)
                      .Select(x => x.Entity)
                      .Cast<BaseEntity>())
                    {
                        entity.UpdatedTime = DateTime.Now;
                    }

                    Entities.Update(entity);
                    return 1;
                }
                catch
                {
                    return -1;
                }
            throw new ArgumentNullException(nameof(entity));
        }

        protected string GetFullErrorTextAndRollbackEntityChanges(DbUpdateException exception)
        {
            if (_context is DbContext dbContext)
            {
                var entries = dbContext.ChangeTracker.Entries()
                                        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified).ToList();

                entries.ForEach(entry =>
                {
                    entry.State = EntityState.Unchanged;
                });
            }

            try
            {
                _context.SaveChanges();
                return exception.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }
}