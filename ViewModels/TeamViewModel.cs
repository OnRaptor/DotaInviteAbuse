using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamBot.Framework;
using SteamBot.Services;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBot.ViewModels
{
	public partial class TeamViewModel : BaseViewModel
	{
		[ObservableProperty]
		public string uiState;

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
		public string teamName;

		[ObservableProperty]
		public List<String> teams;

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
		public string selectedTeam;

		bool CanApply => SelectedTeam != null && !String.IsNullOrEmpty(TeamName);

		List<CMsgDOTATeamInfo> _teams;

		DotaTeamService teamService;
		public TeamViewModel(DotaTeamService _teamService, DotaClient _dota)
		{
			UiState = "Loading";
			teamService = _teamService;
			teamService.teamInfo += TeamService_teamInfo;
			teamService.teamEdited += (r) => TeamService_teamEdited(r);

			if (_dota.IsLoaded)
			{
				teamService.LoadTeams();
				UiState = "Start";
			}
			else
				_dota.DotaLoaded += () =>
				{
					teamService.LoadTeams();
					UiState = "Start";
				};
		}

		private void TeamService_teamInfo(ClientGCMsgProtobuf<CMsgDOTATeamsInfo> response)
		{
			Teams = response.Body.teams.Select(s => s.name).ToList();
			_teams = response.Body.teams;
		}

		private async void TeamService_teamEdited(ClientGCMsgProtobuf<CMsgDOTAEditTeamDetailsResponse> response)
		{
			if (response != null && response.Body.result == CMsgDOTAEditTeamDetailsResponse.Result.SUCCESS)
				await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlert("Success", "Team edited", "OK"));
			else
				await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlert("Fail", response.Body.result.ToString(), "OK"));

			teamService.LoadTeams();
		}

		[RelayCommand(CanExecute = nameof(CanApply))]
		public void Apply()
		{
			var teamId = _teams.Find(team => team.name == SelectedTeam).team_id;
			teamService.ChangeName(TeamName, teamId);
		}
	}
}
