using SteamBot.Framework;
using SteamBot.ViewModels;

namespace SteamBot.Pages;

public partial class SpamPage : BasePage<SpamViewModel>
{
	public SpamPage(SpamViewModel viewmodel) : base(viewmodel)
	{
		InitializeComponent();
	}
}