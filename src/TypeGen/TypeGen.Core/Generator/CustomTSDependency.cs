using System;
using System.Collections.Generic;
using System.Text;

namespace TypeGen.Core.Generator
{
    /// <summary>
    /// Custom dependency information in TS for a specific cshapr type or namespace
    /// </summary>
    public class CustomTSDependency
    {
        /// <summary>
        /// Indicates if the corresponding type is default exported
        /// </summary>
        public bool DefaultExport { get; set; }

        /// <summary>
        /// Indicates a flat export structure (there is one export file for
        /// all types of the package)
        /// </summary>
        public bool FlatDependencyStructure { get; set; }

        /// <summary>
        /// The root path where the type to be imported can be found. <br/>
        /// If <see cref="FlatDependencyStructure"/> is disabled, the remaining
        /// sub namespaces to the provided root path are added (E.g. <c>"import { type } from 'rootPath/subnamespace/subsubnamepsace/..."</c>)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// </summary>
        public CustomTSDependency()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="flat"></param>
        /// <param name="defaultExport"></param>
        public CustomTSDependency(string path, bool flat = false, bool defaultExport = false)
        {
            Path = path;
            FlatDependencyStructure = flat;
            DefaultExport = defaultExport;
        }

    }
}
