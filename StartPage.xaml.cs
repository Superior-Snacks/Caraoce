namespace Caraoce;

public partial class StartPage : ContentPage
{
    public StartPage()
    {
        InitializeComponent();
    }

    private void OnShuffleClicked(object sender, EventArgs e)
    {
        // 1. Define our Library (Eventually this will come from a database)
        var songLibrary = new List<KaraokeSong>
    {
        new KaraokeSong
        {
            Title = "My Test Song",
            Artist = "Me",
            AudioFilename = "testSound3.mp3",
            LrcFilename = "testSound3.lrc"
        },
        // You can add more songs here once you drag the files into Resources/Raw!
    };

        // 2. Pick a random song
        var random = new Random();
        var songToPlay = songLibrary[random.Next(songLibrary.Count)];

        // 3. Navigate to the Player, PASSING the song!
        Navigation.PushAsync(new MainPage(songToPlay));
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