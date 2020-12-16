using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TypeGen.Core.Extensions
{

    /// <summary>
    /// Class to emulate members that are not actually present in a type.
    /// </summary>
    public class CustomMemberInfo : MemberInfo
    {
        private Type _DeclaringType { get; set; }

        /// <inheritdoc/>
        public override Type DeclaringType => _DeclaringType;

        private MemberTypes _MemberTypes { get; set; }

        /// <inheritdoc/>
        public override MemberTypes MemberType => _MemberTypes;

        private string _Name { get; set; }

        /// <inheritdoc/>
        public override string Name => _Name;

        private Type _ReflectedType { get; set; }

        /// <inheritdoc/>
        public override Type ReflectedType => _ReflectedType;

        /// <inheritdoc/>
        public override object[] GetCustomAttributes(bool inherit)
        {
            return new Attribute[] { };
        }

        /// <inheritdoc/>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new Attribute[] { };
        }

        /// <inheritdoc/>
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="declaringType"></param>
        /// <param name="reflectedType"></param>
        /// <param name="memberTypes"></param>
        /// <param name="name"></param>
        public CustomMemberInfo(Type declaringType, Type reflectedType, MemberTypes memberTypes, string name)
        {
            _DeclaringType = declaringType;
            _ReflectedType = reflectedType;
            _MemberTypes = memberTypes;
            _Name = name;
        }
    }
}
