using System.Net;
using Microsoft.Extensions.Logging;
using NetBank.Application;
using NetBank.Application.Commands;
using NetBank.Infrastructure.Buffering;
using NetBank.Infrastructure.Configuration;
using NetBank.Infrastructure.Parsing;
using NetBank.Infrastructure.Storage;
using NetBank.Infrastructure.Storage.Strategies;
using NetBank.Presentation;

namespace NetBank;

class Program
{
    private class BufferFactory: IFactory<AccountServiceCaptureBuffer>
    {
        public AccountServiceCaptureBuffer Create()
        {
            return new AccountServiceCaptureBuffer();
        }
    }
    
    static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder => {
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
        });
    
        Configuration? configuration = null;
        try
        {
            configuration = new ConfigLoader<Configuration>().Load(args);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger<Program>().LogCritical(ex, "Failed to load configuration.");
            return;
        }

        var bufferFactory = new BufferFactory();
        var inmemStorage = new InMemoryStorageStrategy();
        var processor = new StorageBufferProcessor(inmemStorage);
        var buffer = new CoordinatedDoubleBuffer<AccountServiceCaptureBuffer>(bufferFactory, processor);
        
        using var observer = new TimedBufferObserver<AccountServiceCaptureBuffer>(
            () => {
                try { return buffer.TrySwap(); }
                catch (Exception e) { 
                    loggerFactory.CreateLogger("BufferObserver").LogError(e, "Buffer swap failed.");
                    return Task.FromResult(false); 
                }
            }, 
            buffer, 
            configuration.BufferSwapDelay
        );

        var serviceProvider = new LambdaProvider<IAccountService>(() => buffer.Front);
        var commandFactory = new CommandFactory(serviceProvider, configuration);
        var commandParser = new TemplateCommandParser();
        var orchestrator = new SimpleOrchestrator(commandParser, commandFactory);
        
        var server = new TcpOrchestratorServer(
            orchestrator, 
            configuration.ServerPort,
            IPAddress.Parse(configuration.ServerIp),
            configuration.InactivityTimeout,
            loggerFactory.CreateLogger<TcpOrchestratorServer>()
        );

        await server.StartAsync(CancellationToken.None);
    }
}
