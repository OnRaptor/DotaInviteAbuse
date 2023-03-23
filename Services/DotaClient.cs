using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.Internal;
using System.Diagnostics;
using System.Text;

namespace DotaInviteAbuse.Services
{
	public class DotaClient
	{
		SteamClient client;

		SteamUser user;
		SteamFriends steamFriends;
		public SteamGameCoordinator gameCoordinator;

		CallbackManager callbackMgr;

		string userName;
		string Password;
		string authCode;
		string guardCode;

		CancellationTokenSource cancelTokenSource;
		CancellationToken tokenToExit;

		public delegate void LoggedEvent(SteamUser.LoggedOnCallback callback);
		public delegate void DotaLoadedEvent();
		public delegate void GCMessage(SteamGameCoordinator.MessageCallback callback);
		public delegate void DisconnectEvent(SteamClient.DisconnectedCallback callback);
		public delegate void LogUpdate(string newLine);

		public event LoggedEvent LoggedIn;
		public event DotaLoadedEvent DotaLoaded;
		public event GCMessage GCMesage;
		public event DisconnectEvent Disconnected;
		public event LogUpdate LogUpdated;

		public StringBuilder Log = new StringBuilder();

		public bool IsLoaded { get; set; }
		// dota2's appid
		public const int APPID = 570;

		public string AccountName
		{
			get => steamFriends.GetPersonaName();
			set => steamFriends.SetPersonaName(value);
		}

		public DotaClient()
		{
			client = new SteamClient();

			// get our handlers
			user = client.GetHandler<SteamUser>();
			steamFriends = client.GetHandler<SteamFriends>();
			gameCoordinator = client.GetHandler<SteamGameCoordinator>();

			// setup callbacks
			callbackMgr = new CallbackManager(client);

			callbackMgr.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
			callbackMgr.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
			callbackMgr.Subscribe<SteamGameCoordinator.MessageCallback>(OnGCMessage);
			callbackMgr.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);
			callbackMgr.Subscribe<SteamClient.DisconnectedCallback>(e =>
			{
				if (Disconnected != null)
					Disconnected(e);
			});

			DebugLog.Enabled = true;
			DebugLog.AddListener((category, msg) => Log.AppendLine(String.Format("Steam - {0}: {1}", category, msg)));
			DebugLog.AddListener((category, msg) => { if (LogUpdated != null) LogUpdated(String.Format("Steam - {0}: {1}", category, msg)); });

			GCMesage += OnGCMessageWhenDotaLoading;

			cancelTokenSource = new CancellationTokenSource();
			tokenToExit = cancelTokenSource.Token;


			Task.Factory.StartNew(() =>
			{
				while (true)
					if (tokenToExit.IsCancellationRequested)
						break;
					else
						callbackMgr.RunWaitCallbacks(TimeSpan.FromSeconds(2));

			}, tokenToExit);
		}

		public void Exit()
		{
			//100% exit app variant
			client.Disconnect();
			cancelTokenSource.Cancel();
		}

		public void ToggleOnline()
		{
			if (steamFriends.GetPersonaState() == EPersonaState.Online)
				steamFriends.SetPersonaState(EPersonaState.Offline);
			else
				steamFriends.SetPersonaState(EPersonaState.Online);
		}

		public void Connect(string username, string password, string authcode = null, string guard = null)
		{
			userName = username;
			Password = password;
			if (authcode != null) authCode = authcode.ToLower();
			if (guard != null) guardCode = guard.ToLower();

			client.Connect();
		}


		// called when the client successfully (or unsuccessfully) connects to steam
		void OnConnected(SteamClient.ConnectedCallback callback)
		{
			byte[] sentryHash = null;
			if (File.Exists("sentry.bin"))
			{
				// if we have a saved sentry file, read and sha-1 hash it
				byte[] sentryFile = File.ReadAllBytes("sentry.bin");
				sentryHash = CryptoHelper.SHAHash(sentryFile);
			}

			user.LogOn(new SteamUser.LogOnDetails
			{
				Username = userName,
				Password = Password,
				TwoFactorCode = guardCode,
				AuthCode = authCode,
			});
		}
		void OnLoggedOn(SteamUser.LoggedOnCallback callback)
		{
			LoggedIn(callback);
			if (callback.Result != EResult.OK)
				return;


			var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

			playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
			{
				game_id = new GameID(APPID), // or game_id = APPID,
			});

			// send it off
			// notice here we're sending this message directly using the SteamClient
			client.Send(playGame);
			DebugLog.WriteLine("Log", "Attempted to start dota");
			// delay a little to give steam some time to establish a GC connection to us
			Thread.Sleep(3000);

			// inform the dota GC that we want a session
			var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint)EGCBaseClientMsg.k_EMsgGCClientHello);
			clientHello.Body.engine = ESourceEngine.k_ESE_Source2;
			gameCoordinator.Send(clientHello, APPID);
			DebugLog.WriteLine("Log", "Request gc session");
		}

		// called when a gamecoordinator (GC) message arrives
		// these kinds of messages are designed to be game-specific
		// in this case, we'll be handling dota's GC messages
		void OnGCMessage(SteamGameCoordinator.MessageCallback callback)
		{
			Log.AppendLine($"Recv msg ({callback.Message.GetData().Length} bytes):{(EDOTAGCMsg)callback.EMsg}");
			if (GCMesage != null)
				GCMesage(callback);
		}

		void OnGCMessageWhenDotaLoading(SteamGameCoordinator.MessageCallback callback)
		{
			if (callback.EMsg == (uint)EGCBaseClientMsg.k_EMsgGCClientWelcome)
			{
				IsLoaded = true;
				if (DotaLoaded != null)
					DotaLoaded();

				GCMesage -= OnGCMessageWhenDotaLoading;
			}
		}

		void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
		{
			Debug.WriteLine("Updating sentryfile...");

			// write out our sentry file
			// ideally we'd want to write to the filename specified in the callback
			// but then this sample would require more code to find the correct sentry file to read during logon
			// for the sake of simplicity, we'll just use "sentry.bin"

			int fileSize;
			byte[] sentryHash;
			using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
			{
				fs.Seek(callback.Offset, SeekOrigin.Begin);
				fs.Write(callback.Data, 0, callback.BytesToWrite);
				fileSize = (int)fs.Length;

				fs.Seek(0, SeekOrigin.Begin);
				using var sha = System.Security.Cryptography.SHA1.Create();
				sentryHash = sha.ComputeHash(fs);
			}

			// inform the steam servers that we're accepting this sentry file
			user.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
			{
				JobID = callback.JobID,

				FileName = callback.FileName,

				BytesWritten = callback.BytesToWrite,
				FileSize = fileSize,
				Offset = callback.Offset,

				Result = EResult.OK,
				LastError = 0,

				OneTimePassword = callback.OneTimePassword,

				SentryFileHash = sentryHash,
			});

		}
	}
}
