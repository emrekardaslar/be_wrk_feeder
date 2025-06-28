// be_wrk_feeder/FeederWorker.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
// REMOVED: using RabbitMQ.Client; // This line is no longer needed here!
using System;
using System.Text.Json; // For JsonException
using System.Threading;
using System.Threading.Tasks;

// --- UPDATED USING DIRECTIVES FOR core_lib_messaging ---
using core_lib_messaging.Models;       // For FeederCommand, FeederResponse, BaseMessage
using core_lib_messaging.RabbitMq;     // Now this brings in MessageDeliveryContext
using core_lib_messaging.Serialization; // For JsonSerializationHelper
// --- END UPDATED USING DIRECTIVES ---

namespace be_wrk_feeder
{
    public class FeederWorker : BackgroundService
    {
        private readonly ILogger<FeederWorker> _logger;
        private readonly IRabbitMqService _rabbitMqService; // Injected service

        public FeederWorker(ILogger<FeederWorker> logger, IRabbitMqService rabbitMqService)
        {
            _logger = logger;
            _rabbitMqService = rabbitMqService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("be_wrk_feeder: Feeder Worker starting...");

            try
            {
                // Declare Queues using the service
                _rabbitMqService.DeclareQueueWithDeadLetter(RabbitMqConfig.ReqFeederQueue);
                _rabbitMqService.DeclareQueueWithDeadLetter(RabbitMqConfig.ResFeederQueue);

                // Set up Consumer using the service
                _rabbitMqService.Consume<FeederCommand>(RabbitMqConfig.ReqFeederQueue, OnFeederCommandReceived, autoAck: false);

                _logger.LogInformation($"be_wrk_feeder: Listening for commands on '{RabbitMqConfig.ReqFeederQueue}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "be_wrk_feeder: Error starting Feeder Worker. Shutting down.");
                throw; // Re-throw to prevent host from starting successfully
            }

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("be_wrk_feeder: Feeder Worker running. Press Ctrl+C to stop.");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            _logger.LogInformation("be_wrk_feeder: Feeder Worker detected cancellation request.");
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("be_wrk_feeder: Feeder Worker stopping...");
            // The IRabbitMqService will be disposed by the DI container
            _logger.LogInformation("be_wrk_feeder: Feeder Worker stopped gracefully.");
            return base.StopAsync(cancellationToken);
        }

        // --- IMPORTANT CHANGE: Parameter type updated from BasicDeliverEventArgs to MessageDeliveryContext ---
        private async Task OnFeederCommandReceived(FeederCommand? command, MessageDeliveryContext context)
        {
            if (command == null)
            {
                _logger.LogWarning("be_wrk_feeder: [!] Error: Received null or malformed FeederCommand. Nacking.");
                _rabbitMqService.Nack(context.DeliveryTag, requeue: false); // CHANGED: Use context.DeliveryTag
                return;
            }

            _logger.LogInformation($"be_wrk_feeder: [x] Processing FeederCommand (CorrelationId: {command.CorrelationId}) - CommandType: {command.CommandType}");

            // Simulate fetching data asynchronously
            await Task.Delay(1000);
            string simulatedData = $"{{ \"id\": \"{Guid.NewGuid()}\", \"name\": \"SimulatedData-{new Random().Next(1000, 9999)}\", \"value\": {new Random().Next(1, 100)} }}";

            // Prepare response
            var response = new FeederResponse
            {
                CorrelationId = command.CorrelationId,
                IsSuccess = true,
                FetchedData = simulatedData
            };

            // Simulate a random failure (e.g., 15% chance)
            if (new Random().Next(1, 101) <= 15)
            {
                _logger.LogWarning($"be_wrk_feeder: [!] Simulated Feeder failure for CorrelationId: {command.CorrelationId}");
                response.IsSuccess = false;
                response.ErrorMessage = "Simulated data fetching failure. Source unavailable.";
                response.FetchedData = null;
            }

            // Publish response using the injected service
            await _rabbitMqService.PublishAsync(RabbitMqConfig.ResFeederQueue, response);

            // Acknowledge the received command message
            _rabbitMqService.Ack(context.DeliveryTag); // CHANGED: Use context.DeliveryTag

            _logger.LogInformation($"be_wrk_feeder: [x] Completed processing for CorrelationId: {response.CorrelationId}. IsSuccess: {response.IsSuccess}");
        }
    }
}