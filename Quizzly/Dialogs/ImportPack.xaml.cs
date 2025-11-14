using Quizzly.ViewModels;
using System.Windows;

namespace Quizzly.Dialogs;

public partial class ImportPack : Window {
    private readonly MainWindowViewModel _mainVm;

    public ImportPack(MainWindowViewModel mainVm) {
        InitializeComponent();
        _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        DataContext = _mainVm;
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e) {
        try {
            var category = _mainVm.CurrentCategory;
            if(category == null) {
                MessageBox.Show("Please select a category before importing.", "Missing category", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var name = string.IsNullOrWhiteSpace(category.name) ? "Imported Pack" : category.name;
            _mainVm.AddAndActivatePack(
                name: name,
                difficulty: _mainVm.CurrentDifficulty,
                timeLimitInSeconds: (int)timerValue.Value,
                category: category,
                overwriteActive: false
            );
            _mainVm.SetSelectedAmount((int)questionValue.Value);
            await _mainVm.GetQuestionsFromDatabase();
            MessageBox.Show($"Successfully imported \"{name}\".\nPrevious pack was saved and is available in Saved Question Packs.");
            DialogResult = true;
        }
        catch(Exception ex) {
            MessageBox.Show("Something went wrong, you are a bit too fast. Try again. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
