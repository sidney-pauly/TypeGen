using System;
using System.Collections.Generic;
using System.Text;
using TypeGen.Core.TypeAnnotations;

namespace TypeGen.AcceptanceTest.Issues.ComplexDictionaries
{
    [ExportTsClass]
    public class ComplexDictionaryKey
    {
        public string PropA { get; set; }

        public string PropB { get; set; }

    }
}
