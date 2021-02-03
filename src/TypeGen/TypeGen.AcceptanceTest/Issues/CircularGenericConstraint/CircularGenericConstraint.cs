﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeGen.AcceptanceTest.GeneratorTestingUtils;
using TypeGen.AcceptanceTest.SelfContainedGeneratorTest;
using TypeGen.Core.Test.GeneratorTestingUtils;
using TypeGen.Core.TypeAnnotations;
using Xunit;
using Gen = TypeGen.Core.Generator;


namespace TypeGen.AcceptanceTest.Issues
{
    public class CircularGenericConstraint
    {

        /// <summary>
        /// Looks into generating classes and interfaces with circular type constraints
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(typeof(RecursiveConstraintClass<>), @"TypeGen.AcceptanceTest.Issues.CircularGenericConstraint.Expected.RecursiveConstraintClass.ts")]
        [InlineData(typeof(IRecursiveConstraintInterface<>), @"TypeGen.AcceptanceTest.Issues.CircularGenericConstraint.Expected.IRecursiveConstraintInterface.ts")]
        [InlineData(typeof(IRecursiveConstraintInterfaceWithClassConstraint<>), @"TypeGen.AcceptanceTest.Issues.CircularGenericConstraint.Expected.IRecursiveConstraintInterfaceWithClassConstraint.ts")]
        [InlineData(typeof(IRecursiveConstraintInterfaceWithStructConstraint<>), @"TypeGen.AcceptanceTest.Issues.CircularGenericConstraint.Expected.IRecursiveConstraintInterfaceWithStructConstraint.ts")]
        [InlineData(typeof(IMultipleConstraintInterface<>), @"TypeGen.AcceptanceTest.Issues.CircularGenericConstraint.Expected.IMultipleConstraintInterface.ts")]
        [InlineData(typeof(ICicrularConstraintInterface<,>), @"TypeGen.AcceptanceTest.Issues.CircularGenericConstraint.Expected.ICicrularConstraintInterface.ts")]
        public async Task GeneratesCorrectly(Type type, string expectedLocation)
        {
            await new SelfContainedGeneratorTest.SelfContainedGeneratorTest().TestGenerate(type, expectedLocation);
        }

        /// <summary>
        /// Tests that the <see cref="ValueType"/> does not get included.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DoesNotGenerateValueType()
        {

            var generator = new Gen.Generator(new Gen.GeneratorOptions { ComplexDictionaryMode = Gen.GeneratorOptionsDictionaryModes.ArrayMode });
            var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);

            var generatedFiles = await generator.GenerateAsync(typeof(IRecursiveConstraintInterfaceWithStructConstraint<>).Assembly);

            Assert.DoesNotContain("value-type.ts", generatedFiles);
            Assert.DoesNotContain(typeof(ValueType), interceptor.GeneratedOutputs.Values.Select(o => o.GeneratedFor));
        }
    }
}
