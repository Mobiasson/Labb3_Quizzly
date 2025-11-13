using Quizzly.Command;
using System.Windows;

namespace Quizzly.ViewModels {
    public class MenuViewModel : ViewModelBase {
        private readonly MainWindowViewModel _mainVm;
        private WindowStyle _prevWindowStyle;
        private ResizeMode _prevResizeMode;
        private WindowState _prevWindowState;
        private bool _prevTopmost;
        private SizeToContent _prevSizeToContent;
        private bool _isFullscreen;

        public MenuViewModel(MainWindowViewModel mainVm) {
            _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
            FullscreenCommand = new DelegateCommand(_ => ToggleFullscreen());
        }

        public DelegateCommand RemoveQuestionCommand => _mainVm.RemoveQuestionCommand;
        public DelegateCommand AddQuestionCommand => _mainVm.AddQuestionCommand;
        public DelegateCommand ChangePackNameCommand => _mainVm.ChangePackNameCommand;
        public DelegateCommand FullscreenCommand { get; }

        private void ToggleFullscreen() {
            var window = Application.Current?.MainWindow;
            if(window == null) return;
            if(!_isFullscreen) {
                _prevWindowStyle = window.WindowStyle;
                _prevResizeMode = window.ResizeMode;
                _prevWindowState = window.WindowState;
                _prevTopmost = window.Topmost;
                _prevSizeToContent = window.SizeToContent;
                window.WindowState = WindowState.Normal;
                window.SizeToContent = SizeToContent.Manual;
                window.WindowStyle = WindowStyle.None;
                window.ResizeMode = ResizeMode.NoResize;
                window.WindowState = WindowState.Maximized;
                window.Topmost = true;
                _isFullscreen = true;
            } else {
                window.WindowState = WindowState.Normal;
                window.WindowStyle = _prevWindowStyle;
                window.ResizeMode = _prevResizeMode;
                window.SizeToContent = _prevSizeToContent;
                window.Topmost = _prevTopmost;
                window.WindowState = _prevWindowState;
                _isFullscreen = false;
            }
        }
    }
}