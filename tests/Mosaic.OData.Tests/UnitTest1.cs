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