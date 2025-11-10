using Quizzly.Command;
using Quizzly.Models;
using System.Collections.ObjectModel;

namespace Quizzly.ViewModels;
public class ConfigurationViewModel : ViewModelBase {
    private readonly MainWindowViewModel? mainWindowViewModel;
    private readonly MainWindowViewModel _mainVm;
    public DelegateCommand LoadTenQuestionsCommand { get; }

    public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel) {
        this.mainWindowViewModel = mainWindowViewModel;
        _mainVm = mainWindowViewModel ?? throw new ArgumentNullException(nameof(mainWindowViewModel));
        LoadTenQuestionsCommand = new DelegateCommand(_ => mainWindowViewModel?.GetQuestionsFromDatabase());
    }

    public QuestionPackViewModel? ActivePack {
        get => mainWindowViewModel?.ActivePack;
    }

    public ObservableCollection<Question> Questions {
        get => ActivePack?.Questions ?? new ObservableCollection<Question>();
    }

    public Question? SelectedQuestion {
        get => mainWindowViewModel?.ActivePack?.SelectedQuestion;
        set {
            if(mainWindowViewModel?.ActivePack != null) mainWindowViewModel.ActivePack.SelectedQuestion = value;
        }
    }

    public DelegateCommand? RemoveQuestionCommand => mainWindowViewModel?.RemoveQuestionCommand;
}
