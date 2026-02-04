using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace Caraoce
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // --- THE FIX ---
            // Instead of .AddAudio(), we manually tell it to use the Current Audio Manager
            builder.Services.AddSingleton(AudioManager.Current);

            return builder.Build();
        }
    }
}