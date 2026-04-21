namespace VeraPdf.NET.Model.Contracts.Xmp;

public interface IExtensionSchemaProperty
{
    string? Category { get; }

    bool IsCategoryValidText { get; }

    bool IsDescriptionValidText { get; }

    bool IsNameValidText { get; }

    bool IsValueTypeValidText { get; }

    bool IsValueTypeDefined { get; }
}
