﻿using DependencyNamespace;
using DependencyNamespace.DependencySubnamespace;
using DependencyNamespace.DependencySubnamespace.SubSubNamespace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TypeGen.AcceptanceTest.GeneratorTestingUtils;
using TypeGen.AcceptanceTest.Issues.CustomDependencies.TestClasses;
using TypeGen.Core.Generator;
using TypeGen.Core.SpecGeneration;
using TypeGen.Core.Test.GeneratorTestingUtils;
using Xunit;
using Gen = TypeGen.Core.Generator;

namespace TypeGen.AcceptanceTest.Issues.CustomDependencies
{
    public class CustomDependency
    {

        

        static Regex NestedImportRegex = new Regex("import {.+} from \".*\";", RegexOptions.Multiline);
        static Regex DefaultExportImportRegex = new Regex("import .* from \".*\";", RegexOptions.Multiline);

        [InlineData(typeof(MissingBase), typeof(FileStyleUriParser))]
        [InlineData(typeof(MissingProperty), typeof(FileStyleUriParser))]
        [InlineData(typeof(MissingField), typeof(FileStyleUriParser))]
        [InlineData(typeof(IMissingBase), typeof(IAsyncResult))]
        [InlineData(typeof(IMissingProperty), typeof(FileStyleUriParser))]
        [Theory]
        public async Task ThrowsOnMissingDependency(Type type, Type missingDependency)
        {
            var opt = new GeneratorOptions { 
                IncludeExplicitProperties = true,
                StrictDependencies = true
            };

            var generator = new Gen.Generator(opt);
            var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);

            var thrownExcpeiton = await Assert.ThrowsAsync<MissingDependencyException>(
                () => generator.GenerateAsync(new[] { new MinimalTypesSpec(type, new Type[] { }) }
                ));

            Assert.Equal(type, thrownExcpeiton.RequireringType);
            Assert.Equal(missingDependency, thrownExcpeiton.RequiredType);
            
        }

        [InlineData(typeof(ITransientMissingDependency), typeof(FileStyleUriParser), typeof(MissingBase))]
        [InlineData(typeof(TransientMissingDependency), typeof(FileStyleUriParser), typeof(MissingBase))]
        [Theory]
        public async Task ThrowsOnMissingTransientDependency(Type type, Type missingDependency, params Type[] additionalTypes)
        {
            var opt = new GeneratorOptions
            {
                IncludeExplicitProperties = true,
                StrictDependencies = true
            };

            var generator = new Gen.Generator(opt);
            var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);

            var thrownExcpeiton = await Assert.ThrowsAsync<MissingDependencyException>(
                () => generator.GenerateAsync(new[] { new MinimalTypesSpec(type, additionalTypes ?? new Type[] { }) }
                ));

            Assert.Contains(thrownExcpeiton.RequireringType, additionalTypes);
            Assert.Equal(missingDependency, thrownExcpeiton.RequiredType);

        }

        [InlineData(typeof(FileStyleUriParserDependent), typeof(FileStyleUriParser), false)]
        [InlineData(typeof(NamespaceDependent), typeof(NamespacedDependency), false)]
        [InlineData(typeof(SubNamespaceDependent), typeof(SubNamespacedDependency), false)]
        [InlineData(typeof(SubSubNamespaceDependent), typeof(SubSubNamespacedDependency), false)]
        [InlineData(typeof(FileStyleUriParserDependent), typeof(FileStyleUriParser), true)]
        [InlineData(typeof(NamespaceDependent), typeof(NamespacedDependency), true)]
        [InlineData(typeof(SubNamespaceDependent), typeof(SubNamespacedDependency), true)]
        [InlineData(typeof(SubSubNamespaceDependent), typeof(SubSubNamespacedDependency), true)]
        [Theory]
        public async Task DirectlyProvidedDependencyPath(Type type, Type dependency, bool defaultExport)
        {

            Guid importLocation = Guid.NewGuid();
            var importMap = new CustomDependencyMap();
            importMap.Add(dependency, new CustomTSDependency(importLocation.ToString(), false, defaultExport));

            var opt = new GeneratorOptions
            {
                IncludeExplicitProperties = true,
                StrictDependencies = true,
                CustomDependencyMapping = importMap
            };

            var generator = new Gen.Generator(opt);
            var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);

            await generator.GenerateAsync(new[] { new MinimalTypesSpec(type, new Type[] { }) });

            Assert.Single(interceptor.GeneratedOutputs);

            var imports = (defaultExport ? DefaultExportImportRegex : NestedImportRegex).Matches(interceptor.GeneratedOutputs.Values.First().Content);

            Assert.Single(imports);

            var actualImportLocation = imports.First().Value.Split('"')[1];
            Assert.Equal(importLocation.ToString(), actualImportLocation);

        }

        [InlineData(typeof(NamespaceDependent), typeof(NamespacedDependency))]
        [InlineData(typeof(SubNamespaceDependent), typeof(SubNamespacedDependency))]
        [InlineData(typeof(SubSubNamespaceDependent), typeof(SubSubNamespacedDependency))]
        [Theory]
        public async Task ImportsFromSubnamespace(Type type, Type dep)
        {

            Guid importLocation = Guid.NewGuid();
            var importMap = new CustomDependencyMap();
            importMap.Add(dep.Namespace.Split('.').First(), new CustomTSDependency(importLocation.ToString()));

            var opt = new GeneratorOptions
            {
                IncludeExplicitProperties = true,
                StrictDependencies = true,
                CustomDependencyMapping = importMap
            };

            var generator = new Gen.Generator(opt);
            var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);

            await generator.GenerateAsync(new[] { new MinimalTypesSpec(type, new Type[] { }) });

            Assert.Single(interceptor.GeneratedOutputs);

            var imports = NestedImportRegex.Matches(interceptor.GeneratedOutputs.Values.First().Content);

            Assert.Single(imports);

            string namespaceDiff = String.Join("/", dep.Namespace.Split('.').Skip(1));
            if (namespaceDiff.Length > 0)
                namespaceDiff = "/" + namespaceDiff;

            var actualImportLocation = imports.First().Value.Split('"')[1];
            Assert.Equal(importLocation.ToString() + namespaceDiff, actualImportLocation );

        }


    }
}
