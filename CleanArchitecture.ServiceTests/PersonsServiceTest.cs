﻿using System;
using System.Collections.Generic;
using Xunit;
using ServiceContracts;
using Entities;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using EntityFrameworkCoreMock;
using AutoFixture;
using FluentAssertions;
using RepositoryContracts;
using Moq;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        //private fields
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly ICountriesService _countriesService;

        private readonly Mock<IPersonsRepository> _personsRepositoryMock;
        private readonly IPersonsRepository _personsRepository;

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        private readonly ILogger<PersonsAdderService> _logger;

        //constructor
        public PersonsServiceTest(ITestOutputHelper testOutputHelper, ILogger<PersonsAdderService> logger)
        {
            _fixture = new Fixture();
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;

            var diagnosticContextMock = new Mock<IDiagnosticContext>();
            var loggerMock = new Mock<ILogger<PersonsGetterService>>();

            _personsGetterService = new PersonsGetterService(_personsRepository, loggerMock.Object);

            _personsAdderService = new PersonsAdderService(_personsRepository, loggerMock.Object);

            _personsDeleterService = new PersonsDeleterService(_personsRepository, loggerMock.Object);

            _personsSorterService = new PersonsSorterService(_personsRepository, loggerMock.Object);

            _personsUpdaterService = new PersonsUpdaterService(_personsRepository, loggerMock.Object);

            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson

        //When we supply null value as PersonAddRequest, it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;

            //Act
            var action = async () =>
            {
                await _personsAdderService.AddPerson(personAddRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }


        //When we supply null value as PersonName, it should throw ArgumentException
        [Fact]
        public async Task AddPerson_PersonNameIsNull()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "something@gmail.com")
                .With(p => p.PersonName, null as string)
                .Create();

            //Act
            var action = async () =>
            {
                await _personsAdderService.AddPerson(personAddRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When we supply proper person details, it should insert the person into the persons list; and it should return an object of PersonResponse, which includes with the newly generated person id
        [Fact]
        public async Task AddPerson_ProperPersonDetails()
        {
            //Arrange
            var personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(p => p.Email, "something@gmail.com")
                .Create();

            var person = personAddRequest.ToPerson();

            var person_response_expected = person.ToPersonResponse();

            _personsRepositoryMock.Setup(p => p.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

            //Act
            PersonResponse person_response_from_add = await _personsAdderService.AddPerson(personAddRequest);

            person_response_expected.PersonID = person_response_from_add.PersonID;

            //Assert
            person_response_from_add.PersonID.Should().NotBe(Guid.Empty);
            person_response_expected.Should().Be(person_response_from_add);
        }

        #endregion


        #region GetPersonByPersonID

        //If we supply null as PersonID, it should return null as PersonResponse
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID()
        {
            //Arrange
            Guid? personID = null;

            //Act
            PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonID(personID);

            //Assert
            //Assert.Null(person_response_from_get);

            person_response_from_get.Should().BeNull();
        }


        //If we supply a valid person id, it should return the valid person details as PersonResponse object
        [Fact]
        public async Task GetPersonByPersonID_WithPersonID()
        {
            //Arange            
            Person person = _fixture.Build<Person>()
                .With(p => p.Email, "something@gmail.com")
                .With(p => p.Country, null as Country)
                .Create();

            var person_response_expected = person.ToPersonResponse();

            _personsRepositoryMock.Setup(p => p.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);

            PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonID(person.PersonID);

            //Assert
            //Assert.Equal(person_response_from_add, person_response_from_get);

            person_response_expected.Should().Be(person_response_from_get);
        }

        #endregion


        #region GetAllPersons

        //The GetAllPersons() should return an empty list by default
        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            //Arrange
            _personsRepositoryMock.Setup(p => p.GetAllPersons()).ReturnsAsync(new List<Person>());

            //Act
            List<PersonResponse> persons_from_get = await _personsGetterService.GetAllPersons();

            //Assert
            //Assert.Empty(persons_from_get);

            persons_from_get.Should().BeEmpty();
        }


        //First, we will add few persons; and then when we call GetAllPersons(), it should return the same persons that were added
        [Fact]
        public async Task GetAllPersons_AddFewPersons()
        {
            //Arrange
            var persons = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(p => p.Email, "example_1@example.com")
                    .With(p => p.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(p => p.Email, "example_2@example.com")
                    .With(p => p.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(p => p.Email, "example_3@example.com")
                    .With(p => p.Country, null as Country)
                    .Create()
            };

            List<PersonResponse> person_response_list_expected = persons.Select(p => p.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(p => p.GetAllPersons()).ReturnsAsync(persons);

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Act
            List<PersonResponse> persons_list_from_get = await _personsGetterService.GetAllPersons();

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_list_from_get)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert
            persons_list_from_get.Should().BeEquivalentTo(person_response_list_expected);
        }
        #endregion


        #region GetFilteredPersons

        //If the search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText()
        {
            //Arrange
            //Arrange

            var persons = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(p => p.Email, "example_1@example.com")
                    .With(p => p.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(p => p.Email, "example_2@example.com")
                    .With(p => p.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(p => p.Email, "example_3@example.com")
                    .With(p => p.Country, null as Country)
                    .Create()
            };

            List<PersonResponse> person_response_list_expected = persons.Select(p => p.ToPersonResponse()).ToList();

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock.Setup(p => p.GetAllPersons()).ReturnsAsync(persons);

            //Act
            List<PersonResponse> persons_list_from_search = await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert
            persons_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
        }


        //First we will add few persons; and then we will search based on person name with some search string. It should return the matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName()
        {
            //Arrange               
            var persons = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(p => p.PersonName, "Smith")
                    .With(p => p.Email, "example_1@example.com")
                    .With(p => p.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(p => p.PersonName, "Mary")
                    .With(p => p.Email, "example_2@example.com")
                    .With(p => p.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(p => p.PersonName, "Rahman")
                    .With(p => p.Email, "example_3@example.com")
                    .With(p => p.Country, null as Country)
                    .Create()
            };

            _personsRepositoryMock.Setup(p => p.GetAllPersons()).ReturnsAsync(persons);

            //Act
            List<PersonResponse> persons_list_from_search = await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "ma");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert
            persons_list_from_search.Should().OnlyContain(p => p.PersonName == null || p.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase));
        }

        #endregion


        #region GetSortedPersons

        //When we sort based on PersonName in DESC, it should return persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons()
        {
            //Arrange
            var persons = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(p => p.PersonName, "Smith")
                    .With(p => p.Email, "example_1@example.com")
                    .With(p => p.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(p => p.PersonName, "Mary")
                    .With(p => p.Email, "example_2@example.com")
                    .With(p => p.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(p => p.PersonName, "Rahman")
                    .With(p => p.Email, "example_3@example.com")
                    .With(p => p.Country, null as Country)
                    .Create()
            };

            List<PersonResponse> person_response_list_expected = persons.Select(p => p.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(p => p.GetAllPersons()).ReturnsAsync(persons);

            var allPersons = await _personsGetterService.GetAllPersons();

            //Act
            List<PersonResponse> persons_list_from_sort = await _personsSorterService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_list_from_sort)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            person_response_list_expected = person_response_list_expected.OrderByDescending(temp => temp.PersonName).ToList();

            //Assert
            //for (int i = 0; i < person_response_list_from_add.Count; i++)
            //{
            //    Assert.Equal(person_response_list_from_add[i], persons_list_from_sort[i]);
            //}

            persons_list_from_sort.Should().BeEquivalentTo(person_response_list_expected);
            persons_list_from_sort.Should().BeInDescendingOrder(temp => temp.PersonName);
        }
        #endregion


        #region UpdatePerson

        //When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson()
        {
            //Arrange
            PersonUpdateRequest? person_update_request = null;

            //Act
            var action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }


        //When we supply invalid person id, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonID()
        {
            //Arrange
            PersonUpdateRequest? person_update_request = new PersonUpdateRequest() { PersonID = Guid.NewGuid() };

            //Act
            var action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };

            //Assert 
            await action.Should().ThrowAsync<ArgumentException>();
        }


        //When PersonName is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull()
        {
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Email, "someone@example.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse person_response_from_add = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();


            //Act
            var action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }


        //First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetailsUpdation()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(p => p.Email, "example_1@example.com")
                .With(p => p.Country, null as Country)
                .With(P => P.Gender, "Male")
                .Create();

            PersonResponse person_response_from_add = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();

            _personsRepositoryMock.Setup(p => p.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);

            _personsRepositoryMock.Setup(p => p.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            PersonResponse person_response_from_update = await _personsUpdaterService.UpdatePerson(person_update_request);

            //Assert
            person_response_from_update.Should().Be(person_response_from_add);

        }

        #endregion


        #region DeletePerson

        //If you supply an valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonID()
        {
            //Arrange
            Person person_add_request = _fixture.Build<Person>()
                .With(p => p.PersonName, "Smith")
                .With(p => p.Email, "example_1@example.com")
                .With(p => p.Country, null as Country)
                .Create();

            PersonResponse person_response_from_add = person_add_request.ToPersonResponse();

            _personsRepositoryMock.Setup(p => p.DeletePerson(It.IsAny<Guid>())).ReturnsAsync(true);

            //Act
            bool isDeleted = await _personsDeleterService.DeletePerson(person_response_from_add.PersonID);

            //Assert
            isDeleted.Should().BeTrue();
        }


        //If you supply an invalid PersonID, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonID()
        {
            //Act
            bool isDeleted = await _personsDeleterService.DeletePerson(Guid.NewGuid());

            //Assert
            isDeleted.Should().BeFalse();
        }

        #endregion
    }
}