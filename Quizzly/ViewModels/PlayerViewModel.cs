using Quizzly.Command;
using Quizzly.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace Quizzly.ViewModels;
public class AnswerOption : ViewModelBase {
    public string Text { get; }
    public DelegateCommand SelectCommand { get; }

    public AnswerOption(string text, DelegateCommand selectCommand) {
        Text = text;
        SelectCommand = selectCommand;
    }
}

public class PlayerViewModel : ViewModelBase {
    public ObservableCollection<AnswerOption> Answers { get; } = new();
    public QuestionPackViewModel ActivePack => _mainVm.ActivePack!;
    public DelegateCommand BackCommand { get; }
    private readonly Random _rnd = new();
    private int[]? _shuffledIndices;
    private int _currentIndex = -1;
    private readonly MainWindowViewModel _mainVm;

    private Question? _currentQuestion;
    public Question? CurrentQuestion {
        get => _currentQuestion;
        set {
            _currentQuestion = value;
            RaisePropertyChanged(nameof(CurrentQuestion));
            LoadAnswers();
        }
    }

    public PlayerViewModel(MainWindowViewModel mainVm) {
        _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        BackCommand = new DelegateCommand(_ => _mainVm.SwitchToConfiguration());
    }

    public void StartQuiz() {
        if(ActivePack?.Questions == null || ActivePack.Questions.Count == 0) {
            MessageBox.Show("No questions in this pack!");
            _mainVm.SwitchToConfiguration();
            return;
        }
        _shuffledIndices = Enumerable.Range(0, ActivePack.Questions.Count)
                                    .OrderBy(_ => _rnd.Next())
                                    .ToArray();
        _currentIndex = 0;
        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion() {
        if(_shuffledIndices == null || _currentIndex >= _shuffledIndices.Length) {
            MessageBox.Show("Quiz Complete! Well done!");
            _mainVm.SwitchToConfiguration();
            return;
        }
        int idx = _shuffledIndices[_currentIndex];
        CurrentQuestion = ActivePack.Questions[idx];
    }

    private void LoadAnswers() {
        Answers.Clear();
        if(CurrentQuestion == null) return;
        var options = new[] { CurrentQuestion.CorrectAnswer }
            .Concat(CurrentQuestion.IncorrectAnswers)
            .OrderBy(_ => _rnd.Next())
            .ToList();
        var selectCmd = new DelegateCommand(answerObj => {
            var answer = (string)answerObj!;
            bool isCorrect = answer == CurrentQuestion.CorrectAnswer;
            MessageBox.Show(isCorrect ? "Correct!" : $"Wrong! Correct answer: {CurrentQuestion.CorrectAnswer}");
            _currentIndex++;
            ShowCurrentQuestion();
        });
        foreach(var text in options)
            Answers.Add(new AnswerOption(text, selectCmd));
    }
}