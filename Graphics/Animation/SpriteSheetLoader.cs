using Desktop_Creatures.Tools.Images;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Graphics.Animation;

public static class SpriteSheetLoader
{
    public static SpriteSheet Load(
        string imagePath,
        string jsonPath)
    {
        BitmapImage sheetImage =
            AssetImageLoader.Load(imagePath);

        string fullJsonPath =
            ResolveAssetPath(jsonPath);

        if (!File.Exists(fullJsonPath))
        {
            throw new FileNotFoundException(
                $"Sprite sheet metadata not found: {fullJsonPath}",
                fullJsonPath);
        }

        string json =
            File.ReadAllText(fullJsonPath);

        var metadata =
            JsonSerializer.Deserialize<SpriteSheetMetadata>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? throw new InvalidOperationException(
                $"Could not deserialize sprite sheet metadata: {jsonPath}");

        if (metadata.Frames.Count == 0)
        {
            throw new InvalidOperationException(
                $"Sprite sheet contains no frames: {jsonPath}");
        }

        var logicalFrames =
            BuildFrames(
                sheetImage,
                metadata);

        var animations =
            BuildAnimations(
                logicalFrames,
                metadata.Meta.FrameTags);

        return new SpriteSheet(
            sheetImage,
            logicalFrames,
            animations);
    }

    public static SpriteSheet Load(
        BitmapSource spriteSheet,
        string jsonPath)
    {
        string fullJsonPath =
            ResolveAssetPath(jsonPath);

        if (!File.Exists(fullJsonPath))
        {
            throw new FileNotFoundException(
                $"Sprite sheet metadata not found: {fullJsonPath}",
                fullJsonPath);
        }

        string json =
            File.ReadAllText(fullJsonPath);

        var metadata =
            JsonSerializer.Deserialize<SpriteSheetMetadata>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? throw new InvalidOperationException(
                $"Could not deserialize sprite sheet metadata: {jsonPath}");

        if (metadata.Frames.Count == 0)
        {
            throw new InvalidOperationException(
                $"Sprite sheet contains no frames: {jsonPath}");
        }

        var logicalFrames =
            BuildFrames(
                spriteSheet,
                metadata);

        var animations =
            BuildAnimations(
                logicalFrames,
                metadata.Meta.FrameTags);

        return new SpriteSheet(
            spriteSheet,
            logicalFrames,
            animations);
    }

    private static List<SpriteFrame> BuildFrames(
        BitmapSource sheetImage,
        SpriteSheetMetadata metadata)
    {
        var frames =
            new List<SpriteFrame>(
                metadata.Frames.Count);

        // Merge Duplicates means multiple logical frames may point
        // at the same physical rectangle in the PNG.
        //
        // Cache the crop so we only create that BitmapSource once.
        var cropCache =
            new Dictionary<FrameRectangle, BitmapSource>();

        foreach (var frameData in metadata.Frames)
        {
            if (frameData.Rotated)
            {
                throw new NotSupportedException(
                    $"Rotated sprite-sheet frames are not currently supported. " +
                    $"Frame: {frameData.Filename}");
            }

            var rect =
                frameData.Frame;

            if (!cropCache.TryGetValue(
                    rect,
                    out var frameImage))
            {
                frameImage =
                    CropFrame(
                        sheetImage,
                        rect);

                cropCache[rect] =
                    frameImage;
            }

            frames.Add(
                new SpriteFrame(
                    frameData.Filename,
                    frameImage,
                    frameData.Duration));
        }

        return frames;
    }

    private static BitmapSource CropFrame(
        BitmapSource sheetImage,
        FrameRectangle frame)
    {
        var rectangle =
            new Int32Rect(
                frame.X,
                frame.Y,
                frame.Width,
                frame.Height);

        ValidateRectangle(
            sheetImage,
            rectangle);

        var cropped =
            new CroppedBitmap(
                sheetImage,
                rectangle);

        cropped.Freeze();

        return cropped;
    }

    private static void ValidateRectangle(
        BitmapSource sheetImage,
        Int32Rect rectangle)
    {
        bool invalid =
            rectangle.X < 0 ||
            rectangle.Y < 0 ||
            rectangle.Width <= 0 ||
            rectangle.Height <= 0 ||
            rectangle.X + rectangle.Width >
                sheetImage.PixelWidth ||
            rectangle.Y + rectangle.Height >
                sheetImage.PixelHeight;

        if (invalid)
        {
            throw new InvalidOperationException(
                $"Sprite frame rectangle " +
                $"({rectangle.X}, {rectangle.Y}, " +
                $"{rectangle.Width}, {rectangle.Height}) " +
                $"is outside sheet bounds " +
                $"{sheetImage.PixelWidth}x{sheetImage.PixelHeight}.");
        }
    }

    private static Dictionary<string, SpriteAnimation>
        BuildAnimations(
            IReadOnlyList<SpriteFrame> allFrames,
            IReadOnlyList<SpriteSheetTagData> tags)
    {
        var animations =
            new Dictionary<string, SpriteAnimation>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var tag in tags)
        {
            if (tag.From < 0 ||
                tag.To < tag.From ||
                tag.To >= allFrames.Count)
            {
                throw new InvalidOperationException(
                    $"Animation tag '{tag.Name}' has invalid " +
                    $"frame range {tag.From}-{tag.To}.");
            }

            var tagFrames =
                allFrames
                    .Skip(tag.From)
                    .Take(
                        tag.To -
                        tag.From +
                        1)
                    .ToList();

            tagFrames =
                ApplyDirection(
                    tagFrames,
                    tag.Direction);

            animations[tag.Name] =
                new SpriteAnimation(
                    tag.Name,
                    tagFrames);
        }

