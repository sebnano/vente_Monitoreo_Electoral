﻿using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProRecords
{
	public partial class LoginPageModel : BasePageModel, IQueryAttributable
	{
		readonly AuthService _authService;

		[ObservableProperty]
		string username = string.Empty;

        [ObservableProperty]
        string password = string.Empty;

        public LoginPageModel(AuthService authService)
		{
			_authService = authService;
#if DEBUG
            Username = "demo";
			Password = "Aa.12345";
#endif
		}

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
			if (query.ContainsKey("username") && query.ContainsKey("password"))
			{
				Username = query["username"] as string ?? string.Empty;
                Password = query["password"] as string ?? string.Empty;
				LoginCommand.Execute(null);
            }
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
		async Task Login()
		{
			if(string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)) {
				return;
			}

			var result = await _authService.Login(Username, Password, CancellationToken.None);
			if(result != null)
			{
				await Shell.Current.Navigation.PopToRootAsync();
			}
		}
	}
}

