using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ProRecords
{
	public partial class RegisterPageModel : BasePageModel
	{
		[ObservableProperty]
		string names;
        [ObservableProperty]
        string email;
        [ObservableProperty]
        string password;
        [ObservableProperty]
        string confirmPassword;

        public RegisterPageModel()
		{
		}
	}
}

