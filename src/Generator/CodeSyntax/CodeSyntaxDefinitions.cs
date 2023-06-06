using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ConfigManager.Generator.CodeSyntax;

internal static class CodeSyntaxDefinitions
{
    private static readonly SyntaxToken CommaToken = Token(SyntaxKind.CommaToken);


    internal class Property : Field
    {
        private static class PropConstants
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

        internal bool HasGetter { get; }
        internal bool HasSetter { get; }

        internal string? BackingFieldName { get; }

        public Property(string name,
            string fullyQualifiedType,
            string? alternativeName = null,
            string? backingFieldName = null,
            Modifier[]? otherModifiers = null,
            SyntaxList<AttributeListSyntax>? attributes = null,
            Modifier accessModifier = Modifier.Public,
            bool hasGetter = true,
            bool hasSetter = false) :
            base(
                name,
                fullyQualifiedType,
                accessModifier,
                alternativeName,
                otherModifiers,
                attributes)
        {
            BackingFieldName = backingFieldName;
            HasGetter = hasGetter;
            HasSetter = hasSetter;
        }

        protected override MemberDeclarationSyntax MemberSyntax(AttributeSyntax codeGenAttribute)
        {
            SyntaxList<AccessorDeclarationSyntax> accessors = new();
            if (HasGetter)
            {
                accessors = accessors.Add(PropConstants.Getter);
            }

            if (HasSetter)
            {
                accessors = accessors.Add(PropConstants.Setter);
            }

            return PropertyDeclaration(
                    IdentifierName(FullyQualifiedType),
                    Identifier(Name))
                .WithAttributeLists(SingletonList(codeGenAttribute.Singelton()))
                .WithModifiers(TokenList(AccessModifier.SyntaxToken()))
                .WithAccessorList(AccessorList(accessors));
        }
    }

    internal class Field : MemberType
    {
        internal string AlternativeName { get; }

        public Field(string name, string fullyQualifiedType, Modifier accessModifier, string? alternativeName,
            Modifier[]? otherModifiers, SyntaxList<AttributeListSyntax>? attributes) :
            base(name, fullyQualifiedType,
                accessModifier, otherModifiers, attributes) =>
            AlternativeName = alternativeName ?? Name.ToLower();

        public Parameter AsParameter() => Parameter.CreateInstance(this);

