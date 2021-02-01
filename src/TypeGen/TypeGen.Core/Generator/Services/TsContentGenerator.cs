﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using TypeGen.Core.Extensions;
using TypeGen.Core.Logging;
using TypeGen.Core.Metadata;
using TypeGen.Core.SpecGeneration;
using TypeGen.Core.TypeAnnotations;
using TypeGen.Core.Utils;
using TypeGen.Core.Validation;

namespace TypeGen.Core.Generator.Services
{
    /// <summary>
    /// Generates TypeScript file contents
    /// </summary>
    internal class TsContentGenerator : ITsContentGenerator
    {
        private readonly ITypeDependencyService _typeDependencyService;
        private readonly ITypeService _typeService;
        private readonly ITemplateService _templateService;
        private readonly ITsContentParser _tsContentParser;
        private readonly IMetadataReaderFactory _metadataReaderFactory;
        private readonly IGeneratorOptionsProvider _generatorOptionsProvider;
        private readonly ILogger _logger;

        private const string KeepTsTagName = "keep-ts";
        private const string CustomHeadTagName = "custom-head";
        private const string CustomBodyTagName = "custom-body";

        private GeneratorOptions GeneratorOptions => _generatorOptionsProvider.GeneratorOptions;

        public TsContentGenerator(ITypeDependencyService typeDependencyService,
            ITypeService typeService,
            ITemplateService templateService,
            ITsContentParser tsContentParser,
            IMetadataReaderFactory metadataReaderFactory,
            IGeneratorOptionsProvider generatorOptionsProvider,
            ILogger logger)
        {
            _typeDependencyService = typeDependencyService;
            _typeService = typeService;
            _templateService = templateService;
            _tsContentParser = tsContentParser;
            _metadataReaderFactory = metadataReaderFactory;
            _generatorOptionsProvider = generatorOptionsProvider;
            _logger = logger;
        }

        /// <summary>
        /// Gets code for the 'imports' section for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="outputDir">ExportTs... attribute's output dir</param>
        /// <param name="otherTypes"></param>
        /// <param name="customDependencyMapping"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when one of: type, fileNameConverters or typeNameConverters is null</exception>
        public string GetImportsText(Type type, string outputDir, IDictionary<Type, TypeSpec> otherTypes, CustomDependencyMap customDependencyMapping, bool strict)
        {
            Requires.NotNull(type, nameof(type));
            Requires.NotNull(GeneratorOptions.FileNameConverters, nameof(GeneratorOptions.FileNameConverters));
            Requires.NotNull(GeneratorOptions.TypeNameConverters, nameof(GeneratorOptions.TypeNameConverters));

            string result = GetTypeDependencyImportsText(type, outputDir, otherTypes, customDependencyMapping, strict);
            result += GetCustomImportsText(type);

            if (!string.IsNullOrEmpty(result))
            {
                result += "\r\n";
            }

            return result;
        }

        /// <summary>
        /// Gets the text for the "extends" section
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetExtendsText(Type type)
        {
            Requires.NotNull(type, nameof(type));
            Requires.NotNull(GeneratorOptions.TypeNameConverters, nameof(GeneratorOptions.TypeNameConverters));

            Type baseType = _typeService.GetBaseType(type);
            if (baseType == null) return "";

            if (GetFileteredBaseTypes(type, new[] { baseType }).Count() < 1)
                return "";

            string baseTypeName = _typeService.GetTsTypeName(baseType, true);
            return _templateService.GetExtendsText(baseTypeName);
        }

        /// <summary>
        /// Gets the text for the "extends" section for interfaces.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetExtendsForInterfacesText(Type type)
        {
            Requires.NotNull(type, nameof(type));
            Requires.NotNull(GeneratorOptions.TypeNameConverters, nameof(GeneratorOptions.TypeNameConverters));

            IEnumerable<Type> baseTypes = _typeService.GetInterfaces(type);
            if (!baseTypes.Any()) return "";

            List<string> baseTypeNames = GetFileteredBaseTypes(type, baseTypes).Select(baseType => _typeService.GetTsTypeName(baseType, true)).ToList();
            if (baseTypeNames.Count < 1)
                return "";
            return _templateService.GetExtendsText(baseTypeNames);
        }

        /// <summary>
        /// Gets the text for the "implements" section
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetImplementsText(Type type)
        {
            Requires.NotNull(type, nameof(type));
            Requires.NotNull(GeneratorOptions.TypeNameConverters, nameof(GeneratorOptions.TypeNameConverters));

            IEnumerable<Type> baseTypes = _typeService.GetInterfaces(type);
            if (!baseTypes.Any()) return "";

            List<string> baseTypeNames = GetFileteredBaseTypes(type, baseTypes).Select(baseType => _typeService.GetTsTypeName(baseType, true)).ToList();
            if (baseTypeNames.Count < 1)
                return "";
            return _templateService.GetImplementsText(baseTypeNames);
        }

