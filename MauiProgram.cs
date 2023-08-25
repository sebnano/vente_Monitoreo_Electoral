using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace ElectoralMonitoring;

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
        builder.Services.AddTransient<LoginPageModel>();
        builder.Services.AddTransient<RegisterPageModel>();
        builder.Services.AddTransient<MonitorListPageModel>();
        builder.Services.AddTransient<ScannerPreviewPageModel>();
        return builder;
    }

    private static MauiAppBuilder RegisterPages(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<MonitorListPage>();
        builder.Services.AddTransient<ScannerPreviewPage>();
        return builder;
    }

    private static MauiAppBuilder RegisterRoutes(this MauiAppBuilder builder)
    {
        Routing.RegisterRoute(nameof(LoginPageModel), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPageModel), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(MonitorListPageModel), typeof(MonitorListPage));
        Routing.RegisterRoute(nameof(ScannerPreviewPageModel), typeof(ScannerPreviewPage));
        return builder;
    }
}