        protected override MemberDeclarationSyntax MemberSyntax(AttributeSyntax codeGenAttribute) =>
            FieldDeclaration(
                    VariableDeclaration(IdentifierName(FullyQualifiedType))
                        .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(Name)))))
                .WithModifiers(TokenList(AccessModifier.SyntaxToken()))
                .WithAttributeLists(SingletonList(codeGenAttribute.Singelton()));

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

    internal class Constructor : Method
    {
        internal Constructor(string name, Modifier accessModifier, BlockSyntax? body = null,
            SyntaxList<AttributeListSyntax>? attributes = null) :
            base(name, name, accessModifier, null, null, attributes, body)
        {
        }

        protected override MemberDeclarationSyntax MemberSyntax(AttributeSyntax codeGenAttribute) =>
            ConstructorDeclaration(Identifier(Name))
                .WithParameterList(AsParameterList())
                .WithBody(Body);
    }

    internal class Method : MemberType
    {
        internal Parameter[] Parameters { get; }
        internal BlockSyntax Body { get; }

        public Method(string name, string fullyQualifiedReturnType, Modifier accessModifier, Modifier[]? otherModifiers,
            Parameter[]? parameters, SyntaxList<AttributeListSyntax>? attributes, BlockSyntax? body) : base(name,
            fullyQualifiedReturnType, accessModifier, otherModifiers, attributes)
        {
            Parameters = parameters ?? Array.Empty<Parameter>();
            Body = body ?? Block();
        }

        protected ParameterListSyntax AsParameterList()
        {
            SyntaxNodeOrToken[] parameters = new SyntaxNodeOrToken[Parameters.Length];

            for (int i = 0; i < Parameters.Length; i++)
            {
                parameters[i] = Parameters[i].AsSyntax();
            }

            return ParameterList(SeparatedList<ParameterSyntax>(parameters.JoinArray(CommaToken)));
        }

        protected override MemberDeclarationSyntax MemberSyntax(AttributeSyntax codeGenAttribute) =>
            MethodDeclaration(IdentifierName(FullyQualifiedType), Identifier(Name))
                .WithParameterList(AsParameterList())
                .WithBody(Body);
    }

    internal class Parameter
    {
        internal string FullyQualifiedType { get; }
        internal string Name { get; }

        private Parameter(string fullyQualifiedType, string name)
        {
            FullyQualifiedType = fullyQualifiedType;
            Name = name;
        }

        public static Parameter CreateInstance(Field field) => new(field.FullyQualifiedType, field.AlternativeName);

        public ParameterSyntax AsSyntax() =>
            Parameter(Identifier(Name))
                .WithType(IdentifierName(FullyQualifiedType));
    }

    internal class Attribute : Clazz
    {
        private const string FullGlobalPrefix = "global::System";

        private const string FullAttribute = $"{FullGlobalPrefix}.{nameof(Attribute)}";
        private const string FullAttributeUsage = $"{FullGlobalPrefix}.{nameof(AttributeUsageAttribute)}";
        private const string FullAttributeTargets = $"{FullGlobalPrefix}.{nameof(AttributeTargets)}";

        internal Attribute(string name,
            AttributeTargets targets,
            bool allowMultiple = false,
            Modifier accessModifier = Modifier.Public, Field[]? members = null, Method[]? methods = null,
            SyntaxList<AttributeListSyntax>? attributes = null) :
            base(
                $"{name}Attribute",
                accessModifier, null,
                members,
                methods,
                new[] {new BaseType(FullAttribute)},
                (attributes ?? new()).Add(AttributeUsageSyntax(allowMultiple, targets).Singelton()))
        {
        }

        private static AttributeArgumentSyntax BuildTargetsSyntax(AttributeTargets flaggedTargets)
        {
            if (flaggedTargets == AttributeTargets.All || flaggedTargets.HasFlag(AttributeTargets.All))
            {
                return AttributeArgument(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(FullAttributeTargets), IdentifierName(flaggedTargets.ToString()))
                );
            }

            AttributeTargets[] targets = flaggedTargets.SplitFlagEnum();
            string[] mapped = new string[targets.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                AttributeTargets attributeTargets = targets[i];
                mapped[i] = $"{FullAttributeTargets}.{attributeTargets.ToString()}";
            }

            string targetStr = string.Join(" | ", mapped);
            return AttributeArgument(IdentifierName(targetStr));
        }

        private static AttributeSyntax AttributeUsageSyntax(bool allowMultiple, AttributeTargets target)
        {
            SyntaxNodeOrToken[] arguments =
            {
                BuildTargetsSyntax(target), Token(SyntaxKind.CommaToken), AttributeArgument(
                        LiteralExpression(allowMultiple
                            ? SyntaxKind.TrueLiteralExpression
                            : SyntaxKind.FalseLiteralExpression))
                    .WithNameEquals(
                        NameEquals(
                            IdentifierName("AllowMultiple"))),
                Token(SyntaxKind.CommaToken), AttributeArgument(
                        LiteralExpression(
                            SyntaxKind.FalseLiteralExpression))
                    .WithNameEquals(
                        NameEquals(
                            IdentifierName("Inherited")))
            };
            return Attribute(
                    IdentifierName(FullAttributeUsage))
                .WithArgumentList(
                    AttributeArgumentList(
                        SeparatedList<AttributeArgumentSyntax>(
                            arguments)));
        }
    }

    internal class Clazz : MemberType
    {
        public Clazz(string name, Modifier accessModifier,
            Modifier[]? otherModifiers, Field[]? members,
            Method[]? methods, BaseType[]? baseTypes,
            SyntaxList<AttributeListSyntax>? attributes) : base(name, name, accessModifier, otherModifiers, attributes)
        {
            Members = members ?? Array.Empty<Field>();
            BaseTypes = baseTypes ?? Array.Empty<BaseType>();
            Methods = methods ?? Array.Empty<Method>();
        }

        internal Field[] Members { get; }
        internal BaseType[] BaseTypes { get; }
        internal Method[] Methods { get; }

        protected Constructor GetInitConstructor()
        {
            BlockSyntax? body = null;

            if (Members.Length <= 0)
            {
                return new(Name, Modifier.Public, body);
            }

            StatementSyntax[] expressions = new StatementSyntax[Members.Length];
            for (int i = 0; i < Members.Length; i++)
            {
                Field field = Members[i];
                expressions[i] = field.SetExpression();
            }

            body = Block(expressions);

            return new(Name, Modifier.Public, body);
        }

        protected override MemberDeclarationSyntax MemberSyntax(AttributeSyntax codeGenAttribute)
        {
            // ClassBuilder
            ClassDeclarationSyntax builder = ClassDeclaration(Name)
                // Modifiers (private/public static abstract etc)
                .WithModifiers(AsModifierList());

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

            MemberDeclarationSyntax[] classContent = new MemberDeclarationSyntax[Members.Length + Methods.Length + 1];

            for (int i = 0; i < Members.Length; i++)
            {
                classContent[i] = Members[i].AsMember(codeGenAttribute);
            }

            classContent[Members.Length == 0 ? 0 : Members.Length - 1] =
                GetInitConstructor().AsMember(codeGenAttribute);

            for (int classIndex = Members.Length, methodIndex = 0;
                 methodIndex < Methods.Length;
                 methodIndex++, classIndex++)
            {
                classContent[classIndex] = Methods[methodIndex].AsMember(codeGenAttribute);
            }

            return builder.WithMembers(List(classContent));
        }
    }

    internal abstract class MemberType : MultiModifierType
    {
        public string FileName => $"Generated_{Name}.cs";

        internal string Name { get; }
        internal string FullyQualifiedType { get; }
        private SyntaxList<AttributeListSyntax> Attributes { get; }


        protected MemberType(string name, string fullyQualifiedType, Modifier accessModifier,
            Modifier[]? otherModifiers,
            SyntaxList<AttributeListSyntax>? attributes) :
            base(accessModifier, otherModifiers)
        {
            Name = name;
            FullyQualifiedType = fullyQualifiedType;
            Attributes = attributes ?? new();
        }

        public MemberDeclarationSyntax AsMember(AttributeSyntax codeGenAttribute) => MemberSyntax(codeGenAttribute)
            .WithModifiers(AsModifierList())
            .WithAttributeLists(Attributes.Add(codeGenAttribute.Singelton()));

        protected abstract MemberDeclarationSyntax MemberSyntax(AttributeSyntax codeGenAttribute);
    }

    internal abstract class MultiModifierType
    {
        internal Modifier AccessModifier { get; }
        internal Modifier[] OtherModifiers { get; }

        protected MultiModifierType(Modifier accessModifier, Modifier[]? otherModifiers)
        {
            AccessModifier = accessModifier;
            OtherModifiers = otherModifiers ?? Array.Empty<Modifier>();
        }

        protected SyntaxTokenList AsModifierList()
        {
            if (OtherModifiers.Length <= 0)
            {
                TokenList(AccessModifier.SyntaxToken());
            }

            // Modifiers
            SyntaxToken[] modifiers = new SyntaxToken[OtherModifiers.Length + 1];
            modifiers[0] = AccessModifier.SyntaxToken();
            for (int i = 0; i < OtherModifiers.Length; i++) { modifiers[i + 1] = OtherModifiers[i].SyntaxToken(); }

            return TokenList(modifiers);
        }
    }

    internal record struct BaseType(string FullyQualifiedTypeName)
    {
        internal SimpleBaseTypeSyntax GetSyntax() =>
            SimpleBaseType(
                IdentifierName(FullyQualifiedTypeName));
    }
}
