using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeGen.Core.Metadata;
using TypeGen.Core.TypeAnnotations;
using TypeGen.Core.Validation;

namespace TypeGen.Core.Extensions
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Checks if a type is marked with an ExportTs... attribute
        /// </summary>
        /// <param name="type"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static bool HasExportAttribute(this Type type, IMetadataReader reader)
        {
            Requires.NotNull(type, nameof(type));
            Requires.NotNull(reader, nameof(reader));

            return reader.GetAttribute<ExportTsClassAttribute>(type) != null ||
                   reader.GetAttribute<ExportTsInterfaceAttribute>(type) != null ||
                   reader.GetAttribute<ExportTsEnumAttribute>(type) != null;
        }

        /// <summary>
        /// Gets all types marked with ExportTs... attributes
        /// </summary>
        /// <param name="types"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetExportMarkedTypes(this IEnumerable<Type> types, IMetadataReader reader)
        {
            Requires.NotNull(types, nameof(types));
            Requires.NotNull(reader, nameof(reader));

            return types.Where(t => t.HasExportAttribute(reader));
        }

        /// <summary>
        /// Removes members marked with TsIgnore attribute
        /// </summary>
        /// <param name="memberInfos"></param>
        /// <param name="reader"></param>
        /// <param name="ignoredTypes"></param>
        /// <returns></returns>
        public static IEnumerable<T> WithoutTsIgnore<T>(this IEnumerable<T> memberInfos, IMetadataReader reader) where T : MemberInfo
        {
            Requires.NotNull(memberInfos, nameof(memberInfos));
            Requires.NotNull(reader, nameof(reader));

            return memberInfos
                .Where(i => reader.GetAttribute<TsIgnoreAttribute>(i) == null && i.GetCustomAttribute<TsIgnoreAttribute>() == null)
                .Where(p =>
                {
                    if (p.Name.Contains('.'))
                    {
                        var split = p.Name.Split('.');
                        var typeName = String.Join(".", split.Take(split.Length - 1));
                        var t = GetTypeFromFullName(typeName);
                        var typeIgnoreAttr = reader.GetAttribute<TsIgnoreAttribute>(t) ?? t.GetCustomAttribute<TsIgnoreAttribute>();
                        return typeIgnoreAttr == null;
                    }
                    else if (p is PropertyInfo propInfo && !p.DeclaringType.IsInterface)
                    {
                        var getMethod = propInfo.GetGetMethod();
                        foreach (var @interface in p.DeclaringType.GetInterfaces())
                        {
                            var ignoreAttr = reader.GetAttribute<TsIgnoreAttribute>(@interface) ?? @interface.GetCustomAttribute<TsIgnoreAttribute>();
                            if (ignoreAttr == null || !ignoreAttr.IgnoreImplicitlyImplementedProperties)
                                continue;

                            var map = p.DeclaringType.GetInterfaceMap(@interface);
                            for (int i = 0; i < map.InterfaceMethods.Length; i++)
                            {
                                if (map.TargetMethods[i] == getMethod)
                                    return false;
                            }
                        }
                    }
                    return true;
                });
        }

        /// <summary>
        /// Tries to get a type with the specified name from all any of the loaded assemblies
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type GetTypeFromFullName(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(name);
                if (type != null)
                    return type;
            }
            throw new Exception("Type not found");
        }

        /// <summary>
        /// Filters members for TypeScript export
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfos"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> WithMembersFilter(this IEnumerable<FieldInfo> memberInfos)
        {
            Requires.NotNull(memberInfos, nameof(memberInfos));
            return memberInfos.Where(i => i.IsPublic);
        }

        /// <summary>
        /// Filters members for TypeScript export
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfos"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> WithMembersFilter(this IEnumerable<PropertyInfo> memberInfos)
        {
            Requires.NotNull(memberInfos, nameof(memberInfos));
            return memberInfos.Where(i => i.GetMethod.IsPublic);
        }

        /// <summary>
        /// Checks if a property or field is static
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static bool IsStatic(this MemberInfo memberInfo)
        {
            Requires.NotNull(memberInfo, nameof(memberInfo));

            if (memberInfo is FieldInfo fieldInfo) return fieldInfo.IsStatic;
            if (memberInfo is PropertyInfo propertyInfo) return propertyInfo.GetMethod.IsStatic;

            return false;
        }

        /// <summary>
        /// Maps an enumerable to an enumerable of the elements' type names
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTypeNames(this IEnumerable<object> enumerable)
        {
            Requires.NotNull(enumerable, nameof(enumerable));

            return enumerable
                .Select(c => c.GetType().Name);
        }

        /// <summary>
        /// Shim for Type.GetInterface
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceName"></param>
        /// <returns></returns>
        public static Type GetInterface(this Type type, string interfaceName)
        {
            Requires.NotNull(type, nameof(type));
            Requires.NotNullOrEmpty(interfaceName, nameof(interfaceName));

            return type.GetInterfaces()
                .FirstOrDefault(i => i.Name == interfaceName || i.FullName == interfaceName);
        }

        /// <summary>
        /// Gets MemberInfos of all members in a type that can be exported to TypeScript.
        /// Members marked with TsIgnore attribute are not included in the result.
        /// If the passed type is not a class type, empty enumeration is returned.
        /// </summary>
        /// <param name="type">Class type</param>
        /// <param name="metadataReader"></param>
        /// <param name="withoutTsIgnore"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetTsExportableMembers(this Type type, IMetadataReader metadataReader, bool includeExplicitProperties, bool withoutTsIgnore = true)
        {
            Requires.NotNull(type, nameof(type));
            TypeInfo typeInfo = type.GetTypeInfo();

            if (!typeInfo.IsClass && !typeInfo.IsInterface) return Enumerable.Empty<MemberInfo>();

            var fieldInfos = (IEnumerable<MemberInfo>)typeInfo.DeclaredFields
                .WithMembersFilter();

            IEnumerable<MemberInfo> propertyInfos;

            if (!includeExplicitProperties)
                propertyInfos = typeInfo.DeclaredProperties.WithMembersFilter();
            else
                propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);


            if (withoutTsIgnore)
            {
                fieldInfos = fieldInfos.WithoutTsIgnore(metadataReader);
                propertyInfos = propertyInfos.WithoutTsIgnore(metadataReader);
            }

            return fieldInfos.Union(propertyInfos);
        }
    }
}
