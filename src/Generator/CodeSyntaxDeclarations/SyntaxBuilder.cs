using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ConfigManager.Generator.CodeSyntaxDeclarations;

internal static class SyntaxBuilder
{
    internal static string GenerateSourceCode(/*TODO SOME TYPE HERE? -> LIST/PARAMS OF CLASSES?*/)
    {
        return "";
    }

    internal static AttributeSyntax BuildGeneratorAttribute(string generatorName, string generatorVersion) =>
        Attribute(
                QualifiedName(QualifiedName(QualifiedName(AliasQualifiedName(IdentifierName(
                            Token(SyntaxKind.GlobalKeyword)),
                        IdentifierName("System")), IdentifierName("CodeDom")), IdentifierName("Compiler")),
                    IdentifierName("GeneratedCodeAttribute")))
            .WithArgumentList(
                AttributeArgumentList(
                    SeparatedList<AttributeArgumentSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            AttributeArgument(LiteralExpression(
                                    SyntaxKind.StringLiteralExpression, Literal(generatorName)))
                                .WithNameColon(NameColon(IdentifierName("tool"))),
                            Token(SyntaxKind.CommaToken), AttributeArgument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression, Literal(generatorVersion)))
                                .WithNameColon(NameColon(IdentifierName("version")))
                        })));

    internal static AttributeListSyntax Singelton(this AttributeSyntax attribute) => AttributeList(SingletonSeparatedList(attribute));

    internal static class PropConstants
    {
        internal static readonly AccessorDeclarationSyntax Setter = AccessorDeclaration(
                SyntaxKind.SetAccessorDeclaration)
            .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken));

        internal static readonly AccessorDeclarationSyntax Getter = AccessorDeclaration(
                SyntaxKind.GetAccessorDeclaration)
            .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken));
    }
}
