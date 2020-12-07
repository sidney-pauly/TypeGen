using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeGen.Core.Test.GeneratorTestingUtils;
using Xunit;
using Gen = TypeGen.Core.Generator;


namespace TypeGen.AcceptanceTest.Issues.IgnoreSomeBaseTypes
{
    public class IgnoreSomeBaseTypes
    {
        /// <summary>
        /// Looks into generating classes and interfaces with circular type constraints
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(typeof(IgnoresBaseClass), @"TypeGen.AcceptanceTest.Issues.IgnoreSomeBaseTypes.Expected.ignores-base-class.ts")]
        [InlineData(typeof(IgnoresBaseInterface), @"TypeGen.AcceptanceTest.Issues.IgnoreSomeBaseTypes.Expected.ignores-base-interface.ts")]
        [InlineData(typeof(IIgnoresBaseInterface), @"TypeGen.AcceptanceTest.Issues.IgnoreSomeBaseTypes.Expected.i-ignores-base-interface.ts")]
        [InlineData(typeof(IgnoresAllBase), @"TypeGen.AcceptanceTest.Issues.IgnoreSomeBaseTypes.Expected.ignores-all-base.ts")]
        [InlineData(typeof(IIgnoresAllBase), @"TypeGen.AcceptanceTest.Issues.IgnoreSomeBaseTypes.Expected.i-ignores-all-base.ts")]
        public async Task GeneratesCorrectly(Type type, string expectedLocation)
        {
            await new SelfContainedGeneratorTest.SelfContainedGeneratorTest().TestGenerate(type, expectedLocation, true);
        }

        /// <summary>
        /// Tests that the <see cref="ValueType"/> does not get included.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DoesNotContainIgnoredBases()
        {

            var generator = new Gen.Generator();
            var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);

            var generatedFiles = await generator.GenerateAsync(typeof(IIgnoresBaseInterface).Assembly);

            Assert.DoesNotContain("ignored-base-class.ts", generatedFiles);
            Assert.DoesNotContain(typeof(IgnoredBaseClass), interceptor.GeneratedOutputs.Values.Select(o => o.GeneratedFor));

            Assert.DoesNotContain("i-ignored-base-interface.ts", generatedFiles);
            Assert.DoesNotContain(typeof(IIgnoredBaseInterface), interceptor.GeneratedOutputs.Values.Select(o => o.GeneratedFor));
        }
    }
}
