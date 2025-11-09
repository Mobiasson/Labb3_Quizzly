using Quizzly.Models;
using Quizzly.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Quizzly.Views;
public partial class ConfigurationView : UserControl {
    public ConfigurationView() {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
        ClearTextBoxes();
        AddQuestion_button.Visibility = Visibility.Visible;
    }

    private void AddQuestion_button_Click(object sender, RoutedEventArgs e) {
        var vm = DataContext as ConfigurationViewModel;
        if(vm?.ActivePack == null) return;
        var newQuestion = new Question(
            query: question_textbox.Text,
            correctAnswer: correct_textbox.Text,
            incorrectAnswer1: incorrect1_textbox.Text,
            incorrectAnswer2: incorrect2_textbox.Text,
            incorrectAnswer3: incorrect3_textbox.Text
        );
        if(question_textbox.Text == string.Empty || correct_textbox.Text == string.Empty || incorrect1_textbox.Text == string.Empty || incorrect2_textbox.Text == string.Empty || incorrect3_textbox.Text == string.Empty) {
            MessageBox.Show($"One field does not contain a value. Fill all the textboxes to add a question."); return;
        }
        vm.ActivePack.Questions.Add(newQuestion);
        vm.ActivePack.SelectedQuestion = newQuestion;
        if(vm.ActivePack.Questions.Contains(newQuestion)) {
            MessageBox.Show("Question was successfully added to ActivePack");
        }
        ClearTextBoxes();
        AddQuestion_button.Visibility = Visibility.Hidden;
    }

    private void ClearTextBoxes() {
        question_textbox.Clear();
        correct_textbox.Clear();
        incorrect1_textbox.Clear();
        incorrect2_textbox.Clear();
        incorrect3_textbox.Clear();
    }

    private void Play_button_Click(object sender, RoutedEventArgs e) {
        var mainVm = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
        mainVm.SwitchToPlayer();
    }

}

