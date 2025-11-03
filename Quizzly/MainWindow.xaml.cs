using Quizzly.ViewModels;
using System.Windows;

namespace Quizzly;
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}