using System.Diagnostics;
using System.Reflection;
using Mosaic.OData.CSDL;
using Mosaic.OData.EDM;
using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Warning()
        .WriteTo.Console()
        .CreateLogger();

        var dataFilePath = FindDataFile("example89.xml");
        Console.WriteLine($"Using data file: {dataFilePath}");

        Console.WriteLine("Reading CSDL tokens...");
        using var reader = CsdlXmlReader.Create(dataFilePath);

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

        const int maxTokensToShow = 16;
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
            using var modelReader = CsdlXmlReader.Create(dataFilePath);
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

        // Clean up Serilog
        Log.CloseAndFlush();
    }

    /// <summary>
    /// Finds the data file by walking up the directory tree from the assembly location.
    /// This works regardless of how the application is executed (debugger, dotnet run, dotnet run --project).
    /// </summary>
    /// <param name="fileName">The name of the data file to find.</param>
    /// <returns>The full path to the data file.</returns>
    private static string FindDataFile(string fileName)
    {
        // Start from the assembly location
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var currentDir = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException("Could not determine assembly directory");

        // Walk up the directory tree looking for the data folder
        var searchDir = new DirectoryInfo(currentDir);
        while (searchDir != null)
        {
            // Look for a data folder in the current directory
            var dataDir = Path.Combine(searchDir.FullName, "data");
            if (Directory.Exists(dataDir))
            {
                var dataFile = Path.Combine(dataDir, fileName);
                if (File.Exists(dataFile))
                {
                    return dataFile;
                }
            }

            // Move up one level
            searchDir = searchDir.Parent;
        }

        // If we can't find it by walking up, try some common relative paths as fallbacks
        var fallbackPaths = Enumerable.Range(0, 5)
            .Select(i => Path.Combine([.. Enumerable.Repeat("..", i), "data", fileName]))
            .ToArray();

        foreach (var fallbackPath in fallbackPaths)
        {
            if (File.Exists(fallbackPath))
            {
                return Path.GetFullPath(fallbackPath);
            }
        }

        throw new FileNotFoundException($"Could not find data file '{fileName}'. Searched from: {currentDir}");
    }
}