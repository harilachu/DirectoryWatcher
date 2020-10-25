using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MT.DirectoryWatcher.Common
{
    public interface IRepository : IDisposable
    {
        T Single<T>(string collectionName, Expression<Func<T, bool>> expression) where T : class, new();
        IQueryable<T> All<T>(string collectionName) where T : class, new();
        void Add<T>(string collectionName, T item) where T : class, new();
        void Add<T>(string collectionName, IEnumerable<T> items) where T : class, new();
        void Update<T>(string collectionName, string fileName, string newHash) where T : class, new();
        void Delete<T>(string collectionName, Expression<Func<T, bool>> expression) where T : class, new();
        void Delete<T>(string collectionName, IEnumerable<Expression<Func<T, bool>>> expressions) where T : class, new();
        void DeleteAll<T>(string collectionName) where T : class, new();
    }
}