        return animations;
    }

    private static List<SpriteFrame> ApplyDirection(
        List<SpriteFrame> frames,
        string direction)
    {
        switch (
            direction.ToLowerInvariant())
        {
            case "forward":
                return frames;

            case "reverse":
                frames.Reverse();
                return frames;

            case "pingpong":
                return CreatePingPongFrames(
                    frames,
                    reverseFirst: false);

            case "pingpong_reverse":
                frames.Reverse();

                return CreatePingPongFrames(
                    frames,
                    reverseFirst: true);

            default:
                throw new NotSupportedException(
                    $"Unsupported animation direction: {direction}");
        }
    }

    private static List<SpriteFrame> CreatePingPongFrames(
        List<SpriteFrame> frames,
        bool reverseFirst)
    {
        if (frames.Count <= 2)
            return frames;

        var result =
            new List<SpriteFrame>(
                frames);

        // Don't repeat the two endpoints:
        //
        // 0 1 2 3
        // becomes
        // 0 1 2 3 2 1
        //
        // rather than
        // 0 1 2 3 3 2 1 0
        result.AddRange(
            frames
                .Skip(1)
                .Take(frames.Count - 2)
                .Reverse());

        return result;
    }

    private static string ResolveAssetPath(
        string path)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            path.Replace(
                '/',
                Path.DirectorySeparatorChar));
    }
}

// ============================================================
// Runtime sprite-sheet model
// ============================================================

public sealed class SpriteSheet
{
    public BitmapSource Image { get; }

    public IReadOnlyList<SpriteFrame> Frames { get; }

    public IReadOnlyDictionary<string, SpriteAnimation>
        Animations
    { get; }

    private readonly IReadOnlyDictionary<
        string,
        SpriteFrame> _framesByName;

    internal SpriteSheet(
        BitmapSource image,
        IReadOnlyList<SpriteFrame> frames,
        IReadOnlyDictionary<string, SpriteAnimation> animations)
    {
        Image = image;
        Frames = frames;
        Animations = animations;

        _framesByName =
            frames.ToDictionary(
                frame => frame.Name,
                StringComparer.OrdinalIgnoreCase);
    }

    public SpriteFrame GetFrame(
        string name)
    {
        if (!_framesByName.TryGetValue(
                name,
                out SpriteFrame? frame))
        {
            throw new KeyNotFoundException(
                $"Sprite frame '{name}' was not found. " +
                $"Available frames: " +
                $"{string.Join(", ", _framesByName.Keys)}");
        }

        return frame;
    }

    public bool TryGetFrame(
        string name,
        out SpriteFrame? frame)
    {
        return _framesByName.TryGetValue(
            name,
            out frame);
    }

    public SpriteAnimation GetAnimation(
        string name)
    {
        if (!Animations.TryGetValue(
                name,
                out var animation))
        {
            throw new KeyNotFoundException(
                $"Animation '{name}' was not found. " +
                $"Available animations: " +
                $"{string.Join(", ", Animations.Keys)}");
        }

        return animation;
    }

    public bool TryGetAnimation(
        string name,
        out SpriteAnimation? animation)
    {
        return Animations.TryGetValue(
            name,
            out animation);
    }
}

public sealed class SpriteAnimation
{
    public string Name { get; }

    public IReadOnlyList<SpriteFrame> Frames { get; }

    public int FrameCount =>
        Frames.Count;

    internal SpriteAnimation(
        string name,
        IReadOnlyList<SpriteFrame> frames)
    {
        Name = name;
        Frames = frames;
    }
}

public sealed class SpriteFrame
{
    public string Name { get; }

    public BitmapSource Image { get; }

    public int DurationMilliseconds { get; }

    internal SpriteFrame(
        string name,
        BitmapSource image,
        int durationMilliseconds)
    {
        Name = name;
        Image = image;
        DurationMilliseconds =
            durationMilliseconds;
    }
}

// ============================================================
// JSON metadata model
//
// Matches Aseprite's:
//   JSON Data: Array
//   Array Data: Tags
// ============================================================

internal sealed class SpriteSheetMetadata
{
    [JsonPropertyName("frames")]
    public List<SpriteSheetFrameData> Frames { get; set; } = [];

    [JsonPropertyName("meta")]
    public SpriteSheetMetaData Meta { get; set; } = new();
}

internal sealed class SpriteSheetFrameData
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = "";

    [JsonPropertyName("frame")]
    public FrameRectangle Frame { get; set; }

    [JsonPropertyName("rotated")]
    public bool Rotated { get; set; }

    [JsonPropertyName("trimmed")]
    public bool Trimmed { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }
}

internal readonly record struct FrameRectangle
{
    [JsonPropertyName("x")]
    public int X { get; init; }

    [JsonPropertyName("y")]
    public int Y { get; init; }

    [JsonPropertyName("w")]
    public int Width { get; init; }

    [JsonPropertyName("h")]
    public int Height { get; init; }
}

internal sealed class SpriteSheetMetaData
{
    [JsonPropertyName("image")]
    public string Image { get; set; } = "";

    [JsonPropertyName("size")]
    public SpriteSheetSize Size { get; set; }

    [JsonPropertyName("frameTags")]
    public List<SpriteSheetTagData> FrameTags { get; set; } = [];
}

internal readonly record struct SpriteSheetSize
{
    [JsonPropertyName("w")]
    public int Width { get; init; }

    [JsonPropertyName("h")]
    public int Height { get; init; }
}

internal sealed class SpriteSheetTagData
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("from")]
    public int From { get; set; }

    [JsonPropertyName("to")]
    public int To { get; set; }

    [JsonPropertyName("direction")]
    public string Direction { get; set; } =
        "forward";
}