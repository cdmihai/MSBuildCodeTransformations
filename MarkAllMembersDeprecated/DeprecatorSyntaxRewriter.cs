using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace MarkAllMembersDeprecated
{
    public class DeprecatorSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _model;

        public DeprecatorSyntaxRewriter(SemanticModel model)
        {
            _model = model;
        }
        public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitGlobalStatement(GlobalStatementSyntax node)
        {
            Debugger.Launch();
            Debugger.Break();

            return Deprecate(node);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            Debugger.Launch();
            Debugger.Break();

            return Deprecate(node);
        }

        public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
        {
            Debugger.Launch();
            Debugger.Break();

            return Deprecate(node);
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            return Deprecate(node);
        }

        public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
        {
            Debugger.Launch();
            Debugger.Break();

            return Deprecate(node);
        }

        public override SyntaxNode VisitIncompleteMember(IncompleteMemberSyntax node)
        {
            Debugger.Launch();
            Debugger.Break();

            return Deprecate(node);
        }

        private SyntaxNode Deprecate(MemberDeclarationSyntax node)
        {
            var symbolInfo =_model.GetDeclaredSymbol(node, CancellationToken.None);

            if (EnclosingTypesArePublic(symbolInfo) &&
                symbolInfo.DeclaredAccessibility.Equals(Accessibility.Public) &&
                NotDeprecated(symbolInfo))

            {
                return DeprecateNode(node, symbolInfo);
            }
            return node;
        }

        private static SyntaxNode DeprecateNode(MemberDeclarationSyntax node, ISymbol symbolInfo)
        {
            return null;
        }

        private static bool NotDeprecated(ISymbol symbolInfo)
        {
            return
                !symbolInfo.GetAttributes()
                    .All(a => a.AttributeClass.Name.Equals(typeof(ObsoleteAttribute).Name, StringComparison.Ordinal));
        }

        private static bool EnclosingTypesArePublic(ISymbol symbolInfo)
        {
            return false;
        }
    }
}