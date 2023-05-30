using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ConfigManager.Generator.CodeSyntaxDeclarations;

internal static class CodeSyntaxDefinitions
{
    private static readonly SyntaxToken CommaToken = Token(SyntaxKind.CommaToken);


    internal class Property : Field
    {
        public bool HasGetter { get; }
        public bool HasSetter { get; }

        public string? BackingFieldName { get; private set; } // TODO: make private when possible!


        public Property WithBackingFieldName(string name)
        {
            BackingFieldName = name;
            return this;
        }

        public new Property WithAlternativeName(string name) => (Property)base.WithAlternativeName(name);

        public bool HasBackingFieldName => BackingFieldName is not null;

        public Property(string name, string fullyQualifiedType,
            Modifier accessModifier = Modifier.Public, bool hasGetter = true,
            bool hasSetter = false) : base(name, fullyQualifiedType, accessModifier)
        {
            HasGetter = hasGetter;
            HasSetter = hasSetter;
        }


        public override string GetCode(AttributeSyntax codeGenAttribute) =>
            AsMember(codeGenAttribute).NormalizeWhitespace().ToString();

        public override MemberDeclarationSyntax AsMember(AttributeSyntax codeGenAttribute)
        {
            SyntaxList<AccessorDeclarationSyntax> accessors = new();
            if (HasGetter)
            {
                accessors=accessors.Add(SyntaxBuilder.PropConstants.Getter);
            }

            if (HasSetter)
            {
                accessors=accessors.Add(SyntaxBuilder.PropConstants.Setter);
            }

            return PropertyDeclaration(
                    IdentifierName(FullyQualifiedType),
                    Identifier(Name))
                .WithAttributeLists(SingletonList(codeGenAttribute.Singelton()))
                .WithModifiers(TokenList(AccessModifier.SyntaxToken()))
                .WithAccessorList(AccessorList(accessors));
        }
    }

    internal class Field
    {
        private string? _alternativeName;
        public string Name { get; }
        public string FullyQualifiedType { get; }

        private string AlternativeName
        {
            get => _alternativeName ?? Name.ToLower();
            set => _alternativeName = value;
        }

        public Modifier AccessModifier { get; }
        public bool HasAlternativeName => _alternativeName is not null;

        public Field(string name, string fullyQualifiedType, Modifier accessModifier)
        {
            Name = name;
            FullyQualifiedType = fullyQualifiedType;
            AccessModifier = accessModifier;
        }

        public virtual string GetCode(AttributeSyntax codeGenAttribute) =>
            AsMember(codeGenAttribute).NormalizeWhitespace().ToString();


