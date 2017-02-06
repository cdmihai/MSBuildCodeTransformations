using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace MarkAllMembersDeprecated
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var solutionPath = args[0];
            var msWorkspace = MSBuildWorkspace.Create();

            var solution = msWorkspace.OpenSolutionAsync(solutionPath).Result;

            MarkAllMembersDeprecated(solution);

            Console.WriteLine("--Finished. Press any key to exit--");
            Console.ReadKey();
        }

        private static void MarkAllMembersDeprecated(Solution solution)
        {
            var deprecatedProjectNames = new HashSet<string>()
            {
                "OrcasEngine",
                "XMakeConversion"
            };

            var obsoleteRewriters = solution.Projects
                .Where(p => deprecatedProjectNames.Contains(p.Name))
                .SelectMany(p => p.Documents)
                .Where(d => d.SourceCodeKind.Equals(SourceCodeKind.Regular))
                .AsParallel()
                .Select(ObsoleteRewriter);

            foreach (var obsoleteRewriter in obsoleteRewriters)
            {
                obsoleteRewriter.Run();
            }
        }

        private static ObsoleteRewriter ObsoleteRewriter(Document document)
        {
            var tree = document.GetSyntaxRootAsync().Result;
            var semanticModel = document.GetSemanticModelAsync().Result;

            var memberseNeedingDeprecation = tree.DescendantNodesAndSelf()
                .OfType<MemberDeclarationSyntax>()
                .Where(n => !n.Kind().Equals(SyntaxKind.NamespaceDeclaration))
                .Where(n => NeedsDeprecation(n, semanticModel))
                .ToImmutableHashSet();

            return memberseNeedingDeprecation.Any() ? new ObsoleteRewriter(document, tree, semanticModel, memberseNeedingDeprecation) : null;
        }

        private static bool NeedsDeprecation(MemberDeclarationSyntax node, SemanticModel semanticModel)
        {
            var symbolInfo1 = semanticModel.GetSymbolInfo(node, CancellationToken.None).Symbol;
            var symbolInfo2 = semanticModel.GetEnclosingSymbol(node.SpanStart);

            var symbolInfo = GetSymbolInfo(node, semanticModel);

            if (symbolInfo == null)
            {
                return false;
            }

            var needsDeprecation = IsPublic(symbolInfo) &&
                                   EnclosingTypesArePublic(symbolInfo) &&
                                   NotDeprecated(symbolInfo);

            Debug.Assert(!needsDeprecation || 
                !(node.GetText().ToString().Contains("private") &&
                node.GetText().ToString().Contains("internal")));

            return needsDeprecation;
        }

        private static ISymbol GetSymbolInfo(MemberDeclarationSyntax node, SemanticModel semanticModel)
        {
            var fieldDeclaration = node as FieldDeclarationSyntax;
            if (fieldDeclaration == null)
            {
                return semanticModel.GetDeclaredSymbol(node, CancellationToken.None);
            }

            if (fieldDeclaration.Declaration.Variables.Count != 1)
            {
                foreach (var variable in fieldDeclaration.Declaration.Variables)
                {
                    var variableSymbol = semanticModel.GetDeclaredSymbol(variable, CancellationToken.None);
                    Debug.Assert(!IsPublic(variableSymbol));
                }
                return null;
            }

            return semanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables.First(), CancellationToken.None);
        }

        private static bool IsPublic(ISymbol symbol)
        {
            return symbol.DeclaredAccessibility.Equals(Accessibility.Public);
        }

        private static bool EnclosingTypesArePublic(ISymbol symbol)
        {
            var enclosingType = symbol.ContainingType;

            while (enclosingType != null)
            {
                if (!IsPublic(enclosingType))
                {
                    return false;
                }

                enclosingType = enclosingType.ContainingType;
            }

            return true;
        }

        private static bool NotDeprecated(ISymbol symbolInfo)
        {
            return
                !symbolInfo.GetAttributes()
                    .All(a => a.AttributeClass.Name.Equals(typeof(ObsoleteAttribute).Name, StringComparison.Ordinal));
        }
    }
}