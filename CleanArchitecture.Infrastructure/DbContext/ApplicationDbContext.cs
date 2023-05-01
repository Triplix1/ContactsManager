using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CleanArchitecture.Core.Domain.IdentityEntities;

namespace Entities
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        public virtual DbSet<Country> Countries { get; set; }

        public virtual DbSet<Person> Persons { get; set; }

        public List<Person> sp_GetPersons()
        {
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPerson]").ToList();
        }

        public int sp_InsertPerson(Person person)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PersonId",person.PersonID),
                new SqlParameter("@PersonName", person.PersonName),
                new SqlParameter("@Email", person.Email),
                new SqlParameter("@DateOfBirth", person.DateOfBirth),
                new SqlParameter("@Gender", person.Gender),
                new SqlParameter("@CountryID", person.CountryID),
                new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters),
                new SqlParameter("@Address", person.Address)
            };

            return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryId, @Address, @ReceiveNewsLetters");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().ToTable("Country");
            modelBuilder.Entity<Person>().ToTable("Persons");

            string countriesJSON = File.ReadAllText("countries.json");

            var countries = JsonSerializer.Deserialize<List<Country>>(countriesJSON);

            foreach (var country in countries)
            {
                modelBuilder.Entity<Country>().HasData(country);
            }

            string personsJSON = File.ReadAllText("persons.json");

            var persons = JsonSerializer.Deserialize<List<Person>>(personsJSON);

            foreach (var person in persons)
            {
                modelBuilder.Entity<Person>().HasData(person);
            }

            modelBuilder.Entity<Person>().Property(t => t.TIN)
                .HasColumnType("varchar(8)")
                .HasColumnName("TaxIdentificationNumber")
                .HasDefaultValue("asc12345");
        }
    }
}
