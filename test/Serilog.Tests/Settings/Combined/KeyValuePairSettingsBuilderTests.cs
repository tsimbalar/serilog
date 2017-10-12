using System.Collections.Generic;
using System.Linq;
using Serilog.Events;
using Serilog.Settings.KeyValuePairs.Combined;
using Serilog.Tests.Support;
using Xunit;

namespace Serilog.Tests.Settings.Combined
{
    public class KeyValuePairSettingsBuilderTests
    {
        [Fact]
        public void BuilderCombinesDifferentSources()
        {
            var builder = new KeyValuePairSettingsBuilder()
                .AddExpression(lc => lc
                        .MinimumLevel.Fatal()
                        .MinimumLevel.Override("System", LogEventLevel.Error)
                        .Enrich.WithProperty("EnrichedProperty", "EnrichValue", false)

                        )
                .AddKeyValuePairs(new Dictionary<string, string>(){
                    { "minimum-level", "Error"},
                    { "enrich:with-property:Enriched2", "Enrichement2"}
                })
                .AddKeyValuePair("minimum-level", "Information")
                .AddKeyValuePair("enrich:with-property:Enriched3", "Enrichement3")
                ;
            var keyValuePairs = ((KeyValuePairSettingsBuilder)builder).Build();

            var expected = new Dictionary<string, string>()
            {
                { "minimum-level", "Information"},
                { "minimum-level:override:System", "Error"},
                { "enrich:with-property:EnrichedProperty", "EnrichValue"},
                { "enrich:with-property:Enriched2", "Enrichement2"},
                { "enrich:with-property:Enriched3", "Enrichement3"},
            }.ToList();

            Assert.Equal(expected, keyValuePairs.ToList(), new KeyValuePairComparer<string, string>());
        }
    }
}
