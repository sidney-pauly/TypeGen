using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeGen.Core.Test.GeneratorTestingUtils;
using Xunit;
using Gen = TypeGen.Core.Generator;

namespace TypeGen.AcceptanceTest.Issues.IgnoreTypesAndCorrespondingParameters
{
    public class IgnoreTypes
    {
        /// <summary>
        /// Tests that the <see cref="ValueType"/> does not get included.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestTypesNotGenerated()
        {

            var generator = new Gen.Generator();
            var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);

            var generatedFiles = await generator.GenerateAsync(typeof(IgnoredClass).Assembly);

            Assert.DoesNotContain("ignored-class.ts", generatedFiles);
            Assert.DoesNotContain(typeof(IgnoredClass), interceptor.GeneratedOutputs.Values.Select(o => o.GeneratedFor));

            Assert.DoesNotContain("i-ignored-interface.ts", generatedFiles);
            Assert.DoesNotContain(typeof(IIgnoredInterface), interceptor.GeneratedOutputs.Values.Select(o => o.GeneratedFor));

            Assert.DoesNotContain("ignored-struct.ts", generatedFiles);
            Assert.DoesNotContain(typeof(IngoredStruct), interceptor.GeneratedOutputs.Values.Select(o => o.GeneratedFor));
        }
    }
}
