﻿using System.Text;
using System.Text.RegularExpressions;

namespace ProRecords;

public enum FieldType
{
    Text,
    Name,
    DateTime,
    Email,
    Password,
    Username
}

public partial class InputFieldControl : ContentView
{
    public static readonly BindableProperty MaxLenghtProperty = BindableProperty.Create(nameof(MaxLenght), typeof(int), typeof(InputFieldControl), 100);
    public static readonly BindableProperty DateProperty = BindableProperty.Create(nameof(Date), typeof(DateTime), typeof(InputFieldControl), DateTime.Now, BindingMode.TwoWay);
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(InputFieldControl), string.Empty, BindingMode.TwoWay);
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(InputFieldControl), string.Empty);
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(InputFieldControl), string.Empty);
    public static readonly BindableProperty FieldTypeProperty = BindableProperty.Create(nameof(FieldType), typeof(FieldType), typeof(InputFieldControl), FieldType.Text, propertyChanged: OnFieldTypePropertyChanged);
    public static readonly BindableProperty KeyboardTypeProperty = BindableProperty.Create(nameof(KeyboardType), typeof(Keyboard), typeof(InputFieldControl), Keyboard.Text);
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(InputFieldControl), null);

    static void OnFieldTypePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is InputFieldControl control)
        {
            if (newValue is FieldType type)
            {
                switch (type)
                {
                    case FieldType.Text:
                    case FieldType.Name:
                    case FieldType.Username:
                    case FieldType.Email:
                        control.EntryStack.IsVisible = true;
                        control.MyDatePicker.IsVisible = false;
                        control.VisibilityPassword.IsVisible = false;
                        control.MyEntry.IsPassword = false;
                        break;
                    case FieldType.DateTime:
                        control.EntryStack.IsVisible = false;
                        control.MyDatePicker.IsVisible = true;
                        control.VisibilityPassword.IsVisible = false;
                        control.MyEntry.IsPassword = false;
                        break;
                    case FieldType.Password:
                        control.VisibilityPassword.IsVisible = true;
                        control.MyEntry.IsPassword = true;
                        control.EntryStack.IsVisible = true;
                        control.MyDatePicker.IsVisible = false;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public int MaxLenght
    {
        get => (int)GetValue(MaxLenghtProperty);
        set => SetValue(MaxLenghtProperty, value);
    }

    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public FieldType FieldType
    {
        get => (FieldType)GetValue(FieldTypeProperty);
        set => SetValue(FieldTypeProperty, value);
    }

    public Keyboard KeyboardType
    {
        get => (Keyboard)GetValue(KeyboardTypeProperty);
        set => SetValue(KeyboardTypeProperty, value);
    }

    public InputFieldControl()
    {
        InitializeComponent();

        MyEntry.TextChanged += MyEntry_TextChanged;
        MyEntry.Completed += MyEntry_Completed;

        PropertyChanged += InputFieldControl_PropertyChanged;
    }

    private void InputFieldControl_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is InputFieldControl entryField && e.PropertyName == ContentView.IsEnabledProperty.PropertyName)
        {
            EntryStack.IsEnabled = entryField.IsEnabled;
            MyEntry.IsEnabled = entryField.IsEnabled;
            MyDatePicker.IsEnabled = entryField.IsEnabled;
            MyBorder.IsEnabled = entryField.IsEnabled;
        }
    }
    int MyCursorPosition = 0;
    private void MyEntry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            MyEntry.Text = string.Empty;
            return;
        }
        if (string.IsNullOrEmpty(e.NewTextValue)) return;
        if (e.NewTextValue == e.OldTextValue) return;


        if (!string.IsNullOrWhiteSpace(e.OldTextValue) && !Regex.Match(e.OldTextValue, Expr(FieldType)).Success && Regex.Match(e.NewTextValue, Expr(FieldType)).Success)
        {
            //MyEntry.CursorPosition = MyCursorPosition;
            return;
        }
        if (!Regex.Match(e.NewTextValue, Expr(FieldType)).Success)
        {
            MyCursorPosition = MyEntry.CursorPosition;
            MyEntry.TextColor = Colors.Red;
        }
        else
        {
            MyEntry.TextColor = App.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
        }
    }

    async void MyEntry_Completed(object? sender, EventArgs e)
    {
        MyEntry.Text = GetNameValidate(MyEntry.Text);
        MyEntry.IsEnabled = false;
        await Task.Delay(200);
        MyEntry.IsEnabled = true;
    }

    private void TapVisibilityPassword(object sender, TappedEventArgs e)
    {
        if (MyEntry.IsPassword)
        {
            VisibilityPassword.SetDynamicResource(Label.TextProperty, "IconVisibilityoff");
            MyEntry.IsPassword = false;
        }
        else
        {
            VisibilityPassword.SetDynamicResource(Label.TextProperty, "IconVisibility");
            MyEntry.IsPassword = true;
        }
    }

    public static string GetNameValidate(string text)
    {
        var sep = text.Split(" ");
        StringBuilder name = new();
    foreach (var item in sep)
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                name.Append(item).Append(" ");
            }
        }
        return name.ToString().TrimStart().TrimEnd();
    }

    static string Expr(FieldType type)
    {
        switch (type)
        {
            case FieldType.Name:
                return "^[a-zA-Z\\s]*$";
            case FieldType.Email:
                return @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,10}$";
            default:
                return ".";
        }
    }
}
