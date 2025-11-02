namespace Quizzly.ViewModels;
public class ConfigurationViewModel : ViewModelBase {
    private readonly MainWindowViewModel? mainWindowViewModel;

    public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel) {
        this.mainWindowViewModel = mainWindowViewModel;
    }
}
