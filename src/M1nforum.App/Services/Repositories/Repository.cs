using M1nforum.Web.Services.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace M1nforum.Web.Services.Repositories
{
	public abstract class Repository<T> : IRepository<T> where T : IEntity
	{
		private List<T> _cache;
		private readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

		public virtual IRepository<T> ClearCache()
		{
			_readerWriterLockSlim.EnterWriteLock();

			try
			{
				_cache = null;
			}
			catch (Exception exception)
			{
				// On error, clear the cache.
				_cache = null;
				throw;
			}
			finally
			{
				_readerWriterLockSlim.ExitWriteLock();
			}

			return this;
		}

		public virtual IRepository<T> Delete(Func<T, bool> match)
		{
			LoadCache();

			_readerWriterLockSlim.EnterWriteLock();

			try
			{
				_cache.RemoveAll(new Predicate<T>(match));
				Save(_cache);
			}
			finally
			{
				_readerWriterLockSlim.ExitWriteLock();
			}

			return this;
		}

		public virtual IRepository<T> Insert(T entity)
		{
			LoadCache();

			_readerWriterLockSlim.EnterWriteLock();

			try
			{
				_cache.Add(entity);
				Save(_cache);
			}
			finally
			{
				_readerWriterLockSlim.ExitWriteLock();
			}

			return this;
		}

		public List<T> List(Func<T, bool> where = null)
		{
			LoadCache();

			_readerWriterLockSlim.EnterReadLock();

			try
			{
				// This line prevents the external callers from modifing the list outside this process.
				return where == null ? _cache.ToList() : _cache.Where(where).ToList();
			}
			finally
			{
				_readerWriterLockSlim.ExitReadLock();
			}
		}

		private void LoadCache()
		{
			_readerWriterLockSlim.EnterWriteLock();

			try
			{
				if (_cache == null)
				{
					_cache = Load();
				}
			}
			finally
			{
				_readerWriterLockSlim.ExitWriteLock();
			}
		}

		public virtual IRepository<T> Update(T entity)
		{
			// ugh, this method is bad.  Needs to be in a single "transaction", or smarter, or both.

			LoadCache();

			_readerWriterLockSlim.EnterWriteLock();

			try
			{
				_cache.RemoveAll(t => t.Id == entity.Id); // not dry, but prevents an extra save.
				_cache.Add(entity);
				Save(_cache);
			}
			finally
			{
				_readerWriterLockSlim.ExitWriteLock();
			}

			return this;
		}

		protected abstract void Save(List<T> entities);
		protected abstract List<T> Load();
	}
}
