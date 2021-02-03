using System;
using System.Collections.Generic;
using System.Text;
using TypeGen.Core.TypeAnnotations;

namespace TypeGen.AcceptanceTest.Issues.ComplexDictionaries
{

    [ExportTsClass]
    public class GenericComplexDictionaryWrapper<TKey>
    {
        public Dictionary<TKey, string> Dict { get; set; }

        public Dictionary<string, string> SimpleDict { get; set; }

        public IDictionary<TKey, string> IDict { get; set; }
    }
}
