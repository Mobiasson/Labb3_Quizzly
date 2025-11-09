using Newtonsoft.Json;
using Quizzly.Command;
using Quizzly.Http;
using Quizzly.Models;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace Quizzly.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    private HttpClient http = new();
    private Difficulty _selectedDifficulty = Difficulty.Medium; // Needs to be changed later
    private QuestionPackViewModel? _activePack;
    public ObservableCollection<QuestionPackViewModel> Packs { get; } = new();
    public ObservableCollection<CategoryItem> Categories { get; } = new();
    public PlayerViewModel? PlayerViewModel { get; }
    public ConfigurationViewModel? ConfigurationViewModel { get; }
    public MenuViewModel? MenuViewModel { get; }
    public DelegateCommand RemoveQuestionCommand { get; }

    public MainWindowViewModel() {
        PlayerViewModel = new PlayerViewModel(this);
        ConfigurationViewModel = new ConfigurationViewModel(this);
        MenuViewModel = new MenuViewModel(this);
        RemoveQuestionCommand = new DelegateCommand(RemoveQuestionExecute, CanRemoveQuestionExecute);
        _ = LoadCategoriesAsync();
        _ = GetQuestionsFromDatabase();
        var pack = new QuestionPack("Default Pack");
        var packVm = new QuestionPackViewModel(pack, RemoveQuestionCommand);
        ActivePack = packVm;
        Packs.Add(packVm);
    }

    public QuestionPackViewModel? ActivePack {
        get => _activePack;
        set {
            _activePack = value;
            RaisePropertyChanged();
            RemoveQuestionCommand.RaiseCanExecuteChanged();
        }
    }

    private CategoryItem? _selectedCategory;
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

    public async Task GetQuestionsFromDatabase() {
        string diff = SelectedDifficulty.ToString().ToLower();
        string url = $"https://opentdb.com/api.php?amount=10&type=multiple&difficulty={diff}";
        string json = await http.GetStringAsync(url);
        var result = JsonConvert.DeserializeObject<ReadJson>(json);
        string category = result?.results.Count > 0
            ? HtmlDecode(result.results[0].category)
            : "No Category";
        ActivePack.Questions.Clear();
        foreach(var q in result?.results ?? Enumerable.Empty<GetQuestionsFromAPI>()) {
            ActivePack?.Questions.Add(new Question(
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
        foreach(var category in resp?.trivia_categories ?? Enumerable.Empty<CategoryItem>())
            Categories.Add(category);
    }


    private static string HtmlDecode(string text) => System.Net.WebUtility.HtmlDecode(text);
}