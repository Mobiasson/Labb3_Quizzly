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

namespace Quizzly.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        private readonly HttpClient http = new();
        private Difficulty _selectedDifficulty = Difficulty.Medium;
        private QuestionPackViewModel? _activePack;
        private CategoryItem? _selectedCategory;
        private object? _currentView;
        private Question? _currentQuestion;

        private readonly string _filePath;
        public ObservableCollection<QuestionPackViewModel> Packs { get; } = new();
        public ObservableCollection<CategoryItem> Categories { get; } = new();
        public ConfigurationViewModel ConfigVM { get; }
        public PlayerViewModel PlayerVM { get; }
        public MenuViewModel MenuVM { get; }
        public ConfigurationView ConfigView { get; }
        public PlayerView PlayerViewInstance { get; }
        public DelegateCommand RemoveQuestionCommand { get; }
        public DelegateCommand RemovePackCommand { get; }
        public DelegateCommand PlayCommand { get; }

        public MainWindowViewModel() {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Quizzly");
            _filePath = Path.Combine(folder, "packs.json");
            Directory.CreateDirectory(folder);
            RemoveQuestionCommand = new DelegateCommand(RemoveQuestionExecute, CanRemoveQuestionExecute);
            RemovePackCommand = new DelegateCommand(RemovePackExecute, CanRemovePackExecute);
            PlayCommand = new DelegateCommand(ExecutePlay, CanPlay);
            ConfigVM = new ConfigurationViewModel(this);
            PlayerVM = new PlayerViewModel(this);
            MenuVM = new MenuViewModel(this);
            ConfigView = new ConfigurationView { DataContext = ConfigVM };
            PlayerViewInstance = new PlayerView { DataContext = PlayerVM };
            CurrentView = ConfigView;
            LoadPacks();
            _ = LoadCategoriesAsync();
            if(Packs.Count == 0) {
                var defaultPack = new QuestionPack("Default Pack");
                var packVm = new QuestionPackViewModel(defaultPack, RemovePackCommand);
                Packs.Add(packVm);
                ActivePack = packVm;
                _ = GetQuestionsFromDatabase();
            } else {
                ActivePack = Packs[0];
            }
            PickRandomQuestion();
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
            var playerVM = new PlayerViewModel(this);
            playerVM.StartQuiz();
            CurrentView = new PlayerView { DataContext = playerVM };
        }

        private bool CanPlay(object? param) => ActivePack?.Questions.Count > 0;

        public void PickRandomQuestion() {
            if(ActivePack?.Questions.Count > 0) {
                var rnd = new Random();
                int index = rnd.Next(ActivePack.Questions.Count);
                CurrentQuestion = ActivePack.Questions[index];
            } else {
                CurrentQuestion = null;
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
                _activePack = value;
                RaisePropertyChanged();
                PickRandomQuestion();
            }
        }

        private void RemoveQuestionExecute(object? param) {
            if(ActivePack?.SelectedQuestion != null) {
                ActivePack.Questions.Remove(ActivePack.SelectedQuestion);
                ActivePack.SelectedQuestion = null;
                RemoveQuestionCommand.RaiseCanExecuteChanged();
            }
        }

        private void RemovePackExecute(object? param) {
            if(param is QuestionPackViewModel packVm && Packs.Contains(packVm)) {
                Packs.Remove(packVm);
                ActivePack = Packs.FirstOrDefault();
            }
        }

        public async Task GetQuestionsFromDatabase() {
            if(ActivePack == null) return;
            string diff = _selectedDifficulty.ToString().ToLower();
            string url = $"https://opentdb.com/api.php?amount=10&type=multiple&difficulty={diff}";
            string json = await http.GetStringAsync(url);
            var result = JsonConvert.DeserializeObject<ReadJson>(json);
            if(result?.results == null || result.results.Count == 0) return;
            string category = HtmlDecode(result.results[0].category);
            ActivePack.Category = category;
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
        }

        public async Task LoadCategoriesAsync() {
            const string url = "https://opentdb.com/api_category.php";
            string json = await http.GetStringAsync(url);
            var resp = JsonConvert.DeserializeObject<CategoryResponse>(json);
            Categories.Clear();
            foreach(var c in resp?.trivia_categories ?? Enumerable.Empty<CategoryItem>())
                Categories.Add(c);
        }

        private void SavePacks() {
            var raw = Packs.Select(vm => vm.Model).ToList();
            var json = JsonConvert.SerializeObject(raw, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        private void LoadPacks() {
            if(!File.Exists(_filePath)) return;
            var json = File.ReadAllText(_filePath).Trim();
            if(string.IsNullOrWhiteSpace(json) || json == "[]") return;

            var packs = JsonConvert.DeserializeObject<List<QuestionPack>>(json);
            if(packs == null) return;

            foreach(var p in packs) {
                var vm = new QuestionPackViewModel(p, RemovePackCommand);
                Packs.Add(vm);
            }
        }
        private bool CanRemovePackExecute(object? param) => param is QuestionPackViewModel;
        private bool CanRemoveQuestionExecute(object? param) => ActivePack?.SelectedQuestion != null;
        public void SwitchToPlayer() => CurrentView = PlayerViewInstance;
        public void SwitchToConfiguration() => CurrentView = ConfigView;
        public void OnWindowClosing() => SavePacks();
        private static string HtmlDecode(string text) => WebUtility.HtmlDecode(text);
    }
}