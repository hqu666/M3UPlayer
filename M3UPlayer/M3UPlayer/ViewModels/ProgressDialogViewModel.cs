﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using M3UPlayer.Models;
using M3UPlayer.ViewModels;
using M3UPlayer.Views;

namespace M3UPlayer.ViewModels {
    /// <summary>
    /// 
    /// </summary>
    class ProgressDialogViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string prgTitle = "Progress";
        /// <summary>
        /// プログレスダイアログのタイトル
        /// </summary>
        public string PrgTitle {
            get {
                return prgTitle;
            }
            set {
                prgTitle = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// プログレスダイアログのメッセージ
        /// </summary>
        private string prgStatus = "処理実行中";
        public string PrgStatus {
            get {
                return prgStatus;
            }
            set {
                prgStatus = value;
                NotifyPropertyChanged();
            }
        }

        private int prgMin = 1;
        /// <summary>
        /// プログレスダイアログの最小値;開始
        /// </summary>
        public int PrgMin {
            get {
                return prgMin;
            }
            set {
                prgMin = value;
                NotifyPropertyChanged();
            }
        }

        private int prgMax = 100;
        /// <summary>
        /// プログレスダイアログの最大値:終端
        /// </summary>
        public int PrgMax {
            get {
                return prgMax;
            }
            set {
                prgMax = value;
                NotifyPropertyChanged();
            }
        }

        private int prgVal = 0;
        /// <summary>
        /// プログレスダイアログの最大値:終端
        /// </summary>
        public int PrgVal {
            get {
                return prgVal;
            }
            set {
                prgVal = value;
                int range = (prgMax - prgMin) + 1;
                int percent = (int)(((double)prgVal / range) * 100);
                PrgPer = percent.ToString() + "%";
                NotifyPropertyChanged();
            }
        }

        private string prgPer = "0%";
        /// <summary>
        /// プログレスダイアログの%表示文字
        /// </summary>
        public string PrgPer {
            get {
                return prgPer;
            }
            set {
                prgPer = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand ExecProgress => new DelegateCommand(ProgressExec);
        /// <summary>
        /// プログレスダイアログの表示実行
        /// </summary>
        public void ProgressExec() {
         //   get {
            //    return
                //new BaseCommand(new Action(() => {
                    string TAG = "ProgressExec";
                    string dbMsg = "";
                    try {
                        CancellationTokenSource cancelToken = new CancellationTokenSource();
             //           PrgTitle = "処理実行中";
                        dbMsg += "、PrgTitle=" + PrgTitle;
                        PrgVal = 0;
                        PrgMin = 1;
         //               PrgMax = 100;
                        ProgressDialog pd = new ProgressDialog(this, () => {
                            dbMsg += "、" + PrgVal + "/" + PrgMax;
                            for (PrgVal = 0; PrgVal < PrgMax; PrgVal++) {
                                if (cancelToken != null && cancelToken.IsCancellationRequested) {
                                    return;
                                }
                                dbMsg += "、PrgStatus=" + PrgStatus ;
								//   PrgStatus = "処理" + PrgVal.ToString("000") + "を実行しています";
								Thread.Sleep(1);
							}
                        }, cancelToken);

                        pd.ShowDialog();
                        if (pd.IsCanceled) {
                            MessageBox.Show("キャンセルしました", "Info", MessageBoxButton.OK);
                        } else {
                            //MessageBox.Show("完了しました", "Info", MessageBoxButton.OK);
                        }
                        MyLog(TAG, dbMsg);
                    } catch (Exception er) {
                        MyErrorLog(TAG, dbMsg, er);
                    }
                //}));
          //  }
        }

        //デバッグツール///////////////////////////////////////////////////////////その他//

        public static void MyLog(string TAG, string dbMsg) {
            dbMsg = "[ProgressDialogViewModel]" + dbMsg;
            //dbMsg = "[" + MethodBase.GetCurrentMethod().Name + "]" + dbMsg;
            CS_Util Util = new CS_Util();
            Util.MyLog(TAG, dbMsg);
        }

        public static void MyErrorLog(string TAG, string dbMsg, Exception err) {
            dbMsg = "[ProgressDialogViewModel]" + dbMsg;
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