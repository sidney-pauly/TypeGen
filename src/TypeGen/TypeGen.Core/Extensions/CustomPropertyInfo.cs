using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace TypeGen.Core.Extensions
{
    public class CustomPropertyInfo : PropertyInfo
    {
        private PropertyInfo OriginalProperty { get; set; }

        private Type _DeclaringType { get; set; }

        /// <inheritdoc/>
        public override Type DeclaringType => _DeclaringType ?? OriginalProperty.DeclaringType;

        private MemberTypes? _MemberTypes { get; set; }

        /// <inheritdoc/>
        public override MemberTypes MemberType => _MemberTypes == null ? OriginalProperty.MemberType : _MemberTypes.Value;

        private string _Name { get; set; }

        /// <inheritdoc/>
        public override string Name => _Name ?? OriginalProperty.Name;

        private Type _ReflectedType { get; set; }

        /// <inheritdoc/>
        public override Type ReflectedType => _ReflectedType ?? OriginalProperty.ReflectedType;

        private PropertyAttributes _PropertyAttributes { get; set; }

        /// <inheritdoc/>
        public override PropertyAttributes Attributes => _PropertyAttributes;

        /// <inheritdoc/>
        public override bool CanRead => OriginalProperty?.CanRead ?? false;

        /// <inheritdoc/>
        public override bool CanWrite => OriginalProperty?.CanWrite ?? false;

        private Type _PropertyType { get; set; }

        /// <inheritdoc/>
        public override Type PropertyType => _PropertyType ?? OriginalProperty.PropertyType;

        /// <inheritdoc/>
        public override MethodInfo GetMethod => OriginalProperty?.GetMethod;

        /// <inheritdoc/>
        public override MethodInfo SetMethod => OriginalProperty?.SetMethod;

        /// <inheritdoc/>
        public override object[] GetCustomAttributes(bool inherit)
        {
            return new Attribute[] { };
        }

        /// <inheritdoc/>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new Attribute[] { };
        }

        /// <inheritdoc/>
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        /// <inheritdoc/>
        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return new MethodInfo[] { };
        }

        /// <inheritdoc/>
        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return OriginalProperty.GetGetMethod();
        }

        /// <inheritdoc/>
        public override ParameterInfo[] GetIndexParameters()
        {
            return OriginalProperty.GetIndexParameters();
        }

        /// <inheritdoc/>
        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return OriginalProperty.GetSetMethod();
        }

        /// <inheritdoc/>
        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            return OriginalProperty.GetValue(obj, invokeAttr, binder, index, culture);
        }

        /// <inheritdoc/>
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            OriginalProperty.SetValue(obj, value, invokeAttr, binder, index, culture);
        }

        /// <summary>
        /// </summary>
        public CustomPropertyInfo(Type overridenDeclaringType, string overridenName, PropertyInfo originalProperty)
        {
            OriginalProperty = originalProperty;
            _Name = overridenName;
            _DeclaringType = overridenDeclaringType;
        }
    }
}
