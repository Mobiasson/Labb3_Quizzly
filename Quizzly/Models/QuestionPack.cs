namespace Quizzly.Models;

public enum Difficulty { Easy, Medium, Hard }
public class QuestionPack {
    public QuestionPack(string name, Difficulty difficulty = Difficulty.Medium, int timeLimitInSeconds = 30) {
        Name = name;
        Difficulty = difficulty;
        TimeLimitInSeconds = timeLimitInSeconds;
        Questions = new List<Question>();
    }

    public string Name { get; set; }
    public string Category { get; set; }
    public Difficulty Difficulty { get; set; }
    public int TimeLimitInSeconds { get; set; }
    public Question SelectedQuestion { get; set; }
    public List<Question> Questions { get; set; }
}