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
    public class PersonsAdderService : IPersonsAdderService
    {
        //private field
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsGetterService> _logger;

        //constructor
        public PersonsAdderService(IPersonsRepository personsDbContext, ILogger<PersonsGetterService> logger)
        {
            _personsRepository = personsDbContext;
            _logger = logger;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            //check if PersonAddRequest is not null
            if (personAddRequest == null)
            {
                throw new ArgumentNullException(nameof(personAddRequest));
            }

            //Model validation
            ValidationHelper.ModelValidation(personAddRequest);

            //convert personAddRequest into Person type
            Person person = personAddRequest.ToPerson();

            //generate PersonID
            person.PersonID = Guid.NewGuid();

            //add person object to persons list
            await _personsRepository.AddPerson(person);

            //convert the Person object into PersonResponse type
            return person.ToPersonResponse();
        }                
    }
}
