﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Threading;
using System.Text.RegularExpressions;

namespace NSonarQubeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExpressionComplexity : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S1067";
        internal const string Description = "Expressions should not be too complex";
        internal const string MessageFormat = "Reduce the number of conditional operators ({1}) used in the expression (maximum allowed {0}).";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.CompilationUnit); } }

        public int Maximum;

        private IImmutableSet<SyntaxKind> CompoundExpressionKinds = ImmutableHashSet.Create(new SyntaxKind[] {
            SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.ArrayInitializerExpression,
            SyntaxKind.AnonymousMethodExpression,
            SyntaxKind.ObjectInitializerExpression,
            SyntaxKind.InvocationExpression});

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            var rootExpressions =
                node
                .DescendantNodes(e2 => !(e2 is ExpressionSyntax))
                .Where(
                    e =>
                        e is ExpressionSyntax &&
                        !IsCompoundExpression(e));
            var compoundExpressionsDescendants =
                node
                .DescendantNodes()
                .Where(e => IsCompoundExpression(e))
                .SelectMany(
                    e =>
                        e
                        .DescendantNodes(
                            e2 =>
                                e == e2 ||
                                !(e2 is ExpressionSyntax))
                        .Where(
                            e2 =>
                                e2 is ExpressionSyntax &&
                                !IsCompoundExpression(e2)));

            var expressionsToCheck = rootExpressions.Concat(compoundExpressionsDescendants);

            var badExpressions =
                expressionsToCheck
                .Where(
                    e =>
                        e
                        .DescendantNodesAndSelf(
                            e2 => !IsCompoundExpression(e2))
                        .Where(
                            e2 =>
                                e2.IsKind(SyntaxKind.ConditionalExpression) ||
                                e2.IsKind(SyntaxKind.LogicalAndExpression) ||
                                e2.IsKind(SyntaxKind.LogicalOrExpression))
                        .Take(Maximum + 1)
                        .Count() == Maximum + 1);

            foreach (var badExpression in badExpressions)
            {
                addDiagnostic(Diagnostic.Create(Rule, badExpression.GetLocation(), Maximum, Maximum));
            }
        }

        private bool IsCompoundExpression(SyntaxNode node)
        {
            return CompoundExpressionKinds.Any(k => node.IsKind(k));
        }
    }
}
