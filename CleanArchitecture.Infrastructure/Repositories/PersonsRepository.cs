using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PersonsRepository> _logger;

        public PersonsRepository(ApplicationDbContext context, ILogger<PersonsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Person> AddPerson(Person person)
        {
            _logger.LogInformation("AddPerson method in PersonsRepository");
            _context.Persons.Add(person);

            await _context.SaveChangesAsync();

            return person;
        }

        public async Task<bool> DeletePerson(Guid guid)
        {
            _logger.LogInformation("DeletePerson method in PersonsRepository");
            var persons = _context.Persons.Where(p => p.PersonID == guid);

            _context.Persons.RemoveRange(persons);

            await _context.SaveChangesAsync();

            return persons.Count() > 0;
        }

        public async Task<List<Person>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons method in PersonsRepository");
            return await _context.Persons.ToListAsync();
        }

        public async Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> expression)
        {
            _logger.LogInformation("GetFilteredPersons method in PersonsRepository");
            return await _context.Persons.Include(x => x.Country).Where(expression).ToListAsync();
        }

        public async Task<Person?> GetPersonById(Guid id)
        {
            _logger.LogInformation("GetPersonById method in PersonsRepository");
            return await _context.Persons.Include(p => p.Country).FirstOrDefaultAsync(p => p.PersonID == id);
        }

        public async Task<Person> UpdatePerson(Person person)
        {
            _logger.LogInformation("UpdatePerson method in PersonsRepository");
            var matchedPerson = await _context.Persons.FirstOrDefaultAsync(p => p.PersonID == person.PersonID);

            if(matchedPerson == null)
                return person;

            matchedPerson.Country = person.Country;
            matchedPerson.Address = person.Address;
            matchedPerson.Gender = person.Gender;
            matchedPerson.TIN = person.TIN;
            matchedPerson.CountryID = person.CountryID;
            matchedPerson.Email = person.Email;
            matchedPerson.DateOfBirth = person.DateOfBirth;
            matchedPerson.PersonName = person.PersonName;
            matchedPerson.ReceiveNewsLetters = person.ReceiveNewsLetters;

            await _context.SaveChangesAsync();

            return person;
        }   

    }
}
