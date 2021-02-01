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
        /// <param name="overridenDeclaringType"></param>
        /// <returns></returns>
        public static IEnumerable<T> WithoutTsIgnore<T>(this IEnumerable<T> memberInfos, IMetadataReader reader, HashSet<Type> neverImplementedProperyTypes) where T : MemberInfo
        {
            Requires.NotNull(memberInfos, nameof(memberInfos));
            Requires.NotNull(reader, nameof(reader));

            return memberInfos
                .Where(i => reader.GetAttribute<TsIgnoreAttribute>(i) == null && i.GetCustomAttribute<TsIgnoreAttribute>(false) == null)
                .Where(p =>
                {
                    if (p is PropertyInfo pi && neverImplementedProperyTypes.Contains(pi.PropertyType))
                        return false;

                    var declaringType = p.DeclaringType;
                    if (p.Name.Contains('.'))
                    {
                        var split = p.Name.Split('.');
                        var typeName = String.Join(".", split.Take(split.Length - 1));
                        var t = GetTypeFromFullName(typeName);
                        var typeIgnoreAttr = reader.GetAttribute<TsIgnoreAttribute>(t) ?? t.GetCustomAttribute<TsIgnoreAttribute>(false);
                        if (typeIgnoreAttr != null)
                            return false;

                        var interfaceProp = t.GetProperty(split.Last(), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                        if ((reader.GetAttribute<TsIgnoreAttribute>(interfaceProp) ?? interfaceProp.GetCustomAttribute<TsIgnoreAttribute>(false)) != null)
                            return false;

                        return true;
                    }
                    else if (p is PropertyInfo propInfo && !declaringType.IsInterface)
                    {
                        var getMethod = propInfo.GetGetMethod();
                        foreach (var @interface in declaringType.GetInterfaces())
                        {
                            var ignoreAttr = reader.GetAttribute<TsIgnoreAttribute>(@interface) ?? @interface.GetCustomAttribute<TsIgnoreAttribute>(false);
                            bool ignoreAll = !(ignoreAttr == null || !ignoreAttr.IgnoreImplicitlyImplementedProperties);

                            Dictionary<MethodInfo, PropertyInfo> getMethodPropMapping = ignoreAll
                            ?
                            new Dictionary<MethodInfo, PropertyInfo>()
                            :
                            @interface
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Static)
                            .ToDictionary(pr => pr.GetMethod);

                            var map = declaringType.GetInterfaceMap(@interface);
                            for (int i = 0; i < map.InterfaceMethods.Length; i++)
                            {
                                if (ignoreAll)
                                {
                                    if (map.TargetMethods[i] == getMethod)
                                        return false;
                                }
                                else
                                {
                                    if (getMethodPropMapping.ContainsKey(map.InterfaceMethods[i]))
                                    {
                                        var prop = getMethodPropMapping[map.InterfaceMethods[i]];
                                        if ((reader.GetAttribute<TsIgnoreAttribute>(prop) ?? prop.GetCustomAttribute<TsIgnoreAttribute>(false)) != null)
                                            return false;
                                    }
                                }

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
            if (name.Contains("<"))
            {
                FIX FOR NESTED
                var split = name.Split('<');
                name = split[0] + "`" + ((split[1].Split(',').Count()));
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(name);
                if (type != null)
                    return type;
            }
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(name, false, true);
                if (type != null)
                    return type;
            }
            throw new Exception("Type " + name + " not found");
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
        /// <param name="includeExplicitProperties"></param>
        /// <param name="neverImplementedPropertyTypes"></param>
        /// <param name="withoutTsIgnore"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetTsExportableMembers(this Type type,
            IMetadataReader metadataReader,
            bool includeExplicitProperties,
            HashSet<Type> neverImplementedPropertyTypes,
            bool withoutTsIgnore = true)
        {
            Requires.NotNull(type, nameof(type));
            TypeInfo typeInfo = type.GetTypeInfo();

            if (!typeInfo.IsClass && !typeInfo.IsInterface) return Enumerable.Empty<MemberInfo>();

            var fieldInfos = (IEnumerable<MemberInfo>)typeInfo.DeclaredFields
                .WithMembersFilter();

            IEnumerable<MemberInfo> interfaceMembers = new List<MemberInfo>();
            if (type.IsClass)
                foreach (var i in type.GetInterfaces())
                    if (!withoutTsIgnore || ((metadataReader.GetAttribute<TsIgnoreAttribute>(i) ?? i.GetCustomAttribute<TsIgnoreAttribute>(false)) == null))
                        ((List<MemberInfo>)interfaceMembers).AddRange(
                            i.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                            .Select(p => new CustomPropertyInfo(type, p.Name.Contains(".") ? p.Name : (i.Namespace + "." + i.Name + "." + p.Name), p))
                            );

            IEnumerable<MemberInfo> propertyInfos;

            if (!includeExplicitProperties)
                propertyInfos = typeInfo.DeclaredProperties.WithMembersFilter();
            else
                propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);


            if (withoutTsIgnore)
            {
                fieldInfos = fieldInfos.WithoutTsIgnore(metadataReader, neverImplementedPropertyTypes);
                propertyInfos = propertyInfos.WithoutTsIgnore(metadataReader, neverImplementedPropertyTypes);
                interfaceMembers = interfaceMembers.WithoutTsIgnore(metadataReader, neverImplementedPropertyTypes);
            }


            return fieldInfos
                .Concat(propertyInfos)
                .Concat(interfaceMembers)
                .Distinct(new MemberUniquenessEqualitiyComparer());
        }

        private class MemberUniquenessEqualitiyComparer : IEqualityComparer<MemberInfo>
        {
            public bool Equals(MemberInfo x, MemberInfo y)
            {
                if (x == null && y == null)
                    return true;
                if ((x == null) != (y == null))
                    return false;

                return x.Name == y.Name;

            }

            public int GetHashCode(MemberInfo obj)
            {
                return obj?.Name.GetHashCode() ?? 0;
            }
        }
    }
}
