using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConfigManager.Generator.PredicateUtils;

internal static class SemanticHelper
{
    private static bool EqualsName(this ISymbol candidate, string name) =>
        candidate.ToDisplayString().Equals(name, StringComparison.Ordinal);

    internal static bool HasInterface(this INamedTypeSymbol targetType, string interfaceName)
    {
        foreach (INamedTypeSymbol @interface in targetType.AllInterfaces)
        {
            if (@interface.ConstructedFrom.EqualsName(interfaceName))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool HasAttribute(this ISymbol candidate, string targetAttributeName,
        CancellationToken? cancellationToken = null)
    {
        foreach (AttributeData attributeData in candidate.GetAttributes())
        {
            cancellationToken?.ThrowIfCancellationRequested();

            ISymbol? symbol = attributeData.AttributeClass;

            if (symbol is not null && symbol.EqualsName(targetAttributeName))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool HasAttribute(this MemberDeclarationSyntax candidate, string targetAttributeName,
        SemanticModel semanticModel, CancellationToken? cancellationToken = null)
    {
        foreach (AttributeListSyntax attributeList in candidate.AttributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                cancellationToken?.ThrowIfCancellationRequested();

                SymbolInfo symbolInfo =
                    semanticModel.GetSymbolInfo(attribute, cancellationToken ?? default(CancellationToken));
                ISymbol? symbol = symbolInfo.Symbol;

                if (symbol is not null && symbol.ContainingType.EqualsName(targetAttributeName))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
