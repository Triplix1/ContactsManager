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
    public class PersonsUpdaterService : IPersonsUpdaterService
    {
        //private field
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsGetterService> _logger;

        //constructor
        public PersonsUpdaterService(IPersonsRepository personsDbContext, ILogger<PersonsGetterService> logger)
        {
            _personsRepository = personsDbContext;
            _logger = logger;
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            _logger.LogInformation("UpdatePerson method in PersonsService");

            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(Person));

            //validation
            ValidationHelper.ModelValidation(personUpdateRequest);

            var person = await _personsRepository.UpdatePerson(personUpdateRequest.ToPerson());

            return person.ToPersonResponse();
        }
    }
}
