using CommunityToolkit.Maui.Markup;
using SteamKit2.GC.TF2.Internal;

namespace SteamBot.ContentViews;

public class EntryWithPasswordButton : ContentView
{
	public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(EntryWithPasswordButton), string.Empty);

	public string Text
	{
		get => (string)GetValue(EntryWithPasswordButton.TextProperty);
		set => SetValue(EntryWithPasswordButton.TextProperty, value);
	}

	public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(EntryWithPasswordButton), string.Empty);

	public string Placeholder
	{
		get => (string)GetValue(EntryWithPasswordButton.PlaceholderProperty);
		set => SetValue(EntryWithPasswordButton.PlaceholderProperty, value);
	}

	private static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(EntryWithPasswordButton), true);

	public bool IsPassword
	{
		get => (bool)GetValue(EntryWithPasswordButton.IsPasswordProperty);
		set => SetValue(EntryWithPasswordButton.IsPasswordProperty, value);
	}

	public EntryWithPasswordButton()
	{
		var textElement = new Entry();
		textElement.BindingContext = this;
		textElement.SetBinding(Entry.TextProperty, "Text", BindingMode.TwoWay);
		textElement.SetBinding(Entry.PlaceholderProperty, "Placeholder");
		textElement.SetBinding(Entry.IsPasswordProperty, "IsPassword");

		Content = new Grid
		{
			Children = {
				textElement,
				new Button()
				{
					HorizontalOptions = LayoutOptions.End,
					BackgroundColor = Colors.Transparent,
					BorderWidth = 0,
					HeightRequest = 40,
					Text = "👁"
				}.Invoke(b => b.Clicked += (sender, e) => {
					IsPassword = !IsPassword;
				})
			}
		};
	}
}