using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using M3UPlayer.Models;
using M3UPlayer.ViewModels;

namespace M3UPlayer.Views {
    /// <summary>
    /// 【WPF】非同期処理中に進捗ダイアログを表示する
    /// https://madai21.hatenablog.com/entry/vs-2019-csharp-wpf-progress-dialog
    /// </summary>
    public partial class ProgressDialog : Window {

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int GWL_STYLE = -16;
        const int WS_SYSMENU = 0x80000;

        private BackgroundWorker worker = new BackgroundWorker();

        private Action action;

        private CancellationTokenSource cancelToken;

        private bool isCanceled = false;
        public bool IsCanceled {
            get {
                return isCanceled;
            }
        }


        public ProgressDialog(object context, Action action, CancellationTokenSource cancelToken) {
            string TAG = "ProgressDialog";
            string dbMsg = "";
            try {
                InitializeComponent();
                DataContext = context;
                this.action = action;
                this.cancelToken = cancelToken;
                worker.DoWork += DoWork;
                worker.RunWorkerCompleted += RunWorkerCompleted;
                worker.RunWorkerAsync();
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }

        }


		protected override void OnSourceInitialized(EventArgs e) {
            string TAG = "OnSourceInitialized";
            string dbMsg = "";
            try {
                base.OnSourceInitialized(e);
                IntPtr handle = new WindowInteropHelper(this).Handle;
                int style = GetWindowLong(handle, GWL_STYLE);
                style = style & ~WS_SYSMENU;
                SetWindowLong(handle, GWL_STYLE, style);
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e) {
            string TAG = "DoWork";
            string dbMsg = "";
            try {
                if (action == null) {
                    return;
                }
                Task task = Task.Factory.StartNew((obj) => {
                    action.Invoke();
                }, cancelToken);

                task.Wait();
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            string TAG = "RunWorkerCompleted";
            string dbMsg = "";
            try {
                Close();
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            string TAG = "Cancel_Click";
            string dbMsg = "";
            try {
                cancelToken.Cancel();
                isCanceled = true;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        ///////////////////////////////////////////////////////////////////
        public static void MyLog(string TAG, string dbMsg) {
            dbMsg = "[ProgressDialog]" + dbMsg;
            //dbMsg = "[" + MethodBase.GetCurrentMethod().Name + "]" + dbMsg;
            CS_Util Util = new CS_Util();
            Util.MyLog(TAG, dbMsg);
        }

        public static void MyErrorLog(string TAG, string dbMsg, Exception err) {
            dbMsg = "[ProgressDialog]" + dbMsg;
            CS_Util Util = new CS_Util();
            Util.MyErrorLog(TAG, dbMsg, err);
        }

        public MessageBoxResult MessageShowWPF(String titolStr, String msgStr,
                                                                        MessageBoxButton buttns,
                                                                        MessageBoxImage icon
                                                                        ) {
            CS_Util Util = new CS_Util();
            return Util.MessageShowWPF(msgStr, titolStr, buttns, icon);
        }



    }
}
