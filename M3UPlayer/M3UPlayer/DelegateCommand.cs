using System;
using System.Windows;
using System.Windows.Input;

namespace M3UPlayer
{
    public class DelegateCommand : ICommand
    {
        private Action MyAction;

        /// <summary>
        /// <Button Command="{Binding ...からのインターフェイス
        /// Livet.ViewModelCommand と置き換え
        //
        /// </summary>
        public DelegateCommand(Action _action)
        {
            this.MyAction = _action;
        }

        event EventHandler ICommand.CanExecuteChanged {
            add {
                //          throw new NotImplementedException();
            }

            remove {
                //          throw new NotImplementedException();
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
            //                throw new NotImplementedException();
        }

        void ICommand.Execute(object parameter)
        {
            string TAG = "Execute";
            string dbMsg = "";
            try
            {
                dbMsg = ",MyAction.Method.Name=" + MyAction.Method.Name;
                MyAction();
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }
        ///////////////////////
        public MessageBoxResult MessageShowWPF(String titolStr, String msgStr,
                                                                        MessageBoxButton buttns,
                                                                        MessageBoxImage icon
                                                                        )
        {
            CS_Util Util = new CS_Util();
            return Util.MessageShowWPF(msgStr, titolStr, buttns, icon);
        }

        public static void MyLog(string TAG, string dbMsg)
        {
            dbMsg = "[DelegateCommand]" + dbMsg;
            //dbMsg = "[" + MethodBase.GetCurrentMethod().Name + "]" + dbMsg;
            CS_Util Util = new CS_Util();
            Util.MyLog(TAG, dbMsg);
        }

        public static void MyErrorLog(string TAG, string dbMsg, Exception err)
        {
            dbMsg = "[DelegateCommand]" + dbMsg;
            CS_Util Util = new CS_Util();
            Util.MyErrorLog(TAG, dbMsg, err);
        }
        //////////////////////////////////////

    }

}
