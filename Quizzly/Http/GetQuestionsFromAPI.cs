namespace Quizzly.Http;
public class GetQuestionsFromAPI {
    public string category { get; set; }
    public string question { get; set; }
    public string correct_answer { get; set; }
    public List<string> incorrect_answers { get; set; }
}
