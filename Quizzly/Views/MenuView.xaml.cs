using Quizzly.Dialogs;
using System.Windows;
using System.Windows.Controls;


namespace Quizzly.Views;
public partial class MenuView : UserControl {
    public MenuView() {
        InitializeComponent();
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e) {
        var importDialog = new ImportPack();
        importDialog.ShowDialog();
    }

    private void ExitFromMenu(object sender, RoutedEventArgs e) {
        Application.Current.Shutdown();
    }
}
