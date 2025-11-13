using Quizzly.Command;
using Quizzly.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Quizzly.ViewModels;
public class ConfigurationViewModel : ViewModelBase {
    private readonly MainWindowViewModel? mainWindowViewModel;
    private QuestionPackViewModel? _subscribedPack;
    public DelegateCommand LoadTenQuestionsCommand { get; }

    public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel) {
        this.mainWindowViewModel = mainWindowViewModel;
        LoadTenQuestionsCommand = new DelegateCommand(_ => this.mainWindowViewModel!.GetQuestionsFromDatabase());
        this.mainWindowViewModel.PropertyChanged += MainVm_PropertyChanged;
        if(this.mainWindowViewModel.ActivePack != null)
            SubscribeToActivePackQuestions(this.mainWindowViewModel.ActivePack);
    }

    private void MainVm_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(MainWindowViewModel.ActivePack)) {
            SubscribeToActivePackQuestions(mainWindowViewModel?.ActivePack);
            RaisePropertyChanged(nameof(ActivePack));
            RaisePropertyChanged(nameof(Questions));
            RaisePropertyChanged(nameof(SelectedQuestion));
            RaisePropertyChanged(nameof(QuestionCount));
        }
    }

    private void SubscribeToActivePackQuestions(QuestionPackViewModel? pack) {
        if(_subscribedPack == pack) return;
        if(_subscribedPack != null)
            _subscribedPack.Questions.CollectionChanged -= Questions_CollectionChanged;
        _subscribedPack = pack;
        if(_subscribedPack != null)
            _subscribedPack.Questions.CollectionChanged += Questions_CollectionChanged;
        RaisePropertyChanged(nameof(ActivePack));
        RaisePropertyChanged(nameof(Questions));
        RaisePropertyChanged(nameof(QuestionCount));
    }

    private void Questions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        RaisePropertyChanged(nameof(QuestionCount));
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
    public DelegateCommand? AddQuestionCommand => mainWindowViewModel?.AddQuestionCommand;
    public int QuestionCount => ActivePack?.Questions.Count ?? 0;
}
