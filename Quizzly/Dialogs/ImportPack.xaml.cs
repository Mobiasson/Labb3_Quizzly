using Quizzly.ViewModels;
using System.Windows;

namespace Quizzly.Dialogs;

public partial class ImportPack : Window {
    public ImportPack() {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void ImportQuestionPack_Click(object sender, RoutedEventArgs e) {
        var dialog = new ImportPack();
        if(dialog.ShowDialog() == true) {
        }
    }

}
