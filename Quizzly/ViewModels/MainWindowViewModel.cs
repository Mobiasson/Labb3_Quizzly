using Newtonsoft.Json;
using Quizzly.Command;
using Quizzly.Http;
using Quizzly.Models;
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
        private readonly string _filePath;

        public ObservableCollection<QuestionPackViewModel> Packs { get; } = new();
        public ObservableCollection<CategoryItem> Categories { get; } = new();
        public PlayerViewModel? PlayerViewModel { get; }
        public ConfigurationViewModel? ConfigurationViewModel { get; }
        public MenuViewModel? MenuViewModel { get; }
        public DelegateCommand RemoveQuestionCommand { get; }
        public DelegateCommand RemovePackCommand { get; }

        public MainWindowViewModel() {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Quizzly");
            _filePath = Path.Combine(folder, "packs.json");
            Directory.CreateDirectory(folder);
            RemoveQuestionCommand = new DelegateCommand(RemoveQuestionExecute, CanRemoveQuestionExecute);
            RemovePackCommand = new DelegateCommand(RemovePackExecute, CanRemovePackExecute);
            PlayerViewModel = new PlayerViewModel(this);
            ConfigurationViewModel = new ConfigurationViewModel(this);
            MenuViewModel = new MenuViewModel(this);
            LoadPacks();
            if(Packs.Count == 0) {
                var defaultPack = new QuestionPack("Default Pack");
                var packVm = new QuestionPackViewModel(defaultPack, RemovePackCommand);
                Packs.Add(packVm);
                ActivePack = packVm;
                _ = GetQuestionsFromDatabase();
            }
            else {
                ActivePack = Packs[0];
            }

            _ = LoadCategoriesAsync();
        }

        public QuestionPackViewModel? ActivePack {
            get => _activePack;
            set {
                _activePack = value;
                RaisePropertyChanged();
                RemoveQuestionCommand.RaiseCanExecuteChanged();
            }
        }

        public CategoryItem? SelectedCategory {
            get => _selectedCategory;
            set {
                _selectedCategory = value;
                RaisePropertyChanged();
            }
        }

        public Difficulty SelectedDifficulty {
            get => _selectedDifficulty;
            set {
                _selectedDifficulty = value;
                RaisePropertyChanged();
                _ = GetQuestionsFromDatabase();
            }
        }

        private void RemoveQuestionExecute(object? param) {
            if(ActivePack?.SelectedQuestion != null) {
                ActivePack.Questions.Remove(ActivePack.SelectedQuestion);
                ActivePack.SelectedQuestion = null;
                RemoveQuestionCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanRemoveQuestionExecute(object? param) => ActivePack?.SelectedQuestion != null;

        private void RemovePackExecute(object? param) {
            if(param is QuestionPackViewModel packVm && Packs.Contains(packVm)) {
                Packs.Remove(packVm);
                if(ActivePack == packVm)
                    ActivePack = Packs.FirstOrDefault();
            }
        }

        private bool CanRemovePackExecute(object? param) => param is QuestionPackViewModel;

        public async Task GetQuestionsFromDatabase() {
            if(ActivePack == null) return;
            string diff = SelectedDifficulty.ToString().ToLower();
            string url = $"https://opentdb.com/api.php?amount=10&type=multiple&difficulty={diff}";
            string json = await http.GetStringAsync(url);
            var result = JsonConvert.DeserializeObject<ReadJson>(json);
            if(result?.results == null || result.results.Count == 0) {
                MessageBox.Show("No questions from API.");
                return;
            }
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
            MessageBox.Show($"Loaded 10 questions: {category}");
        }

        public async Task LoadCategoriesAsync() {
            const string url = "https://opentdb.com/api_category.php";
            string json = await http.GetStringAsync(url);
            var resp = JsonConvert.DeserializeObject<CategoryResponse>(json);
            Categories.Clear();
            foreach(var category in resp?.trivia_categories ?? Enumerable.Empty<CategoryItem>())
                Categories.Add(category);
        }

        private void SavePacks() {
            var rawPacks = Packs.Select(vm => vm.Model).ToList();
            var json = JsonConvert.SerializeObject(rawPacks, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        private void LoadPacks() {
            if(!File.Exists(_filePath)) return;
            var json = File.ReadAllText(_filePath).Trim();
            if(string.IsNullOrWhiteSpace(json) || json == "[]") return;
            var packs = JsonConvert.DeserializeObject<List<QuestionPack>>(json);
            if(packs == null || packs.Count == 0) return;
            foreach(var p in packs) {
                var vm = new QuestionPackViewModel(p, RemovePackCommand);
                Packs.Add(vm);
            }
        }

        private static string HtmlDecode(string text) => WebUtility.HtmlDecode(text);

        public void OnWindowClosing() {
            SavePacks();
        }
    }
}