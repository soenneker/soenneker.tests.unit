using System;
using AutoBogus;
using Bogus;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.XUnit.Injectable;
using Serilog.Sinks.XUnit.Injectable.Extensions;
using Soenneker.Tests.Logging;
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
    ///     Syntactic sugar for lazy Faker
    /// </summary>
    public Faker Faker => _faker.Value;

    private readonly Lazy<IAutoFaker> _autoFaker;

    /// <summary>
    /// Used for generating fake objects with real values (without mocking)
    /// </summary>
    public IAutoFaker AutoFaker => _autoFaker.Value;

    private readonly InjectableTestOutputSink _injectableTestOutputSink = new();

    /// <param name="testOutputHelper"></param>
    /// <param name="createLogger">Typically this is true unless this is being used with a fixture that will resolve a logger via DI</param>
    protected UnitTest(ITestOutputHelper testOutputHelper, bool createLogger = true)
    {
        if (createLogger)
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

        _faker = new Lazy<Faker>(() => new Faker(), true);

        _autoFaker = new Lazy<IAutoFaker>(() => AutoBogus.AutoFaker.Create(), true);
    }
}