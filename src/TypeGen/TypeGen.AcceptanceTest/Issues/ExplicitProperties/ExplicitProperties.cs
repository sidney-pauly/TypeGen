using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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
            await new SelfContainedGeneratorTest.SelfContainedGeneratorTest().TestGenerate(type, expectedLocation, true);
        }
    }
}
