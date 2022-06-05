using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
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
				Dispatcher.BeginInvoke(new Action(() => {
					VM.FreamWidth = webView.ActualWidth;
					VM.FreamHeigh = webView.ActualHeight;
				}),
				DispatcherPriority.Loaded);
				SizeChanged += (sender, args) => {
					VM.FreamWidth = webView.ActualWidth;
					VM.FreamHeigh = webView.ActualHeight;
				};
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
		public List<PlayListModel> dropPlayListFiles;
		private Brush _previousFill = null;

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
		/// VMのリスト追加へ継続
		/// </summary>
		/// <param name="dataGrid">対象となるDataGrid</param>
		/// <param name="point">DataGrid上のDrop位置</param>
		/// <param name="dropPlayListFiles">ファイル配列</param>
		private void DropFileAdder(DataGrid dataGrid, Point point, List<PlayListModel> dropPlayListFiles) {
			string TAG = "DropFileAdder";
			string dbMsg = "";
			try {
				DataGridRow row = GetDataGridObject<DataGridRow>(dataGrid, point);
				if (row == null) {
					dbMsg += "行が拾えない";
				} else {
					// 行オブジェクトから行インデックス(0起算)を取得します。
					int dropRow = row.GetIndex();
					dbMsg += ",dropRow=" + dropRow + "行目に" + dropPlayListFiles.Count + "件";
					VM.PlayListItemMoveTo(dropRow, dropPlayListFiles);
					IsDragging = false;
				}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}

		/// <summary>
		/// Dropで割り付けたリスナー
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
					if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
						dbMsg += "," + dropPlayListFiles.Count + "件" ;
						DataGrid? dataGrid = sender as DataGrid;
						if (dataGrid != null) {
							Point point = e.GetPosition(dataGrid);
							DropFileAdder(dataGrid, point, dropPlayListFiles);
						} else {
							dbMsg += ",DataGridがnull";
						}
					}
				}
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
			string TAG = "PlayListBox_DragOver";// + fileName;
			string dbMsg = "";
			try {
				// https://araramistudio.jimdo.com/2021/02/15/c-%E3%81%AEwpf%E3%81%A7%E3%83%95%E3%82%A1%E3%82%A4%E3%83%AB%E5%90%8D%E3%82%92%E3%83%89%E3%83%A9%E3%83%83%E3%82%B0-%E3%83%89%E3%83%AD%E3%83%83%E3%83%97%E3%81%A7%E5%8F%97%E3%81%91%E5%8F%96%E3%82%8B/
				string dataString = "";
				BrushConverter converter = new BrushConverter();
				Ellipse? ellipse = sender as Ellipse;		//Fileでは発生しない
				if (ellipse != null) {
					// Save the current Fill brush so that you can revert back to this value in DragLeave.
					_previousFill = ellipse.Fill;
				}
				if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
					dbMsg += "File";
					dropPlayListFiles = new List<PlayListModel>();
					var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
					foreach (var fName in fileNames) {
						PlayListModel plm = new PlayListModel();
						plm.UrlStr = fName;
						dropPlayListFiles.Add(plm);
						dbMsg += "\r\n[" + dropPlayListFiles.Count + "]" + plm.UrlStr;
						dataString += fName + "\r\n";
					}
					dbMsg += "," + dropPlayListFiles.Count + "件";
					e.Effects = DragDropEffects.All;
					popup_text.Text = dataString;	
				} else if (e.Data.GetDataPresent(DataFormats.StringFormat)) {
					dbMsg += "data";
			//		if (ellipse != null) {
						// If the DataObject contains string data, extract it.
						if (e.Data.GetDataPresent(DataFormats.StringFormat)) {
							dataString = (string)e.Data.GetData(DataFormats.StringFormat);

						// If the string can be converted into a Brush, convert it.
						if (converter.IsValid(dataString)) {
							Brush newFill = (Brush)converter.ConvertFromString(dataString);
							ellipse.Fill = newFill;
						}
					}
					//} else {
					//	dbMsg += ",ellipse = null";
					//}
				} else {
					dbMsg += ",Drag終了";
					e.Effects = DragDropEffects.None;
				}

				//if (!dataString.Equals("") && ellipse != null) {
				//	dbMsg += ",dataString=" + dataString;
				//	// If the string can be converted into a Brush, convert it.
				//	if (converter.IsValid(dataString)) {
				//		Brush newFill = (Brush)converter.ConvertFromString(dataString);
				//		ellipse.Fill = newFill;
				//	}
				//}
				e.Handled = true;
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
						//			IsDragging = true;    だけでは表示されない
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

					Ellipse? ellipse = sender as Ellipse;        //楕円？
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


		private ListSortDirection? _isDescending = ListSortDirection.Descending;
		/// <summary>
		/// ソート機能があるDataGridのHeaderをクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayList_Sorting(object sender, DataGridSortingEventArgs e) {
			string TAG = "PlayList_Sorting";
			string dbMsg = "";
			try {
				// 既存のソート処理が行われなくなる。これをしないとカスタムソートが効かない。
				e.Handled = true;
				DataGrid DG = (DataGrid)sender;
				int ColCount = DG.Columns.Count();
				// クリックされたヘッダー
				DataGridColumn Cols = e.Column;

				ListSortDirection? isDescending = Cols.SortDirection;
				//isDescending = (Cols.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;

				int selCol = 0;
				for (selCol = 0; selCol< ColCount; selCol++) {
					dbMsg += "[" + selCol + "/" + ColCount + "]" + DG.Columns[selCol].Header;
					if (DG.Columns[selCol].Header.Equals(Cols.Header)) {
						isDescending = DG.Columns[selCol].SortDirection;
						break;
					}
				}
				dbMsg += ",index= "+ selCol + " を選択";
				dbMsg += ",sort= " + isDescending;
				if (isDescending == null) {
					if (_isDescending == ListSortDirection.Descending) {
						isDescending = ListSortDirection.Ascending;
					} else {
						isDescending = ListSortDirection.Descending;
					}
					dbMsg += ">> " + isDescending;
				}
				_isDescending = isDescending;

				VM.PlayListSort(selCol, (string)Cols.Header, isDescending);
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}

		}


		//public void PlayList2Explore(string pathStr) {
		//	string TAG = "PlayList2Explore";
		//	string dbMsg = "";
		//	try {
		//		dbMsg += ">>" + pathStr;


		//		//https://dobon.net/vb/dotnet/process/openfile.html
		//		//Processオブジェクトを作成する
		//		System.Diagnostics.Process pEXPLORER = new System.Diagnostics.Process();
		//		//起動するファイルを指定する
		//		pEXPLORER.StartInfo.FileName = "EXPLORER.exe";
		//		pEXPLORER.StartInfo.Arguments = pathStr;
		//		//イベントハンドラがフォームを作成したスレッドで実行されるようにする
		////pEXPLORER.SynchronizingObject = (ISynchronizeInvoke?)this;
		//		//Unable to cast object of type 'M3UPlayer.Views.MainWindow' to type 'System.ComponentModel.ISynchronizeInvoke'.
		//		//イベントハンドラの追加
		//		pEXPLORER.Disposed += new EventHandler(EXPLORER_Exited);
		//		//プロセスが終了したときに Exited イベントを発生させる
		//		pEXPLORER.EnableRaisingEvents = true;
		//		//起動する
		//		pEXPLORER.Start();
		//		//pEXPLORER = Process.Start("EXPLORER.EXE", pathStr);
		//		dbMsg += ",pEXPLORER[" + pEXPLORER.Id + "]start" + pEXPLORER.StartTime.ToString("HH:mm:ss.fff") + ",Arguments=" + pEXPLORER.StartInfo.Arguments;
		//		pEXPLORER.WaitForExit();
		//		dbMsg += ">>Exit" + pEXPLORER.ExitTime.ToString("HH:mm:ss.fff") + ",Arguments=" + pEXPLORER.StartInfo.Arguments;
		//		dbMsg += ">>ExitCode=" + pEXPLORER.ExitCode.ToString();
		//		pEXPLORER.Kill();
		//		MyLog(TAG, dbMsg);
		//	} catch (Exception er) {
		//		MyErrorLog(TAG, dbMsg, er);
		//	}
		//}

		//private void EXPLORER_Exited(object sender, EventArgs e) {
		//	string TAG = "EXPLORER_Exited";
		//	string dbMsg = "";
		//	try {
		//		System.Diagnostics.Process process = (System.Diagnostics.Process)sender;
		//		dbMsg += ">>Exit" + process.ExitTime.ToString("HH:mm:ss.fff") + ",Arguments=" + process.StartInfo.Arguments;
		//		dbMsg += ">>ExitCode=" + process.ExitCode.ToString();

		//		MyLog(TAG, dbMsg);
		//	} catch (Exception er) {
		//		MyErrorLog(TAG, dbMsg, er);
		//	}
		//}






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
				//slider.ToolTip = VM.GetHMS(newValue.ToString());
				//dbMsg += ">>" + slider.ToolTip;
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(TAG, dbMsg);
			}
		}

		/// <summary>
		/// 再生ポジションスライダーのクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PositionSL_MouseUp(object sender, MouseButtonEventArgs e) {
			string TAG = "positionsl_mouseup";
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

		private void Window_KeyUp(object sender, KeyEventArgs e) {
			string TAG = "Window_KeyUp";
			string dbMsg = "";
			try {
				Key targetKey = e.Key;
				//dbMsg = "Key" + e.Key;
				if (targetKey == Key.Return) {
					//		PlayBt.Focus();だとクリックが二重に発生する
					// 他のエレメントにフォーカスが当たっているとそのアイテムのクリック＝returnになる
					//WPFでボタンクリックを発生させる////////////////////////////
					//if (PlayBt == null)
					//	throw new ArgumentNullException("PlayBt");

					//var provider = new ButtonAutomationPeer(PlayBt) as IInvokeProvider;
					//provider.Invoke();
					//return;
				} else if (targetKey == Key.Left) {
					ForwardBT.Focus();
				} else if (targetKey == Key.Right) {
					RewBt.Focus();
				} else if (targetKey == Key.Up
						|| targetKey == Key.Down
						|| targetKey == Key.Delete
					) {
					PlayList.Focus();
				} else if (targetKey == Key.PageUp
						|| targetKey == Key.PageDown
					) {
					PLCombo.Focus();
				}
				//MyLog(TAG, dbMsg);
				VM.WindowKeyUp(targetKey);
				//動作後は一番問題ないところへフォーカスを逃がす
				PlayList.Focus();
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
