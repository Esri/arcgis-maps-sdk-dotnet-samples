// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ArcGIS.Samples.CatalogGenerator
{
    [Generator(LanguageNames.CSharp)]
    public sealed class SampleCatalogGenerator : IIncrementalGenerator
    {
        private const string SampleAttributeName = "ArcGIS.Samples.Shared.Attributes.SampleAttribute";
        private const string OfflineDataAttributeName = "ArcGIS.Samples.Shared.Attributes.OfflineDataAttribute";
        private const string XamlFilesAttributeName = "ArcGIS.Samples.Shared.Attributes.XamlFilesAttribute";
        private const string AndroidLayoutAttributeName = "ArcGIS.Samples.Shared.Attributes.AndroidLayoutAttribute";
        private const string ClassFileAttributeName = "ArcGIS.Samples.Shared.Attributes.ClassFileAttribute";
        private const string EmbeddedResourceAttributeName = "ArcGIS.Samples.Shared.Attributes.EmbeddedResourceAttribute";

        private static readonly DiagnosticDescriptor DuplicateFormalNameDiagnostic = new DiagnosticDescriptor(
            id: "AGSAMPLECAT001",
            title: "Duplicate sample formal name",
            messageFormat: "Multiple sample classes use the formal name '{0}'. Sample formal names must be unique.",
            category: "SampleCatalog",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MalformedSampleAttributeDiagnostic = new DiagnosticDescriptor(
            id: "AGSAMPLECAT002",
            title: "Malformed sample attribute",
            messageFormat: "Sample class '{0}' has a malformed SampleAttribute and cannot be added to the generated sample catalog",
            category: "SampleCatalog",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<SampleMetadata> samples = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (node, _) => node is ClassDeclarationSyntax classDeclaration && classDeclaration.AttributeLists.Count > 0,
                    static (context, _) => GetSampleMetadata(context))
                .Where(static sample => sample != null)!;

            context.RegisterSourceOutput(samples.Collect(), static (context, samples) => Execute(context, samples));
        }

        private static SampleMetadata? GetSampleMetadata(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol || classSymbol.IsAbstract)
            {
                return null;
            }

            ImmutableArray<AttributeData> attributes = classSymbol.GetAttributes();
            AttributeData? sampleAttribute = GetAttribute(attributes, SampleAttributeName);
            if (sampleAttribute == null)
            {
                return null;
            }

            if (sampleAttribute.ConstructorArguments.Length < 4)
            {
                return SampleMetadata.Malformed(classSymbol.Name, classDeclaration.Identifier.GetLocation());
            }

            return new SampleMetadata(
                formalName: classSymbol.Name,
                sampleName: GetString(sampleAttribute.ConstructorArguments[0]),
                category: GetString(sampleAttribute.ConstructorArguments[1]),
                description: GetString(sampleAttribute.ConstructorArguments[2]),
                instructions: GetString(sampleAttribute.ConstructorArguments[3]),
                tags: sampleAttribute.ConstructorArguments.Length > 4 ? GetStringArray(sampleAttribute.ConstructorArguments[4]) : Array.Empty<string>(),
                offlineDataItems: GetAttributeStringArray(attributes, OfflineDataAttributeName),
                xamlLayouts: GetAttributeStringArray(attributes, XamlFilesAttributeName),
                androidLayouts: GetAttributeStringArray(attributes, AndroidLayoutAttributeName),
                classFiles: GetAttributeStringArray(attributes, ClassFileAttributeName),
                embeddedResources: GetAttributeStringArray(attributes, EmbeddedResourceAttributeName),
                typeName: classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }

        private static AttributeData? GetAttribute(ImmutableArray<AttributeData> attributes, string metadataName)
        {
            foreach (AttributeData attribute in attributes)
            {
                if (attribute.AttributeClass?.ToDisplayString() == metadataName)
                {
                    return attribute;
                }
            }

            return null;
        }

        private static string[]? GetAttributeStringArray(ImmutableArray<AttributeData> attributes, string metadataName)
        {
            AttributeData? attribute = GetAttribute(attributes, metadataName);
            if (attribute == null || attribute.ConstructorArguments.Length == 0)
            {
                return null;
            }

            return GetStringArray(attribute.ConstructorArguments[0]);
        }

        private static string GetString(TypedConstant constant)
        {
            return constant.Value as string ?? string.Empty;
        }

        private static string[] GetStringArray(TypedConstant constant)
        {
            if (constant.Kind == TypedConstantKind.Array)
            {
                return constant.Values
                    .Select(static value => value.Value as string)
                    .Where(static value => value != null)
                    .Select(static value => value!)
                    .ToArray();
            }

            return constant.Value is string value ? new[] { value } : Array.Empty<string>();
        }

        private static void Execute(SourceProductionContext context, ImmutableArray<SampleMetadata> samples)
        {
            if (samples.IsDefaultOrEmpty)
            {
                return;
            }

            foreach (SampleMetadata malformedSample in samples.Where(static sample => sample.IsMalformed))
            {
                context.ReportDiagnostic(Diagnostic.Create(MalformedSampleAttributeDiagnostic, malformedSample.Location, malformedSample.FormalName));
            }

            List<SampleMetadata> validSamples = samples.Where(static sample => !sample.IsMalformed).ToList();
            if (validSamples.Count == 0)
            {
                return;
            }

            List<SampleMetadata> orderedSamples = validSamples
                .GroupBy(static sample => sample.FormalName, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    if (group.Count() > 1)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DuplicateFormalNameDiagnostic, Location.None, group.Key));
                    }

                    return group.First();
                })
                .OrderBy(static sample => sample.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static sample => sample.SampleName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var categories = orderedSamples
                .Select(static sample => sample.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static category => category, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var source = new StringBuilder();
            source.AppendLine("// <auto-generated />");
            source.AppendLine("#nullable enable");
            source.AppendLine("namespace ArcGIS.Samples.Managers");
            source.AppendLine("{");
            source.AppendLine("    public sealed class GeneratedSampleCatalog : global::ArcGIS.Samples.Managers.ISampleCatalogProvider");
            source.AppendLine("    {");
            source.AppendLine($"        private static readonly string[] Categories = {ArrayLiteral(categories)};");
            source.AppendLine($"        private static readonly string[] SampleNames = {ArrayLiteral(orderedSamples.Select(static sample => sample.FormalName).ToList())};");
            source.AppendLine();
            source.AppendLine("        public global::System.Collections.Generic.IReadOnlyList<string> GetCategories() => Categories;");
            source.AppendLine();
            source.AppendLine("        public global::System.Collections.Generic.IReadOnlyList<string> GetSampleNames() => SampleNames;");
            source.AppendLine();
            AppendCategoryLookup(source, orderedSamples, categories);
            source.AppendLine();
            AppendSampleInfoFactory(source, orderedSamples);
            source.AppendLine();
            AppendSampleFactory(source, orderedSamples);
            source.AppendLine("    }");
            source.AppendLine();
            source.AppendLine("    public partial class SampleManager");
            source.AppendLine("    {");
            source.AppendLine("        static partial void TryCreateGeneratedCatalogProvider(ref global::ArcGIS.Samples.Managers.ISampleCatalogProvider provider)");
            source.AppendLine("        {");
            source.AppendLine("            provider = new global::ArcGIS.Samples.Managers.GeneratedSampleCatalog();");
            source.AppendLine("        }");
            source.AppendLine("    }");
            source.AppendLine("}");

            context.AddSource("GeneratedSampleCatalog.g.cs", source.ToString());
        }

        private static void AppendCategoryLookup(StringBuilder source, List<SampleMetadata> samples, List<string> categories)
        {
            source.AppendLine("        public global::System.Collections.Generic.IReadOnlyList<string> GetSampleNamesForCategory(string category)");
            source.AppendLine("        {");
            source.AppendLine("            switch (category)");
            source.AppendLine("            {");

            foreach (string category in categories)
            {
                List<string> sampleNames = samples
                    .Where(sample => string.Equals(sample.Category, category, StringComparison.OrdinalIgnoreCase))
                    .Select(static sample => sample.FormalName)
                    .ToList();

                source.AppendLine($"                case {Literal(category)}:");
                source.AppendLine($"                    return {ArrayLiteral(sampleNames)};");
            }

            source.AppendLine("                default:");
            source.AppendLine("                    return global::System.Array.Empty<string>();");
            source.AppendLine("            }");
            source.AppendLine("        }");
        }

        private static void AppendSampleInfoFactory(StringBuilder source, List<SampleMetadata> samples)
        {
            source.AppendLine("        public global::ArcGIS.Samples.Shared.Models.SampleInfo CreateSampleInfo(string formalName)");
            source.AppendLine("        {");
            source.AppendLine("            switch (formalName)");
            source.AppendLine("            {");

            foreach (SampleMetadata sample in samples)
            {
                source.AppendLine($"                case {Literal(sample.FormalName)}:");
                source.AppendLine("                    return new global::ArcGIS.Samples.Shared.Models.SampleInfo(");
                source.AppendLine($"                        formalName: {Literal(sample.FormalName)},");
                source.AppendLine($"                        sampleName: {Literal(sample.SampleName)},");
                source.AppendLine($"                        category: {Literal(sample.Category)},");
                source.AppendLine($"                        description: {Literal(sample.Description)},");
                source.AppendLine($"                        instructions: {Literal(sample.Instructions)},");
                source.AppendLine($"                        tags: {ArrayLiteral(sample.Tags)},");
                source.AppendLine($"                        offlineDataItems: {ArrayLiteral(sample.OfflineDataItems)},");
                source.AppendLine($"                        androidLayouts: {ArrayLiteral(sample.AndroidLayouts)},");
                source.AppendLine($"                        xamlLayouts: {ArrayLiteral(sample.XamlLayouts)},");
                source.AppendLine($"                        classFiles: {ArrayLiteral(sample.ClassFiles)},");
                source.AppendLine($"                        embeddedResources: {ArrayLiteral(sample.EmbeddedResources)},");
                source.AppendLine($"                        sampleType: typeof({sample.TypeName}));");
            }

            source.AppendLine("                default:");
            source.AppendLine("                    throw new global::System.ArgumentException($\"Unknown sample '{formalName}'.\", nameof(formalName));");
            source.AppendLine("            }");
            source.AppendLine("        }");
        }

        private static void AppendSampleFactory(StringBuilder source, List<SampleMetadata> samples)
        {
            source.AppendLine("        public object CreateSample(string formalName)");
            source.AppendLine("        {");
            source.AppendLine("            switch (formalName)");
            source.AppendLine("            {");

            foreach (SampleMetadata sample in samples)
            {
                source.AppendLine($"                case {Literal(sample.FormalName)}:");
                source.AppendLine($"                    return new {sample.TypeName}();");
            }

            source.AppendLine("                default:");
            source.AppendLine("                    throw new global::System.ArgumentException($\"Unknown sample '{formalName}'.\", nameof(formalName));");
            source.AppendLine("            }");
            source.AppendLine("        }");
        }

        private static string ArrayLiteral(IReadOnlyList<string>? values)
        {
            if (values == null)
            {
                return "null";
            }

            if (values.Count == 0)
            {
                return "global::System.Array.Empty<string>()";
            }

            return "new string[] { " + string.Join(", ", values.Select(Literal)) + " }";
        }

        private static string Literal(string value)
        {
            return SymbolDisplay.FormatLiteral(value ?? string.Empty, quote: true);
        }

        private sealed class SampleMetadata
        {
            public SampleMetadata(
                string formalName,
                string sampleName,
                string category,
                string description,
                string instructions,
                string[] tags,
                string[]? offlineDataItems,
                string[]? xamlLayouts,
                string[]? androidLayouts,
                string[]? classFiles,
                string[]? embeddedResources,
                string typeName)
            {
                FormalName = formalName;
                SampleName = sampleName;
                Category = category;
                Description = description;
                Instructions = instructions;
                Tags = tags;
                OfflineDataItems = offlineDataItems;
                XamlLayouts = xamlLayouts;
                AndroidLayouts = androidLayouts;
                ClassFiles = classFiles;
                EmbeddedResources = embeddedResources;
                TypeName = typeName;
            }

            private SampleMetadata(string formalName, Location location)
            {
                FormalName = formalName;
                SampleName = string.Empty;
                Category = string.Empty;
                Description = string.Empty;
                Instructions = string.Empty;
                Tags = Array.Empty<string>();
                TypeName = string.Empty;
                IsMalformed = true;
                Location = location;
            }

            public static SampleMetadata Malformed(string formalName, Location location)
            {
                return new SampleMetadata(formalName, location);
            }

            public string FormalName { get; }
            public string SampleName { get; }
            public string Category { get; }
            public string Description { get; }
            public string Instructions { get; }
            public string[] Tags { get; }
            public string[]? OfflineDataItems { get; }
            public string[]? XamlLayouts { get; }
            public string[]? AndroidLayouts { get; }
            public string[]? ClassFiles { get; }
            public string[]? EmbeddedResources { get; }
            public string TypeName { get; }
            public bool IsMalformed { get; }
            public Location? Location { get; }
        }
    }
}
