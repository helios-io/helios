using System.Collections.Generic;
using System.Linq;
using Helios.Net;
using Helios.Util.Collections;

namespace Helios.Channels
{
    /// <summary>
    /// Interface used to help configure <see cref="IConnection"/> instances
    /// </summary>
    public interface IConnectionConfig
    {
        /// <summary>
        /// Set an option for this configuration.
        /// 
        /// Overwrites any previously stored value.
        /// </summary>
        /// <param name="optionKey">The name of the option.</param>
        /// <param name="optionValue">The value of the option.</param>
        /// <returns>the <see cref="IConnectionConfig"/> with this option set</returns>
        IConnectionConfig SetOption(string optionKey, object optionValue);

        /// <summary>
        /// Checks to see if we have a set option of this value in the dictionary
        /// </summary>
        /// <param name="optionKey">The name of the value to check</param>
        /// <returns>true if found, false otherwise</returns>
        bool HasOption(string optionKey);

        /// <summary>
        /// Checks to see if we have a set option of ths key in the dictionary AND
        /// that the value of this option is of type <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="optionKey">The name of the value to check</param>
        /// <returns>true if found and of type T, false otherwise</returns>
        bool HasOption<T>(string optionKey);

        /// <summary>
        /// Gets the option from the configuration of the specified name.
        /// </summary>
        /// <param name="optionKey">The name of the value to get</param>
        /// <returns>the object if found, null otherwise</returns>
        object GetOption(string optionKey);

        /// <summary>
        /// Gets the option from the configuration of the specified name AND
        /// automatically casts it into <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="optionKey">The name of the value to get</param>
        /// <returns>the object as instance of type T if found, default(T) otherwise</returns>
        T GetOption<T>(string optionKey);

        IList<KeyValuePair<string, object>> Options { get; }
    }

    /// <summary>
    /// Configuration class for <see cref="IConnection"/> objects
    /// </summary>
    public class DefaultConnectionConfig : IConnectionConfig
    {
        private readonly Dictionary<string, object> _options = new Dictionary<string, object>();

        public IConnectionConfig SetOption(string optionKey, object optionValue)
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
