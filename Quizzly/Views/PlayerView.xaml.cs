using Quizzly.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Quizzly.Views;

public partial class PlayerView : UserControl {
    public PlayerView() {
        InitializeComponent();
    }

    private void Back_Click(object sender, RoutedEventArgs e) {
        var mainVm = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
        mainVm.SwitchToConfiguration();
    }
}
