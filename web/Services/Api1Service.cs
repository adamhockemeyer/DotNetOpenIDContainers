
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using api1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace web.Services
{
    public static class Api1ServiceExtensions
    {
        public static void AddApi1Service(this IServiceCollection services, IConfiguration configuration)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient<IApi1Service, Api1Service>();
        }
    }

    public class Api1Service : IApi1Service
    {

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly HttpClient _httpClient;
        private readonly string _Api1ServiceScope = string.Empty;
        private readonly string _Api1ServiceBaseAddress = string.Empty;
        private readonly ITokenAcquisition _tokenAcquisition;

        public Api1Service(HttpClient httpClient, IConfiguration configuration, ITokenAcquisition tokenAcquisition, IHttpContextAccessor contextAccessor)
        {
            _httpClient = httpClient;
            _tokenAcquisition = tokenAcquisition;
            _contextAccessor = contextAccessor;
            _Api1ServiceScope = configuration["Api1Service:Scope"];
            _Api1ServiceBaseAddress = configuration["Api1Service:BaseAddress"];
        }

        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            await PrepareAuthenticatedClient();
            var response = await _httpClient.GetAsync($"{_Api1ServiceBaseAddress}/WeatherForecast");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                IEnumerable<WeatherForecast> todolist = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(content);

                return todolist;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        private async Task PrepareAuthenticatedClient()
        {
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _Api1ServiceScope });
            Debug.WriteLine($"access token-{accessToken}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}