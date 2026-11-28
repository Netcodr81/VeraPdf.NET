namespace VeraPdf.Library.Validation.Gf.Model.Factory.Operators;

public enum RenderingMode
{
    Fill = 0,
    Stroke = 1,
    FillStroke = 2,
    Neither = 3,
    FillClip = 4,
    StrokeClip = 5,
    FillStrokeClip = 6,
    NeitherClip = 7
}

public static class RenderingModeExtensions
{
    public static RenderingMode? GetRenderingMode(int value) =>
        value is >= 0 and <= 7 ? (RenderingMode)value : null;

    public static int GetValue(this RenderingMode mode)
    {
        Validate(mode);
        return (int)mode;
    }

    public static bool IsFill(this RenderingMode mode) =>
        mode switch
        {
            RenderingMode.Fill or RenderingMode.FillStroke or RenderingMode.FillClip or RenderingMode.FillStrokeClip
                => true,
            RenderingMode.Stroke or RenderingMode.Neither or RenderingMode.StrokeClip or RenderingMode.NeitherClip
                => false,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown text rendering mode.")
        };

    public static bool IsStroke(this RenderingMode mode) =>
        mode switch
        {
            RenderingMode.Stroke or RenderingMode.FillStroke or RenderingMode.StrokeClip or RenderingMode.FillStrokeClip
                => true,
            RenderingMode.Fill or RenderingMode.Neither or RenderingMode.FillClip or RenderingMode.NeitherClip
                => false,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown text rendering mode.")
        };

    private static void Validate(RenderingMode mode)
    {
        if (mode is < RenderingMode.Fill or > RenderingMode.NeitherClip)
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown text rendering mode.");
        }
    }
}
