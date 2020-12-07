using System;
using System.Collections.Generic;
using System.Text;
using TypeGen.Core.TypeAnnotations;

namespace TypeGen.AcceptanceTest.Issues.IgnoreTypesAndCorrespondingParameters
{
    [TsIgnore]
    public class IgnoredClass
    {
        public virtual int NoneOverittenPrimitiveProperty { get; set; }

        public virtual int OverwrittenPrimitiveProperty { get; set; }

        public virtual FileStyleUriParser NoneOverwrittenCustomProperty { get; set; }

        public virtual FileStyleUriParser OverwrittenCustomProperty { get; set; }

    }

    [TsIgnore]
    public interface IIgnoredInterface
    {
        int ImplicitPrimitiveProperty { get; set; }

        int ExplicitPrimitiveProperty { get; set; }

        FileStyleUriParser ImplicitCustomProperty { get; set; }

        FileStyleUriParser ExplicitCustomProperty { get; set; }

    }

    [TsIgnore(false)]
    public interface IIgnoredInterfaceIncludeExplicit
    {
        int ImplicitPrimitiveProperty { get; set; }

        int ExplicitPrimitiveProperty { get; set; }

        FileStyleUriParser ImplicitCustomProperty { get; set; }

        FileStyleUriParser ExplicitCustomProperty { get; set; }
    }

    [TsIgnore]
    public struct IngoredStruct
    {
        public int ImplicitPrimitiveProperty { get; set; }

        public int ExplicitPrimitiveProperty { get; set; }

        public FileStyleUriParser ImplicitCustomProperty { get; set; }

        public FileStyleUriParser ExplicitCustomProperty { get; set; }
    }

    [ExportTsClass]
    public class ClassWithIgnoredBaseClass : IgnoredClass
    {
        public override int OverwrittenPrimitiveProperty { get => base.OverwrittenPrimitiveProperty; set => base.OverwrittenPrimitiveProperty = value; }

        public override FileStyleUriParser OverwrittenCustomProperty { get => base.OverwrittenCustomProperty; set => base.OverwrittenCustomProperty = value; }

    }

    [ExportTsClass]
    public class ClassWithIgnoredInterfaceBase : IIgnoredInterface
    {
        public int ImplicitPrimitiveProperty { get; set; }
        public FileStyleUriParser ImplicitCustomProperty { get; set; }
        public int IncludedProperty { get; set; }
        int IIgnoredInterface.ExplicitPrimitiveProperty { get; set; }
        FileStyleUriParser IIgnoredInterface.ExplicitCustomProperty { get; set; }
    }

    [ExportTsClass]
    public class ClassWithIgnoredInterfaceBaseKeepExplicit : IIgnoredInterfaceIncludeExplicit
    {
        public int ImplicitPrimitiveProperty { get; set; }
        public FileStyleUriParser ImplicitCustomProperty { get; set; }
        public int IncludedProperty { get; set; }
        int IIgnoredInterfaceIncludeExplicit.ExplicitPrimitiveProperty { get; set; }
        FileStyleUriParser IIgnoredInterfaceIncludeExplicit.ExplicitCustomProperty { get; set; }
    }

    [ExportTsInterface]
    public interface IInterfaceWithIgnoredBase : IIgnoredInterface
    {
    }

    [ExportTsInterface]
    public interface IInterfaceWithIgnoredBaseKeepExplicit : IIgnoredInterfaceIncludeExplicit
    {
    }

    [ExportTsClass]
    public class ClassWithTransientIgnoredInterfaceBase : IInterfaceWithIgnoredBase
    {
        public int ImplicitPrimitiveProperty { get; set; }
        public FileStyleUriParser ImplicitCustomProperty { get; set; }
        public int IncludedProperty { get; set; }
        int IIgnoredInterface.ExplicitPrimitiveProperty { get; set; }
        FileStyleUriParser IIgnoredInterface.ExplicitCustomProperty { get; set; }
    }

    [ExportTsClass]
    public class ClassWithTransientIgnoredInterfaceBaseKeepExplicit : IInterfaceWithIgnoredBaseKeepExplicit
    {
        public int ImplicitPrimitiveProperty { get; set; }
        public FileStyleUriParser ImplicitCustomProperty { get; set; }
        public int IncludedProperty { get; set; }
        int IIgnoredInterfaceIncludeExplicit.ExplicitPrimitiveProperty { get; set; }
        FileStyleUriParser IIgnoredInterfaceIncludeExplicit.ExplicitCustomProperty { get; set; }
    }

}
