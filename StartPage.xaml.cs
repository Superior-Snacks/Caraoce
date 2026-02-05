namespace Caraoce;

public partial class StartPage : ContentPage
{
    public StartPage()
    {
        InitializeComponent();
    }

    private async void OnShuffleClicked(object sender, EventArgs e)
    {
        // Load the songs dynamically
        var songLibrary = await SongRepository.GetAllSongsAsync();

        if (songLibrary.Count == 0)
        {
            await DisplayAlertAsync("Error", "No songs found in songs.json!", "OK");
            return;
        }

        // Pick random
        var random = new Random();
        var songToPlay = songLibrary[random.Next(songLibrary.Count)];

        await Navigation.PushAsync(new MainPage(songToPlay));
    }

    private void OnSelectClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SongListPage());
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