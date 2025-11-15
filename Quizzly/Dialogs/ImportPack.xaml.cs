using Quizzly.ViewModels;
using System.Windows;
using System.Windows.Controls; // Button
using System.Net.Http;

namespace Quizzly.Dialogs;

public partial class ImportPack : Window {
    private readonly MainWindowViewModel _mainVm;
    private bool _isImporting;

    public ImportPack(MainWindowViewModel mainVm) {
        InitializeComponent();
        _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        DataContext = _mainVm;
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e) {
        if(_isImporting) return;
        _isImporting = true;
        var btn = sender as Button;
        var prevEnabled = btn?.IsEnabled ?? true;
        try {
            if(btn != null) btn.IsEnabled = false;
            IsEnabled = false;
            var category = _mainVm.CurrentCategory;
            if(category == null) {
                MessageBox.Show("Please select a category before importing.", "Missing category", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int timeLimit = (int)(timerValue?.Value ?? 30);
            int amount = (int)(questionValue?.Value ?? 10);
            var name = string.IsNullOrWhiteSpace(category.name) ? "Imported Pack" : category.name;

            _mainVm.AddAndActivatePack(
                name: name,
                difficulty: _mainVm.CurrentDifficulty,
                timeLimitInSeconds: timeLimit,
                category: category,
                overwriteActive: false
            );
            _mainVm.SetSelectedAmount(amount);
            await _mainVm.GetQuestionsFromDatabase();
            MessageBox.Show($"Successfully imported \"{name}\".\nPrevious pack was saved and is available in Saved Question Packs.");
            DialogResult = true;
        }
        finally {
            IsEnabled = true;
            if(btn != null) btn.IsEnabled = prevEnabled;
            _isImporting = false;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
