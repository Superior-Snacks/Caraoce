using Plugin.Maui.Audio;
using System.Linq;
#if ANDROID
using Android.Content.PM; // Required for screen rotation
#endif

namespace Caraoce;

public partial class MainPage : ContentPage
{
    private IAudioPlayer player;
    private bool isPlaying = false;
    private bool isDraggingSlider = false; // Prevents stutter while dragging
    private List<LyricLine> lyrics;
    private KaraokeSong currentSong;

    public MainPage(KaraokeSong songToPlay)
    {
        InitializeComponent();
        currentSong = songToPlay;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Force Landscape Mode (Android Only)
#if ANDROID
        if (Platform.CurrentActivity != null)
        {
            Platform.CurrentActivity.RequestedOrientation = ScreenOrientation.Landscape;
        }
#endif

        await StartKaraoke();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopKaraoke();

        // 2. Reset to Portrait when leaving (Android Only)
#if ANDROID
        if (Platform.CurrentActivity != null)
        {
            Platform.CurrentActivity.RequestedOrientation = ScreenOrientation.Unspecified;
        }
#endif
    }

    private async Task StartKaraoke()
    {
        // ... (Lyric parsing code same as before) ...
        // Re-paste your ParseLrcFile logic here or ensure it's in the file
        // For brevity, I'm skipping the parsing lines, assuming you kept them!

        // Load Audio
        var audioStream = await FileSystem.OpenAppPackageFileAsync(currentSong.AudioFilename);
        player = AudioManager.Current.CreatePlayer(audioStream);

        // Set Total Time Label
        TotalTimeLabel.Text = TimeSpan.FromSeconds(player.Duration).ToString(@"m\:ss");

        player.Play();
        isPlaying = true;

        // Start the Loop
        _ = Dispatcher.DispatchAsync(UpdateLoop);
    }

    private async Task UpdateLoop()
    {
        while (isPlaying && player != null)
        {
            if (player.IsPlaying && !isDraggingSlider)
            {
                double currentPosition = player.CurrentPosition;
                double totalDuration = player.Duration;

                // Update Slider (0.0 to 1.0)
                PositionSlider.Value = currentPosition / totalDuration;
                CurrentTimeLabel.Text = TimeSpan.FromSeconds(currentPosition).ToString(@"m\:ss");

                // Sync Lyrics
                var currentLine = lyrics.LastOrDefault(l => l.TimeSeconds <= currentPosition);
                if (currentLine != null)
                {
                    LyricsCarousel.Position = lyrics.IndexOf(currentLine);
                }
            }

            await Task.Delay(100);
        }
    }

    // --- CONTROLS ---

    private void OnPlayPauseClicked(object sender, EventArgs e)
    {
        if (player.IsPlaying)
        {
            player.Pause();
            PlayPauseButton.Text = "▶"; // Play Icon
        }
        else
        {
            player.Play();
            PlayPauseButton.Text = "⏸"; // Pause Icon
        }
    }

    private void OnSliderDragStarted(object sender, EventArgs e)
    {
        isDraggingSlider = true; // Stop the loop from fighting the user
    }

    private void OnSliderDragCompleted(object sender, EventArgs e)
    {
        if (player != null)
        {
            // Calculate new time: 0.5 * 180 seconds = 90 seconds
            double newTime = PositionSlider.Value * player.Duration;
            player.Seek(newTime);
        }
        isDraggingSlider = false; // Resume the loop
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
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