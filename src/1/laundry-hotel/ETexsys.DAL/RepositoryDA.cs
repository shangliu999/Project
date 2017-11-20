﻿using ETexsys.IDAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace ETexsys.DAL
{
    public class RepositoryDA<T> : IRepository<T> where T : class, new()
    {
        //private DbContext ent = EFContextFactory.GetCurrentDbContext();

        public IUnitOfWork UnitOfWork { get; set; }

        protected IUnitOfWorkContext EFContext
        {
            get
            {
                UnitOfWorkContextBase nowcb = new UnitOfWorkContextBase();
                return nowcb;
            }
        }

        public IQueryable<T> Entities
        {
            get
            {
                return EFContext.Set<T>();
            }
        }

        public int Delete(IEnumerable<T> entities, bool isSave = true)
        {
            EFContext.RegisterDeleted(entities);
            return isSave ? EFContext.Commit() : 0;
        }

        public int Delete(Expression<Func<T, bool>> predicate, bool isSave = true)
        {
            List<T> entities = EFContext.Set<T>().Where(predicate).ToList();
            return entities.Count > 0 ? Delete(entities, isSave) : (isSave ? EFContext.Commit() : 0);
        }

        public int Delete(T entity, bool isSave = true)
        {
            EFContext.RegisterDeleted(entity);
            return isSave ? EFContext.Commit() : 0;
        }

        public int Delete(object id, bool isSave = true)
        {
            T entity = EFContext.Set<T>().Find(id);
            return entity != null ? Delete(entity, isSave) : 0;
        }

        public T GetByKey(object key)
        {
            return EFContext.Set<T>().Find(key);
        }

        public int Insert(IEnumerable<T> entities, bool isSave = true)
        {
            EFContext.RegisterNew(entities);
            return isSave ? EFContext.Commit() : 0;
        }

        public int Insert(T entity, bool isSave = true)
        {
            EFContext.RegisterNew(entity);
            return isSave ? EFContext.Commit() : 0;
        }

        public int Update(T entity, bool isSave = true)
        {
            EFContext.RegisterModified(entity);
            return isSave ? EFContext.Commit() : 0;
        }


        public IQueryable<T> LoadEntities(Func<T, bool> whereLambda)
        {
            return EFContext.Set<T>().Where(whereLambda).AsQueryable();
        }

        public System.Data.Entity.Infrastructure.DbRawSqlQuery<T> SQLQuery(string sql, params object[] param)
        {
            return EFContextFactory.GetCurrentDbContext().Database.SqlQuery<T>(sql, param);
        }

        public int ExecuteSql(string sql)
        {
            return EFContextFactory.GetCurrentDbContext().Database.ExecuteSqlCommand(sql);
        }
    }
}
