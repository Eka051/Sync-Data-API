using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sync_Data_API.Data;
using Sync_Data_API.Models;

namespace Sync_Data_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public UserController(IHttpClientFactory httpClientFactory, AppDbContext context)
        {
            _httpClient = httpClientFactory.CreateClient();
            _context = context;
        }

        [HttpGet(Name = "Get All User")]
        public async Task<ActionResult<IEnumerable<Person>>> Get()
        {
            var people = await _context.Persons.ToListAsync();
            if (people == null)
            {
                return NotFound("Data tidak ditemukan");
            }
            return Ok(people);
        }

        [HttpGet("{id}", Name = "Get User By Id")]
        public async Task<ActionResult> Get(int id)
        {
            var foundPerson = await _context.Persons.FindAsync(id);
            if (foundPerson == null)
            {
                return NotFound();
            }
            return Ok(foundPerson);
        }

        [HttpGet("detail/{id}", Name = "Get User Detail By Id")]
        public async Task<ActionResult> GetDetail(int id)
        {
            var foundPerson = await _context.Persons.FindAsync(id);
            if (foundPerson == null)
            {
                return NotFound();
            }
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://dummy-user-tan.vercel.app/user/{id}";
                HttpResponseMessage response = await client.GetAsync(url);
                PersonDetail personDetail;
                if (response.IsSuccessStatusCode)
                {
                    PersonDetailFromAPI? personDetailFromAPI = await response.Content.ReadFromJsonAsync<PersonDetailFromAPI>();
                    if (personDetailFromAPI != null)
                    {
                        personDetail = new PersonDetail
                        {
                            Id = foundPerson.Id,
                            Name = foundPerson.Name,
                            Age = foundPerson.Age,
                            Detail = new Detail
                            {
                                Saldo = personDetailFromAPI.Saldo,
                                Hutang = personDetailFromAPI.Hutang
                            }
                        };
                        return Ok(personDetail);
                    }
                }
                personDetail = new PersonDetail
                {
                    Id = foundPerson.Id,
                    Name = foundPerson.Name,
                    Age = foundPerson.Age,
                    Detail = null
                };
                return Ok(personDetail);
            }
        }

        [HttpPost("sync", Name = "Sync Data From API")]
        public async Task<ActionResult> SyncDataFromAPI()
        {
            var response = await _httpClient!.GetFromJsonAsync<List<PersonNew>>($"https://dummy-user-tan.vercel.app/user");
            if (response == null)
            {
                return NotFound("Data tidak ditemukan di API");
            }

            foreach (var item in response)
            {
                var existing = await _context.PersonNews.FindAsync(item.Id);
                if (existing != null)
                {
                    existing.Name = item.Name;
                    existing.Saldo = item.Saldo;
                    existing.Hutang = item.Hutang;
                }
                else
                {
                    _context.PersonNews.Add(item);
                }
            };
            await _context.SaveChangesAsync();
            return Ok(response);
        }
    }
}
