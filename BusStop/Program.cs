using System;
using System.Threading;
using System.Threading.Tasks;
using BusStop.Domain;
using BusStop.Domain.IO;
using BusStop.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BusStop
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var serviceProvider = GetServiceProvider();
                using var serviceScope = serviceProvider.CreateScope();
                var services = serviceScope.ServiceProvider;

                string inputFilePath = GetInputFilePath(args);
                var cancellationToken = new CancellationTokenSource();

                var timeTableProcessor = services.GetRequiredService<TimeTableProcessor>();

                await timeTableProcessor.ProcessTimeTableAsync(inputFilePath, cancellationToken.Token).ConfigureAwait(false);

                Console.WriteLine("Processing done successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(FormattableString.Invariant($"Processing stopped unexpectedly.{Environment.NewLine}{ex.Message}"));
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("(Press Enter to close the application.)");
                Console.ReadLine();
            }
        }

        private static string GetInputFilePath(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("Input file path is missing");
            }

            return args[0];
        }

        private static IServiceProvider GetServiceProvider()
        {
            var builder = new HostBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<IFileReader, FileReader>();
                    services.AddSingleton<IFileWriter, FileWriter>();
                    services.AddTransient<ITimeTableReader, TimeTableReader>();
                    services.AddTransient<ITimeTableWriter, TimeTableWriter>();
                    services.AddTransient<TimeTableProcessor>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();

            return host.Services;
        }
    }
}
