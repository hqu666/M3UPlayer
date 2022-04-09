using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using M3UPlayer.Models;
using M3UPlayer.ViewModels;

namespace M3UPlayer.Views {
	/// <summary>
	/// ベースになる画面
	/// </summary>
	public partial class MainWindow : Window {
		MainViewModel VM;
		/// <summary>
		/// ドラッグ中
		/// </summary>
		public bool IsDragging;

		public MainWindow() {

			string TAG = "this_loaded";
			string dbMsg = "";
			try {
				InitializeComponent();
				VM = new MainViewModel();
				VM.MyView = this;
				this.DataContext = VM;
				this.Loaded += this_loaded;
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}

		}

		/// <summary>
		/// リソースの読み込み後
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void this_loaded(object sender, RoutedEventArgs e) {
			string TAG = "this_loaded";
			string dbMsg = "";
			try {
				//ViewModelのViewプロパティに自分のインスタンス（つまりViewのインスタンス）を渡しています。
				VM.MyView = this;
				InitializeAsync();
				//初期表示
				PlayListModel targetItem = new PlayListModel();
				targetItem.UrlStr = "https://www.yahoo.co.jp/";
				//	targetItem.UrlStr = "https://www.google.co.jp/maps/";
				targetItem.Summary = "StartUp";
				//	VM.PlayListToPlayer(targetItem);
				IsDragging = false;
				if (Properties.Settings.Default.IsFullScreen) {
					WindowState = WindowState.Maximized;
				} else {
					WindowState = WindowState.Normal;
				}
				dbMsg += "、WindowState=" + WindowState;
				if (WindowState == WindowState.Normal) {
					dbMsg += "(" + Left + "," + Top + ")[" + Width + "," + Height + "]";
					// Settings の値をウィンドウに反映
					Left = Properties.Settings.Default.WindowLeft;
					Top = Properties.Settings.Default.WindowTop;
					Width = Properties.Settings.Default.WindowWidth;
					Height = Properties.Settings.Default.WindowHeight;
				}
				dbMsg += ",ListWidth=" + Properties.Settings.Default.ListWidth;
				ListColumnDefinition.Width = new GridLength(Properties.Settings.Default.ListWidth);
				//		PlayList.Width =  Properties.Settings.Default.ListWidth;
				if (Double.Parse(ListColumnDefinition.Width.ToString()) < 600) {
					ListColumnDefinition.Width = new GridLength(600);
				}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		///webView2 ///////////////////////////////////////////////////////////////////////////////////////////
		readonly CountdownEvent condition = new CountdownEvent(1);
		/// <summary>
		/// webView2初期化
		/// </summary>
		async void InitializeAsync() {
			//初期化の完了を待たないでwebView2.CoreWebView2にアクセスするとNullReferenceExceptionが発生するので対策
			await webView.EnsureCoreWebView2Async(null);
			////イベント割り付け
			//webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e) {
		//	string TAG = "WebView_NavigationCompleted";
		//	string dbMsg = "";
		//	try {
		//		//読み込み結果を判定
		//		if (e.IsSuccess) {
		//			dbMsg = "complete";
		//		} else {
		//			dbMsg = "WebErrorStatus=" + e.WebErrorStatus;
		//		}

		//		//シグナル初期化
		//		condition.Signal();
		//		System.Threading.Thread.Sleep(1);
		//		condition.Reset(); MyLog(TAG, dbMsg);
		//	} catch (Exception er) {
		//		MyErrorLog(TAG, dbMsg, er);
		//	}
		//}
		//////////////////////////////////////////////////////////////////////////////////////////webView2////

		private void Window_Closed(object sender, EventArgs e) {
		}

		/// <summary>
		/// クローズボックスなどで終了する場合
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			string TAG = "Window_Closing";
			string dbMsg = "";
			try {
				VM.BeforeClose();
				dbMsg += "、WindowState=" + WindowState;
				if (WindowState == WindowState.Normal) {
					dbMsg += "(" + Left + "," + Top + ")[" + Width + "," + Height + "]";                    
					// ウィンドウの値を Settings に格納
					Properties.Settings.Default.WindowLeft = Left;
					Properties.Settings.Default.WindowTop = Top;
					Properties.Settings.Default.WindowWidth = Width;
					Properties.Settings.Default.WindowHeight = Height;
					Properties.Settings.Default.IsFullScreen = false;
				} else {
					Properties.Settings.Default.IsFullScreen = true;
				}
				dbMsg += ",ListWidth=" + ListColumnDefinition.Width;
				Properties.Settings.Default.ListWidth = Double.Parse(ListColumnDefinition.Width.ToString());
				//dbMsg += ",ListWidth=" + PlayList.Width;
				//Properties.Settings.Default.ListWidth = PlayList.Width;
				// ファイルに保存
				Properties.Settings.Default.Save();
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		//Drag: https://hilapon.hatenadiary.org/entry/20110209/1297247754 ///////////////////////////////////////////////////////////////////////
		private bool _isEditing;
		private ObservableCollection<PlayListModel> _shareTable;
		/// <summary>
		/// DraggedItem Dependency Property
		/// </summary>
		public static readonly DependencyProperty DraggedItemProperty =
				DependencyProperty.Register("DraggedItem", typeof(PlayListModel), typeof(MainWindow));

		/// <summary>
		/// Gets or sets the DraggedItem property. This dependency property indicates ....
		/// </summary>
		public PlayListModel DraggedItem {
			get { return (PlayListModel)GetValue(DraggedItemProperty); }
			set { SetValue(DraggedItemProperty, value); }
		}

		/// <summary>
		/// State flag which indicates whether the grid is in edit
		/// mode or not.
		/// </summary>
		//public void OnBeginEdit(object sender, DataGridBeginningEditEventArgs e) {
		//	string TAG = "[OnBeginEdit]";
		//	string dbMsg = "";
		//	try {
		//		_isEditing = true;
		//		if (IsDragging) ResetDragDrop();
		//		MyLog(TAG, dbMsg);
		//	} catch (Exception er) {
		//		MyErrorLog(TAG, dbMsg, er);
		//	}
		//}

		//public void OnEndEdit(object sender, DataGridCellEditEndingEventArgs e) {
		//	string TAG = "[OnEndEdit]";
		//	string dbMsg = "";
		//	try {
		//		_isEditing = false;
		//		MyLog(TAG, dbMsg);
		//	} catch (Exception er) {
		//		MyErrorLog(TAG, dbMsg, er);
		//	}
		//}

		///// <summary>
		///// Initiates a drag action if the grid is not in edit mode.
		///// </summary>
		//private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
		//	string TAG = "[OnMouseLeftButtonDown]";
		//	string dbMsg = "";
		//	try {
		//		DataGrid droplist = (DataGrid)sender;
		//		dbMsg += ",AllowDrop=" + droplist.AllowDrop;
		//		dbMsg += "[" + droplist.SelectedIndex + "]";
		//		PlayListModel selectedItem = (PlayListModel)droplist.SelectedItem;
		//		dbMsg += ",Summary=" + selectedItem.Summary;
		//		dbMsg += ",UrlStr=" + selectedItem.UrlStr;
		//		//if (_isEditing) return;

		//		//				var row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(shareGrid));
		//	//	if (row == null || row.IsEditing) return;

		//		//set flag that indicates we're capturing mouse movements
		//		IsDragging = true;
		//		DraggedItem = (PlayListModel)droplist.SelectedItem;
		//		//				DraggedItem = (PlayListModel)row.Item;
		//		MyLog(TAG, dbMsg);
		//	} catch (Exception er) {
		//		MyErrorLog(TAG, dbMsg, er);
		//	}
		//}

		/// <summary>
		/// Completes a drag/drop operation.
		/// </summary>
		//private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
		//	string TAG = "[OnMouseLeftButtonUp]";
		//	string dbMsg = "";
		//	try {
		//		DataGrid droplist = (DataGrid)sender;
		//		dbMsg += ",AllowDrop=" + droplist.AllowDrop;
		//		dbMsg += "[" + droplist.SelectedIndex + "]";
		//		PlayListModel selectedItem = (PlayListModel)droplist.SelectedItem;
		//		dbMsg += ",Summary=" + selectedItem.Summary;
		//		dbMsg += ",UrlStr=" + selectedItem.UrlStr;
		//		if (!IsDragging || _isEditing) {
		//			return;
		//		}

		//		//get the target item
		//		PlayListModel targetItem = (PlayListModel)droplist.SelectedItem;

		//		if (targetItem == null || !ReferenceEquals(DraggedItem, targetItem)) {

		//			//// create tempporary row
		//			//var temp = DraggedItem.Row.Table.NewRow();
		//			//temp.ItemArray = DraggedItem.Row.ItemArray;
		//			//int tempIndex = _shareTable.Rows.IndexOf(DraggedItem.Row);

		//			////remove the source from the list
		//			//_shareTable.Rows.Remove(DraggedItem.Row);

		//			////get target index
		//			//var targetIndex = _shareTable.Rows.IndexOf(targetItem.Row);

		//			////insert temporary at the target's location
		//			//_shareTable.Rows.InsertAt(temp, targetIndex);

		//			////select the dropped item
		//			//shareGrid.SelectedItem = shareGrid.Items[targetIndex];
		//		}

		//		//reset
		//		ResetDragDrop();
		//		MyLog(TAG, dbMsg);
		//	} catch (Exception er) {
		//		MyErrorLog(TAG, dbMsg, er);
		//	}
		//}

		///// <summary>
		///// Updates the popup's position in case of a drag/drop operation.
		///// </summary>
		//private void OnMouseMove(object sender, MouseEventArgs e) {
		//	string TAG = "[OnMouseMove]";
		//	string dbMsg = "";
		//	try {
		//		DataGrid droplist = (DataGrid)sender;
		//		dbMsg += ",AllowDrop=" + droplist.AllowDrop;
		//		dbMsg += "[" + droplist.SelectedIndex + "]";
		//		PlayListModel selectedItem = (PlayListModel)droplist.SelectedItem;
		//		dbMsg += ",Summary=" + selectedItem.Summary;
		//		dbMsg += ",UrlStr=" + selectedItem.UrlStr;
		//		if (!IsDragging || e.LeftButton != MouseButtonState.Pressed) return;

		//		//display the popup if it hasn't been opened yet
		//		if (!popup1.IsOpen) {
		//			//switch to read-only mode
		//		//	PlayList.IsReadOnly = true;

		//			//make sure the popup is visible
		//			popup1.IsOpen = true;
		//		}

		//		Size popupSize = new Size(popup1.ActualWidth, popup1.ActualHeight);
		//		popup1.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);

		//		//make sure the row under the grid is being selected
		//		Point position = e.GetPosition(PlayList);
		//	//	var row = UIHelpers.TryFindFromPoint<DataGridRow>(PlayList, position);
		//	//	if (row != null) PlayList.SelectedItem = droplist.SelectedItem;
		//		MyLog(TAG, dbMsg);
		//	} catch (Exception er) {
		//		MyErrorLog(TAG, dbMsg, er);
		//	}
		//}

		/// <summary>
		/// Closes the popup and resets the grid to read-enabled mode.
		/// </summary>
		///  PlayList_MouseUpから呼ばれる
		private void ResetDragDrop() {
			string TAG = "[ResetDragDrop]";
			string dbMsg = "";
			try {
				DraggedItem = null;
				IsDragging = false;
				popup1.IsOpen = false;
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}


		/// <summary>
		/// ドラッグが開始された時のイベント処理
		/// ファイルドラッグに反応
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayList_PreviewDragOver(object sender, DragEventArgs e) {
			string TAG = "PlayList_PreviewDragOver";
			string dbMsg = "";
			try {
				//ファイルがドラッグされたとき、カーソルをドラッグ中のアイコンに変更し、そうでない場合は何もしない。
				e.Effects = (e.Data.GetDataPresent(DataFormats.FileDrop)) ? DragDropEffects.Copy : e.Effects = DragDropEffects.None;
				e.Handled = true;
				//MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		/// <summary>
		/// ファイルのドロップ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayList_PreviewDrop(object sender, DragEventArgs e) {
			string TAG = "PlayList_PreviewDrop";
			string dbMsg = "";
			try {
				if (e.Data.GetDataPresent(DataFormats.FileDrop)) // ドロップされたものがファイルかどうか確認する。
				{
					List<PlayListModel> dropPlayListFiles = new List<PlayListModel>();
					string[] paths = ((string[])e.Data.GetData(DataFormats.FileDrop));
					foreach (string path in paths) {
						dbMsg += "\r\n" + path;
						IEnumerable<string> files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
						foreach (string file in files) {
							PlayListModel plm = new PlayListModel();
							plm.UrlStr = file;
							dropPlayListFiles.Add(plm);
							dbMsg += "\r\n[" + dropPlayListFiles.Count + "]" + plm.UrlStr;
						}

					}
					var dataGrid = sender as DataGrid;
					var point = e.GetPosition(dataGrid);
					var row = GetDataGridObject<DataGridRow>(dataGrid, point);
					if (row == null) {
						return;
					}
					// 行オブジェクトから行インデックス(0起算)を取得します。
					int dropRow = row.GetIndex();
					dbMsg += ",dropRow=" + dropRow + "" + dropPlayListFiles.Count + "件";
					VM.PlayListItemMoveTo(dropRow, dropPlayListFiles);
					IsDragging = false;
				}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		///////////////////////////////////////////////////////Drag: https://hilapon.hatenadiary.org/entry/20110209/1297247754 //////////////////
		private Brush _previousFill = null;
		/// <summary>
		/// ドラッグオブジェクトがコントロールの境界内にドラッグされると発生
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayList_DragEnter(object sender, DragEventArgs e) {
			string TAG = "[PlayListBox_DragEnter]";
			string dbMsg = "";
			try {
				Ellipse ellipse = sender as Ellipse;
				if (ellipse != null) {
					// Save the current Fill brush so that you can revert back to this value in DragLeave.
					_previousFill = ellipse.Fill;

					// If the DataObject contains string data, extract it.
					if (e.Data.GetDataPresent(DataFormats.StringFormat)) {
						string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

						// If the string can be converted into a Brush, convert it.
						BrushConverter converter = new BrushConverter();
						if (converter.IsValid(dataString)) {
							Brush newFill = (Brush)converter.ConvertFromString(dataString);
							ellipse.Fill = newFill;
						}
					}
				} else {
					dbMsg += ",ellipse = null";
				}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		/// <summary>
		/// DragLeave	オブジェクトがコントロールの境界外にドラッグされたときに発生
		/// //		https://dobon.net/vb/dotnet/control/draganddrop.html
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayList_DragLeave(object sender, DragEventArgs e) {
			string TAG = "[PlayList_DragLeave]";// + fileName;
			string dbMsg = "";
			try {
				Ellipse? ellipse = sender as Ellipse;
				if (ellipse != null) {
					ellipse.Fill = _previousFill;
				} else {
					dbMsg += ",ellipse = null";
				}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}

		}

		/// <summary>
		/// ファイルのドロップ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayList_Drop(object sender, DragEventArgs e) {
			string TAG = "[PlayList_Drop]";
			string dbMsg = "";
			try {
				Ellipse? ellipse = sender as Ellipse;
				if (ellipse != null) {
					// If the DataObject contains string data, extract it.
					if (e.Data.GetDataPresent(DataFormats.StringFormat)) {
						string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

						// If the string can be converted into a Brush,
						// convert it and apply it to the ellipse.
						BrushConverter converter = new BrushConverter();
						if (converter.IsValid(dataString)) {
							Brush newFill = (Brush)converter.ConvertFromString(dataString);
							ellipse.Fill = newFill;
						}
					}
				} else {
					dbMsg += ",ellipse = null";
				}
				////DataGrid DG = (DataGrid)sender;
				////// ドロップ先(dataGridView2)のクライアント位置からDataGridViewの位置情報を取得します。
				////var point = DG.po.PointToClient(new Point(e.X, e.Y));
				////var hitTest = DG.HitTest(point.X, point.Y);
				//int InsertTo = 0;   //挿入位置は先頭固定
				//// ファイルのドラッグアンドドロップのみを受け付けるようにしています。
				//if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				//	// ドロップされたファイルは、アプリケーション側に内容がコピーされるものとします。
				//	//	e.Effect = DragDropEffects.Copy;
				//}
				//// ドラッグアンドドロップされたファイルのパス情報を取得します。

				//foreach (String filename in (string[])e.Data.GetData(DataFormats.FileDrop)) {
				//	dbMsg += "\r\n" + filename;
				//}

				//string[] rFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
				//if (0 < rFiles.Count()) {
				//	foreach (string url in rFiles) {
				//		dbMsg += "\r\n" + url;
				//		if (File.Exists(url)) {
				//			if (VM.AddToPlayList(url, 0)) {
				//				dbMsg += ">>格納";
				//			}
				//		} else if (Directory.Exists(url)) {
				//			//フォルダなら中身の全ファイルで再起する
				//			string[] r2files = System.IO.Directory.GetFiles(url, "*", SearchOption.AllDirectories);
				//			VM.FilesAdd(r2files, InsertTo);
				//		}
				//	}
				//}
				IsDragging = false;
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}


		/// <summary>
		/// ファイルがコントロールの境界を越えてドラッグされると発生
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayList_DragOver(object sender, DragEventArgs e) {
			string TAG = "[PlayListBox_DragOver]";// + fileName;
			string dbMsg = "";
			try {
				//		https://dobon.net/vb/dotnet/control/draganddrop.html
				//dbMsg += "dragFrom=" + dragFrom;
				//dbMsg += ",dragSouceUrl=" + dragSouceUrl;
				//dbMsg += ",DDEfect=" + DDEfect;
				////		Object senderObject = sender;                                 //playListが参照される
				////		+Items   { System.Windows.Forms.ListBox.ObjectCollection}		System.Windows.Forms.ListBox.ObjectCollection
				//if (dragFrom == playListBox.Name) {
				//	if (e.Data.GetDataPresent(typeof(string))) {                //ドラッグされているデータがstring型か調べる
				//		if ((e.KeyState & 8) == 8 && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) {                //Ctrlキーが押されていればCopy//"8"はCtrlキーを表す
				//			e.Effect = DragDropEffects.Copy;
				//		} else if ((e.KeyState & 32) == 32 && (e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link) {   //Altキーが押されていればLink//"32"はAltキーを表す
				//			e.Effect = DragDropEffects.Link;
				//		} else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move) {                              //何も押されていなければMove
				//			e.Effect = DragDropEffects.Move;
				//		} else {
				//			//			e.Effect = DragDropEffects.None;
				//		}
				//	} else {
				//		//		e.Effect = DragDropEffects.None;                    //string型でなければ受け入れない
				//	}
				//} else {
				//	e.Effect = DragDropEffects.All;
				//}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}


		//        /// <summary>
		//        /// ドラッグ アンド ドロップ操作が完了したときに発生
		//        /// </summary>
		//        /// <param name="sender"></param>
		//        /// <param name="e"></param>
		//        private void PlayListBox_DragDrop(object sender, DragEventArgs e)
		//        {
		//            string TAG = "[PlayListBox_DragDrop]";
		//            string dbMsg = TAG;
		//            try
		//            {
		//                dbMsg += "dragFrom=" + dragFrom;
		//                dbMsg += ",dragSouceUrl=" + dragSouceUrl;
		//                dbMsg += ",DDEfect=" + DDEfect;

		//                /*
		//								if (DragURLs.Count < 1) {
		//									DragURLs = new List<string>();
		//									foreach (string item in (string[])e.Data.GetData(DataFormats.FileDrop)) {
		//										dbMsg += ",=" + item.ToString();
		//										DragURLs.Add(item.ToString());
		//									}
		//									dbMsg += ",=" + DragURLs.Count + "件";
		//									dragFrom = "other";
		//									dragSouceUrl = DragURLs[0];
		//									DDEfect = DragDropEffects.Copy;
		//								}
		//								*/
		//                if (dragFrom != "" && dragSouceUrl != "")
		//                {                                               //
		//                    Point dropPoint = Control.MousePosition;                            //dropPoint取得☆最優先にしないと取れなくなる
		//                    dropPoint = playListBox.PointToClient(dropPoint);                   //ドロップ時のマウスの位置をクライアント座標に変換
		//                    dbMsg += "(dropPoint;" + dropPoint.X + "," + dropPoint.Y + ")";
		//                    int dropPointIndex = playListBox.IndexFromPoint(dropPoint);         //マウス下のＬＢのインデックスを得る
		//                    dbMsg += "(dropPointIndex;" + dropPointIndex + "/" + playListBox.Items.Count + ")";//

		//                    ListBox droplist = (ListBox)sender;
		//                    string dropSouceUrl = "";
		//                    if (-1 < dropPointIndex)
		//                    {
		//                        dropSouceUrl = playListBox.Items[dropPointIndex].ToString();             //☆ (ListBox)senderで拾えない
		//                    }
		//                    else if (0 < playListBox.Items.Count)
		//                    {
		//                        dropSouceUrl = playListBox.Items[playListBox.Items.Count - 1].ToString();             //☆ (ListBox)senderで拾えない
		//                        dropPointIndex = playListBox.Items.Count;
		//                        dbMsg += ">>(dropPointIndex;" + dropPointIndex + ")";//
		//                    }
		//                    else
		//                    {
		//                        dropPointIndex = 0;
		//                    }
		//                    dbMsg += ",dropSouceUrl=" + dropSouceUrl;
		//                    string playList = PlaylistComboBox.Text;
		//                    dbMsg += ",playList=" + playList;
		//                    if (e.Data.GetDataPresent(typeof(string)))
		//                    {                                 //ドロップされたデータがstring型か調べる
		//                        dragSouceUrl = (string)e.Data.GetData(typeof(string));                    //ドロップされたデータ(string型)を取得
		//                        dbMsg += ",e.Data:dragSouceUrl=" + dragSouceUrl;
		//                    }

		//                    if (b_dragSouceUrl != dragSouceUrl)
		//                    {                                           //二重動作回避？？発生原因不明
		//                                                                //			if (dropPointIndex > -1 && dropPointIndex < playListBox.Items.Count) {      //dropPointがplayList内で取得出来たら
		//                        b_dragSouceUrl = dragSouceUrl;                                                                   //	dropSouceUrl = e.Data.GetData(DataFormats.Text).ToString(); //ドラッグしてきたアイテムの文字列をstrに格納する☆他リストからは参照できない
		//                        if (dragFrom == playListBox.Name)
		//                        {                                     //プレイリスト内の移動なら		draglist == droplist
		//                            if (dragSouceIDl != dropPointIndex)
		//                            {
		//                                dbMsg += "を;" + dropPointIndex + "に移動";
		//                                DelFromPlayList(playList, dragSouceIDl);                        //一旦削除
		//                                if (dragSouceIDl < dropPointIndex)
		//                                {
		//                                    dropPointIndex--;
		//                                }
		//                            }
		//                        }
		//                        dbMsg += ">>>" + dropSouceUrl;
		//                        Files2PlayListIndex(playList, dragSouceUrl, dropPointIndex);
		//                        dragSouceUrl = "";
		//                        dbMsg += ",最終選択=" + dropPointIndex;
		//                        droplist.SelectedIndex = dropPointIndex;          //選択先のインデックスを指定
		//                        plaingItem = playListBox.SelectedValue.ToString();
		//                        dbMsg += ";plaingItem=" + plaingItem;
		//                        //					playListBox.Items[dragSouceIDP] = playListBox.Items[ind];
		//                        //					playListBox.Items[ind] = str;
		//                        //draglist.DoDragDrop("", DragDropEffects.None);//ドラッグスタート
		//                        //			}
		//                    }
		//                    else
		//                    {
		//                        dbMsg += "<<二重発生回避>>";
		//                    }
		//                }
		//                dragFrom = "";
		//                //	dragSouceUrl = "";
		//                dragSouceIDl = -1;
		//                DDEfect = DragDropEffects.None;
		//                MyLog(TAG, dbMsg);
		//            }
		//            catch (Exception er)
		//            {
		//                dbMsg += "<<以降でエラー発生>>" + er.Message;
		//                MyLog(TAG, dbMsg);
		//            }
		//        }


		private void PlayList_PreviewDragEnter(object sender, DragEventArgs e) {
			string TAG = "[PlayList_PreviewDragEnter]";
			string dbMsg = "";
			try {
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		private void PlayList_PreviewDragLeave(object sender, DragEventArgs e) {
			string TAG = "[PlayList_PreviewDragLeave]";
			string dbMsg = "";
			try {
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		/// <summary>
		/// 引数の位置のDataGridのオブジェクトを取得
		/// https://www.paveway.info/entry/2020/05/27/wpf_datgridpos
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dataGrid"> dataGrid データグリッド</param>
		/// <param name="point">point 位置</param>
		/// <returns>DataGridのオブジェクト</returns>
		private T GetDataGridObject<T>(DataGrid dataGrid, Point point) {
			T result = default(T);
			var hitResultTest = VisualTreeHelper.HitTest(dataGrid, point);
			if (hitResultTest != null) {
				var visualHit = hitResultTest.VisualHit;
				while (visualHit != null) {
					if (visualHit is T) {
						result = (T)(object)visualHit;
						break;
					}
					visualHit = VisualTreeHelper.GetParent(visualHit);
				}
			}
			return result;
		}

		/// <summary>
		/// 始めのマウスクリック
		/// https://dobon.net/vb/dotnet/control/draganddrop.html
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayList_MouseDown(object sender, MouseButtonEventArgs e) {
			string TAG = "[PlayList_MouseDown]";// + fileName;
			string dbMsg = "";
			try {
				//	DataGrid droplist = (DataGrid)sender;
				//	if (droplist != null) {
				//		dbMsg += ",AllowDrop=" + droplist.AllowDrop;
				//		dbMsg += "[" + droplist.SelectedIndex + "]";
				//		DraggedItem = (PlayListModel)droplist.SelectedItem;
				////		dbMsg += ",UrlStr=" + DraggedItem.UrlStr;
				////20220320		IsDragging = true;
				//	} else {
				//		dbMsg += "droplist == null";
				IsDragging = false;
				MoveCount = 0;
				//	}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		private int MoveCount = 0;


		private void PlayList_MouseMove(object sender, MouseEventArgs e) {
			string TAG = "PlayList_MouseMove";
			string dbMsg = "";
			try {
				dbMsg += "[" + MoveCount + "]";

				dbMsg += "左クリック";
				if (e.LeftButton == MouseButtonState.Released) {
					dbMsg += "してない";
					if (IsDragging) {
						dbMsg += "離した";
						var dataGrid = sender as DataGrid;
						var point = e.GetPosition(dataGrid);
						var row = GetDataGridObject<DataGridRow>(dataGrid, point);
						if (row == null) {
							return;
						}
						// 行オブジェクトから行インデックス(0起算)を取得します。
						int dropRow = row.GetIndex();
						dbMsg += ",dropRow=" + dropRow;
						VM.PlayList_Drop(dropRow);
						IsDragging = false;
					}
					VM.Drag_now = false;

					Ellipse? ellipse = sender as Ellipse;
					if (ellipse != null && e.LeftButton == MouseButtonState.Pressed) {
						DragDrop.DoDragDrop(ellipse,
											 ellipse.Fill.ToString(),
											 DragDropEffects.Copy);
					}
					MoveCount = 0;
				} else if (e.LeftButton == MouseButtonState.Pressed) {
					dbMsg += "している";
					if (!IsDragging && 2 < MoveCount) {
						dbMsg += "、まだドラッグしていない";
						IsDragging = VM.PlayList_DragEnter();              //Drag_nowが返される
																		   //display the popup if it hasn't been opened yet
						if (!popup1.IsOpen) {
							popup1.IsOpen = true;
							dbMsg += "DataGrid内のpopアップを表示させる";
						}

					}
					Size popupSize = new Size(popup1.ActualWidth, popup1.ActualHeight);
					popup1.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);

					//make sure the row under the grid is being selected
					Point position = e.GetPosition(PlayList);

					Ellipse ellipse = sender as Ellipse;        //楕円？
					if (ellipse != null && e.LeftButton == MouseButtonState.Pressed) {
						DragDrop.DoDragDrop(ellipse,
											 ellipse.Fill.ToString(),
											 DragDropEffects.Copy);
					}
				}
				//	MyLog(TAG, dbMsg);
				MoveCount++;
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		/// <summary>
		/// マウスボタンを離す
		/// 右クリックされたアイテムからフルパスをグローバル変数に設定
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayList_MouseUp(object sender, MouseButtonEventArgs e) {
			string TAG = "[PlayList_MouseUp]";
			string dbMsg = "";
			try {
				dbMsg += "IsDragging=" + IsDragging;
				if (IsDragging) {
					dbMsg += "Drag中";
					var dataGrid = sender as DataGrid;
					var point = e.GetPosition(dataGrid);
					var row = GetDataGridObject<DataGridRow>(dataGrid, point);
					if (row == null) {
						return;
					}
					// 行オブジェクトから行インデックス(0起算)を取得します。
					int dropRow = row.GetIndex();
					dbMsg += ",dropRow=" + dropRow;
					VM.PlayList_Drop(dropRow);
					ResetDragDrop();
				} else {
					dbMsg += "Drag中ではない";
					VM.PLMouseUp();
				}
				IsDragging = false;
				MoveCount = 0;
				//DataGrid droplist = (DataGrid)sender;
				//if (droplist != null) {
				//	dbMsg += ",AllowDrop=" + droplist.AllowDrop;
				//	dbMsg += "[" + droplist.SelectedIndex + "]";
				//	PlayListModel targetItem = (PlayListModel)droplist.SelectedItem;
				//	//get the target item
				//	if (DraggedItem != null) {
				//		dbMsg += "<<Dragged=" + DraggedItem.Summary;
				//		if (DraggedItem == targetItem) {
				//			VM.PlayListToPlayer(targetItem);
				//		} else {
				//			VM.PlayListItemMoveTo(DraggedItem, targetItem);
				//		}
				//	} else {

				//	}

				//	//　参考
				//	if (targetItem == null || ReferenceEquals(DraggedItem, targetItem)) {
				//		dbMsg += ">参考>ReferenceEquals";
				//	}

				//reset
				//} else {
				//	dbMsg += "droplist == null";
				//}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}


		////アイコン
		//private Cursor noneCursor = new Cursor("none.cur");
		//private Cursor moveCursor = new Cursor("move.cur");
		//private Cursor copyCursor = new Cursor("copy.cur");
		//private Cursor linkCursor = new Cursor("link.cur");

		private void PlayListBox_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
			string TAG = "PlayListBox_GiveFeedback";
			string dbMsg = TAG;
			try {
				//	https://dobon.net/vb/dotnet/control/draganddrop.html

				dbMsg += "Effect=" + e.Effects.ToString();
				e.UseDefaultCursors = false;                //既定のカーソルを使用しない
															//ドロップ効果にあわせてカーソルを指定する
				if ((e.Effects & DragDropEffects.Move) == DragDropEffects.Move) {
					//Cursor = moveCursor;
				} else if ((e.Effects & DragDropEffects.Copy) == DragDropEffects.Copy) {
					//Cursor = copyCursor;
				} else if ((e.Effects & DragDropEffects.Link) == DragDropEffects.Link) {
					//Cursor = linkCursor;
				} else {
					//Cursor = noneCursor;
				}

				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(TAG, dbMsg);
			}
		}

		/// <summary>
		/// 再生ポジションスライダーのツマミMouseDown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PositionSL_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
			string TAG = "PositionSL_DragStarted";
			string dbMsg = TAG;
			try {
				Slider slider = (Slider)sender;
				double newValue = slider.Value;
				dbMsg += "newValue=" + newValue;
				VM.PauseVideo();
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(TAG, dbMsg);
			}
		}

		/// <summary>
		/// FFコンボの変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ForwardCB_DropDownClosed(object sender, EventArgs e) {
			string TAG = "ForwardCB_DropDownClosed";
			string dbMsg = TAG;
			try {
				VM.ClickForwardAsync();
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(TAG, dbMsg);
			}
		}

		private void RewCB_DropDownClosed(object sender, EventArgs e) {
			string TAG = "RewCB_DropDownClosed";
			string dbMsg = TAG;
			try {
				VM.ClickRewAsync();
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(TAG, dbMsg);
			}
		}


		/// <summary>
		/// 再生ポジションスライダーのツマミ Thumb の DragCompleted
		/// 再生ポジションスライダーが変更された
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PositionSL_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
			string TAG = "PositionSL_DragCompleted";
			string dbMsg = TAG;
			try {
				Slider slider = (Slider)sender;
				double newValue = slider.Value;
				dbMsg += "newValue=" + newValue;
				VM.PositionSliderValueChang(newValue);
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(TAG, dbMsg);
			}
		}


		/// <summary>
		/// 音量調整スライダー
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SoundSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
			string TAG = "SoundSlider_DragCompleted";
			string dbMsg = TAG;
			try {
				Slider slider = (Slider)sender;
				double newValue = slider.Value;
				dbMsg += "newValue=" + newValue;
				VM.SetMediaVolume();
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(TAG, dbMsg);
			}
		}
		/// ///////////////////////////////////////////////////////////////////////
		private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
			string TAG = "[MainFrame_Navigated]";
			string dbMsg = "";
			try {
				Frame FR = (Frame)sender;
				string targetUrl = e.ExtraData.ToString();
				//string contentName = FR.Content.GetType().Name;
				//if (contentName.Equals("WebPage")) {
				//	dbMsg += contentName +"へ" + targetUrl;
				//}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}


		/// /////////////////////////////////////////////////////////////////////////
		public static void MyLog(string TAG, string dbMsg) {
			dbMsg = "[MainWindow]" + dbMsg;
			//dbMsg = "[" + MethodBase.GetCurrentMethod().Name + "]" + dbMsg;
			CS_Util Util = new CS_Util();
			Util.MyLog(TAG, dbMsg);
		}

		public static void MyErrorLog(string TAG, string dbMsg, Exception err) {
			dbMsg = "[MainWindow]" + dbMsg;
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
