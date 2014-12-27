using System.Collections.Generic;
using System.Linq;
using Helios.Util.Collections;

namespace Helios.Net
{
    /// <summary>
    /// Configuration class for <see cref="IConnection"/> objects
    /// </summary>
    public class DefaultConnectionConfig : IHeliosConfig
    {
        private readonly Dictionary<string, object> _options = new Dictionary<string, object>();

        public IHeliosConfig SetOption(string optionKey, object optionValue)
        {
            _options.AddOrSet(optionKey, optionValue);
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

        public IList<KeyValuePair<string, object>> Options { get { return _options.ToList(); } }
    }
}