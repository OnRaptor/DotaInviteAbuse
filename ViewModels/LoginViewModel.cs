using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamBot.Framework;
using SteamBot.Services;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBot.ViewModels
{
	public partial class LoginViewModel : BaseViewModel
	{
		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(LoginSteamCommand))]
		public string login;

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(LoginSteamCommand))]
		public string password;

		[ObservableProperty]
		public string uiState;


		private bool CanLogin => Login != null && Password != null;
		private DotaClient dota;

		public LoginViewModel(DotaClient _dota)
		{
			dota = _dota;
			dota.LoggedIn += Dota_LoggedIn;
			UiState = "Display";

			App.Current.Windows.FirstOrDefault().Destroying += (s, e) =>
			{
				dota.Exit();
			};
		}

		private async void Dota_LoggedIn(SteamUser.LoggedOnCallback callback)
		{
			if (callback.Result != EResult.OK)
			{
				switch(callback.Result)
				{
					case EResult.TwoFactorCodeMismatch:
						var guard = await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayPromptAsync("Steam Guard", "Guard code mismatch\nTry one more time"));
						if(guard != null)
							dota.Connect(login, password, guard: guard);
						break;
					case EResult.AccountLoginDeniedNeedTwoFactor:
						guard = await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayPromptAsync("Steam Guard", "Your Steam Guard code"));
						if (guard != null)
							dota.Connect(login, password, guard:guard);
						break;

					case EResult.InvalidLoginAuthCode:
					case EResult.AccountLogonDenied:
						var authcode = await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayPromptAsync("Email verify", "Code"));
						if (authcode != null)
							dota.Connect(login, password, authcode: authcode);
						break;

					case EResult.InvalidPassword:
					case EResult.InvalidName:
					case EResult.InvalidEmail:
						await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlert("Error", "Check login or password", "OK"));
						break;

					case EResult.RateLimitExceeded:
						await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlert("Error", "Rate limit reached\nPlease try later", "OK"));
						break;

					default:
						await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlert("Error", "Unresolved error:\n" + callback.ExtendedResult, "OK"));
						break;
				}
				UiState = "Display";
				return;
			}

			if (uiState != "Loading")
				await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//MainPage"));
			uiState = "Loading";
		}

		[RelayCommand(CanExecute = nameof(CanLogin))]
		public void LoginSteam()
		{
			UiState = "Loading";
			dota.Connect(login, password);
		}
	}
}
