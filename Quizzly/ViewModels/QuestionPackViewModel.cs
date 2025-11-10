using Quizzly.Command;
using Quizzly.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Quizzly.ViewModels;
public class QuestionPackViewModel : ViewModelBase {

    private readonly MainWindowViewModel mainWindowViewModel;
    private readonly QuestionPack _model;
    private readonly DelegateCommand _removePackCommand;
    private Question? _selectedQuestion;
    public QuestionPack Model => _model;
    public ObservableCollection<Question> Questions { get; }


    public QuestionPackViewModel(MainWindowViewModel mainVm, QuestionPack model, DelegateCommand removePackCommand) {
        mainWindowViewModel = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _removePackCommand = removePackCommand ?? throw new ArgumentNullException(nameof(removePackCommand));
        Questions = new ObservableCollection<Question>(model.Questions);
        Questions.CollectionChanged += Questions_CollectionChanged;
    }


    public string Name {
        get => _model.Name;
        set { _model.Name = value; RaisePropertyChanged(); }
    }

    public string Category {
        get => _model.Category;
        set { _model.Category = value; RaisePropertyChanged(); }
    }

    public Difficulty Difficulty {
        get => _model.Difficulty;
        set { _model.Difficulty = value; RaisePropertyChanged(); }
    }

    public int TimeLimitInSeconds {
        get => _model.TimeLimitInSeconds;
        set { _model.TimeLimitInSeconds = value; RaisePropertyChanged(); }
    }

    public Question? SelectedQuestion {
        get => _selectedQuestion;
        set {
            if(_selectedQuestion == value) return;
            _selectedQuestion = value;
            _model.SelectedQuestion = value;
            RaisePropertyChanged();
            mainWindowViewModel.RemoveQuestionCommand.RaiseCanExecuteChanged();
        }
    }

    private void Questions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        switch(e.Action) {
            case NotifyCollectionChangedAction.Add when e.NewItems != null:
                foreach(Question q in e.NewItems)
                    _model.Questions.Add(q);
                break;
            case NotifyCollectionChangedAction.Remove when e.OldItems != null:
                foreach(Question q in e.OldItems)
                    _model.Questions.Remove(q);
                break;
            case NotifyCollectionChangedAction.Replace when e.OldItems != null && e.NewItems != null:
                _model.Questions[e.OldStartingIndex] = (Question)e.NewItems[0]!;
                break;
            case NotifyCollectionChangedAction.Reset:
                _model.Questions.Clear();
                foreach(Question q in Questions)
                    _model.Questions.Add(q);
                break;
        }
        mainWindowViewModel.RemoveQuestionCommand.RaiseCanExecuteChanged();
    }
}