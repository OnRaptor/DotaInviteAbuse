using DotaInviteAbuse.Services;
using MudBlazor;
using MudBlazor.Services;

namespace DotaInviteAbuse;

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
			});

		builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

		builder.Services.AddMudServices(config =>
#if __MOBILE__
			config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter
#else
			config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft
#endif
		);

		builder.Services.AddSingleton(typeof(DotaClient));
		builder.Services.AddSingleton(typeof(DotaTeamService));
		return builder.Build();
	}
}
