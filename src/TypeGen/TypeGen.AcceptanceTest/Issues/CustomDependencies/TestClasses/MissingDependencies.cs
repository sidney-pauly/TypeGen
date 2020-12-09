using System;
using System.Collections.Generic;
using System.Text;
using TypeGen.Core.TypeAnnotations;

namespace TypeGen.AcceptanceTest.Issues.CustomDependencies.TestClasses
{
    public class MissingBase : FileStyleUriParser
    {
    }

    public class MissingProperty
    {
        public FileStyleUriParser MissingPropertyDependency { get; set; }
    }

    [ExportTsClass]
    public class MissingField
    {
        public FileStyleUriParser MissingFieldDependency;
    }

    public class TransientMissingDependency
    {
        public MissingBase TransientMissingProperty { get; set; }
    }

    public interface IMissingBase : IAsyncResult
    {
    }

    public interface IMissingProperty
    {
        FileStyleUriParser MissingPropertyDependency { get; set; }

    }

    public interface ITransientMissingDependency
    {
        MissingBase TransientMissingProperty { get; set; }
    }

}
