using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace ProRecords;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>().UseMauiCommunityToolkit().UseMauiCommunityToolkitMarkup()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("icons.ttf", "AppIcons");
            })

            .RegisterServices()
            .RegisterPageModels()
            .RegisterPages()
            .RegisterRoutes();

#if DEBUG
		builder.Logging.AddDebug();
#endif
        EntryHandler.RemoveBorders();
        return builder.Build();
	}

    private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<AnalyticsService>();
        builder.Services.AddSingleton<IFirebaseAnalytics, LoggerAnalytics>();

        builder.Services.AddSingleton(Preferences.Default);
        //builder.Services.AddSingleton(CrossFirebaseAnalytics.Current);

        //Refit services
        IAuthApi authApi = RefitExtensions.For<IAuthApi>(BaseApiService.GetApi(Fusillade.Priority.Explicit));
        builder.Services.AddSingleton(authApi);

        return builder;
    }

    private static MauiAppBuilder RegisterPageModels(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<MainPageModel>();
        builder.Services.AddTransient<LoginPageModel>();
        builder.Services.AddTransient<RegisterPageModel>();
        builder.Services.AddTransient<SearchPageModel>();
        builder.Services.AddTransient<ScannerPageModel>();
        builder.Services.AddTransient<CollectionPageModel>();
        return builder;
    }

    private static MauiAppBuilder RegisterPages(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ScannerPage>();
        builder.Services.AddTransient<CollectionPage>();
        return builder;
    }

    private static MauiAppBuilder RegisterRoutes(this MauiAppBuilder builder)
    {
        Routing.RegisterRoute(nameof(LoginPageModel), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPageModel), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(SearchPageModel), typeof(SearchPage));
        Routing.RegisterRoute(nameof(ScannerPageModel), typeof(ScannerPage));
        Routing.RegisterRoute(nameof(CollectionPageModel), typeof(CollectionPage));
        return builder;
    }
}

