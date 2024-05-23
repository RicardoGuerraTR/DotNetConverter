using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ConsoleHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private FileUploader.FileUploader _fileUploader;
    private CancellationTokenSource _cancellationTokenSource; // Declare at the class level

    private Task? _applicationTask;
    private int? _exitCode;

    public ConsoleHostedService(
        ILogger<ConsoleHostedService> logger,
        IHostApplicationLifetime appLifetime,
        FileUploader.FileUploader fileUploader)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _fileUploader = fileUploader;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug($"Starting with arguments: {string.Join(" ", Environment.GetCommandLineArgs())}");

        _appLifetime.ApplicationStarted.Register(() =>
        {
            _logger.LogDebug("Application has started");
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _applicationTask = Task.Run(async () =>
            {
                try
                {
                    //Do some code
                    _logger.LogDebug($"Started!!");
                    
                    await CallMenu();                    
                }
                catch (TaskCanceledException)
                {
                    // This means the application is shutting down, so just swallow this exception
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception!");
                    _exitCode = 1;
                }
                finally
                {
                    // Stop the application once the work is done
                    _appLifetime.StopApplication();
                }
            }, _cancellationTokenSource.Token);
        });

        _appLifetime.ApplicationStopping.Register(() =>
        {
            _logger.LogDebug("Application is stopping");
            _cancellationTokenSource?.Cancel();
        });

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Wait for the application logic to fully complete any cleanup tasks.
        // Note that this relies on the cancellation token to be properly used in the application.
        if (_applicationTask != null)
        {
            await _applicationTask;
        }

        _logger.LogDebug($"Exiting with return code: {_exitCode}");

        // Exit code may be null if the user cancelled via Ctrl+C/SIGTERM
        Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
    }

    async public Task CallMenu()
    {
        Console.WriteLine($"Please select an option: ");
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine($"Option 1: Read and push files to AWS S3");
        Console.WriteLine($"Option 2: Chat");
        Console.WriteLine($"Option 3: Test");
        Console.WriteLine($"Option 4: Exit");
        Console.WriteLine(Environment.NewLine);
        var optionSelected = Console.ReadLine();

        switch (optionSelected)
        {
            case "1":
                Console.WriteLine("Option 1 selected");
                Console.WriteLine(Environment.NewLine);
                await _fileUploader.StartUploading();
                break;
            case "2":
                Console.WriteLine("Option 2 selected");
                Console.WriteLine(Environment.NewLine);
                Console.ReadKey();
                break;
            case "3":
                Console.WriteLine("Option 3 selected");
                Console.WriteLine(Environment.NewLine);
                Console.ReadKey();
                break;
            case "4":
                Console.WriteLine("Option 4 selected");
                Console.WriteLine(Environment.NewLine);
                Console.ReadKey();
                break;
            default:
                Console.WriteLine("Option not available!");
                Console.WriteLine(Environment.NewLine);
                CallMenu();
                break;
        }
    }
}