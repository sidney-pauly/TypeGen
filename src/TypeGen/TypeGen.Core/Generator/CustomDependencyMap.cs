using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TypeGen.Core.Extensions;

namespace TypeGen.Core.Generator
{
    public class CustomDependencyMap : Dictionary<string, CustomTSDependency>
    {
        /// <summary>
        /// Add a entry to the map 
        /// </summary>
        /// <param name="csharpNamespace"></param>
        /// <param name="nodeImport"></param>
        /// <returns></returns>
        public new void Add(string csharpNamespace, CustomTSDependency nodeImport)
        {
            base.Add(csharpNamespace, nodeImport);
        }

        /// <summary>
        /// Adds a alternative import path for the provided assebly
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="nodeImport"></param>
        public void Add(Assembly assembly, CustomTSDependency nodeImport)
        {
            Add(assembly.FullName, nodeImport);
        }

        /// <summary>
        /// Adds a alternativ import path for the provided type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="nodeImport"></param>
        public void Add(Type type, CustomTSDependency nodeImport)
        {
            Add(type.GetOrCreateFullName(), nodeImport);
        }


    }
}
