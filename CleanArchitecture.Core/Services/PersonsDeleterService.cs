using System;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using System.ComponentModel.DataAnnotations;
using Services.Helpers;
using ServiceContracts.Enums;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class PersonsDeleterService : IPersonsDeleterService
    {
        //private field
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsGetterService> _logger;

        //constructor
        public PersonsDeleterService(IPersonsRepository personsDbContext, ILogger<PersonsGetterService> logger)
        {
            _personsRepository = personsDbContext;
            _logger = logger;
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            if (personID == null)
            {
                throw new ArgumentNullException(nameof(personID));
            }

            var result = await _personsRepository.DeletePerson(personID.Value);

            return result;
        }
    }
}
