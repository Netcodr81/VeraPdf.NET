using VeraPdf.Model.BaseLayer;

namespace VeraPdf.Model;

/// <summary>Minimal upstream-compatible model root support; not a PDF parser adapter.</summary>
/// <remarks>
/// Subclasses implement their scalar getters and named-link access.
/// This class only provides schema names and the upstream empty context defaults.
/// </remarks>
public abstract class GenericModelObject : IModelObject
{
    private readonly string _objectType;

    /// <summary>Initializes a known original simple model type name.</summary>
    protected GenericModelObject(string objectType)
    {
        _ = ModelHelper.GetDefinition(objectType);
        _objectType = objectType;
    }

    /// <summary>False by default; set to null while dependence has not yet been established.</summary>
    protected bool? ContextDependent { get; set; } = false;

    /// <inheritdoc />
    public string GetObjectType() => _objectType;
    /// <inheritdoc />
    public virtual string? GetID() => null;
    /// <inheritdoc />
    public virtual string? GetExtraContext() => null;
    /// <inheritdoc />
    public virtual string? GetContext() => null;
    /// <inheritdoc />
    public virtual bool? IsContextDependent() => ContextDependent;
    /// <inheritdoc />
    public virtual IReadOnlyList<string> GetLinks() => ModelHelper.GetListOfLinks(_objectType);
    /// <inheritdoc />
    public virtual IReadOnlyList<string> GetProperties() => ModelHelper.GetListOfProperties(_objectType);
    /// <inheritdoc />
    public virtual IReadOnlyList<string> GetSuperTypes() => ModelHelper.GetListOfSuperNames(_objectType);
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">No implementation of the requested traversal is supplied.</exception>
    public virtual IReadOnlyList<IModelObject> GetLinkedObjects(string linkName) =>
        throw new InvalidOperationException($"Model '{_objectType}' does not implement link access for '{linkName}'.");
}
