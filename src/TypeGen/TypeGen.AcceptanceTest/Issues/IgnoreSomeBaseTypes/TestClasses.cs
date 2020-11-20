using System;
using System.Collections.Generic;
using System.Text;
using TypeGen.Core.TypeAnnotations;

namespace TypeGen.AcceptanceTest.Issues.IgnoreSomeBaseTypes
{


    public interface IIgnoredBaseInterface
    {

    }

    public interface ISometimesIgnoredBaseInterface
    {

    }

    public class IgnoredBaseClass
    {

    }

    [ExportTsClass]
    [TsIgnoreBase(typeof(IgnoredBaseClass))]
    public class IgnoresBaseClass : IgnoredBaseClass, ISometimesIgnoredBaseInterface
    {

    }

    [ExportTsClass]
    [TsIgnoreBase(typeof(IIgnoredBaseInterface))]
    public class IgnoresBaseInterface : IIgnoredBaseInterface, ISometimesIgnoredBaseInterface
    {

    }

    [ExportTsInterface]
    [TsIgnoreBase(typeof(IIgnoredBaseInterface))]
    public interface IIgnoresBaseInterface : IIgnoredBaseInterface, ISometimesIgnoredBaseInterface
    {

    }

    [ExportTsClass]
    [TsIgnoreBase]
    public class IgnoresAllBase : IgnoredBaseClass, IIgnoredBaseInterface, ISometimesIgnoredBaseInterface
    {

    }

    [ExportTsInterface]
    [TsIgnoreBase]
    public interface IIgnoresAllBase : IIgnoredBaseInterface, ISometimesIgnoredBaseInterface
    {

    }
}
