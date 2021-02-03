using System;
using System.Collections.Generic;
using System.Text;
using TypeGen.Core.TypeAnnotations;

namespace TypeGen.AcceptanceTest.Issues.ComplexDictionaries
{


    [ExportTsClass]
    public class ComplexDictionaryWrapper
    {
        public Dictionary<ComplexDictionaryKey, string> Dict { get; set; }

        public Dictionary<string, string> SimpleDict { get; set; }

        public IDictionary<ComplexDictionaryKey, string> IDict { get; set; }
    }
}
