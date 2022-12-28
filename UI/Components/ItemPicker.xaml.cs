using CommunityToolkit.Maui.Views;
using SteamBot.Framework;
using SteamBot.ViewModels;

namespace SteamBot.UI.Components;

public partial class ItemPicker : Popup 
{
	public ItemPicker(List<String> allItems) 
	{
		InitializeComponent();
		BindingContext = new ItemPickerViewModel(allItems);
	}

	private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
	{
		Close(e.SelectedItem);
	}

	private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Close(e.CurrentSelection.FirstOrDefault());
	}
}