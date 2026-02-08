using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CyreneMvvm.SourceGenerator;

public static class GeneratorHelper
{
    public const string INotifyCallback = "CyreneMvvm.Model.INotifyCallback";
    public const string ObservableObject = "CyreneMvvm.Model.ObservableObject";
    public const string ObservableList = "CyreneMvvm.Model.ObservableList";
    public const string ObservableDictionary = "CyreneMvvm.Model.ObservableDictionary";
    public const string ObProp = "CyreneMvvm.Attributes.ObPropAttribute";
    public const string ObColumn = "CyreneMvvm.Attributes.ObColumnAttribute";

    public static bool IsPrimary(ITypeSymbol typeSymbol)
    {
        var actualType = typeSymbol;
        if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedType)
            actualType = namedType.TypeArguments[0];

        if (actualType.TypeKind == TypeKind.Enum) return true;
        return actualType.SpecialType switch
        {
            SpecialType.System_Boolean or SpecialType.System_Char or
            SpecialType.System_SByte or SpecialType.System_Byte or
            SpecialType.System_Int16 or SpecialType.System_UInt16 or
            SpecialType.System_Int32 or SpecialType.System_UInt32 or
            SpecialType.System_Int64 or SpecialType.System_UInt64 or
            SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal => true,
            _ => false
        };
    }

    public static bool IsObservableObject(INamedTypeSymbol classSymbol)
    {
        var baseType = classSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.ToDisplayString().Contains(ObservableObject)) return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    public static bool IsPartialProperty(PropertyDeclarationSyntax prop)
    {
        return prop.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    public static bool IsObservableCollection(PropertyDeclarationSyntax prop, SemanticModel model)
    {
        var propertySymbol = model.GetDeclaredSymbol(prop);
        if (propertySymbol == null) return false;

        var typeString = propertySymbol.Type.ToDisplayString();
        return typeString.Contains(ObservableList) || typeString.Contains(ObservableDictionary);
    }

    public static bool HasObservablePropAttr(PropertyDeclarationSyntax prop, SemanticModel model)
    {
        foreach (var attributeList in prop.AttributeLists)
            foreach (var attribute in attributeList.Attributes)
            {
                var symbol = model.GetSymbolInfo(attribute).Symbol?.ContainingType;
                if (symbol != null && symbol.ToDisplayString().Contains(ObProp)) return true;
            }
        return false;
    }

    public static bool HasObservableColumnAttr(PropertyDeclarationSyntax prop, SemanticModel model)
    {
        foreach (var attributeList in prop.AttributeLists)
            foreach (var attribute in attributeList.Attributes)
            {
                var symbol = model.GetSymbolInfo(attribute).Symbol?.ContainingType;
                if (symbol != null && symbol.ToDisplayString().Contains(ObColumn)) return true;
            }
        return false;
    }

    public static bool HasObservableColumnAttr(IPropertySymbol propSymbol)
    {
        foreach (var attribute in propSymbol.GetAttributes())
            if (attribute.AttributeClass?.ToDisplayString().Contains(ObColumn) == true)
                return true;
        return false;
    }

    public static bool ShouldGenProp(PropertyDeclarationSyntax prop, SemanticModel model)
    {
        return IsPartialProperty(prop) && !IsObservableCollection(prop, model) &&
            (HasObservablePropAttr(prop, model) || HasObservableColumnAttr(prop, model));
    }

    public static bool ShouldGenCollection(PropertyDeclarationSyntax prop, SemanticModel model)
    {
        return IsPartialProperty(prop) && IsObservableCollection(prop, model);
    }
}
