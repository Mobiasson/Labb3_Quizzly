using Quizzly.Command;
using Quizzly.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Quizzly.ViewModels;
public class AnswerOption : ViewModelBase {
    private static readonly Brush DefaultBackground = Brushes.LightGray;
    private Brush _background = DefaultBackground;
    public string Text { get; }
    public DelegateCommand SelectCommand { get; }

    public Brush Background {
        get => _background;
        set {
            if(_background != value) {
                _background = value;
                RaisePropertyChanged(nameof(Background));
            }
        }
    }

    public AnswerOption(string text, DelegateCommand selectCommand) {
        Text = text;
        SelectCommand = selectCommand;
    }
}

public class PlayerViewModel : ViewModelBase {
    private readonly MainWindowViewModel _mainVm;
    private readonly Random _rnd = new();
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _stopwatch;
    private int[]? shuffles;
    private int _currentIndex = -1;
    private int _initialSeconds = 30;
    private TimeSpan _timeRemaining = TimeSpan.Zero;
    public ObservableCollection<AnswerOption> Answers { get; } = new();
    public QuestionPackViewModel ActivePack => _mainVm.ActivePack!;
    public DelegateCommand BackCommand { get; }
    private Question? _currentQuestion;

    public Question? CurrentQuestion {
        get => _currentQuestion;
        set {
            _currentQuestion = value;
            RaisePropertyChanged(nameof(CurrentQuestion));
            LoadAnswers();
        }
    }

    public TimeSpan TimeRemaining {
        get => _timeRemaining;
        private set {
            if(_timeRemaining != value) {
                _timeRemaining = value;
                RaisePropertyChanged(nameof(TimeRemaining));
                RaisePropertyChanged(nameof(TimeRemainingDisplay));
            }
        }
    }
    public string TimeRemainingDisplay => TimeRemaining.ToString(@"mm\:ss");

    public PlayerViewModel(MainWindowViewModel mainVm) {
        _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        BackCommand = new DelegateCommand(_ => _mainVm.SwitchToConfiguration());
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _timer.Tick += Timer_Tick;
        _stopwatch = new Stopwatch();
    }

    public void StartQuiz() {
        if(ActivePack?.Questions == null || ActivePack.Questions.Count == 0) {
            MessageBox.Show("No questions in this pack!");
            _mainVm.SwitchToConfiguration();
            return;
        }
        shuffles = Enumerable.Range(0, ActivePack.Questions.Count)
                                    .OrderBy(_ => _rnd.Next())
                                    .ToArray();
        _currentIndex = 0;
        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion() {
        if(shuffles == null || _currentIndex >= shuffles.Length) {
            StopTimer();
            MessageBox.Show("Quiz Complete! Well done!");
            _mainVm.SwitchToConfiguration();
            return;
        }
        int idx = shuffles[_currentIndex];
        CurrentQuestion = ActivePack.Questions[idx];
        _initialSeconds = ActivePack?.TimeLimitInSeconds ?? 30;
        StartTimer(_initialSeconds);
    }

    private void LoadAnswers() {
        Answers.Clear();
        if(CurrentQuestion == null) return;

        var options = new[] { CurrentQuestion.CorrectAnswer }
            .Concat(CurrentQuestion.IncorrectAnswers)
            .OrderBy(_ => _rnd.Next())
            .ToList();
        var selectCmd = new DelegateCommand(async answerObj => {
            StopTimer();

            var clickedText = (string)answerObj!;
            bool isCorrect = clickedText == CurrentQuestion.CorrectAnswer;
            foreach(var opt in Answers) opt.Background = Brushes.LightGray;
            var clicked = Answers.FirstOrDefault(o => o.Text == clickedText);
            if(clicked != null) {
                clicked.Background = isCorrect ? Brushes.Green : Brushes.IndianRed;
            }
            await Task.Delay(900);
            _currentIndex++;
            ShowCurrentQuestion();
        });

        foreach(var text in options)
            Answers.Add(new AnswerOption(text, selectCmd));
    }

    private void StartTimer(int seconds) {
        TimeRemaining = TimeSpan.FromSeconds(seconds);
        _stopwatch.Restart();
        if(!_timer.IsEnabled) _timer.Start();
    }

    private void StopTimer() {
        if(_timer.IsEnabled) _timer.Stop();
        _stopwatch.Stop();
    }

    private void Timer_Tick(object? sender, EventArgs e) {
        var elapsed = _stopwatch.Elapsed;
        var remaining = TimeSpan.FromSeconds(_initialSeconds) - elapsed;
        if(remaining <= TimeSpan.Zero) {
            TimeRemaining = TimeSpan.Zero;
            StopTimer();
            OnTimeExpired();

        } else {
            TimeRemaining = remaining;
        }
    }

    private void OnTimeExpired() {
        MessageBox.Show("Time is up you suck", "You ran out of time!", MessageBoxButton.OK, MessageBoxImage.Information);
        _currentIndex++;
        ShowCurrentQuestion();
    }
}