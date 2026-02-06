using Plugin.Maui.Audio;
#if ANDROID
using Android.Content.PM; // Required for screen rotation
#endif

namespace Caraoce;

public partial class MainPage : ContentPage
{
    private IAudioPlayer player;
    private bool isPlaying = false;
    private bool isDraggingSlider = false;
    private List<LyricLine> lyrics = new List<LyricLine>(); // Initialize to avoid null errors
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
        StopKaraoke(); // <--- This now exists!

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
        StatusLabel.Text = "Loading...";

        // --- A. LOAD LYRICS ---
        try
        {
            // Read the LRC file from the raw resources
            using var lrcStream = await FileSystem.OpenAppPackageFileAsync(currentSong.LrcFilename);
            using var reader = new StreamReader(lrcStream);
            var fileContent = await reader.ReadToEndAsync();

            // Parse them
            lyrics = ParseLrcFile(fileContent);
            LyricsCarousel.ItemsSource = lyrics;
        }
        catch
        {
            // Fallback if no .lrc file found
            lyrics = new List<LyricLine> { new LyricLine(0, "No lyrics found for this song.") };
            LyricsCarousel.ItemsSource = lyrics;
        }

        // --- B. LOAD AUDIO ---
        var audioStream = await FileSystem.OpenAppPackageFileAsync(currentSong.AudioFilename);
        player = AudioManager.Current.CreatePlayer(audioStream);

        // Set Total Time Label
        TotalTimeLabel.Text = TimeSpan.FromSeconds(player.Duration).ToString(@"m\:ss");

        player.Play();
        isPlaying = true;
        StatusLabel.Text = currentSong.Title; // Show Title in the label

        // Start the Loop
        _ = Dispatcher.DispatchAsync(UpdateLoop);
    }

    // --- C. THE MISSING METHOD ---
    private void StopKaraoke()
    {
        isPlaying = false;
        if (player != null)
        {
            player.Dispose(); // Cleans up the audio engine
        }
    }

    private async Task UpdateLoop()
    {
        while (isPlaying && player != null)
        {
            if (player.IsPlaying && !isDraggingSlider)
            {
                double currentPosition = player.CurrentPosition;
                double totalDuration = player.Duration;

                // Update Slider (0.0 to 1.0) and Time Label
                if (totalDuration > 0)
                {
                    PositionSlider.Value = currentPosition / totalDuration;
                    CurrentTimeLabel.Text = TimeSpan.FromSeconds(currentPosition).ToString(@"m\:ss");
                }

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
        isDraggingSlider = true;
    }

    private void OnSliderDragCompleted(object sender, EventArgs e)
    {
        if (player != null)
        {
            double newTime = PositionSlider.Value * player.Duration;
            player.Seek(newTime);
        }
        isDraggingSlider = false;
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    // --- PARSER ---
    private List<LyricLine> ParseLrcFile(string lrcContent)
    {
        var lines = new List<LyricLine>();
        var fileLines = lrcContent.Split('\n');

        foreach (var line in fileLines)
        {
            if (line.StartsWith("[") && line.Contains("]"))
            {
                try
                {
                    var timePart = line.Substring(1, line.IndexOf("]") - 1);
                    var textPart = line.Substring(line.IndexOf("]") + 1).Trim();
                    var timeParts = timePart.Split(':');
                    double minutes = double.Parse(timeParts[0]);
                    double seconds = double.Parse(timeParts[1]);
                    lines.Add(new LyricLine((minutes * 60) + seconds, textPart));
                }
                catch { continue; }
            }
        }
        return lines;
    }
}