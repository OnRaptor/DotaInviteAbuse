using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamBot.Framework;
using SteamBot.Services;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SteamKit2.GC.Dota.Internal;
using SteamBot.UI.Components;
using CommunityToolkit.Maui.Views;
using System.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using SteamKit2.GC;
using SteamKit2.Internal;
using System.Runtime.InteropServices;

namespace SteamBot.ViewModels
{
	public partial class UtilViewModel : BaseViewModel
	{
		[ObservableProperty]
		string log;

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(ChangeNameCommand))]
		string accountName;

		[ObservableProperty]
		List<string> gcmessages;

		[ObservableProperty]
		string selectedMessage;


		bool canChange => !String.IsNullOrEmpty(AccountName);
		//private string PickedMessge;
		//private ObservableCollection<string> allMessages;
		ItemPicker popup;
		DotaClient dota;
		bool CanPick => true;
		public UtilViewModel(DotaClient _dota) {
			dota = _dota;
			dota.DotaLoaded += Dota_DotaLoaded;
			dota.GCMesage += Dota_GCMesage;
			dota.Disconnected += Dota_Disconnected;

			DebugLog.AddListener((category, msg) => Log += String.Format("Steam - {0}: {1}", category, msg) + '\n');
			/*Task.Factory.StartNew(async () =>
			{
				using var stream = await FileSystem.OpenAppPackageFileAsync("AllDota2Messages.txt");
				using var reader = new StreamReader(stream);
				List<string> lines =  new List<string>();
				while (!reader.EndOfStream)
					lines.Add(reader.ReadLine());

				allMessages = lines.ToObservableCollection();
			});*/

			if (dota.IsLoaded)
				AccountName = dota.AccountName;


		}

		[RelayCommand]
		void ToggleOnline() => dota.ToggleOnline();

		[RelayCommand(CanExecute = nameof(canChange))]
		void ChangeName()
		{
			dota.AccountName = AccountName;
		    MainThread.BeginInvokeOnMainThread(() => Shell.Current.DisplayAlert("Success", "Name changed to " + AccountName, "OK"));
			AccountName = "";
		}

		[RelayCommand]
		public void SendMessage()
		{
			if (!String.IsNullOrEmpty(selectedMessage))
			{
				/*try
				{
					var message = MsgUtil.GetMsg((uint)Enum.Parse<EDOTAGCMsg>(selectedMessage));
					new ClientGCMsg()
					dota.gameCoordinator.Send(message, (uint)DotaClient.APPID);
				}*/
			}
		}

		/*[RelayCommand(CanExecute = nameof(CanPick))]
		public async Task PickGCMessage()
		{
			popup = new ItemPicker(allMessages.ToList());
			MainThread.BeginInvokeOnMainThread(() => Shell.Current.ShowPopup(popup));
			var item = await popup.Result;
			PickedMessge = item?.ToString();
		}*/

		private IEnumerable<String> GetTypesInNamespace(string nameSpace)
		{
			foreach (var item in Assembly.GetAssembly(typeof(SteamKit2.GC.Dota.Internal.CAttribute_String)).GetTypes())
			{
				if (String.Equals(item?.Namespace, nameSpace, StringComparison.Ordinal))
					yield return item.Name;
			}
		}
		private void Dota_Disconnected(SteamClient.DisconnectedCallback callback)
		{
			Log += $"Disconnected, UserInitiated -> {callback.UserInitiated}\n";
		}

		private void Dota_GCMesage(SteamGameCoordinator.MessageCallback callback)
		{
			Log += $"Recv msg ({callback.Message.GetData().Length} bytes):{(EDOTAGCMsg)callback.EMsg}\n";
		}

		private void Dota_DotaLoaded()
		{
			Log += $"Dota Loaded!\n";
			MainThread.BeginInvokeOnMainThread(() => AccountName = dota.AccountName);
		}
	}
}
