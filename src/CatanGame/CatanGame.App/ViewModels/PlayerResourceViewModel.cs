namespace CatanGame.App.ViewModels;

public class PlayerResourceViewModel : ViewModelBase
{
    private string _playerName;
    public string PlayerName
    {
        get => _playerName;
        set => SetProperty(ref _playerName, value);
    }

    private string _colorBrush;
    public string ColorBrush
    {
        get => _colorBrush;
        set => SetProperty(ref _colorBrush, value);
    }

    private string _resources;
    public string Resources
    {
        get => _resources;
        set => SetProperty(ref _resources, value);
    }

    public PlayerResourceViewModel()
    {
        _playerName = "";
        _colorBrush = "";
        _resources = "";
    }
}
