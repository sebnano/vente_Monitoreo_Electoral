﻿namespace ElectoralMonitoring;

public partial class SearchPage : ContentPage
{
	public SearchPage(SearchPageModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
