using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConfigManager.Generator.PredicateUtils;

internal static class SemanticTransformHelper
{
    internal static bool HasInterface(INamedTypeSymbol? type, INamedTypeSymbol targetInterface) =>
        type is not null && type.AllInterfaces.Any(@interface =>
            @interface.OriginalDefinition.Equals(targetInterface, SymbolEqualityComparer.Default));

    internal static bool TryGetTypeByName(Compilation compilation, string typeName, out INamedTypeSymbol? type)
    {
        type = compilation.GetTypeByMetadataName(typeName);

        return type != null;
    }

    internal static bool TryGetAttributeTypeOfInfo(MemberDeclarationSyntax candidate,
        ISymbol? targetAttribute, SemanticModel semanticModel,
        out INamedTypeSymbol? typeofSymbol)
    {
        foreach (AttributeListSyntax attributeList in candidate.AttributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                if (TryExtractSymbol(targetAttribute, semanticModel, out typeofSymbol, attribute))
                {
                    return true;
                }
            }
        }

        typeofSymbol = null;
        return false;
    }

    private static bool TryExtractSymbol(ISymbol? target, SemanticModel semanticModel,
        out INamedTypeSymbol? typeofSymbol,
        AttributeSyntax attribute)
    {
        SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(attribute);
        ISymbol? symbol = symbolInfo.Symbol;
        typeofSymbol = null;

        // check that the attribute is of the target type and as 1 argument
        if (symbol is null
            || !SymbolEqualityComparer.Default.Equals(symbol.ContainingSymbol, target)
            || attribute.ArgumentList is
                not
                {
                    Arguments.Count: 1
                } argumentList)
        {
            return false;
        }

        // check that the argument is "typeof(...)" expression
        AttributeArgumentSyntax argument = argumentList.Arguments[0];
        if (argument.Expression is not TypeOfExpressionSyntax typeOf
            || semanticModel.GetSymbolInfo(typeOf.Type).Symbol is not INamedTypeSymbol type)
        {
            return false;
        }

        typeofSymbol = type;
        return true;
    }
}
