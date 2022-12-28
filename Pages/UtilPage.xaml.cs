using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using Microsoft.Win32;
using SteamBot.Framework;
using SteamBot.UI.Components;
using SteamBot.ViewModels;
using SteamKit2;
using SteamKit2.Internal;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using static SteamKit2.GC.Dota.Internal.CMsgDOTAFrostivusTimeElapsed;

namespace SteamBot.Pages;

class AndroidMachineInfoProvider : IMachineInfoProvider
{
	byte[] IMachineInfoProvider.GetDiskId() => new byte[] { 50, 49, 51, 56, 49, 74, 56, 48, 48, 48, 53, 50 };

	byte[] IMachineInfoProvider.GetMacAddress() => new byte[] { 0 };

	byte[] IMachineInfoProvider.GetMachineGuid() => new byte[] { 57, 101, 98, 57, 51, 55, 51, 52, 45, 101, 57, 53, 56, 45, 52, 102, 50, 55, 45, 98, 54, 97, 97, 45, 98, 56, 48, 97, 101, 52, 97, 99, 50, 99, 48, 101 };
}


public partial class UtilPage : BasePage<UtilViewModel>
{
	public UtilPage(UtilViewModel viewmodel) : base(viewmodel)
	{
		InitializeComponent();
	}

}

