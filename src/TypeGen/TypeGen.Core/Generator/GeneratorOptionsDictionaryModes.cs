using System;
using System.Collections.Generic;
using System.Text;

namespace TypeGen.Core.Generator
{
    /// <summary>
    /// Options for how <see cref="IDictionary{TKey, TValue}"/> should be generated
    /// </summary>
    public enum GeneratorOptionsDictionaryModes
    {
        /// <summary>
        /// Only <see cref="Dictionary{TKey, TValue}"/> with basic key types (strings, numbers, etc.) are
        /// generated. Simple dictionaries are always implemented as <c>type = {[index: keyType]: valueTyoe}</c>.
        /// If a <see cref="IDictionary{TKey, TValue}"/> with complex keys is encoutered a exception is thrown
        /// </summary>
        BasicTypesOnly,

        /// <summary>
        /// Dictionaries with complex keys are transformed into KeyValue arrays: <c>type = {key: keyType, value: valueType}[]</c>
        /// </summary>
        ArrayMode,

        /// <summary>
        /// A custom ts type like "Map&lt;T, K&gt;" is used to build the Dictionary. 
        /// The <see cref="GeneratorOptions.CustomComplexDictionaryType"/> has to be set for this option.
        /// </summary>
        CustomComplexDictionaryType

    }
}
