using Newtonsoft.Json;

namespace Quizzly.Models;
public enum Difficulty { Easy, Medium, Hard }
public class QuestionPack {
    [JsonConstructor]
    public QuestionPack(string name, Difficulty difficulty = Difficulty.Medium, int timeLimitInSeconds = 30, int categoryId = 5) {
        Name = name;
        Difficulty = difficulty;
        TimeLimitInSeconds = timeLimitInSeconds;
        CategoryId = categoryId;
        Questions = new List<Question>();
    }

    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public int CategoryId { get; set; } = 5;
    public Difficulty Difficulty { get; set; } = Difficulty.Medium;
    public int TimeLimitInSeconds { get; set; } = 30;
    public Question? SelectedQuestion { get; set; }
    public List<Question> Questions { get; set; } = new();
}