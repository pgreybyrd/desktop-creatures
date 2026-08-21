using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Desktop_Creatures.Audio;

public sealed class CachedSound
{
    public float[] AudioData { get; }

    public WaveFormat WaveFormat { get; }

    public CachedSound(
        string filePath,
        WaveFormat targetFormat)
    {
        using var reader =
            new AudioFileReader(filePath);

        ISampleProvider provider = reader;

        if (provider.WaveFormat.SampleRate !=
            targetFormat.SampleRate)
        {
            provider =
                new WdlResamplingSampleProvider(
                    provider,
                    targetFormat.SampleRate);
        }

        if (provider.WaveFormat.Channels == 1 &&
            targetFormat.Channels == 2)
        {
            provider =
                new MonoToStereoSampleProvider(
                    provider);
        }

        if (provider.WaveFormat.Channels !=
            targetFormat.Channels)
        {
            throw new InvalidOperationException(
                $"Unsupported channel conversion: " +
                $"{provider.WaveFormat.Channels} -> " +
                $"{targetFormat.Channels}");
        }

        WaveFormat =
            targetFormat;

        var samples =
            new List<float>();

        float[] buffer =
            new float[
                targetFormat.SampleRate *
                targetFormat.Channels];

        int samplesRead;

        while ((samplesRead =
            provider.Read(
                buffer,
                0,
                buffer.Length)) > 0)
        {
            samples.AddRange(
                buffer.AsSpan(
                    0,
                    samplesRead)
                .ToArray());
        }

        AudioData =
            samples.ToArray();
    }
}