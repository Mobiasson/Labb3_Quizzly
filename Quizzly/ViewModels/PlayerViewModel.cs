using Quizzly.Command;
using Quizzly.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private Question? _currentQuestion;
    private int _correctAnswers;
    public ObservableCollection<AnswerOption> Answers { get; } = new();
    public DelegateCommand BackCommand { get; }


    public Question? CurrentQuestion {
        get => _currentQuestion;
        set {
            _currentQuestion = value;
            RaisePropertyChanged(nameof(CurrentQuestion));
            LoadAnswers();
            RaisePropertyChanged(nameof(QuestionProgress));
            RaisePropertyChanged(nameof(CurrentQuestionNumber));
            RaisePropertyChanged(nameof(TotalQuestions));
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

    private bool _hasAnsweredCurrent;
    private bool _isStopped;
    private DelegateCommand? _answerSelectCommand;

    public PlayerViewModel(MainWindowViewModel mainVm) {
        _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        BackCommand = new DelegateCommand(_ => _mainVm.StopPlaying());
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _timer.Tick += Timer_Tick;
        _stopwatch = new Stopwatch();
    }

    public void Stop() {
        _isStopped = true;
        StopTimer();
        _answerSelectCommand?.RaiseCanExecuteChanged();
    }

    public void StartQuiz() {
        if(ActivePack?.Questions == null || ActivePack.Questions.Count == 0) {
            MessageBox.Show("No questions in this pack!");
            _mainVm.StopPlaying();
            return;
        }
        _isStopped = false;
        _correctAnswers = 0;
        shuffles = Enumerable.Range(0, ActivePack.Questions.Count)
                                    .OrderBy(_ => _rnd.Next())
                                    .ToArray();
        _currentIndex = 0;
        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion() {
        if(_isStopped) return;
        if(shuffles == null || _currentIndex >= shuffles.Length) {
            StopTimer();
            int total = shuffles?.Length ?? ActivePack.Questions.Count;
            _mainVm.SwitchToEnd(_correctAnswers, total);
            return;
        }
        int idx = shuffles[_currentIndex];
        _hasAnsweredCurrent = false;
        _answerSelectCommand?.RaiseCanExecuteChanged();
        CurrentQuestion = ActivePack.Questions[idx];
        _initialSeconds = ActivePack?.TimeLimitInSeconds ?? 30;
        StartTimer(_initialSeconds);
        RaisePropertyChanged(nameof(QuestionProgress));
        RaisePropertyChanged(nameof(CurrentQuestionNumber));
        RaisePropertyChanged(nameof(TotalQuestions));
    }

    private void LoadAnswers() {
        Answers.Clear();
        if(CurrentQuestion == null) return;

        var options = new[] { CurrentQuestion.CorrectAnswer }
            .Concat(CurrentQuestion.IncorrectAnswers)
            .OrderBy(_ => _rnd.Next())
            .ToList();

        _answerSelectCommand = new DelegateCommand(async answerObj => {
            if(_isStopped || _hasAnsweredCurrent) return;
            _hasAnsweredCurrent = true;
            _answerSelectCommand.RaiseCanExecuteChanged();

            StopTimer();
            var clickedText = (string)answerObj!;
            bool isCorrect = clickedText == CurrentQuestion!.CorrectAnswer;
            if(isCorrect) _correctAnswers++;

            foreach(var opt in Answers) opt.Background = Brushes.LightGray;
            var clicked = Answers.FirstOrDefault(o => o.Text == clickedText);
            if(clicked != null) {
                clicked.Background = isCorrect ? Brushes.Green : Brushes.IndianRed;
            }

            await Task.Delay(900);
            if(_isStopped) return;

            _currentIndex++;
            ShowCurrentQuestion();
        },
        _ => !_hasAnsweredCurrent && !_isStopped);

        foreach(var text in options)
            Answers.Add(new AnswerOption(text, _answerSelectCommand));
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
        _hasAnsweredCurrent = true;
        _answerSelectCommand?.RaiseCanExecuteChanged();

        MessageBox.Show("Time is up, can't you read?", "You ran out of time!", MessageBoxButton.OK, MessageBoxImage.Information);
        _currentIndex++;
        ShowCurrentQuestion();
    }

    public int CurrentQuestionNumber => (_currentIndex >= 0 && shuffles != null && _currentIndex < shuffles.Length) ? _currentIndex + 1 : 0;
    public int TotalQuestions => ActivePack?.Questions.Count ?? 0;
    public QuestionPackViewModel ActivePack => _mainVm.ActivePack!;
    public string TimeRemainingDisplay => TimeRemaining.ToString(@"mm\:ss");
    public string QuestionProgress => $"{CurrentQuestionNumber}/{TotalQuestions}";
}