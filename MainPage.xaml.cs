using Plugin.Maui.Audio;
using System.IO;

namespace Caraoce
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }
        private async void OnPlayClicked(object sender, EventArgs e)
        {
            var audioStream = await FileSystem.OpenAppPackageFileAsync("testSound1.mp3");

            // CHANGED: No 'await', and removed 'Async' from the name
            var player = AudioManager.Current.CreatePlayer(audioStream);
            PlayButton.Text = "Playing...";
            //working?
            player.Play();
        }
    }
}
