using Mosaic.OData.EDM;

namespace Mosaic.OData.Examples;

public static class ExceptionTest
{
    public static void TestNavigationPropertyBindingException()
    {
        Console.WriteLine("\nTesting NavigationPropertyBinding exception handling...");
        
        var context = new ModelBuilderContext();
        
        // Test with missing Path attribute
        var attributesMissingPath = new Dictionary<string, string>
        {
            { "Target", "SomeTarget" }
            // Missing "Path" attribute
        };
        
        try
        {
            var binding = NavigationPropertyBinding.Create(context, attributesMissingPath);
            Console.WriteLine("ERROR: Should have thrown EdmElementCreationException!");
        }
        catch (EdmElementCreationException ex)
        {
            Console.WriteLine($"✓ Correctly caught EdmElementCreationException: {ex.Message}");
        }
        
        // Test with missing Target attribute
        var attributesMissingTarget = new Dictionary<string, string>
        {
            { "Path", "SomePath" }
            // Missing "Target" attribute
        };
        
        try
        {
            var binding = NavigationPropertyBinding.Create(context, attributesMissingTarget);
            Console.WriteLine("ERROR: Should have thrown EdmElementCreationException!");
        }
        catch (EdmElementCreationException ex)
        {
            Console.WriteLine($"✓ Correctly caught EdmElementCreationException: {ex.Message}");
        }
        
        // Test with valid attributes
        var validAttributes = new Dictionary<string, string>
        {
            { "Path", "SomePath" },
            { "Target", "SomeTarget" }
        };
        
        try
        {
            var binding = NavigationPropertyBinding.Create(context, validAttributes);
            Console.WriteLine($"✓ Successfully created NavigationPropertyBinding with Path='{binding.PathExpression}' and Target='{binding.TargetExpression}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Unexpected exception: {ex.Message}");
        }
        
        Console.WriteLine("NavigationPropertyBinding exception testing completed.\n");
    }
}
