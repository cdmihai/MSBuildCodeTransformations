using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace MSBuildCodeTransformations
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var solutionPath = args[0];
            var msWorkspace = MSBuildWorkspace.Create();

            var solution = msWorkspace.OpenSolutionAsync(solutionPath).Result;

            AddSuperclassToTestMethods(solution);

            Console.WriteLine("--Finished. Press any key to exit--");
            Console.ReadKey();
        }

        private static void AddSuperclassToTestMethods(Solution solution)
        {
            Console.WriteLine($"Searching test classes in {solution.FilePath}");

            var testClasses = solution
                .Projects
                .AsParallel()
                .SelectMany(TestClassesFromProject)
                .ToImmutableHashSet();

            Console.WriteLine($"Found {testClasses.Count()} test classes");

            var superClassAdder = new SuperClassAdder(solution, testClasses);

            foreach (var testClass in testClasses)
            {
                superClassAdder.AddSuperclassTo(testClass);
            }
        }

        private static IEnumerable<ClassDeclarationSyntax> TestClassesFromProject(Project project)
        {
            Console.WriteLine($"Looking in {project.FilePath}");

            var testClasses =  project
                .Documents
                .AsParallel()
                .Where(d => d.SourceCodeKind == SourceCodeKind.Regular)
                .SelectMany(TestClassesFromDocument);

            if (testClasses.Any())
            {
                Console.WriteLine($"\tFound {testClasses.Count()} test classes");
            }

            return testClasses;
        }

        private static ISet<ClassDeclarationSyntax> TestClassesFromDocument(Document document)
        {
            var tree = document.GetSyntaxTreeAsync().Result;

            var finder = new XunitTestClassFinder();
            finder.Visit(tree.GetRoot());

            return finder.GetTestClasses();
        }
    }
}