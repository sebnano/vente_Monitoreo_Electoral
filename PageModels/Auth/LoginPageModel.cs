using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ProRecords
{
	public partial class LoginPageModel : BasePageModel
	{
		[ObservableProperty]
		string email;

        [ObservableProperty]
        string password;

        public LoginPageModel()
		{
		}
	}
}

