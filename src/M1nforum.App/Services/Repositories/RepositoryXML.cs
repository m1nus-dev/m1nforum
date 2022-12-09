using M1nforum.Web.Services.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

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

    public class XmlRepository<T> : Repository<T> where T : IEntity
    {
        private readonly string _path;

        public XmlRepository()
            : this("./App_Data/{0}.xml")
        {
        }

        public XmlRepository(string path)
        {
            _path = path;
        }

        protected override List<T> Load()
        {
            var fileName = GetFileName();

            if (File.Exists(fileName))
            {
                var xmlSerializer = new XmlSerializer(typeof(List<T>));
                using (var streamReader = new StreamReader(fileName))
                {
                    return (List<T>)xmlSerializer.Deserialize(streamReader);
                }
            }
            else
            {
                return new List<T>();
            }
        }

        protected override void Save(List<T> entities)
        {
            if (entities != null)
            {
                var fileName = GetFileName();

                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                var xmlSerializer = new XmlSerializer(typeof(List<T>));
                using (var streamWriter = new StreamWriter(fileName))
                {
                    xmlSerializer.Serialize(streamWriter, entities);
                }
            }
        }

        private string GetFileName()
        {
            var fileName = string.Format(_path, typeof(T).FullName);

            if (fileName.StartsWith("~"))
            {
                // dev - 2011-12-16 - Changed to use assembly location.
                // fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName.Replace("~", ""));
            }

            return fileName;
        }
    }
}


// http://stackoverflow.com/questions/1464883/how-can-i-easily-convert-datareader-to-listt