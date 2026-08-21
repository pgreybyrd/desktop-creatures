namespace Desktop_Creatures.Audio;

public static class UiSounds
{
    private const string ButtonClickId =
        "ui.button_click";

    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
            return;

        AudioEngine.Instance.Preload(
            ButtonClickId,
            "Assets/Sounds/UI/button_click.wav");

        _initialized = true;
    }

    public static void PlayButtonClick()
    {
        Initialize();

        AudioEngine.Instance.Play(
            ButtonClickId);
    }
}