        /// <summary>
        /// Filters out all base types that should be ignored because of the <see cref="TsIgnoreBaseAttribute"/>
        /// or the <see cref="TsIgnoreAttribute"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseTypes"></param>
        /// <returns></returns>
        private IEnumerable<Type> GetFileteredBaseTypes(Type type, IEnumerable<Type> baseTypes)
        {
            var filtered = baseTypes
                .Where(t => !_typeService.IsIgnoredType(t.IsGenericType ? t.GetGenericTypeDefinition() : t));
            var ignoreBaseAttribute = _metadataReaderFactory.GetInstance().GetAttribute<TsIgnoreBaseAttribute>(type);
            if (ignoreBaseAttribute == null)
                return filtered;
            if (ignoreBaseAttribute.IgnoreAll)
                return new Type[] { };

            var toIgnore = new HashSet<Type>(ignoreBaseAttribute.Ignore);
            return filtered
                .Where(t => !toIgnore.Contains(t));
        }

        /// <summary>
        /// Returns TypeScript imports source code related to type dependencies
        /// </summary>
        /// <param name="type"></param>
        /// <param name="outputDir"></param>
        /// <param name="otherTypes"></param>
        /// <param name="customDependencyMapping"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        private string GetTypeDependencyImportsText(Type type, string outputDir, IDictionary<Type, TypeSpec> otherTypes, CustomDependencyMap customDependencyMapping, bool strict)
        {
            if (!string.IsNullOrEmpty(outputDir) && !outputDir.EndsWith("/") && !outputDir.EndsWith("\\")) outputDir += "\\";
            var result = "";
            IEnumerable<TypeDependencyInfo> typeDependencies = _typeDependencyService.GetTypeDependencies(type);

            // exclude base type dependency if TsCustomBaseAttribute is specified (it will be added in custom imports)
            if (_metadataReaderFactory.GetInstance().GetAttribute<TsCustomBaseAttribute>(type) != null)
            {
                typeDependencies = typeDependencies.Where(td => !td.IsBase);
            }

            // Filter out dependencies to complex dictionary if none where provided
            if (!GeneratorOptions.CustomDependencyMapping.ContainsKey(typeof(ComplexDictionaryDummyType).FullName))
                typeDependencies = typeDependencies.Where(t => t.Type != typeof(ComplexDictionaryDummyType));

            foreach (TypeDependencyInfo typeDependencyInfo in typeDependencies)
            {
                Type typeDependency = typeDependencyInfo.Type;

                string typeDependencyName = typeDependency.Name.RemoveTypeArity();
                string typeName = GeneratorOptions.TypeNameConverters.Convert(typeDependencyName, typeDependency);

                bool defaultExport;
                string dependencyPath;

                if (!TryGetCutomImport(typeDependency, customDependencyMapping, out dependencyPath, out defaultExport))
                {

                    if (strict && !otherTypes.ContainsKey(typeDependency))
                        throw new MissingDependencyException(type, typeDependency);

                    string dependencyOutputDir = GetTypeDependencyOutputDir(typeDependencyInfo, outputDir);

                    // get path diff
                    string pathDiff = FileSystemUtils.GetPathDiff(outputDir, dependencyOutputDir);
                    pathDiff = pathDiff.StartsWith("..\\") || pathDiff.StartsWith("../") ? pathDiff : $"./{pathDiff}";

                    // get type & file name
                    string fileName = GeneratorOptions.FileNameConverters.Convert(typeDependencyName, typeDependency);

                    // get file path
                    dependencyPath = Path.Combine(pathDiff.EnsurePostfix("/"), fileName);
                    dependencyPath = dependencyPath.Replace('\\', '/');

                    defaultExport = _typeService.UseDefaultExport(typeDependency);

                }

                result += defaultExport ?
                    _templateService.FillImportDefaultExportTemplate(typeName, dependencyPath) :
                    _templateService.FillImportTemplate(typeName, "", dependencyPath);
            }

            return result;
        }

        /// <summary>
        /// Gets code for imports that are specified in TsTypeAttribute.ImportPath or TsCustomBaseAttribute.ImportPath properties
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetCustomImportsText(Type type)
        {
            var resultLines = new List<string>();

            resultLines.AddRange(GetCustomImportsFromCustomBase(type));
            resultLines.AddRange(GetCustomImportsFromMembers(type));

            return string.Join("", resultLines.Distinct());
        }

