using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ConfigManager.Generator.CodeSyntaxDeclarations;

internal enum Modifier
{
    Private,
    Internal,
    Protected,
    Public,
    
    Static,
    Sealed,
    Partial,
    
    Abstract
}

internal static class AccessModifierExtensions
{
    internal static SyntaxToken SyntaxToken(this Modifier modifier) =>
        SyntaxFactory.Token( modifier switch
        {
            Modifier.Public => SyntaxKind.PublicKeyword,
            Modifier.Private => SyntaxKind.PrivateKeyword,
            Modifier.Protected => SyntaxKind.ProtectedKeyword,
            Modifier.Internal => SyntaxKind.InternalKeyword,
            
            Modifier.Static => SyntaxKind.StaticKeyword,
            Modifier.Sealed => SyntaxKind.SealedKeyword,
            Modifier.Partial => SyntaxKind.PartialKeyword,
            
            Modifier.Abstract => SyntaxKind.AbstractKeyword,
            _ => throw new($"Access modifier {modifier.ToString()} not supported yet.")
        });
}
