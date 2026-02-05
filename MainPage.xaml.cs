using Plugin.Maui.Audio;
using System.Linq;

namespace Caraoce;

public partial class MainPage : ContentPage
{
    private IAudioPlayer player;
    private bool isPlaying = false;
    private List<LyricLine> lyrics;

    public MainPage()
    {
        InitializeComponent();

        // Define lyrics immediately
        lyrics = new List<LyricLine>
        {
            new LyricLine(0, "Get Ready... 🎤"),
            new LyricLine(2, "This is the horizontal view!"),
            new LyricLine(5, "Lines slide into place..."),
            new LyricLine(8, "Sing it loud and clear!"),
            new LyricLine(12, "Karaoke master!")
        };

        // Feed the data to the Carousel
        LyricsCarousel.ItemsSource = lyrics;
    }

    // This method runs automatically when the page appears on screen
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await StartKaraoke();
    }

    // This runs when you leave the page (hits back button)
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopKaraoke();
    }

    private async Task StartKaraoke()
    {
        StatusLabel.Text = "Loading Audio...";

        var audioStream = await FileSystem.OpenAppPackageFileAsync("testSound3.mp3");
        player = AudioManager.Current.CreatePlayer(audioStream);

        player.Play();
        isPlaying = true;
        StatusLabel.Text = "Now Playing 🎵";

        // The Sync Loop
        while (isPlaying && player.IsPlaying)
        {
            double currentPosition = player.CurrentPosition;

            // Find the index of the current line
            var currentLine = lyrics.LastOrDefault(l => l.TimeSeconds <= currentPosition);

            if (currentLine != null)
            {
                // Scroll the carousel to this line automatically!
                LyricsCarousel.Position = lyrics.IndexOf(currentLine);
            }

            await Task.Delay(100);
        }
    }

    private void StopKaraoke()
    {
        isPlaying = false;
        if (player != null)
        {
            player.Dispose(); // Kills the audio so it doesn't play in the background
        }
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        // Pop this page off the stack (go back to menu)
        Navigation.PopAsync();
    }
}

// Keep your class definition at the bottom
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