        public virtual MemberDeclarationSyntax AsMember(AttributeSyntax codeGenAttribute) =>
            FieldDeclaration(
                    VariableDeclaration(IdentifierName(FullyQualifiedType))
                        .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(Name)))))
                .WithModifiers(TokenList(AccessModifier.SyntaxToken()))
                .WithAttributeLists(SingletonList(codeGenAttribute.Singelton()));

        public virtual ParameterSyntax AsParameter() =>
            Parameter(Identifier(AlternativeName)).WithType(IdentifierName(FullyQualifiedType));

        public Field WithAlternativeName(string name)
        {
            AlternativeName = name;
            return this;
        }

        public ExpressionStatementSyntax SetExpression() =>
            ExpressionStatement(AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName(Name)
                ), IdentifierName(AlternativeName)
            ));
    }

    internal class Constructor
    {
        public string Name { get; }
        public Field[] Fields { get; }
        public Modifier AccessModifier { get; }

        public Constructor(string name, Field[] fields, Modifier accessModifier)
        {
            Name = name;
            Fields = fields;
            AccessModifier = accessModifier;
        }

        public virtual string GetCode(AttributeSyntax codeGenAttribute) =>
            GetSyntax(codeGenAttribute).NormalizeWhitespace().ToString();


        public virtual MemberDeclarationSyntax GetSyntax(AttributeSyntax codeGenAttribute)
        {
            ConstructorDeclarationSyntax builder = ConstructorDeclaration(Identifier(Name))
                .WithAttributeLists(SingletonList(codeGenAttribute.Singelton()))
                .WithModifiers(TokenList(AccessModifier.SyntaxToken()));

            SyntaxNodeOrToken[] parameterFields = new SyntaxNodeOrToken[Fields.Length];
            StatementSyntax[] setExpressions = new StatementSyntax[Fields.Length];

            if (Fields.Length == 0)
            {
                return builder.WithBody(Block());
            }

            for (int i = 0; i < Fields.Length; i++)
            {
                Field field = Fields[i];

                parameterFields[i] = field.AsParameter();
                setExpressions[i] = field.SetExpression();
            }

            return builder.WithParameterList(
                    ParameterList(SeparatedList<ParameterSyntax>(parameterFields.JoinArray(CommaToken))))
                .WithBody(Block(setExpressions));
        }
    }

    internal class Clazz
    {
        private Clazz(string name, Modifier accessModifier, Field[] members,
            Modifier[] otherModifiers, SyntaxList<AttributeListSyntax> attributes, BaseType[] baseTypes)
        {
            Name = name;
            Members = members;
            Attributes = attributes;
            BaseTypes = baseTypes;
            AccessModifier = accessModifier;
            OtherModifiers = otherModifiers;
        }

        public static Clazz CreateInstance(string name, Modifier accessModifier, Field[]? members = null,
            Modifier[]? otherModifiers = null, SyntaxList<AttributeListSyntax>? attributes = null,
            BaseType[]? baseTypes = null) =>
            new(name, accessModifier, members ?? Array.Empty<Field>(),
                otherModifiers ?? Array.Empty<Modifier>(), attributes ?? new(), baseTypes ?? Array.Empty<BaseType>());

        public string Name { get; }
        public Modifier AccessModifier { get; }
        public Modifier[] OtherModifiers { get; }
        public Field[] Members { get; set; }
        public BaseType[] BaseTypes { get; }
        public SyntaxList<AttributeListSyntax> Attributes { get; }

        public string GetCode(AttributeSyntax codeGenAttribute) =>
            GetSyntax(codeGenAttribute).NormalizeWhitespace().ToString();

        private ClassDeclarationSyntax GetSyntax(AttributeSyntax codeGenAttribute)
        {
            // Modifiers
            SyntaxToken[] modifiers = new SyntaxToken[OtherModifiers.Length + 1];

            modifiers[0] = AccessModifier.SyntaxToken();
            if (OtherModifiers.Length > 0)
            {
                for (int i = 0; i < OtherModifiers.Length; i++) { modifiers[i + 1] = OtherModifiers[i].SyntaxToken(); }
            }

            // ClassBuilder
            ClassDeclarationSyntax builder = ClassDeclaration(Name);
            builder = builder.WithAttributeLists(Attributes.Add(codeGenAttribute.Singelton()));

            SyntaxTokenList syntaxTokenList = TokenList(modifiers);
            builder = builder.WithModifiers(syntaxTokenList);

            // BaseClasses
            if (BaseTypes.Length > 0)
            {
                SyntaxNodeOrToken[] baseTypes = new SyntaxNodeOrToken[BaseTypes.Length];
                for (int i = 0; i < BaseTypes.Length; i++)
                {
                    baseTypes[i] = BaseTypes[i].GetSyntax();
                }

                builder = builder.WithBaseList(
                    BaseList(
                        SeparatedList<BaseTypeSyntax>(
                            baseTypes.JoinArray(CommaToken)
                        )));
            }

            MemberDeclarationSyntax[] members = new MemberDeclarationSyntax[Members.Length + 1];

            for (int i = 0; i < Members.Length; i++)
            {
                members[i] = Members[i].AsMember(codeGenAttribute);
            }

            members[members.Length - 1] = new Constructor(Name, Members, Modifier.Public).GetSyntax(codeGenAttribute);

            return builder.WithMembers(List(members));
        }
    }

    internal record struct BaseType(string FullyQualifiedTypeName)
    {
        internal SimpleBaseTypeSyntax GetSyntax() =>
            SimpleBaseType(
                IdentifierName(FullyQualifiedTypeName));
    }
}

