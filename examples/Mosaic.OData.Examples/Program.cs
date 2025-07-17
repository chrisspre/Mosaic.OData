
using System.Diagnostics;
using Mosaic.OData.CSDL;
using Mosaic.OData.EDM;

if (Debugger.IsAttached)
{
    Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, @"..\..\..\");
}

Console.WriteLine("Reading CSDL tokens...");
using var reader = CsdlXmlReader.Create(@"..\..\data\example90.xml");

var tokens = reader.ReadTokens().ToList();
Console.WriteLine($"Found {tokens.Count} tokens:");

// Find NavigationPropertyBinding tokens specifically
var navBindingTokens = tokens.OfType<CsdlToken.Start>()
    .Where(t => t.ElementName == "NavigationPropertyBinding")
    .ToList();

Console.WriteLine($"\nFound {navBindingTokens.Count} NavigationPropertyBinding tokens:");
foreach (var token in navBindingTokens)
{
    Console.WriteLine($"  NavigationPropertyBinding with attributes: {string.Join(", ", token.Attributes.Select(kv => $"{kv.Key}='{kv.Value}'"))}");
}

const int maxTokensToShow = 90;
Console.WriteLine($"\nFirst {maxTokensToShow} tokens:");
foreach (var token in tokens.Take(maxTokensToShow))
{
    switch (token)
    {
        case CsdlToken.Start start:
            Console.WriteLine($"  SRT: {start.ElementName} with attributes: {string.Join(", ", start.Attributes.Select(kv => $"{kv.Key}='{kv.Value}'"))}");
            break;

        case CsdlToken.End end:
            Console.WriteLine($"  END: {end.ElementName}");
            break;
        case CsdlToken.Expression expr:
            Console.WriteLine($"  EXP: {expr.Value}");
            break;
        default:
            Console.WriteLine($"  Unknown: {token}");
            break;
    }
}

Console.WriteLine("\n\nLoading EDM model...");
try
{
    using var modelReader = CsdlXmlReader.Create(@"..\..\data\example89.xml");
    var model = EdmModel.Load(modelReader);
    Console.WriteLine($"Successfully loaded EDM model!");

    var schema = model.RootSchema;
    Console.WriteLine($"\nSchema: {schema.Namespace} (Name: {schema.Name})");
    Console.WriteLine($"  Schema children count: {schema.Children.Count}");

    var entityTypes = schema.Children.OfType<EntityType>().ToList();
    var complexTypes = schema.Children.OfType<ComplexType>().ToList();
    var entityContainer = schema.Children.OfType<EntityContainer>().FirstOrDefault();

    Console.WriteLine($"  EntityTypes: {entityTypes.Count}");
    foreach (var et in entityTypes)
    {
        Console.WriteLine($"    - {et.Name}");
    }
    Console.WriteLine($"  ComplexTypes: {complexTypes.Count}");
    foreach (var ct in complexTypes)
    {
        Console.WriteLine($"    - {ct.Name}");
    }
    Console.WriteLine($"  EntityContainer: {(entityContainer != null ? entityContainer.Name : "None")}");

    if (entityContainer != null)
    {
        var entitySets = entityContainer.Children.OfType<EntitySet>().ToList();
        var singletons = entityContainer.Children.OfType<Singleton>().ToList();
        var actionImports = entityContainer.Children.OfType<ActionImport>().ToList();
        var functionImports = entityContainer.Children.OfType<FunctionImport>().ToList();

        Console.WriteLine($"    EntitySets: {entitySets.Count}");
        Console.WriteLine($"    Singletons: {singletons.Count}");
        Console.WriteLine($"    ActionImports: {actionImports.Count}");
        Console.WriteLine($"    FunctionImports: {functionImports.Count}");

        // Show some NavigationPropertyBindings from the first EntitySet
        var firstEntitySet = entitySets.FirstOrDefault();
        if (firstEntitySet != null)
        {
            var navBindings = firstEntitySet.Children.OfType<NavigationPropertyBinding>().Take(3);
            Console.WriteLine($"    Sample NavigationPropertyBindings from '{firstEntitySet.Name}':");
            foreach (var binding in navBindings)
            {
                Console.WriteLine($"      {binding.Path} -> {binding.Target}");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading EDM model: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
