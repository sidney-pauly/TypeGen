using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeGen.AcceptanceTest.GeneratorTestingUtils;
using TypeGen.Core.Converters;
using TypeGen.Core.Test.GeneratorTestingUtils;
using Xunit;
using Gen = TypeGen.Core.Generator;


namespace TypeGen.AcceptanceTest.Issues.IgnoresGenericParamsIfWrappingTypeIsIgnored
{

    public class IgnoresGenericParamsIfWrappingTypeIsIgnored
    {

        /// <summary>
        /// Looks into generating classes and interfaces with circular type constraints
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [InlineData(typeof(IImplementingWrapper))]
        [InlineData(typeof(TransientImplementingWrapper), typeof(IImplementingWrapper))]
        [Theory]
        public async Task GeneratesCorrectly(Type type, params Type[] otherTypes)
        {

            var opt = new Gen.GeneratorOptions { IncludeExplicitProperties = true };
            opt.PropertyNameConverters = new MemberNameConverterCollection(opt.PropertyNameConverters.Where(c => c.GetType() != typeof(PascalCaseToCamelCaseConverter)));
            opt.StrictDependencies = true;
            var generator = new Gen.Generator(opt);
            var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);


            await generator.GenerateAsync(new[] { new MinimalTypesSpec(type, otherTypes) });

            Assert.DoesNotContain(typeof(IGenericWrapper<>), interceptor.GeneratedOutputs.Keys);
        }
    }

}
