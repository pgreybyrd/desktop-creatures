using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;

namespace Desktop_Creatures.Audio;

public sealed class AudioEngine :
    IDisposable
{
    private static readonly Lazy<AudioEngine>
        LazyInstance =
            new(() => new AudioEngine());

    public static AudioEngine Instance =>
        LazyInstance.Value;

    private readonly WaveOutEvent _output;

    private readonly MixingSampleProvider _mixer;

    private readonly Dictionary<
        string,
        CachedSound> _cache =
            new(
                StringComparer.OrdinalIgnoreCase);

    public WaveFormat WaveFormat { get; }

    private AudioEngine()
    {
        WaveFormat =
            WaveFormat.CreateIeeeFloatWaveFormat(
                44100,
                2);

        _mixer =
            new MixingSampleProvider(
                WaveFormat)
            {
                ReadFully = true
            };

        _output =
            new WaveOutEvent
            {
                DesiredLatency = 50,
                NumberOfBuffers = 3
            };

        _output.Init(
            _mixer);

        _output.Play();
    }

    public void Preload(
        string soundId,
        string filePath)
    {
        if (_cache.ContainsKey(soundId))
            return;

        string fullPath =
            Path.IsPathRooted(filePath)
                ? filePath
                : Path.Combine(
                    AppContext.BaseDirectory,
                    filePath);

        _cache[soundId] =
            new CachedSound(
                fullPath,
                WaveFormat);
    }

    public void Play(
        string soundId,
        float volume = 1f)
    {
        if (!_cache.TryGetValue(
            soundId,
            out CachedSound? sound))
        {
            return;
        }

        var source =
            new CachedSoundSampleProvider(
                sound);

        var volumeProvider =
            new VolumeSampleProvider(
                source)
            {
                Volume =
                    Math.Clamp(
                        volume,
                        0f,
                        1f)
            };

        _mixer.AddMixerInput(
            volumeProvider);
    }

    public void Dispose()
    {
        _output.Stop();
        _output.Dispose();

        _cache.Clear();
    }
}