using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Controls;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace SteamBot.UI.Components
{
	public class AlertPopup : Popup
	{
		public AlertPopup(string title = "Message", string text = "")
		{
		
			Color = Colors.Transparent;
			Content = new Frame() {
				WidthRequest = 300,
				BackgroundColor = Color.FromArgb("#ff716DD7"),
				CornerRadius = 10,
				Content = new Grid()
				{
					RowDefinitions = Rows.Define(40, Star, 50),
					Padding = 5,
					Children =
				{
					new Label()
						.Text(title)
						.FontSize(25),
					new Label()
						.Text(text)
						.Invoke(_ => _.LineBreakMode = LineBreakMode.WordWrap)
						.Row(1)
						.Paddings(bottom:5)
						.FontSize(19),
					new Button()
							.Text("OK")
							.Invoke(_ => _.Clicked += (s,e) => Close())
							.Style(new Style<Button>(
									(Button.BackgroundProperty, "Transparent"),
									(Button.BorderWidthProperty, "0")
									)).Row(2)
				}
				}
			};
		}
	}
}
