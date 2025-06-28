# be_wrk_feeder

This project is the Feeder service for the OrchestrationDemo solution. It receives commands from the orchestrator, simulates data fetching, and responds with the results via RabbitMQ.

## Features

- Listens for `FeederCommand` messages from the orchestrator
- Simulates asynchronous data fetching and generates random data
- Publishes `FeederResponse` messages with the fetched data or error information
- Handles malformed or null commands robustly
- Logs all major events and errors

## Project Structure

- `FeederWorker.cs` - Main background service implementing the feeder logic
- `Program.cs` - Application entry point and host configuration
- `appsettings.json` / `appsettings.Development.json` - Configuration files for RabbitMQ and other settings

## Running the Service

1. Ensure RabbitMQ is running and accessible as configured in `appsettings.json`.
2. Build and run the project:
   ```sh
   dotnet run
   ```
3. The feeder will listen for commands and respond with simulated data.

## Configuration

Edit `appsettings.json` or `appsettings.Development.json` to set RabbitMQ connection details and other settings.

## Development

- .NET 8.0
- Uses dependency injection and background services
- Requires the `core_lib_messaging` library for RabbitMQ integration

## Related Projects

- `be_wrk_orchestrator` - Orchestrator service
- `be_wrk_writer` - Writer service
- `core_lib_messaging` - Shared messaging library

---

**Note:**  
This feeder is designed for demo and development purposes. In production, replace the simulated data logic with real data fetching and add more robust error handling as needed.