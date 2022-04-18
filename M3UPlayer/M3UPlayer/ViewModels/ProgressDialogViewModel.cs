using System;
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
    public class ProgressDialogViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ProgressDialogViewModel() {
            PrgTitle = "";
            PrgStatus = "";
            prgMax = -1;
            PrgMin =-1;
            PrgVal = -1;
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
                if (value == prgTitle) return;
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
                if (value == prgStatus) return;
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
                if (value == prgMin) return;
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
                if (value == prgMax) return;
                prgMax = value;
                NotifyPropertyChanged("PrgMax");
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
                string TAG = "PrgVal(set)";
                string dbMsg = "";
                try {
                    dbMsg += value + "/" + PrgMax;
                    if (value == prgVal) return;
                    prgVal = value;
                    //	if (0< value && 0< PrgMax) {
                    int range = (prgMax - prgMin) + 1;
                    int percent = (int)(((double)prgVal / range) * 100);
                    //   int percent = (int)(((double)prgVal / prgMax) * 100);
                    PrgPer = percent.ToString() + "%";
                    dbMsg +=  ":" + PrgPer + PrgStatus;
                    //   }
                    NotifyPropertyChanged("PrgVal");
                    MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }

  //      public string PrgPer;

		private string prgPer = "0%";
		/// <summary>
		/// プログレスダイアログの%表示文字
		/// </summary>
		public string PrgPer {
			get {
				return prgPer;
			}
			set {
				if (value == prgPer) return;
				prgPer = value;
				NotifyPropertyChanged();
			}
		}

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="titol"></param>
        /// <param name="maxVar"></param>
        /// <param name="miniVar"></param>
        public void IntProgress( string titol, int maxVar, int miniVar) {
            string TAG = "IntProgress";
            string dbMsg = "";
            try {
                PrgTitle = titol;
                PrgMax = maxVar;
                PrgMin = miniVar;
                dbMsg += PrgTitle + ":"+ PrgMin + "～" + PrgMax;
                PrgStatus = "";
                PrgVal = 0;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 表示値更新
        /// </summary>
        /// <param name="PrgressVal"></param>
        /// <param name="messege"></param>
        public void DoProgress(int PrgressVal, string messege) {
			string TAG = "DoProgress";
			string dbMsg = "";
			try {
				PrgVal = PrgressVal;
				PrgStatus = messege;
				dbMsg += PrgVal + "/" + PrgMax;
				dbMsg += "、PrgStatus=" + PrgStatus;
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}


		//       public ICommand ExecProgress => new DelegateCommand(ProgressExec);
		/// <summary>
		/// プログレスダイアログの表示実行
		/// </summary>
		public ICommand ExecProgress {
            get {
                return new BaseCommand(new Action(() => {
                    string TAG = "ExecProgress";
                    string dbMsg = "";
                    try {
                        CancellationTokenSource cancelToken = new CancellationTokenSource();
                        dbMsg += "、PrgTitle=" + PrgTitle;
                        dbMsg += "、PrgMax=" + PrgMax;
                        ProgressDialog pd = new ProgressDialog(this, () => {
                            while (0 < PrgVal && PrgVal < PrgMax) {
                         //   for (PrgVal = 0; PrgVal < PrgMax; PrgVal++) {
								if (cancelToken != null && cancelToken.IsCancellationRequested) {
                                    return;
                                }
                                Thread.Sleep(10);
                            }
                        }, cancelToken);

                        pd.ShowDialog();
                        if (pd.IsCanceled) {
                            MessageBox.Show("キャンセルしました", "Info", MessageBoxButton.OK);
                        } else {
              //              MessageBox.Show("完了しました", "Info", MessageBoxButton.OK);
                        }
                        MyLog(TAG, dbMsg);
                    } catch (Exception er) {
                        MyErrorLog(TAG, dbMsg, er);
                    }
                }));
            }
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
