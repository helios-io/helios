using System.Collections.Generic;
using System.Linq;
using Helios.Net;
using Helios.Util.Collections;

namespace Helios.Channels
{
    /// <summary>
    /// Configuration class for <see cref="IConnection"/> objects
    /// </summary>
    public class DefaultConnectionConfig : IConnectionConfig
    {
        private readonly Dictionary<string, object> _options = new Dictionary<string, object>();

        public IConnectionConfig SetOption(string optionKey, object optionValue)
        {
            _options[optionKey] = optionValue;
            return this;
        }

        public bool HasOption(string optionKey)
        {
            return _options.ContainsKey(optionKey);
        }

        public bool HasOption<T>(string optionKey)
        {
            return _options.ContainsKey(optionKey) && _options[optionKey] is T;
        }

        public object GetOption(string optionKey)
        {
            return _options.GetOrDefault(optionKey);
        }

        public T GetOption<T>(string optionKey)
        {
            return _options.GetOrDefault<string,T>(optionKey);
        }

        public IList<KeyValuePair<string, object>> Options => _options.ToList();
    }
}