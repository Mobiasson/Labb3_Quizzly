using Newtonsoft.Json;
using Quizzly.Http;
using Quizzly.Models;
using System.Collections.ObjectModel;
using System.Net.Http;
namespace Quizzly.ViewModels;

class MainWindowViewModel : ViewModelBase {
    public ObservableCollection<QuestionPackViewModel> Packs { get; } = new();
    private HttpClient http = new();
    public PlayerViewModel? PlayerViewModel { get; }
    public ConfigurationViewModel? ConfigurationViewModel { get; }
    private QuestionPackViewModel _activePack;

    public QuestionPackViewModel ActivePack {
        get => _activePack;
        set {
            _activePack = value;
            RaisePropertyChanged();
            PlayerViewModel?.RaisePropertyChanged(nameof(PlayerViewModel.ActivePack));
        }
    }

    public MainWindowViewModel() {
        PlayerViewModel = new PlayerViewModel(this);
        ConfigurationViewModel = new ConfigurationViewModel(this);
        var pack = new QuestionPack("RANDOM");
        ActivePack = new QuestionPackViewModel(pack);
        Packs.Add(ActivePack);
        _ = GetQuestionsFromDatabase();
    }

    public async Task GetQuestionsFromDatabase() {
        string url = "https://opentdb.com/api.php?amount=10";
        string json = await http.GetStringAsync(url);
        var result = JsonConvert.DeserializeObject<ReadJson>(json);
        string category = result?.results.Count > 0
            ? HtmlDecode(result.results[0].category)
            : "Unknown";
        ActivePack.Category = category;
        ActivePack.Questions.Clear();
        foreach(var q in result?.results) {
            ActivePack.Questions.Add(new Question(
                query: HtmlDecode(q.question),
                correctAnswer: HtmlDecode(q.correct_answer),
                incorrectAnswer1: HtmlDecode(q.incorrect_answers[0]),
                incorrectAnswer2: HtmlDecode(q.incorrect_answers[1]),
                incorrectAnswer3: HtmlDecode(q.incorrect_answers[2])
            ));
        }
    }

    private static string HtmlDecode(string text) =>
    System.Net.WebUtility.HtmlDecode(text);

}