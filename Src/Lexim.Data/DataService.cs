using System;
using System.Data;
using System.Linq;
using NHibernate;

namespace Lexim.Data
{
    public class NHibernateDataService : IDataService
    {
        private readonly ISession _session;
        private ITransaction _activeTransaction;

        public NHibernateDataService(ISession session)
        {
            _session = session;
        }

        public T GetById<T>(int id) where T : Entity => _session.Get<T>(id);

        public IQueryable<T> Query<T>() where T : Entity => _session.Query<T>();

        public void Save<T>(T entity) where T : Entity
        {
            if (_activeTransaction == null)
                Transaction(() => SaveCore(entity));
            else
                SaveCore(entity);
        }

        private void SaveCore<T>(T entity) where T : Entity
        {
            if (entity.Id == 0)
                entity.CreatedOn = DateTime.Now;
            else
                entity.ModifiedOn = DateTime.Now;

            _session.SaveOrUpdate(entity);
        }

        public void Transaction(Action action, bool relaxIsolationLevel = false)
        {
            _activeTransaction =
                relaxIsolationLevel
                    ? _session.BeginTransaction(IsolationLevel.ReadUncommitted)
                    : _session.BeginTransaction();
            try
            {
                action();
                _activeTransaction.Commit();
            }
            finally
            {
                _activeTransaction.Dispose();
                _activeTransaction = null;
            }
        }

        public void ClearSession() => _session.Clear();
    }
}