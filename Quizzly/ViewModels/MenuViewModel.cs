using Quizzly.Command;
using Quizzly.Models;


namespace Quizzly.ViewModels;
public class MenuViewModel : ViewModelBase {
    public MainWindowViewModel MainWindowViewModel { get; }

    public MenuViewModel(MainWindowViewModel mainWindowViewModel) {
        MainWindowViewModel = mainWindowViewModel;
    }
}
