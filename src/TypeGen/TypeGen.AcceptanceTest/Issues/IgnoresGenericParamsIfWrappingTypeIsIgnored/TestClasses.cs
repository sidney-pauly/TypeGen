using System;
using System.Collections.Generic;
using System.Text;
using TypeGen.Core.TypeAnnotations;

namespace TypeGen.AcceptanceTest.Issues.IgnoresGenericParamsIfWrappingTypeIsIgnored
{
    [TsIgnore]
    public interface IGenericWrapper<T>
    {
    }

    [ExportTsInterface]
    public interface IImplementingWrapper : IGenericWrapper<FileStyleUriParser>
    {
    }

    [ExportTsClass]
    public class TransientImplementingWrapper : IImplementingWrapper
    {

    }
}
