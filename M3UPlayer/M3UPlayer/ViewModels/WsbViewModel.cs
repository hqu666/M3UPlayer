using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
//using System.Text.Json;
//using System.Text.Json.Serialization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Shapes;

using Microsoft.Web.WebView2.Core;

//using Google.Apis.Drive.v3.Data;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Drive.v3;
//using Google.Apis.Calendar.v3;
//using Google.Apis.Util.Store;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Serialization;
//using Google.Apis.Services;

using M3UPlayer.Views;
using M3UPlayer.Models;
using System.Threading;
using System.Runtime.CompilerServices;

namespace M3UPlayer.ViewModels {
	public class WsbViewModel : INotifyPropertyChanged
    {
        //ViewMode
        public string titolStr = "【WsbViewModel】";

        public Views.WebWindow MyView { get; set; }

        private Uri _TargetURI;
		/// <summary>
		/// 遷移先URL
		/// </summary>
		public Uri? TargetURI {
			get {
				return _TargetURI;
			}
			set {
				string TAG = "TargetURI.set";
				string dbMsg = "";
				try {
					dbMsg += ">>遷移先URL=  " + value;
					if (value == _TargetURI)
						return;
					_TargetURI = value;
					MyLog(TAG, dbMsg);
				} catch (Exception er) {
					MyErrorLog(TAG, dbMsg, er);
				}
			}
		}
		private string _TargetURLStr;
		/// <summary>
		/// 遷移先URL文字列
		/// </summary>
		public string TargetURLStr {
			get {
				return _TargetURLStr;
			}
			set {
				string TAG = "TargetURLStr.set";
				string dbMsg = "";
				try {
					dbMsg += ">>遷移先URL=  " + value;
					if (value == _TargetURLStr)
						return;
						_TargetURLStr = value;
                    if(value !=null)
                    {
                        TargetURI = new Uri(value);
                        NotifyPropertyChanged("TargetURI");
                    }
                    MyLog(TAG, dbMsg);
				} catch (Exception er) {
					MyErrorLog(TAG, dbMsg, er);
				}
			}
		}
	
		/// <summary>
		/// 操作パネルの表示
		/// </summary>
		public string TopPanelVisibility { set; get; }
		/// <summary>
		/// 現在遷移中
		/// </summary>
		bool _isNavigating = false;
		/// <summary>
		/// 遷移上限から外れた場合の戻し先
		/// </summary>
		public string RedirectUrl = "";
	/// <summary>
	/// 基本的な遷移先
	/// </summary>
		public string BaceUrl { set; get; }

		public WsbViewModel()
		{
			TopPanelVisibility = "Hidden";
            NotifyPropertyChanged("TopPanelVisibility");
            Initialize();
		}

