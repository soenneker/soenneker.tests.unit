using Bogus;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.XUnit.Injectable;
using Serilog.Sinks.XUnit.Injectable.Extensions;
using Soenneker.Tests.Logging;
using Soenneker.Utils.AutoBogus;
using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Core;
using Soenneker.Extensions.ValueTask;
using Xunit;

namespace Soenneker.Tests.Unit;

/// <summary>
/// A base class providing Faker, AutoFaker, and logging. <para/>
/// Does NOT have the ability to resolve services (there's no ServiceProvider involved when instantiating this)
/// </summary>
public abstract class UnitTest : LoggingTest, IAsyncLifetime
{
    private readonly Lazy<Faker> _faker;

    /// <summary>
    /// Syntactic sugar for lazy Faker
    /// </summary>
    public Faker Faker => _faker.Value;

    private readonly Lazy<AutoFaker> _autoFaker;

    /// <summary>
    /// Used for generating fake objects with real values (without mocking)
    /// </summary>
    public AutoFaker AutoFaker => _autoFaker.Value;

    private readonly ILoggerFactory? _standaloneFactory;
    private readonly InjectableTestOutputSink? _sink;

    ///<summary>Initializes faker and AutoFaker, and optionally creates a logger (which if you're using a fixture, you should not pass testOutputHelper)</summary>
    /// <param name="testOutputHelper">If you do not pass this, you will not get logger capabilities</param>
    /// <param name="autoFaker"></param>
    protected UnitTest(ITestOutputHelper? testOutputHelper = null, AutoFaker? autoFaker = null)
    {
        if (testOutputHelper != null)
        {
            // Build a PRIVATE Serilog logger (do NOT assign to Log.Logger)
            _sink = new InjectableTestOutputSink();
            _sink.Inject(testOutputHelper);

            Logger serilog = new LoggerConfiguration().MinimumLevel.Verbose()
                .WriteTo.InjectableTestOutput(_sink)
                .Enrich.FromLogContext()
                .CreateLogger();

            // Provider owns the Serilog logger and will dispose it
            var standaloneProvider = new SerilogLoggerProvider(serilog, dispose: true);

            _standaloneFactory = LoggerFactory.Create(b => b.AddProvider(standaloneProvider));

            // Provide a Microsoft ILogger<T> source for this test
            LazyLogger = new Lazy<ILogger<LoggingTest>>(() => _standaloneFactory.CreateLogger<LoggingTest>(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        _autoFaker = new Lazy<AutoFaker>(() => autoFaker ?? new AutoFaker(), LazyThreadSafetyMode.ExecutionAndPublication);
        _faker = new Lazy<Faker>(() => _autoFaker.Value.Faker, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public virtual async ValueTask DisposeAsync()
    {
        _standaloneFactory?.Dispose();

        if (_sink != null)
            await _sink.DisposeAsync().NoSync();
    }

    public virtual ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }
}