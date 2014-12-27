using System.Collections.Generic;
using Helios.Channel;
using Helios.Net;

namespace Helios
{
    /// <summary>
    /// Interface used to help configure different types of Helios objects, including <see cref="IConnection"/> 
    /// and <see cref="IChannel"/> instances
    /// </summary>
    public interface IHeliosConfig
    {
        /// <summary>
        /// Set an option for this configuration.
        /// 
        /// Overwrites any previously stored value.
        /// </summary>
        /// <param name="optionKey">The name of the option.</param>
        /// <param name="optionValue">The value of the option.</param>
        /// <returns>the <see cref="IHeliosConfig"/> with this option set</returns>
        IHeliosConfig SetOption(string optionKey, object optionValue);

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
}
