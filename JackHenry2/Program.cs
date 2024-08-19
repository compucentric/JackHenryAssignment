using JackHenry2.Helpers;
using JackHenry2.Models;
using JackHenry2.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace JackHenry2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration( (context, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    } )
                    .ConfigureServices((context, services) => 
                    {
                        services.Configure<Config>(context.Configuration.GetSection("AppSettings"));
                        services.AddSingleton<IStatisticsService, StatisticsService>();
                        services.AddSingleton<IPrintingService, PrintingService>();
                        services.AddSingleton<IRedditClientService, RedditClientService>();
                        services.AddSingleton<IPersistenceService, PersistenceService>();
                        services.AddSingleton<IRedditApiClient, RedditApiClient>();
                        services.AddSingleton<App>(); // Register the main application class
                    })
                    .Build();

            var app = host.Services.GetRequiredService<App>();
            await app.RunAsync();
        }

    }

    public class App
    {
        private readonly IPrintingService _printingService;
        private readonly IPersistenceService _persistenceService;
        private readonly IRedditClientService _redditClientService;

        public App(IPrintingService printingService, IRedditClientService redditClientService,  IPersistenceService persistenceService)
        {
            _printingService = printingService;
            _persistenceService = persistenceService;
            _redditClientService = redditClientService;
        }
        private void ShowWelcomeScreen()
        {
            Console.WriteLine("Welcome to Redit Statistsics Collector!");
            Console.WriteLine("");
            Console.WriteLine("Please select one of the following options:");
            Console.WriteLine("");
            Console.WriteLine("'p' - to print statistics for the latest completed batch");
            Console.WriteLine("'p BatchId' - to print statistics for the specific batch (example: p 08-13-2024 15:33:57). Use 'b' option to get BatchIds");
            Console.WriteLine("'b' - to print list all the batches");
            Console.WriteLine("'s' - to print the Reddit Request Limits");
            Console.WriteLine("'n' - to show Program Notes");
            Console.WriteLine("'c' - to clear the screen");
            Console.WriteLine("'q' - to quit.");
        }
        public async Task RunAsync()
        {
            var cts = new CancellationTokenSource();

            try
            {
                await _persistenceService.LoadBatchesAsync();

                ShowWelcomeScreen();

                var monitoringTask = _redditClientService.StartMonitoringAsync(cts.Token);
                var userInputTask = Task.Run(() => ReadUserInput(cts.Token));


                while (true)
                {
                    var completedTask = await Task.WhenAny(monitoringTask, userInputTask);
                    if (completedTask == userInputTask)
                    {
                        var userInput = userInputTask.Result;

                        if (userInput == "p")
                            _printingService.PrintStatistics();
                        else if (userInput[0] == 'p' && userInput.Length > 1)
                        {
                            string batchId = userInput.Substring(1).Trim();
                            _printingService.PrintStatistics(batchId);
                        }
                        else if (userInput == "b")
                            _printingService.PrintAllBatchInfo();

                        else if (userInput == "c")
                        {
                            Console.Clear();
                            ShowWelcomeScreen();
                        }
                        else if (userInput == "q")
                        {
                            cts.Cancel();
                            break;
                        }
                        else if (userInput == "s")
                        {
                            _printingService.PrintRedditRequestLimits();
                        }
                        else if (userInput == "n")
                        {
                            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProgramNotes.txt");

                            if (File.Exists(filePath))
                            {
                                Process.Start("notepad.exe", filePath);
                            }
                            else
                            {
                                Console.WriteLine("File not found: " + filePath);
                            }
                        }
                        else
                            Console.WriteLine("Bad input. Please try again");

                        // Restart user input task
                        userInputTask = Task.Run(() => ReadUserInput(cts.Token));
                    }
                    else
                    {

                        if (monitoringTask.IsFaulted)
                        {
                            Console.WriteLine($"Monitoring task failed: {monitoringTask.Exception?.InnerException?.Message}");
                        }
                        else if (monitoringTask.IsCanceled)
                        {
                            Console.WriteLine("Monitoring task was canceled.");
                        }
                        else
                        {
                            Console.WriteLine("Monitoring task completed.");
                        }
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Monitoring was canceled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Bye");
            }
        }
        private string ReadUserInput(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var userInput = Console.ReadLine();
                if (userInput != null)
                {
                    return userInput;
                }
            }
            return string.Empty; 
        }
    }
}