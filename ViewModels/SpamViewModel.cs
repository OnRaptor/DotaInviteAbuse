using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamBot.Framework;
using SteamBot.Services;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBot.ViewModels
{
	public partial class SpamViewModel : BaseViewModel
	{
		[ObservableProperty]
		string uiState;
		[ObservableProperty]
		string buttonState;

		[ObservableProperty]
		List<String> teams;

		[ObservableProperty]
		string selectedTeam;

		[ObservableProperty]
		string targetId;

		[ObservableProperty]
		bool autoKick = true;

		List<CMsgDOTATeamInfo> _teams;

		DotaTeamService teamService;
		public SpamViewModel(DotaTeamService _teamService, DotaClient _dota)
		{
			uiState = "Loading";
			buttonState = "Start";
			teamService = _teamService;
			teamService.teamInfo += TeamService_teamInfo;
			teamService.teamEdited += (r) => teamService.LoadTeams();
			if (_dota.IsLoaded)
				teamService.LoadTeams();
			else
			_dota.DotaLoaded += teamService.LoadTeams;

		}

		[RelayCommand]
		public void StartSpamming()
		{
			var teamId = _teams.Find(team => team.name == SelectedTeam).team_id;
		    teamService.StartSpam(teamId, uint.Parse(TargetId), autoKick);
			ButtonState = "Stop";
		}

		[RelayCommand]
		public void StopSpamming()
		{
			teamService.StopSpam(autoKick);
			ButtonState = "Start";
		}

		private void TeamService_teamInfo(ClientGCMsgProtobuf<CMsgDOTATeamsInfo> response)
		{
			Teams = response.Body.teams.Select(s => s.name).ToList();
			_teams = response.Body.teams;
			UiState = "Start";
		}
	}
}
