using Quizzly.Command;
using Quizzly.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Quizzly.ViewModels {
    public class QuestionPackViewModel : ViewModelBase {
        private readonly QuestionPack _model;
        private readonly DelegateCommand? _removeCommand;
        private readonly DelegateCommand _removePackCommand;
        private Question? _selectedQuestion;

        public QuestionPackViewModel(QuestionPack model, DelegateCommand removePackCommand) {
            _model = model;
            _removePackCommand = removePackCommand;
            Questions = new ObservableCollection<Question>(model.Questions);
            Questions.CollectionChanged += Questions_CollectionChanged;
            PropertyChanged += (s, e) => {
                if(e.PropertyName == nameof(SelectedQuestion))
                    _removePackCommand?.RaiseCanExecuteChanged();
            };
        }

        public QuestionPack Model => _model;

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
                _selectedQuestion = value;
                _model.SelectedQuestion = value;
                RaisePropertyChanged();
                var mainVm = Application.Current.MainWindow?.DataContext as MainWindowViewModel;
                mainVm?.RemoveQuestionCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<Question> Questions { get; }

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
                    break;
            }
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(SelectedQuestion))
                _removeCommand?.RaiseCanExecuteChanged();
        }
    }
}