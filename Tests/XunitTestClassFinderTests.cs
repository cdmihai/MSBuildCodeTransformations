using System.Linq;
using MSBuildCodeTransformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Tests
{
    public class XunitTestClassFinderTests
    {
        private static readonly string TestClassTemplate =
            @"class
TestClass{
{methods}
}";

        [Theory]
        [InlineData("[Fact] public void m(){}")]
        [InlineData("[Fact(Skip=\"skipped\")] public void m(){}")]
        [InlineData("[Theory] public void m(){}")]
        [InlineData("[Theory][InlineData(\"foo\")] public void m(string s){}")]
        [InlineData(
            @"#if true
            [Fact]
            #endif
            public void m(){}")]
        public void ShouldFindTestMethodWithAttribute(string method)
        {
            var tree = ParseMethods(method);
            var finder = new XunitTestClassFinder();
            finder.Visit(tree.GetRoot());

            Assert.Equal(1, finder.TestMethods.Count);
            Assert.Equal(0, finder.DisabledXunitAttributeUnderClass.Count);
            Assert.Equal(0, finder.DisabledXunitAttributeNotUnderClass.Count);
            Assert.Equal(1, finder.ClassesWithXunitAttributeStringInThem.Count);
            Assert.Equal(1, finder.GetTestClasses().Count);
        }

        [Theory]
        [InlineData(
        @"#if false
        [Fact]
        #endif
        public void m(){}")]
        [InlineData(
        @"#if false
        [Fact(Skip=""foo"")]
        #endif
        public void m(){}")]
        [InlineData(
        @"#if false
        [Theory]
        [InlineData(""foo"")]
        #endif
        public void m(string s){}")]
        public void ShouldFindTestMethodWithDisabledAttribute(string method)
        {
            var tree = ParseMethods(method);
            var finder = new XunitTestClassFinder();
            finder.Visit(tree.GetRoot());

            Assert.Equal(1, finder.DisabledXunitAttributeUnderClass.Count);
            Assert.Equal("TestClass", finder.DisabledXunitAttributeUnderClass.First().Item2.Identifier.Text);

            Assert.Equal(0, finder.TestMethods.Count);
            Assert.Equal(0, finder.DisabledXunitAttributeNotUnderClass.Count);
            Assert.Equal(1, finder.ClassesWithXunitAttributeStringInThem.Count);
            Assert.Equal(1, finder.GetTestClasses().Count);
        }

        private SyntaxTree ParseMethods(string methods)
        {
            var code = TestClassTemplate.Replace("{methods}", methods);
            var tree = SyntaxFactory.ParseSyntaxTree(code);

            var diagnostics = tree.GetDiagnostics();
            Assert.Equal(0, diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error));

            return tree;
        }
    }
}