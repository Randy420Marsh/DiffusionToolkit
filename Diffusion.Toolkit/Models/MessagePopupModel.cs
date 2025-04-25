﻿using System.Windows;
using System.Windows.Input;

namespace Diffusion.Toolkit.Models;

public class MessagePopupModel : BaseNotify
{
    private ICommand _okCommand;
    private bool _isVisible;
    private string _message;
    private string _title;
    private ICommand _cancelCommand;
    private ICommand _yesCommand;
    private ICommand _noCommand;
    private bool _hasOk;
    private bool _hasCancel;
    private bool _hasYes;
    private bool _hasNo;

    private double _height;
    private double _width;
    private UIElement _placementTarget;
    private string? _input;
    private bool _showInput;


    public MessagePopupModel()
    {

    }

    public UIElement PlacementTarget
    {
        get => _placementTarget;
        set => SetField(ref _placementTarget, value);
    }


    public bool IsVisible
    {
        get => _isVisible;
        set => SetField(ref _isVisible, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }

    public ICommand OKCommand
    {
        get => _okCommand;
        set => SetField(ref _okCommand, value);
    }

    public ICommand CancelCommand
    {
        get => _cancelCommand;
        set => SetField(ref _cancelCommand, value);
    }

    public ICommand YesCommand
    {
        get => _yesCommand;
        set => SetField(ref _yesCommand, value);
    }

    public ICommand NoCommand
    {
        get => _noCommand;
        set => SetField(ref _noCommand, value);
    }

    public bool HasOk
    {
        get => _hasOk;
        set => SetField(ref _hasOk, value);
    }

    public bool HasCancel
    {
        get => _hasCancel;
        set => SetField(ref _hasCancel, value);
    }

    public bool HasYes
    {
        get => _hasYes;
        set => SetField(ref _hasYes, value);
    }

    public bool HasNo
    {
        get => _hasNo;
        set => SetField(ref _hasNo, value);
    }

    public double Width
    {
        get => _width;
        set => SetField(ref _width, value);
    }

    public double Height
    {
        get => _height;
        set => SetField(ref _height, value);
    }

    public string? Input
    {
        get => _input;
        set => SetField(ref _input, value);
    }

    public bool ShowInput
    {
        get => _showInput;
        set => SetField(ref _showInput, value);
    }
}