        private IEnumerable<string> GetCustomImportsFromMembers(Type type)
        {
            IEnumerable<MemberInfo> members = type.GetTsExportableMembers(_metadataReaderFactory.GetInstance(), GeneratorOptions.IncludeExplicitProperties, GeneratorOptions.NeverImplementedPropertyTypes);

            IEnumerable<TsTypeAttribute> typeAttributes = members
                .Select(memberInfo => _metadataReaderFactory.GetInstance().GetAttribute<TsTypeAttribute>(memberInfo))
                .Where(tsTypeAttribute => !string.IsNullOrEmpty(tsTypeAttribute?.ImportPath))
                .Distinct(new TsTypeAttributeComparer());

            foreach (TsTypeAttribute attribute in typeAttributes)
            {
                yield return FillCustomImportTemplate(attribute.FlatTypeName, attribute.ImportPath, attribute.OriginalTypeName, attribute.IsDefaultExport);
            }
        }

        private IEnumerable<string> GetCustomImportsFromCustomBase(Type type)
        {
            var tsCustomBaseAttribute = _metadataReaderFactory.GetInstance().GetAttribute<TsCustomBaseAttribute>(type);
            if (tsCustomBaseAttribute == null || string.IsNullOrEmpty(tsCustomBaseAttribute.ImportPath)) yield break;

            yield return FillCustomImportTemplate(tsCustomBaseAttribute.Base, tsCustomBaseAttribute.ImportPath, tsCustomBaseAttribute.OriginalTypeName, tsCustomBaseAttribute.IsDefaultExport);
        }

        private string FillCustomImportTemplate(string typeName, string importPath, string originalTypeName, bool isDefaultExport)
        {
            bool withOriginalTypeName = !string.IsNullOrEmpty(originalTypeName);

            string name = withOriginalTypeName ? originalTypeName : typeName;
            string typeAlias = withOriginalTypeName ? typeName : null;

            return isDefaultExport ? _templateService.FillImportDefaultExportTemplate(name, importPath) :
                _templateService.FillImportTemplate(name, typeAlias, importPath);
        }

        /// <summary>
        /// Tries to get a custom import path for the provided type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="customDependencyMapping"></param>
        /// <param name="alternativeImportPath"></param>
        /// <param name="defaultExport"></param>
        /// <returns></returns>
        private bool TryGetCutomImport(Type type, CustomDependencyMap customDependencyMapping, out string? alternativeImportPath, out bool defaultExport)
        {
            if (customDependencyMapping.ContainsKey(type.FullName))
            {
                var alternativeImport = customDependencyMapping[type.FullName];
                alternativeImportPath = alternativeImport.Path;
                defaultExport = alternativeImport.DefaultExport;
                return true;
            }
            string _namespace = type.Namespace;
            List<string> droppedNamepaces = new List<string>();
            while (true)
            {
                if (customDependencyMapping.ContainsKey(_namespace))
                {
                    var alternativeImport = customDependencyMapping[_namespace];
                    if (alternativeImport.FlatDependencyStructure)
                        alternativeImportPath = alternativeImport.Path;
                    else if (droppedNamepaces.Count < 1)
                        alternativeImportPath = alternativeImport.Path;
                    else
                    {
                        droppedNamepaces.Reverse();
                        alternativeImportPath = alternativeImport.Path + droppedNamepaces.Prepend("").Aggregate((a, b) => a + "/" + b);
                    }

                    defaultExport = false;
                    return true;
                }
                if (!_namespace.Contains("."))
                    break;
                var split = _namespace.Split('.');
                    droppedNamepaces.Add(split[split.Length - 1]);
                _namespace = String.Join(".", split.Take(split.Length - 1));
            }
            alternativeImportPath = null;
            defaultExport = false;
            return false;

        }

        /// <summary>
        /// Gets the output directory for a type dependency
        /// </summary>
        /// <param name="typeDependencyInfo"></param>
        /// <param name="parentTypeOutputDir"></param>
        /// <returns></returns>
        private string GetTypeDependencyOutputDir(TypeDependencyInfo typeDependencyInfo, string parentTypeOutputDir)
        {
            var classAttribute = _metadataReaderFactory.GetInstance().GetAttribute<ExportTsClassAttribute>(typeDependencyInfo.Type);
            var interfaceAttribute = _metadataReaderFactory.GetInstance().GetAttribute<ExportTsInterfaceAttribute>(typeDependencyInfo.Type);
            var enumAttribute = _metadataReaderFactory.GetInstance().GetAttribute<ExportTsEnumAttribute>(typeDependencyInfo.Type);

            if (classAttribute == null && enumAttribute == null && interfaceAttribute == null)
            {
                TsDefaultTypeOutputAttribute defaultTypeOutputAttribute = typeDependencyInfo.MemberAttributes
                    ?.SingleOrDefault(a => a.GetType() == typeof(TsDefaultTypeOutputAttribute))
                    as TsDefaultTypeOutputAttribute;

                return defaultTypeOutputAttribute?.OutputDir ?? parentTypeOutputDir;
            }

            return classAttribute?.OutputDir
                    ?? interfaceAttribute?.OutputDir
                    ?? enumAttribute?.OutputDir;
        }

