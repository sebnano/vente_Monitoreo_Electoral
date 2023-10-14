namespace ElectoralMonitoring;

public partial class OptionsButtonsFieldControl : ContentView, IFieldControl
{
    public static List<string> TypesAvailable = new()
    {
        "options_buttons"
    };

    public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(string), typeof(OptionsButtonsFieldControl), string.Empty, BindingMode.OneTime);
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(OptionsButtonsFieldControl), string.Empty);
    public static readonly BindableProperty IsRequiredFieldProperty = BindableProperty.Create(nameof(IsRequiredField), typeof(bool), typeof(OptionsButtonsFieldControl), null);

    public bool IsRequiredField
    {
        get => (bool)GetValue(IsRequiredFieldProperty);
        set => SetValue(IsRequiredFieldProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Key
    {
        get => (string)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public List<SelectOption> Options { get; set; }

    public OptionsButtonsFieldControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Inicializa el picker con las opciones como string a|Option 1,b|Option2,c|Option 3
    /// </summary>
    /// <param name="value">The string values</param>
    public void InitControl(string value)
    {
        Options = new();
        var splited = value.Split(",");
        foreach (var item in splited)
        {
            var itemSplited = item.Split("|");
            Options.Add(new() { Key = itemSplited[0].TrimStart().TrimEnd(), Value = itemSplited[1] });
        }
        InitControl(Options);
    }

    /// <summary>
    /// Inicializa el picker con las opciones como Objeto de opciones
    /// </summary>
    /// <param name="value">The string values</param>
    public void InitControl(List<SelectOption> values)
    {
        foreach (var item in values)
        {
            var cb = new LabeledCheckbox() { Key = item.Key, Text = item.Value };
            cb.CheckedChanged += Cb_CheckedChanged;
            Checkboxs.Add(cb);
        }
    }

    private void Cb_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is LabeledCheckbox && e.Value)
        {
            ClearStatusRequired();
        }
    }

    public string GetKey()
    {
        return Key;
    }

    public object GetValue()
    {
        var values = new List<Node>();
        foreach (var item in Checkboxs.Children)
        {
            if (item is LabeledCheckbox cb)
            {
                if (cb.IsChecked)
                {
                    values.Add(new() { Value = cb.Key });
                }
            }
        }

        return values;
    }

    public bool HasValue()
    {
        return Checkboxs.Children.Any(x => (x as LabeledCheckbox)?.IsChecked == true);
    }

    public bool IsRequired()
    {
        return IsRequiredField;
    }

    public void SetRequiredStatus()
    {
        MyBorder.SetDynamicResource(Border.StrokeProperty, "Red");
        RequiredLabel.IsVisible = true;
    }

    public void ClearStatusRequired()
    {
        if (!RequiredLabel.IsVisible) return;
        RequiredLabel.IsVisible = false;
        var dark = Color.FromArgb("#404040");
        var light = Color.FromArgb("#ACACAC");
        MyBorder.SetAppTheme(Border.StrokeProperty, light, dark);
    }

    public void SetValue(object value)
    {
        var index = Options.FindIndex(x => x.Key == value.ToString());
    }
}
