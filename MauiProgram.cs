using CommunityToolkit.Maui;
using MauiIcons.FontAwesome;
using MauiIcons.FontAwesome.Solid;
using Microsoft.Extensions.Logging;
using zoft.MauiExtensions.Controls;

namespace NWork
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.UseZoftAutoCompleteEntry()
				.UseFontAwesomeSolidMauiIcons()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}
