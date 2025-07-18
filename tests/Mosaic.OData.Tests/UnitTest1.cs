using Mosaic.OData.EDM;

namespace Mosaic.OData.Tests;

public class IncludeAnnotationsTests
{
    [Fact]
    public void IncludeAnnotations_Create_WithAllAttributes_ShouldSucceed()
    {
        // Arrange
        var context = new ModelBuilderContext();
        var attributes = new Dictionary<string, string>
        {
            ["TermNamespace"] = "MyApp.Annotations",
            ["Qualifier"] = "MyQualifier",
            ["TargetNamespace"] = "MyApp.Core"
        };

        // Act
        var includeAnnotations = IncludeAnnotations.Create(context, attributes);

        // Assert
        Assert.NotNull(includeAnnotations);
        Assert.Equal("MyApp.Annotations", includeAnnotations.TermNamespace);
        Assert.Equal("MyQualifier", includeAnnotations.Qualifier);
        Assert.Equal("MyApp.Core", includeAnnotations.TargetNamespace);
        Assert.Equal("IncludeAnnotations", includeAnnotations.Name);
    }

    [Fact]
    public void IncludeAnnotations_Create_WithMinimalAttributes_ShouldSucceed()
    {
        // Arrange
        var context = new ModelBuilderContext();
        var attributes = new Dictionary<string, string>
        {
            ["TermNamespace"] = "MyApp.Annotations"
        };

        // Act
        var includeAnnotations = IncludeAnnotations.Create(context, attributes);

        // Assert
        Assert.NotNull(includeAnnotations);
        Assert.Equal("MyApp.Annotations", includeAnnotations.TermNamespace);
        Assert.Null(includeAnnotations.Qualifier);
        Assert.Null(includeAnnotations.TargetNamespace);
    }

    [Fact]
    public void IncludeAnnotations_Create_WithoutTermNamespace_ShouldUseDefault()
    {
        // Arrange
        var context = new ModelBuilderContext();
        var attributes = new Dictionary<string, string>();

        // Act
        var includeAnnotations = IncludeAnnotations.Create(context, attributes);

        // Assert
        Assert.NotNull(includeAnnotations);
        Assert.Equal("UnknownTermNamespace", includeAnnotations.TermNamespace);
    }
}

public class NavigationPropertyBindingTests
{
    [Fact]
    public void NavigationPropertyBinding_Create_WithMissingPath_ShouldThrowEdmElementCreationException()
    {
        // Arrange
        var context = new ModelBuilderContext();
        var attributes = new Dictionary<string, string>
        {
            { "Target", "SomeTarget" }
            // Missing "Path" attribute
        };

        // Act & Assert
        var exception = Assert.Throws<EdmElementCreationException>(() => 
            NavigationPropertyBinding.Create(context, attributes));
        
        Assert.Contains("Missing required 'Path' attribute", exception.Message);
        Assert.Contains("Available attributes: Target", exception.Message);
    }

    [Fact]
    public void NavigationPropertyBinding_Create_WithMissingTarget_ShouldThrowEdmElementCreationException()
    {
        // Arrange
        var context = new ModelBuilderContext();
        var attributes = new Dictionary<string, string>
        {
            { "Path", "SomePath" }
            // Missing "Target" attribute
        };

        // Act & Assert
        var exception = Assert.Throws<EdmElementCreationException>(() => 
            NavigationPropertyBinding.Create(context, attributes));
        
        Assert.Contains("Missing required 'Target' attribute", exception.Message);
        Assert.Contains("Available attributes: Path", exception.Message);
    }

    [Fact]
    public void NavigationPropertyBinding_Create_WithValidAttributes_ShouldSucceed()
    {
        // Arrange
        var context = new ModelBuilderContext();
        var attributes = new Dictionary<string, string>
        {
            { "Path", "SomePath" },
            { "Target", "SomeTarget" }
        };

        // Act
        var binding = NavigationPropertyBinding.Create(context, attributes);

        // Assert
        Assert.NotNull(binding);
        Assert.Equal("SomePath", binding.PathExpression);
        Assert.Equal("SomeTarget", binding.TargetExpression);
        Assert.Equal("NavigationPropertyBinding", binding.Name);
    }
}