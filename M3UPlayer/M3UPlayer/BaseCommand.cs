using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace M3UPlayer {
    class BaseCommand : ICommand {
        private Action execAction = null;

        public event EventHandler CanExecuteChanged;

        public BaseCommand(Action action) {
            execAction = action;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (execAction == null) {
                return;
            }

            execAction();
        }
    }
}
