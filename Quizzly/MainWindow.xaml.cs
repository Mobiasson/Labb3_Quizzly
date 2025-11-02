using Quizzly.ViewModels;
using System.Windows;

namespace Quizzly; 
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void ConfigurationsView_Loaded(object sender, RoutedEventArgs e) {

    }


}