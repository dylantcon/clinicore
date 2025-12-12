using Core.CliniCore.Domain;
using Core.CliniCore.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Repositories
{
    /// <summary>
    /// Base repository interface for all entity types
    /// </summary>
    public interface IRepository<T> where T : IIdentifiable
    {
        T? GetById(Guid id);
        IEnumerable<T> GetAll();
        void Add(T entity);
        void Update(T entity);
        void Delete(Guid id);
        IEnumerable<T> Search(string query);
    }
}
