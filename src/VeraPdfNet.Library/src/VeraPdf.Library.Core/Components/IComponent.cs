namespace VeraPdf.Library.Components;

public interface IComponent : IDisposable
{
    ComponentDetails Details { get; }
}
