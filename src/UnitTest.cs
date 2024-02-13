using System;
using System.Threading;
using Bogus;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.XUnit.Injectable;
using Serilog.Sinks.XUnit.Injectable.Extensions;
using Soenneker.Tests.Logging;
using Soenneker.Utils.AutoBogus;
using Soenneker.Utils.Logger;
using Xunit.Abstractions;
using ILogger = Serilog.ILogger;

namespace Soenneker.Tests.Unit;

/// <summary>
/// A base class providing Faker, AutoFaker, and logging. <para/>
/// Does NOT have the ability to resolve services (there's no ServiceProvider involved when instantiating this)
/// </summary>
public abstract class UnitTest : LoggingTest
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

    private readonly InjectableTestOutputSink _injectableTestOutputSink = new();

    ///<summary>Initializes faker and AutoFaker, and optionally creates a logger (which if you're using a fixture, you should not pass testOutputHelper)</summary>
    /// <param name="testOutputHelper">If you do not pass this, you will not get logger capabilities</param>
    /// <param name="autoFaker"></param>
    protected UnitTest(ITestOutputHelper? testOutputHelper = null, AutoFaker? autoFaker = null)
    {
        if (testOutputHelper != null)
        {
            LazyLogger = new Lazy<ILogger<LoggingTest>>(() =>
            {
                ILogger serilogLogger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.InjectableTestOutput(_injectableTestOutputSink) // This NEEDS to stay synchronous
                    .Enrich.FromLogContext()
                    .CreateLogger();

                _injectableTestOutputSink.Inject(testOutputHelper);

                Log.Logger = serilogLogger;

                return LoggerUtil.BuildLogger<UnitTest>();
            }, true);
        }

        _autoFaker = new Lazy<AutoFaker>(() =>
        {
            if (autoFaker != null)
                return autoFaker;

            return new AutoFaker();
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        _faker = new Lazy<Faker>(() => _autoFaker.Value.Faker, LazyThreadSafetyMode.ExecutionAndPublication);
    }
}