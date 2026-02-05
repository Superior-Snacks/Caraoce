namespace Caraoce;

public partial class SongListPage : ContentPage
{
    private List<KaraokeSong> allSongs;

    public SongListPage()
    {
        InitializeComponent();
        LoadSongs();
    }
    private async void LoadSongs()
    {
        // Wait for the file to be read
        allSongs = await SongRepository.GetAllSongsAsync();
        SongsCollection.ItemsSource = allSongs;
    }

    // Handle clicking a song
    private async void OnSongSelected(object sender, SelectionChangedEventArgs e)
    {
        // Get the item that was clicked
        var selectedSong = e.CurrentSelection.FirstOrDefault() as KaraokeSong;

        if (selectedSong != null)
        {
            // Clear selection so you can click it again later if you want
            SongsCollection.SelectedItem = null;

            // Navigate to Player with the chosen song!
            await Navigation.PushAsync(new MainPage(selectedSong));
        }
    }

    // Handle Search
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchTerm = e.NewTextValue.ToLower();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            SongsCollection.ItemsSource = allSongs;
        }
        else
        {
            // Filter the list based on Title or Artist
            SongsCollection.ItemsSource = allSongs
                .Where(s => s.Title.ToLower().Contains(searchTerm) ||
                            s.Artist.ToLower().Contains(searchTerm))
                .ToList();
        }
    }
}