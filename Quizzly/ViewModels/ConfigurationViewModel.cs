using Quizzly.Command;
using Quizzly.Models;
using System.Collections.ObjectModel;

namespace Quizzly.ViewModels;
public class ConfigurationViewModel : ViewModelBase {
    private readonly MainWindowViewModel? mainWindowViewModel;

    public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel) {
        this.mainWindowViewModel = mainWindowViewModel;
    }

    public QuestionPackViewModel? ActivePack {
        get => mainWindowViewModel?.ActivePack;
    }

    public ObservableCollection<Question> Questions {
        get => ActivePack?.Questions ?? new ObservableCollection<Question>();
    }

    public Question? SelectedQuestion {
        get => ActivePack?.SelectedQuestion;
        set {
            if(ActivePack != null) ActivePack.SelectedQuestion = value;
        }
    }

    public DelegateCommand RemoveQuestionCommand {
        get => mainWindowViewModel?.RemoveQuestionCommand ?? new DelegateCommand(_ => { });
    }
}
