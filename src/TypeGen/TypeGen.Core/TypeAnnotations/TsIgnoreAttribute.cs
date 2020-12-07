using System;

namespace TypeGen.Core.TypeAnnotations
{
    /// <summary>
    /// Identifies a property that should be ignored when generating a TypeScript file
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class TsIgnoreAttribute : Attribute
    {

        /// <summary>
        /// Flag indicating if properties implementing this type in classes implementing this type,
        /// should be ingored. Default is true
        /// </summary>
        public bool IgnoreImplicitlyImplementedProperties { get; set; } = true;

        /// <summary>
        /// </summary>
        public TsIgnoreAttribute()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="ignoreImplicitlyImplementedProperties">Can be set to false to still include implicitly implemented properties</param>
        public TsIgnoreAttribute(bool ignoreImplicitlyImplementedProperties = true)
        {
            IgnoreImplicitlyImplementedProperties = ignoreImplicitlyImplementedProperties;   
        }
    }
}
