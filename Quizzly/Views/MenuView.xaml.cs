using System.Windows.Controls;
using Quizzly.Dialogs;


namespace Quizzly.Views; 
public partial class MenuView : UserControl {
    public MenuView() {
        InitializeComponent();
    }

    private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e) {
        ImportPack importDialog = new ImportPack();
 
    }
}
