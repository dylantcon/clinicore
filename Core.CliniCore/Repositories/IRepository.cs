using Core.CliniCore.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Repositories
{
    public interface IRepository<T> where T : class, IIdentifiable
    {
        T? GetById(Guid id);
        IEnumerable<T> GetAll();
        void Add(T entity);
        void Update(T entity);
        void Delete(Guid id);
        IEnumerable<T> Search(string query);
    }
}
