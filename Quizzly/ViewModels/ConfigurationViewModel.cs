using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizzly.ViewModels {
    class ConfigurationViewModel : ViewModelBase {
        private readonly MainWindowViewModel? mainWindowViewModel;

        public ConfigurationViewModel(MainWindowViewModel? mainWindowViewModel) {
            this.mainWindowViewModel = mainWindowViewModel;
        }
    }
}