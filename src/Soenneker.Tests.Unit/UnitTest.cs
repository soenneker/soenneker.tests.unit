using Bogus;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Soenneker.Atomics.ValueBools;
using Soenneker.Serilog.Sinks.TUnit;
using Soenneker.Tests.Logging;
using Soenneker.Utils.AutoBogus;
using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Core;
using Soenneker.Extensions.ValueTask;
using TUnit.Core.Interfaces;

namespace Soenneker.Tests.Unit;

/// <summary>
/// A base class providing Faker, AutoFaker, and logging. <para/>
/// Does NOT have the ability to resolve services (there's no ServiceProvider involved when instantiating this).
/// </summary>
public abstract class UnitTest : LoggingTest, IAsyncInitializer, IAsyncDisposable
{
    private readonly Lazy<Faker> _faker;
    private readonly Lazy<AutoFaker> _autoFaker;

    private readonly ILoggerFactory? _standaloneFactory;
    private readonly SerilogLoggerProvider? _provider;
    private readonly TUnitTestContextSink? _sink;
    private readonly Logger? _serilogLogger;

    private ValueAtomicBool _disposed;

    /// <summary>
    /// Syntactic sugar for lazy Faker.
    /// </summary>
    public Faker Faker => _faker.Value;

    /// <summary>
    /// Used for generating fake objects with real values (without mocking).
    /// </summary>
    public AutoFaker AutoFaker => _autoFaker.Value;

    /// <summary>
    /// Initializes faker and AutoFaker, and optionally creates a private Serilog-backed logger for the test.
    /// </summary>
    /// <param name="autoFaker"></param>
    /// <param name="enableLogging">Whether to enable logging to the current TUnit test context.</param>
    protected UnitTest(AutoFaker? autoFaker = null, bool enableLogging = true)
    {
        if (enableLogging)
        {
            _sink = new TUnitTestContextSink();

            Logger serilog = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(_sink).Enrich.FromLogContext().CreateLogger();

            _serilogLogger = serilog;
            _provider = new SerilogLoggerProvider(serilog, dispose: false);
            _standaloneFactory = LoggerFactory.Create(b => b.AddProvider(_provider));

            LazyLogger = new Lazy<ILogger<LoggingTest>>(() => _standaloneFactory.CreateLogger<LoggingTest>(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        _autoFaker = new Lazy<AutoFaker>(() => autoFaker ?? new AutoFaker(), LazyThreadSafetyMode.ExecutionAndPublication);

        _faker = new Lazy<Faker>(() => _autoFaker.Value.Faker, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (!_disposed.TrySetTrue())
            return;

        _standaloneFactory?.Dispose();

        if (_provider is not null)
            await _provider.DisposeAsync().NoSync();

        if (_serilogLogger is not null)
            await _serilogLogger.DisposeAsync().NoSync();

        if (_sink is not null)
            await _sink.DisposeAsync().NoSync();
    }
}