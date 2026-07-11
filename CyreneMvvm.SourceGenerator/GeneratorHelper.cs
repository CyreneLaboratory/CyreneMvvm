using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CyreneMvvm.SourceGenerator;

public static class GeneratorHelper
{
    public const string INotifyCallback = "CyreneMvvm.Model.INotifyCallback";
    public const string ObObject = "CyreneMvvm.Model.ObObject";
    public const string ObList = "CyreneMvvm.Model.ObList";
    public const string ObDictionary = "CyreneMvvm.Model.ObDictionary";
    public const string ObIgnore = "CyreneMvvm.Attributes.ObIgnoreAttribute";
    public const string ObShadow = "CyreneMvvm.Attributes.ObShadowAttribute";

    public static bool IsPrimary(ITypeSymbol typeSymbol)
    {
        var actualType = typeSymbol;
        if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedType)
            actualType = namedType.TypeArguments[0];

        if (actualType.TypeKind == TypeKind.Enum) return true;
        return actualType.SpecialType switch
        {
            SpecialType.System_Boolean or
            SpecialType.System_SByte or SpecialType.System_Byte or
            SpecialType.System_Int16 or SpecialType.System_UInt16 or
            SpecialType.System_Int32 or SpecialType.System_UInt32 or
            SpecialType.System_Int64 or SpecialType.System_UInt64 or
            SpecialType.System_Char or SpecialType.System_String or
            SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal => true,
            _ => false
        };
    }

    public static bool IsObObject(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
            typeSymbol = nullable.TypeArguments[0];

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.ToDisplayString().Contains(ObObject)) return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    public static bool IsObCollection(PropertyDeclarationSyntax prop, SemanticModel model)
    {
        var propertySymbol = model.GetDeclaredSymbol(prop);
        if (propertySymbol == null) return false;

        var typeString = propertySymbol.Type.ToDisplayString();
        return typeString.Contains(ObList) || typeString.Contains(ObDictionary);
    }

    public static bool IsSupportedType(ITypeSymbol typeSymbol)
    {
        var typeString = typeSymbol.ToDisplayString();
        if (typeString.Contains(ObList) || typeString.Contains(ObDictionary)) return true;
        return IsPrimary(typeSymbol) || IsObObject(typeSymbol);
    }

    public static bool HasObIgnoreAttr(PropertyDeclarationSyntax prop, SemanticModel model)
    {
        foreach (var attributeList in prop.AttributeLists)
            foreach (var attribute in attributeList.Attributes)
            {
                var symbol = model.GetSymbolInfo(attribute).Symbol?.ContainingType;
                if (symbol != null && symbol.ToDisplayString().Contains(ObIgnore)) return true;
            }
        return false;
    }

    public static bool HasObShadowAttr(INamedTypeSymbol classSymbol)
    {
        var current = classSymbol;
        while (current != null)
        {
            foreach (var attribute in current.GetAttributes())
                if (attribute.AttributeClass?.ToDisplayString().Contains(ObShadow) == true)
                    return true;
            current = current.BaseType;
        }
        return false;
    }

    public static bool ShouldGenShadow(INamedTypeSymbol classSymbol, IPropertySymbol propSymbol)
    {
        if (!HasObShadowAttr(classSymbol)) return false;
        if (IsPrimary(propSymbol.Type)) return false;
        return true;
    }

    public static readonly DiagnosticDescriptor UnsupportedType = new("CYM001", "Unsupported observable type",
        "Property '{0}' with type '{1}' cannot be observed.", "CyreneMvvm", DiagnosticSeverity.Error, true);
}
