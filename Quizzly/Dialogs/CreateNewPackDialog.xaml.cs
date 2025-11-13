using Quizzly.ViewModels;
using Quizzly.Models;
using System;
using System.Windows;

namespace Quizzly.Dialogs;
public partial class CreateNewPackDialog : Window {
    private readonly MainWindowViewModel _mainVm;
    public CreateNewPackDialog(MainWindowViewModel mainVm) {
        InitializeComponent();
        _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        DataContext = _mainVm;
    }

    private void cancel_button_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }

    private void confirm_button_Click(object sender, RoutedEventArgs e) {
        var name = packNameTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name)) {
            MessageBox.Show("Please enter a pack name.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Warning);
            packNameTextBox.Focus();
            return;
        }

        var difficulty = _mainVm.CurrentDifficulty;
        int timeLimit = (int)timerValue.Value;

        // Overwrite the active pack so the saved JSON does not keep the imported questions.
        _mainVm.AddAndActivatePack(name, difficulty, timeLimit, _mainVm.CurrentCategory, overwriteActive: true);

        DialogResult = true;
        Close();
    }
}
