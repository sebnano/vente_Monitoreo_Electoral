﻿using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Shared;
using Plugin.Firebase.Analytics;
using MonkeyCache.FileStore;

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

            .RegisterFirebaseServices()
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
        builder.Services.AddSingleton<NodeService>();
        builder.Services.AddSingleton<AnalyticsService>();

        builder.Services.AddSingleton(Preferences.Default);
        builder.Services.AddSingleton(Connectivity.Current);

        //Firebase services
        builder.Services.AddSingleton(CrossFirebaseAnalytics.Current);

        //Refit services
        IAuthApi authApi = RefitExtensions.For<IAuthApi>(BaseApiService.GetApi(Fusillade.Priority.Explicit));
        INodeApi nodeApi = RefitExtensions.For<INodeApi>(BaseApiService.GetApi(Fusillade.Priority.Explicit));
        builder.Services.AddSingleton(authApi);
        builder.Services.AddSingleton(nodeApi);

        //cache
        Barrel.ApplicationId = Helpers.AppSettings.AppId;

        return builder;
    }

    private static MauiAppBuilder RegisterPageModels(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<LoginPageModel>();
        builder.Services.AddTransient<RegisterPageModel>();
        builder.Services.AddTransient<MonitorListPageModel>();
        builder.Services.AddTransient<ScannerPreviewPageModel>();
        builder.Services.AddTransient<SummaryPageModel>();
        builder.Services.AddTransient<HomePageModel>();
        builder.Services.AddTransient<ReportListPageModel>();
        builder.Services.AddTransient<ReportFormPageModel>();
        return builder;
    }

    private static MauiAppBuilder RegisterPages(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<MonitorListPage>();
        builder.Services.AddTransient<ScannerPreviewPage>();
        builder.Services.AddTransient<SummaryPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ReportListPage>();
        builder.Services.AddTransient<ReportFormPage>();
        return builder;
    }

    private static MauiAppBuilder RegisterRoutes(this MauiAppBuilder builder)
    {
        Routing.RegisterRoute(nameof(LoginPageModel), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPageModel), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(MonitorListPageModel), typeof(MonitorListPage));
        Routing.RegisterRoute(nameof(ScannerPreviewPageModel), typeof(ScannerPreviewPage));
        Routing.RegisterRoute(nameof(SummaryPageModel), typeof(SummaryPage));
        Routing.RegisterRoute(nameof(ReportFormPageModel), typeof(ReportFormPage));
        Routing.RegisterRoute(nameof(ReportListPageModel), typeof(ReportListPage));
        return builder;
    }

    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.FinishedLaunching((app, launchOptions) => {
                Plugin.Firebase.iOS.CrossFirebase.Initialize(app, launchOptions, CreateCrossFirebaseSettings());
                Firebase.Crashlytics.Crashlytics.SharedInstance.Init();
                Firebase.Crashlytics.Crashlytics.SharedInstance.SendUnsentReports();
                new ImageCropper.Maui.Platform().Init();
                return false;
            }));
#else

            events.AddAndroid(android => android.OnCreate((activity, state) =>
            {
                Plugin.Firebase.Android.CrossFirebase.Initialize(activity, state, CreateCrossFirebaseSettings());
                Plugin.Firebase.Crashlytics.CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
            }));
#endif

        });
        return builder;
    }

    private static CrossFirebaseSettings CreateCrossFirebaseSettings()
    {
        return new CrossFirebaseSettings(
            isAnalyticsEnabled: true,
            isAuthEnabled: true,
            isCloudMessagingEnabled: false,
            isDynamicLinksEnabled: false,
            isFirestoreEnabled: false,
            isCrashlyticsEnabled: true,
            isFunctionsEnabled: true,
            isRemoteConfigEnabled: false,
            isStorageEnabled: false,
            googleRequestIdToken: Helpers.AppSettings.GoogleRequestIdToken
            );
    }
}

