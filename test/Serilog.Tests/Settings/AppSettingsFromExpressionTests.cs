using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;
using TestDummies;
using Xunit;
using System.Reflection;

namespace Serilog.Tests.Settings
{
    public class AppSettingsFromExpressionTests
    {
        [Fact]
        public void SupportMinimumLevel()
        {
            var actual = AppSettingsConverter.From(lc =>
                lc
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Information()
                    .MinimumLevel.Warning()
                    .MinimumLevel.Error()
                    .MinimumLevel.Fatal()
            ).ToList();

            var expected = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("minimum-level", "Verbose"),
                new KeyValuePair<string, string>("minimum-level", "Debug"),
                new KeyValuePair<string, string>("minimum-level", "Information"),
                new KeyValuePair<string, string>("minimum-level", "Warning"),
                new KeyValuePair<string, string>("minimum-level", "Error"),
                new KeyValuePair<string, string>("minimum-level", "Fatal")
            };

            Assert.Equal(expected.ToList(), actual, new KeyValuePairComparer<string, string>());
        }

        [Fact]
        public void SupportEnrichWithProperty()
        {
            var actual = AppSettingsConverter.From(lc =>
                lc
                    .Enrich.WithProperty("Prop1", "Prop1Value", false)
                    .Enrich.WithProperty("Prop2", 42, false)
                    .Enrich.WithProperty("Prop3", new Uri("https://www.perdu.com/bar"), false)
                    .Enrich.WithProperty("Prop4", true, false)
            ).ToList();

            var expected = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("enrich:with-property:Prop1", "Prop1Value"),
                new KeyValuePair<string, string>("enrich:with-property:Prop2", "42"),
                new KeyValuePair<string, string>("enrich:with-property:Prop3", "https://www.perdu.com/bar"),
                new KeyValuePair<string, string>("enrich:with-property:Prop4", "True"),
            };

            Assert.Equal(expected.ToList(), actual, new KeyValuePairComparer<string, string>());
        }

        [Fact]
        public void SupportSink()
        {
            var actual = AppSettingsConverter.From(lc =>
                    lc
                    .WriteTo.DummyRollingFile(
                                @"C:\toto.log",
                                LogEventLevel.Warning,
                                null,
                                null)
            ).ToList();

            var expected = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("using:TestDummies", typeof(DummyLoggerConfigurationExtensions).GetTypeInfo().Assembly.FullName),
                new KeyValuePair<string, string>("write-to:DummyRollingFile.pathFormat", @"C:\toto.log"),
                new KeyValuePair<string, string>("write-to:DummyRollingFile.restrictedToMinimumLevel", "Warning")
            };

            Assert.Equal(expected.ToList(), actual, new KeyValuePairComparer<string, string>());
        }

        public class KeyValuePairComparer<TK, TValue> : IEqualityComparer<KeyValuePair<TK, TValue>>
        {
            public bool Equals(KeyValuePair<TK, TValue> x, KeyValuePair<TK, TValue> y)
            {
                return x.Key.Equals(y.Key) && x.Value.Equals(y.Value);
            }

            public int GetHashCode(KeyValuePair<TK, TValue> obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
