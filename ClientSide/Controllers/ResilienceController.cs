using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Registry;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ClientSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResilienceController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHttpClientFactory httpClientFactory;

        public ResilienceController(
            IServiceProvider sp,
            IHttpClientFactory httpClientFactory)
        {
            this.serviceProvider = sp;
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet("ping")]
        public string Ping()
        {
            return "Pong";
        }

        [HttpGet]
        public string PingHedging()
        {
            // Retrieve a ResiliencePipelineProvider that dynamically creates and caches the resilience pipelines
            var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

            // Retrieve your resilience pipeline using the name it was registered with
            ResiliencePipeline pipeline = pipelineProvider.GetPipeline("my-pipeline");

            // Alternatively, you can use keyed services to retrieve the resilience pipeline
            pipeline = serviceProvider.GetRequiredKeyedService<ResiliencePipeline>("my-pipeline");

            // Execute the pipeline
            await pipeline.ExecuteAsync(static async token =>
            {
                // Your custom logic goes here
            });

            HttpClient httpClient = this.httpClientFactory.CreateClient("hedgingOnSameHttpClient");
            
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
