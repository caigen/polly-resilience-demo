using Microsoft.AspNetCore.Mvc;
using Polly;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ClientSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResilienceController : ControllerBase
    {
        [HttpGet("ping")]
        public string Ping()
        {
            return "Pong";
        }

        [HttpGet]
        public string PingHedging()
        {
            
            return "Pong";
        }


        // GET: api/<ResilienceController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ResilienceController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ResilienceController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ResilienceController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ResilienceController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
