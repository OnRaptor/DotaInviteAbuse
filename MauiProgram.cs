using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.Logging;
using SteamBot.Framework;
using SteamBot.Pages;
using SteamBot.Services;
using SteamBot.ViewModels;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace SteamBot;
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if DEBUG
								.UseMauiCommunityToolkit()
#else
								.UseMauiCommunityToolkit(options =>
								{
									options.SetShouldSuppressExceptionsInConverters(false);
									options.SetShouldSuppressExceptionsInBehaviors(false);
									options.SetShouldSuppressExceptionsInAnimations(false);
								})
#endif
			.UseMauiCommunityToolkitMarkup()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
		builder.Services.AddSingleton(typeof(DotaClient));
		builder.Services.AddSingleton(typeof(DotaTeamService));
		
		RegisterViewsAndViewModels(builder.Services);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}	

	static void RegisterViewsAndViewModels(in IServiceCollection services)
	{
		services.AddSingletonWithShellRoute<LoginPage, LoginViewModel>("Login");
		services.AddSingleton<UtilPage, UtilViewModel>();
		services.AddSingleton<SpamPage, SpamViewModel>();
		services.AddSingleton<TeamPage, TeamViewModel>();
	}
}
