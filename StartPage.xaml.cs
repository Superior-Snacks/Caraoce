namespace Caraoce;

public partial class StartPage : ContentPage
{
    public StartPage()
    {
        InitializeComponent();
    }

    private void OnShuffleClicked(object sender, EventArgs e)
    {
        // We will add logic here later to pick a random song
        // For now, let's just push the Lyrics page to see it work
        Navigation.PushAsync(new MainPage());
    }

    private void OnSelectClicked(object sender, EventArgs e)
    {
        // This will eventually go to a Song List page
        DisplayAlertAsync("Selection", "Song list coming soon!", "OK");
    }
}

public class KaraokeSong
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public string AudioFilename { get; set; } // e.g., "mysong.mp3"
    public string LrcFilename { get; set; }   // e.g., "mysong.lrc"
    public string CoverImage { get; set; }    // Optional: for album art later
}