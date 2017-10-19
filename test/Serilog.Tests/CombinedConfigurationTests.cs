using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Tests.Support;
using Xunit;

namespace Serilog.Tests
{
    public class CombinedConfigurationTests
    {
        [Fact]
        public void CombinedCombinesSettings()
        {
            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.Combined(b => b
                    .AddKeyValuePairs(new Dictionary<string, string>
                    {
                        {"minimum-level", "Information" }
                    })
                    .AddKeyValuePairs(new Dictionary<string, string>
                    {
                        {"minimum-level", "Warning" }
                    })
                    )
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Write(Some.InformationEvent());
            Assert.Null(evt);
            log.Write(Some.WarningEvent());
            Assert.NotNull(evt);
        }

        [Fact]
        public void ConfigBuilderConsumesEnumerablesAsLateAsPossible()
        {
            var consumeCount = 0;

            IEnumerable<KeyValuePair<string, string>> Enumerable1()
            {
                consumeCount++;
                yield break;
            }

            IEnumerable<KeyValuePair<string, string>> Enumerable2()
            {
                consumeCount++;
                yield break;
            }

            var builder = new ConfigBuilder();
            builder.AddSource(Enumerable1());
            builder.AddSource(Enumerable2());
            Assert.Equal(0, consumeCount);

            var combined = builder.BuildCombinedEnumerable();
            Assert.Equal(0, consumeCount);

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            combined.ToList();

            Assert.Equal(2, consumeCount);
        }
    }

    public static class LoggerSettingsConfigurationExtensions
    {
        public static LoggerConfiguration Combined(this LoggerSettingsConfiguration lsc, Func<IConfigBuilder, IConfigBuilder> build)
        {
            var configBuilder = new ConfigBuilder();
            configBuilder = (ConfigBuilder)build(configBuilder);
            var enumerable = configBuilder.BuildCombinedEnumerable();

            return lsc.KeyValuePairs(enumerable);
        }
    }

    public interface IConfigBuilder
    {
        IConfigBuilder AddSource(IEnumerable<KeyValuePair<string, string>> source);
    }

    public static class ConfigBuilderExtensions
    {
        public static IConfigBuilder AddKeyValuePairs(this IConfigBuilder builder, Dictionary<string, string> keyValuePairs)
        {
            return builder.AddSource(new ReadOnlyDictionary<string, string>(keyValuePairs));
        }
    }

    public class ConfigBuilder : IConfigBuilder
    {
        List<IEnumerable<KeyValuePair<string, string>>> _sources;

        public ConfigBuilder()
        {
            _sources = new List<IEnumerable<KeyValuePair<string, string>>>();
        }

        public IEnumerable<KeyValuePair<string, string>> BuildCombinedEnumerable()
        {
            IEnumerable<KeyValuePair<string, string>> Combined()
            {
                var result = new Dictionary<string, string>();
                foreach (var source in _sources)
                {
                    foreach (var kvp in source)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
                return result;
            }

            foreach (var kvp in Combined())
            {
                yield return kvp;
            }
        }

        public IConfigBuilder AddSource(IEnumerable<KeyValuePair<string, string>> source)
        {
            _sources.Add(source);
            return this;
        }
    }
}
