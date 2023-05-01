using Entities;

namespace RepositoryContracts
{
    /// <summary>
    /// Represents business logic for manipulating Country entity
    /// </summary>
    public interface ICountriesRepository
    {
        /// <summary>
        /// Adds a country object to the list of countries
        /// </summary>
        /// <param name="countryAddRequest">Country object to add</param>
        /// <returns>Returns the country object after adding it (including newly generated country id)</returns>
        Task<Country> AddCountry(Country country);

        /// <summary>
        /// Returns all countries from the list
        /// </summary>
        /// <returns>All countries from the list as List of CountryResponse</CountryResponse></returns>
        Task<List<Country>> GetAllCountries();


        /// <summary>
        /// Returns a country object based on the given country id
        /// </summary>
        /// <param name="countryID">CountryID (guid) to search</param>
        /// <returns>Matching country as CountryResponse object</returns>
        Task<Country?> GetCountryByCountryID(Guid countryID);

        Task<Country?> GetCountryByCountryName(string countryName);
    }
}