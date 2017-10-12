using System.Collections.Generic;

namespace Serilog.Settings.KeyValuePairs.Combined
{
    /// <summary>
    /// Allows the combination of several sources of settings to configure a Logger instance
    /// </summary>
    public interface ICombinedSettingsOptions
    {
        /// <summary>
        /// Adds a key value pair after the existing ones
        /// </summary>
        /// <param name="key">the key for the setting</param>
        /// <param name="value">the value for the setting</param>
        /// <returns>the same <see cref="ICombinedSettingsOptions"/> to allow method-chaining</returns>
        ICombinedSettingsOptions AddKeyValuePair(string key, string value);

        /// <summary>
        /// Adds several key-value pairs after the existing ones
        /// </summary>
        /// <param name="keyValuePairs">a collection of key-value pairs to add</param>
        /// <returns>the same <see cref="ICombinedSettingsOptions"/> to allow method-chaining</returns>
        ICombinedSettingsOptions AddKeyValuePairs(IEnumerable<KeyValuePair<string, string>> keyValuePairs);
    }
}
