using System;
using System.Collections.Generic;
using System.Text;

namespace TypeGen.Core.TypeAnnotations
{
    /// <summary>
    /// Marked TypeScript classes/interfaces will not have base type declaration.
    /// Also, base classes/interfaces will not be generated if they're not marked with an ExportTs... attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class TsIgnoreBaseAttribute : Attribute
    {

        /// <summary>
        /// Constructor ignoring all base types
        /// </summary>
        public TsIgnoreBaseAttribute()
        {

        }

        /// <summary>
        /// Constructor to ignore some base types
        /// </summary>
        /// <param name="toIgnore"></param>
        public TsIgnoreBaseAttribute(params Type[] toIgnore)
        {
            Ignore = toIgnore;
        }


        /// <summary>
        /// Specify the types that should be ignored. If empty
        /// all base types are ignored
        /// </summary>
        public Type[] Ignore { get; set; } = new Type[] { };

        /// <summary>
        /// Indicates if all base types are ignored.
        /// This is the case if <see cref="Ignore"/> is empty
        /// </summary>
        public bool IgnoreAll => Ignore.Length < 1;
    }
}