		public void Initialize()
		{
			string TAG = "Initialize";
			string dbMsg = "";
			try {
				RedirectUrl = "";
				dbMsg += "TargetURLStr=" + TargetURLStr;
				//	TargetURLStr = Constant.WebStratUrl;
				NotifyPropertyChanged("TargetURLStr");
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		/// <summary>
		/// URLを指定する
		/// </summary>
		/// <param name="urlStr"></param>
		public void SetMyUrl(string urlStr) {
			string TAG = "SetMyUrl";
			string dbMsg = "";
			try {
				TargetURLStr = urlStr;
				RedirectUrl = "";
				dbMsg += "TargetURLStr=" + TargetURLStr;
				//	TargetURLStr = Constant.WebStratUrl;
				NotifyPropertyChanged("TargetURLStr");
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}




		#region Webエレメントの読み込み終了
		public ICommand WebLoaded => new DelegateCommand(LoadedWebView);
        /// <summary>
        /// エレメントの読み込み終了
        /// </summary>
        public void LoadedWebView()
		{
			string TAG = "LoadedWebView";
			string dbMsg = "";
			try {
				TopPanelVisibility = "Visible";
                NotifyPropertyChanged("TopPanelVisibility");
                MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}
        #endregion

        #region 接続先変更イベント
        public ICommand ChangedSource => new DelegateCommand(SourceChanged);
        public void SourceChanged()
		{
			string TAG = "SourceChanged";
			string dbMsg = "";
			try {
				dbMsg += "RedirectUrl= " + RedirectUrl;
				dbMsg += " >>TargetURI= " + TargetURI;
				TargetURLStr = TargetURI.ToString();
                NotifyPropertyChanged("TargetURLStr");
				//if (CanGoto(TargetURLStr)) {
				//	_isNavigating = true;
				//	RedirectUrl = TargetURI.ToString();
				//	RequeryCommands();
				//} else {
					//if (!RedirectUrl.Equals("")) {
					//	dbMsg += " >Redirect>  " + RedirectUrl;
					//	TargetURLStr = RedirectUrl;
					//} else {
					//	dbMsg += " >Reset>  " + BaceUrl;
					//	TargetURLStr = BaceUrl;
					//}
                    NotifyPropertyChanged("TargetURLStr");
                    if (TargetURLStr !=null){
                        TargetURI = new Uri(TargetURLStr);
                        NotifyPropertyChanged("TargetURI");
                    }
                //}
				dbMsg += " >> " + TargetURLStr;
                NotifyPropertyChanged();
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		private bool CanGoto(string checkUrl)
		{
			string TAG = "CanGoto";
			string dbMsg = "";
			bool retBool = true;
			try {
				List<string> GoogleDriveFolderNames = new List<string>();
				dbMsg += "checkUrl=  " + checkUrl;
				dbMsg += ";DriveId=  " + Constant.WebStratUrl;
				if (!checkUrl.StartsWith(@"https://drive.google.com/drive/u/0/folders")) {
					dbMsg += "::googleDriveではない ";
					retBool = false;
				}
				//if (retBool) {
				//	if (Constant.GDriveFolders == null) {
				//		Constant.GDriveFolders = new Dictionary<string, Google.Apis.Drive.v3.Data.File>();
				//	}
				//	int FoldersCount = Constant.GDriveFiles.Count();
				//	dbMsg += ";FoldersCount=  " + FoldersCount + "件";
				//	if (GoogleDriveFolderNames.Count < 1) {
				//		//GoogleDriveUtil GDU = new GoogleDriveUtil();
				//		//GoogleDriveFolders = GDU.GDFolderListUp(DriveId);
				//		//FoldersCount = GoogleDriveFolders.Count();
				//		//dbMsg += ">>" + FoldersCount + "件";
				//		//			GoogleDriveFolderNames = new List<string>();
				//		//12/18；取敢えずファイルで確認
				//		foreach (Google.Apis.Drive.v3.Data.File forlder in Constant.GDriveFiles) {
				//			GoogleDriveFolderNames.Add(@"https://drive.google.com/drive/u/0/folders/" + forlder.Id);
				//		}
				//		dbMsg += ">>" + GoogleDriveFolderNames.Count + "件";
				//	}
				//	if (GoogleDriveFolderNames.IndexOf(checkUrl) < 0) {
				//		dbMsg += "::該当フォルダ無し";
				//		retBool = false;
				//	}
				//}
				dbMsg += ">retBool= " + retBool;
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
			return retBool;
		}
        #endregion

        #region コンテンツ読込み終了イベント
        public ICommand CompletedNavigation => new DelegateCommand(NavigationCompleted);
        /// <summary>
        /// リダイレクト先を設定する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NavigationCompleted()
		{
			string TAG = "NavigationCompleted";
			string dbMsg = "";
			try {
				dbMsg += "RedirectUrl=" + RedirectUrl;
	//			RedirectUrl = TargetURLStr;
				_isNavigating = false;
				RequeryCommands();
                //			dbMsg += ">>" + RedirectUrl;
                NotifyPropertyChanged("TargetURLStr");
                MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}
		#endregion

		#region Goボタンクリック
        public ICommand GoBTCommand => new DelegateCommand(GoBTClick);

        private void GoBTClick()
		{
			string TAG = "ButtonGo_Click";
			string dbMsg = "";
			try {
				dbMsg += ",TargetURLStr= " + TargetURLStr;
				if (TargetURLStr.StartsWith("http://") || TargetURLStr.StartsWith("https://")) {
					TargetURI = new Uri(TargetURLStr);
		//			RaisePropertyChanged("TargetURI");
				} else {
					String titolStr = "URLを入力して下さい";
					String msgStr = "アドレスバーにはhttp://もしくはhttps://で始るURLを入力して下さい";
					MessageBoxButton buttns = MessageBoxButton.YesNo;
					MessageBoxImage icon = MessageBoxImage.Exclamation;
					MessageBoxResult res = MessageShowWPF(titolStr, msgStr, buttns, icon);
				}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}
		#endregion

		#region ホームボタンクリック
        public ICommand HomeBTCommand => new DelegateCommand(HomeBTClick);
        private void HomeBTClick()
		{
			string TAG = "HomeBTClick";
			string dbMsg = "";
			try {
				dbMsg += ",TargetURLStr=" + TargetURLStr;
                TargetURLStr = @"https://www.google.com/";
        //        RaisePropertyChanged("TargetURLStr");
				dbMsg += ">>" + TargetURLStr;
                RequeryCommands();
                MyLog(TAG, dbMsg);
		//		RaisePropertyChanged();

			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}
        #endregion


        /// <summary>
        /// プロパティ変更通知を行うBindableBase
        /// ；ヘルパなしでRaisePropertyChangedの代りにNotifyPropertyChangedを使う
        /// </summary>

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //System.Windows.Input; Livet Messengerでも使う///////////////////////
        void RequeryCommands()
		{
			// Seems like there should be a way to bind CanExecute directly to a bool property
			// so that the binding can take care keeping CanExecute up-to-date when the property's
			// value changes, but apparently there isn't.  Instead we listen for the WebView events
			// which signal that one of the underlying bool properties might have changed and
			// bluntly tell all commands to re-check their CanExecute status.
			//
			// Another way to trigger this re-check would be to create our own bool dependency
			// properties on this class, bind them to the underlying properties, and implement a
			// PropertyChangedCallback on them.  That arguably more directly binds the status of
			// the commands to the WebView's state, but at the cost of having an extraneous
			// dependency property sitting around for each underlying property, which doesn't seem
			// worth it, especially given that the WebView API explicitly documents which events
			// signal the property value changes.
			CommandManager.InvalidateRequerySuggested();
		}

        public static void MyLog(string TAG, string dbMsg)
		{
			dbMsg = "[WsbViewModel ]" + dbMsg;
			//dbMsg = "[" + MethodBase.GetCurrentMethod().Name + "]" + dbMsg;
			CS_Util Util = new CS_Util();
			Util.MyLog(TAG, dbMsg);
		}

		public static void MyErrorLog(string TAG, string dbMsg, Exception err)
		{
			dbMsg = "[WsbViewModel ]" + dbMsg;
			CS_Util Util = new CS_Util();
			Util.MyErrorLog(TAG, dbMsg, err);
		}

		public MessageBoxResult MessageShowWPF(String titolStr, String msgStr,
																		MessageBoxButton buttns,
																		MessageBoxImage icon
																		)
		{
			CS_Util Util = new CS_Util();
			return Util.MessageShowWPF(msgStr, titolStr, buttns, icon);
		}
	}
}

/*
 https://docs.microsoft.com/ja-jp/microsoft-edge/webview2/gettingstarted/wpf
 */
