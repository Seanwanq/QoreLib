﻿using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace QoreLib.ViewModels.ControlsViewModels;

public partial class SideBarLeftViewModel : BarViewModelBase
{
    public ObservableCollection<SideBarButtonViewModel> Buttons { get; } =
        new ObservableCollection<SideBarButtonViewModel>();

    [ObservableProperty] private int _pageIndex = 0;

    [ObservableProperty] private string _greeting;
    public SideBarLeftViewModel()
    {
        IsActive = true;
        Buttons.Add(new SideBarButtonViewModel("按钮1", SideBarButtonClickedCommand, 0));
        Buttons.Add(new SideBarButtonViewModel("按钮2", SideBarButtonClickedCommand, 1));
        Buttons.Add(new SideBarButtonViewModel("按钮3", SideBarButtonClickedCommand, 2));
    }

    [RelayCommand]
    private void SideBarButtonClicked(int parameter)
    {
        switch (parameter)
        {
            case 0:
                PageIndex = 0;
                Greeting = "123";
                break;
            case 1:
                PageIndex = 1;
                Greeting = "345";
                break;
            case 2:
                PageIndex = 2;
                Greeting = "789";
                break;
        }
    }

    partial void OnPageIndexChanged(int value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<int>(value), "leftbarbuttonchannel");
    }
}