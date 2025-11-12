using Quizzly.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Quizzly.Views;

public partial class EndView : UserControl {
    public EndView() {
        InitializeComponent();
    }

    private void back_button_Click(object sender, RoutedEventArgs e) {
        var mainVm = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
        mainVm.SwitchToConfiguration();
    }

    private void playagain_button_Click(object sender, RoutedEventArgs e) {
        var mainVm = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
        _ = mainVm.RestartAsync();
    }
}
