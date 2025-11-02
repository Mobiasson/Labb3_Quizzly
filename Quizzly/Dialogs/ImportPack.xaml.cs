using Quizzly.ViewModels;
using System.Windows;

namespace Quizzly.Dialogs;

public partial class ImportPack : Window {
    public ImportPack() {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void ImportQuestionPack_Click(object sender, RoutedEventArgs e) {
        var dialog = new Quizzly.Dialogs.ImportPack();
        if(dialog.ShowDialog() == true) {
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e) {

    }
}
