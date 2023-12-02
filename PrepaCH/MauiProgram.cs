using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using PrepaCH.Pages;

namespace PrepaCH
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
                })
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitCore();
            //Added 3 services based on https://github.com/dotnet-presentations/dotnet-maui-workshop/blob/main/Part%204%20-%20Platform%20Features/README.md
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            builder.Services.AddSingleton<IMap>(Map.Default);

            //For showing dialogs from ViewModels, see
            // - https://github.com/dotnet/maui-samples/blob/main/7.0/Beginners-Series/MauiApp2/MauiProgram.cs
            // - https://github.com/dotnet/maui-samples/blob/main/7.0/Beginners-Series/MauiApp2/MainPage.xaml.cs
            builder.Services.AddSingleton<Services.IPopupService, Services.PopupService>();

            //Required if want to set BIndingContext for Page in C#
            builder.Services.AddTransient<RangeDates>();
            builder.Services.AddTransient<RangeDatesViewModel>();
            builder.Services.AddTransient<ConvocationDates>();
            builder.Services.AddTransient<ConvocationDatesViewModel>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}