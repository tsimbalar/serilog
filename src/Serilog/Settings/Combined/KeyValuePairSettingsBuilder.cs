// Copyright 2013-2017 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Serilog.Settings.KeyValuePairs.Combined
{
    /// <summary>
    /// Allows the combination of several sources of settings to configure a Logger instance
    /// </summary>
    class KeyValuePairSettingsBuilder : ICombinedSettingsOptions
    {
        readonly List<KeyValuePair<string, string>> _keyValuePairs = new List<KeyValuePair<string, string>>();

        void AddKeyValuePair(KeyValuePair<string, string> pair)
        {
            if (pair.Key == null) throw new ArgumentNullException($"{nameof(pair)}.{nameof(pair.Key)}");
            _keyValuePairs.Add(pair);
        }

        public ICombinedSettingsOptions AddKeyValuePair(string key, string value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _keyValuePairs.Add(new KeyValuePair<string, string>(key, value));

            return this;
        }

        public ICombinedSettingsOptions AddKeyValuePairs(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            if (keyValuePairs == null) throw new ArgumentNullException(nameof(keyValuePairs));

            foreach (var kvp in keyValuePairs)
            {
                AddKeyValuePair(kvp);
            }
            return this;
        }

        /// <summary>
        /// Extracts the result of combining all the sources of settings
        /// </summary>
        /// <returns>A list of unique key value pairs</returns>
        internal IEnumerable<KeyValuePair<string, string>> Build()
        {
            var uniques = new Dictionary<string, string>();
            foreach (var keyValuePair in _keyValuePairs)
            {
                uniques[keyValuePair.Key] = keyValuePair.Value;
            }

            return uniques.ToList();
        }
    }
}
