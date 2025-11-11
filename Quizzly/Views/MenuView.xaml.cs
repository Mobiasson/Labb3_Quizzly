using Quizzly.Dialogs;
using Quizzly.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;


namespace Quizzly.Views;
public partial class MenuView : UserControl {
    public MenuView() {
        InitializeComponent();
    }

    private void ExitFromMenu(object sender, RoutedEventArgs e) {
        Application.Current.Shutdown();
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e) {
        if(Application.Current?.MainWindow?.DataContext is MainWindowViewModel mainVm) {
            var importPack = new ImportPack(mainVm) {
                Owner = Window.GetWindow(this)
            };
            importPack.ShowDialog();
        } else {
            var importPack = new ImportPack(new MainWindowViewModel());
            importPack.ShowDialog();
        }
    }
}
