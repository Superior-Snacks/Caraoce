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

        try
        {
            // 1. Open the .lrc file
            using var stream = await FileSystem.OpenAppPackageFileAsync("mysong.lrc");
            using var reader = new StreamReader(stream);
            var fileContent = await reader.ReadToEndAsync();

            // 2. Convert text to Lyric objects using our new tool
            lyrics = ParseLrcFile(fileContent);

            // 3. Update the UI
            LyricsCarousel.ItemsSource = lyrics;
        }
        catch (Exception ex)
        {
            // Fail-safe if file is missing
            StatusLabel.Text = "Error loading lyrics!";
            return;
        }

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

    private List<LyricLine> ParseLrcFile(string lrcContent)
    {
        var lines = new List<LyricLine>();

        // Split the file into individual lines
        var fileLines = lrcContent.Split('\n');

        foreach (var line in fileLines)
        {
            // specific format: [00:12.34] Lyrics here
            if (line.StartsWith("[") && line.Contains("]"))
            {
                try
                {
                    // 1. Cut out the timestamp: "[00:12.34]" -> "00:12.34"
                    var timePart = line.Substring(1, line.IndexOf("]") - 1);

                    // 2. Cut out the text: "Lyrics here"
                    var textPart = line.Substring(line.IndexOf("]") + 1).Trim();

                    // 3. Convert time to seconds
                    // Split "00:12.34" into minutes and seconds
                    var timeParts = timePart.Split(':');
                    double minutes = double.Parse(timeParts[0]);
                    double seconds = double.Parse(timeParts[1]);

                    double totalSeconds = (minutes * 60) + seconds;

                    lines.Add(new LyricLine(totalSeconds, textPart));
                }
                catch
                {
                    // If a line is formatted weirdly, just skip it to prevent crashing
                    continue;
                }
            }
        }
        return lines;
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