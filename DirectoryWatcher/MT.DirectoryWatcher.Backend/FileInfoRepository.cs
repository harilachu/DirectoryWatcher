using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MT.DirectoryWatcher.Common;

namespace MT.DirectoryWatcher.Backend
{
    public class FileInfoRepository : IRepository
    {
        #region Private members
        private readonly IDatabaseSettings _databaseSettings;
        private IMongoClient _provider;
        private IMongoDatabase _db => this._provider.GetDatabase(_databaseSettings.DatabaseName);
        #endregion

        public FileInfoRepository(IDatabaseSettings databaseSettings)
        {
            this._databaseSettings = databaseSettings ?? throw new ArgumentNullException(nameof(databaseSettings));
            this._provider = new MongoClient(databaseSettings.ConnectionString);
        }

        #region Select

        public T Single<T>(string collectionName, Expression<Func<T, bool>> expression) where T : class, new()
        {
            return _db.GetCollection<T>(collectionName).Find<T>(expression).SingleOrDefault();
        }

        public IQueryable<T> All<T>(string collectionName) where T : class, new()
        {
            return _db.GetCollection<T>(collectionName).AsQueryable();
        }
        #endregion

        #region Add
        public void Add<T>(string collectionName, T item) where T : class, new()
        {
            _db.GetCollection<T>(collectionName).InsertOne(item);
        }

        public void Add<T>(string collectionName, IEnumerable<T> items) where T : class, new()
        {
            _db.GetCollection<T>(collectionName).InsertMany(items);
        }
        #endregion

        #region Update
        public void Update<T>(string collectionName, string fileName, string newHash) where T : class, new()
        {
            var collection= _db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("FileName", fileName);
            var update = Builders<BsonDocument>.Update.Set("NewHash", newHash);
            collection.UpdateOne(filter, update);
            var update2 = Builders<BsonDocument>.Update.Set("FileChange", ChangeType.Modified.ToString());
            collection.UpdateOne(filter, update2);
        }

        public void UpdateFile<T>(string collectionName, string oldFileName, string newFileName, string directoryPath) where T : class, new()
        {
            var collection = _db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("FileName", oldFileName);
            var update = Builders<BsonDocument>.Update.Set("FileName", newFileName);
            collection.UpdateOne(filter, update);
        }

        #endregion

        #region Delete
        public void Delete<T>(string collectionName, Expression<Func<T, bool>> expression) where T : class, new()
        {
            _db.GetCollection<T>(collectionName).DeleteOne(expression);
        }

        public void Delete<T>(string collectionName, IEnumerable<Expression<Func<T, bool>>> expressions) where T : class, new()
        {
            expressions.ToList().ForEach(i => Delete(collectionName, i));
        }

        public void DeleteAll<T>(string collectionName) where T : class, new()
        {
            _db.DropCollection(collectionName);
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RegistryChangeMonitor()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        
        #endregion
    }
}
