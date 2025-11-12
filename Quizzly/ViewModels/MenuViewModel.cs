using Quizzly.Command;

namespace Quizzly.ViewModels {
    public class MenuViewModel : ViewModelBase {
        private readonly MainWindowViewModel _mainVm;

        public MenuViewModel(MainWindowViewModel mainVm) {
            _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        }
        public DelegateCommand RemoveQuestionCommand => _mainVm.RemoveQuestionCommand;
        public DelegateCommand AddQuestionCommand => _mainVm.AddQuestionCommand;
        public DelegateCommand ChangePackNameCommand => _mainVm.ChangePackNameCommand;
    }
}