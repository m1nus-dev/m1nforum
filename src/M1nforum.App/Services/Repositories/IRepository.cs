using M1nforum.Web.Services.Entities;
using System;
using System.Collections.Generic;

namespace M1nforum.Web.Services.Repositories
{
    public interface IRepository<T> where T : IEntity
    {
        IRepository<T> ClearCache();
        IRepository<T> Delete(Func<T, bool> match);
        IRepository<T> Insert(T entity);
        List<T> List(Func<T, bool> where = null);
        IRepository<T> Update(T entity);
    }
}
