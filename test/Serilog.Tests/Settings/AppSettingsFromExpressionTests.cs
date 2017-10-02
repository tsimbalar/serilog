using System.Collections.Generic;
using System.Linq;
using Xunit;

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
