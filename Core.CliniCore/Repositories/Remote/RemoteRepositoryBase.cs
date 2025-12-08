using System.Net.Http.Json;
using System.Text.Json;

namespace Core.CliniCore.Repositories.Remote
{
    /// <summary>
    /// Base class for remote repositories that communicate with the API via HTTP.
    /// Provides shared HttpClient configuration and JSON serialization helpers.
    /// </summary>
    public abstract class RemoteRepositoryBase
    {
        protected readonly HttpClient _httpClient;
        protected readonly JsonSerializerOptions _jsonOptions;

        protected RemoteRepositoryBase(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Performs a GET request and deserializes the response
        /// </summary>
        protected async Task<T?> GetAsync<T>(string endpoint) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<T>(_jsonOptions).ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        /// <summary>
        /// Performs a GET request and deserializes to a list
        /// </summary>
        protected async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return new List<T>();

                return await response.Content.ReadFromJsonAsync<List<T>>(_jsonOptions).ConfigureAwait(false) ?? new List<T>();
            }
            catch (HttpRequestException)
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// Performs a POST request with JSON body
        /// </summary>
        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
            where TResponse : class
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions).ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        /// <summary>
        /// Performs a POST request without expecting a response body
        /// </summary>
        protected async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Performs a PUT request with JSON body
        /// </summary>
        protected async Task<bool> PutAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Performs a DELETE request
        /// </summary>
        protected async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Synchronous wrapper for async GET operation
        /// </summary>
        protected T? Get<T>(string endpoint) where T : class
            => GetAsync<T>(endpoint).GetAwaiter().GetResult();

        /// <summary>
        /// Synchronous wrapper for async GET list operation
        /// </summary>
        protected List<T> GetList<T>(string endpoint)
            => GetListAsync<T>(endpoint).GetAwaiter().GetResult();

        /// <summary>
        /// Synchronous wrapper for async POST operation
        /// </summary>
        protected TResponse? Post<TRequest, TResponse>(string endpoint, TRequest data) where TResponse : class
            => PostAsync<TRequest, TResponse>(endpoint, data).GetAwaiter().GetResult();

        /// <summary>
        /// Synchronous wrapper for async POST operation without response
        /// </summary>
        protected bool Post<TRequest>(string endpoint, TRequest data)
            => PostAsync(endpoint, data).GetAwaiter().GetResult();

        /// <summary>
        /// Synchronous wrapper for async PUT operation
        /// </summary>
        protected bool Put<TRequest>(string endpoint, TRequest data)
            => PutAsync(endpoint, data).GetAwaiter().GetResult();

        /// <summary>
        /// Synchronous wrapper for async DELETE operation
        /// </summary>
        protected bool Delete(string endpoint)
            => DeleteAsync(endpoint).GetAwaiter().GetResult();
    }
}
