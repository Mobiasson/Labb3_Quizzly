using Quizzly.ViewModels;

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

    public string ResultText => $"{CorrectAnswers} out of {TotalQuestions} questions were correct";
}
