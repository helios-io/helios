using System;
using System.Collections.Concurrent;

namespace Helios.Util
{
    /// <summary>
    /// A pool of constants.
    /// </summary>
    /// <remarks>
    /// This class in Netty was implemented in a way that made my brain hurt.
    /// 
    /// So I found a sane explanation from Eric Lippert as to what the JVM implementation
    /// was trying to do with its generic constraints: http://blogs.msdn.com/b/ericlippert/archive/2011/02/03/curiouser-and-curiouser.aspx
    /// 
    /// I've decided to forgo all of that and keep it simple, at the risk of some loss of type-safety.
    /// </remarks>
    /// <typeparam name="T">The type ofthe constant.</typeparam>
    public abstract class ConstantPool<T> where T:AbstractConstant
    {
        private readonly ConcurrentDictionary<string, T> _constants = new ConcurrentDictionary<string,T>();
        private readonly AtomicCounter _nextId = new AtomicCounter(1);

        /// <summary>
        /// As shortcut of <code>ValueOf(firstNameComponent.Name + "#" + secondNameComponent);</code>
        /// </summary>
        /// <param name="firstNameComponent">The type name to use in the name of the <see cref="AbstractConstant"/></param>
        /// <param name="secondNameComponent">The human-readable name of the constant</param>
        /// <returns>The <see cref="AbstractConstant"/> associated with this name.</returns>
        public T ValueOf(Type firstNameComponent, string secondNameComponent)
        {
            if(firstNameComponent == null)
                throw new ArgumentNullException("firstNameComponent");
            if(secondNameComponent == null)
                throw new ArgumentNullException("secondNameComponent");

            return ValueOf(firstNameComponent.Name + "#" + secondNameComponent);
        }

        /// <summary>
        /// Returns the <see cref="AbstractConstant"/> which is assigned to the specified <see cref="name"/>.
        /// If there's no such <see cref="AbstractConstant"/>, a new one will be created and returned.
        /// 
        /// Once created, the subsequent calls with the same <see cref="name"/> will always return the previously created
        /// one (singleton.)
        /// </summary>
        /// <param name="name">The name of the constant.</param>
        /// <returns>The <see cref="AbstractConstant"/> associated with this name.</returns>
        public T ValueOf(string name)
        {
            T c;
            if (!_constants.TryGetValue(name, out c))
            {
                c = NewInstance(name);
            }

            return c;
        }

        /// <summary>
        /// Returns <c>true</c> if a value exists for the given <see cref="name"/>.
        /// </summary>
        /// <param name="name">The name of the constant we want to check</param>
        /// <returns><c>true</c> if the value exists, <c>false</c> otherwise.</returns>
        public bool Exists(string name)
        {
            name.NotNullOrEmtpy();
            return _constants.ContainsKey(name);
        }

        internal T NewInstance(string name)
        {
            if (Exists(name))
            {
                throw new ArgumentException(name + " already exists!", "name");
            }
            var c = NewConstant(_nextId.GetAndIncrement(), name);
            _constants.AddOrUpdate(name, c, (s, arg2) => c);
            return c;
        }

        protected abstract T NewConstant(int id, string name);
    }
}
