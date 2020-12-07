using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using  System.Text;
using TypeGen.Core.Utils;

namespace TypeGen.Core.Converters
{
    /// <summary>
    /// Hashes the <see cref="Type.FullName"/> to allow for explizitly
    /// implemented properties to show up in typescript. Properties wich have the
    /// same name, but come from two different itnerfaces are therefore made
    /// possible.
    /// <br/>
    /// The resulting affix is base64 encoded (<c>[A-Za-z0-9_$]</c> instead of <c>[A-Za-z0-9+/]</c> to
    /// comply with TS property naming rules) and gets postfix to the property.
    /// <br/>
    /// As typescript is actually weakly typed (because it is a superset of JS)
    /// mutliinheritance and interface "owned" properties are not nativly supported. The
    /// type is hashed to guarantee no collisions between two properties of the same type.
    /// Collision probabbillity is roughly 1e18 for two types to have the same hash.
    /// </summary>
    public class ExplicitImplementationPostfixTypeNameConverter : IMemberNameConverter
    {
        /// <inheritdoc/>
        public bool IsPreffix => false;

        /// <summary>
        /// Base namespace to generate the Hashes with minimal collision.
        /// </summary>
        public static readonly Guid PROPERTY_TYPE_UUID_HASH_NAMESPACE = new Guid("d8dd89f4-0dbe-4e32-84f4-2cb3d34601b1");

        /// <summary>    
        /// See class description (<see cref="ExplicitImplementationPostfixTypeNameConverter"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public string Convert(string name, MemberInfo memberInfo)
        {
            // Return if not explicit property
            if (!memberInfo.Name.Contains('.'))
                return name;

            if (name.Contains('.'))
            {
                var split = name.Split('.');
                name = split[split.Length - 1];
            }

            if (name.Contains('`'))
                name = name.Split('`')[0];

            var typeHash = System.Convert.ToBase64String(PROPERTY_TYPE_UUID_HASH_NAMESPACE.Create(memberInfo.DeclaringType.FullName).ToByteArray())
               .Substring(0, 10)
               .Replace('+', '_')
               .Replace('/', '$');

            return name + "_" + typeHash;
        }
    }
}
