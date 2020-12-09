using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TypeGen.Core.Converters;

namespace TypeGen.Core.Generator.Services
{

    /// <summary>
    /// Adds the number of generic parameters to the typename to
    /// make up for the fact that c# allows overloading of the
    /// generic types and ts not.
    /// </summary>
    internal class GenericTypeNameCollisionConverter : ITypeNameConverter
    {


        /// <summary>
        /// Finds type names ending with the pattern that would normaly
        /// be added by this Converter.
        /// </summary>
        static Regex TypesToEscape = new Regex("_[0-9]+\\z");

        /// <inheritdoc/>
        public string Convert(string name, Type type)
        {


            string newName = name;
            var typeArgs = type.GetGenericArguments();

            if (typeArgs.Length < 1)
                if (TypesToEscape.Match(name).Success)
                    return newName + "_0";
                else
                    return newName;

            string namespacedName = type.Namespace + '.' + name;


            // Try to find a generic with the same name and fewer type args
            int i = typeArgs.Length;
            do
                i--;
            while (i > 0 && type.Assembly.GetType(namespacedName + '`' + i, false) == null);

            // If no generic with the same name and fewer genric arguments was found and
            // no type with the same name and no generic argumetns
            if (i < 1 && type.Assembly.GetType(namespacedName, false) == null)
                return newName;

            return newName + "_" + typeArgs.Length;
        }
    }
}

