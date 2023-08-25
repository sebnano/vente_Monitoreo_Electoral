using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectoralMonitoring
{
    public partial class MonitorListPageModel : BasePageModel
    {
        [ObservableProperty]
        string username;

        public MonitorListPageModel(AuthService authService) : base(authService)
        {
            Username = _authService.NameUser;
        }
    }
}

