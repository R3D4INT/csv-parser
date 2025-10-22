using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParserCSV.Services;

namespace ParserCSV
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;

                services.AddSingleton<ITripRepository>(sp =>
                {
                    string? connectionString = configuration.GetConnectionString("DefaultConnection");

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException(
                            "Connection string 'DefaultConnection' is not set in appsettings.json");
                    }

                    return new SqlTripRepository(connectionString);
                });

                services.AddTransient<EtlOrchestrator>();
            });

            var host = builder.Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var config = services.GetRequiredService<IConfiguration>();

                try
                {
                    string inputPath = config.GetValue<string>("EtlSettings:InputCsvPath")
                                       ?? throw new InvalidOperationException("InputCsvPath not set.");

                    string outputPath = config.GetValue<string>("EtlSettings:DuplicatesCsvPath")
                                        ?? throw new InvalidOperationException("DuplicatesCsvPath not set.");

                    int batchSize = config.GetValue<int>("EtlSettings:BatchSize", 50000);

                    if (!File.Exists(inputPath))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: Input file not found at '{inputPath}'");
                        Console.ResetColor();
                        return;
                    }

                    var orchestrator = services.GetRequiredService<EtlOrchestrator>();

                    await orchestrator.RunAsync(inputPath, outputPath, batchSize);

                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n--- A critical error occurred ---");
                    Console.WriteLine(ex.ToString());
                    Console.ResetColor();
                }
            }
        }
    }
}
