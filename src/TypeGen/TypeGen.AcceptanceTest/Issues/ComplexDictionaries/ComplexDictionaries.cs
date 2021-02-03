using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Gen = TypeGen.Core.Generator;


namespace TypeGen.AcceptanceTest.Issues.ComplexDictionaries
{

    public class ComplexDictionaries
    {

        [Theory]
        [InlineData(typeof(ComplexDictionaryWrapper), @"TypeGen.AcceptanceTest.Issues.ComplexDictionaries.Expected.complex-dictionary-wrapper.ts")]
        [InlineData(typeof(GenericComplexDictionaryWrapper<>), @"TypeGen.AcceptanceTest.Issues.ComplexDictionaries.Expected.generic-complex-dictionary-wrapper.ts")]
        [InlineData(typeof(GenericComplexDictionaryWrapperConstraint<>), @"TypeGen.AcceptanceTest.Issues.ComplexDictionaries.Expected.generic-complex-dictionary-wrapper-constraint.ts")]
        public async Task Generate(Type type, string expectedLocation)
        {

            var opt = new Gen.GeneratorOptions
            {
                ComplexDictionaryMode = Gen.GeneratorOptionsDictionaryModes.ArrayMode
            };

            await new SelfContainedGeneratorTest.SelfContainedGeneratorTest().TestGenerate(type, expectedLocation, opt);

        }
    }
}
