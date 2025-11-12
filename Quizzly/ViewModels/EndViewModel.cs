namespace Quizzly.ViewModels;
public class EndViewModel : ViewModelBase {
    private readonly MainWindowViewModel _mainVM;

    public EndViewModel(MainWindowViewModel mainVM, int correctAnswers, int totalQuestions) {
        _mainVM = mainVM;
        CorrectAnswers = correctAnswers;
        TotalQuestions = totalQuestions;
    }
    public int CorrectAnswers { get; }
    public int TotalQuestions { get; }

    public string EndText => $"Game is over. You did so well wow......";

    public string ResultText =>
    (TotalQuestions - CorrectAnswers) switch {
        0 => $"Excellent! {CorrectAnswers}/{TotalQuestions} Maybe you're not useless after all",
        1 or 2 => $"Not so bad, not so good either.. {CorrectAnswers}/{TotalQuestions}",
        3 or 4 or 5 => $"Did you skip preschool? {CorrectAnswers}/{TotalQuestions}",
        _ => $"HAHAHAH {CorrectAnswers}/{TotalQuestions}"
    };
}

