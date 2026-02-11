using Plugin.Maui.Audio;
#if ANDROID
using Android.Content.PM; // Required for screen rotation
using Android.Views;
#endif

namespace Caraoce;

public partial class MainPage : ContentPage
{
    private IAudioPlayer player;
    private bool isPlaying = false;
    private bool isDraggingSlider = false;
    private List<LyricLine> lyrics = new List<LyricLine>(); // Initialize to avoid null errors
    private KaraokeSong currentSong;
    private DateTime lastInteractionTime;

    public MainPage(KaraokeSong songToPlay)
    {
        InitializeComponent();
        currentSong = songToPlay;
    }
    private async void PlayNextRandomSong()
    {
        // 1. Get the list of songs
        var allSongs = await SongRepository.GetAllSongsAsync();

        // 2. Pick a random one
        if (allSongs.Count > 0)
        {
            var random = new Random();
            var nextSong = allSongs[random.Next(allSongs.Count)];

            // Optional: Ensure we don't play the exact same song twice in a row
            if (allSongs.Count > 1 && nextSong.Title == currentSong.Title)
            {
                // Pick again if it's the same song
                nextSong = allSongs[random.Next(allSongs.Count)];
            }

            // 3. Switch the data
            currentSong = nextSong;

            // 4. Clean up the old song
            StopKaraoke();

            // 5. Start the new one!
            await StartKaraoke();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

#if ANDROID
        if (Platform.CurrentActivity?.Window != null)
        {
            // 1. Force Landscape
            Platform.CurrentActivity.RequestedOrientation = ScreenOrientation.Landscape;

            // 2. Define the "Immersive Mode" flags
            // This huge chunk tells Android: "Hide everything, and if the user swipes 
            // to bring it back, hide it again automatically after a few seconds."
            var uiOptions = (int)Android.Views.SystemUiFlags.LayoutStable |
                            (int)Android.Views.SystemUiFlags.LayoutHideNavigation |
                            (int)Android.Views.SystemUiFlags.LayoutFullscreen |
                            (int)Android.Views.SystemUiFlags.HideNavigation | // Hides bottom bar
                            (int)Android.Views.SystemUiFlags.Fullscreen |     // Hides top bar
                            (int)Android.Views.SystemUiFlags.ImmersiveSticky; // Auto-hides after swipe

            // 3. Apply the flags
            Platform.CurrentActivity.Window.DecorView.SystemUiVisibility = (Android.Views.StatusBarVisibility)uiOptions;
        }
#endif

        await StartKaraoke();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopKaraoke();

#if ANDROID
        if (Platform.CurrentActivity?.Window != null)
        {
            // 1. Reset Orientation to Portrait
            Platform.CurrentActivity.RequestedOrientation = ScreenOrientation.Unspecified;

            // 2. Clear the flags (Bring bars back)
            // Setting visibility to 'Visible' resets everything to default
            Platform.CurrentActivity.Window.DecorView.SystemUiVisibility = Android.Views.StatusBarVisibility.Visible;
        }
#endif
    }

    private async Task StartKaraoke()
    {
        StatusLabel.Text = "Loading...";
        lastInteractionTime = DateTime.Now;
        UiOverlay.Opacity = 1;
        UiOverlay.InputTransparent = false;

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

        player.PlaybackEnded += (s, e) =>
        {
            // When song ends, run the next song logic on the main UI thread
            Dispatcher.Dispatch(() => PlayNextRandomSong());
        };

        // Set Total Time Label
        TotalTimeLabel.Text = TimeSpan.FromSeconds(player.Duration).ToString(@"m\:ss");

        player.Play();
        isPlaying = true;
        StatusLabel.Text = currentSong.Title;

        // Start the Loop
        _ = Dispatcher.DispatchAsync(UpdateLoop);
    }

    // --- C. THE MISSING METHOD ---
    private void StopKaraoke()
    {
        isPlaying = false;

        if (player != null)
        {
            // Important: Remove the event listener so it doesn't fire twice
            player.PlaybackEnded -= (s, e) => { };
            player.Dispose();
        }
        // Reset UI for the next song
        PositionSlider.Value = 0;
        CurrentTimeLabel.Text = "0:00";
    }

    private async Task UpdateLoop()
    {
        while (isPlaying && player != null)
        {
            // --- NEW AUTO-HIDE LOGIC ---
            // Check how much time passed since last touch
            var timeSinceTouch = DateTime.Now - lastInteractionTime;

            // If controls are visible AND it's been more than 2 seconds AND we aren't dragging the slider
            if (UiOverlay.Opacity == 1 && timeSinceTouch.TotalSeconds > 2 && !isDraggingSlider)
            {
                // Fade out
                await UiOverlay.FadeTo(0, 500);
                UiOverlay.InputTransparent = true; // Disable buttons so you don't click invisible things
            }
            // ---------------------------

            if (player.IsPlaying && !isDraggingSlider)
            {
                // ... (Your existing slider/lyric update code stays here) ...
                double currentPosition = player.CurrentPosition;
                double totalDuration = player.Duration;

                if (totalDuration > 0)
                {
                    PositionSlider.Value = currentPosition / totalDuration;
                    CurrentTimeLabel.Text = TimeSpan.FromSeconds(currentPosition).ToString(@"m\:ss");
                }

                var currentLine = lyrics.LastOrDefault(l => l.TimeSeconds <= currentPosition);
                if (currentLine != null)
                {
                    LyricsCarousel.Position = lyrics.IndexOf(currentLine);
                }
            }

            await Task.Delay(100);
        }
    }

    private async void OnScreenTapped(object sender, EventArgs e)
    {
        lastInteractionTime = DateTime.Now;

        if (UiOverlay.Opacity < 1.0)
        {
            UiOverlay.InputTransparent = false;
            UiOverlay.CancelAnimations();
            await UiOverlay.FadeTo(1, 250);
        }
    }


    // --- CONTROLS ---

    private void OnPlayPauseClicked(object sender, EventArgs e)
    {
        lastInteractionTime = DateTime.Now;
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
        lastInteractionTime = DateTime.Now;
        isDraggingSlider = true;
    }

    private void OnSliderDragCompleted(object sender, EventArgs e)
    {
        lastInteractionTime = DateTime.Now;
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
    private static List<LyricLine> ParseLrcFile(string lrcContent)
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