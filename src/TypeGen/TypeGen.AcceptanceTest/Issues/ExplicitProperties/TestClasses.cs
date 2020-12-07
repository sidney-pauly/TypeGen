using System;
using System.Collections.Generic;
using System.Text;
using TypeGen.Core.TypeAnnotations;

namespace TypeGen.AcceptanceTest.Issues.ExplicitProperties
{
    [ExportTsInterface]
    public interface IInterface
    {
        int PrimitiveExplicitProperty { get; set; }

        int PrimitiveImplicitProperty { get; set; }

        FileStyleUriParser ComplexExplicitProperty { get; set; }

        FileStyleUriParser ComplexImplicitProperty { get; set; }
    }

    [ExportTsClass]
    public class ImplementingClass : IInterface
    {
        public int PrimitiveImplicitProperty { get; set; }
        public FileStyleUriParser ComplexImplicitProperty { get; set; }
        int IInterface.PrimitiveExplicitProperty { get; set; }
        FileStyleUriParser IInterface.ComplexExplicitProperty { get; set; }
    }
}
