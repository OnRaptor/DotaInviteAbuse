using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;

namespace DotaInviteAbuse.Services
{
	public class DotaTeamService
	{
		DotaClient dota;
		const int APPID = 570;
		Dictionary<uint, Action<object>> messageMap;

		public delegate void TeamInfoResponse(ClientGCMsgProtobuf<CMsgDOTATeamsInfo> response);
		public event TeamInfoResponse teamInfo;

		public delegate void EditTeamResponse(ClientGCMsgProtobuf<CMsgDOTAEditTeamDetailsResponse> response);
		public event EditTeamResponse teamEdited;

		public DotaTeamService(DotaClient _dota)
		{
			dota = _dota;
			dota.GCMesage += Dota_GCMesage;
			messageMap = new Dictionary<uint, Action<object>>
			{
				{ ( uint )EDOTAGCMsg.k_EMsgGCToClientTeamsInfo, (r) =>  teamInfo(new ClientGCMsgProtobuf<CMsgDOTATeamsInfo>((IPacketGCMsg)r))},
				{ ( uint )EDOTAGCMsg.k_EMsgGCEditTeamDetailsResponse, (r) =>  teamEdited(new ClientGCMsgProtobuf<CMsgDOTAEditTeamDetailsResponse>((IPacketGCMsg)r))}
			};
		}

		uint spamTargetId;
		uint targetTeamId;

		void SendInvite()
		{
			var inviteRequest = new ClientGCMsgProtobuf<CMsgDOTATeamInvite_InviterToGC>((uint)EDOTAGCMsg.k_EMsgGCTeamInvite_InviterToGC);
			inviteRequest.Body.team_id = targetTeamId;
			inviteRequest.Body.account_id = spamTargetId;
			dota.gameCoordinator.Send(inviteRequest, APPID);
		}
		public void StartSpam(uint team_id, uint accountId, bool autoKick)
		{
			spamTargetId = accountId;
			targetTeamId = team_id;
			SendInvite();
			dota.GCMesage += autoKick ? OnGCMessageWhenSpamWithAutoKick : OnGCMessageWhenSpam;
		}

		public void StopSpam(bool autoKick)
		{
			dota.GCMesage -= autoKick ? OnGCMessageWhenSpamWithAutoKick : OnGCMessageWhenSpam;
		}

		public void OnGCMessageWhenSpamWithAutoKick(SteamGameCoordinator.MessageCallback callback)
		{
			if (callback.EMsg == (uint)EDOTAGCMsg.k_EMsgGCTeamInvite_GCImmediateResponseToInviter)
			{
				var result = new ClientGCMsgProtobuf<CMsgDOTATeamInvite_GCImmediateResponseToInviter>(callback.Message);
				if (result.Body.result == ETeamInviteResult.TEAM_INVITE_ERROR_INVITEE_ALREADY_MEMBER)
				{
					var kickRequest = new ClientGCMsgProtobuf<CMsgDOTAKickTeamMember>((uint)EDOTAGCMsg.k_EMsgGCKickTeamMember);
					kickRequest.Body.account_id = spamTargetId;
					kickRequest.Body.team_id = targetTeamId;
					dota.gameCoordinator.Send(kickRequest, APPID);
					SendInvite();
				}
				else
					SendInvite();
			}
		}

		public void OnGCMessageWhenSpam(SteamGameCoordinator.MessageCallback callback)
		{
			if (callback.EMsg == (uint)EDOTAGCMsg.k_EMsgGCTeamInvite_GCImmediateResponseToInviter)
				SendInvite();
		}
		public void LoadTeams()
		{
			var requestTeams = new ClientGCMsgProtobuf<CMsgDOTAMyTeamInfoRequest>((uint)EDOTAGCMsg.k_EMsgClientToGCMyTeamInfoRequest);
			dota.gameCoordinator.Send(requestTeams, APPID);
		}

		public void ChangeName(string name, uint team_id)
		{
			var editDetails = new ClientGCMsgProtobuf<CMsgDOTAEditTeamDetails>((uint)EDOTAGCMsg.k_EMsgGCEditTeamDetails);
			editDetails.Body.name = name;
			editDetails.Body.team_id = team_id;
			dota.gameCoordinator.Send(editDetails, APPID);
		}

		private void Dota_GCMesage(SteamKit2.SteamGameCoordinator.MessageCallback callback)
		{
			Action<object> func;
			if (!messageMap.TryGetValue(callback.EMsg, out func))
				return;

			func(callback.Message);
		}
	}
}
