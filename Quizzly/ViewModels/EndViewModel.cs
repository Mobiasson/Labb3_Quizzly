
namespace Quizzly.ViewModels;
public class EndViewModel : ViewModelBase {
    private readonly MainWindowViewModel _mainVM;
    public EndViewModel(MainWindowViewModel mainVM) {
        _mainVM = mainVM;
    }
}
