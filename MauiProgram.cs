using CommunityToolkit.Maui;
using MauiIcons.FontAwesome.Solid;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
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
#if WINDOWS
            // use the correct first day of the week
            DatePickerHandler.Mapper.AppendToMapping("FixFirstDayOfWeek", (handler, view) =>
            {
                handler.PlatformView.FirstDayOfWeek = (Windows.Globalization.DayOfWeek)(int)
                    System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            });
#endif


            return builder.Build();
		}
	}
}
