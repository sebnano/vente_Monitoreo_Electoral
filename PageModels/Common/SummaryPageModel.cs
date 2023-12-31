﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ElectoralMonitoring
{
    public record ActionButtonDTO(
        string Text, string Style, IAsyncRelayCommand ActionCommand
    );

    public partial class SummaryPageModel : BasePageModel, IQueryAttributable
    {
        public const string TYPE_SUCCESS = "success";
        public const string TYPE_ERROR = "error";

        [ObservableProperty]
        Color color;

        [ObservableProperty]
        string icon;

        [ObservableProperty]
        string message;

        [ObservableProperty]
        ObservableCollection<ActionButtonDTO> actions;

        public SummaryPageModel(AuthService authService) : base(authService)
        {
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        public async Task ActionButton(ActionButtonDTO btn)
        {
            IsBusy = true;

            await (btn.ActionCommand as AsyncRelayCommand)?.ExecuteAsync(null);

            IsBusy = false;

        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {

            if (query.ContainsKey("type") && query.ContainsKey("message") && query.ContainsKey("actions"))
            {
                Message = query["message"] as string ?? string.Empty;

                var type = query["type"] as string ?? string.Empty;
                var actions = query["actions"] as List<ActionButtonDTO>;
                Actions = new();
                if (actions != null)
                {
                    foreach (var item in actions)
                    {
                        Actions.Add(item);
                    }
                }
                if (type == TYPE_SUCCESS) {
                    Icon = IconFont.CheckCircle;
                    Color = Color.FromArgb("#134077");
                    Actions.Add(new ActionButtonDTO("Inicio", "ButtonPrimary", new AsyncRelayCommand(async () => await Shell.Current.Navigation.PopToRootAsync() )));
                }
                else if(type == TYPE_ERROR) {
                    Icon = IconFont.AlertComment;
                    Color = Color.FromArgb("#dc3623");
                    Actions.Add(new ActionButtonDTO("Inicio", "ButtonPrimary", new AsyncRelayCommand(async () => await Shell.Current.Navigation.PopToRootAsync())));
                    Actions.Add(new ActionButtonDTO("Atrás", "ButtonPrimary", new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("..?fromsummary=true"))));
                }
            }
        }
    }
}

