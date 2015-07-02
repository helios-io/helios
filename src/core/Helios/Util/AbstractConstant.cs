using System;

namespace Helios.Util
{
    /// <summary>
    /// A singleton instance of a class which is type-safe and can be compared via the == operator.
    /// 
    /// Created and managed by <see cref="ConstantPool{T}"/>. 
    /// </summary>
    public abstract class AbstractConstant
    {
        protected AbstractConstant(int id, string name, Type type)
        {
            Type = type;
            Id = id;
            Name = name;
        }

        public string Name { get; private set; }

        public int Id { get; private set; }

        public Type Type { get; private set; }

        #region Equality

        protected bool Equals(AbstractConstant other)
        {
            return string.Equals(Name, other.Name) && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as AbstractConstant;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ Id;
            }
        }

        public static bool operator ==(AbstractConstant left, AbstractConstant right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AbstractConstant left, AbstractConstant right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
