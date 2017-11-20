using ETexsys.IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Data.Entity.Validation;

namespace ETexsys.DAL
{
    public class UnitOfWorkContextBase : IUnitOfWorkContext
    {
        public DbContext Context
        {
            get { return EFContextFactory.GetCurrentDbContext(); }
        }

        public bool IsCommitted
        {
            get;
            private set;
        }

        public int Commit()
        {
            if (IsCommitted)
            {
                return 0;
            }
            try
            {
                int result = Context.SaveChanges();
                IsCommitted = true;
                return result;
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Class: {0}, Property: {1}, Error: {2}", validationErrors.Entry.Entity.GetType().FullName,
                            validationError.PropertyName,
                            validationError.ErrorMessage), "error");
                    }
                }
                throw new Exception("提交数据库发生异常：" + dbEx.Message); 
            }
            catch (DbUpdateException e)
            {
                throw new Exception("提交数据库发生异常：" + e.Message);
            }
        }

        public void Dispose()
        {
            if (!IsCommitted)
            {
                Commit();
            }
            Context.Dispose();
        }

        public void RegisterDeleted<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            try
            {
                Context.Configuration.AutoDetectChangesEnabled = false;
                foreach (TEntity entity in entities)
                {
                    RegisterDeleted(entity);
                }
            }
            finally
            {
                Context.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public void RegisterDeleted<TEntity>(TEntity entity) where TEntity : class
        {
            Context.Entry(entity).State = EntityState.Deleted;
            IsCommitted = false;
        }

        public void RegisterModified<TEntity>(TEntity entity) where TEntity : class
        {
            if (Context.Entry(entity).State == EntityState.Detached)
            {
                Context.Set<TEntity>().Attach(entity);
            }
            Context.Entry(entity).State = EntityState.Modified;
            IsCommitted = false;
        }

        public void RegisterNew<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            try
            {
                Context.Configuration.AutoDetectChangesEnabled = false;
                foreach (TEntity entity in entities)
                {
                    RegisterNew(entity);
                }
            }
            finally
            {
                Context.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public void RegisterNew<TEntity>(TEntity entity) where TEntity : class
        {
            EntityState state = Context.Entry(entity).State;
            if (state == EntityState.Detached)
            {
                Context.Entry(entity).State = EntityState.Added;
            }
            IsCommitted = false;
        }

        public void Rollback()
        {
            IsCommitted = false;
        }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return Context.Set<TEntity>();
        }

    }
}
