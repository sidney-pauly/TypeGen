using System;
using System.Collections.Generic;
using System.Text;

namespace TypeGen.Core.Generator
{
    /// <summary>
    /// Exception thrown if the <see cref="GeneratorOptions.StrictDependencies"/> option is set and
    /// a dependency required by a type was not found
    /// </summary>
    public class MissingDependencyException : Exception
    {

        /// <summary>
        /// The type requireing the dependency
        /// </summary>
        public Type RequireringType { get; private set; }

        /// <summary>
        /// The required type
        /// </summary>
        public Type RequiredType { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="reqireingType"></param>
        /// <param name="requiredType"></param>
        public MissingDependencyException(Type reqireingType, Type requiredType)
            : base("Dependency " + requiredType.FullName + " not found. Needed to generate "
                  + reqireingType.FullName + ". Make sure you either ignore the type, include it in your "
                  + nameof(SpecGeneration.GenerationSpec) + " or provide a custom dependency mapping for it.")
        {
            RequireringType = reqireingType;
            RequiredType = requiredType;
        }
    }
}
