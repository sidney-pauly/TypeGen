using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TypeGen.AcceptanceTest.GeneratorTestingUtils;
using Gen = TypeGen.Core.Generator;
using TypeGen.Core.Test.GeneratorTestingUtils;
using Xunit;
using TypeGen.Core.Converters;
using System.Linq;

namespace TypeGen.AcceptanceTest.Issues.ExplicitProperties
{
    public class ExplicitProperties
    {
        /// <summary>
        /// Looks into generating classes and interfaces with circular type constraints
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(typeof(ImplementingClass), @"TypeGen.AcceptanceTest.Issues.ExplicitProperties.Expected.implementing-class.ts")]
        public async Task GeneratesCorrectly(Type type, string expectedLocation)
        {
            var readExpectedTask = EmbededResourceReader.GetEmbeddedResourceAsync(expectedLocation);

            var opt = new Gen.GeneratorOptions { IncludeExplicitProperties = true, ComplexDictionaryMode = Gen.GeneratorOptionsDictionaryModes.ArrayMode };
            opt.PropertyNameConverters = new MemberNameConverterCollection(opt.PropertyNameConverters.Where(c => c.GetType() != typeof(PascalCaseToCamelCaseConverter)));
            var generator = new Gen.Generator(opt);
            var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);


            await generator.GenerateAsync(type.Assembly);
            var expected = (await readExpectedTask).Trim();

            Assert.True(interceptor.GeneratedOutputs.ContainsKey(type));
            Assert.Equal(expected, GeneratedOutput.FromatOutput(interceptor.GeneratedOutputs[type].Content));
        }
    }
}
