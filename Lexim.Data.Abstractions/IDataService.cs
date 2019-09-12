using System;
using System.Linq;

namespace Lexim.Data
{
    public interface IDataService
    {
        T GetById<T>(int id) where T : Entity;
        IQueryable<T> Query<T>() where T : Entity;
        void Save<T>(T entity) where T : Entity;
        void Transaction(Action action, bool relaxIsolationLevel = false);
    }
}