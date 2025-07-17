# OASIS CSDL XML Elements and Attributes  Classification

Based on the OASIS OData CSDL XML Representation Version 4.02 specification, Appendix B, 
this document lists all EDM elements and categorizes their attribute types.
[Appendix B](https://docs.oasis-open.org/odata/odata-csdl-xml/v4.02/csd01/odata-csdl-xml-v4.02-csd01.html#TableofXMLElementsandAttributes)


## Attribute Type Categories

- **Basic Attribute**: Simple value types (strings, booleans, numbers, identifiers)
- **Reference Attribute**: References to other model elements represented in XML/JSON as qualified names, type names, etc.
  - **Global Reference**: Fully qualified names that reference elements anywhere in the model
  - **Relative Reference**: Names that reference elements relative to a specific parent context
- **Path Attribute**: Path expressions in form of a sequence of model elements represented in XML/JSON as a string with delimiters (e.g., `/`, `.`, `@` ...)
  - **Absolute Path**: Paths that start from a well-defined root context
  - **Relative Path**: Paths that are resolved relative to a specific element context

---

## Edm Elements and their Attributes 

### Core EDMX Elements

#### `edmx:Edmx`
- **Version** - Basic Attribute (version number string)

#### `edmx:DataServices`
- No attributes

#### `edmx:Reference`
- **Uri** - Basic Attribute (URI string)

#### `edmx:Include`
- **Namespace** - Reference Attribute (namespace reference)
- **Alias** - Basic Attribute (simple identifier)

#### `edmx:IncludeAnnotations`
- **TermNamespace** - Reference Attribute (namespace reference)
- **Qualifier** - Basic Attribute (simple identifier)
- **TargetNamespace** - Reference Attribute (namespace reference)

### Schema Elements

#### `edm:Schema`
- **Namespace** - Basic Attribute (namespace string)
- **Alias** - Basic Attribute (simple identifier)

#### `edm:Annotations`
- **Target** - Path Attribute (target path expression) - Absolute Path (can reference any model element)
- **Qualifier** - Basic Attribute (simple identifier)

### Entity Model Elements

#### `edm:EntityType`
- **Name** - Basic Attribute (simple identifier)
- **BaseType** - Reference Attribute (qualified type name) - Global Reference
- **Abstract** - Basic Attribute (boolean)
- **OpenType** - Basic Attribute (boolean)
- **HasStream** - Basic Attribute (boolean)

#### `edm:Key`
- No attributes

#### `edm:PropertyRef`
- **Name** - Path Attribute (path to structural property) - Relative Path (relative to the containing entity/complex type)
- **Alias** - Basic Attribute (simple identifier)

#### `edm:Property`
- **Name** - Basic Attribute (simple identifier)
- **Type** - Reference Attribute (qualified type name or collection)
- **Nullable** - Basic Attribute (boolean)
- **DefaultValue** - Basic Attribute (primitive value)

#### `edm:NavigationProperty`
- **Name** - Basic Attribute (simple identifier)
- **Type** - Reference Attribute (qualified entity type name or collection) - Global Reference
- **Nullable** - Basic Attribute (boolean)
- **Partner** - Path Attribute (path to partner navigation property) - Relative Path (relative to the target entity type specified in Type attribute)
- **ContainsTarget** - Basic Attribute (boolean)

#### `edm:ReferentialConstraint`
- **Property** - Path Attribute (path to dependent property) - Relative Path (relative to the containing navigation property's source entity type)
- **ReferencedProperty** - Path Attribute (path to principal property) - Relative Path (relative to the containing navigation property's target entity type)

#### `edm:OnDelete`
- **Action** - Basic Attribute (action enum: Cascade, None, SetNull, SetDefault)

#### `edm:ComplexType`
- **Name** - Basic Attribute (simple identifier)
- **BaseType** - Reference Attribute (qualified type name) - Global Reference
- **Abstract** - Basic Attribute (boolean)
- **OpenType** - Basic Attribute (boolean)

#### `edm:EnumType`
- **Name** - Basic Attribute (simple identifier)
- **UnderlyingType** - Reference Attribute (qualified name of integer type - restricted to: `Edm.Byte`, `Edm.SByte`, `Edm.Int16`, `Edm.Int32`, or `Edm.Int64`)
- **IsFlags** - Basic Attribute (boolean)

#### `edm:Member`
- **Name** - Basic Attribute (simple identifier)
- **Value** - Basic Attribute (integer value)

#### `edm:TypeDefinition`
- **Name** - Basic Attribute (simple identifier)
- **UnderlyingType** - Reference Attribute (qualified name of primitive type - cannot be another type definition)

### Action and Function Elements

#### `edm:Action`
- **Name** - Basic Attribute (simple identifier)
- **IsBound** - Basic Attribute (boolean)
- **EntitySetPath** - Path Attribute (entity set path expression) - Relative Path (relative to the action's parameters when bound)

#### `edm:Function`
- **Name** - Basic Attribute (simple identifier)
- **IsBound** - Basic Attribute (boolean)
- **EntitySetPath** - Path Attribute (entity set path expression) - Relative Path (relative to the function's parameters when bound)
- **IsComposable** - Basic Attribute (boolean)

#### `edm:ReturnType`
- **Type** - Reference Attribute (qualified type name or collection)
- **Nullable** - Basic Attribute (boolean)

#### `edm:Parameter`
- **Name** - Basic Attribute (simple identifier)
- **Type** - Reference Attribute (qualified type name or collection)
- **Nullable** - Basic Attribute (boolean)

### Entity Container Elements

#### `edm:EntityContainer`
- **Name** - Basic Attribute (simple identifier)
- **Extends** - Reference Attribute (qualified entity container name) - Global Reference

#### `edm:EntitySet`
- **Name** - Basic Attribute (simple identifier)
- **EntityType** - Reference Attribute (qualified entity type name) - Global Reference
- **IncludeInServiceDocument** - Basic Attribute (boolean)

#### `edm:Singleton`
- **Name** - Basic Attribute (simple identifier)
- **Type** - Reference Attribute (qualified entity type name) - Global Reference
- **Nullable** - Basic Attribute (boolean)

#### `edm:NavigationPropertyBinding`
- **Path** - Path Attribute (navigation property path expression) - Relative Path (relative to the containing entity set's entity type)
- **Target** - Path Attribute (target path to entity set/singleton) - Relative Path (relative to the containing entity container)

#### `edm:ActionImport`
- **Name** - Basic Attribute (simple identifier)
- **Action** - Reference Attribute (qualified unbound action name) - Global Reference
- **EntitySet** - Reference Attribute (entity set name or target path) - Relative Reference (relative to the containing entity container)

#### `edm:FunctionImport`
- **Name** - Basic Attribute (simple identifier)
- **Function** - Reference Attribute (qualified unbound function name) - Global Reference
- **EntitySet** - Reference Attribute (entity set name or target path) - Relative Reference (relative to the containing entity container)
- **IncludeInServiceDocument** - Basic Attribute (boolean)

### Vocabulary and Annotation Elements

#### `edm:Term`
- **Name** - Basic Attribute (simple identifier)
- **Type** - Reference Attribute (qualified type name or collection)
- **Nullable** - Basic Attribute (boolean)
- **DefaultValue** - Basic Attribute (primitive value)
- **BaseTerm** - Reference Attribute (qualified term name)
- **AppliesTo** - Basic Attribute (whitespace-separated list of symbolic values)

#### `edm:Annotation`
- **Term** - Reference Attribute (qualified term name)
- **Qualifier** - Basic Attribute (simple identifier)


### Type Facet Attributes
(These can appear on various elements that reference types)

- **MaxLength** - Basic Attribute (positive integer or symbolic value: `max`)
- **Precision** - Basic Attribute (positive integer for decimal; non-negative integer 0-12 for temporal)
- **Scale** - Basic Attribute (non-negative integer or symbolic values: `floating`, `variable`)
- **Unicode** - Basic Attribute (boolean)
- **SRID** - Basic Attribute (non-negative integer or symbolic value: `variable`)



## Symbolic Values for Specific Attributes

Some Basic Attribute attributes accept specific symbolic values in addition to numeric values:

### MaxLength Attribute
- **Numeric value**: Positive integer specifying maximum length
- **Symbolic value**: `max` - shorthand for the maximum length supported by the service
- **Note**: The symbolic value `max` is only allowed in OData 4.0 responses and is deprecated in OData 4.01+

### Precision Attribute
- **For decimal values**: Positive integer (maximum number of significant decimal digits)
- **For temporal values**: Non-negative integer between 0 and 12 (decimal places in seconds portion)
- **Default**: Unspecified precision for decimal, 0 for temporal if not specified

### Scale Attribute  
- **Numeric value**: Non-negative integer specifying maximum digits to the right of decimal point
- **Symbolic values**:
  - `floating` - decimal floating-point number where precision specifies significant digits (not allowed in OData 4.0 responses)
  - `variable` - number of digits to the right of decimal point can vary from zero to the precision value
- **Constraint**: Scale value MUST be less than or equal to Precision value
- **Default**: 0 if not specified
- **Note**: Services should use lower-case values; clients should accept values case-insensitively

### SRID Attribute
- **Numeric value**: Non-negative integer identifying spatial reference system
- **Symbolic value**: `variable` - allows different spatial reference systems
- **Default values**: `0` for Geometry types, `4326` for Geography types if not specified



## Annotations

Annotations in OData are values associated with model elements to provide additional metadata. They are used to specify constraints, documentation, or other information about the model. 

They can be static or dynamic, and can be applied to various model elements like entity types, properties, actions, functions, etc.

### Annotation Expression Elements

#### Constant Expressions
- **`edm:Binary`** - Basic Attribute (binary value)
- **`edm:Bool`** - Basic Attribute (boolean literal)
- **`edm:Date`** - Basic Attribute (date value)
- **`edm:DateTimeOffset`** - Basic Attribute (datetime value)
- **`edm:Decimal`** - Basic Attribute (decimal value)
- **`edm:Duration`** - Basic Attribute (duration value)
- **`edm:EnumMember`** - Reference Attribute (qualified enum member name)
- **`edm:Float`** - Basic Attribute (floating-point value)
- **`edm:Guid`** - Basic Attribute (GUID value)
- **`edm:Int`** - Basic Attribute (integer value)
- **`edm:String`** - Basic Attribute (string value)
- **`edm:TimeOfDay`** - Basic Attribute (time value)

#### Path Expressions
- **`edm:AnnotationPath`** - Path Attribute (annotation path expression)
- **`edm:ModelElementPath`** - Path Attribute (model element path expression)
- **`edm:NavigationPropertyPath`** - Path Attribute (navigation property path expression)
- **`edm:PropertyPath`** - Path Attribute (property path expression)
- **`edm:Path`** - Path Attribute (value path expression)

#### Dynamic Expressions
- **`edm:And`** - No attributes (logical operator)
- **`edm:Or`** - No attributes (logical operator)
- **`edm:Not`** - No attributes (logical operator)
- **`edm:Eq`** - No attributes (comparison operator)
- **`edm:Ne`** - No attributes (comparison operator)
- **`edm:Gt`** - No attributes (comparison operator)
- **`edm:Ge`** - No attributes (comparison operator)
- **`edm:Lt`** - No attributes (comparison operator)
- **`edm:Le`** - No attributes (comparison operator)
- **`edm:Has`** - No attributes (flag test operator)
- **`edm:In`** - No attributes (membership test operator)
- **`edm:Neg`** - No attributes (arithmetic negation)
- **`edm:Add`** - No attributes (arithmetic operator)
- **`edm:Sub`** - No attributes (arithmetic operator)
- **`edm:Mul`** - No attributes (arithmetic operator)
- **`edm:Div`** - No attributes (arithmetic operator)
- **`edm:DivBy`** - No attributes (arithmetic operator)
- **`edm:Mod`** - No attributes (arithmetic operator)

#### Other Dynamic Expressions
- **`edm:Apply`**
  - **Function** - Reference Attribute (function name)
- **`edm:Cast`**
  - **Type** - Reference Attribute (qualified type name)
- **`edm:Collection`** - No attributes
- **`edm:If`** - No attributes
- **`edm:IsOf`**
  - **Type** - Reference Attribute (qualified type name)
- **`edm:LabeledElement`**
  - **Name** - Basic Attribute (simple identifier)
- **`edm:LabeledElementReference`** - No attributes
- **`edm:Null`** - No attributes
- **`edm:Record`**
  - **Type** - Reference Attribute (qualified structured type name)
- **`edm:PropertyValue`**
  - **Property** - Basic Attribute (property name)
- **`edm:UrlRef`** - No attributes

---

## Summary by Property Type

### Basic Properties (92 total)
Simple values like identifiers, booleans, numbers, and string literals

### Reference Properties (26 total)
Qualified names that reference other model elements like types, terms, actions, functions, containers.
Some references are "global", fully qualified names, while others are relative to the elements parent or other ancestor.


### Path Properties (12 total)
Path expressions that navigate through model structures:
- AnnotationPath, ModelElementPath, NavigationPropertyPath, PropertyPath, Path
- Target paths in Annotations and NavigationPropertyBinding
- Entity set paths in Actions/Functions
- Property paths in ReferentialConstraint
- Partner paths in NavigationProperty
- PropertyRef Name paths (for key properties in entities)

This classification helps understand how EDN elements relate to each other and enables proper handling of symbolic references vs. direct values in model processing.
