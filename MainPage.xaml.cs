using Plugin.Maui.Audio;
using System.IO;

namespace Caraoce
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }
        private async void OnPlayClicked(object sender, EventArgs e)
        {
            // 1. Setup the Lyrics (Mock Data)
            // We create a list of lyrics with timestamps (0s, 2s, 4s...)
            var lyrics = new List<LyricLine>
    {
        new LyricLine(0, "Music starts... 🎵"),
        new LyricLine(2, "This is the first line"),
        new LyricLine(5, "Here comes the second line"),
        new LyricLine(8, "And now we are singing!"),
        new LyricLine(12, "Karaoke is fun!")
    };

            // 2. Prepare the Audio
            var audioStream = await FileSystem.OpenAppPackageFileAsync("mysong.mp3");
            var player = AudioManager.Current.CreatePlayer(audioStream);

            // 3. Start Playing
            player.Play();
            PlayButton.Text = "Playing...";

            // 4. The Sync Loop 🔄
            // This loop runs continuously while the player is playing
            while (player.IsPlaying)
            {
                // Get the current position of the song in seconds
                double currentPosition = player.CurrentPosition;

                // Find the last lyric that has "passed" the current time
                // .LastOrDefault() finds the most recent line we should have shown
                var currentLine = lyrics.LastOrDefault(l => l.TimeSeconds <= currentPosition);

                if (currentLine != null)
                {
                    // Update the screen!
                    LyricsLabel.Text = currentLine.Text;
                }

                // Wait a tiny bit (100ms) so we don't freeze the phone
                await Task.Delay(100);
            }

            PlayButton.Text = "Play Song";
            LyricsLabel.Text = "Song Finished!";
        }
    }
    public class LyricLine
    {
        public double TimeSeconds { get; set; }
        public string Text { get; set; }

        public LyricLine(double time, string text)
        {
            TimeSeconds = time;
            Text = text;
        }
    }
}