class tmp
{
    tmp()
    {
        var x = ClassDeclaration("ATTRIBUTENAME")
            .WithMembers(
                List(
                    new MemberDeclarationSyntax[]
                    {
                        // PROPERTIES

                        PropertyDeclaration(
                                IdentifierName("PROPTYPE1"),
                                Identifier("PROPNAME1"))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithAccessorList(
                                AccessorList(
                                    SingletonList<AccessorDeclarationSyntax>(
                                        AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(
                                                Token(SyntaxKind.SemicolonToken))))),
                        PropertyDeclaration(
                                IdentifierName("PROPTYPE2"),
                                Identifier("PROPNAME2"))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithAccessorList(
                                AccessorList(
                                    SingletonList<AccessorDeclarationSyntax>(
                                        AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(
                                                Token(SyntaxKind.SemicolonToken))))),
                        PropertyDeclaration(
                                IdentifierName("PROPTYPE3"),
                                Identifier("PROPNAME3"))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithAccessorList(
                                AccessorList(
                                    SingletonList<AccessorDeclarationSyntax>(
                                        AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration)
                                            .WithSemicolonToken(
                                                Token(SyntaxKind.SemicolonToken))))),


                        // FIELDS


                        FieldDeclaration(
                                VariableDeclaration(
                                        IdentifierName("FIELDTYPE1"))
                                    .WithVariables(
                                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            VariableDeclarator(
                                                Identifier("FIELDNAME1")))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword))),
                        FieldDeclaration(
                                VariableDeclaration(
                                        IdentifierName("FIELDTYPE2"))
                                    .WithVariables(
                                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            VariableDeclarator(
                                                Identifier("FIELDNAME2")))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword))),
                        FieldDeclaration(
                                VariableDeclaration(
                                        IdentifierName("FIELDTYPE3"))
                                    .WithVariables(
                                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            VariableDeclarator(
                                                Identifier("FIELDNAME3")))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword))),


                        // CONSTRUCTOR


                        ConstructorDeclaration(
                                Identifier("ATTRIBUTENAME"))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithParameterList(
                                ParameterList(
                                    SeparatedList<ParameterSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            Parameter(
                                                    Identifier("propalt1"))
                                                .WithType(
                                                    IdentifierName("PROPTYPE1")),
                                            Token(SyntaxKind.CommaToken), Parameter(
                                                    Identifier("propalt2"))
                                                .WithType(
                                                    IdentifierName("PROPTYPE2")),
                                            Token(SyntaxKind.CommaToken), Parameter(
                                                    Identifier("propalt3"))
                                                .WithType(
                                                    IdentifierName("PROPTYPE3")),
                                            Token(SyntaxKind.CommaToken), Parameter(
                                                    Identifier("fieldalt1"))
                                                .WithType(
                                                    IdentifierName("FIELDTYPE1")),
                                            Token(SyntaxKind.CommaToken), Parameter(
                                                    Identifier("fieldalt2"))
                                                .WithType(
                                                    IdentifierName("FIELDTYPE2")),
                                            Token(SyntaxKind.CommaToken), Parameter(
                                                    Identifier("fieldalt3"))
                                                .WithType(
                                                    IdentifierName("FIELDTYPE3"))
                                        })))
                            .WithBody(
                                Block(
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName("PROPNAME1")),
                                            IdentifierName("propalt1"))),
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName("PROPNAME2")),
                                            IdentifierName("propalt2"))),
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName("PROPNAME3")),
                                            IdentifierName("propalt3"))),
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName("FIELDNAME1")),
                                            IdentifierName("fieldalt1"))),
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName("FIELDNAME2")),
                                            IdentifierName("fieldalt2"))),
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName("FIELDNAME3")),
                                            IdentifierName("fieldalt3")))))
                    }));
    }
}

/*
Examples:

Property:
[global::System.CodeDom.Compiler.GeneratedCode(tool:"GEN NAME",version:"GEN VERSION")]
public TYPE NAME {get;}

Attribute:
// <auto-generated>
#nullable enable
namespace ConfigManager.Attributes
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(tool:"ConfigPropertyChangeGenerator", version:"1.0.0.0")]
    [global::System.AttributeUsage(global::System.AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    internal sealed class TMPAttribute : global::System.Attribute
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute(tool:"ConfigPropertyChangeGenerator", version:"1.0.0.0")]
        public string Prop1 { get; }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute(tool:"ConfigPropertyChangeGenerator", version:"1.0.0.0")]
        public string Prop2 { get; }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute(tool:"ConfigPropertyChangeGenerator", version:"1.0.0.0")]
        public int Prop3 { get; }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute(tool:"ConfigPropertyChangeGenerator", version:"1.0.0.0")]
        public TMPAttribute(string prop1, string prop2, int prop3) {
            this.Prop1 = prop1;
            this.Prop2 = prop2;
            this.Prop3 = prop3;
        }
    }
}


 */
