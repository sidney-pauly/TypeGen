using System;
using System.Collections.Generic;
using System.Text;
using TypeGen.Core.SpecGeneration;

namespace TypeGen.AcceptanceTest.GeneratorTestingUtils
{
    class MinimalTypesSpec : GenerationSpec
    {

        Type MainType { get; set; }
        Type[] OtherTypes { get; set; }

        public MinimalTypesSpec(Type mainType, Type[] otherTypes)
        {
            MainType = mainType;
            OtherTypes = otherTypes;
        }
        public override void OnBeforeGeneration(OnBeforeGenerationArgs args)
        {
            if (MainType.IsInterface)
                AddInterface(MainType);
            else
                AddClass(MainType);

            foreach (var type in OtherTypes)
                if (type.IsInterface)
                    AddInterface(type);
                else
                    AddClass(type);

        }
    }
}
