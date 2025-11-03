using Newtonsoft.Json;
using Quizzly.Command;
using Quizzly.Http;
using Quizzly.Models;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace Quizzly.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    public ObservableCollection<QuestionPackViewModel> Packs { get; } = new();
    private HttpClient http = new();
    public PlayerViewModel? PlayerViewModel { get; }
    public ConfigurationViewModel? ConfigurationViewModel { get; }
    public DelegateCommand RemoveQuestionCommand { get; }

    private QuestionPackViewModel? _activePack;
    public QuestionPackViewModel? ActivePack {
        get => _activePack;
        set {
            _activePack = value;
            RaisePropertyChanged();
            RemoveQuestionCommand.RaiseCanExecuteChanged();
        }
    }

    public MainWindowViewModel() {
        PlayerViewModel = new PlayerViewModel(this);
        ConfigurationViewModel = new ConfigurationViewModel(this);
        RemoveQuestionCommand = new DelegateCommand(RemoveQuestionExecute, CanRemoveQuestionExecute);
        var pack = new QuestionPack("");
        ActivePack = new QuestionPackViewModel(pack);
        Packs.Add(ActivePack);
        _ = GetQuestionsFromDatabase();
    }

    private void RemoveQuestionExecute(object? param) {
        if(ActivePack?.SelectedQuestion != null) {
            ActivePack.Questions.Remove(ActivePack.SelectedQuestion);
            ActivePack.SelectedQuestion = null;
            RemoveQuestionCommand.RaiseCanExecuteChanged();
        }
    }

    private bool CanRemoveQuestionExecute(object? param) {
        return ActivePack?.SelectedQuestion != null;
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
        foreach(var q in result?.results ?? Enumerable.Empty<GetQuestionsFromAPI>()) {
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