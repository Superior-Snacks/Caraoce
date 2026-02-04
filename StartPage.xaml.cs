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