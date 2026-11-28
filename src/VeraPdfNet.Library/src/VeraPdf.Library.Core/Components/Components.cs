namespace VeraPdf.Library.Components;

public static class Components
{
    public static ComponentDetails DefaultDetails => ComponentDetails.Default;
    public static AuditDuration DefaultDuration => AuditDuration.Default;

    public static ComponentDetails VeraDetails(Uri id, string name, string version, string description) =>
        ComponentDetails.FromValues(id, name, version, "The veraPDF Consortium.", description);

    public static ComponentDetails DetailsFromValues(
        Uri id, string name, string version, string provider, string description) =>
        ComponentDetails.FromValues(id, name, version, provider, description);

    public sealed class Timer
    {
        private readonly long _start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        private Timer()
        {
        }

        public AuditDuration Stop() =>
            AuditDuration.FromValues(_start, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        public static Timer Start() => new();
    }
}
