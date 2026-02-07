using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CyreneMvvm.SourceGenerator;

public static class GeneratorHelper
{
    public const string ObservableObject = "CyreneMvvm.Model.ObservableObject";
    public const string ObservableList = "CyreneMvvm.Model.ObservableList";
    public const string ObservableDictionary = "CyreneMvvm.Model.ObservableDictionary";
    public const string ObProp = "CyreneMvvm.Attributes.ObPropAttribute";

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

    public static bool ShouldGenProp(PropertyDeclarationSyntax prop, SemanticModel model)
    {
        return IsPartialProperty(prop) && HasObservablePropAttr(prop, model) && !IsObservableCollection(prop, model);
    }

    public static bool ShouldGenCollection(PropertyDeclarationSyntax prop, SemanticModel model)
    {
        return IsPartialProperty(prop) && IsObservableCollection(prop, model);
    }
}
