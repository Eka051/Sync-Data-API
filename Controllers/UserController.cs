using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public IEnumerable<Person> Get()
        {
            return Person.people;
        }

        [HttpGet("{id}", Name = "Get User By Id")]
        public ActionResult Get(int id)
        {
            Person? foundPerson = Person.people.FirstOrDefault(x => x.Id == id);
            if (foundPerson == null)
            {
                return NotFound();
            }
            return Ok(foundPerson);
        }

        [HttpGet("detail/{id}", Name = "Get User Detail By Id")]
        public async Task<ActionResult> GetDetail(int id)
        {
            Person? foundPerson = Person.people.FirstOrDefault(x => x.Id == id);
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

        [HttpPost("sync/{id}", Name = "Sync Data From API")]
        public async Task<ActionResult> SyncDataFromAPI(int id)
        {
            var response = await _httpClient!.GetFromJsonAsync<PersonNew>($"https://dummy-user-tan.vercel.app/user/{id}");
            if (response == null)
            {
                return NotFound("Data tidak ditemukan di API");
            }

            var existing = await _context!.PersonNews.FindAsync(id);
            if (existing != null)
            {
                existing.Name = response.Name;
                existing.Saldo = response.Saldo;
                existing.Hutang = response.Hutang;
                await _context.SaveChangesAsync();
                return Ok("Data berhasil disinkronkan");
            }
            else
            {
                _context.PersonNews.Add(response);
            }
            await _context.SaveChangesAsync();
            return Ok("Data berhasil disinkronkan dan disimpan ke database");
        }
    }
}
