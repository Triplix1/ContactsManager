using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContracts
{
    public interface IPersonsRepository
    {
        Task<Person> AddPerson(Person person);

        Task<bool> DeletePerson(Guid guid);

        Task<List<Person>> GetAllPersons();

        Task<Person?> GetPersonById(Guid id);

        Task<List<Person>> GetFilteredPersons(Expression<Func<Person,bool>> expression);

        Task<Person> UpdatePerson(Person person);
    }
}
