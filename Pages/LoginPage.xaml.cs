using SteamBot.Framework;
using SteamBot.ViewModels;

namespace SteamBot.Pages;

public partial class LoginPage : BasePage<LoginViewModel>
{
	public LoginPage(LoginViewModel viewModel) : base(viewModel)
	{
		InitializeComponent();
	}
}