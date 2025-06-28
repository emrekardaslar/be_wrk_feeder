// be_wrk_feeder/Program.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection; // Important for AddRabbitMqService

// --- IMPORTANT: UPDATED USING DIRECTIVES ---
using be_wrk_feeder; // To find FeederWorker
using core_lib_messaging.RabbitMq; // To find AddRabbitMqService extension method (implicitly via DI namespace)
// --- END UPDATED USING DIRECTIVES ---

namespace be_wrk_feeder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    // You can add more logging providers here
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Register our custom RabbitMQ service
                    services.AddRabbitMqService(); // This is our extension method from core_lib_messaging!

                    // Register our FeederWorker as a hosted service
                    services.AddHostedService<FeederWorker>();
                });
    }
}