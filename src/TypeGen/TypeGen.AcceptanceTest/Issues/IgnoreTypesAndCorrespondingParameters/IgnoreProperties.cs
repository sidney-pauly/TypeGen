using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TypeGen.AcceptanceTest.Issues.IgnoreTypesAndCorrespondingParameters
{
    public class IgnoreProperties
    {
        // <summary>
        /// Looks into generating classes and interfaces with circular type constraints
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(typeof(ClassWithIgnoredBaseClass), @"TypeGen.AcceptanceTest.Issues.IgnoreTypesAndCorrespondingParameters.Expected.ClassWithIgnoredBaseClass.ts")]
        [InlineData(typeof(ClassWithIgnoredInterfaceBase), @"TypeGen.AcceptanceTest.Issues.IgnoreTypesAndCorrespondingParameters.Expected.ClassWithIgnoredInterfaceBase.ts")]
        [InlineData(typeof(IInterfaceWithIgnoredBase), @"TypeGen.AcceptanceTest.Issues.IgnoreTypesAndCorrespondingParameters.Expected.IInterfaceWithIgnoredBase.ts")]
        [InlineData(typeof(IInterfaceWithIgnoredBaseKeepExplicit), @"TypeGen.AcceptanceTest.Issues.IgnoreTypesAndCorrespondingParameters.Expected.IInterfaceWithIgnoredBaseKeepExplicit.ts")]
        [InlineData(typeof(ClassWithTransientIgnoredInterfaceBase), @"TypeGen.AcceptanceTest.Issues.IgnoreTypesAndCorrespondingParameters.Expected.ClassWithTransientIgnoredInterfaceBase.ts")]
        [InlineData(typeof(ClassWithTransientIgnoredInterfaceBaseKeepExplicit), @"TypeGen.AcceptanceTest.Issues.IgnoreTypesAndCorrespondingParameters.Expected.ClassWithTransientIgnoredInterfaceBaseKeepExplicit.ts")]
        public async Task GeneratesCorrectly(Type type, string expectedLocation)
        {
            await new SelfContainedGeneratorTest.SelfContainedGeneratorTest().TestGenerate(type, expectedLocation, true);
        }
    }
}
