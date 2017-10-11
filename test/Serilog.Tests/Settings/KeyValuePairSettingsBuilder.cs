using System;
using System.Collections.Generic;
using System.Linq;

namespace Serilog.Tests.Settings
{
    public class KeyValuePairSettingsBuilder
    {
        List<KeyValuePair<string, string>> _keyValuePairs = new List<KeyValuePair<string, string>>();

        void AddKeyValuePair(KeyValuePair<string, string> pair)
        {
            if (pair.Key == null) throw new ArgumentNullException($"{nameof(pair)}.{nameof(pair.Key)}");
            _keyValuePairs.Add(pair);
        }

        public KeyValuePairSettingsBuilder AddKeyValuePair(string key, string value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _keyValuePairs.Add(new KeyValuePair<string, string>(key, value));

            return this;
        }

        public KeyValuePairSettingsBuilder AddKeyValuePairs(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            if (keyValuePairs == null) throw new ArgumentNullException(nameof(keyValuePairs));

            foreach (var kvp in keyValuePairs)
            {
                AddKeyValuePair(kvp);
            }
            return this;
        }

        public IReadOnlyList<KeyValuePair<string, string>> Build()
        {
            var uniques = new Dictionary<string, string>();
            foreach (var keyValuePair in _keyValuePairs)
            {
                uniques[keyValuePair.Key] = keyValuePair.Value;
            }

            return uniques.ToList().AsReadOnly();
        }
    }
}
