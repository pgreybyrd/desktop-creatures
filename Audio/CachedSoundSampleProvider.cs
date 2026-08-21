using NAudio.Wave;

namespace Desktop_Creatures.Audio;

public sealed class CachedSoundSampleProvider :
    ISampleProvider
{
    private readonly CachedSound _cachedSound;

    private long _position;

    public CachedSoundSampleProvider(
        CachedSound cachedSound)
    {
        _cachedSound =
            cachedSound;
    }

    public WaveFormat WaveFormat =>
        _cachedSound.WaveFormat;

    public int Read(
        float[] buffer,
        int offset,
        int count)
    {
        long availableSamples =
            _cachedSound.AudioData.Length -
            _position;

        int samplesToCopy =
            (int)Math.Min(
                availableSamples,
                count);

        Array.Copy(
            _cachedSound.AudioData,
            _position,
            buffer,
            offset,
            samplesToCopy);

        _position +=
            samplesToCopy;

        return samplesToCopy;
    }
}