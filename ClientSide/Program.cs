using Polly;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.Logging;
using Polly.Hedging;

namespace ClientSide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Latency Mode Hedging: Requests are on the same httpclient
            // [Request A]----------------------------------->
            //            <---3s--->[Request B]-------------->
            builder.Services.AddHttpClient("leafLayerHedging").AddResilienceHandler(
                pipelineName: "leafLayerHedgingHandler",
                configure: builder =>
                {
                    // Polly Hedging Strategy: Latency Mode
                    builder.AddHedging(new HedgingStrategyOptions<HttpResponseMessage>
                    {
                        MaxHedgedAttempts = 2,

                        // In this layer, we don't use DelayGenerator & Dynamic Mode
                        Delay = TimeSpan.FromSeconds(3),
                    });
                });

            // Add Named HttpClient
            // Latency Mode Hedging with Hedging
            // [Requesst
            builder.Services.AddHttpClient("resilience").AddResilienceHandler(
                "resilienceHandler",
                static builder =>
                {
                    // See: https://www.pollydocs.org/strategies/retry.html
                    builder.AddRetry(new HttpRetryStrategyOptions
                    {
                        BackoffType = DelayBackoffType.Exponential,
                        MaxRetryAttempts = 2,
                        Delay = TimeSpan.FromMilliseconds(100),
                        UseJitter = false,
                        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()

                            .Handle<SocketException>()
                            .HandleResult(response => response.StatusCode == HttpStatusCode.RequestTimeout)
                            .HandleResult(response =>
                            {
                                Console.WriteLine($"Response status code: {response.StatusCode}");
                                return false;
                            }),
                        OnRetry = static args =>
                        {
                            Console.WriteLine($"retrying");
                            return default;
                        },
                    });

                    builder.AddHedging(new HedgingStrategyOptions<HttpResponseMessage>
                    {
                        MaxHedgedAttempts = 3,

                        // Using DelayGenerator to switch from Parallel mode to Fallback mode
                        // https://www.pollydocs.org/strategies/hedging.html
                        DelayGenerator = args =>
                        {
                            //var delay = args.AttemptNumber switch
                            //{
                            //    0 or 1 => TimeSpan.Zero, // Parallel mode
                            //    _ => TimeSpan.FromSeconds(-1) // switch to Fallback mode
                            //};
                            var delay = args.AttemptNumber switch
                            {
                                0 or 1 => TimeSpan.Zero, // Parallel mode
                                _ => TimeSpan.FromSeconds(-1) // switch to Fallback mode
                            };

                            return new ValueTask<TimeSpan>(delay);
                        },
                        OnHedging = static args =>
                        {
                            Console.WriteLine($"OnHedging: Attempt number {args.AttemptNumber}");
                            return default;
                        }
                    });
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
