using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MSBuildCodeTransformations
{
    internal class SuperClassAdder
    {
        private Solution _solution;
        private readonly ISet<ClassDeclarationSyntax> _testClasses;

        public SuperClassAdder(Solution solution, ISet<ClassDeclarationSyntax> testClasses)
        {
            this._solution = solution;
            _testClasses = testClasses;

            CheckPreconditions();
        }

        public void AddSuperclassTo(ClassDeclarationSyntax testClass)
        {
        }

        private static HashSet<string> TypeBlackList = new HashSet<string>()
        {
            "IDisposable"
        };

        private void CheckPreconditions()
        {
            var testClassNames = _testClasses
                .Select(c => c.Identifier.Text)
                .Where(c => !TypeBlackList.Contains(c))
                .ToImmutableHashSet();

            foreach (var testClass in _testClasses)
            {
                if (testClass.BaseList != null && !SuperClassIsATestClass(testClass, testClassNames))
                {
                    Debugger.Launch();
                    Debugger.Break();
                }
            }
        }

        private static bool SuperClassIsATestClass(ClassDeclarationSyntax testClass, ImmutableHashSet<string> testClassNames)
        {
            var baseListNames = testClass.BaseList.Types.Select(t => t.ToString()).ToArray();

            Console.WriteLine($"{testClass.Identifier.Text} : {string.Join(", ", baseListNames)}");

            return baseListNames.All(testClassNames.Contains);
        }
    }
}