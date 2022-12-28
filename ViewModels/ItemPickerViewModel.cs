using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamBot.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBot.ViewModels
{
	public partial class ItemPickerViewModel : BaseViewModel
	{
		[ObservableProperty]
		public List<String> items;

		[ObservableProperty]
		public string selectedItem;

		private List<String> allItems;

		public ItemPickerViewModel(List<String> _allItems)
		{
			allItems = _allItems;
		}

		[RelayCommand]
		public void Search(string text)
		{
			Items = allItems.Where(m => m.ToLower().Contains(text.ToLower())).ToList();
		}
	}
}
