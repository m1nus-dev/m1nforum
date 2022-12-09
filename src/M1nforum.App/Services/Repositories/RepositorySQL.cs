//using M1nforum.Web.Models.Entities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;

//namespace M1nforum.Web.Models.Repositories
//{
//    public abstract class DatabaseRepository<T> : IRepository<T> where T : IEntity
//    {
//        private List<T> _cache;
//        private readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

//        public virtual IRepository<T> ClearCache()
//        {
//            _readerWriterLockSlim.EnterWriteLock();

//            try
//            {
//                _cache = null;
//            }
//            catch (Exception exception)
//            {
//                // On error, clear the cache.
//                _cache = null;
//                throw;
//            }
//            finally
//            {
//                _readerWriterLockSlim.ExitWriteLock();
//            }

//            return this;
//        }

//        public virtual IRepository<T> Delete(Func<T, bool> match)
//        {
//            LoadCache();

//            var entities = List(match);

//            _readerWriterLockSlim.EnterWriteLock();

//            try
//            {
//                foreach (var entity in entities)
//                {
//                    Remove(entity);
//                    _cache.Remove(entity);
//                }
//            }
//            catch (Exception exception)
//            {
//                // On error, clear the cache.
//                _cache = null;
//                throw;
//            }
//            finally
//            {
//                _readerWriterLockSlim.ExitWriteLock();
//            }

//            return this;
//        }

//        public virtual IRepository<T> Insert(T entity)
//        {
//            LoadCache();

//            _readerWriterLockSlim.EnterWriteLock();

//            try
//            {
//                Add(entity);
//                _cache.Add(entity);
//            }
//            catch (Exception exception)
//            {
//                // On error, clear the cache.
//                _cache = null;
//                throw;
//            }
//            finally
//            {
//                _readerWriterLockSlim.ExitWriteLock();
//            }

//            return this;
//        }

//        public List<T> List(Func<T, bool> where = null)
//        {
//            LoadCache();

//            _readerWriterLockSlim.EnterReadLock();

//            try
//            {
//                // This line prevents the external callers from modifing the list outside this process.
//                return where == null ? _cache.ToList() : _cache.Where(where).ToList();
//            }
//            finally
//            {
//                _readerWriterLockSlim.ExitReadLock();
//            }
//        }

//        private void LoadCache()
//        {
//            _readerWriterLockSlim.EnterWriteLock();

//            try
//            {
//                if (_cache == null)
//                {
//                    _cache = Load();
//                }
//            }
//            finally
//            {
//                _readerWriterLockSlim.ExitWriteLock();
//            }
//        }

//        public virtual IRepository<T> Update(T entity)
//        {
//            // ugh, this method is bad.  Needs to be in a single "transaction", or smarter, or both.

//            LoadCache();

//            _readerWriterLockSlim.EnterWriteLock();

//            try
//            {
//                Edit(entity);
//                _cache.RemoveAll(t => t.Id == entity.Id); // not dry, but prevents an extra save.
//                _cache.Add(entity);
//            }
//            catch (Exception exception)
//            {
//                // On error, clear the cache.
//                _cache = null;
//                throw;
//            }
//            finally
//            {
//                _readerWriterLockSlim.ExitWriteLock();
//            }

//            return this;
//        }

//        protected abstract void Add(T entity);
//        protected abstract void Remove(T entity);
//        protected abstract void Edit(T entity);
//        protected abstract List<T> Load();
//    }

//    public class SqlRepository<T> : DatabaseRepository<T> where T : IEntity
//    {
//        protected override void Add(T entity)
//        {
//            using (var database = Database.Open("TAN"))
//            {
//                database.Insert(entity);
//            }
//        }

//        protected override void Edit(T entity)
//        {
//            using (var database = Database.Open("TAN"))
//            {
//                database.Update(entity);
//            }
//        }

//        protected override List<T> Load()
//        {
//            using (var database = Database.Open("TAN"))
//            {
//                var entities = database.List<T>();
//                return entities.ToList();
//            }
//        }

//        protected override void Remove(T entity)
//        {
//            using (var database = Database.Open("TAN"))
//            {
//                database.Delete<T>("where Id = @0", entity.Id);
//            }
//        }
//    }

//    public class TableRepository<T> : DatabaseRepository<T> where T : IEntity
//    {
//        public class Node
//        {
//            public Guid Id { get; set; }
//            public string EntityType { get; set; }
//            public string Body { get; set; }
//            public DateTime CreatedOn { get; set; }
//            public DateTime UpdatedOn { get; set; }
//        }

//        private string _type = typeof(T).FullName;

//        protected override void Add(T entity)
//        {
//            var node = new Node();
//            node.Body = entity.SerializeToString();
//            node.Id = entity.Id;
//            node.EntityType = _type;
//            node.CreatedOn = node.UpdatedOn = DateTime.UtcNow;

//            using (var database = Database.Open("TAN"))
//            {
//                database.Insert(node);
//            }
//        }

//        protected override void Edit(T entity)
//        {
//            var node = new Node();
//            node.Body = entity.SerializeToString();
//            node.Id = entity.Id;
//            node.EntityType = _type;
//            node.UpdatedOn = DateTime.UtcNow;

//            using (var database = Database.Open("TAN"))
//            {
//                database.Update(node);
//            }
//        }

//        protected override List<T> Load()
//        {
//            using (var database = Database.Open("TAN"))
//            {
//                var nodes = database.List<Node>("where EntityType = @0", _type);

//                var entities = new List<T>();

//                foreach (var node in nodes)
//                {
//                    entities.Add(node.Body.DeserializeFromString<T>());
//                }

//                return entities;
//            }
//        }

//        protected override void Remove(T entity)
//        {
//            var node = new Node();
//            node.Body = entity.SerializeToString();
//            node.Id = entity.Id;
//            node.EntityType = _type;

//            using (var database = Database.Open("TAN"))
//            {
//                database.Delete<Node>("where Id = @0", entity.Id);
//            }
//        }
//    }
//}


//// http://stackoverflow.com/questions/1464883/how-can-i-easily-convert-datareader-to-listt