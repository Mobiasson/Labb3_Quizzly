using Quizzly.Command;

namespace Quizzly.ViewModels;
public class PlayerViewModel : ViewModelBase {
    private readonly MainWindowViewModel? _mainWindowViewModel;

    public DelegateCommand SetPackNameCommand { get; }
    public QuestionPackViewModel? ActivePack { get => _mainWindowViewModel?.ActivePack; }
    public PlayerViewModel(MainWindowViewModel? mainWindowViewModel) {
        this._mainWindowViewModel = mainWindowViewModel;
    }
}