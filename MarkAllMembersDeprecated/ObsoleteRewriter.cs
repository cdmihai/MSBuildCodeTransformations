using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MarkAllMembersDeprecated
{
    internal class ObsoleteRewriter : CSharpSyntaxRewriter
    {
        private readonly Document _document;
        private readonly IEnumerable<MemberDeclarationSyntax> _membersNeedingDeprecation;
        private readonly SemanticModel _semanticModel;
        private readonly SyntaxNode _tree;

        public ObsoleteRewriter(
            Document document,
            SyntaxNode tree,
            SemanticModel semanticModel,
            ICollection<MemberDeclarationSyntax> membersNeedingDeprecation)
        {
            _document = document;
            _tree = tree;
            _semanticModel = semanticModel;
            _membersNeedingDeprecation = membersNeedingDeprecation;
        }

        public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitDelegateDeclaration(node);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitGlobalStatement(GlobalStatementSyntax node)
        {
            Debugger.Break();

            return base.VisitGlobalStatement(node);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitStructDeclaration(node);
        }

        public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitInterfaceDeclaration(node);
        }

        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitEnumDeclaration(node);
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            Debugger.Break();
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitEventFieldDeclaration(node);
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitMethodDeclaration(node);
        }

        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            Debugger.Break();
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitOperatorDeclaration(node);
        }

        public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
        {
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitConversionOperatorDeclaration(node);
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitConstructorDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
        {
            Debugger.Break();
            if (NeedsDeprecation(node))
                return node.AddAttributeLists(CreateObsoleteNode());

            return base.VisitEventDeclaration(node);
        }

        private bool NeedsDeprecation(SyntaxNode node)
        {
            return _membersNeedingDeprecation.Contains(node);
        }

        private static AttributeListSyntax CreateObsoleteNode()
        {
            return AttributeList(
                SingletonSeparatedList(
                    Attribute(
                            IdentifierName("Obsolete")
                        )
                        .WithArgumentList(
                            AttributeArgumentList(
                                SingletonSeparatedList(
                                    AttributeArgument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal("foo")
                                        )
                                    )
                                )
                            )
                        )
                )
            );
        }

        public void Run()
        {
            var tree = Visit(_tree);

            File.WriteAllText(_document.FilePath, tree.ToFullString());
        }
    }
}