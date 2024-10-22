using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Polly.Timeout;
using System;

namespace ClientSide
{
    public class HedgingOverHedging
    {
        public static ResiliencePipeline GetRegisteredPipeline(IServiceProvider serviceProvider, string pipelineName)
        {
            // Retrieve a ResiliencePipelineProvider that dynamically creates and caches the resilience pipelines
            var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

            // Retrieve your resilience pipeline using the name it was registered with
            ResiliencePipeline pipeline = pipelineProvider.GetPipeline(pipelineName);

            // Alternatively, you can use keyed services to retrieve the resilience pipeline
            pipeline = serviceProvider.GetRequiredKeyedService<ResiliencePipeline>(pipelineName);

            return pipeline;
        }

        public static async Task<bool> ExecuteOnPipeline(IServiceProvider serviceProvider, string pipelineName)
        {
            ResiliencePipeline pipeline = HedgingOverHedging.GetRegisteredPipeline(serviceProvider, pipelineName);

            // Define the tasks
            Func<Task<HttpResponseMessage>> task1 = async () =>
            {
                Console.WriteLine("Executing Task 1");
                await Task.Delay(1000); // Simulate work
                throw new Exception("Task 1 failed");
            };

            // Execute the pipeline
            await pipeline.ExecuteAsync(async context => await task1());

            return true;
        }
    }
}
