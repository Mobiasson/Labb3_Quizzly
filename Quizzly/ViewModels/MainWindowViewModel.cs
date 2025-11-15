using Newtonsoft.Json;
using Quizzly.Command;
using Quizzly.Http;
using Quizzly.Models;
using Quizzly.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;

namespace Quizzly.ViewModels;
public class MainWindowViewModel : ViewModelBase {
    private readonly HttpClient http = new();
    private readonly string _filePath;
    private object? _currentView;
    private Difficulty _selectedDifficulty = Difficulty.Medium;
    private CategoryItem _selectedCategory = new CategoryItem { id = 5 };
    private int _selectedAmount = 10;
    private QuestionPackViewModel? _activePack;
    private Question? _currentQuestion;
    private readonly SemaphoreSlim _importLock = new(1, 1);
    private PlayerViewModel? _runningPlayer;
    private bool _isPlaying;

    public IEnumerable<Difficulty> Difficulties => Enum.GetValues<Difficulty>();
    public ObservableCollection<QuestionPackViewModel> Packs { get; } = new();
    public ObservableCollection<CategoryItem> Categories { get; } = new();
    public ConfigurationViewModel ConfigVM { get; }
    public PlayerViewModel PlayerVM { get; }
    public MenuViewModel MenuVM { get; }
    public EndViewModel EndVM { get; }
    public EndView EndView { get; }
    public ConfigurationView ConfigView { get; }
    public PlayerView PlayerView { get; }
    public MenuView MenuViewInstance { get; }
    public DelegateCommand RemoveQuestionCommand { get; }
    public DelegateCommand RemovePackCommand { get; }
    public DelegateCommand PlayCommand { get; }
    public DelegateCommand AddQuestionCommand { get; }
    public DelegateCommand ChangePackNameCommand { get; }
    public DelegateCommand StopPlayingCommand { get; }

    public MainWindowViewModel() {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Quizzly");
        _filePath = Path.Combine(folder, "packs.json");
        Directory.CreateDirectory(folder);
        RemoveQuestionCommand = new DelegateCommand(RemoveQuestionExecute, CanRemoveQuestionExecute);
        RemovePackCommand = new DelegateCommand(RemovePackExecute, CanRemovePackExecute);
        PlayCommand = new DelegateCommand(ExecutePlay, CanPlay);
        ChangePackNameCommand = new DelegateCommand(ChangePackName, CanChangePackName);
        AddQuestionCommand = new DelegateCommand(AddQuestion, CanAddQuestion);
        StopPlayingCommand = new DelegateCommand(_ => StopPlaying(), _ => IsPlaying);
        ConfigVM = new ConfigurationViewModel(this);
        PlayerVM = new PlayerViewModel(this);
        MenuVM = new MenuViewModel(this);
        ConfigView = new ConfigurationView { DataContext = ConfigVM };
        PlayerView = new PlayerView { DataContext = PlayerVM };
        EndView = new EndView { DataContext = EndVM };
        MenuViewInstance = new MenuView { DataContext = MenuVM };
        CurrentView = ConfigView;
        LoadPacks();
        _ = LoadCategoriesAsync();
        if(Packs.Count == 0) {
            var defaultPack = new QuestionPack("Default Pack");
            var packVm = CreatePackVm(defaultPack);
            Packs.Add(packVm);
            ActivePack = packVm;
        } else {
            ActivePack = Packs[0];
        }
        PickRandomQuestion();
    }


