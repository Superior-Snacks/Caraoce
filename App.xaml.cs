using Microsoft.Extensions.DependencyInjection;

namespace Caraoce
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(new StartPage()));
        }
    }
}