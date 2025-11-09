using Newtonsoft.Json;

namespace Quizzly.Models {
    public class Question {
        [JsonConstructor]
        public Question(string query, string correctAnswer, string incorrectAnswer1, string incorrectAnswer2, string incorrectAnswer3) {
            Query = query;
            CorrectAnswer = correctAnswer;
            IncorrectAnswers = new[] { incorrectAnswer1, incorrectAnswer2, incorrectAnswer3 };
        }

        public string Query { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";
        public string[] IncorrectAnswers { get; set; } = new string[3];
    }
}