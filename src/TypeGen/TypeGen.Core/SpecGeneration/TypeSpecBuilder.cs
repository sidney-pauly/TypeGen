using System;

namespace TypeGen.Core.SpecGeneration
{
    /// <summary>
    /// Base class for all type spec builders
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TDerived"></typeparam>
    public abstract class TypeSpecBuilder<T, TDerived> where TDerived : TypeSpecBuilder<T, TDerived>
    {
        private readonly TypeSpec _spec;
        private string _activeMemberName;
        
        protected internal readonly T _instance;

        internal TypeSpecBuilder(TypeSpec spec)
        {
            _spec = spec;
            _instance = default;
        }

        public void AddActiveMemberAttribute(Attribute attribute) => _spec.MemberAttributes[_activeMemberName].Add(attribute);
        public void AddTypeAttribute(Attribute attribute) => _spec.AdditionalAttributes.Add(attribute);
        
        public TDerived Member(string memberName)
        {
            _activeMemberName = memberName;
            _spec.AddMember(_activeMemberName);
            
            return this as TDerived;
        }
    }
}