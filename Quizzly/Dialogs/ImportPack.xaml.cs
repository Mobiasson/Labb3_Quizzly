using Quizzly.Models;
using Quizzly.ViewModels;
using System;
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
            if(_mainVm.ActivePack == null) {
                var defaultPack = new Models.QuestionPack("Defualt pack");
                var vm = _mainVm.CreatePackVm(defaultPack);
                _mainVm.Packs.Add(vm);
                _mainVm.ActivePack = vm;
            }
            _mainVm.ActivePack!.Difficulty = _mainVm.CurrentDifficulty;
            await _mainVm.GetQuestionsFromDatabase();
            MessageBox.Show($"Succesfully imported category");
            DialogResult = true;
        }
        catch(Exception ex) {
            MessageBox.Show("Something went wrong, you are a bit too fast. Try again" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
