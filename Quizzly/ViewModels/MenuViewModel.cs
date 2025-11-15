using Quizzly.Command;
using Quizzly.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Quizzly.ViewModels {
    public class MenuViewModel : ViewModelBase {
        public DelegateCommand AddQuestionCommand { get; }
        public DelegateCommand RemoveQuestionCommand { get; }
        public DelegateCommand PlayCommand { get; }
        public DelegateCommand ChangePackNameCommand { get; }
        public DelegateCommand NewPackCommand { get; }
        public DelegateCommand ImportPackCommand { get; }
        public DelegateCommand ExitCommand { get; }
        public DelegateCommand PackOptionsCommand { get; }
        public DelegateCommand SetPackNameCommand { get; }
        public DelegateCommand FullscreenCommand { get; }
        public DelegateCommand ActivatePackCommand { get; }
        public DelegateCommand DeletePackCommand { get; }
        public DelegateCommand StopPlayingCommand { get; }

        public ObservableCollection<QuestionPackViewModel> Packs => _mainVm.Packs;
        private readonly MainWindowViewModel _mainVm;
        private WindowStyle _prevWindowStyle;
        private ResizeMode _prevResizeMode;
        private WindowState _prevWindowState;
        private bool _prevTopmost;
        private SizeToContent _prevSizeToContent;
        private bool _isFullscreen;

        public MenuViewModel(MainWindowViewModel mainVm) {
            _mainVm = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
            AddQuestionCommand = _mainVm.AddQuestionCommand;
            RemoveQuestionCommand = _mainVm.RemoveQuestionCommand;
            PlayCommand = _mainVm.PlayCommand;
            ChangePackNameCommand = _mainVm.ChangePackNameCommand;
            NewPackCommand = new DelegateCommand(_ => CreateNewPack());
            ImportPackCommand = new DelegateCommand(_ => ImportPack());
            ExitCommand = new DelegateCommand(_ => ExitApp());
            PackOptionsCommand = new DelegateCommand(_ => ShowPackOptions());
            SetPackNameCommand = new DelegateCommand(_ => ShowSetPackName());
            FullscreenCommand = new DelegateCommand(_ => ToggleFullscreen());
            StopPlayingCommand = _mainVm.StopPlayingCommand;

            ActivatePackCommand = new DelegateCommand(
                p => {
                    if(p is QuestionPackViewModel vm) {
                        _mainVm.ActivePack = vm;
                        _mainVm.SavePacks();
                        DeletePackCommand.RaiseCanExecuteChanged();
                    }
                },
                p => p is QuestionPackViewModel
            );

            DeletePackCommand = new DelegateCommand(
                _ => DeleteActivePack(),
                _ => _mainVm.ActivePack != null
            );

            _mainVm.PropertyChanged += MainVm_PropertyChanged;
        }

        private void MainVm_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(MainWindowViewModel.ActivePack)) {
                DeletePackCommand.RaiseCanExecuteChanged();
            }
            // keep MenuView in sync if you bind IsEnabled elsewhere
            if(e.PropertyName == nameof(MainWindowViewModel.IsPlaying)) {
                StopPlayingCommand.RaiseCanExecuteChanged();
            }
        }
        private void DeleteActivePack() {
            var pack = _mainVm.ActivePack;
            if(pack == null) return;
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{pack.Name}'?",
                "Delete Pack",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if(result != MessageBoxResult.Yes) return;
            _mainVm.Packs.Remove(pack);
            _mainVm.ActivePack = _mainVm.Packs.FirstOrDefault();
            _mainVm.SavePacks();
        }

        private void CreateNewPack() {
            var owner = Application.Current?.MainWindow;
            var dlg = new CreateNewPackDialog(_mainVm) { Owner = owner };
            dlg.ShowDialog();
        }

        private void ImportPack() {
            var owner = Application.Current?.MainWindow;
            var dlg = new ImportPack(_mainVm) { Owner = owner };
            dlg.ShowDialog();
        }

        private void ShowPackOptions() {
            var owner = Application.Current?.MainWindow;
            var dlg = new PackOptionsDialog(_mainVm) { Owner = owner };
            dlg.ShowDialog();
        }

        private void ShowSetPackName() {
            var owner = Application.Current?.MainWindow;
            var dlg = new SetPackName(_mainVm) { Owner = owner };
            dlg.ShowDialog();
        }

        private void ExitApp() {
            Application.Current?.Shutdown();
        }

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