        /// <summary>
        /// Gets custom code for a TypeScript file given by filePath.
        /// Returns an empty string if a file does not exist.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="indentSize"></param>
        /// <returns></returns>
        public string GetCustomBody(string filePath, int indentSize)
        {
            Requires.NotNull(filePath, nameof(filePath));

            string content = _tsContentParser.GetTagContent(filePath, indentSize, KeepTsTagName, CustomBodyTagName);
            string tab = StringUtils.GetTabText(indentSize);

            return string.IsNullOrEmpty(content)
                ? ""
                : $"\r\n\r\n{tab}//<{CustomBodyTagName}>\r\n{tab}{content}{tab}//</{CustomBodyTagName}>";
        }

        /// <summary>
        /// Gets custom code for a TypeScript file given by filePath.
        /// Returns an empty string if a file does not exist.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string GetCustomHead(string filePath)
        {
            Requires.NotNull(filePath, nameof(filePath));

            string content = _tsContentParser.GetTagContent(filePath, 0, CustomHeadTagName);
            return string.IsNullOrEmpty(content)
                ? ""
                : $"//<{CustomHeadTagName}>\r\n{content}//</{CustomHeadTagName}>\r\n\r\n";
        }

        /// <summary>
        /// Gets text to be used as a member value
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns>The text to be used as a member value. Null if the member has no value or value cannot be determined.</returns>
        public string GetMemberValueText(MemberInfo memberInfo)
        {
            if (memberInfo.DeclaringType == null) return null;

            try
            {
                object instance = memberInfo.IsStatic() ? null : ActivatorUtils.CreateInstanceAutoFillGenericParameters(memberInfo.DeclaringType);
                var valueObj = new object();
                object valueObjGuard = valueObj;

                switch (memberInfo)
                {
                    case FieldInfo fieldInfo:
                        valueObj = fieldInfo.GetValue(instance);
                        break;
                    case PropertyInfo propertyInfo:
                        valueObj = propertyInfo.GetValue(instance);
                        break;
                }

                // if valueObj hasn't been assigned in the switch
                if (valueObj == valueObjGuard) return null;

                // if valueObj's value is the default value for its type
                if (valueObj == null || valueObj.Equals(TypeUtils.GetDefaultValue(valueObj.GetType()))) return null;

                string memberType = _typeService.GetTsTypeName(memberInfo).GetTsTypeUnion(0);
                string quote = GeneratorOptions.SingleQuotes ? "'" : "\"";


                switch (valueObj)
                {
                    case Guid valueGuid when memberType == "string":
                        return quote + valueGuid + quote;
                    case DateTime valueDateTime when memberType == "Date":
                        return $@"new Date({quote}{valueDateTime.ToString(CultureInfo.InvariantCulture)}{quote})";
                    case DateTime valueDateTime when memberType == "string":
                        return quote + valueDateTime.ToString(CultureInfo.InvariantCulture) + quote;
                    case DateTimeOffset valueDateTimeOffset when memberType == "Date":
                        return $@"new Date({quote}{valueDateTimeOffset.ToString(CultureInfo.InvariantCulture)}{quote})";
                    case DateTimeOffset valueDateTimeOffset when memberType == "string":
                        return quote + valueDateTimeOffset.ToString(CultureInfo.InvariantCulture) + quote;
                    default:
                        return JsonConvert.SerializeObject(valueObj).Replace("\"", quote);
                }
            }
            catch (MissingMethodException e)
            {
                _logger?.Log($"Cannot determine the default value for member '{memberInfo.DeclaringType.FullName}.{memberInfo.Name}', because type '{memberInfo.DeclaringType.FullName}' has no default constructor.", LogLevel.Debug);
            }
            catch (ArgumentException e) when (e.InnerException is TypeLoadException)
            {
                _logger?.Log($"Cannot determine the default value for member '{memberInfo.DeclaringType.FullName}.{memberInfo.Name}', because type '{memberInfo.DeclaringType.FullName}' has generic parameters with base class or interface constraints.", LogLevel.Debug);
            }
            catch (Exception e)
            {
                _logger?.Log($"Cannot determine the default value for member '{memberInfo.DeclaringType.FullName}.{memberInfo.Name}', because an unknown exception occurred: '{e.Message}'", LogLevel.Debug);
            }

            return null;
        }
    }
}
