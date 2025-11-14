using Quizzly.Models;
using Quizzly.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Quizzly.Views;
public partial class ConfigurationView : UserControl {
    public ConfigurationView() {
        InitializeComponent();
    }

    internal void ClearTextBoxes() {
        question_textbox.Text = string.Empty;
        correct_textbox.Text = string.Empty;
        incorrect1_textbox.Text = string.Empty;
        incorrect2_textbox.Text = string.Empty;
        incorrect3_textbox.Text = string.Empty;
    }
}

