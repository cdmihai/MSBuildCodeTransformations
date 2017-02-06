using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MSBuildCodeTransformations
{
    public class XunitTestClassFinder : CSharpSyntaxWalker
    {
        private static readonly string[] XunitTestAttributeNames = {"Fact", "Theory"};
        private static Regex XunitAttributeRegex;

        public ISet<MethodDeclarationSyntax> TestMethods { get; }
        public ISet<Tuple<SyntaxTrivia, ClassDeclarationSyntax>> DisabledXunitAttributeUnderClass { get; }
        public ISet<SyntaxTrivia> DisabledXunitAttributeNotUnderClass { get; }
        public ISet<ClassDeclarationSyntax> ClassesWithXunitAttributeStringInThem { get; }

        static XunitTestClassFinder()
        {
            var attributes = string.Join("|", XunitTestAttributeNames);
            XunitAttributeRegex = new Regex($"\\[\\s*({attributes})", RegexOptions.Compiled);
        }

        public XunitTestClassFinder()
        {
            TestMethods = new HashSet<MethodDeclarationSyntax>();
            DisabledXunitAttributeUnderClass = new HashSet<Tuple<SyntaxTrivia, ClassDeclarationSyntax>>();
            DisabledXunitAttributeNotUnderClass = new HashSet<SyntaxTrivia>();
            ClassesWithXunitAttributeStringInThem = new HashSet<ClassDeclarationSyntax>();
        }

        public ISet<ClassDeclarationSyntax> GetTestClasses()
        {
            var testClasses = new HashSet<ClassDeclarationSyntax>();

            foreach (var method in TestMethods)
            {
                testClasses.Add((ClassDeclarationSyntax) method.Parent);
            }

            foreach (var tuple in DisabledXunitAttributeUnderClass)
            {
                testClasses.Add(tuple.Item2);
            }

            Debug.Assert(testClasses.SetEquals(ClassesWithXunitAttributeStringInThem));

            return testClasses;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax method)
        {
            var attributes = method.AttributeLists.SelectMany(al => al.Attributes).Where(IsXunitTestAttribute);

            if (attributes.Any())
            {
                Debug.Assert(method.Parent.Kind() == SyntaxKind.ClassDeclaration);
                TestMethods.Add(method);
            }

            base.VisitMethodDeclaration(method);
        }

        public override void VisitCompilationUnit(CompilationUnitSyntax cu)
        {
            var disabledCodeWithXunitAtrributes = cu
                .DescendantTrivia()
                .Where(t => t.Kind() == SyntaxKind.DisabledTextTrivia)
                .Where(ContainsXunitAttribute);

            foreach (var trivia in disabledCodeWithXunitAtrributes)
            {
                var parent = trivia.Token.Parent.FirstAncestorOrSelf<ClassDeclarationSyntax>();

                if (parent != null)
                {
                    DisabledXunitAttributeUnderClass.Add(Tuple.Create(trivia, parent));
                }
                else
                {
                    DisabledXunitAttributeNotUnderClass.Add(trivia);
                }
            }

            base.VisitCompilationUnit(cu);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax klass)
        {
            if (XunitAttributeRegex.IsMatch(klass.ToFullString()))
            {
                ClassesWithXunitAttributeStringInThem.Add(klass);
            }

            base.VisitClassDeclaration(klass);
        }

        private static bool ContainsXunitAttribute(SyntaxTrivia trivia)
        {
            return XunitAttributeRegex.IsMatch(trivia.ToFullString());
        }

        private static bool IsXunitTestAttribute(AttributeSyntax attribute)
        {
            return XunitTestAttributeNames.Contains(attribute.Name.ToFullString());
        }
    }
}
