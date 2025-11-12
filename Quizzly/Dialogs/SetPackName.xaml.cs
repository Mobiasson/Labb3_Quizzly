using Quizzly.ViewModels;
using System.Windows;

namespace Quizzly.Dialogs;

public partial class SetPackName : Window {
    private readonly MainWindowViewModel _mainVm;

    public SetPackName(MainWindowViewModel mainVm) {
        InitializeComponent();
        _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        DataContext = _mainVm;
        packNameTextBox.Text = _mainVm.ActivePack?.Name ?? string.Empty;
        packNameTextBox.SelectAll();
        packNameTextBox.Focus();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }

    private void Confirm_Click(object sender, RoutedEventArgs e) {
        var newName = packNameTextBox.Text?.Trim();
        if(string.IsNullOrWhiteSpace(newName)) {
            MessageBox.Show("Please enter a pack name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            packNameTextBox.Focus();
            return;
        }
        if(_mainVm.ChangePackNameCommand != null && _mainVm.ChangePackNameCommand.CanExecute(newName)) {
            _mainVm.ChangePackNameCommand.Execute(newName);
        } else {
            _mainVm.GetType().GetMethod("ChangePackName", new[] { typeof(object) })?
                .Invoke(_mainVm, new object[] { newName });
        }
        DialogResult = true;
        Close();
    }
}