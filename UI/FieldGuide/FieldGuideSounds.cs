using System.IO;
using System.Media;

namespace Desktop_Creatures.UI.FieldGuide
{
    public sealed class FieldGuideSounds
    {
        private readonly Random _random = new();

        private readonly string[] _pageFlips =
        [
            "page_flip_01.wav",
            "page_flip_02.wav",
            "page_flip_03.wav",
            "page_flip_04.wav",
            "page_flip_05.wav",
            "page_flip_06.wav"
        ];

        private readonly string[] _bookCloses =
        [
            "book_close_01.wav",
            "book_close_02.wav",
            "book_close_03.wav",
            "book_close_04.wav"
        ];

        private readonly string[] _bookOpens =
        [
            "book_open_01.wav",
            "book_open_02.wav",
            "book_open_03.wav",
            "book_open_04.wav"
        ];

        public void PlayBookOpen() =>
            PlayRandom(_bookOpens);

        public void PlayPageFlip() =>
            PlayRandom(_pageFlips);

        public void PlayBookClose() =>
            PlayRandom(_bookCloses);

        private void PlayRandom(string[] sounds)
        {
            string file = sounds[
                _random.Next(sounds.Length)];

            string path = Path.Combine(
                AppContext.BaseDirectory,
                    "Assets",
                    "Sounds",
                    "UI",
                    "FieldGuide",
                    file);

            new SoundPlayer(path).Play();
        }
    }
}
