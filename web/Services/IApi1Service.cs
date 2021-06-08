
using System.Collections.Generic;
using System.Threading.Tasks;
using api1;

namespace web.Services
{
    public interface IApi1Service
    {
        Task<IEnumerable<WeatherForecast>> GetAsync();
    }
}