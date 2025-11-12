using Quizzly.ViewModels;
using System.Windows;

namespace Quizzly.Dialogs;
public partial class PackOptionsDialog : Window {
    private readonly MainWindowViewModel _mainVm;
    public PackOptionsDialog(MainWindowViewModel mainVm) {
        InitializeComponent();
        _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        DataContext = _mainVm;
    }

    private void cancel_button_Click(object sender, RoutedEventArgs e) {
        Close();
    }

    private void confirm_button_Click(object sender, RoutedEventArgs e) {
        try {
            _mainVm.ActivePack!.TimeLimitInSeconds = (int)timerValue.Value;
            _mainVm.ActivePack!.Difficulty = _mainVm.CurrentDifficulty;
            DialogResult = true;
            MessageBox.Show("Options changed");
        }
        catch(Exception ex) {
            MessageBox.Show("Something went wrong, you are a bit too fast. Try again" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
