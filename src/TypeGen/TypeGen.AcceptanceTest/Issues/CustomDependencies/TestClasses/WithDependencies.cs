﻿using DependencyNamespace;
using DependencyNamespace.DependencySubnamespace;
using DependencyNamespace.DependencySubnamespace.SubSubNamespace;
using System;
using System.Collections.Generic;
using System.Text;
using TypeGen.Core.TypeAnnotations;

namespace TypeGen.AcceptanceTest.Issues.CustomDependencies.TestClasses
{
    public class FileStyleUriParserDependent : FileStyleUriParser
    {
    }

    public class NamespaceDependent : NamespacedDependency
    { 
    }

    public class SubNamespaceDependent : SubNamespacedDependency
    {
    }

    public class SubSubNamespaceDependent : SubSubNamespacedDependency
    {
    }

}
