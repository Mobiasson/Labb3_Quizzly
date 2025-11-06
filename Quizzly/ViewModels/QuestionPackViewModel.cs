using Quizzly.Command;
using Quizzly.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Quizzly.ViewModels;

public class QuestionPackViewModel : ViewModelBase {
    private readonly QuestionPack _model;
    private readonly DelegateCommand? _removeCommand;
    private Question? _selectedQuestion;
    public QuestionPackViewModel(QuestionPack model, DelegateCommand removeCommand) {
        _model = model;
        _removeCommand = removeCommand;
        Questions = new ObservableCollection<Question>(model.Questions);
        PropertyChanged += (s, e) =>
        {
            if(e.PropertyName == nameof(SelectedQuestion))
                _removeCommand?.RaiseCanExecuteChanged();
        };
    }

    private void Questions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if(e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            foreach(Question q in e.NewItems) _model.Questions.Add(q);

        if(e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            foreach(Question q in e.OldItems) _model.Questions.Remove(q);

        if(e.Action == NotifyCollectionChangedAction.Replace && e.OldItems != null && e.NewItems != null)
            _model.Questions[e.OldStartingIndex] = (Question)e.NewItems[0]!;

        if(e.Action == NotifyCollectionChangedAction.Reset)
            _model.Questions.Clear();
    }

    public string Name {
        get => _model.Name;
        set {
            _model.Name = value;
            RaisePropertyChanged();
        }
    }

    public string Category {
        get => _model.Category;
        set {
            _model.Category = value;
            RaisePropertyChanged();
        }
    }

    public Difficulty Difficulty {
        get => _model.Difficulty;
        set {
            _model.Difficulty = value;
            RaisePropertyChanged();
        }
    }

    public int TimeLimitInSeconds {
        get => _model.TimeLimitInSeconds;
        set {
            _model.TimeLimitInSeconds = value;
            RaisePropertyChanged();
        }
    }

    public Question? SelectedQuestion {
        get => _selectedQuestion;
        set {
            _selectedQuestion = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<Question> Questions { get; set; }
}