    private void AddQuestion(object? obj) {
        if(ActivePack == null) return;
        if(ActivePack.Questions.Count >= 20) {
            MessageBox.Show("You cannot add above 20 questions, remove one to add another.");
            return;
        }
        var confirm = MessageBox.Show(
            "Are you sure you want to add a new blank question to the active pack?",
            "Add Question",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if(confirm != MessageBoxResult.Yes) return;
        var newQuestion = new Question("", "", "", "", "");
        ActivePack.Questions.Add(newQuestion);
        ActivePack.SelectedQuestion = newQuestion;
        ConfigView?.ClearTextBoxes();
        SavePacks();
        RemoveQuestionCommand.RaiseCanExecuteChanged();
        AddQuestionCommand.RaiseCanExecuteChanged();
        PlayCommand.RaiseCanExecuteChanged();
    }

    public QuestionPackViewModel CreatePackVm(QuestionPack model) {
        return new QuestionPackViewModel(this, model, RemovePackCommand);
    }

    public void AddAndActivatePack(string name, Difficulty difficulty, int timeLimitInSeconds, CategoryItem? category, bool overwriteActive = false) {
        if(overwriteActive && ActivePack != null) {
            var m = ActivePack.Model;
            m.Name = name;
            m.Difficulty = difficulty;
            m.TimeLimitInSeconds = timeLimitInSeconds;
            m.CategoryId = category?.id ?? m.CategoryId;
            m.Category = category?.name ?? m.Category;
            m.Questions = new List<Question>();
            ActivePack.Name = m.Name;
            ActivePack.Difficulty = m.Difficulty;
            ActivePack.TimeLimitInSeconds = m.TimeLimitInSeconds;
            ActivePack.CategoryId = m.CategoryId;
            ActivePack.Category = m.Category;
            ActivePack.Questions.Clear();
            SavePacks();
            PlayCommand.RaiseCanExecuteChanged();
            return;
        }
        var model = new QuestionPack(name, difficulty, timeLimitInSeconds, category?.id ?? 5) {
            Category = category?.name ?? string.Empty,
            Questions = new List<Question>()
        };
        var vm = CreatePackVm(model);
        Packs.Add(vm);
        ActivePack = vm;
        SavePacks();
    }


    public int SelectedAmount {
        get => _selectedAmount;
        set {
            int clamped = Math.Clamp(value, 1, 20);
            if(_selectedAmount == clamped) return;
            _selectedAmount = clamped;
            RaisePropertyChanged(nameof(SelectedAmount));
        }
    }

    public Difficulty CurrentDifficulty {
        get => _selectedDifficulty;
        set {
            if(_selectedDifficulty == value) return;
            _selectedDifficulty = value;
            RaisePropertyChanged(nameof(CurrentDifficulty));
        }
    }

    public CategoryItem? CurrentCategory {
        get => _selectedCategory;
        set {
            _selectedCategory = value;
            RaisePropertyChanged(nameof(CurrentCategory));
        }
    }

    public Question? CurrentQuestion {
        get => _currentQuestion;
        set {
            _currentQuestion = value;
            RaisePropertyChanged(nameof(CurrentQuestion));
        }
    }

    public object? CurrentView {
        get => _currentView;
        set { _currentView = value; RaisePropertyChanged(); }
    }

    public QuestionPackViewModel? ActivePack {
        get => _activePack;
        set {
            if(_activePack == value) return;
            _activePack = value;
            RaisePropertyChanged();
            if(value != null) {
                int idx = Packs.IndexOf(value);
                if(idx > 0) {
                    Packs.Move(idx, 0);
                }
                SavePacks();
            }
            PickRandomQuestion();
            RemoveQuestionCommand.RaiseCanExecuteChanged();
            AddQuestionCommand.RaiseCanExecuteChanged();
            PlayCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsPlaying {
        get => _isPlaying;
        private set {
            if(_isPlaying == value) return;
            _isPlaying = value;
            RaisePropertyChanged(nameof(IsPlaying));
            StopPlayingCommand.RaiseCanExecuteChanged();
        }
    }

    public async Task GetQuestionsFromDatabase() {
        if(ActivePack == null) return;
        if(_selectedCategory == null) return;
        await _importLock.WaitAsync();
        try {
            string diff = _selectedDifficulty.ToString().ToLower();
            int categoryId = _selectedCategory.id;
            int amount = _selectedAmount;
            string url = $"https://opentdb.com/api.php?amount={amount}&category={categoryId}&type=multiple&difficulty={diff}";
            string json = await http.GetStringAsync(url);
            var result = JsonConvert.DeserializeObject<ReadJson>(json);
            if(result?.results == null || result.results.Count == 0) return;
            string categoryName = HtmlDecode(result.results[0].category);
            ActivePack.Name = categoryName;
            ActivePack.Category = categoryName;
            ActivePack.CategoryId = categoryId;

            ActivePack.Questions.Clear();
            foreach(var q in result.results) {
                ActivePack.Questions.Add(new Question(
                    query: HtmlDecode(q.question),
                    correctAnswer: HtmlDecode(q.correct_answer),
                    incorrectAnswer1: HtmlDecode(q.incorrect_answers[0]),
                    incorrectAnswer2: HtmlDecode(q.incorrect_answers[1]),
                    incorrectAnswer3: HtmlDecode(q.incorrect_answers[2])
                ));
            }
            SavePacks();
            PlayCommand.RaiseCanExecuteChanged();
        }
        catch(HttpRequestException ex) {
            MessageBox.Show("Network error while loading questions. Try again.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally {
            _importLock.Release();
        }
    }

    private async void ExecutePlay(object? param) {
        if(ActivePack == null) return;
        if(ActivePack.Questions.Count == 0) {
            var result = MessageBox.Show("Load 10 questions from API?", "No Questions", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes) {
                await GetQuestionsFromDatabase();
            } else {
                return;
            }
        }
        _runningPlayer = new PlayerViewModel(this);
        _runningPlayer.StartQuiz();
        IsPlaying = true;
        CurrentView = new PlayerView { DataContext = _runningPlayer };
    }
    public async Task RestartAsync() {
        await LoadCategoriesAsync();
        if(ActivePack == null)
            ActivePack = Packs.FirstOrDefault();
        if(ActivePack == null || ActivePack.Questions.Count == 0) {
            await GetQuestionsFromDatabase();
        }
        var playerVM = new PlayerViewModel(this);
        playerVM.StartQuiz();
        CurrentView = new PlayerView { DataContext = playerVM };
    }

    public async Task LoadCategoriesAsync() {
        const string url = "https://opentdb.com/api_category.php";
        string json = await http.GetStringAsync(url);
        var resp = JsonConvert.DeserializeObject<CategoryResponse>(json);
        Categories.Clear();
        foreach(var c in resp?.trivia_categories ?? Enumerable.Empty<CategoryItem>())
            Categories.Add(c);
    }

    public void SavePacks() {
        try {
            foreach(var vm in Packs) {
                var m = vm.Model;
                m.Name = vm.Name;
                m.Category = vm.Category;
                m.CategoryId = vm.CategoryId;
                m.Difficulty = vm.Difficulty;
                m.TimeLimitInSeconds = vm.TimeLimitInSeconds;
                m.SelectedQuestion = vm.SelectedQuestion;
                m.Questions = vm.Questions.ToList();
            }

            var raw = Packs.Select(vm => vm.Model).ToList();
            var json = JsonConvert.SerializeObject(raw, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
        catch(Exception ex) {
            MessageBox.Show($"Failed saving packs.json: {ex.Message}", "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadPacks() {
        if(!File.Exists(_filePath)) return;
        var json = File.ReadAllText(_filePath).Trim();
        if(string.IsNullOrWhiteSpace(json) || json == "[]") return;
        var packs = JsonConvert.DeserializeObject<List<QuestionPack>>(json);
        if(packs == null) return;
        foreach(var p in packs) {
            Packs.Add(CreatePackVm(p));
        }
    }

    public void PickRandomQuestion() {
        if(ActivePack?.Questions.Count > 0) {
            var rnd = new Random();
            int index = rnd.Next(ActivePack.Questions.Count);
            CurrentQuestion = ActivePack.Questions[index];
        } else {
            CurrentQuestion = null;
        }
    }
    private void RemoveQuestionExecute(object? param) {
        if(ActivePack?.SelectedQuestion != null) {
            ActivePack.Questions.Remove(ActivePack.SelectedQuestion);
            ActivePack.SelectedQuestion = null;
            RemoveQuestionCommand.RaiseCanExecuteChanged();
            PlayCommand.RaiseCanExecuteChanged();
        }
    }
    private void RemovePackExecute(object? param) {
        if(param is QuestionPackViewModel packVm && Packs.Contains(packVm)) {
            Packs.Remove(packVm);
            ActivePack = Packs.FirstOrDefault();
            PlayCommand.RaiseCanExecuteChanged();
        }
    }
    private void ChangePackName(object? param) {
        var newName = param as string;
        if(string.IsNullOrWhiteSpace(newName) || ActivePack == null) return;
        ActivePack.Name = newName.Trim();
        SavePacks();
    }

    public void StopPlaying() {
        if(!IsPlaying) { CurrentView = ConfigView; return; }
        _runningPlayer?.Stop();
        _runningPlayer = null;
        IsPlaying = false;
        CurrentView = ConfigView;
    }

    public void SwitchToEnd(int correctAnswers, int totalQuestions) {
        _runningPlayer?.Stop();
        _runningPlayer = null;
        IsPlaying = false;
        CurrentView = new EndView { DataContext = new EndViewModel(this, correctAnswers, totalQuestions) };
    }

    private bool CanRemovePackExecute(object? param) => param is QuestionPackViewModel;
    private bool CanPlay(object? param) => ActivePack?.Questions.Count > 0;
    private bool CanRemoveQuestionExecute(object? param) => ActivePack?.SelectedQuestion != null;
    private bool CanChangePackName(object? arg) => ActivePack != null;
    public void SwitchToPlayer() => CurrentView = PlayerView;
    public void SwitchToConfiguration() => CurrentView = ConfigView;
    public void SwitchToEnd() => CurrentView = EndView;
    public void OnWindowClosing() => SavePacks();
    public void SetSelectedAmount(int amount) => SelectedAmount = amount;
    private static string HtmlDecode(string text) => WebUtility.HtmlDecode(text);
    private bool CanAddQuestion(object? arg) => ActivePack != null;


}