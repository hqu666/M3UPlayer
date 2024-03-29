﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Text;
using System.Windows.Threading;
using System.Threading;
using M3UPlayer.Views;
using System.IO;
//using System.Windows;だとDragDropEffectsでエラー発生
using System.Runtime.InteropServices;

///FileOpenDialogのカスタマイズ//////////////////////////////////////////////////////////////////////
//WMP//////////////////////////////////////
using ListBox = System.Windows.Controls.ListBox;
//using Point = System.Drawing.Point;
using Path = System.IO.Path;
//using WMPLib;
//using System.Windows.Controls.MediaElement;
using M3UPlayer.Models;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text.RegularExpressions;
using System.Collections;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using AxShockwaveFlashObjects;
using ShockwaveFlashObjects;
using AxWMPLib;
using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;

namespace M3UPlayer.ViewModels {
    public class MainViewModel : INotifyPropertyChanged {
        public Views.MainWindow MyView { get; set; }
        /// <summary>
        /// WindowsMediaPlayerのコントローラ
        /// </summary>
        //public PlayerWFCL.WMPControl axWmp;

        private ObservableCollection<PlayListModel> _PLList;
        /// <summary>
        /// プレイリストのソース
        /// </summary>
        public ObservableCollection<PlayListModel> PLList {
            get => _PLList;
            set {
                string TAG = "PLList(set)";
                string dbMsg = "";
                try {
                    if (_PLList == value)
                        return;
                    _PLList = value;
                    dbMsg += PLList.Count + "件 ";
                    if (0 < PLList.Count) {
                        dbMsg += PLList[0];
                        dbMsg += "～" + PLList[PLList.Count - 1];
                    }
                    MyLog(TAG, dbMsg);
                    RaisePropertyChanged("PLList");
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }
        public PlayListModel PLListSelectedItem { get; set; }

        /// <summary>
        /// 再生中のアイテム
        /// </summary>
        public SelectionModel NowSelect { get; set; }
        /// <summary>
        /// 前の再生アイテム
        /// </summary>
        public SelectionModel BeforSelect { get; set; }

        /// <summary>
        /// 他のリストへの移動/コピー前のアイテムの配置
        /// </summary>
        public SelectionModel MoveBeforSelect { get; set; }
        public string BeforSelectListUrl;

        /// <summary>
        /// 選択されているプレイリストアイテムの配列:SelectedItemsは読み取り専用でBindingできない
        /// SelectionUnit が　FullRow;行単位,CellOrRowHeader:セル単位だが、行で選択も可能,Cell;セル単位
        /// </summary>
        public List<PlayListModel> SelectedPlayListFiles { get; set; }

        /// <summary>
        /// 複数行選択ならExtended、単行選択ならSingle
        /// </summary>
        public string PlayListSelectionMode { get; set; }

        // 一つの行が選択された時点
        private int _SelectedPlayListIndex;
        /// <summary>
        /// プレイリスト上で 一つの行が選択された時点で発生するアイテムのインデックス。
        /// 選択しているアイテム配列も作る
        /// </summary>
        public int SelectedPlayListIndex {
            get => _SelectedPlayListIndex;
            set {
                string TAG = "SelectedPlayListIndex(set)";
                string dbMsg = "";
                try {
                    if (_SelectedPlayListIndex == value || value < 0)
                        return;
                    _SelectedPlayListIndex = value;
                    RaisePropertyChanged("SelectedPlayListIndex");
                    //dbMsg += "[" + SelectedPlayListIndex + "]" + MyView.PlayList.CurrentItem + "Drag_now=" + Drag_now;
                    //// 選択されているものがあって、Dragで無ければ 
                    //if (MyView.PlayList.SelectedItems != null && !Drag_now) {
                    //    SelectedPlayListFiles = new List<PlayListModel>();
                    //    IList selectedItems = MyView.PlayList.SelectedItems;
                    //    dbMsg += "、selectedItems=" + selectedItems.Count + "件";
                    //    if (selectedItems.Count == 1) {
                    //        PlayListModel? PLItem = new PlayListModel();      //直接代入でクラッシュしたのでローカル変数に取得
                    //        PLItem = (PlayListModel)PLList[SelectedPlayListIndex];
                    //        SelectedPlayListFiles.Add(PLItem);
                    //    } else {
                    //        foreach (object pli in selectedItems) {
                    //            PlayListModel PLItem = new PlayListModel();      //直接代入でクラッシュしたのでローカル変数に取得
                    //            PLItem = (PlayListModel)pli;
                    //            SelectedPlayListFiles.Add(PLItem);
                    //            dbMsg += "\r\n[" + SelectedPlayListFiles.Count + "]" + SelectedPlayListFiles[SelectedPlayListFiles.Count - 1].Summary;              //PLItem.UrlStr;

                    //        }
                    //        for (int i = 0; i < (MyView.PlayList.Items.Count - 1); ++i) {
                    //            PlayListModel oneItem = SelectedPlayListFiles[i];
                    //            dbMsg += "\r\n[" + i + "]" + oneItem.UrlStr;
                    //        }
                    //    }
                    //}

                    //foreach (PlayListModel pli in MyView.PlayList.Items) {

                    //}
                    MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }

        /// <summary>
        /// 最後に選択したフォルダ
        /// </summary>
        public string NowSelectedPath {
            get { return Properties.Settings.Default.NowSelectedPath; }
            set { Properties.Settings.Default.NowSelectedPath = value; }
        }
        /// <summary>
        /// 最後に選択したファイル
        /// </summary>
        public string NowSelectedFile {
            get { return Properties.Settings.Default.NowSelectedFile; }
            set { Properties.Settings.Default.NowSelectedFile = value; }
        }

        private bool _IsHideControl;
        /// <summary>
        /// プレイヤーコントロールの表示/非表示
        /// </summary>
        public bool IsHideControl {
            get => _IsHideControl;
            set {
                string TAG = "IsHideControl(set)";
                string dbMsg = "";
                try {
                    dbMsg += "value=" + value;
                    if (_IsHideControl == value)
                        return;
                    _IsHideControl = value;
                    RaisePropertyChanged("IsHideControl");
                    dbMsg += ">>IsHideControl=" + IsHideControl;
                    dbMsg += ",PlayerContlolGR(h)=" + MyView.PlayerContlolGR.Height;
                    if (IsHideControl) {
                        MyView.PlayerContlolGR.Height = 0;
                        MyView.ControlHideBT.Content = "⇗";
                    } else {
                        MyView.PlayerContlolGR.Height = 96;
                        MyView.ControlHideBT.Content = "⇙";
                    }
                    dbMsg += ">>=" + MyView.PlayerContlolGR.Height;
                    MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }

        /// <summary>
        /// 全長:duration
        /// </summary>
        private string _DurationStr;
        /// <summary>
        /// 全長
        /// </summary>
        public string DurationStr {
            get => _DurationStr;
            set {
                string TAG = "DurationStr(set)";
                string dbMsg = "";
                try {
                    dbMsg += "value=" + value;
                    if (_DurationStr == value)
                        return;
                    _DurationStr = value;
                    RaisePropertyChanged("DurationStr");
                    //             MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }

            }
        }

        /// <summary>
        /// 再生ポジションスライダーの最大値；Durationの数値
        /// </summary>
        private double _SliderMaximum;
        /// <summary>
        /// スライダー上限
        /// </summary>
        public double SliderMaximum {
            get => _SliderMaximum;
            set {
                string TAG = "SliderMaximum(set)";
                string dbMsg = "";
                try {
                    dbMsg += "value=" + value;
                    if (_SliderMaximum == value)
                        return;
                    _SliderMaximum = value;
                    RaisePropertyChanged("SliderMaximum");
                    MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }

        private string _PositionStr;
        /// <summary>
        /// 再生ポジション：currentTime
        /// </summary>
        public string PositionStr {
            //get { return GetDataBindItem<string>("Title").Value; }
            //private set { GetDataBindItem<string>("Title").Value = value; }
            get => _PositionStr;
            set {
                if (_PositionStr == value)
                    return;
                _PositionStr = value;
                RaisePropertyChanged("PositionStr");
            }
        }

        private string _infoStr = "再生中のファイルに情報はありません";
        /// <summary>
        /// タイトルなどの表示
        /// </summary>
        public string infoStr {
            //get { return GetDataBindItem<string>("Title").Value; }
            //private set { GetDataBindItem<string>("Title").Value = value; }
            get => _infoStr;
            set {
                if (_infoStr == value)
                    return;
                if (value.Equals("") || value == null) {
                    _infoStr = "再生中のファイルに情報はありません";
                } else {
                    _infoStr = value;
                }
                RaisePropertyChanged("infoStr");
            }
        }

        /// <summary>
        /// ポジションスライダーをドラッグ中
        /// PositionSL_DragStartedでtrue/PositionSL_MouseUpでfalse
        /// </summary>
        public bool IsPositionSLDraging = false;

        private double _SliderValue;
        /// <summary>
        /// スライダー位置 
        /// </summary>
        public double SliderValue {
            get => _SliderValue;
            set {
                string TAG = "SliderValue(set)";
                string dbMsg = "";
                try {
                    dbMsg += "value=" + value;
                    if (_SliderValue == value)
                        return;
                    _SliderValue = value;
                    RaisePropertyChanged("SliderValue");
                    //                  MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }

        /// <summary>
        /// スライダーのToolTip文字
        /// </summary>
        public string PositionSLTTText { get; set; }

        private bool _IsSendAuto;
        /// <summary>
        /// 自動送り 
        /// </summary>
        public bool IsSendAuto {
            get => _IsSendAuto;
            set {
                string TAG = "IsSendAuto(set)";
                string dbMsg = "";
                try {
                    dbMsg += "value=" + value;
                    if (_IsSendAuto == value)
                        return;
                    _IsSendAuto = value;
                    RaisePropertyChanged("IsSendAuto");
                    Properties.Settings.Default.IsSendAuto = value;
                    //                  MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }


        public BitmapImage playImage;
        public BitmapImage pouseImage;
        public BitmapImage MuteOnImage;
        public BitmapImage MuteOffImage;

        public bool toWeb = true;  // false;
        public System.Windows.Forms.Integration.WindowsFormsHost host;
        /// <summary>
        /// WindowsMwdiaPlayer
        /// </summary>
        public AxWindowsMediaPlayer? axWmp;
        //   public ShockwaveFlashObjects.FlashObject flash;
        public AxShockwaveFlash? flash;

        private WebView2 webView2;
        /// <summary>
        /// 0:web,1:WMP,2;Flash をPlayListToPlayerで設定
        /// </summary>
        public int movieType = 0;
        public string[] videoFiles = new string[] {  ".mp4",".flv",".f4v",".webm",  ".ogv",".3gp",  ".rm",  ".asf",   ".swf",
                                              "mpa",".mpe",".webm",  ".ogv",".3gp",  ".3g2",  ".asf",  ".asx",
                                                ".dvr-ms",".ivf",".wax",".wmv", ".wvx",  ".wm",  ".wmx",  ".wmz",
                                             };
        public string[] WebVideo = new string[] { ".webm", ".3gp", ".rm", ".dvr-ms", ".ivf" };
        public string[] WMPFiles = new string[] {  ".mp4",  ".asf","mpa",".mpe",".3gp",  ".3g2",  ".asf",  ".asx",
                                                ".ivf",".wax",".wmv", ".wvx",  ".wm",  ".wmx",  ".wmz" };
        public string[] FlashVideo = new string[] { ".flv", ".f4v", ".swf" };
        private bool _IsPlaying;
        /// <summary>
        /// 再生中
        /// </summary>
        public bool IsPlaying {
            get => _IsPlaying;
            set {
                string TAG = "IsPlaying(set)";
                string dbMsg = "";
                try {
                    //        dbMsg += "value=" + value;
                    dbMsg += ">>IsPlaying=" + _IsPlaying;
                    if (_IsPlaying == true) { 
                        _IsPlaying = false;
					} else if (_IsPlaying == false) {
                        _IsPlaying = true;
                    }
                    dbMsg += ">>" + _IsPlaying;
					if (MyView != null) {
                        if (_IsPlaying) {
                            switch (movieType) {
                                case 0:
                                    if (MyView.webView != null) {
                                        MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").play();");
                                    }
                                    break;
                                case 1:
                                    if (axWmp != null) {
                                        axWmp.Ctlcontrols.play();
                                    }
                                    break;
                                case 2:
                                    if (flash != null) {
                                       // flash.Playing=true;
                                        //             flash.SetVariable("Playing", "true");
                                        //		flash.Play();
                                    }
                                    break;
                            }
                            MyView.PlayBtImage.Source = pouseImage;
                        } else {
                            switch (movieType) {
                                case 0:
                                    if (MyView.webView != null) {
                                        MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").pause();");
                                    }
                                    break;
                                case 1:
                                    if (axWmp != null) {
                                        axWmp.Ctlcontrols.pause();
                                    }
                                    break;
                                case 2:
                                    if (flash != null) {
                                      //  flash.Playing = false;
                                        //flash.SetVariable("Playing", "false");
                                      //  flash.Stop();
									}
                                    break;
                            }
                            MyView.PlayBtImage.Source = playImage;
                        }
                    }
                    //              RaisePropertyChanged("IsPlaying");
                    //RaisePropertyChanged("PlayBtImageSource");
                    //         dbMsg += ">>PlayBtImageSource==" + PlayBtImageSource.ToString();
                    MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }

        private double _SoundValue;
        /// <summary>
        /// 音量
        /// </summary>
        public double SoundValue {
            get => _SoundValue;
            set {
                string TAG = "SoundValue(set)";
                string dbMsg = "";
                try {
                    dbMsg += "value=" + value;
                    if (_SoundValue == value)
                        return;
                    _SoundValue = value;

                    //if (axWmp != null) {
                    //	axWmp.SetVolume(value);
                    //}
                    RaisePropertyChanged("SoundValue");
                    //              MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }

        private bool _IsMute;
        /// <summary>
        /// 消音
        /// </summary>
        public bool IsMute {
            get => _IsMute;
            set {
                string TAG = "IsMute(set)";
                string dbMsg = "";
                try {
                    dbMsg += "value=" + value;
                    if (_IsMute == value)
                        return;
                    _IsMute = value;
                    RaisePropertyChanged("IsMute");
                    dbMsg += ">>IsMute=" + IsMute;
                    if (IsMute) {
                        switch (movieType) {
                            case 0:
                                MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").volume=" + 0 + ";");
                                break;
                            case 1:
                                if (axWmp != null) {
                                    axWmp.settings.volume = 0;
                                }
                                break;
                            case 2:
                                if (flash != null) {
                                    string flashVvars = "mute=true";
                                    flash.FlashVars = flashVvars;
                                }
                                break;
                        }
                        MyView.MuteBtImage.Source = MuteOnImage;        // new BitmapImage(new Uri("/views/ei_silence.png", UriKind.Relative));
                    } else {
                        dbMsg += ",SoundValue=" + SoundValue;
                        switch (movieType) {
                            case 0:
                                MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").volume=" + SoundValue + ";");
                                break;
                            case 1:
                                if (axWmp != null) {
                                    axWmp.settings.volume = (int)Math.Round(SoundValue * 100);
                                }
                                break;
                            case 2:
                                if (flash != null) {
                                    string flashVvars = "mute=false";
                                    //             string flashVvars = @"vol=" + SoundValue + '"';
                                    flash.FlashVars = flashVvars;
                                }
                                break;
                        }
						//      MyView.MuteBtImage.Source = new BitmapImage(new Uri("pack://application:,,,/views/ei-sound.png", UriKind.Relative));
						MyView.MuteBtImage.Source = MuteOffImage;
					}
                    MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }

        /// <summary>
        /// 送りコンボのソース
        /// </summary>
        public List<string> ForwardCBComboSource { get; set; }
        /// <summary>
        /// 送りコンボの選択値
        /// </summary>
        public string ForwardCBComboSelected { get; set; }

        /// <summary>
        /// 戻しコンボのソース
        /// </summary>
        public List<string> RewCBComboSource { get; set; }
        /// <summary>
        /// 戻しコンボの選択値
        /// </summary>
        public string RewCBComboSelected { get; set; }


        ///// <summary>
        ///// 選択されている
        ///// </summary>
        //public string PlsyListFileURL;
        public string ComboLastItemKey = "AddNew";
        public string ComboLastItemVal = "新規リスト";
        private int _ListItemCount;
        /// <summary>
        /// プレイリストの登録件数。
        /// 0で無ければコンテキストメニューも各アイテムを有効にする。
        /// </summary>
		public int ListItemCount {
            get { return _ListItemCount; }
            set {
                if (_ListItemCount == value)
                    return;
                _ListItemCount = value;
                if (0 < value) {
                    PlayListItemViewExplore.IsEnabled = true;
                    PlayListItemMove.IsEnabled = true;
                    PlayListDeleteCannotRead.IsEnabled = true;
                    PlayListDeleteDoubling.IsEnabled = true;
                    PlayListItemRemove.IsEnabled = true;
                    PlayListSaveRoot.IsEnabled = true;
                } else {
                    PlayListItemViewExplore.IsEnabled = false;
                    PlayListItemMove.IsEnabled = false;
                    PlayListDeleteCannotRead.IsEnabled = false;
                    PlayListDeleteDoubling.IsEnabled = false;
                    PlayListItemRemove.IsEnabled = false;
                }
            }
        }
        #region 設定ファイルの項目
        public string[] PlayLists;
        /// <summary>
        /// 登録されたプレイリストファイルのリスト文字列
        /// </summary>
        public string PlayListStr {
            get { return Properties.Settings.Default.PlayListStr; }
            set { Properties.Settings.Default.PlayListStr = value; }
        }
        /// <summary>
        /// 選択されているプレイリストファイルのURL
        /// 常にSettingsから読み出し、更新される
        /// </summary>
        public string CurrentPlayListFileName {
            get { return Properties.Settings.Default.CurrentPlayListFileName; }
            set {
                if (value.Contains(".m3u")) {
                    Properties.Settings.Default.CurrentPlayListFileName = value;
                }
            }
        }

        private string _PlayListComboSelected;
        /// <summary>
        /// 選択jされたプレイリストファイル
        /// </summary>
        public string PlayListComboSelected {
            get => _PlayListComboSelected;
            set {
                if (_PlayListComboSelected == value)
                    return;
                _PlayListComboSelected = value;
                RaisePropertyChanged("PlayListComboSelected");
            }
        }
        private Uri _TargetURI;
        /// <summary>
        /// webViewのSource
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
                    NotifyPropertyChanged("TargetURI");
                    MyLog(TAG, dbMsg);
                } catch (Exception er) {
                    MyErrorLog(TAG, dbMsg, er);
                }
            }
        }

        /// <summary>
        /// 最後に再生したメディアファイルの再生ポジション
        /// </summary>
        public TimeSpan NowSelectedPosition {
            get { return Properties.Settings.Default.NowSelectedPosition; }
            set { Properties.Settings.Default.NowSelectedPosition = value; }
        }


        /// <summary>
        /// フルスクリーンで起動するか
        /// </summary>
        public bool IsFullScreen {
            get { return Properties.Settings.Default.IsFullScreen; }
            set { Properties.Settings.Default.IsFullScreen = value; }
        }


        public double FreamWidth { get; set; }
        public double FreamHeigh { get; set; }


        //追加待ち///////////////////////////////////////////////////////////////////////////////////////////////////////////
        public int summaryCol = 2;
        #endregion

        public string FrameSource { get; set; }
        public object FrameDataContext { get; set; }
        /// <summary>
        /// EXPLORERのプロセス
        /// </summary>
        public System.Diagnostics.Process pEXPLORER;
        public double VWidth = 960;
        public double VHeight = 540;
        private ProgressDialog pd;
        /// <summary>
        /// プログレスのキャンセル
        /// </summary>
        private CancellationTokenSource cancelToken;
        /// <summary>
        /// プログレスのVM
        /// </summary>
        private ProgressDialogViewModel PDVM;
        private int DelCount = 0;
        private string progTitol;
        /// <summary>
        /// メイン画面
        /// </summary>
        public MainViewModel() {
            Initialize();
        }


        /// <summary>
        /// 起動メソッド
        /// </summary>
        public void Initialize() {
            string TAG = "Initialize";
            string dbMsg = "";
            try {
                cancelToken = new CancellationTokenSource();
                // プログレスダイアログのコンテキストになるVMはここで作って渡す。
                PDVM = new ProgressDialogViewModel();
                MakePlayListMenu();
                PlayListItemViewExplore.IsEnabled = false;
                PlayListItemMove.IsEnabled = false;
                PlayListDeleteCannotRead.IsEnabled = false;     //20220519null発生
                PlayListDeleteDoubling.IsEnabled = false;
                PLList = new ObservableCollection<PlayListModel>();
                pouseImage = new BitmapImage(new Uri("/views/pousebtn.png", UriKind.Relative));
                playImage = new BitmapImage(new Uri("/views/pl_r_btn.png", UriKind.Relative));
                MuteOnImage = new BitmapImage(new Uri("/views/ei_silence.png", UriKind.Relative));//ei_silence
                MuteOffImage = new BitmapImage(new Uri("/views/ei-sound.png", UriKind.Relative));
                assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;  //実行デレクトリ		+Path.AltDirectorySeparatorChar + "brows.htm";
                dbMsg += ",assemblyPath=" + assemblyPath;
                dbMsg += ",CurrentPlayListFileName=" + CurrentPlayListFileName;
                PLComboSource = new Dictionary<string, string>();
                dbMsg += "\r\nPlayListStr=" + PlayListStr;
				if (0<CurrentPlayListFileName.Length && PlayListStr == null) {
                    Properties.Settings.Default.PlayListStr = CurrentPlayListFileName;
                    Properties.Settings.Default.Save();
                    dbMsg += ">>" + PlayListStr;
                }
                AddPlayListCombo("");
                MakePlayListComboMenu();
                dbMsg += "[" + VWidth + "×" + VHeight + "]";
                if (CurrentPlayListFileName.Contains(".M3u")) {
                    //PlayListsからCurrentPlayListFileNameのインデックスを取得
                    int listIndex = Array.IndexOf(PlayLists, CurrentPlayListFileName);
                    PLComboSelectedIndex = listIndex;
                }
                if (PLComboSelectedIndex == 0) {
                    ListUpFiles(CurrentPlayListFileName);
                }
                dbMsg += " [" + PLComboSelectedIndex + "]" + CurrentPlayListFileName;
                dbMsg += ",NowSelectedFile=" + NowSelectedFile;
                dbMsg += " [" + NowSelectedPosition + "]";
                PlayListSaveRoot.IsEnabled = false;
                RaisePropertyChanged("PlayListSaveBTVisble");
                PlayListSelectionMode = "Extended";
                RaisePropertyChanged("PlayListSelectionMode");
                Drag_now = false;
                RaisePropertyChanged("Drag_now");

                ForwardCBComboSource = new List<String> { "5", "30", "60", "120", "300", "600" };
                RaisePropertyChanged("ForwardCBComboSource");
                dbMsg += ",ff=" + ForwardCBComboSource[0] + "～" + ForwardCBComboSource[ForwardCBComboSource.Count - 1];
                ForwardCBComboSelected = Constant.ForwardCBComboSelected;
                RaisePropertyChanged("ForwardCBComboSelected");
                dbMsg += ",ForwardCBComboSelected=" + ForwardCBComboSelected;

                RewCBComboSource = new List<String> { "5", "30", "60", "120", "300", "600" };
                RaisePropertyChanged("RewCBComboSource");
                dbMsg += ",ff=" + RewCBComboSource[0] + "～" + RewCBComboSource[RewCBComboSource.Count - 1];
                RewCBComboSelected = Constant.RewCBComboSelected;
                RaisePropertyChanged("RewCBComboSelected");
                dbMsg += ",RewCBComboSelected=" + RewCBComboSelected;

                IsPlaying = false;
                RaisePropertyChanged("IsPlaying");
                SoundValue = Constant.SoundValue;
                RaisePropertyChanged("SoundValue");
                IsMute = false;
                //      WVM = new WsbViewModel();

                SoundValue = Properties.Settings.Default.SoundValue;
                ForwardCBComboSelected = Properties.Settings.Default.ForwardCBComboSelected;
                RewCBComboSelected = Properties.Settings.Default.RewCBComboSelected;

                flash = new AxShockwaveFlash();
                //_NowSelect = new SelectionModel();
                //_BeforSelect = new SelectionModel();
                NowSelect = new SelectionModel();
                BeforSelect = new SelectionModel();
                IsSendAuto=Properties.Settings.Default.IsSendAuto;
                dbMsg += ",IsSendAuto=" + IsSendAuto;

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        /// <summary>
        /// 終了前イベントのメソッド
        /// このVMで管理しているSettings項目の保存
        /// </summary>
        public void BeforeClose() {
            string TAG = "BeforeClose";
            string dbMsg = "";
            try {
                if (_timer != null) {
                    _timer.Stop();
                    _timer = null;  
                }
                if (PlayListSaveBTVisble != null) {
                if (PlayListSaveBTVisble.Equals("Visible")) {
                    IsDoSavePlayList(false);
                }
                }
                Properties.Settings.Default.SoundValue = SoundValue;
                Properties.Settings.Default.ForwardCBComboSelected = ForwardCBComboSelected;
                Properties.Settings.Default.RewCBComboSelected = RewCBComboSelected;
                Properties.Settings.Default.Save();

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// webVeiw2の初期設定
        /// </summary>
		public void CallWeb() {
            string TAG = "CallWeb";
            string dbMsg = "";
            try {
                if (MyView.webView.CoreWebView2 != null) {
                    //JavaScriptからC#のメソッドが実行できる様に仕込む
                    MyView.webView.CoreWebView2.AddHostObjectToScript("class", CsClass);
                    dbMsg += ",class,CsClass追加";
                    //ダミーで良ければ　CS_Util　に差し替え
                } else {
                    dbMsg += ",CoreWebView2==null";
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// テキストファイルをStreamReaderで読み込む
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="emCord"></param>
        /// <returns></returns>
        /// テキスト系ファイルの読込み	http://www.atmarkit.co.jp/ait/articles/0306/13/news003.html
        private string ReadTextFile(string fileName, string emCord) {
            string TAG = "[ReadTextFile]";
            string dbMsg = TAG;
            string retStr = "";
            try {
                dbMsg += ",fileName=" + fileName + ",emCord=" + emCord;
                StreamReader sr;
                if (emCord != null) {               //エンコードが省略されるとUTF-8で読み込まれる
                    sr = new StreamReader(fileName);
                } else {
                    sr = new StreamReader(fileName, Encoding.GetEncoding(emCord));
                }
                retStr = sr.ReadToEnd();                                                        //内容をすべて読み込む
                sr.Close();                                                                     //閉じる☆必須
            } catch (Exception e) {
                Console.WriteLine(TAG + "でエラー発生" + e.Message + ";" + dbMsg);
            }
            MyLog(TAG, dbMsg);
            return retStr;
        }

        private string _FileURL = "";
        /// <summary>
        /// 指定されたプレイリストの内容を読み込み、DataGridにBaindinｇする
        /// </summary>
        /// <param name="FileURL"></param>
        private void ListUpFiles(string FileURL) {
            string TAG = "ListUpFiles";
            string dbMsg = "";
            try {
                dbMsg += "、FileURL=" + FileURL;
                if (_FileURL.Equals(FileURL)) {
                    dbMsg += "重複";
                    MyLog(TAG, dbMsg);
                    return;

                }
                string rText = ReadTextFile(FileURL, "UTF-8"); //"Shift_JIS"では文字化け発生
                string[] delimiter = { "\r\n" };
                string[] Strs = rText.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                PLList = new ObservableCollection<PlayListModel>();
                var CompareList = new List<string>();
                CompareList.AddRange(videoFiles);
                // 複数スレッドで使用されるコレクションへの参加
                BindingOperations.EnableCollectionSynchronization(PLList, new object());
                //CancellationTokenSource cancelToken = new CancellationTokenSource();
                //// コンテキストになるVMはここで作って渡す。
                //ProgressDialogViewModel PDVM = new ProgressDialogViewModel();
                pd = new ProgressDialog(PDVM, async () => {
                    PDVM.IntProgress(FileURL + "をプレイリストに書き込みます。", Strs.Count(), 1);
                    dbMsg += "," + PDVM.PrgTitle;
                    dbMsg += ",PrgMax" + PDVM.PrgMax;
                    int loopCount = 0;
                    foreach (string item in Strs) {
                        //拡張部分を破棄してURLを読み出す
                        string[] items = item.Split(',');
                        string url = items[0];
                        bool IsWright = false;
                        foreach (string CompareStr in CompareList) {
                            if (url.Contains(CompareStr)) {
                                IsWright = true;
                            }
                        }
                        if (IsWright) {
                            PlayListModel playListModel = MakeOneItem(url);
                            if (playListModel.UrlStr != null) {
                                if (-1 < Array.IndexOf(WMPFiles, playListModel.extentionStr)
                                    || -1 < Array.IndexOf(FlashVideo, playListModel.extentionStr)
                                    || -1 < Array.IndexOf(WebVideo, playListModel.extentionStr)
                                    ) {
                                    PLList.Add(playListModel);
                                }

                                //PDVM.PrgStatus = playListModel.Summary + "";
                                //PDVM.PrgVal = PLList.Count();
                                PDVM.DoProgress(PLList.Count(), playListModel.Summary + "");
                                dbMsg += "\r\n[" + PDVM.PrgVal + "/" + PDVM.PrgMax + "]" + PDVM.PrgStatus;
                                loopCount++;
                            }
                        }
                    }
                }, cancelToken);

                pd.ShowDialog();
                if (pd.IsCanceled) {
                    MessageBox.Show("キャンセルしました", "Info", MessageBoxButton.OK);
                } else {
                    dbMsg += ",完了しました";
                }
                RaisePropertyChanged("PLList");
                ListItemCount = PLList.Count();
                RaisePropertyChanged("ListItemCount");
                dbMsg += "\r\n" + ListItemCount + "件";
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }

        }

        /// <summary>
        /// 動画ファイルのURLからプレイリスト1行分を作成
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private PlayListModel MakeOneItem(string item) {
            string TAG = "MakeOneItem";
            string dbMsg = "";
            PlayListModel playListModel = new PlayListModel();
            try {
                dbMsg += "、FileURL=" + item;
                string url = item;
                //拡張部分を破棄してURLを読み出す
                string[] items = item.Split(',');
                if (1 < items.Length) {
                    url = items[0];
                }
                playListModel.UrlStr = item;
                dbMsg += ">UrlStr=" + playListModel.UrlStr;
                dbMsg += ",fileName=" + playListModel.fileName;
                dbMsg += ",extention=" + playListModel.extentionStr;
                dbMsg += ",Summary=" + playListModel.Summary;
                dbMsg += ",ParentDir=" + playListModel.ParentDir;
                dbMsg += ",GranDir=" + playListModel.GranDir;
                dbMsg += ",extention=" + playListModel.extentionStr;

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
            return playListModel;
        }

        /// <summary>
        /// ファイル名配列からプレイリストに一連登録
        /// </summary>
        /// <param name="files">追加するファイル配列</param>
        /// <param name="InsertTo">挿入位置</param>
        /// ShowFolderDlogの時は
        public void FilesAdd(string[] files, int InsertTo) {
            string TAG = "FilesAdd";
            string dbMsg = "";
            try {
                dbMsg += ",files=" + files.Length + "件";
                dbMsg += ">PLList>" + PLList.Count + "件";
                int InsPosition = 0;
                foreach (string url in files) {
                    dbMsg += "\r\n" + url;
                    if (File.Exists(url)) {
                        PlayListModel playListModel = MakeOneItem(url);
                        if (playListModel.UrlStr != null) {
                            if (-1 < Array.IndexOf(WMPFiles, playListModel.extentionStr)
                                || -1 < Array.IndexOf(FlashVideo, playListModel.extentionStr)
                                || -1 < Array.IndexOf(WebVideo, playListModel.extentionStr)
                                ) {
                                if (InsertTo == -1) {
                                    PLList.Add(playListModel);
                                } else {
                                    PLList.Insert(InsPosition, playListModel);
                                    InsPosition++;
                                }
                            }
                        }
                    } else if (Directory.Exists(url)) {
                        //フォルダなら中身の全ファイルで再起する
                        string[] rfiles = System.IO.Directory.GetFiles(url, "*", SearchOption.AllDirectories);
                        FilesAdd(files, InsertTo);
                    }
                }
                RaisePropertyChanged("PLList");
                ListItemCount = PLList.Count();
                RaisePropertyChanged("ListItemCount");
                dbMsg += "\r\n" + ListItemCount + "件";
                //変更されたプレイリストを変更させる
                IsDoSavePlayList(true);
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// プレイリストに1ファイルづつ追加する。
        /// 0で先頭、-1で最後に追加
        /// </summary>
        /// <param name="FilsName"></param>
        /// <param name="InsertTo"></param>
        public bool AddToPlayList(string url, int InsertTo) {
            string TAG = "AddToPlayList";
            string dbMsg = "";
            bool retBool = false;
            try {

                string extention = System.IO.Path.GetExtension(NowSelectedFile);
                if (-1 < Array.IndexOf(videoFiles, extention)) {
                    //if (InsertTo == -1) {
                    //	InsertTo = ListItemCount;
                    //}
                    dbMsg += "[" + InsertTo + "/" + ListItemCount + "番目]" + url;
                    PlayListModel playListModel = MakeOneItem(url);
                    if (-1 < Array.IndexOf(WMPFiles, playListModel.extentionStr)
                        || -1 < Array.IndexOf(FlashVideo, playListModel.extentionStr)
                        || -1 < Array.IndexOf(WebVideo, playListModel.extentionStr)
                        ) {
                        if (InsertTo == -1) {
                            PLList.Add(playListModel);
                        } else {
                            PLList.Insert(InsertTo, playListModel);
                        }
                    }
                    RaisePropertyChanged("PLList");
                    ListItemCount = PLList.Count();
                    RaisePropertyChanged("ListItemCount");
                    dbMsg += "\r\n" + ListItemCount + "件";
                    retBool = true;
                    IsDoSavePlayList(false);
                } else {
                    dbMsg += ">>映像ではない";
                }

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
            return retBool;
        }


        #region プレイリスト選択コンボボックス
        /// <summary>
        /// プレイリスト選択コンボボックス
        /// </summary>
        public Dictionary<string, string> PLComboSource { get; set; }           //protectedにするとアウトブレークモードになる
        public IList<string> _pLComboSelectedItem;
        /// <summary>
        /// 選択されているプレイリストのアイテム
        /// </summary>
        public IList<string> PLComboSelectedItem {
            get => _pLComboSelectedItem;
            set {
                string TAG = "PLComboSelectedItem(set)";
                string dbMsg = "";
                dbMsg += "[" + value + "]";
                if (_pLComboSelectedItem == value || value == null)
                    return;
                _pLComboSelectedItem = value;
                RaisePropertyChanged("PLComboSelectedItem");
                CurrentPlayListFileName = value[0];
                dbMsg += CurrentPlayListFileName;
                Properties.Settings.Default.Save();
                if (CurrentPlayListFileName.Equals(ComboLastItemKey)) {
                    MakeNewPlayListFile();
                } else {
                    ListUpFiles(CurrentPlayListFileName);
                }
                MyLog(TAG, dbMsg);
            }
        }
        private int _plcomboselectedindex;
        /// <summary>
        /// 選択されているプレイリストのインデックス
        /// ここで選択中のプレイリスト名も更新され、Settingsに登録される
        /// </summary>
        public int PLComboSelectedIndex {
            get => _plcomboselectedindex;
            set {
                string TAG = "PLComboSelectedIndex(set)";
                string dbMsg = "";
                dbMsg += "[" + value + "]";
                if (_plcomboselectedindex == value || value < 0)
                    return;
                _plcomboselectedindex = value;
                RaisePropertyChanged("PLComboSelectedIndex");
                KeyValuePair<string, string>[] items = PLComboSource.ToArray();
                CurrentPlayListFileName = items[value].Key;
                dbMsg += CurrentPlayListFileName;
                Properties.Settings.Default.Save();
                if (CurrentPlayListFileName.Equals(ComboLastItemKey)) {
                    MakeNewPlayListFile();
                } else {
                    ListUpFiles(CurrentPlayListFileName);
                }
                MyLog(TAG, dbMsg);
            }
        }

        /// <summary>
        /// 設問してプレイリスト作成に入る
        /// </summary>
        public void PlayListInfo() {
            string TAG = "PlayListInfo";
            string dbMsg = "";
            try {
				string titolStr = "プレイリストコンボボックスの作成";
				string msgStr = "読み込めるプレイリストがありません\r\nプレイリストを作成しますか？";
				MessageBoxResult result = MessageShowWPF(titolStr, msgStr, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                dbMsg += ",result=" + result;
                if (result == MessageBoxResult.Yes) {
                    MakeNewPlayListFile();
                } else {
                    return;
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        /// <summary>
        /// PlayListComboBoxにアイテムを追加する
        /// 未登録リストは追加する。
        /// </summary>
        public void AddPlayListCombo(string AddFlieName) {
            string TAG = "AddPlayListCombo";
            string dbMsg = "";
            try {
                dbMsg += "PLComboSource=" + PLComboSource.Count() + "件";
                dbMsg += "、AddFlieName=" + AddFlieName;
                //登録済みのPlayリストと照合
                dbMsg += "、登録済み=" + PlayListStr;
				if (PlayListStr.Equals("") || PlayListStr == null) {
                    if (CurrentPlayListFileName!= null) {
                        Properties.Settings.Default.PlayListStr = CurrentPlayListFileName;
                        Properties.Settings.Default.Save();
					} else {
                        PlayListInfo();
                    }
                    PlayListStr = Properties.Settings.Default.PlayListStr;
                    dbMsg += ">>" + PlayListStr;
					////20220515:PlayListStr=nullでコケる
				} 
                //セパレータの入れ直し
                Regex reg = new Regex(".m3u");
                PlayListStr = reg.Replace(PlayListStr, ".m3u,");
                reg = new Regex(".m3u,8");
                PlayListStr = reg.Replace(PlayListStr, ".m3u8,");
                reg = new Regex(",,");
                PlayListStr = reg.Replace(PlayListStr, ",");
                PlayListStr = PlayListStr.Remove(PlayListStr.Length - 1);
                PlayLists = PlayListStr.Split(',');
                var list = new List<string>();
                list.AddRange(PlayLists);
                if (list.Count == 0 && AddFlieName.Equals("")) {
                    return;
                }
                //重複確認
                if (list.IndexOf(AddFlieName) < 0 && !AddFlieName.Equals("")) {
                    //無ければリスト先頭に追加
                    PlayListStr = AddFlieName + "," + PlayListStr;
                    CurrentPlayListFileName = AddFlieName;
                    //設定ファイル更新
                    //Properties.Settings.Default.Save();
                }
                PlayLists = PlayListStr.Split(',');
                //コンボボックスソースの更新
                PLComboSource = new Dictionary<string, string>();
                foreach (string item in PlayLists) {
                    if (!item.Equals("")) {
                        string DispName = System.IO.Path.GetFileName(item);
                        PLComboSource.Add(item, DispName);
                    }
                }
                //新規リスト
                PLComboSource.Add(ComboLastItemKey, ComboLastItemVal);
                RaisePropertyChanged("PLComboSource");
                int listIndex = Array.IndexOf(PlayLists, CurrentPlayListFileName);
                if (listIndex < 0) {
                    dbMsg += "リストに不在";
                    PLComboSelectedIndex = 0;       // PLComboSource.Count() - 1;
                } else {
                    PLComboSelectedIndex = listIndex;
                }
                RaisePropertyChanged("PLComboSelectedIndex");
                dbMsg += ">>" + PLComboSource.Count() + "件[" + PLComboSelectedIndex + "]" + CurrentPlayListFileName;
                PlayListComboSelected = CurrentPlayListFileName;
                RaisePropertyChanged("PlayListComboSelected");
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 指定した位置のコンボボックスアイテムを置き換える
        /// </summary>
        /// <param name="tIndex"></param>
        /// <param name="AddFlieName"></param>
        public void ReplacePlayListComboItem(int tIndex, string AddFlieName) {
            string TAG = "ReplacePlayListComboItem";
            string dbMsg = "";
            try {
                dbMsg += "[" + tIndex + "/" + PLComboSource.Count() + "を" + AddFlieName + "に置き換える";
                //登録済みのPlayリストと照合
                dbMsg += "、登録済み=" + PlayListStr;
                PlayLists = PlayListStr.Split(',');
                PlayListStr = "";
                for (int i = 0; i < PlayLists.Length; ++i) {
                    if (tIndex == i) {
                        PlayListStr += AddFlieName;
                    } else {
                        PlayListStr += PlayLists[i];
                    }
                }
                dbMsg += ">>" + PlayListStr;
                RaisePropertyChanged("PlayListStr");
                Properties.Settings.Default.PlayListStr = PlayListStr;
                Properties.Settings.Default.Save();
                AddPlayListCombo("");
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        #region プレイリストコンボのメニュー
        public ContextMenu PlayListComboItemMenu { get; set; }
        public MenuItem PlayListComboItemDelete;
        public MenuItem PlayListComboItemMove;
        public MenuItem PlayListComboItemMoveUp;
        public MenuItem PlayListComboItemMoveDown;
        public MenuItem BackBeforeItem;
        public MenuItem ComboMenuPlayListSave;
        public MenuItem ComboMenuMakeNewPlayList;
        /// <summary>
        /// コンボボックスにコンテキストメニューを追加する
        /// </summary>
        public void MakePlayListComboMenu() {
            string TAG = "MakePlayListComboMenu";
            string dbMsg = "";
            try {
                PlayListComboItemMenu = new ContextMenu();
                PlayListComboItemDelete = new MenuItem();
                PlayListComboItemDelete.Header = "コンボボックスから外す";
                //コンテキストメニュー表示時に発生するイベントを追加
                PlayListComboItemDelete.Click += PlayListComboItemDelete_Click;
                PlayListComboItemMenu.Items.Add(PlayListComboItemDelete);

                PlayListComboItemMove = new MenuItem();
                PlayListComboItemMove.Header = "コンボ内の順番…";

                PlayListComboItemMoveUp = new MenuItem();
                PlayListComboItemMoveUp.Header = "上に移動";
                PlayListComboItemMoveUp.Click += PlayListComboItemMoveUp_Click;
                PlayListComboItemMove.Items.Add(PlayListComboItemMoveUp);

                PlayListComboItemMoveDown = new MenuItem();
                PlayListComboItemMoveDown.Header = "下に移動";
                PlayListComboItemMoveDown.Click += PlayListComboItemMoveDown_Click;
                PlayListComboItemMove.Items.Add(PlayListComboItemMoveDown);
                PlayListComboItemMenu.Items.Add(PlayListComboItemMove);

                ComboMenuPlayListSave = new MenuItem();
                ComboMenuPlayListSave.Header = "このプレイリストを保存";
                ComboMenuPlayListSave.Click += ComboMenuPlayListSave_Click;
                PlayListComboItemMenu.Items.Add(ComboMenuPlayListSave);


                ComboMenuMakeNewPlayList = new MenuItem();
                ComboMenuMakeNewPlayList.Header = "新規プレイリスト作成";
                ComboMenuMakeNewPlayList.Click += ComboMenuMakeNewPlayList_Click;
                PlayListComboItemMenu.Items.Add(ComboMenuMakeNewPlayList);

                BackBeforeItem = new MenuItem();
                BackBeforeItem.Header = "前に再生していたアイテムに戻る";
                BackBeforeItem.Click += BackBeforeItem_Click;
                PlayListComboItemMenu.Items.Add(BackBeforeItem);

                RaisePropertyChanged("PlayListComboItemMenu");
                MyLog(TAG, dbMsg);
                //  Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// プレイリストコンボからの削除
        /// </summary>
        /// https://rksoftware.wordpress.com/2015/06/13/combobox-%E3%81%AE-itemssource-%E3%81%A8-selectedvalue-%E3%81%AE%E4%B8%A1%E6%96%B9%E3%81%AB-binding-%E3%82%92%E3%81%99%E3%82%8B/
        /// 
        private void PlayListComboItemDelete_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListComboItemDelete_Click";
            string dbMsg = "";
            try {
                string titolStr = "プレイリスト・コンボ：アイテムの削除";
                string msgStr = "削除するプレイリストが選択されていないようです。\r\n削除したいプレイリストファイル名をクリックしてください。";
                dbMsg += ",PlayListComboSelected= " + PlayListComboSelected;
                if (PlayListComboSelected == null) {
                    MessageBoxResult result = MessageShowWPF(titolStr, msgStr, MessageBoxButton.OK, MessageBoxImage.Error);
                    dbMsg += ",result=" + result;
                    return;
                } else {
                    msgStr = PlayListComboSelected + "をコンボボックスから外しますか？\r\n（ファイルは削除しません。)";
                    MessageBoxResult result = MessageShowWPF(titolStr, msgStr, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                    dbMsg += ",result=" + result;
                    if (result.Equals(MessageBoxResult.Cancel)) {
                        return;
                    }
                }
                //削除するアイテムを特定する  
                dbMsg += ",現在のプレイリストは[ " + PLComboSelectedIndex + "]";
                CurrentPlayListFileName = PlayLists[PLComboSelectedIndex];
                dbMsg += CurrentPlayListFileName;
                if (!CurrentPlayListFileName.Equals(ComboLastItemKey)) {
                    if (PLComboSource.ContainsKey(CurrentPlayListFileName)) {
                        //Binding変更
                        PLComboSource.Remove(CurrentPlayListFileName);
                        RaisePropertyChanged("PLComboSource");
                        //設定ファイル更新
                        string rPlayListStr = "";
                        string kVal = "";

                        foreach (KeyValuePair<string, string> item in PLComboSource) {
                            kVal = item.Key;
                            if (!kVal.Equals(ComboLastItemKey)) {
                                rPlayListStr += kVal;
                                CurrentPlayListFileName = kVal;
                            }
                        }
                        PlayListStr = rPlayListStr;
                        PLComboSelectedIndex = 0;
                        //Properties.Settings.Default.PlayListStr = PlayListStr;
                        //Properties.Settings.Default.CurrentPlayListFileName = CurrentPlayListFileName;
                        Properties.Settings.Default.Save();
                        AddPlayListCombo(CurrentPlayListFileName);
                        dbMsg += ",更新後のプレイリスト一覧＝ " + PlayListStr + "[" + PLComboSelectedIndex + "]" + CurrentPlayListFileName;
                    } else {
                        dbMsg += ",該当なし ";
                    }
                } else {
                    dbMsg += ",固定メニュー";
                }

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// コンボ内の順番…上に移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayListComboItemMoveUp_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListComboItemMoveUp_Click";
            string dbMsg = "";
            try {

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// コンボ内の順番…下に移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayListComboItemMoveDown_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListComboItemMoveDown_Click";
            string dbMsg = "";
            try {

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// このプレイリストを保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboMenuPlayListSave_Click(object sender, RoutedEventArgs e) {
            string TAG = "ComboMenuPlayListSave_Click";
            string dbMsg = "";
            try {
                SavePlayList();
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 新規プレイリスト作成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboMenuMakeNewPlayList_Click(object sender, RoutedEventArgs e) {
            string TAG = "ComboMenuMakeNewPlayList_Click";
            string dbMsg = "";
            try {
                MakeNewPlayListFile();
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// コンボボックスの選択変更
        /// </summary>
        /// <param name="adderIndex"></param>
        public void PlayListComboSelect(int adderIndex) {
            string TAG = "PlayListComboSelect";
            string dbMsg = "";
            try {
                int listIndex = Array.IndexOf(PlayLists, CurrentPlayListFileName);
                int IndexEnd = PlayLists.Length-1;
                dbMsg += ",[ " + listIndex + " / " + IndexEnd + "]" + CurrentPlayListFileName;
                listIndex += adderIndex;
                if (listIndex < 0) {
                    listIndex = IndexEnd;
                }else if (IndexEnd <=  listIndex) {
                    listIndex = 0;
                }
                CurrentPlayListFileName=PlayLists[listIndex];   
                RaisePropertyChanged("CurrentPlayListFileName");
                MyView.PLCombo.SelectedIndex=listIndex; 
                dbMsg += ">>[ " + listIndex + " / " + IndexEnd + "]" + CurrentPlayListFileName;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        /// <summary>
        /// 前に再生していたアイテムに戻る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackBeforeItem_Click(object sender, RoutedEventArgs e) {
            string TAG = "BackBeforeItem_Click";
            string dbMsg = "";
            try {
                
                CurrentPlayListFileName = BeforSelectListUrl;
                PlayListComboSelected = BeforSelectListUrl;
                //CurrentPlayListFileName = MoveBeforSelect.PlayListUrlStr;
                //PlayListComboSelected = MoveBeforSelect.PlayListUrlStr;
                int listIndex = Array.IndexOf(PlayLists, CurrentPlayListFileName);
                dbMsg += ",切り替える前は[" + listIndex + "/" + PlayLists.Length + "]" + PlayLists[listIndex];
                //元のコンボを再選択
                PLComboSelectedIndex = listIndex;

                /// 切り替える前の再生アイテム
                PLListSelectedItem = new PlayListModel();
                PLListSelectedItem = MoveBeforSelect.ListItem;
                SelectedPlayListIndex = PLList.IndexOf(PLListSelectedItem);
				RaisePropertyChanged("SelectedPlayListIndex");
				dbMsg += "の[" + SelectedPlayListIndex + "/" + PLList.Count + "]" + PLListSelectedItem.UrlStr;

                var cellInfo = MyView.PlayList.SelectedCells.FirstOrDefault();
                MyView.PlayList.Focus();
                MyView.PlayList.CurrentCell = cellInfo; 
                    //Select(MyView.PlayList.CurrentCell.RowNumber);

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }
        #endregion

        /// <summary>
        /// プレイリストのソート
        /// </summary>
        /// <param name="sortIndex"></param>
        /// <param name="headerName"></param>
        /// <param name="isDescending"></param>
        public void PlayListSort(int sortIndex, string headerName , System.ComponentModel.ListSortDirection? isDescending) {
            string TAG = "PlayListSort";
            string dbMsg = "";
            string retStr = "";
            try {
                dbMsg += "[" + sortIndex + "]" + headerName + ",降順" + isDescending;
				if (isDescending== ListSortDirection.Ascending) {
                    switch (sortIndex) {
                        case 0:
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderByDescending(n => n.Summary));
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderBy(n => n.ParentDir));
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderBy(n => n.UrlStr));
                            break;
                        case 1:
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderBy(n => n.ParentDir));
                            break;
                        case 2:
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderBy(n => n.Summary));
                            break;
                        case 3:
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderBy(n => n.extentionStr));
                            break;
                        default:
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderBy(n => n.UrlStr));
                            break;
                    }
				} else {
                    switch (sortIndex) {
                        case 0:
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderBy(n => n.Summary));
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderByDescending(n => n.ParentDir));
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderByDescending(n => n.UrlStr));
                            break;
                        case 1:
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderByDescending(n => n.ParentDir));
                            break;
                        case 2:
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderByDescending(n => n.Summary));
                            break;
                        case 3:
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderByDescending(n => n.extentionStr));
                            break;
                        default:
                            PLList = new ObservableCollection<PlayListModel>(PLList.OrderByDescending(n => n.UrlStr));
                            break;
                    }
                }
                RaisePropertyChanged("PLList");
                MyLog(TAG, dbMsg);
			} catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        readonly CountdownEvent condition = new CountdownEvent(1);

        /// <summary>
        /// 文字で渡された秒を時分秒の文字列で返す
        /// </summary>
        /// <param name="secStr"></param>
        /// <returns></returns>
        public string GetHMS(string secStr) {
            string TAG = "GetHMS";
            string dbMsg = "";
            string retStr = "";
            try {
                decimal sec = Decimal.Parse(secStr);
                dbMsg += "、sec=" + sec;
                var retH = Math.Floor(sec / 3600);
                if (0 < retH) {
                    retStr = retH + ":";
                    sec = sec - retH * 3600;
                }
                var retM = Math.Floor(sec / 60);
                if (0 < retM) {
                    if (retM < 10) {
                        retStr += "0" + retM + ":";
                    } else {
                        retStr += retM + ":";
                    }
                    sec = sec - retM * 60;
                } else {
                    retStr += "00:";
                }
                sec = Math.Floor(sec);
                if (0 < sec) {
                    if (sec < 10) {
                        retStr += "0" + sec;
                    } else {
                        retStr += sec;
                    }
                } else {
                    retStr = retStr + "00";
                }
                dbMsg += "、retStr=" + retStr;
                //MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
            return retStr;
        }


        /// <summary>
        /// メディアリソースの長さを取得
        /// </summary>
        public async void GetDuration() {
            string TAG = "GetDuration";
            string dbMsg = "";
            try {
                //       DurationStr = null;
                //string rStr = await MyView.webView.ExecuteScriptAsync($"document.getElementById(" +'"' + "durationSP" + '"' + ").innerHTML;");
                //dbMsg += "、rStr=" + rStr;
                //if (0<rStr.Length) {
                string CurrentTime = await MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").currentTime;");
                dbMsg += "、CurrentTime=" + CurrentTime;
                if (CurrentTime == null) {
                } else {
                    PositionStr = GetHMS(CurrentTime);
                    dbMsg += "、PositionStr=" + PositionStr;
                    double Position = double.Parse(CurrentTime);
                    if (0 < Position) {
                        MyLog(TAG, dbMsg);
                        return;
                    }
                }
                string Duration = await MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").duration;");
                dbMsg += "、Duration=" + Duration;
                if (Duration == null) {
                    //                 return;
                } else if (Duration.Equals("null")) {
                    dbMsg += "、Duration=nullという文字";
                } else if (Duration.Equals("")) {
                    dbMsg += "、Duration=空白";
                } else {
                    DurationStr = GetHMS(Duration);
                    dbMsg += "、DurationStr=" + DurationStr;
                    RaisePropertyChanged("DurationStr");
                    SliderMaximum = double.Parse(Duration);
                    RaisePropertyChanged("SliderMaximum");
                }
                //if (DurationStr == null) {
                //	Task.Delay(1000);
                //                GetDuration();
                //            }
                //}
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        // https://teratail.com/questions/363162?sort=3
        /// <summary>JavaScriptで呼ぶ関数を保持するオブジェクト</summary>
        private JsToCs CsClass = new JsToCs();

        /// <summary>WebView2のロード完了時</summary>
        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e) {
            string TAG = "WebView_NavigationCompleted";
            string dbMsg = "";
            try {
                dbMsg += ",ロード完了時";
                if (MyView.webView.CoreWebView2 != null) {
                    //JavaScriptからC#のメソッドが実行できる様に仕込む
                    //MyView.webView.CoreWebView2.AddHostObjectToScript("class", CsClass);  //20220331コメントアウト
                    dbMsg += ",class,CsClass追加済";
                    //ダミーで良ければ　CS_Util　に差し替え
                    GetDuration();
                } else {
                    dbMsg += ",CoreWebView2==null";
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// JavaScriptのaddEventListenerからpostMessageされた文字列を受け取る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void webView_CurrentTimeeReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e) {
            string TAG = "webView_CurrentTimeeReceived";
            string dbMsg = "";
            try {
                //JavaScriptのaddEventListenerからpostMessageされた文字列を受け取る
                var s = e.TryGetWebMessageAsString();
                dbMsg += ",s=" + s;
                if (s.Equals("ended")) {
                    dbMsg += "、再生終了";
                    //              await Task.Delay(1000);
                    ForwardList();
                    MyLog(TAG, dbMsg);
                } else {
                    //decimalに変換できれば（保留）再生ポジション
                    string CurrentTime = await MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").currentTime;");
                    PositionStr = GetHMS(CurrentTime);
                    dbMsg += "、PositionStr=" + PositionStr;
                    RaisePropertyChanged("PositionStr");
                    SliderValue = double.Parse(CurrentTime);
                    RaisePropertyChanged("SliderValue");
                    //          MyLog(TAG, dbMsg);
                }
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// プレイヤーへ
        /// </summary>
        /// <param name="targetItem">PlayListModelでurlを渡す</param>
        /// 
        public async void PlayListToPlayer(PlayListModel targetItem) {
            //int oldIndex,
            string TAG = "PlayListToPlayer";
            string dbMsg = "";
            try {
                SliderValue= 0;

                BeforSelect = NowSelect;
                //前に使っていた
                string? BeforSelecExtention = BeforSelect.ListItem.extentionStr;


				IsPlaying = false;
                //リストとインデックスは更新
                SelectedPlayListIndex= PLList.IndexOf(targetItem);
                dbMsg += "、現在のリスト:" + PlayListComboSelected + "[" + SelectedPlayListIndex + "]";
                NowSelect.PlayListUrlStr = PlayListComboSelected;
                NowSelect.SelectedIndex = SelectedPlayListIndex;
                infoStr = "[" + SelectedPlayListIndex + "/" + PLList.Count + "]" + targetItem.GranDir + " ・ " + targetItem.ParentDir + " ・ " + targetItem.Summary + " ( " + targetItem.extentionStr + " ) ";


                //同じファイルの再生なら再生動作をスキップ
                string targetURLStr = targetItem.UrlStr;
                dbMsg += "、targetURLStr=" + targetURLStr;
                if (NowSelect.ListItem.UrlStr != null) {
                    if (NowSelect.ListItem.UrlStr.Equals(targetURLStr)) {
                        dbMsg += "は既に再生中";
                        MyLog(TAG, dbMsg);
                        return;
                    }
                }
                string extention = System.IO.Path.GetExtension(targetURLStr);
                dbMsg += "、拡張子=" + extention;
				if (NowSelect != null) {
                    BeforSelect = NowSelect;
                    dbMsg += ">>BeforSelect:" + BeforSelect.PlayListUrlStr + "[" + BeforSelect.SelectedIndex + "]" + BeforSelect.ListItem.UrlStr;
                }
                NowSelect.ListItem = targetItem;
                dbMsg += ">>NowSelect:" + NowSelect.PlayListUrlStr + "[" + NowSelect.SelectedIndex + "]" + NowSelect.ListItem.UrlStr;

                IsHideControl = false;
                if (1 < MyView.FrameGrid.Children.Count) {
                    dbMsg += ">delete既存=" + MyView.FrameGrid.Children.Count + "件";
                    if (axWmp != null) {
                        axWmp.close();
                        axWmp = null;
                    } else if (flash != null) {
                        flash.Dispose();
                        flash = null;
                    }
                    MyView.FrameGrid.Children.RemoveAt(1);
                }else if ((0 <= Array.IndexOf(WebVideo, BeforSelecExtention))) {
                    dbMsg += "前に再生していたのはWeb" + BeforSelect.ListItem.UrlStr;
                    await MyView.webView.EnsureCoreWebView2Async(null);
                }

                toWeb = true;  // false;

                //if (-1 < Array.IndexOf(WebVideo, extention) ||
                //	targetURLStr.StartsWith("https")) {
                //	toWeb = true;
                //}
                //Frame frame = new Frame();
                dbMsg += "、[" + FreamWidth + "×" + FreamHeigh + "]";
                if (-1 < Array.IndexOf(WMPFiles, extention)
					|| -1 < Array.IndexOf(FlashVideo, extention)
					) {
                    host = new System.Windows.Forms.Integration.WindowsFormsHost();
                    //       host.HorizontalAlignment= "Stretch"
                    toWeb = false;
                    MyView.webView.Visibility = Visibility.Hidden;
                }
                dbMsg += "、Web=" + toWeb;
                if (MyView == null) {
                } else if ((0 <= Array.IndexOf(WebVideo, extention))) {
                    movieType = 0;
                    MyView.webView.Visibility = Visibility.Visible;
                    //WebView2のロード完了時のイベント
                    MyView.webView.NavigationCompleted += WebView_NavigationCompleted; PlayListSaveBTVisble = "Hidden";
                    //JavaScriptからのデータ送信    https://knooto.info/csharp-webview2-snippets/
                    MyView.webView.WebMessageReceived += webView_CurrentTimeeReceived;


                    //video要素、audio要素をJavaScriptから操作する http://www.htmq.com/video/
                    string tagStr = MakeVideoSouce(targetURLStr, (int)FreamWidth - 24, (int)FreamHeigh - 24);
                    dbMsg += "、tagStr\r\n" + tagStr;
                    // 実行ディレクトリを取得
                    dbMsg += "、\r\n" + Constant.currentDirectory;
                    SaveFile(Constant.currentDirectory, tagStr);
                    // ローカルファイルのURIを作成
                    Uri uri = new Uri(Constant.currentDirectory);
                    TargetURI = uri;
                    RaisePropertyChanged("TargetURI");
                    // WebView2にローカルファイルのURIを設定
                    MyView.webView.CoreWebView2.Navigate(TargetURI.AbsoluteUri);
                    IsPlaying = false;

                    //               GetDuration();
                    await Task.Run(() => {
                        MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").play();");
                    });
                    ClickPlayBt();
                } else if ((0 <= Array.IndexOf(WMPFiles, extention))) {
                    movieType = 1;
                    axWmp = new AxWindowsMediaPlayer();
					if (axWmp == null) {
                        dbMsg += "、axWmp == null" ;
                        return;
					}
                    host.Child = axWmp;
                    MyView.FrameGrid.Children.Add(host);
                    axWmp.stretchToFit = true;
                    axWmp.URL = targetURLStr;
                    AxWMPLib._WMPOCXEvents_MediaChangeEventHandler handler = null;
                    handler = delegate (object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e) {
                        axWmp.MediaChange -= handler;

                        // 新しいメディア情報の取得；これでしかdurationを取得できなかった
                        WMPLib.IWMPMedia media = (WMPLib.IWMPMedia)e.item;
                        SliderMaximum = media.duration;                   ///GetDuration();
                        RaisePropertyChanged("SliderMaximum");
                        if (0 < SliderMaximum) {
                            DurationStr = media.durationString;                  // span.ToString("HH:mm:ss");
                        }
                        RaisePropertyChanged("DurationStr");
                    };
                    axWmp.MediaChange += handler;
                    SetupTimer();
                    //    axWmp.PositionChange += axWindowsMediaPlayer_PositionChange;  //はシークなどの操作がされた時のみ発生  
                    // UIを無効化
                    axWmp.uiMode = "none";
                    //            infoStr = axWmp.currentMedia.name;

                } else if ((0 <= Array.IndexOf(FlashVideo, extention))) {
                    movieType = 2;
                    if (flash == null) {
                        flash = new AxShockwaveFlash();
                    }

                    flash.BeginInit();
                    flash.Name = "flashPlayer";
                    flash.EndInit();

                    host.Child = flash;
                    MyView.FrameGrid.Children.Add(host);
                    //System.Runtime.InteropServices.COMException: 'クラスが登録されていません (0x80040154 (REGDB_E_CLASSNOTREG))'
                    //>>C:\Windows\System32\FlashにFlash.ocxなどをコピー
                    //>>ツール/ツールボックスアイテムの選択 のCOMコンポーネントタグから参照して読み込ませる
                    //>>(これでもツールボックスには表示されない)
                    //     flashMovie.stretchToFit = true;
                    dbMsg += ",assemblyPath=" + assemblyPath;
                    string[] urlStrs = assemblyPath.Split(Path.DirectorySeparatorChar);
                    assemblyName = urlStrs[urlStrs.Length - 1];
                    dbMsg += ">>" + assemblyName;
					//          playerUrl = @assemblyPath.Replace(assemblyName, "flvplayer-305.swf");       //☆デバッグ用を\bin\Debugにコピーしておく

                    //フラダンス　
					playerUrl = @assemblyPath.Replace(assemblyName, "fladance.swf");   
                    //☆デバッグ用を\bin\Debugにコピーしておく
					dbMsg += ",playerUrl=" + playerUrl;
                    //,playerUrl=C:\Users\博臣\source\repos\file_tree_clock_web1\file_tree_clock_web1\bin\Debug\fladance.swf 
                    if (File.Exists(playerUrl)) {
                        dbMsg += ",Exists=true";
                    } else {
                        dbMsg += ",Exists=false";
                    }

                    ////clsId = "clsid:d27cdb6e-ae6d-11cf-96b8-444553540000";       //ブラウザーの ActiveX コントロール
                    ////codeBase = @"http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=10,0,0,0";
                    ////string pluginspage = @"http://www.macromedia.com/go/getflashplayer";
                    string flashVvars = @"fms_app=&video_file=" + targetURLStr +
                                        "&vol=" + SoundValue +          //0 ～ 1
                                        "&mute=false" +
                                        // & amp;		"link_url ="+ nextMove + "&" +
                                        "&image_file=&link_url=&autoplay=true&controllbar=true&buffertime=10" + '"';
                    flash.FlashVars = flashVvars;
                    flash.MovieData = targetURLStr;
                    flash.LoadMovie(0, playerUrl);
                    flash.AllowScriptAccess = "always";
                    /*
                         Flash 4 で新しくサポートされたスクリプトメソッド       http://kb2.adobe.com/jp/cps/228/228681.html
                         https://csharp.hotexamples.com/jp/examples/AxShockwaveFlashObjects/AxShockwaveFlash/-/php-axshockwaveflash-class-examples.html
                    ActiveX を使用するデスクトップアプリケーションとの通信
                    https://help.adobe.com/ja_JP/as3/dev/WS5b3ccc516d4fbf351e63e3d118a9b90204-7cb0zephyr_serranozephyr.html
                        */
                    // IsHideControl = true;
                }
                IsPlaying = true;
                bTime = 0;
                RaisePropertyChanged("infoStr");
                dbMsg += "、infoStr=" + infoStr;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        /// <summary>
        /// タイマのインスタンス
        /// </summary>
        private DispatcherTimer _timer;

        /// <summary>
        /// カウントアップ前の再生ポジション
        /// </summary>
        private double bTime = 0;

        /// <summary>
        /// タイマメソッド
        /// WMPはここで再生ポジションを取得する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyTimerMethod(object sender, EventArgs e) {
            string TAG = "MyTimerMethod";
            string dbMsg = "";
            try {
                if (axWmp != null && !IsPositionSLDraging) {
                    dbMsg += ",currentPosition=" + SliderValue;
                    SliderValue = 0;

                    if (0 < axWmp.Ctlcontrols.currentPosition) {
                        SliderValue = axWmp.Ctlcontrols.currentPosition;                        //GetPlayPosition();
                        RaisePropertyChanged("SliderValue");
                        dbMsg += ">>" + SliderValue + "/" + SliderMaximum;
                        PositionStr = GetHMS(SliderValue.ToString()); 
                        RaisePropertyChanged("PositionStr");
                        dbMsg += "=" + PositionStr;

                    } else {
                        SetupTimer();
                        MyLog(TAG, dbMsg);
                    }

                }
                dbMsg += ",IsSendAuto=" + IsSendAuto;
                if (IsSendAuto && (SliderMaximum - SliderValue) < 1) {
                    //少数にすると拾えない事がある
                    _timer.Stop();
                    MyLog(TAG, dbMsg);
                    ForwardList();
					if (!IsPlaying) {
                        IsPlaying=true;
                    }
                } else {
                    bTime = SliderValue;
                }
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        /// <summary>
        /// タイマを設定する
        /// </summary>
        public void SetupTimer() {
            string TAG = "SetupTimer";
            string dbMsg = "";
            try {
                dbMsg += ",SliderValue=" + SliderValue + " / " + SliderMaximum;
                if (_timer != null) {
                    _timer.Stop();
                    _timer = null;
                }
                //if (_timer == null) {
                    // タイマのインスタンスを生成
                    _timer = new DispatcherTimer(); // 優先度はDispatcherPriority.Background
                                                    // インターバルを設定
                    _timer.Interval = new TimeSpan(0, 0, 1);
                    // タイマメソッドを設定
                    _timer.Tick += new EventHandler(MyTimerMethod);
					// タイマを開始
				//} else {
    //                _timer.Stop();
    //            }
                _timer.Start();
                // 画面が閉じられるときに、タイマを停止
                //MyView.Closing += new CancelEventHandler(StopTimer);
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        public void TimerStop() {
            string TAG = "TimerStop";
            string dbMsg = "";
            try {
                dbMsg += ",SliderValue=" + SliderValue +" / " + SliderMaximum;

                if (_timer != null) {
                    _timer.Stop();
                    _timer = null;
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        // タイマを停止
        private void StopTimer(object sender, CancelEventArgs e) {
            string TAG = "StopTimer";
            string dbMsg = "";
            try {
                _timer.Stop();
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        public void BackFromPlayer(PlayListModel targetItem) {
            //int oldIndex,
            string TAG = "BackFromPlayer";
            string dbMsg = "";
            try {
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        //public AxShockwaveFlashObjects.AxShockwaveFlash SFPlayer;
        private System.ComponentModel.IContainer components = null;
        private void SFPlayer_Move(object sender, EventArgs e) {
            string TAG = "[SFPlayer_Move]";
            string dbMsg = TAG;
            try {
                dbMsg += e.ToString() + ";";
                //switch (e.command)
                //{
                //    case 0:
                //        dbMsg += "1";
                //        //            PlayTitolLabel.Text = ("Undefined;WindowsMediaPlayerの状態が定義されていません");
                //        break;
                //}
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                dbMsg += "<<以降でエラー発生>>" + er.Message;
                MyLog(TAG, dbMsg);
            }
        }

        private void SFPlayer_RegionChanged(object sender, EventArgs e) {
            string TAG = "[SFPlayer_RegionChanged]";
            string dbMsg = TAG;
            try {
                dbMsg += e.ToString() + ";";
                //switch (e.command)
                //{
                //    case 0:
                //        dbMsg += "1";
                //        //            PlayTitolLabel.Text = ("Undefined;WindowsMediaPlayerの状態が定義されていません");
                //        break;
                //}
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                dbMsg += "<<以降でエラー発生>>" + er.Message;
                MyLog(TAG, dbMsg);
            }
        }

        #endregion
        ///Drag & Drop///////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// PopupのTextBlockのtext
        /// </summary>
        public string DraggedItem_name { get; set; }
        public bool Drag_now = false;           // { get; set; }

        /// <summary>
        /// ドラッグ開始；正常に処理が開始されればtrueを返す。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool PlayList_DragEnter() {
            string TAG = "PlayList_DragEnter";
            string dbMsg = "";
            try {

                if (Drag_now) {
                    dbMsg += "既にDrag中";
                } else {
                    string titolStr = "プレイリストアイテムファイルの操作";
                    string errorStr = "ドラッグ";
                    string? doStr = null;
                    if (SelectedPlayListFiles == null) {
                        MyView.popup_text.Text = "";
                        Drag_now = false;
                        dbMsg += ".SelectedPlayListFiles == null";
                    } else if (PlayListOpelate(titolStr, errorStr, doStr)) {
                        dbMsg += ",選択" + SelectedPlayListFiles.Count + "件";
                        DraggedItem_name = "" + SelectedPlayListFiles[0].UrlStr;
                        RaisePropertyChanged("DraggedItem_name");
                        dbMsg += "Drag開始＝" + DraggedItem_name;
                        MyView.popup_text.Text = DraggedItem_name;
                        Drag_now = true;
                        PlayListSelectionMode = "Single";
                        RaisePropertyChanged("PlayListSelectionMode");

                    } else {
                        MyView.popup_text.Text = "";
                        Drag_now = false;
                    }
                    RaisePropertyChanged("Drag_now");
                    dbMsg += ",Drag_now=" + Drag_now;
                    MyLog(TAG, dbMsg);
                }
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
            return Drag_now;
        }

        /// <summary>
        /// アイテムのドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PlayList_Drop(int dropRow) {
            string TAG = "PlayList_Drop";
            string dbMsg = "";
            try {
                dbMsg += "Drag_now=" + Drag_now;
                if (Drag_now) {
                    dbMsg += ">>dropRow=" + dropRow;
                    PlayListItemMoveTo(dropRow, SelectedPlayListFiles);
                    Drag_now = false;
                    //      RaisePropertyChanged("Drag_now");
                    PlayListSelectionMode = "Extended";
                    RaisePropertyChanged("PlayListSelectionMode");
                    MyView.popup_text.Text = "";
                    MyView.popup1.IsOpen = false;

                }
                ////DataGrid DG = (DataGrid)sender;
                ////// ドロップ先(dataGridView2)のクライアント位置からDataGridViewの位置情報を取得します。
                ////var point = DG.po.PointToClient(new Point(e.X, e.Y));
                ////var hitTest = DG.HitTest(point.X, point.Y);
                //int InsertTo = 0;   //挿入位置は先頭固定
                //                    // ファイルのドラッグアンドドロップのみを受け付けるようにしています。
                //if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                //    // ドロップされたファイルは、アプリケーション側に内容がコピーされるものとします。
                //    //	e.Effect = DragDropEffects.Copy;
                //}
                //// ドラッグアンドドロップされたファイルのパス情報を取得します。

                //foreach (String filename in (string[])e.Data.GetData(DataFormats.FileDrop)) {
                //    dbMsg += "\r\n" + filename;
                //}

                //string[] rFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                //if (0 < rFiles.Count()) {
                //    foreach (string url in rFiles) {
                //        dbMsg += "\r\n" + url;
                //        if (File.Exists(url)) {
                //            if (VM.AddToPlayList(url, 0)) {
                //                dbMsg += ">>格納";
                //            }
                //        } else if (Directory.Exists(url)) {
                //            //フォルダなら中身の全ファイルで再起する
                //            string[] r2files = System.IO.Directory.GetFiles(url, "*", SearchOption.AllDirectories);
                //            VM.FilesAdd(r2files, InsertTo);
                //        }
                //    }
                //}

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
        public void PlayList_DragLeave() {
            string TAG = "PlayList_DragLeave";// + fileName;
            string dbMsg = "";
            try {
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }

        }

        /// <summary>
        /// オブジェクトがコントロールの境界を越えてドラッグされると発生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PlayList_DragOver() {
            string TAG = "PlayListBox_DragOver";// + fileName;
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

        ///Drop/////////////////////////////////////////////////////////////
        //#region PlayListアイテムでマウスダウン
        //private ViewModelCommand _PlayListLeftClick;

        //public ViewModelCommand PlayListLeftClick {
        //	get {
        //		if (_PlayListLeftClick == null) {
        //			_PlayListLeftClick = new ViewModelCommand(PLMouseDown);
        //		}
        //		return _PlayListLeftClick;
        //	}
        //}

        ///// <summary>
        ///// 始めのマウスクリック
        //// https://dobon.net/vb/dotnet/control/draganddrop.html
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        private void PLMouseDown() {
            string TAG = "[PlayList_MouseDown]";// + fileName;
            string dbMsg = "";
            try {
                if (PLListSelectedItem != null) {
                    //DraggedItem = PLListSelectedItem;
                    //dbMsg += ",UrlStr=" + PLListSelectedItem.UrlStr;
                    //int oldIndex = PLList.IndexOf(PLListSelectedItem);
                    //dbMsg += "[" + oldIndex + "]";
                    //_isDragging = true;
                    //				var row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(shareGrid));
                    //	if (row == null || row.IsEditing) return;

                    //set flag that indicates we're capturing mouse movements
                    //draglist = (ListBox)sender;
                    //PlayListMouseDownNo = draglist.SelectedIndex;
                    //dbMsg += "(Down;" + PlayListMouseDownNo + ")";
                    //if (e.Button == System.Windows.Forms.MouseButtons.Left) {                   //マウス左ボタン
                    //	dbMsg += ",選択モード切替；ModifierKeys=" + Control.ModifierKeys;
                    //	if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) {                //シフト
                    //		playListBox.SelectionMode = SelectionMode.MultiExtended;               //3:		インデックスが配列の境界外です。?
                    //	} else if ((Control.ModifierKeys & Keys.Control) == Keys.Control) {     //コントロール
                    //		playListBox.SelectionMode = SelectionMode.MultiSimple;
                    //		2:	MultiSimple / MultiExtended   http://www.atmarkit.co.jp/fdotnet/chushin/introwinform_03/introwinform_03_02.html

                    //				} else {                                                                //無しなら
                    //		playListBox.SelectionMode = SelectionMode.One;                         //1:単一選択
                    //	}
                    //	dbMsg += " ,SelectionMode=" + draglist.SelectionMode;
                    //}
                    //if (-1 < PlayListMouseDownNo) {
                    //	PlayListMouseDownValue = draglist.SelectedValue.ToString();
                    //	dbMsg += PlayListMouseDownValue;
                    //	dragFrom = draglist.Name;
                    //	dragSouceIDl = draglist.SelectedIndex;
                    //	mouceDownPoint = Control.MousePosition;
                    //	mouceDownPoint = draglist.PointToClient(mouceDownPoint);//ドラッグ開始時のマウスの位置をクライアント座標に変換
                    //	dbMsg += "(mouceDownPoint;" + mouceDownPoint.X + "," + mouceDownPoint.Y + ")";
                    //	dragSouceIDP = draglist.IndexFromPoint(mouceDownPoint);//マウス下のListBoxのインデックスを得る
                    //	dbMsg += "(Pointから;" + dragSouceIDP + ")";
                    //}
                } else {
                    dbMsg += "選択値無し";
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        //public ICommand PlayListClick => new DelegateCommand(PlayListKeyUp);
        /// <summary>
        /// キーショートカットの割付
        /// </summary>
        /// <param name="targetKey"></param>
        public void WindowKeyUp(Key targetKey) {
            string TAG = "WindowKeyUp";
            string dbMsg = "";
            try {

                dbMsg += "targetKey=" + targetKey;
				Key KeyReturn = default;
				if (targetKey == Key.Return) {
					ClickPlayBt();
				} else if (targetKey == Key.Up) {
                    RewindList();
                } else if (targetKey == Key.Down) {
                    ForwardList();
                } else if (targetKey == Key.Right) {
                    ClickForwardAsync();
                } else if (targetKey == Key.Left) {
                    ClickRewAsync();
                } else if (targetKey == Key.PageUp) {
                    PlayListComboSelect(-1);
                } else if (targetKey == Key.PageDown) {
                    PlayListComboSelect(1);
                } else if (targetKey == Key.Delete) {
                    PlayListItemDelete();
                }


                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// プレイリストでのマウスアップ                    Completes a drag/drop operation.
        /// </summary>
        public void PLMouseUp(PlayListModel selectedItem) {
            string TAG = "[PLMouseUp]";
            string dbMsg = "";
            try {

                if (selectedItem != null) {
                    dbMsg += ",選択ファイル" + selectedItem.UrlStr;
                    PlayListModel targetItem = new PlayListModel();
                    targetItem.UrlStr = "https://www.yahoo.co.jp/";
                    string titolStr = "プレイリストアイテムファイルの操作";
                    string errorStr = "マウスアップ";
                    string? doStr = null;
                    if (PlayListOpelate(titolStr, errorStr, doStr)) {
                    //if (DraggedItem != null) {
                    //	//				DraggedItem = PLListSelectedItem;
                    //	dbMsg += ",UrlStr=" + PLListSelectedItem.UrlStr;
                    //	int oldIndex = PLList.IndexOf(PLListSelectedItem);
                    //	dbMsg += "[" + oldIndex + "]";
                    //	if (!_isDragging) {            //|| _isEditing
                    //		return;
                    //	}

                    //	//get the target item
                    //	PlayListModel
                        targetItem = selectedItem;  // ;
                        dbMsg += ",UrlStr=" + targetItem.UrlStr;
                        SelectedPlayListFiles[0] = targetItem;
                        //	if (targetItem == null) {           // || !ReferenceEquals(DraggedItem, targetItem)

                        //		//// create tempporary row
                        //		//var temp = DraggedItem.Row.Table.NewRow();
                        //		//temp.ItemArray = DraggedItem.Row.ItemArray;
                        //		//int tempIndex = _shareTable.Rows.IndexOf(DraggedItem.Row);

                        //		////remove the source from the list
                        //		//_shareTable.Rows.Remove(DraggedItem.Row);

                        //		////get target index
                        //		//var targetIndex = _shareTable.Rows.IndexOf(targetItem.Row);

                        //		////insert temporary at the target's location
                        //		//_shareTable.Rows.InsertAt(temp, targetIndex);

                        //		////select the dropped item
                        //		//shareGrid.SelectedItem = shareGrid.Items[targetIndex];
                        //	}

                        //reset
                        //ResetDragDrop();
                    } else {
                        dbMsg += "選択値無し";
                    }
                    //   dbMsg += ",UrlStr=" + targetItem.UrlStr;
                    //IsPlaying = false;
                    PlayListToPlayer(targetItem);
				} else {
                    dbMsg += ",選択ファイル無し";
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }
        //#endregion

        /////////////////////////////////////////////////////////////Drop///


        #region inputダイアログのサンプル
        /// <summary>
        /// 三択ダイアログのコールバック
        /// https://days-of-programming.blogspot.com/2018/01/livetwpf.html
        /// </summary>
        /// <param name="msg"></param>
        //public void MessageBoxClosed(ConfirmationMessage msg)
        //{
        //    string TAG = "MessageBoxClosed";
        //    string dbMsg = "";
        //    try
        //    {
        //        dbMsg += ",ConfirmationMessage=" + msg.Response;
        //        if (msg.Response == null)
        //        {
        //            dbMsg += "null>>Cancel";
        //        } else if (msg.Response.Value){     //true
        //            dbMsg += ">>Yes";
        //        }else{                              //false
        //            dbMsg += ">>No";
        //        }
        //        MyLog(TAG, dbMsg);
        //    }
        //    catch (Exception er)
        //    {
        //        MyErrorLog(TAG, dbMsg, er);
        //    }
        //}

        /// <summary>
        /// ボタン内で表示させたConfirmからのコールバック
        /// 》OKボタンで現在日時算出
        /// </summary>
        /// <param name="message"></param>
        //public void ConfirmFromView(ConfirmationMessage message)
        //{
        //    string TAG = "ConfirmFromView";
        //    string OutputMessage = $"{DateTime.Now}: ConfirmFromView: {message.Response ?? false}";
        //    string dbMsg = "OutputMessage=" + OutputMessage;
        //    MyLog(TAG, dbMsg);
        //}


        #endregion

        public string DResult { get; private set; }

        #region playList　/////////////////////////////////////////////////////////////
        /// <summary>
        /// 保存ボタンの表示
        /// </summary>
        public string PlayListSaveBTVisble { get; set; }

   //     public ICommand FileNameInputShow => new DelegateCommand(MakeNewPlayListFile);
        /// <summary>
        /// 新規プレイリストを作成する
        /// </summary>
        /// https://water2litter.net/rye/post/c_io_save_dialog/
        public void MakeNewPlayListFile() {
            string TAG = "MakeNewPlayListFile";
            string dbMsg = "";
            try {
                NowSelectedPath = System.IO.Path.GetDirectoryName(CurrentPlayListFileName);
                SaveFileDialog SFDialog = new SaveFileDialog() {
                    Title = "プレイリストを新規作成",
                    InitialDirectory = NowSelectedPath,
                    FileName = String.Format("{0:yyyyMM_ss}", DateTime.Now),
                    DefaultExt = ".m3u8",

                    AddExtension = true,        // ユーザーが拡張子を省略したときに、自動的に拡張子を付けるか。規定値はtrue。
                    CheckFileExists = false,    // ユーザーが存在しないファイルを指定したときに、警告するか。規定値はfalse。
                    CheckPathExists = true,     // ユーザーが存在しないパスを指定したときに、警告するか。規定値はtrue。
                    CreatePrompt = false,       // ユーザーが存在しないファイルを指定したときに、作成の許可を求めるか。規定値はfalse。
                    CustomPlaces = null,        // ダイアログ左側のショートカットのリスト。
                    DereferenceLinks = false,   // ショートカットが参照先を返す場合はtrue。リンクファイルを返す場合はfalse。規定値はfalse。
                    Filter = string.Empty,      // ダイアログで表示するファイルの種類のフィルタを指定する文字列。
                    FilterIndex = 1,            // 選択されたFilterのインデックス。規定値は1。
                    OverwritePrompt = true,     // 存在するファイルを指定したときに、警告するか。規定値はtrue。
                    ValidateNames = true,       // ファイル名がWin32に適合するか検査するかどうか。規定値はfalse。
                };
                if (SFDialog.ShowDialog() == true) {
                    string nSelectedFile = SFDialog.FileName;
                    dbMsg += ">>" + nSelectedFile;
                    if (File.Exists(nSelectedFile)) {
                        string titolStr = "既に存在するファイルです";
                        string msgStr = "再度、ファイル作成ができるダイアログを開きますか";
                        MessageBoxResult result = MessageShowWPF(titolStr, msgStr, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                        dbMsg += ",result=" + result;
                        if (result == MessageBoxResult.Yes) {
                            MakeNewPlayListFile();
                        } else {
                            return;
                        }
                    } else {
                        dbMsg += "新規作成";
                        NowSelectedPath = Path.GetDirectoryName(nSelectedFile);
                        string newFileName = System.IO.Path.GetFileName(nSelectedFile);
                        CurrentPlayListFileName = nSelectedFile;
                        dbMsg += ">>CurrentPlayListFileName=" + CurrentPlayListFileName;

                        StreamWriter sw = File.CreateText(CurrentPlayListFileName);
                        sw.Close();

                        //設定ファイル更新
                        Properties.Settings.Default.Save();
                        AddPlayListCombo(CurrentPlayListFileName);
                    }
                    //} else {
                    //    dbMsg += "キャンセルされました";
                    //}

                }

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        public ICommand PlayListSave => new DelegateCommand(SavePlayList);
        /// <summary>
        /// 表示されてるプレイリストを保存する
        /// </summary>
        /// <param name="url"></param>
        private void SavePlayList() {
            string TAG = "SavePlayList";
            string dbMsg = "";
            try {
                ListItemCount = PLList.Count();
                dbMsg += "\r\n" + ListItemCount + "件";
                string text = "";
                foreach (PlayListModel One in PLList) {
                    text += One.UrlStr + "\r\n";
                }
                StreamWriter sw = new StreamWriter(CurrentPlayListFileName, false, Encoding.UTF8);
                sw.Write(text);
                sw.Close();
                PlayListSaveBTVisble = "Hidden";
                RaisePropertyChanged("PlayListSaveBTVisble");
                PlayListSaveRoot.IsEnabled = false;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// プレイリストに変化があった場合、trueを渡せば保存、falseで設問によって保存。
        /// 保存しなければ保存準備だけする。
        /// </summary>
        /// <param name="isSave">trueを渡せば保存、falseで設問によって保存</param>
        private void IsDoSavePlayList(bool isSave) {
            string TAG = "IsDoSavePlayList";
            string dbMsg = "";
            try {
                dbMsg += "isSave=" + isSave;
                PlayListSaveRoot.IsEnabled = true;
                PlayListSaveBTVisble = "Visible";
                RaisePropertyChanged("PlayListSaveBTVisble");
                dbMsg += "＞isSave＞" + isSave;
                if (isSave) {
                    SavePlayList();
                    dbMsg += "＞＞保存済み";
                } else {
                    string titolStr = "プレイリストが変更されています";
                    string msgStr = "保存しますか？";
                    MessageBoxResult result = MessageShowWPF(titolStr, msgStr, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    dbMsg += ",result=" + result;
                    if (result == MessageBoxResult.Yes) {
                        SavePlayList();
                    }
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 選択、左クリックされたアイテムを取得し実行確認ダイアログを表示
        /// メッセージが渡されなければダイアログは表示しない
        /// </summary>
        /// <param name="titolStr">表示するダイアログのタイトル</param>
        /// <param name="errorStr">操作対象が取得できない場合のメッセージ</param>
        /// <param name="doStr">実行確認ダイアログのメッセージ</param>
        /// <returns> 実行可否のBool </returns>
        public Boolean PlayListOpelate(string titolStr, string? errorStr, string? doStr) {
            //int oldIndex,
            string TAG = "PlayListOpelate";
            string dbMsg = "";
            Boolean retBool = false;
            try {
                Boolean isNotselect = false;

                SelectedPlayListFiles = new List<PlayListModel>();      //  (List<PlayListModel>)MyView.PlayList.SelectedItems;
                                                                        //       NowSelectedFile = PLListSelectedItem.UrlStr;
                if (MyView.PlayList.SelectedItems != null) {
                    dbMsg += "," + MyView.PlayList.SelectedItems.Count + "件";
                    if (0 < MyView.PlayList.SelectedItems.Count) {
                        for (int i = 0; i < MyView.PlayList.SelectedItems.Count; ++i) {
                            PlayListModel oneItem = (PlayListModel)MyView.PlayList.SelectedItems[i];
                            SelectedPlayListFiles.Add(oneItem);
                            dbMsg += "\r\n[" + i + "]" + oneItem.UrlStr;
                        }
                        if (1 == MyView.PlayList.SelectedItems.Count) {
                            PLListSelectedItem = (PlayListModel)MyView.PlayList.SelectedItems[0];
                        }

                    } else {
                        isNotselect = false;
                    }
                } else {
                    isNotselect = false;
                }
                int oldIndex = PLList.IndexOf(PLListSelectedItem);
                //       dbMsg += "[" + SelectedPlayListIndex + "]" + NowSelectedFile;
                dbMsg += "操作するリストは[" + oldIndex + "]" + NowSelectedFile;
                if (isNotselect) {  //PLListSelectedItem == null || SelectedPlayListIndex < 0
                    if (errorStr != null) {
                        errorStr = errorStr + "するプレイリストアイテムが選択されていないようです。\r\n" + errorStr + "したいプレイリストアイテムをクリックしてください。";
                        MessageBoxResult result = MessageShowWPF(titolStr, errorStr, MessageBoxButton.OK, MessageBoxImage.Error);
                        dbMsg += ",result=" + result;
                    }
                } else {
                    retBool = true;
                    if (doStr != null) {
						//        doStr = SelectedPlayListFiles[0].fileName + doStr;
						doStr = PLListSelectedItem.UrlStr + doStr;
						MessageBoxResult result = MessageShowWPF(titolStr, doStr, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                        dbMsg += ",result=" + result;
                        if (result.Equals(MessageBoxResult.Cancel)) {
                            retBool = false;
                        }
                    }
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
            return retBool;
        }


        ////プレイリストのアイテム移動////////////////////////////////////

        /// <summary>
        /// プレイリスト上での移動/追加
        /// </summary>
        public void PlayListItemMoveTo(int dropRow, List<PlayListModel> dropPlayListFiles) {
            //int oldIndex,
            string TAG = "PlayListItemMoveTo";
            string dbMsg = "";
            try {
                if (ofDialog != null) {
                    dbMsg += "OpenFileDialog";
                    //   ofDialog.Dispose();

                }
                if (cofDialog != null) {
                    dbMsg += "CommonOpenFileDialog";
                    //  cofDialog.Dispose();ではない
                }
                dbMsg += "[" + dropRow + "/" + PLList.Count + "]へ" + dropPlayListFiles.Count + "件移動";
                int insertRow = dropRow;

                foreach (PlayListModel one in dropPlayListFiles) {
					string? extention = one.extentionStr;
					if (-1 < Array.IndexOf(WMPFiles, extention)
                        || -1 < Array.IndexOf(FlashVideo, extention)
                        || -1 < Array.IndexOf(WebVideo, extention)
                        ) {
                        int removeIndex = PLList.IndexOf(one);
                        if (-1 < removeIndex) {
                            dbMsg += "\r\n[" + removeIndex + "/" + PLList.Count + "]";
                            PLList.Remove(one);
                        }
                        dbMsg += ">>[" + insertRow + "/" + PLList.Count + "]" + one.UrlStr;
                        if (insertRow < PLList.Count) {
                            PLList.Insert(insertRow, one);
                        } else {
                            PLList.Add(one);
                        }
                        if (insertRow == dropRow) {
                            PLListSelectedItem = one;
                        }
                        insertRow++;
					} else {
                        dbMsg += "\r\n" + one.UrlStr + "は対象外" ;
                    }

                }
                RaisePropertyChanged("PLList");
                RaisePropertyChanged("PLListSelectedItem");
                ListItemCount = PLList.Count();
                RaisePropertyChanged("ListItemCount");
                IsDoSavePlayList(false);
                dbMsg += ">>[" + insertRow + "/" + PLList.Count + "]";
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        /// <summary>
        /// 指定されたプレイリストリストに切り替え、URLで指定されたアイテムを選択する
        /// </summary>
        /// <param name="selectListFile"></param>
        /// <param name="SelectedMediaFile"></param>
        public void PlayListItemSelect(string selectListFile , string SelectedMediaFile) {
            //int oldIndex,
            string TAG = "PlayListItemSelect";
            string dbMsg = "";
            try {
                //        BeforSelect = NowSelect;
                dbMsg += ",現在[" + PLComboSelectedIndex + "/"+ PLComboSource.Count + "]" + PlayListComboSelected ;
                dbMsg += ">>" + selectListFile + " の " + SelectedMediaFile + "を指定";
                //移動先にリストを切り替える
                CurrentPlayListFileName = selectListFile;
                RaisePropertyChanged("CurrentPlayListFileName");
                NowSelectedFile = SelectedMediaFile;
                RaisePropertyChanged("NowSelectedFile");
                dbMsg += ">リスト切替>" + CurrentPlayListFileName;
                if (PLComboSource.ContainsKey(CurrentPlayListFileName)) {
                    dbMsg += "追加済み;";
                } else {
                    AddPlayListCombo(CurrentPlayListFileName);
                    //         listIndex = 0;
                    PlayListStr = "";
                    foreach (var PLComboItem in PLComboSource) {
                        PlayListStr += PLComboItem.Key + ",";
                    }
                    PlayListStr = PlayListStr.Substring(0, PlayListStr.Length - 1);
                    dbMsg += ",PlayListStr=" + PlayListStr;
                    PlayLists = PlayListStr.Split(',');
                    dbMsg += ":追加";
                }

				int PLIndex = 0;
				foreach (string PLKey in PLComboSource.Keys) {
                    if (PLKey.Equals(selectListFile)) {
                        break;
                    }
                    PLIndex++;
                }
                PLComboSelectedIndex = PLIndex;
                RaisePropertyChanged("PLComboSelectedIndex");
                ListUpFiles(CurrentPlayListFileName);
                dbMsg += "、現在" + PLList.Count + "件";
                int listIndex = 0;
                foreach (PlayListModel PLItem in PLList) {
                    if (PLItem.UrlStr.Equals(NowSelectedFile)) {
                        NowSelect.PlayListUrlStr = CurrentPlayListFileName;
                        NowSelect.ListItem = PLItem;
                        NowSelect.SelectedIndex = listIndex;
                        dbMsg += "," + NowSelect.PlayListUrlStr + "[" + NowSelect.SelectedIndex + "]" + NowSelect.ListItem.UrlStr;
                        SelectedPlayListIndex = NowSelect.SelectedIndex;
                        RaisePropertyChanged("SelectedPlayListIndex");
                    }
                    listIndex++;
                }
                Properties.Settings.Default.Save();
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 選択されているプレイリストアイテムの削除
        /// </summary>
        private void PlayListItemDelete() {
            string TAG = "PlayListItemDelete";
            string dbMsg = "";
            try {
                string titolStr = "プレイリストアイテムファイルの操作";
                string errorStr = "削除";
                string? doStr = null;
                if (!PlayListOpelate(titolStr, errorStr, doStr)) {
                    return;
                }

                dbMsg += "開始時=" + SelectedPlayListFiles.Count + " / " + PLList.Count + "件";
                for (int i = 0; i < SelectedPlayListFiles.Count; ++i) {
                    PlayListModel oneItem = SelectedPlayListFiles[i];
                    dbMsg += "\r\n[" + i + "]" + oneItem.UrlStr;
                    PLList.Remove(oneItem);
                }
                RaisePropertyChanged("PLList");
                IsDoSavePlayList(true);
                ListItemCount = PLList.Count();
                RaisePropertyChanged("ListItemCount");
                dbMsg += ">> " + ListItemCount + "件";
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        #region プレイリストのコンテキストメニュー
        public ContextMenu PlayListMenu { get; set; }
        public MenuItem PlayListItemViewExplore;
        public MenuItem PlayListItemMove;
        public MenuItem PlayListItemMoveTop;
        public MenuItem PlayListItemMoveBottom;
        public MenuItem PlayListItemCopyOtherList;
        public MenuItem PlayListItemMoveOtherList;
        public MenuItem PlayListDeleteCannotRead;
        public MenuItem PlayListDeleteDoubling;
        public MenuItem PlayListDeleteFrontDoubling;
        public MenuItem PlayListDeleteAfterDoubling;
        public MenuItem PlayListItemRemove;
        public MenuItem PlayListSaveRoot;
        public MenuItem PlayListSaveMenu;
        public MenuItem PlayListSaveAsMenu;
        /// <summary>
        /// コンボボックスにコンテキストメニューを追加する
        /// </summary>
        public void MakePlayListMenu() {
            string TAG = "MakePlayListoMenu";
            string dbMsg = "";
            try {
                //  dbMsg += ",PLComboSelectedIndex=" + PLComboSelectedIndex;
                PlayListMenu = new ContextMenu();

                PlayListItemViewExplore = new MenuItem();
                PlayListItemViewExplore.Header = "エクスプローラーで開く";
                //コンテキストメニュー表示時に発生するイベントを追加
                PlayListItemViewExplore.Click += PlayListItemViewExplore_Click;
                PlayListMenu.Items.Add(PlayListItemViewExplore);

                PlayListItemMove = new MenuItem();
                PlayListItemMove.Header = "移動...";

                PlayListItemMoveTop = new MenuItem();
                PlayListItemMoveTop.Header = "先頭へ移動";
                PlayListItemMoveTop.Click += PlayListItemMoveTop_Click;
                PlayListItemMove.Items.Add(PlayListItemMoveTop);

                PlayListItemMoveBottom = new MenuItem();
                PlayListItemMoveBottom.Header = "末尾へ移動";
                PlayListItemMoveBottom.Click += PlayListItemMoveBottom_Click;
                PlayListItemMove.Items.Add(PlayListItemMoveBottom);

                PlayListItemCopyOtherList = new MenuItem();
                PlayListItemCopyOtherList.Header = "他のリストへコピー";
                PlayListItemCopyOtherList.Click += PlayListItemCopyOtherListm_Click;
                PlayListItemMove.Items.Add(PlayListItemCopyOtherList);

                PlayListItemMoveOtherList = new MenuItem();
                PlayListItemMoveOtherList.Header = "他のリストへ移動";
                PlayListItemMoveOtherList.Click += PlayListItemMoveOtherListm_Click;
                PlayListItemMove.Items.Add(PlayListItemMoveOtherList);
                //サブメニューに追加
                PlayListMenu.Items.Add(PlayListItemMove); 

                PlayListDeleteCannotRead = new MenuItem();
                PlayListDeleteCannotRead.Header = "読めないアイテムを削除";
                PlayListDeleteCannotRead.Click += PlayListDeleteCannotRead_Click;
                PlayListMenu.Items.Add(PlayListDeleteCannotRead);

                PlayListDeleteDoubling = new MenuItem();
                PlayListDeleteDoubling.Header = "重複アイテムを削除...";

                PlayListDeleteFrontDoubling = new MenuItem();
                PlayListDeleteFrontDoubling.Header = "先頭側";
                PlayListDeleteFrontDoubling.Click += PlayListDeleteFrontDoubling_Click;
                PlayListDeleteDoubling.Items.Add(PlayListDeleteFrontDoubling);

                PlayListDeleteAfterDoubling = new MenuItem();
                PlayListDeleteAfterDoubling.Header = "末尾側";
                PlayListDeleteAfterDoubling.Click += PlayListDeleteAfterDoubling_Click;
                PlayListDeleteDoubling.Items.Add(PlayListDeleteAfterDoubling);
                PlayListMenu.Items.Add(PlayListDeleteDoubling);

                PlayListItemRemove = new MenuItem();
                PlayListItemRemove.Header = "選択しているアイテムを削除";
                PlayListItemRemove.Click += PlayListItemRemove_Click;
                PlayListMenu.Items.Add(PlayListItemRemove);

                PlayListSaveRoot = new MenuItem();
                PlayListSaveRoot.Header = "このプレイリストを保存...";

                PlayListSaveMenu = new MenuItem();
                PlayListSaveMenu.Header = "上書き";
                PlayListSaveMenu.Click += PlayListSaveMenu_Click;
                PlayListSaveRoot.Items.Add(PlayListSaveMenu);

                PlayListSaveAsMenu = new MenuItem();
                PlayListSaveAsMenu.Header = "名前を付けて保存";
                PlayListSaveAsMenu.Click += PlayListSaveAsMenu_Click;
                PlayListSaveRoot.Items.Add(PlayListSaveAsMenu);
                PlayListMenu.Items.Add(PlayListSaveRoot);

                RaisePropertyChanged("PlayListMenu");
                MyLog(TAG, dbMsg);
                //  Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// プレイリストで選択したアイテムをエクスプローラーで開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// https://dobon.net/vb/dotnet/process/openexplore.html
		private void PlayListItemViewExplore_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListItemViewExplore_Click";
            string dbMsg = "";
            try {
                string titolStr = "プレイリストアイテムファイルの操作";
                string errorStr = "操作";
                string? doStr = null;
                if (!PlayListOpelate(titolStr, errorStr, doStr)) {
                    return;
                }

                BeforSelectListUrl = NowSelect.PlayListUrlStr;
                NowSelectedFile = PLListSelectedItem.UrlStr;
                //flasfでNull
                dbMsg += "[" + SelectedPlayListIndex + "]" + NowSelectedFile;
                string[] Strs = NowSelectedFile.Split('/');
                if (NowSelectedFile.Contains('/')) {
                } else if (NowSelectedFile.Contains(Path.DirectorySeparatorChar)) {
                    Strs = NowSelectedFile.Split(Path.DirectorySeparatorChar);
                }
                dbMsg += "," + Strs.Length + "階層";
                string fileNameStr = Strs[Strs.Length - 1];
                dbMsg += ",fileNameStr=" + fileNameStr;
                string pathStr = NowSelectedFile.Remove(NowSelectedFile.Length - fileNameStr.Length - 1);
                pathStr = pathStr.Replace("file://", "");
                pathStr = pathStr.Replace("///", "/");
                pathStr = pathStr.Replace("//", "/");
                pathStr = pathStr.Replace('/', Path.DirectorySeparatorChar);
                dbMsg += ">>" + pathStr + " の " + fileNameStr;
				//       MyView.PlayList2Explore(pathStr);
				//https://dobon.net/vb/dotnet/process/openfile.html
				//Processオブジェクトを作成する
				pEXPLORER = new System.Diagnostics.Process();
				//起動するファイルを指定する
				pEXPLORER.StartInfo.FileName = "EXPLORER.exe";
				pEXPLORER.StartInfo.Arguments = pathStr;
                //イベントハンドラがフォームを作成したスレッドで実行されるようにする
                //         pEXPLORER.SynchronizingObject = (ISynchronizeInvoke?)this;
                //		//Unable to cast object of type 'M3UPlayer.Views.MainWindow' to type 'System.ComponentModel.ISynchronizeInvoke'.
                //イベントハンドラの追加
                pEXPLORER.Disposed += new EventHandler(EXPLORER_Exited);
				//プロセスが終了したときに Exited イベントを発生させる
				pEXPLORER.EnableRaisingEvents = true;
				//起動する
				pEXPLORER.Start();
				////pEXPLORER = Process.Start("EXPLORER.EXE", pathStr);
				//dbMsg += ",pEXPLORER[" + pEXPLORER.Id + "]start" + pEXPLORER.StartTime.ToString("HH:mm:ss.fff") + ",Arguments=" + pEXPLORER.StartInfo.Arguments;
				//pEXPLORER.WaitForExit();
				//dbMsg += ">>Exit" + pEXPLORER.ExitTime.ToString("HH:mm:ss.fff") + ",Arguments=" + pEXPLORER.StartInfo.Arguments;
				//dbMsg += ">>ExitCode=" + pEXPLORER.ExitCode.ToString();
				//pEXPLORER.Kill();
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

		private void EXPLORER_Exited(object sender, EventArgs e) {
			string TAG = "EXPLORER_Exited";
			string dbMsg = "";
			try {
				Process process = (Process)sender;
				dbMsg += ">>Exit" + process.ExitTime.ToString("HH:mm:ss.fff") + ",Arguments=" + process.StartInfo.Arguments;
				dbMsg += ">>ExitCode=" + process.ExitCode.ToString();

				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
		}



		/// <summary>
		/// 先頭へ移動
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayListItemMoveTop_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListItemMoveTop_Click";
            string dbMsg = "";
            try {
                string titolStr = "プレイリストのアイテム移動";
                string errorStr = "移動";
                string doStr = "をリスト先頭に移動しますか？";
                if (!PlayListOpelate(titolStr, errorStr, doStr)) {
                    return;
                }

                NowSelectedFile = PLListSelectedItem.UrlStr;
                dbMsg += "urlStr=" + NowSelectedFile;
                int oldIndex = PLList.IndexOf(PLListSelectedItem);
                dbMsg += ",oldIndex=" + oldIndex;
                PLList.Move(oldIndex, 0);
                IsDoSavePlayList(false);
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 末尾へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void PlayListItemMoveBottom_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListItemMoveBottom_Click";
            string dbMsg = "";
            try {
                string titolStr = "プレイリストのアイテム移動";
                string errorStr = "移動";
                string doStr = "をリスト末尾に移動しますか？";
                if (!PlayListOpelate(titolStr, errorStr, doStr)) {
                    return;
                }
                NowSelectedFile = PLListSelectedItem.UrlStr;
                dbMsg += "urlStr=" + NowSelectedFile;
                int oldIndex = PLList.IndexOf(PLListSelectedItem);
                dbMsg += ",oldIndex=" + oldIndex;
                PLList.Move(oldIndex, PLList.Count - 1);
                IsDoSavePlayList(false);
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 他のリストへコピー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayListItemCopyOtherListm_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListItemCopyOtherListm_Click";
            string dbMsg = "";
            try {
                string titolStr = "プレイリストのアイテムコピー";
                string errorStr = "移動";
                string doStr = "を他のリストにコピーしますか？";
                if (!PlayListOpelate(titolStr, errorStr, doStr)) {
                    return;
                }
                //切り替える前
                BeforSelectListUrl = NowSelect.PlayListUrlStr;
                MoveBeforSelect = new SelectionModel();
                MoveBeforSelect = NowSelect;
                dbMsg += "," + MoveBeforSelect.PlayListUrlStr + " の[ "+ MoveBeforSelect.SelectedIndex + "]"+ MoveBeforSelect.ListItem.UrlStr;

                NowSelectedFile = PLListSelectedItem.UrlStr;
                dbMsg += "=" + NowSelectedFile;
                // ダイアログのインスタンスを生成
                ofDialog = new OpenFileDialog();
                // ファイルの種類を設定
                ofDialog.Filter = "プレイリスト (*.m3u*)|*.m3u*|全てのファイル (*.*)|*.*";
                //② デフォルトのフォルダを指定する
                if (CurrentPlayListFileName.Equals("")) {
                    CurrentPlayListFileName = "C;";
                }
                if (NowSelectedPath == null || NowSelectedPath.Equals("")) {
                    NowSelectedPath = Path.GetDirectoryName(CurrentPlayListFileName);
                }
                dbMsg += ",CurrentPlayListFileName=" + CurrentPlayListFileName;
                ofDialog.InitialDirectory = Path.GetDirectoryName(CurrentPlayListFileName);
                //③ダイアログのタイトルを指定する
                ofDialog.Title = "ファイル選択";
                //ダイアログを表示する
                if (ofDialog.ShowDialog() == true) {
					string selectListFile = ofDialog.FileName;
                    //usingで中の処理が終わったタイミングでファイルを保存する
                    using (var writer=new StreamWriter(selectListFile,true)) {
                        //末尾行に追記する
                        writer.WriteLine(NowSelectedFile);
                    }
                    //usingを使わない例: File.AppendAllText(selectListFile, NowSelectedFile);

                    CurrentPlayListFileName = selectListFile;
                    RaisePropertyChanged("CurrentPlayListFileName");
                    dbMsg += ">>" + CurrentPlayListFileName;
                    if (PLComboSource.ContainsKey(CurrentPlayListFileName)) {
                        dbMsg += "追加済み;";
                    } else {
                        AddPlayListCombo(CurrentPlayListFileName);
                        //         listIndex = 0;
                        PlayListStr = "";
						foreach (var PLComboItem in PLComboSource) {
                            PlayListStr += PLComboItem.Key + ",";
                        }
                        PlayListStr = PlayListStr.Substring(0, PlayListStr.Length - 1);
                        dbMsg += ",PlayListStr=" + PlayListStr;
                        PlayLists = PlayListStr.Split(','); 
                        dbMsg +=  ":追加";
                    }
                    int listIndex = Array.IndexOf(PlayLists, selectListFile);
                    dbMsg += "[" + listIndex + "/" + PlayLists.Length + "]" + selectListFile + "=" + CurrentPlayListFileName;
					PLComboSelectedIndex = listIndex;

					NowSelectedPath = System.IO.Path.GetDirectoryName(CurrentPlayListFileName);
					dbMsg += ">>NowSelectedPath=" + NowSelectedPath;
					RaisePropertyChanged("NowSelectedPath");
					//設定ファイル更新
					Properties.Settings.Default.Save();
                } else {
                    dbMsg += "キャンセルされました";
                }

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 他のリストへ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayListItemMoveOtherListm_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListItemMoveOtherListm_Click";
            string dbMsg = "";
            try {
                //string titolStr = "プレイリストのアイテム移動";
                //string errorStr = "移動";
                //string doStr = "を他のリストに移動しますか？";
                //if (!PlayListOpelate(titolStr, errorStr, doStr)) {
                //    return;
                //}
                //切り替える前
                BeforSelectListUrl = NowSelect.PlayListUrlStr;
                MoveBeforSelect = new SelectionModel();
                MoveBeforSelect = NowSelect;
                int BeforSelectIndex = NowSelect.SelectedIndex;
                dbMsg += "," + MoveBeforSelect.PlayListUrlStr + " の[ " + MoveBeforSelect.SelectedIndex + "]" + MoveBeforSelect.ListItem.UrlStr;

                NowSelectedFile = PLListSelectedItem.UrlStr;
                dbMsg += "=" + NowSelectedFile;
                // ダイアログのインスタンスを生成
                ofDialog = new OpenFileDialog();
                // ファイルの種類を設定
                ofDialog.Filter = "プレイリスト (*.m3u*)|*.m3u*|全てのファイル (*.*)|*.*";
                //② デフォルトのフォルダを指定する
                if (CurrentPlayListFileName.Equals("")) {
                    CurrentPlayListFileName = "C;";
                }
                if (NowSelectedPath == null || NowSelectedPath.Equals("")) {
                    NowSelectedPath = Path.GetDirectoryName(CurrentPlayListFileName);
                }
                dbMsg += ",CurrentPlayListFileName=" + CurrentPlayListFileName;
                ofDialog.InitialDirectory = Path.GetDirectoryName(CurrentPlayListFileName);
                //③ダイアログのタイトルを指定する
                ofDialog.Title = "ファイル選択";
                //ダイアログを表示する
                if (ofDialog.ShowDialog() == true) {
                    string selectListFile = ofDialog.FileName;
                    //usingで中の処理が終わったタイミングでファイルを保存する
                    using (var writer = new StreamWriter(selectListFile, true)) {
                        //末尾行に追記する
                        writer.WriteLine(NowSelectedFile);
                    }
                    //usingを使わない例: File.AppendAllText(selectListFile, NowSelectedFile);
                    //ここから削除
                    dbMsg += "\r\n削除前[" + BeforSelectIndex + "/" + PLList.Count + "]";
                    PLList.RemoveAt(BeforSelectIndex);
                    MoveBeforSelect.SelectedIndex--;
                    if (MoveBeforSelect.SelectedIndex < 0) {
                        MoveBeforSelect.SelectedIndex = 0;

                    }
					dbMsg += ">削除後>"+ MoveBeforSelect.PlayListUrlStr + "の[" + MoveBeforSelect.SelectedIndex + "/" + PLList.Count + "]";
                    string text = "";
                    foreach (PlayListModel One in PLList) {
                        text += One.UrlStr + "\r\n";
                    }
                    StreamWriter sw = new StreamWriter(CurrentPlayListFileName, false, Encoding.UTF8);
                    sw.Write(text);
                    sw.Close();
					//削除ここまで//移動先に表示を切り替える
					string titolStr = "";
					string doStr = "プレイリストのアイテムを" + selectListFile + " の " + NowSelectedFile + "移動しました\r\n移動先のリストに切り替えますか？";
					MessageBoxResult result = MessageShowWPF(titolStr, doStr, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                    dbMsg += ",result=" + result;
                    if (result.Equals(MessageBoxResult.OK)) {
                        PlayListItemSelect(selectListFile, NowSelectedFile);
                    }
                } else {
                    dbMsg += "キャンセルされました";
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        /// <summary>
        /// 読めないファイルを削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void PlayListDeleteCannotRead_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListDeleteCannotRead_Click";
            string dbMsg = "";
            try {

                dbMsg += "ListItemCount=" + ListItemCount + "件";
                progTitol = "読めないファイルを削除";
                // 複数スレッドで使用されるコレクションへの参加
                BindingOperations.EnableCollectionSynchronization(PLList, new object());
                pd = new ProgressDialog(PDVM, async () => {
                    PDVM.IntProgress(progTitol, PLList.Count, 1);
                    dbMsg += "," + PDVM.PrgTitle;
                    dbMsg += ",PrgMax=" + PDVM.PrgMax;
                    ObservableCollection<PlayListModel> DeleteList = new ObservableCollection<PlayListModel>();
                    int lCount = 0;
                    foreach (PlayListModel item in PLList) {
                        if (File.Exists(item.UrlStr)) {
                        } else {
                            dbMsg += "不在=" + item.UrlStr;
                            DeleteList.Add(item);
                        }
                        lCount++;
                        PDVM.DoProgress(lCount, item.Summary + "");
                        dbMsg += "\r\n[" + PDVM.PrgVal + "/" + PDVM.PrgMax + "]" + PDVM.PrgStatus;
                    }
                    dbMsg += ">削除待ち>" + DeleteList.Count + "件";
                    if (0 < DeleteList.Count) {
                        foreach (PlayListModel item in DeleteList) {
                            PLList.Remove(item);
                        }
                        RaisePropertyChanged("PLList");
                    }
                    IsDoSavePlayList(true);
                }, cancelToken);

                pd.ShowDialog();
                if (pd.IsCanceled) {
                    MessageBox.Show("キャンセルしました", progTitol, MessageBoxButton.OK);
                } else {
                    dbMsg += ",完了しました";
                }

                ListItemCount = PLList.Count();
                RaisePropertyChanged("ListItemCount");
                dbMsg += ">>" + ListItemCount + "件";

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 先頭側の重複アイテムを削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void PlayListDeleteFrontDoubling_Click(object sender, RoutedEventArgs e) {
            string TAG = "DeletePlayListComboItem";
            string dbMsg = "";
            try {
                dbMsg += "ListItemCount=" + ListItemCount + "件";
                progTitol = "先頭側の重複アイテムを削除";
                BindingOperations.EnableCollectionSynchronization(PLList, new object());
                pd = new ProgressDialog(PDVM, async () => {
                    PDVM.IntProgress(progTitol, PLList.Count, 1);
                    dbMsg += "," + PDVM.PrgTitle;
                    dbMsg += ",PrgMax=" + PDVM.PrgMax;
                    ObservableCollection<PlayListModel> DeleteList = new ObservableCollection<PlayListModel>();
                    int lCount = 0;
                    foreach (PlayListModel checkItem in PLList) {
                        int checkIndex = PLList.IndexOf(checkItem);
                        foreach (PlayListModel tItem in PLList) {
                            int tIndex = PLList.IndexOf(tItem);
                            if (checkIndex < tIndex) {
                                if (tItem.UrlStr.Equals(checkItem.UrlStr)) {
                                    dbMsg += ">重複>=" + tItem.UrlStr;
                                    checkItem.ActionFlag = true;
                                    DeleteList.Add(checkItem);
                                }
                            }
                        }
                        lCount++;
                        PDVM.DoProgress(lCount, checkItem.Summary + "");
                        dbMsg += "\r\n[" + PDVM.PrgVal + "/" + PDVM.PrgMax + "]" + PDVM.PrgStatus;
                    }
                    DelCount = DeleteList.Count;
                    dbMsg += ">削除待ち>" + DelCount + "件";
                    if (0 < DelCount) {
                        foreach (PlayListModel item in DeleteList) {
                            PLList.Remove(item);
                        }
                        RaisePropertyChanged("PLList");
                    }
                    IsDoSavePlayList(false);
                }, cancelToken);

                pd.ShowDialog();
                if (pd.IsCanceled) {
                    MessageBox.Show("キャンセルしました", progTitol, MessageBoxButton.OK);
                } else if (0 < DelCount) {
                    MessageBox.Show(DelCount + "件削除しました", progTitol, MessageBoxButton.OK);
                    DelCount = 0;
                } else {
                    MessageBox.Show("重複はありませんでした", progTitol, MessageBoxButton.OK);
                }
                ListItemCount = PLList.Count();
                RaisePropertyChanged("ListItemCount");
                dbMsg += ">>" + ListItemCount + "件";
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 末尾側の重複アイテムを削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void PlayListDeleteAfterDoubling_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListDeleteAfterDoubling_Click";
            string dbMsg = "";
            try {
                dbMsg += "ListItemCount=" + ListItemCount + "件";
                progTitol = "末尾側の重複アイテムを削除";
                BindingOperations.EnableCollectionSynchronization(PLList, new object());
                pd = new ProgressDialog(PDVM, async () => {
                    PDVM.IntProgress(progTitol, PLList.Count, 1);
                    dbMsg += "," + PDVM.PrgTitle;
                    dbMsg += ",PrgMax=" + PDVM.PrgMax;
                    ObservableCollection<PlayListModel> DeleteList = new ObservableCollection<PlayListModel>();
                    int lCount = 0;
                    foreach (PlayListModel checkItem in PLList) {
                        int checkIndex = PLList.IndexOf(checkItem);
                        foreach (PlayListModel tItem in PLList) {
                            int tIndex = PLList.IndexOf(tItem);
                            if (checkIndex < tIndex) {
                                if (tItem.UrlStr.Equals(checkItem.UrlStr)) {
                                    dbMsg += ">重複>=" + tItem.UrlStr;
                                    tItem.ActionFlag = true;
                                    DeleteList.Add(tItem);
                                }
                            }
                        }
                        lCount++;
                        PDVM.DoProgress(lCount, checkItem.Summary + "");
                        dbMsg += "\r\n[" + PDVM.PrgVal + "/" + PDVM.PrgMax + "]" + PDVM.PrgStatus;
                    }
                    DelCount = DeleteList.Count;
                    dbMsg += ">削除待ち>" + DelCount + "件";
                    if (0 < DelCount) {
                        while (0 < DeleteList.Count) {
                            PLList.Remove(DeleteList[0]);
                            DeleteList.Remove(DeleteList[0]);
                        }
                        RaisePropertyChanged("PLList");
                    }
                    IsDoSavePlayList(false);
                }, cancelToken);

                pd.ShowDialog();
                if (pd.IsCanceled) {
                    MessageBox.Show("キャンセルしました", progTitol, MessageBoxButton.OK);
                } else if (0 < DelCount) {
                    MessageBox.Show(DelCount + "件削除しました", progTitol, MessageBoxButton.OK);
                    DelCount = 0;
                } else {
                    MessageBox.Show("重複はありませんでした", progTitol, MessageBoxButton.OK);
                }
                ListItemCount = PLList.Count();
                RaisePropertyChanged("ListItemCount");
                dbMsg += ">>" + ListItemCount + "件";
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 選択しているアイテムを削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayListItemRemove_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListItemRemove_Click";
            string dbMsg = "";
            try {
                PlayListItemDelete();
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 上書きでこのプレイリストを保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void PlayListSaveMenu_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListSaveMwnu_Click";
            string dbMsg = "";
            try {
                SavePlayList();
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 名前を付けてこのプレイリストを保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void PlayListSaveAsMenu_Click(object sender, RoutedEventArgs e) {
            string TAG = "PlayListSaveAsMenu_Click";
            string dbMsg = "";
            try {
                string oldPlayListFileName = CurrentPlayListFileName;
                dbMsg += "[" + PLComboSelectedIndex + " / " + PLComboSource.Count() + "]" + oldPlayListFileName;
                dbMsg += "\r\n" + ListItemCount + "件";
                string text = "";           //プレイリスト内の要素
                foreach (PlayListModel One in PLList) {
                    text += One.UrlStr + "\r\n";                    //プレイリストはurlのみ
                }

                SaveFileDialog dialog = new SaveFileDialog() {
                    Title = "プレイリストの複製",
                    InitialDirectory = System.IO.Path.GetDirectoryName(CurrentPlayListFileName),
                    FileName = System.IO.Path.GetFileNameWithoutExtension(CurrentPlayListFileName) + "のコピー",
                    DefaultExt = ".m3u8",

                    AddExtension = true,        // ユーザーが拡張子を省略したときに、自動的に拡張子を付けるか。規定値はtrue。
                    CheckFileExists = false,    // ユーザーが存在しないファイルを指定したときに、警告するか。規定値はfalse。
                    CheckPathExists = true,     // ユーザーが存在しないパスを指定したときに、警告するか。規定値はtrue。
                    CreatePrompt = false,       // ユーザーが存在しないファイルを指定したときに、作成の許可を求めるか。規定値はfalse。
                    CustomPlaces = null,        // ダイアログ左側のショートカットのリスト。
                    DereferenceLinks = false,   // ショートカットが参照先を返す場合はtrue。リンクファイルを返す場合はfalse。規定値はfalse。
                    Filter = "プレイリスト (*.m3u*)|*.m3u*|すべて (*.*)|*.*",      // ダイアログで表示するファイルの種類のフィルタを指定する文字列。
                    FilterIndex = 1,            // 選択されたFilterのインデックス。規定値は1。
                    OverwritePrompt = true,     // 存在するファイルを指定したときに、警告するか。規定値はtrue。
                    ValidateNames = true,       // ファイル名がWin32に適合するか検査するかどうか。規定値はfalse。
                };


                if (true == dialog.ShowDialog()) {
                    string newFileFullName = dialog.FileName;
                    dbMsg += ">>" + newFileFullName;
                    string fName = dialog.SafeFileName;              //System.IO.Path.GetFileName(CurrentPlayListFileName);
                    dbMsg += "と" + fName;
                    File.WriteAllText(newFileFullName, text, Encoding.UTF8);

                    //using (Stream fileStream = dialog.OpenFile())
                    //using (StreamWriter sr = new StreamWriter(fileStream)) {
                    //	sr.Write(text);
                    //}
                    //　コンボボックスデータの更新
                    ReplacePlayListComboItem(PLComboSelectedIndex, newFileFullName);
                    PlayListComboSelected = fName;
                    RaisePropertyChanged("PlayListComboSelected");
                    CurrentPlayListFileName = newFileFullName;
                    RaisePropertyChanged("CurrentPlayListFileName");
                    NowSelectedPath = System.IO.Path.GetDirectoryName(CurrentPlayListFileName);
                    RaisePropertyChanged("NowSelectedPath");
                    dbMsg += ">>NowSelectedPath=" + NowSelectedPath;
                    //設定ファイル更新
                    Properties.Settings.Default.Save();
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        #endregion


        #endregion
        //ファイル選択///////////////////////////////////////////////////////////playList//
        //        //プレイリスト///////////////////////////////////////////////////////////FileListVewの操作//
        public OpenFileDialog? ofDialog;
        public CommonOpenFileDialog? cofDialog;

        #region FileDlogShow	　単一ファイルの選択
        public ICommand FileDlogShow => new DelegateCommand(ShowFileDlog);
        /// <summary>
        /// 単一ファイルの選択ダイアログから選択されたファイルをPLリストもしくは現在のプレイリストに追加する
        /// </summary>
        /// https://johobase.com/wpf-file-folder-common-dialog/
        public void ShowFileDlog() {
            string TAG = "File_bt_Click";
            string dbMsg = "";
            try {

                //         string SelectFileName = "";
                //①
                // ダイアログのインスタンスを生成
                ofDialog = new OpenFileDialog();
                // ファイルの種類を設定
                ofDialog.Filter = "全てのファイル (*.*)|*.*|ムービー (*.mp4)|*.mp4|プレイリスト (*.m3u*)|*.m3u*";
                //② デフォルトのフォルダを指定する
                if (CurrentPlayListFileName.Equals("")) {
                    CurrentPlayListFileName = "C;";
                }
                if (NowSelectedPath == null || NowSelectedPath.Equals("")) {
                    NowSelectedPath = System.IO.Path.GetDirectoryName(CurrentPlayListFileName);
                }
                dbMsg += ",NowSelectedPath=" + NowSelectedPath;
                ofDialog.InitialDirectory = @NowSelectedPath;
                //③ダイアログのタイトルを指定する
                ofDialog.Title = "ファイル選択";
                //ダイアログを表示する
                if (ofDialog.ShowDialog() == true) {
                    NowSelectedFile = ofDialog.FileName;
                    dbMsg += ">>" + NowSelectedFile;
                    NowSelectedPath = System.IO.Path.GetDirectoryName(NowSelectedFile);
                    dbMsg += ">>NowSelectedPath=" + NowSelectedPath;
                    RaisePropertyChanged("NowSelectedPath");
                    //設定ファイル更新
                    Properties.Settings.Default.Save();
                    string extention = System.IO.Path.GetExtension(NowSelectedFile);
                    if (extention.Contains("m3u")) {
                        dbMsg += "PLListに追加";
                        AddPlayListCombo(NowSelectedFile);
                    } else {
                        dbMsg += "現在のプレイリストの先頭に追加";
                        if (AddToPlayList(NowSelectedFile, 0)) {
                            //				SavePlayList();
                        }
                    }
                } else {
                    dbMsg += "キャンセルされました";
                }
                // オブジェクトを破棄する
                //		ofDialog.Dispose();

                if (!NowSelectedFile.Equals("")) {
                    string[] files = { NowSelectedFile };
                    //         FilesFromLocal(files);
                }
                ofDialog = null;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }
        #endregion

        #region FolderDlogShow	　フォルダ選択
        public ICommand FolderDlogShow => new DelegateCommand(ShowFolderDlog);
        /// <summary>
        /// フォルダ選択ダイアログから選択されたフォルダのファイルリストをファイルをアイコン化処理に渡す
        /// https://johobase.com/file-folder-common-dialog/
        /// </summary>
        private void ShowFolderDlog() {
            string TAG = "Folder_bt_Click";
            string dbMsg = "";
            try {
                // ダイアログのインスタンスを生成
                cofDialog = new CommonOpenFileDialog("フォルダーの選択");

                // 選択形式をフォルダースタイルにする IsFolderPicker プロパティを設定
                cofDialog.IsFolderPicker = true;

                // ダイアログを表示
                if (cofDialog.ShowDialog() == CommonFileDialogResult.Ok) {
					int count = 0;
					//    MessageBox.Show(dialog.FileName);
					NowSelectedPath = cofDialog.FileName;              //fbDialog.SelectedPath;
                    dbMsg += ">>" + NowSelectedPath;
					//フォルダ内のファイルを読み込む※順番は保証されない
					string[] files = System.IO.Directory.GetFiles(@NowSelectedPath, "*", SearchOption.AllDirectories);
					dbMsg += ">>" + files.Length + "件";

					//CultureInfoを作成
					System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");
					//StringComparerを作成、大文字小文字を区別しない
					StringComparer cmp = StringComparer.Create(ci, true);

					//並び替える
					Array.Sort(files, cmp);
					foreach (string file in files) {
						dbMsg += "\r\n[" + count + "]" + file;
                        count++;
					}

					FilesAdd(files, 0);
					/*
					DirectoryInfo dir = new DirectoryInfo(@NowSelectedPath);
					FileInfo[] files = dir.GetFiles();
					//FileInfo[] files =new FileInfo[];
					//foreach (FileInfo fi in dir.GetFiles()) {
					//    files[count] = fi;
					//    dbMsg += "\r\n[" + count + "]" + files[count];
					//    count++;
					//}
					dbMsg += ">>" + files.Length + "件";
					Array.Sort<FileInfo>(files, delegate (FileInfo a, FileInfo b) {
						// ファイルサイズで昇順ソート
						// return (int)(a.Length - b.Length);

						// ファイル名でソート
						return a.Name.CompareTo(b.Name);
					});
					//設定ファイル更新
					Properties.Settings.Default.Save();
					count = 0;
					string[] readfiles = new string[files.Length];
					foreach (FileInfo file in files) {
						readfiles[count] = file.Name;
						dbMsg += "\r\n[" + count + "]" + readfiles[count];
                        count++;
					}
					FilesAdd(readfiles, 0);
                    */
					IsDoSavePlayList(false);
                } else {
                    dbMsg += "キャンセルされました";
                }
                // オブジェクトを破棄する
                //        dialog.Dispose();
                cofDialog = null;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }
        #endregion


        #region ExploreShow	　エクスプローラー表示
        //private ViewModelCommand _ExploreShow;

        //public ViewModelCommand ExploreShow
        //{
        //    get {
        //        if (_ExploreShow == null)
        //        {
        //            _ExploreShow = new ViewModelCommand(ShowExplore);
        //        }
        //        return _ExploreShow;
        //    }
        //}
        /// <summary>
        /// エクスプローラで作業対象フォルダを表示する
        /// </summary>
        //private void ShowExplore() {
        //    string TAG = "ShowExplore";
        //    string dbMsg = "";
        //    try {
        //        //最近表示した場所	をシェルなら
        //        //	explorer.exe shell:::{ 22877A6D - 37A1 - 461A - 91B0 - DBDA5AAEBC99}
        //        pEXPLORER = System.Diagnostics.Process.Start("EXPLORER.EXE", @"{ 22877A6D - 37A1 - 461A - 91B0 - DBDA5AAEBC99}");
        //        MyLog(TAG, dbMsg);
        //    } catch (Exception er) {
        //        MyErrorLog(TAG, dbMsg, er);
        //    }
        //}
        #endregion

        /// <summary>
        /// プレイヤーコントロール　//////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// http://memopad.bitter.jp/w3c/html5/html5_video_dom.html
        public ICommand ControlHide => new DelegateCommand(HideControl);
        /// <summary>
        /// プレイヤーコントロールの表示/非表示
        /// </summary>
        public async void HideControl() {
            string TAG = "HideControl";
            string dbMsg = "";
            try {
                dbMsg += "IsHideControl=" + IsHideControl;
                if (IsHideControl) {
                    IsHideControl = false;
                } else {
                    IsHideControl = true;
                }
                dbMsg += ">>=" + IsHideControl;
                RaisePropertyChanged("IsHideControl");
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        public ICommand PlayBtClick => new DelegateCommand(ClickPlayBt);
 
        /// <summary>
        /// Playボタンのクリック
        /// </summary>
        public void ClickPlayBt() {
            string TAG = "ClickPlayBt";
            string dbMsg = "";
            try {
                dbMsg += "IsPlaying=" + IsPlaying;
                if (IsPlaying) {
                    IsPlaying = false;
                } else {
                    IsPlaying = true;
                }
                RaisePropertyChanged("IsPlaying");
                dbMsg += ">>IsPlaying=" + IsPlaying;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// pauseにするだけ(Thumb.DragStarted)
        /// </summary>
        public void PauseVideo() {
            string TAG = "PauseVideo";
            string dbMsg = "";
            try {
                dbMsg += ",SliderValue=" + SliderValue;
                dbMsg += ",IsPlaying=" + IsPlaying;
                if (IsPlaying) {
                    IsPlaying = false;
                    RaisePropertyChanged("IsPlaying");
                }
                dbMsg += ">>=" + IsPlaying;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        //     public ICommand FFCBChanged => new DelegateCommand(ClickForwardAsync);
        public ICommand FFBtClick => new DelegateCommand(ClickForwardAsync);
        /// <summary>
        /// 送りコンボのクリック
        /// </summary>
        public async void ClickForwardAsync() {
            string TAG = "ClickForwardAsync";
            string dbMsg = "";
            try {
                dbMsg += "ForwardCBComboSelected=" + ForwardCBComboSelected;
                double newPosition = SliderValue + double.Parse(ForwardCBComboSelected);
                dbMsg += ">>" + newPosition;
                if (SliderMaximum < newPosition) {
                    double difference = (SliderMaximum - SliderValue) / 2;
                    dbMsg += ">修正>" + difference;
                    newPosition = SliderMaximum - difference;
                    dbMsg += ">>" + newPosition;
                }
                dbMsg += " / " + SliderMaximum;
                IsPlaying = false;
                RaisePropertyChanged("IsPlaying");
                if (toWeb) {
                    await MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").currentTime=" + "'" + newPosition + "'" + ";");
                } else if (axWmp != null) {
                    axWmp.Ctlcontrols.currentPosition = newPosition;
                }
                IsPlaying = true;
                RaisePropertyChanged("IsPlaying");
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        public ICommand RewBtClick => new DelegateCommand(ClickRewAsync);
        /// <summary>
        /// 戻りコンボのクリック
        /// </summary>
        public async void ClickRewAsync() {
            string TAG = "ClickRewAsync";
            string dbMsg = "";
            try {
                dbMsg += "RewCBComboSelected=" + RewCBComboSelected;
                double newPosition = SliderValue - double.Parse(RewCBComboSelected);
                dbMsg += ">>" + newPosition;
                if (newPosition < 0) {
                    double difference = SliderValue / 2;
                    dbMsg += ">修正>" + difference;
                    newPosition = difference;
                    dbMsg += ">>" + newPosition;
                }
                dbMsg += " / " + SliderMaximum;
                IsPlaying = false;
                RaisePropertyChanged("IsPlaying");
                switch (movieType) {
                    case 0:
                        await MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").currentTime=" + "'" + newPosition + "'" + ";");
                        break;
                    case 1:
                        if (axWmp != null) {
                            axWmp.Ctlcontrols.currentPosition = newPosition;
                        }
                        break;
                    case 2:
                        if (flash != null) {
                            //flash.SetVariable("isPlay", "true");
                            //flash.Play();
                        }
                        break;
                }
                IsPlaying = true;
                RaisePropertyChanged("IsPlaying");
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 再生ポジションスライダー操作中
        /// </summary>
   //     public bool isPSLDrag=false;


        /// <summary>
        /// 再生ポジションスライダーの Thumb 位置変更
        /// </summary>
        public async void PositionSliderValueChang(double newPosition) {
            string TAG = "PositionSliderValueChang";
            string dbMsg = "";
            try {
                dbMsg += ",SliderValue=" + SliderValue;
                dbMsg += ">newPosition>" + newPosition;
                SliderValue = newPosition;
                RaisePropertyChanged("SliderValue");
                switch (movieType) {
                    case 0:
                        //await Task.Run(() => {の中では設定できなかった
                        await MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").currentTime=" + "'" + newPosition + "'" + ";");
                        break;
                    case 1:
                        if (axWmp != null) {
							//SetupTimer();
							axWmp.Ctlcontrols.currentPosition = newPosition;
                        }
                        break;
                    case 2:
                        if (flash != null) {
                            //flash.SetVariable("isPlay", "true");
                            //flash.Play();
                        }
                        break;
                }
                //PositionSLTTText = GetHMS(newPosition.ToString());
                //dbMsg += ">>" + PositionSLTTText;
                //RaisePropertyChanged("PositionSLTTText");
                ////IsPlaying = true;
                ////RaisePropertyChanged("IsPlaying");

                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        public ICommand ListForwarding => new DelegateCommand(ForwardList);
        /// <summary>
        /// プレイリストの次アイテムへ
        /// </summary>
        public void ForwardList() {
            string TAG = "ForwardList";
            string dbMsg = "";
            try {
                SliderValue = 0;
                PauseVideo();
                dbMsg += "SelectedPlayListIndex=" + SelectedPlayListIndex;
                if ((PLList.Count - 2) < SelectedPlayListIndex) {
                    SelectedPlayListIndex = 0;
                } else {
                    SelectedPlayListIndex++;
                }
                RaisePropertyChanged("SelectedPlayListIndex");
                dbMsg += ">>=" + SelectedPlayListIndex;
                dbMsg += "/" + PLList.Count;
                PLMouseUp(PLList[SelectedPlayListIndex]);
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        public ICommand ListRewind => new DelegateCommand(RewindList);
        public void RewindList() {
            string TAG = "RewindList";
            string dbMsg = "";
            try {
                SliderValue = 0;
                dbMsg += "SelectedPlayListIndex=" + SelectedPlayListIndex;
                if (SelectedPlayListIndex < 1) {
                    SelectedPlayListIndex = PLList.Count - 1;
                } else {
                    SelectedPlayListIndex--;
                }
                RaisePropertyChanged("SelectedPlayListIndex");
                dbMsg += ">>=" + SelectedPlayListIndex;
                dbMsg += "/" + PLList.Count;
                PLMouseUp(PLList[SelectedPlayListIndex]);
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        public async void SetMediaVolume() {
            string TAG = "SetMediaVolume";
            string dbMsg = "";
            try {
                dbMsg += ",SoundValue=" + SoundValue;
                //double setVolVal = (double)SoundValue/100;
                //dbMsg += ">>" + setVolVal;
                switch (movieType) {
                    case 0:
                        await MyView.webView.ExecuteScriptAsync($"document.getElementById(" + "'" + Constant.PlayerName + "'" + ").volume=" + SoundValue + ";");
                        break;
                    case 1:
                        if (axWmp != null) {
                            dbMsg += ",volume=" + axWmp.settings.volume;
                            axWmp.settings.volume = (int)Math.Round(SoundValue * 100);
                            dbMsg += ">>" + axWmp.settings.volume;
                        }
                        break;
                    case 2:
                        if (flash != null) {
							//           flash.SetVariable("volume", SoundValue + "");

							string flashVvars = "volume=" + SoundValue + '"';
							flash.FlashVars = flashVvars;
						}
                        break;
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        public ICommand MuteClick => new DelegateCommand(SoundMute);
        /// <summary>
        /// 消音
        /// </summary>
        public async void SoundMute() {
            string TAG = "SoundMute";
            string dbMsg = "";
            try {
                dbMsg += "IsMute=" + IsMute;
                if (IsMute) {
                    IsMute = false;
                } else {
                    IsMute = true;
                }
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        //WindowsMediaPlayer  /////////////////////////////////////////////////////////////////////////////// プレイヤーコントロール　///

        ///以下使用待ち//////////////////////////////////////////////////////////////////////////////////////////WindowsMediaPlayer  ///

        Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(FEATURE_BROWSER_EMULATION);
        const string FEATURE_BROWSER_EMULATION = @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
        string process_name = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";
        string process_dbg_name = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".vshost.exe";

        ///システムメニューのカスタマイズ /////////////////////////////////////////////////////////////////////////////////////////////
        [StructLayout(LayoutKind.Sequential)]
        struct MENUITEMINFO {
            public uint cbSize;
            public uint fMask;
            public uint fType;
            public uint fState;
            public uint wID;
            public IntPtr hSubMenu;
            public IntPtr hbmpChecked;
            public IntPtr hbmpUnchecked;
            public IntPtr dwItemData;
            public string dwTypeData;
            public uint cch;
            public IntPtr hbmpItem;

            // return the size of the structure
            public static uint SizeOf {
                get { return (uint)Marshal.SizeOf(typeof(MENUITEMINFO)); }
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);      //ウィンドウのシステムメニューを取得

        [DllImport("user32.dll")]
        static extern bool InsertMenuItem(IntPtr hMenu, uint uItem, bool fByPosition,
          [In] ref MENUITEMINFO lpmii);

        private const uint MENU_ID_20 = 0x0001;                         //ファイルエリア開閉
        private const uint MENU_ID_60 = 0x0002;                     //プレイリストエリア開閉
        private const uint MENU_ID_99 = 0x0003;

        private const uint MFT_BITMAP = 0x00000004;
        private const uint MFT_MENUBARBREAK = 0x00000020;
        private const uint MFT_MENUBREAK = 0x00000040;
        private const uint MFT_OWNERDRAW = 0x00000100;
        private const uint MFT_RADIOCHECK = 0x00000200;
        private const uint MFT_RIGHTJUSTIFY = 0x00004000;
        private const uint MFT_RIGHTORDER = 0x000002000;

        private const uint MFT_SEPARATOR = 0x00000800;
        private const uint MFT_STRING = 0x00000000;

        private const uint MIIM_FTYPE = 0x00000100;
        private const uint MIIM_STRING = 0x00000040;
        private const uint MIIM_ID = 0x00000002;

        private const uint WM_SYSCOMMAND = 0x0112;

        bool IsWriteSysMenu = false;    //システムメニューを追記した
                                        /////////////////////////////////////////////////////////////////////////////////////////////システムメニューのカスタマイズ///
                                        //       Settings appSettings = new Settings();
        public System.ComponentModel.ComponentResourceManager resources;
        //	public SplitContainer MediaPlayerSplitter;
        //public System.Windows.Forms.WebBrowser playerWebBrowser;

        string[] systemFiles = new string[] { "RECYCLE", ".bak", ".bdmv", ".blf", ".BIN", ".cab",  ".cfg",  ".cmd",".css",  ".dat",".dll",
                                                ".inf",  ".inf", ".ini", ".lsi", ".iso",  ".lst", ".jar",  ".log", ".lock",".mis",
                                                ".mni",".MARKER",  ".mbr", ".manifest","swapfile",
                                              ".properties",".pnf" ,  ".prx", ".scr", ".settings",  ".so",  ".sys",  ".xml", ".exe"};
        string[] mpFiles = new string[] { ".mov", ".qt", ".mpg",".mpeg",  ".mp4",  ".m1v", ".mp2", ".mpa",".mpe",".3gp",  ".3g2",  ".asf",  ".asx",
                                                ".m2ts",".ts",".dvr-ms",".ivf",".wax",".wmv", ".wvx",  ".wm",  ".wmx",  ".wmz",
                                                ".adt",  ".adts", ".aif",  ".aifc", ".aiff", ".au", ".snd", ".cda",
                                                ".mp3", ".m4a", ".aac",  ".mid", ".midi", ".flac", ".wax", ".wma", ".wav"};
        string[] imageFiles = new string[] { ".jpg", ".jpeg", ".gif", ".png", ".tif", ".ico", ".bmp" };
        string[] audioFiles = new string[] { ".adt",  ".adts", ".aif",  ".aifc", ".aiff", ".au", ".snd", ".cda",
                                                ".mp3", ".m4a", ".aac", ".ogg", ".mid", ".midi", ".rmi", ".ra",".ram", ".flac", ".wax", ".wma", ".wav" };
        string[] textFiles = new string[] { ".txt", ".html", ".htm", ".xhtml", ".xml", ".rss", ".xml", ".css", ".js", ".vbs", ".cgi", ".php" };
        string[] applicationFiles = new string[] { ".zip", ".pdf", ".doc", ".xls", ".wpl", ".wmd", ".wms", ".wmz", ".wmd" };
        string[] playListFiles = new string[] { ".m3u" };
        //ListViewItemComparer listViewItemSorter;        //ListViewItemSorterに指定するフィールド

        string flRightClickItemUrl = "";        //fileTreeクリックアイテムのFullPath
        string copySouce = "";      //コピーするアイテムのurl
        string cutSouce = "";       //カットするアイテムのurl
        private string assemblyPath = "";       //実行デレクトリ
        private string configFileName;      //設定ファイル名 
        private string assemblyName = "";       //実行ファイル名
        private string playerUrl = "";
        double CurrentPosition;
        double CurrentMediaDuration;

        int SettingsVolum = 50;     //this.mediaPlayer.settings.volume;
        string lsFullPathName = ""; //リストで選択されたアイテムのフルパス
        string plaingItem = "";             //再生中アイテムのフルパス;連続再生スタート時、自動送り、プレイリストからのアイテムクリックで更新
        string listUpDir = "";             //プレイリストにリストアップするデレクトリ
        string wiPlayerID = Constant.PlayerName;       //webに埋め込むプレイヤーのID
                                                       //       List<PlayListItems> PlayListBoxItem = new List<PlayListItems>();
        List<int> treeSelectList = new List<int>();
        string nowLPlayList = "";               //現在使っているプレイリスト
        int plIndex;             //プレイリスト上のアイテムのインデックスを取得
        int PlaylistDragDropNo;
        int PlaylistDragOverNo;
        int PlaylistDragEnterNo;
        int PlaylistMouseUp;
        List<string> DragURLs = new List<string>();

        string plRightClickItemUrl = "";       //PlayListクリックアイテムのFullPath
        string dragFrom = "";
        ListBox draglist;
        System.Drawing.Point mouceDownPoint;
        int PlayListMouseDownNo;
        string PlayListMouseDownValue = "";
        //      DragDropEffects DDEfect;
        TreeNode ftSelectNode;
        TreeNode dragNode;
        TreeNode fileTreeDropNode; //ドロップ先のTreeNodeを取得する
        int dragSouceIDl = -1;
        int dragSouceIDP = -1;                          //ドラッグ開始時のマウスの位置から取得
        string dragSouceUrl = "";
        string b_dragSouceUrl = "";
        //     private Point PlaylistMouseDownPoint = Point.Empty;     //マウスの押された位置
        //アイコン
        //Properties
        //private Cursor noneCursor = new Cursor("none.cur");
        //private Cursor moveCursor = new Cursor("move.cur");
        //private Cursor copyCursor = new Cursor("copy.cur");
        //private Cursor linkCursor = new Cursor("link.cur");

        //	int playListWidth = 234;            //プレイリストの幅
        //	ProgressDialog pDialog;
        List<String> PlayListFileNames = new List<String>();
        bool isPlay = true;

        //public Form1()
        //{
        //    string TAG = "[Form1]";
        //    string dbMsg = TAG;
        //    try
        //    {
        //        typeof(Form).GetField("defaultIcon",
        //        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(
        //        null, new System.Drawing.Icon("M3UPlayerb_icon.ico"));

        //        InitializeComponent();
        //        //	configFileName =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        //        //							Application.CompanyName + "\\" + Application.ProductName +"\\" + Application.ProductName + ".config");
        //        configFileName = assemblyPath.Replace(".exe", ".config");
        //        dbMsg += ",configFileName=" + configFileName;
        //        //			SplitContainer MediaPlayerSplitter = this.splitContainer1;
        //        this.resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        //        ReadSetting();
        //        ///WebBrowserコントロールを配置すると、IEのバージョン 7をIE11の Edgeモードに変更//http://blog.livedoor.jp/tkarasuma/archives/1036522520.html
        //        regkey.SetValue(process_name, 11001, Microsoft.Win32.RegistryValueKind.DWord);
        //        regkey.SetValue(process_dbg_name, 11001, Microsoft.Win32.RegistryValueKind.DWord);

        //        fileTree.LabelEdit = true;         //ツリーノードをユーザーが編集できるようにする

        //        ReWriteSysMenu();   //システムメニューカスタマイズ
        //                            //イベントハンドラの追加
        //                            /*		fileTree.BeforeLabelEdit += new NodeLabelEditEventHandler( FileTree_BeforeLabelEdit );
        //			fileTree.AfterLabelEdit += new NodeLabelEditEventHandler( FileTree1_AfterLabelEdit );
        //			fileTree.KeyUp += new KeyEventHandler( FileTree_KeyUp );*/

        //        元に戻す.Visible = false;
        //        ペーストToolStripMenuItem.Visible = false;
        //        このファイルを再生ToolStripMenuItem.Visible = false;                //プレイリストへボタン非表示
        //        MyLog(TAG, dbMsg);
        //    }
        //    catch (Exception er)
        //    {
        //        Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
        //    }
        //}

        //private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    string TAG = "[Form1_FormClosing]";
        //    string dbMsg = TAG;
        //    try
        //    {
        //        regkey.DeleteValue(process_name);
        //        regkey.DeleteValue(process_dbg_name);
        //        regkey.Close();
        //        WriteSetting();
        //        Application.ApplicationExit -= new EventHandler(Application_ApplicationExit);         //ApplicationExitイベントハンドラを削除
        //        MyLog(TAG, dbMsg);
        //    }
        //    catch (Exception er)
        //    {
        //        Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
        //    }
        //}

        //private void Form1_Load(object sender, EventArgs e)
        //{
        //    string TAG = "[Form1_Load]";
        //    string dbMsg = TAG;
        //    try
        //    {
        //        fileTree.ImageList = this.imageList1;             //☆treeView1では設定できなかった
        //        FilelistView.SmallImageList = this.imageList1;
        //        AWSFileBroeser.Properties.Settings.Default.SettingChanging += new System.Configuration.SettingChangingEventHandler(Default_SettingChanging);//プリファレンスの変更イベント
        //                                                                                                                                                    //		playListWidth = splitContainer2.Width;
        //                                                                                                                                                    //dbMsg += "playListWidth" + playListWidth;
        //        fileTree.AllowDrop = true;
        //        fileTree.ItemDrag += new ItemDragEventHandler(FileTree_ItemDrag);      //イベントハンドラを追加する
        //        fileTree.DragOver += new DragEventHandler(FileTree_DragOver);
        //        fileTree.DragEnter += new DragEventHandler(FileTree_DragEnter);         //※追加；他のコントロールからのDrag			
        //        fileTree.DragDrop += new DragEventHandler(FileTree_DragDrop);
        //        this.ScrollControlIntoView(fileTree);

        //        FilelistView.AllowDrop = true;
        //        FilelistView.ItemDrag += new ItemDragEventHandler(FilelistView_ItemDrag);               //☆Dragの発生源をここだけに限定しないと二重発生する
        //        FilelistView.DragOver += new DragEventHandler(FilelistView_DragOver);
        //        FilelistView.DragDrop += new DragEventHandler(FilelistView_DragDrop);

        //        FilelistView.View = View.Details;                                                       //詳細表示にする
        //        FilelistView.ColumnClick += new ColumnClickEventHandler(FilelistView_ColumnClick);        //ColumnClickイベントハンドラの追加☆Form1.csと重複させない
        //                                                                                                  /*
        //																					  listViewItemSorter = new ListViewItemComparer();                //ListViewItemComparerの作成と設定

        //																																					   listViewItemSorter.ColumnModes = new ListViewItemComparer.ComparerMode[]
        //																																						  {
        //																																										  ListViewItemComparer.ComparerMode.String,
        //																																										  ListViewItemComparer.ComparerMode.Integer,
        //																																										  ListViewItemComparer.ComparerMode.DateTime
        //																																						  };
        //																																						  FilelistView.ListViewItemSorter = listViewItemSorter;               //ListViewItemSorterを指定する
        //																																						  */
        //        playListBox.AllowDrop = true;
        //        playListBox.DragEnter += new DragEventHandler(PlayListBox_DragEnter);
        //        playListBox.DragDrop += new DragEventHandler(PlayListBox_DragDrop);

        //        continuousPlayCheckBox.Checked = false;//連続再生ボタン

        //        MakeDriveList();

        //        MyLog(TAG, dbMsg);
        //    }
        //    catch (Exception er)
        //    {
        //        Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
        //    }

        //}

        //private void Application_ApplicationExit(object sender, EventArgs e)
        //{
        //    string TAG = "[Application_ApplicationExit]";
        //    string dbMsg = TAG;
        //    try
        //    {
        //        WriteSetting();
        //        Application.ApplicationExit -= new EventHandler(Application_ApplicationExit);         //ApplicationExitイベントハンドラを削除
        //        MyLog(TAG, dbMsg);
        //    }
        //    catch (Exception er)
        //    {
        //        Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
        //    }
        //}       //ApplicationExitイベントハンドラ

        /////////////////////////////////////////////////////////////////////////////////////////////////////Formイベント/////

        /// <summary>
        /// 連続再生時、再生対象をファイルリストで選択しているファイルに切り替える
        /// プレイリストファイルを選択した場合の読込み
        /// </summary>
        /// <param name="fullName"></param>
        public void PlayFromFileBrousert(string fullName) {
            string TAG = "[PlayFromFileList]";
            string dbMsg = TAG;
            try {
                dbMsg += fullName;
                string[] extStrs = fullName.Split('.');
                string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
                dbMsg += ",extentionStr=" + extentionStr;
                MyLog(TAG, dbMsg);
                /*		if (extentionStr == ".m3u") {
							fullName = fullName.Replace(@":\\", @":\");
							ComboBoxAddItems(PlaylistComboBox, fullName);
							string[] PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };
							dbMsg += ",PLArray=" + PLArray.Length + "件";
							int plSelIndex = Array.IndexOf(PLArray, fullName) + 1;
							dbMsg += "," + plSelIndex + "番目";
							PlaylistComboBox.SelectedIndex = plSelIndex;
						} else {*/
                playerUrl = fullName; //リストで選択されたアイテムのフルパス
                plaingItem = fullName;             //再生中アイテムのフルパス
                lsFullPathName = fullName;
                //    ToView(fullName);
                //	}
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                dbMsg += "<<以降でエラー発生>>" + er.Message;
                MyLog(TAG, dbMsg);
            }
        }

        /// <summary>
        /// 再生状態取得	未使用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayStateChangeEvent(object sender, EventArgs e)           //AxWMPLib._WMPOCXEvents_PlayStateChangeEvent
        {
            string TAG = "[PlayStateChangeEvent]";
            string dbMsg = TAG;
            try {
                /*	switch (e.newState) {
						case 0:    // Undefined
						dbMsg+= "Undefined";
						break;

						case 1:    // Stopped
						dbMsg +=  "Stopped";
						break;

						case 2:    // Paused
						currentStateLabel.Text = "Paused";
						break;

						case 3:    // Playing
						currentStateLabel.Text = "Playing";
						break;

						case 4:    // ScanForward
						currentStateLabel.Text = "ScanForward";
						break;

						case 5:    // ScanReverse
						currentStateLabel.Text = "ScanReverse";
						break;

						case 6:    // Buffering
						currentStateLabel.Text = "Buffering";
						break;

						case 7:    // Waiting
						currentStateLabel.Text = "Waiting";
						break;

						case 8:    // MediaEnded
						currentStateLabel.Text = "MediaEnded";
						break;

						case 9:    // Transitioning
						currentStateLabel.Text = "Transitioning";
						break;

						case 10:   // Ready
						currentStateLabel.Text = "Ready";
						break;

						case 11:   // Reconnecting
						currentStateLabel.Text = "Reconnecting";
						break;

						case 12:   // Last
						currentStateLabel.Text = "Last";
						break;

						default:
						currentStateLabel.Text = ( "Unknown State: " + e.newState.ToString() );
						break;
					}*/
                MyLog(TAG, dbMsg);
            } catch (NotImplementedException er) {
                dbMsg += "<<以降でエラー発生>>" + er.Message;
                MyLog(TAG, dbMsg);
                throw new NotImplementedException();
            } catch (Exception er) {
                dbMsg += "<<以降でエラー発生>>" + er.Message;
                MyLog(TAG, dbMsg);
            }

        }



        /*		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
				{
					string selectDrive = comboBox1.SelectedItem.ToString();
			//		listBox1.Items.Clear();
					MakeFolderList( selectDrive );
				}           //ドライブセレクト*/

        private void MakeFolderList(string sarchDir)//, string sarchTyp
        {
            try {
                string[] files = Directory.GetFiles(sarchDir);
                if (files != null) {
                    foreach (string fileName in files) {
                        if (-1 < fileName.IndexOf("RECYCLE.BIN", StringComparison.OrdinalIgnoreCase)) {
                        } else {

                            string rfileName = fileName.Replace(sarchDir, "");
                            //					listBox1.Items.Add( rfileName );      //ListBox1に結果を表示する
                        }
                    }
                }
                string[] folderes = Directory.GetDirectories(sarchDir);//
                if (folderes != null) {
                    foreach (string directoryName in folderes) {
                        if (-1 < directoryName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
                            -1 < directoryName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase)
                            ) { } else {
                            //	listBox1.Items.Add( directoryName );
                            //        MakeFolderList(directoryName);
                        }
                    }           //ListBox1に結果を表示する

                }
            } catch (UnauthorizedAccessException UAEx) {
                Console.WriteLine(UAEx.Message);
            } catch (PathTooLongException PathEx) {
                Console.WriteLine(PathEx.Message);
            }

        }       //ファイルリストアップ

        private void MakeFileList(string sarchDir, string sarchType) {
            string[] files = Directory.GetFiles("c:\\");
            foreach (string fileName in files) {
                //			listBox1.Items.Add( fileName );
            }           //ListBox1に結果を表示する

            //     System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(sarchDir);
            //     System.IO.FileInfo[] files =di.GetFiles(sarchType, System.IO.SearchOption.AllDirectories);
            //        foreach (System.IO.FileInfo f in files)
            //       {
            //           listBox1.Items.Add(f.FullName);
            //       }           //ListBox1に結果を表示する

            //以下2行でも同様      https://dobon.net/vb/dotnet/file/getfiles.html
            //            string[] files = System.IO.Directory.GetFiles( sarchDir, sarchType, System.IO.SearchOption.AllDirectories);           //"C:\test"以下のファイルをすべて取得する
            //         listBox1.Items.AddRange(files);           //ListBox1に結果を表示する
        }       //ファイルリストアップ

        private void MakeDriveList() {
            TreeNode tn;
            foreach (DriveInfo drive in DriveInfo.GetDrives())//http://www.atmarkit.co.jp/fdotnet/dotnettips/557driveinfo/driveinfo.html
            {
                string driveNames = drive.Name; // ドライブ名
                if (drive.IsReady) { // ドライブの準備はOK？
                                     //      tn = new TreeNode(driveNames, 0, 0);
                                     ////      fileTree.Nodes.Add(tn);//親ノードにドライブを設定
                                     //  //    FolderItemListUp(driveNames, tn);
                                     //      tn.ImageIndex = 0;          //hd_icon.png
                }
            }
        }//使用可能なドライブリスト取得

        ////ファイル操作////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 拡張子からファイルタイプを返し、MIMEをセットする
        /// </summary>
        /// <param name="checkFileName"></param>
        /// <returns></returns>
        public string GetFileTypeStr(string checkFileName) {
            string TAG = "[GetFileTypeStr]";
            string dbMsg = "";
            string retMIME = "";
            try {
                string retType = "";
                string[] extStrs = checkFileName.Split('.');
                string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
                dbMsg += ",拡張子=" + extentionStr;
                if (-1 < extentionStr.IndexOf(".mov", StringComparison.OrdinalIgnoreCase) ||
                    -1 < extentionStr.IndexOf(".qt", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "video/quicktime";
                } else if (-1 < extentionStr.IndexOf(".mpg", StringComparison.OrdinalIgnoreCase) ||
                    -1 < extentionStr.IndexOf(".mpeg", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "video/mpeg";
                } else if (-1 < extentionStr.IndexOf(".mp4", StringComparison.OrdinalIgnoreCase)) {          //動画コーデック：H.264/音声コーデック：MP3、AAC
                    retType = "video";
                    retMIME = "video/mp4";        //ver12:MP4 ビデオ ファイル <source src="movie.mp4" type='video/mp4; codecs="avc1.42E01E, mp4a.40.2"' />
                } else if (-1 < extentionStr.IndexOf(".webm", StringComparison.OrdinalIgnoreCase)) {          //動画コーデック：VP8 / Vorbis
                    retType = "video";
                    retMIME = "video/webm";//  <source src="movie.webm" type='video/webm; codecs="vp8, vorbis"' />
                } else if (-1 < extentionStr.IndexOf(".ogv", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "video/ogv";
                } else if (-1 < extentionStr.IndexOf(".avi", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "video/x-msvideo";
                } else if (-1 < extentionStr.IndexOf(".3gp", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "video/3gpp";     //audio/3gpp
                } else if (-1 < extentionStr.IndexOf(".3g2", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "video/3gpp2";            //audio/3gpp2
                } else if (-1 < extentionStr.IndexOf(".asf", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "video/x-ms-asf";
                } else if (-1 < extentionStr.IndexOf(".asx", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "video/x-ms-asf";   //ver9:Windows Media メタファイル 
                } else if (-1 < extentionStr.IndexOf(".wax", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";   //ver9:Windows Media メタファイル 
                } else if (-1 < extentionStr.IndexOf(".wmv", StringComparison.OrdinalIgnoreCase)) {
                    retMIME = "video/x-ms-wmv";      //ver9:Windows Media 形式
                    retType = "video";
                } else if (-1 < extentionStr.IndexOf(".wvx", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "video/x-ms-wvx";       //ver9:Windows Media メタファイル 
                } else if (-1 < extentionStr.IndexOf(".wmx", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "video/x-ms-wmx";       //ver9:Windows Media メタファイル 
                } else if (-1 < extentionStr.IndexOf(".wmz", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "application/x-ms-wmz";
                } else if (-1 < extentionStr.IndexOf(".wmd", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "application/x-ms-wmd";
                } else if (-1 < extentionStr.IndexOf(".swf", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "application/x-shockwave-flash";
                } else if (-1 < extentionStr.IndexOf(".flv", StringComparison.OrdinalIgnoreCase)) {          //動画コーデック：Sorenson Spark / On2VP6/音声コーデック：MP3
                    retType = "video";
                    retMIME = "application/x-shockwave-flash";
                    //	retMIME = "video/x-flv";
                } else if (-1 < extentionStr.IndexOf(".f4v", StringComparison.OrdinalIgnoreCase)) {          //動画コーデック：H.264/音声コーデック：MP3、AAC、HE - AAC
                    retType = "video";
                    retMIME = "video/mp4";
                } else if (-1 < extentionStr.IndexOf(".rm", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    retMIME = "application/vnd.rn-realmedia";
                } else if (-1 < extentionStr.IndexOf(".ivf", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";     //ver10:Indeo Video Technology
                } else if (-1 < extentionStr.IndexOf(".dvr-ms", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";            //ver12:Microsoft デジタル ビデオ録画
                } else if (-1 < extentionStr.IndexOf(".m2ts", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";           //m2tsと同じ
                    /*
                     .htaccess や Apache のMIME Type設定
                     AddType application/x-mpegURL .m3u8
AddType video/MP2T .ts
                 */
                } else if (-1 < extentionStr.IndexOf(".ts", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";           //ver12:MPEG-2 TS ビデオ ファイル 
                } else if (-1 < extentionStr.IndexOf(".m1v", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                } else if (-1 < extentionStr.IndexOf(".mp2", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                } else if (-1 < extentionStr.IndexOf(".mpa", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                } else if (-1 < extentionStr.IndexOf(".mpe", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                } else if (-1 < extentionStr.IndexOf(".m4v", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                } else if (-1 < extentionStr.IndexOf(".mp4v", StringComparison.OrdinalIgnoreCase)) {
                    retType = "video";
                    //image/////////////////////////////////////////////////////////////////////////
                } else if (-1 < extentionStr.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) ||
                         -1 < extentionStr.IndexOf(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                    retType = "image";
                    retMIME = "image/jpeg";
                } else if (-1 < extentionStr.IndexOf(".gif", StringComparison.OrdinalIgnoreCase)) {
                    retType = "image";
                    retMIME = "image/gif";
                } else if (-1 < extentionStr.IndexOf(".png", StringComparison.OrdinalIgnoreCase)) {
                    retType = "image";
                    retMIME = "image/png";
                } else if (-1 < extentionStr.IndexOf(".ico", StringComparison.OrdinalIgnoreCase)) {
                    retType = "image";
                    retMIME = "image/vnd.microsoft.icon";
                } else if (-1 < extentionStr.IndexOf(".bmp", StringComparison.OrdinalIgnoreCase)) {
                    retType = "image";
                    retMIME = "image/x-ms-bmp";
                    //audio/////////////////////////////////////////////////////////////////////////
                } else if (-1 < extentionStr.IndexOf(".mp3", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";
                    retMIME = "audio/mpeg";
                } else if (-1 < extentionStr.IndexOf(".m4a", StringComparison.OrdinalIgnoreCase) ||
                    -1 < extentionStr.IndexOf(".aac", StringComparison.OrdinalIgnoreCase)
                    ) {
                    retType = "audio";
                    retMIME = "audio/aac";         //var12;MP4 オーディオ ファイル
                } else if (-1 < extentionStr.IndexOf(".ogg", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";
                    retMIME = "audio/ogg";
                } else if (-1 < extentionStr.IndexOf(".midi", StringComparison.OrdinalIgnoreCase) ||
                    -1 < extentionStr.IndexOf(".mid", StringComparison.OrdinalIgnoreCase) ||
                    -1 < extentionStr.IndexOf(".rmi", StringComparison.OrdinalIgnoreCase)
                    ) {
                    retType = "audio";
                    retMIME = "audio/midi";          //var9;MIDI 
                } else if (-1 < extentionStr.IndexOf(".ra", StringComparison.OrdinalIgnoreCase) ||
                    -1 < extentionStr.IndexOf(".ram", StringComparison.OrdinalIgnoreCase)
                    ) {
                    retType = "audio";
                    retMIME = "audio/vnd.rn-realaudio";
                } else if (-1 < extentionStr.IndexOf(".flac", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";
                    retMIME = "audio/flac";
                } else if (-1 < extentionStr.IndexOf(".wma", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";
                    retMIME = "audio/x-ms-wma";
                } else if (-1 < extentionStr.IndexOf(".wav", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";
                    retMIME = "audio/wav";           //var9;Windows 用オーディオ   
                } else if (-1 < extentionStr.IndexOf(".aif", StringComparison.OrdinalIgnoreCase) ||
                    -1 < extentionStr.IndexOf(".aifc", StringComparison.OrdinalIgnoreCase) ||
                    -1 < extentionStr.IndexOf(".aiff", StringComparison.OrdinalIgnoreCase)
                    ) {
                    retType = "audio";           //var9;Audio Interchange File FormatI 
                } else if (-1 < extentionStr.IndexOf(".au", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";          //var9;Sun Microsystems  
                } else if (-1 < extentionStr.IndexOf(".snd", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";          //var9; NeXT  
                } else if (-1 < extentionStr.IndexOf(".cda", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";          //var9;CD オーディオ トラック 
                } else if (-1 < extentionStr.IndexOf(".adt", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";          //var12;Windows オーディオ ファイル 
                } else if (-1 < extentionStr.IndexOf(".adts", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";           //var12;Windows オーディオ ファイル 
                } else if (-1 < extentionStr.IndexOf(".asx", StringComparison.OrdinalIgnoreCase)) {
                    retType = "audio";
                    //text/////////////////////////////////////////////////////////////////////////
                } else if (-1 < extentionStr.IndexOf(".txt", StringComparison.OrdinalIgnoreCase)) {
                    retType = "text";
                    retMIME = "text/plain";
                } else if (-1 < extentionStr.IndexOf(".html", StringComparison.OrdinalIgnoreCase) ||
                    -1 < extentionStr.IndexOf(".htm", StringComparison.OrdinalIgnoreCase)
                    ) {
                    retType = "text";
                    retMIME = "text/html";
                } else if (-1 < extentionStr.IndexOf(".xhtml", StringComparison.OrdinalIgnoreCase)) {
                    retMIME = "application/xhtml+xml";
                } else if (-1 < extentionStr.IndexOf(".xml", StringComparison.OrdinalIgnoreCase)) {
                    retType = "text";
                    retMIME = "text/xml";
                } else if (-1 < extentionStr.IndexOf(".rss", StringComparison.OrdinalIgnoreCase)) {
                    retType = "text";
                    retMIME = "application/rss+xml";
                } else if (-1 < extentionStr.IndexOf(".xml", StringComparison.OrdinalIgnoreCase)) {
                    retType = "text";
                    retMIME = "application/xml";            //、text/xml
                } else if (-1 < extentionStr.IndexOf(".css", StringComparison.OrdinalIgnoreCase)) {
                    retType = "text";
                    retMIME = "text/css";
                } else if (-1 < extentionStr.IndexOf(".js", StringComparison.OrdinalIgnoreCase)) {
                    retType = "text";
                    retMIME = "text/javascript";
                } else if (-1 < extentionStr.IndexOf(".vbs", StringComparison.OrdinalIgnoreCase)) {
                    retType = "text";
                    retMIME = "text/vbscript";
                } else if (-1 < extentionStr.IndexOf(".cgi", StringComparison.OrdinalIgnoreCase)) {
                    retType = "text";
                    retMIME = "application/x-httpd-cgi";
                } else if (-1 < extentionStr.IndexOf(".php", StringComparison.OrdinalIgnoreCase)) {
                    retType = "text";
                    retMIME = "application/x-httpd-php";
                    //application/////////////////////////////////////////////////////////////////////////
                } else if (-1 < extentionStr.IndexOf(".zip", StringComparison.OrdinalIgnoreCase)) {
                    retType = "application";
                    retMIME = "application/zip";
                } else if (-1 < extentionStr.IndexOf(".pdf", StringComparison.OrdinalIgnoreCase)) {
                    retType = "application";
                    retMIME = "application/pdf";
                } else if (-1 < extentionStr.IndexOf(".doc", StringComparison.OrdinalIgnoreCase)) {
                    retType = "application";
                    retMIME = "application/msword";
                } else if (-1 < extentionStr.IndexOf(".xls", StringComparison.OrdinalIgnoreCase)) {
                    retType = "application";
                    retMIME = "application/msexcel";
                } else if (-1 < extentionStr.IndexOf(".wmx", StringComparison.OrdinalIgnoreCase)) {
                    retType = "application";        //ver9:Windows Media Player スキン 
                } else if (-1 < extentionStr.IndexOf(".wms", StringComparison.OrdinalIgnoreCase)) {
                    retType = "application";       //ver9:Windows Media Player スキン  
                } else if (-1 < extentionStr.IndexOf(".wmz", StringComparison.OrdinalIgnoreCase)) {
                    retType = "application";       //ver9:Windows Media Player スキン  
                } else if (-1 < extentionStr.IndexOf(".wpl", StringComparison.OrdinalIgnoreCase)) {
                    retType = "application";       //ver9:Windows Media Player スキン  
                } else if (-1 < extentionStr.IndexOf(".wmd", StringComparison.OrdinalIgnoreCase)) {
                    retType = "application";       //ver9:Windows Media Download パッケージ   
                    /*		} else if (-1 < extentionStr.IndexOf(".m3u", StringComparison.OrdinalIgnoreCase)) {
                                retType = "video";*/

                } else if (-1 < extentionStr.IndexOf(".wm", StringComparison.OrdinalIgnoreCase)) {        //以降wmで始まる拡張子が誤動作
                    retType = "video";
                    retMIME = "video/x-ms-wm";
                }

                //typeName.Text = retType;
                //mineType.Text = retMIME;
                dbMsg += ",retMIME=" + retMIME;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
            return retMIME;
        }       //拡張子からタイプとMIMEを返す

        /// <summary>
        /// 「ファイルを開く」ダイアログボックスを表示
        /// </summary>
        /// <param name="dlogTitol"></param>
        /// <param name="filterStr"></param>
        /// <param name="initialDirectory"></param>
        /// <returns>選択したファイルのフルパス</returns>
        /// 「ファイルを開く」ダイアログボックスを表示	https://dobon.net/vb/dotnet/form/openfiledialog.html
        private string SelrctFile(string dlogTitol, string filterStr, string initialDirectory) {            // "プレイリストを選択してください"
            string TAG = "[SelrctFile]" + dlogTitol;
            string dbMsg = TAG;
            string rPlayList = "";
            try {
                //OpenFileDialog ofd = new OpenFileDialog();             //OpenFileDialogクラスのインスタンスを作成☆
                //ofd.Title = dlogTitol;              //タイトルを設定する
                //                                    //	ofd.FileName = "default.m3u";                          //はじめのファイル名を指定する
                //                                    //はじめに「ファイル名」で表示される文字列を指定する
                //dbMsg += ",initialDirectory=" + initialDirectory;
                //ofd.InitialDirectory = initialDirectory;              //はじめに表示されるフォルダを指定する
                //ofd.Filter = filterStr;                             //[ファイルの種類]に表示される選択肢を指定する	
                //ofd.FilterIndex = 1;                                //[ファイルの種類]ではじめに選択されるものを指定する
                //ofd.RestoreDirectory = true;                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                //if (ofd.ShowDialog() == DialogResult.OK)
                //{        //OpenFileDialogでは == DialogResult.OK)
                //    rPlayList = ofd.FileName;
                //}
                dbMsg += ",選択されたファイル名=" + rPlayList;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                dbMsg += "<<以降でエラー発生>>" + er.Message;
                MyLog(TAG, dbMsg);
            }
            return rPlayList;
        }

        /// <summary>
        /// 指定された階層にあるアイテム数を返す
        /// ☆ボリューム直下対策
        /// </summary>
        /// <param name="passNameStr"></param>
        /// <returns></returns>
        private int CurrentItemCount(string passNameStr) {
            string TAG = "[CurrentItemCount]";
            string dbMsg = TAG;
            int retIntr = 0;
            try {
                dbMsg += ",対象階層=" + passNameStr;
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(passNameStr);
                //		dbMsg += "；Dir;;Attributes=" + dirInfo.Attributes;
                if (dirInfo.Parent != null) {
                    //	System.IO.FileAttributes attr = System.IO.Path.GetAttributes(passNameStr);
                    if ((dirInfo.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden) {
                        dbMsg += ">>Hidden";
                    } else if ((dirInfo.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System) {
                        dbMsg += ">>System";
                    } else {

                        dbMsg += ",Parent=" + dirInfo.Parent;//☆ドライブルートはこれで落ちる
                        dbMsg += "フォルダ内";
                        System.IO.FileInfo[] files = dirInfo.GetFiles("*", System.IO.SearchOption.AllDirectories);
                        retIntr = files.Length;      // サブディレクトリ内のファイルもカウントする場合	, SearchOption.AllDirectories
                                                     //			System.IO.DirectoryInfo[] dirs = dirInfo.GetDirectories( "*", System.IO.SearchOption.AllDirectories );
                                                     //			retIntr += dirs.Length;
                    }
                } else {
                    dbMsg += "ドライブ確認";
                    System.IO.DriveInfo driveInfo = new System.IO.DriveInfo(passNameStr);     //.Substring( 0, 1 )
                    if (driveInfo.IsReady) {
                        dbMsg += ">>ドライブ直下" + driveInfo.RootDirectory.ToString();
                        var rootItems = Directory.EnumerateFiles(passNameStr);//.Where( x => !x.Contains( ( passNameStr + "System Volume Infomation") ) );
                                                                              //	dbMsg += ",rootItems=" + rootItems.All.ToString();
                        foreach (var rootItem in rootItems) {
                            dbMsg += "(" + retIntr + ")" + rootItem;
                            //System Volume Infomation(復元ポイントが保存されている隠しフォルダ)にアクセスが発生して落ちる
                            dirInfo = new System.IO.DirectoryInfo(rootItem);
                            if ((dirInfo.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden) {
                                dbMsg += ">>Hidden";
                            } else if ((dirInfo.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System) {
                                dbMsg += ">>System";
                            } else {

                                if (-1 < rootItem.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
                                -1 < rootItem.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase)) {
                                } else {
                                    try {
                                        /*	string[] folderes = Directory.GetDirectories( rootItem );
											if (folderes != null) {
												dbMsg += "\nfolderes=" + folderes.Length + "件";
												foreach (string directoryName in folderes) {
													//		if (-1 < directoryName.IndexOf( "RECYCLE", StringComparison.OrdinalIgnoreCase ) ||
													//			-1 < directoryName.IndexOf( "System Vol", StringComparison.OrdinalIgnoreCase )) {
													//		} else {
													dirInfo = new System.IO.DirectoryInfo( directoryName );
													dbMsg += ";dirInfo=" + dirInfo.Attributes;
													if (dirInfo.Attributes.ToString() == "Directory") {
														System.IO.DirectoryInfo[] rootDirs = dirInfo.GetDirectories( "*", System.IO.SearchOption.AllDirectories );
														dbMsg += ",rootDirs=" + rootDirs.Length;
														retIntr += rootDirs.Length;
														System.IO.FileInfo[] rootFiles = dirInfo.GetFiles( "*", System.IO.SearchOption.AllDirectories );
														dbMsg += ",rootFiles=" + rootFiles.Length;
														retIntr += rootFiles.Length;      // サブディレクトリ内のファイルもカウントする場合	, SearchOption.AllDirectories
													} else {
														retIntr++;
													}
													//	}
												}           //ListBox1に結果を表示する
											}
											*/


                                        //	if (rootItem.ToString() != ( passNameStr + "System" + "*" )) {
                                        //		dirInfo = new System.IO.DirectoryInfo(rootItem);
                                        string dirAttributes = dirInfo.Attributes.ToString();
                                        dbMsg += ";dirInfo=" + dirAttributes;
                                        if (dirAttributes == "Directory") {
                                            System.IO.DirectoryInfo[] rootDirs = dirInfo.GetDirectories("*", System.IO.SearchOption.AllDirectories);
                                            dbMsg += ",rootDirs=" + rootDirs.Length;
                                            retIntr += rootDirs.Length;
                                            System.IO.FileInfo[] rootFiles = dirInfo.GetFiles("*", System.IO.SearchOption.AllDirectories);
                                            dbMsg += ",rootFiles=" + rootFiles.Length;
                                            retIntr += rootFiles.Length;      // サブディレクトリ内のファイルもカウントする場合	, SearchOption.AllDirectories
                                        } else {
                                            retIntr++;
                                        }
                                        /*=I:\an\workspace2015\参考資料\Android SDK逆引きハンドブック\sample\Chap-15\244\assets；Dir;;Attributes=Directory,Parent=244フォルダ内,
										 * このデレクトリには0件 マネージ デバッグ アシスタント 'ContextSwitchDeadlock' 
	CLR は、COM コンテキスト 0x6aa35230 から COM コンテキスト 0x6aa35108 へ 60 秒で移行できませんでした。ターゲット コンテキストおよびアパートメントを所有するスレッドが、ポンプしない待機を行っているか、Windows のメッセージを表示しないで非常に長い実行操作を処理しているかのどちらかです。この状態は通常、パフォーマンスを低下させたり、アプリケーションが応答していない状態および増え続けるメモリ使用を導く可能性があります。この問題を回避するには、すべての Single Thread Apartment (STA) のスレッドが、CoWaitForMultipleHandles のようなポンプする待機プリミティブを使用するか、長い実行操作中に定期的にメッセージをポンプしなければなりません。*/
                                        //									}
                                    } catch (Exception e) {
                                        dbMsg += "<<以降でエラー発生>>" + e.Message;
                                        MyLog(TAG, dbMsg);
                                        return retIntr;
                                        throw;
                                    }
                                }
                            }
                        }//for
                    }
                }
                dbMsg += ",このデレクトリには" + retIntr + "件";
                MyLog(TAG, dbMsg);
            } catch (Exception e) {
                dbMsg += "<<以降でエラー発生>>" + e.Message;
                MyLog(TAG, dbMsg);
            }
            return retIntr;
        }

        /// <summary>
        /// フォルダの中身をリストアップ
        ///		フリーズ発生
        /// </summary>
        /// <param name="sarchDir"></param>
        /// <returns></returns>
        private List<string> GetFolderFiles(string sarchDir) {
            string TAG = "[GetFolderFiles]";
            string dbMsg = TAG;
            List<string> retItems = new List<string>();
            try {
                dbMsg += "sarchDir=" + sarchDir;
                IEnumerable<string> files = Directory.EnumerateFiles(sarchDir, "*"); // サブ・ディレクトも含める	, System.IO.SearchOption.AllDirectories
                foreach (string fileName in files) {
                    dbMsg += "(" + retItems.Count + ")" + fileName;
                    string[] extStrs = fileName.Split('.');
                    string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
                    if (-1 < Array.IndexOf(systemFiles, extentionStr) ||
                        0 < fileName.IndexOf("BOOTNXT", StringComparison.OrdinalIgnoreCase) ||
                        0 < fileName.IndexOf("-ms", StringComparison.OrdinalIgnoreCase) ||
                        0 < fileName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase)
                        ) {
                    } else {
                        retItems.Add(fileName);
                    }
                }
                /*		try {
							List<string> folders = new List<string>( Directory.EnumerateDirectories( sarchDir ) );

							//	IEnumerable<string> folders = Microsoft.VisualBasic.FileIO.FileSystem.GetDirectories( sarchDir );
							//	IEnumerable<string> folders = Directory.EnumerateDirectories( sarchDir, "*" ).Where( x => !x.Contains( ( sarchDir + "System Volume Infomation" ) ) );

							// サブ・ディレクトも含める	, System.IO.SearchOption.AllDirectories
							dbMsg += "," + folders.Count() + "件";
							foreach (string directoryName in folders) {
								dbMsg += "," + directoryName;
								if (-1 < directoryName.IndexOf( "RECYCLE", StringComparison.OrdinalIgnoreCase ) ||
										-1 < directoryName.IndexOf( "System Vol", StringComparison.OrdinalIgnoreCase )) {      // 'M:\System Volume Information' へのアクセスが拒否されました。
								} else {
									dbMsg += "sarchDir=" + sarchDir;  
									files = Directory.EnumerateFiles( sarchDir, "*", System.IO.SearchOption.AllDirectories ); // サブ・ディレクトも含める	
									foreach (string file in files) {
										dbMsg += "(" + retItems.Count + ")" + file;
										retItems.Add( file );
									}
								}
							}
						} catch (UnauthorizedAccessException UAEx) {
							dbMsg += "<<以降でエラー発生>>" + UAEx.Message;
							MyLog( dbMsg );
							throw;
						} catch (PathTooLongException PathEx) {
							dbMsg += "<<以降でエラー発生>>" + PathEx.Message;
							MyLog( dbMsg );
							throw;
						} catch (Exception er) {
							dbMsg += "<<以降でエラー発生>>" + er.Message;
							MyLog( dbMsg );
							throw;
						}*/
                dbMsg += ",結果" + retItems.Count + "件";
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                dbMsg += "<<以降でエラー発生>>" + er.Message;
                MyLog(TAG, dbMsg);
                throw;
            }
            return retItems;
        }

        /// <summary>
        /// 指定した階層以下のファイルのフルパス名をListにして返す
        /// </summary>
        /// <param name="sarchDir"></param>
        /// <returns></returns>
        private List<string> GetFolderItems(string sarchDir, List<string> retItems) {
            string TAG = "[GetFolderItems]";
            string dbMsg = TAG;
            try {
                dbMsg += "sarchDir=" + sarchDir;
                string[] files = Directory.GetFiles(sarchDir);
                dbMsg += "," + files.Length + "件";
                if (files != null) {
                    foreach (string fileName in files) {
                        string[] extStrs = fileName.Split('.');
                        string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
                        if (-1 < Array.IndexOf(systemFiles, extentionStr) ||
                            0 < fileName.IndexOf("BOOTNXT", StringComparison.OrdinalIgnoreCase) ||
                            0 < fileName.IndexOf("-ms", StringComparison.OrdinalIgnoreCase) ||
                            0 < fileName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase)
                            ) {
                        } else {
                            dbMsg += "(" + retItems.Count + ")" + fileName;
                            retItems.Add(fileName);
                        }
                    }
                }
                string[] folderes = Directory.GetDirectories(sarchDir);
                if (folderes != null) {
                    dbMsg += "\nfolderes=" + folderes.Length + "件";
                    foreach (string directoryName in folderes) {
                        if (-1 < directoryName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
                            -1 < directoryName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase)) {
                        } else {
                            /*	List<string> retItems2 = GetFolderFiles( directoryName, retItems );
								dbMsg += ",retItems2=" + retItems2.Count + "件";
								for (int i = 0; i < retItems2.Count; ++i) {
									retItems.Add( retItems2[i] );
								}
                            */
                        }
                    }           //ListBox1に結果を表示する
                }
                MyLog(TAG, dbMsg);
            } catch (UnauthorizedAccessException UAEx) {
                dbMsg += "<<以降でエラー発生>>" + UAEx.Message;
                //	MyLog( dbMsg );
                throw;
            } catch (PathTooLongException PathEx) {
                dbMsg += "<<以降でエラー発生>>" + PathEx.Message;
                //	MyLog( dbMsg );
                throw;
            } catch (Exception er) {
                dbMsg += "<<以降でエラー発生>>" + er.Message;
                //	MyLog( dbMsg );
                throw;
            }
            return retItems;
        }

        /// <summary>
        /// 渡されたwindowsドライブ～ファイル名を正当なものにして返す
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string checKLocalFile(string fileName) {
            string TAG = "[checKLocalFile]";
            string dbMsg = TAG;
            string retStr = "";
            try {
                dbMsg += ",fileName=" + fileName;
                FileInfo fi = new FileInfo(fileName);
                retStr = fi.DirectoryName + Path.DirectorySeparatorChar + fi.Name;
                dbMsg += ",retStr=" + retStr;
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                dbMsg += "<<以降でエラー発生>>" + er.Message;
                MyLog(TAG, dbMsg);
            }
            MyLog(TAG, dbMsg);
            return retStr;
        }

        //   #region WebBlock

        //   private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        //   {
        //       string TAG = "[WebBrowser1_DocumentCompleted]";
        //       string dbMsg = TAG;
        //       try
        //       {
        //           /*	HtmlDocument wDoc = playerWebBrowser.Document;
        //string wText = playerWebBrowser.DocumentText;

        //				if (( wText.Contains( "object" ) ) || ( wText.Contains( "embed" ) )) {
        //					HtmlElement playerElem = playerWebBrowser.Document.GetElementById( wiPlayerID );
        //					playerElem.AttachEventHandler( "PlayStateChangeEvent", new EventHandler( PlayStateChangeEvent ) );     //PlayState.MediaEnded		CurrentState 
        //					dbMsg += ",Controls=" + playerWebBrowser.Controls;
        //					dbMsg += ",ReadyState=" + playerWebBrowser.ReadyState;
        //				}
        //*/
        //           MyLog(TAG, dbMsg);
        //       }
        //       catch (Exception er)
        //       {
        //           dbMsg += "<<以降でエラー発生>>" + er.Message;
        //           MyLog(TAG, dbMsg);
        //       }
        //   }


        /// <summary>
        /// テキストファイルを作成
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async static Task SaveFile(string path, string content) {//ファイルをセーブする
            string TAG = "SaveFile";
            string dbMsg = "";
            try {
                dbMsg += "path=" + path + "に\r\n" + content;
                await Task.Run(() => {
                    File.WriteAllText(path, content);
                });
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }

        }


        /// <summary>
        /// webのビデオ再生HTMLを作成する
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="webWidth"></param>
        /// <param name="webHeight"></param>
        /// <returns></returns>
        private string MakeVideoSouce(string fileName, int webWidth, int webHeight) {
            string TAG = "MakeVideoSouce";
            string dbMsg = "";
            string contlolPart = "";
            try {
                dbMsg += ",lsFullPathName=" + lsFullPathName;
                lsFullPathName = lsFullPathName.Replace(@":\\\", @":\\");
                dbMsg += ",fileName=" + fileName;
                string comentStr = "このタイプの表示は検討中です。";
                string[] extStrs = fileName.Split('.');
                string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
                string[] souceNames = fileName.Split(Path.DirectorySeparatorChar);
                string souceName = souceNames[souceNames.Length - 1];
                string mineTypeStr = GetFileTypeStr(fileName);      // "video/x-ms-asf";     //.asf
                dbMsg += ",mineTypeStr=" + mineTypeStr;
                string clsId = "";
                string codeBase = "";
                string dbWorning = "";

                if (lsFullPathName != "" && fileName != "未選択" && lsFullPathName != fileName) {       //8/31;仮対応；書き換わり対策
                    dbMsg += ",***書き換わり発生*<<" + lsFullPathName + " ; " + fileName + ">>";
                    fileName = lsFullPathName;
                }

                contlolPart = "<!DOCTYPE html>\n<html lang=" + '"' + "ja" + '"' + ">\n";
                contlolPart += "\t<head>\n\t\t<meta charset=utf-8>\n";
                contlolPart += "\t\t<meta http - equiv = " + '"' + "X - UA - Compatible" + '"' + " content = " + '"' + "IE = edge,chrome = 1" + '"' + " />\n";
                contlolPart += "\t</head>\n";

                if (extentionStr == ".webm" ||    // ★video要素、audio要素をJavaScriptから操作する   http://www.htmq.com/video/#ui 
                                                  //           extentionStr == ".flv" ||  //http://mrs.suzu841.com/mini_memo/numero_23.html
                    extentionStr == ".mp4" ||
                    extentionStr == ".ogv"
                    ) {
                    contlolPart += "\t<body style = " + '"' + "background-color: #000000;color:#333333;" + '"' + " onLoad=" + '"' + "myOnLoad()" + '"' + ">\n";
                    contlolPart += "\t\t<div class=" + '"' + "middle" + '"' + " style =" + '"' + "padding: 0px; " + '"' + " >\n";

                    contlolPart += "\t\t\t<video id=" + '"' + wiPlayerID + '"' + " controls style = " + '"' + "width:-webkit-fill-available ;height:-webkit-fill-available;text-align: center;" + '"' + ">\n";
                    contlolPart += "\t\t\t\t<source src=" + '"' + "file://" + fileName + '"' + " type=" + '"' + mineTypeStr + '"' + ">\n";
                    contlolPart += "\t\t\t</video>\n";

                    //                    src：動画ファイルの場所
                    //controls：動画コントロールを表示する
                    //muted：ミュート（消音）にする
                    //autoplay：Webページを表示した際に自動再生を行う(muted時のみ)
                    //playsinline：スマートフォンのブラウザでもWebページ内で再生する
                    //loop：動画を繰り返し再生する
                    //    comentStr = "読み込めないファイルは対策検討中です。。";
                } else if (extentionStr == ".flv" ||
                   extentionStr == ".f4v" ||
                   extentionStr == ".swf"
                   ) {
                    //  https://so-zou.jp/web-app/tech/html/sample/video.htm
                    //  https://tridentwebdesign.blog.fc2.com/blog-entry-279.html
                    Uri urlObj = new Uri(fileName);
                    if (urlObj.IsFile) {             //Uriオブジェクトがファイルを表していることを確認する
                        fileName = urlObj.AbsoluteUri;                 //Windows形式のパス表現に変換する
                        dbMsg += "Path=" + fileName;
                    }
                    dbMsg += ",assemblyPath=" + assemblyPath;
                    string[] urlStrs = assemblyPath.Split(Path.DirectorySeparatorChar);
                    assemblyName = urlStrs[urlStrs.Length - 1];
                    dbMsg += ">>" + assemblyName;
                    playerUrl = @assemblyPath.Replace(assemblyName, "fladance.swf");       //☆デバッグ用を\bin\Debugにコピーしておく
                                                                                           //		string nextMove = assemblyPath.Replace( assemblyName, "tonext.htm" );
                    dbMsg += ",playerUrl=" + playerUrl;
                    //,playerUrl=C:\Users\博臣\source\repos\file_tree_clock_web1\file_tree_clock_web1\bin\Debug\fladance.swf 
                    if (File.Exists(playerUrl)) {
                        dbMsg += ",Exists=true";
                    } else {
                        dbMsg += ",Exists=false";
                    }


                    clsId = "clsid:d27cdb6e-ae6d-11cf-96b8-444553540000";       //ブラウザーの ActiveX コントロール
                    codeBase = @"http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=10,0,0,0";
                    string pluginspage = @"http://www.macromedia.com/go/getflashplayer";
                    dbMsg += "[" + webWidth + "×" + webHeight + "]";        //4/3=1.3		1478/957=1.53  801/392=2.04
                    /*		if (4 / 3 < webWidth / webHeight) {
                                webWidth = webHeight/3*4;		.
                            } else {
                                webWidth = webHeight / 3 * 4;
                            }
                            dbMsg += ">>[" + webWidth + "×" + webHeight + "]";*/
                    string flashVvars = @"fms_app=&video_file=" + fileName + "&" +       // & amp;		"link_url ="+ nextMove + "&" +
                                             "image_file=&link_url=&autoplay=true&mute=false&controllbar=true&buffertime=10" + '"';
                    //常にバーを表示する
                    contlolPart += "\t<body style = " + '"' + "background-color: #000000;color:#333333;" + '"' + ">\n";
                    contlolPart += "\t\t<div class=" + '"' + "middle" + '"' + " style =" + '"' + "padding: 0px; " + '"' + " >\n";

                    contlolPart += "\t\t\t<object id=" + '"' + wiPlayerID + '"' +
                                        " classid=" + '"' + clsId + '"' +
                                    " codebase=" + '"' + codeBase + '"' +
                                    " width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
                                     ">\n";
                    contlolPart += "\t\t\t\t<param name=" + '"' + "FlashVars" + '"' + " value=" + '"' + flashVvars + "/>\n";   // + '"'  
                    contlolPart += "\t\t\t\t<param name= " + '"' + "allowFullScreen" + '"' + " value=" + '"' + "true" + '"' + "/>\n";
                    contlolPart += "\t\t\t\t<param name =" + '"' + "movie" + '"' + " value=" + '"' + playerUrl + '"' + "/>\n";
                    contlolPart += "\t\t\t\t<embed name=" + '"' + wiPlayerID + '"' +
                                                " src=" + '"' + playerUrl + '"' + "\n" +            // "file://" + fileName
                                                                                                    //		"left=-10 width=100% height= auto" +         // '"' + webWidth + '"'
                                     "\t\t\t\t\t width=" + '"' + webWidth + '"' + " height= " + '"' + webHeight + '"' +            // '"' + webWidth + '"'
                                                " type=" + '"' + mineTypeStr + '"' +
                                                " allowfullscreen=" + '"' + "true" + '"' + "\n" +
                                                "\t\t\t\t\t flashvars=" + '"' + flashVvars +          //+ '"' 
                                                " type=" + '"' + "application/x-shockwave-flash" + '"' +
                                                //"\n\t\t\t\t\t pluginspage=" + '"' + pluginspage + '"' +
                                                "/>\n";

                    // 「ふらだんす」http://www.streaming.jp/fladance/　を使っています。" + dbWorning;
                    /*
                     
                      <object classid="clsid:d27cdb6e-ae6d-11cf-96b8-444553540000" 
                                codebase="http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=10,0,0,0" 
                                width="横幅" height="高さ">
                        <param name = "flashvars" value = "fms_app=FMSアプリケーションディレクトリのパス&video_file=動画ファイルのパス&image_file=サムネイル画像のパス&link_url=リンク先のURL&autoplay=オートプレイのON・OFF&mute=ミュートのON・OFF&volume=音量&controller=操作パネルの表示・非表示&buffertime=バッファ時間" />
                        <param name="allowFullScreen" value="フルスクリーン化を可能にするかどうか" />
                        <param name="movie" value="ふらだんすswfファイルのパス" />

                            <embed src="ふらだんすswfファイルのパス" width="横幅" height="高さ" allowFullScreen="フルスクリーン化を可能にするかどうか" flashvars="fms_app=FMSアプリケーションディレクトリのパス&video_file=動画ファイルのパス&image_file=サムネイル画像のパス&link_url=リンク先のURL&autoplay=オートプレイのON・OFF&mute=ミュートのON・OFF&volume=音量&controller=操作パネルの表示・非表示&buffertime=バッファ時間" 
                                type="application/x-shockwave-flash" 
                                pluginspage="http://www.macromedia.com/go/getflashplayer" />
                    </object>
                     
                     
                     
                     
                     */
                    /*				playerUrl = assemblyPath.Replace(assemblyName, "flvplayer-305.swf");       //☆デバッグ用を\bin\Debugにコピーしておく
                                                                                                               //	string flashVvars = "fms_app=&video_file=" + fileName + "&" +       // & amp;
                                    contlolPart += "<object id=" + '"' + wiPlayerID + '"' +
                                                                                        " width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
                                                                                        " classid=" + '"' + clsId + '"' +
                                                                                    " codebase=" + '"' + codeBase + '"' +
                                                                                         //	" type=" + '"' + "application/x-shockwave-flash" + '"' +
                                                                                         //						" data=" + '"' + playerUrl + '"' +
                                                                                         ">\n";
                                    contlolPart += "\t\t\t<param name =" + '"' + "movie" + '"' + " value=" + '"' + playerUrl + '"' + "/>\n";
                                    contlolPart += "\t\t\t<param name=" + '"' + "allowFullScreen" + '"' + " value=" + '"' + "true" + '"' + "/>\n";
                                    contlolPart += "\t\t\t<param name=" + '"' + "FlashVars" + '"' + " value=" + '"' + fileName + '"' + "/>\n";
                                    contlolPart += "\t\t\t\t<embed name=" + '"' + wiPlayerID + '"' +
                                                                    " width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
                                                                    " src=" + '"' + playerUrl + '"' +
                                                                    " flashvars=" + '"' + fileName + '"' +           //" flashvars=" + '"' + @"flv=" + fileName + +'"' +
                                                                    " allowFullScreen=" + '"' + "true" + '"' +
                                                           ">\n";
                                    contlolPart += "\t\t\t\t</ embed>\n";
                                    comentStr = souceName + " ; プレイヤーには「Adobe Flash Player」https://www.mi-j.com/service/FLASH/player/index.html　を使っています。";
                    */

                    /*
                    playerUrl = assemblyPath.Replace( assemblyName, "player_flv_maxi.swf" );       //☆デバッグ用を\bin\Debugにコピーしておく
                                    contlolPart += "<object type=" + '"' + "application/x-shockwave-flash" + '"' +
                                                                                " data=" + '"' + playerUrl + '"' +
                                                                    " width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
                                                                     ">\n";
                                    contlolPart += "\t\t\t<param name =" + '"' + "movie" + '"' + " value=" + '"' + playerUrl + '"' + "/>\n";
                                    contlolPart += "\t\t\t<param name=" + '"' + "allowFullScreen" + '"' + " value=" + '"' + "true" + '"' + "/>\n";
                                    contlolPart += "\t\t\t<param name=" + '"' + "FlashVars" + '"' + " value=" + '"' + fileName + "&" +
                                                                                     "width=" + webWidth + "&" +
                                                                                     "height=" + webHeight + "&" +
                                                                                     "showstop=" + 1 + "&" +          //ストップボタンを表示
                                                                                     "showvolume=" + 1 + "&" +
                                                                                     "showtime=" + 1 + "&" +
                                                                                     "showfullscreen=" + 1 + "&" +
                                                                                                         "showplayer = always" +
                                        '"' + "/>\n";
                                    contlolPart += "\t\t\t\t<embed name=" + '"' + "monFlash" + '"' +
                                                                    " src=" + '"' + playerUrl + '"' +            // "file://" + fileName
                                                                    " flashvars=" + '"' + @"flv=" + fileName + +'"' +
                                                                    " pluginspage=" + '"' + pluginspage + '"' +
                                                                    " type=" + '"' + "application/x-shockwave-flash" + '"' +
                                                           "/>\n";
                                    comentStr = souceName + " ; プレイヤーには「FLV Player」http://flv-player.net/　を使っています。";


                                                    playerUrl = assemblyPath.Replace( assemblyName, "flaver.swf" );       //☆デバッグ用を\bin\Debugにコピーしておく
                            contlolPart += "<object id=" + '"' + "flvp" + '"' +
                                                                " data=" + '"' + playerUrl + '"' +
                                                            " type=" + '"' + "application/x-shockwave-flash" + '"' +
                                                            " width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
                                                            " ALIGN=" + '"' + "right" + '"' +
                                                             ">\n";
                                            contlolPart += "\t\t\t<param name =" + '"' + "movie" + '"' + " value=" + '"' + fileName + '"' + "/>\n";
                                            contlolPart += "\t\t\t<param name=" + '"' + "FlashVars" + '"' + " value=" + '"' + fileName + '"' + "/>\n";
                                            contlolPart += "\t\t\t<param name= " + '"' + "allowFullScreen" + '"' + " value=" + '"' + "true" + '"' + "/>\n";
                                            contlolPart += "\t\t\t<param name= " + '"' + "allowScriptAccess" + '"' + " value=" + '"' + "always" + '"' + "/>\n";
                                            comentStr = souceName + " ; プレイヤーには「FLAVER 3.0」http://rexef.com/webtool/flaver3/installation.html　を使っています。";


                                            */
                    /*-		//		fileName = "media.flv";
                                //	fileName = "file:///"+fileName.Replace( Path.DirectorySeparatorChar ,'/');
                                    codeBase = "http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,115,0";      //26,0,0,151は？
                                    contlolPart += "<object id=" + '"' + "flvp" + '"' +
                                                        //			" type=" + '"' + mineTypeStr + '"' +
                                                            //		 " data=" + '"' + fileName + //"&fullscreen=true" + '"' +
                                                            //		 " data=" + '"' + "player_flv_mini.swf" + '"' +
                                                        " classid=" + '"' + clsId + '"' +
                                                    " codebase=" + '"' + codeBase + '"' +
                                                    " width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
                                            //		" menu=" + true  +
                                                     ">\n";
                                //	contlolPart += "\t\t\t<param name =" + '"' + "movie" + '"' + " value=" + '"' + "flvplayer-305.swf" + '"' + "/>\n";
                                    contlolPart += "\t\t\t<param name =" + '"' + "bgcolor" + '"' + " value=" + '"' + "#FFFFFF" + '"' + "/>\n";
                                    //		contlolPart += "\t\t\t<param name= " + '"' + "bgcolor" + '"' + " value=" + '"' + "#fff" + '"' + "/>\n";
                                    contlolPart += "\t\t\t<param name =" + '"' + "loop" + '"' + " value=" + '"' + "false" + '"' + "/>\n";
                                    contlolPart += "\t\t\t<param name =" + '"' + "quality" + '"' + " value=" + '"' + "high" + '"' + "/>\n";
                                    contlolPart += "\t\t\t<param name =" + '"' + "menu" + '"' + " value=" + '"' + "true" + '"' + "/>\n";
                                    //		contlolPart += "\t\t\t<param name =" + '"' + "allowScriptAccess" + '"' + " value = " + '"' + "sameDomain" + '"' + "/>\n";
                                //	contlolPart += "\t\t\t<param name= " + '"' + "allowScriptAccess" + '"' + " value=" + '"' + "always" + '"' + "/>\n";
                                        contlolPart += "\t\t\t<param name=" + '"' + "FlashVars" + '"' + " value=" + '"' +
                                                                            "src=" + fileName + "&" +       // & amp;
                                                                 //			"flvmov=" + fileName + "&" +       // & amp;
                                                                 //		"flv=" + fileName +"&" +       // & amp;
                                                                 "width=" + webWidth + "&" + "height=" + webHeight + "&" +
                                                                 "showstop=" + 1 + "&" +                              //ストップボタンを表示
                                                                 "showvolume=" + 1 + "&" +                            //showvolume
                                                                 "showtime=" + 1 + "&" +                              //時間を表示
                                                                 "showfullscreen=" + 1 + "&" +                        //全画面表示ボタンを表示
                                                                 "showplayer=always" + '"' + "/>\n";                        //常にバーを表示する


                                    contlolPart += "\t\t\t\t<embed name=" + '"' + "flvp" + '"' +
                                                                    " type=" + '"' + mineTypeStr + '"' +
                                                                    " src=" + '"' + fileName + '"' +            // "file://" + fileName
                                                                                                                //		" allowScriptAccess=" + '"' + " sameDomain= " + '"' +
                                                                    " width=" + '"' + webWidth + '"' + " height= " + '"' + webHeight + '"' + " bgcolor=" + '"' + "#FFFFFF" + '"' +
                                                                    " pluginspage=" + '"' + pluginspage + '"' + 
                                                                    " loop=" + '"' + "false" + '"' + " quality=" + '"' + "high" + '"' +
                                                           "/>\n";
                    */
                    //グローバルセキュリティ設定パネルで)「これらの場所にあるファイルを常に信頼する」で、[追加]-[フォルダを参照]にローカルディスクを登録する？
                    //	http://www.macromedia.com/support/documentation/jp/flashplayer/help/settings_manager04.html
                    //属性指定は	https://helpx.adobe.com/jp/flash/kb/231465.html
                    //C#でFlashファイルを読み込み表示する	http://sivaexstrage.orz.hm/blog/softwaredevelopment/800
                    //		contlolPart += "\n\t\t< param name = " + '"' + "FlashVars" + '"' + "value = " + '"' + "flv= + '"' +fileName + '"' +"&autoplay=1&margin=0" + '"' + "/>\n\t\t\t";
                    contlolPart += "\t\t</object>\n";
                } else if (extentionStr == ".rm") {
                    contlolPart += "\t</head>\n";
                    contlolPart += "\t<body style = " + '"' + "background-color: #000000;color:#ffffff;" + '"' + " >\n";
                    clsId = "clsid:CFCDAA03-8BE4-11CF-B84B-0020AFBBCCFA";       //ブラウザーの ActiveX コントロール
                    contlolPart += "\t\t<object  id=" + '"' + wiPlayerID + '"' +
                                        "  classid=" + '"' + clsId + '"' +
                                        " width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
                                     ">\n";
                    contlolPart += "\t\t\t<param name =" + '"' + "src" + '"' + " value=" + '"' + fileName + '"' + "/>\n";
                    contlolPart += "\t\t\t<param name =" + '"' + "AUTOSTART" + '"' + " value=" + '"' + "TRUEF" + '"' + "/>\n";
                    contlolPart += "\t\t\t<param name =" + '"' + "CONTROLS" + '"' + " value=" + '"' + "All" + '"' + "/>\n"; //http://www.tohoho-web.com/wwwmmd3.htm
                    contlolPart += "\t\t</object>\n";
                } else if (extentionStr == ".wmv" ||        //ver9:Windows Media 形式
                    extentionStr == ".asf" ||
                    extentionStr == ".wm" ||
                    extentionStr == ".asx" ||        //ver9:Windows Media メタファイル 
                    extentionStr == ".wax" ||        //ver9:Windows Media メタファイル 
                    extentionStr == ".wvx" ||        //ver9:Windows Media メタファイル 
                    extentionStr == ".wmx" ||        //ver9:Windows Media メタファイル 
                    extentionStr == ".ivf" ||        //ver10:Indeo Video Technology
                    extentionStr == ".dvr-ms" ||        //ver12:Microsoft デジタル ビデオ録画
                    extentionStr == ".m2ts" ||        //ver12:MPEG-2 TS ビデオ ファイル 
                    extentionStr == ".ts" ||
                    extentionStr == ".mpg" ||
                    extentionStr == ".m1v" ||
                    extentionStr == ".mp2" ||
                    extentionStr == ".mpa" ||
                    extentionStr == ".mpe" ||
                    //      extentionStr == ".mp4" ||        //ver12:MP4 ビデオ ファイル 
                    extentionStr == ".m4v" ||
                    extentionStr == ".mp4v" ||
                    extentionStr == ".mpeg" ||
                    extentionStr == ".mpeg" ||
                    extentionStr == ".mpeg" ||
                    extentionStr == ".3gp" ||
                    extentionStr == ".3gpp" ||
                    extentionStr == ".qt" ||
                    extentionStr == ".mov"       //ver12:QuickTime ムービー ファイル 
                    ) {
                    /*	contlolPart += "\t\t\t<script type=" + '"' + "text/javascript" + '"' + " > \n";

                        contlolPart += "\t\t\t</script>\n\n";

                    contlolPart += "\t\t\t<script for=" + '"' + wiPlayerID + '"' +            //"MediaPlayer"		document.getElementById( )
                                                    " event=" + '"' + "PlayStateChange(lOldState, lNewState)" + '"' +
                                                    " type=" + '"' + "text/javascript" + '"' + ">\n";

                    contlolPart += "\t\t\t\tdocument.getElementById(" + '"' + wiPlayerID + '"' + " ).PlayStateChanged = function( old_state, new_state ){\n" +
                                    "\t\t\t\t\t var comentStr =" + "new_state" + ";\n" +
                                    "\t\t\t\t\t switch (new_state) {\n" +
                                    "\t\t\t\t\t\tcase 0:\n" +
                                    "\t\t\t\t\t\t comentStr =" + '"' + "Windows Media Player の状態が定義されません。" + '"' + "\n" +
                                    "\t\t\t\t\t\tbreak;\n" +
                                    "\t\t\t\t\t\tcase 8:\n" +
                                    "\t\t\t\t\t\t comentStr =" + '"' + "メディアの再生が完了し、最後の位置にあります。" + '"' + "\n" +
                                    "\t\t\t\t\t\tbreak;\n" +
                                    "\t\t\t\t\t}\n" +
                                    "\t\t\t\t\t alert( " +  "comentStr" + " );\n" +         // it_dispRate.value = mplayer.Rate;	 '"' + "+new_state" + 
                                    "\t\t\t\t\t document.getElementById(" + '"' + "statediv" + '"' + ").innerHTML = "  + "comentStr"  + ";\n" +
                                    "\t\t\t\t" + "}\n" +
                                    "\t\t\t</script>\n\n";          //https://msdn.microsoft.com/ja-jp/library/cc364798.aspx

                        contlolPart += "\t\t\t<script for=" + '"' + wiPlayerID + '"' + " event=" + '"' + "EndOfStream(lResult)" + '"' + 
                                            "\t\t\t\t type=" + '"' + "text/javascript" + '"' + ">\n" +
                                                            //		"\t\t\t\t\t alert( " + '"' + "EndOfStream" + '"' + " );\n" +
                                            "\t\t\t\t\t document.getElementById(" + '"' + "statediv" + '"' + ").innerHTML = " +'"' + "次へ" + '"' + ";\n" +
                                            "\t\t\t</script>\n\n";          //http://www.tohoho-web.com/wwwmmd2.htm
                        */
                    contlolPart += "\t\t</head>\n";
                    contlolPart += "\t\t<body style = " + '"' + "background-color: #000000;color:#ffffff;" + '"' + " >\n";
                    clsId = "CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6";   //Windows Media Player9
                    contlolPart += "\t\t\t<object classid =" + '"' + clsId + '"' + " id=" + '"' + wiPlayerID + '"' + "  width = " + '"' + webWidth + '"' + " height = " + '"' + webHeight + '"' + ">\n";
                    contlolPart += "\t\t\t\t<param name =" + '"' + "url" + '"' + "value = " + '"' + "file://" + fileName + '"' + "/>\n";
                    contlolPart += "\t\t\t\t<param name =" + '"' + "stretchToFit" + '"' + " value = true />\n";//右クリックして縮小/拡大で200％
                    contlolPart += "\t\t\t\t<param name =" + '"' + "autoStart" + '"' + " value = " + true + "/>\n";
                    //     contlolPart += "\t\t\t\t<param name =" + '"' + "Volume" + '"' + " value = " + appSettings.SoundVolume + "/>\n";
                    contlolPart += "\t\t\t</object>\n";
                    comentStr = souceName + " ; " + "Windows Media Playerで読み込めないファイルは対策検討中です。";
                    ///参照 http://so-zou.jp/web-app/tech/html/sample/embed-video.htm/////
                    /////https://support.microsoft.com/ja-jp/help/316992/file-types-supported-by-windows-media-player
                } else {
                    comentStr = "この形式は対応確認中です。";
                }
                contlolPart += "\t\t</div>\n";
                if (contlolPart.Contains("<video")) {
                    //contlolPart += "\t\t<div style=" + '"' + "background-color:#333333; color:#ffffff;" + '"' + ">\n";
                    //contlolPart += "\t\t\t<input type=" + '"' + "button" + '"' + " value=" + '"' + "再生" + '"' + " onClick=" + '"' + "playVideo()" + '"' + ">\n";
                    //contlolPart += "\t\t\t<input type=" + '"' + "button" + '"' + " value=" + '"' + "一時停止" + '"' + " onClick=" + '"' + "pauseVideo()" + '"' + ">\n";
                    //contlolPart += "\t\t\t<input type=" + '"' + "button" + '"' + " value=" + '"' + "↑" + '"' + " onClick=" + '"' + "upVolume()" + '"' + ">\n";
                    //contlolPart += "\t\t\t<input type=" + '"' + "button" + '"' + " value=" + '"' + "↓" + '"' + " onClick=" + '"' + "downVolume()" + '"' + ">\n";
                    ////contlolPart += "\t\t\t現在（秒）<span id=" + '"' + "currentTimeSP" + '"' + ">0</span>\n";
                    ////contlolPart += "\t\t\t / <span id=" + '"' + "durationSP" + '"' + "></span>\n";
                    ////contlolPart += "\t\t\t<span id=" + '"' + "kanryou" + '"' + "></span>\n";
                    //contlolPart += "\t\t</div>\n";
                    contlolPart += "\t\t<script type=" + '"' + "text/javascript" + '"' + ">\n";
                    contlolPart += "\t\t\tvar v = document.getElementById(" + '"' + wiPlayerID + '"' + ");\n";
                    //contlolPart += "\t\t\tfunction getHMS(sec) {\n";
                    //contlolPart += "\t\t\t\tvar retStr = " + '"' + '"' + ";\n";
                    //contlolPart += "\t\t\t\tvar retH = Math.floor(sec/3600);\n";
                    //contlolPart += "\t\t\t\tif(0 < retH){\n";
                    //contlolPart += "\t\t\t\t\tretStr = retH + " + '"' + ":" + '"' + ";\n";
                    //contlolPart += "\t\t\t\t\tsec = sec - retH * 3600;\n";
                    //contlolPart += "\t\t\t\t}\n";
                    //contlolPart += "\t\t\t\tvar retM = Math.floor(sec/60);\n";
                    //contlolPart += "\t\t\t\tif(0 < retM){\n";
                    //contlolPart += "\t\t\t\t\tif(retM < 10){\n";
                    //contlolPart += "\t\t\t\t\t\tretStr += " + '"' + "0" + '"' + " + retM + " + '"' + ":" + '"' + ";\n";
                    //contlolPart += "\t\t\t\t\t}else{\n";
                    //contlolPart += "\t\t\t\t\t\tretStr += retM + " + '"' + ":" + '"' + ";\n";
                    //contlolPart += "\t\t\t\t\t}\n";
                    //contlolPart += "\t\t\t\t\tsec = sec - retM * 60;\n";
                    //contlolPart += "\t\t\t\t}else{\n";
                    //contlolPart += "\t\t\t\t\tretStr += " + '"' + "00:" + '"' + ";\n";
                    //contlolPart += "\t\t\t\t}\n";
                    //contlolPart += "\t\t\t\tsec = Math.floor(sec);\n";
                    //contlolPart += "\t\t\t\tif(0 < sec){\n";
                    //contlolPart += "\t\t\t\t\tif(sec < 10){\n";
                    //contlolPart += "\t\t\t\t\t\tretStr += " + '"' + "0" + '"' + " + sec;\n";
                    //contlolPart += "\t\t\t\t\t}else{\n";
                    //contlolPart += "\t\t\t\t\t\tretStr += sec;\n";
                    //contlolPart += "\t\t\t\t\t}\n";
                    //contlolPart += "\t\t\t\t}else{\n";
                    //contlolPart += "\t\t\t\t\tretStr = retStr + " + '"' + "00" + '"' + ";\n";
                    //contlolPart += "\t\t\t\t}\n";
                    //contlolPart += "\t\t\t\treturn retStr;\n";
                    //contlolPart += "\t\t\t}\n\n";
                    //contlolPart += "\t\t\tfunction getDuration() {\n";               //動画の長さ（秒）を表示
                    //contlolPart += "\t\t\t\tvar myDuration = v.duration;\n";
                    //contlolPart += "\t\t\t\treturn myDuration;\n";
                    //contlolPart += "\t\t\t}\n\n";
                    //contlolPart += "\t\t\tfunction getCurrentTime() {\n";                //現在の再生位置（秒）を表示
                    //contlolPart += "\t\t\t\tvar myCurrentTime = v.currentTime;\n";
                    //contlolPart += "\t\t\t\treturn myCurrentTime;\n";
                    //contlolPart += "\t\t\t}\n\n";
                    //contlolPart += "\t\t\tfunction playVideo() {\n";                //再生完了の表示をクリア
                    ////動画を再生
                    //contlolPart += "\t\t\t\tv.play();\n";
                    //contlolPart += "\t\t\t\tisEnded=false;\n";
                    //contlolPart += "\t\t\t}\n\n";
                    //contlolPart += "\t\t\tfunction pauseVideo() {\n";            //動画を一時停止
                    //contlolPart += "\t\t\t\tv.pause();\n";
                    //contlolPart += "\t\t\t}\n\n";
                    //contlolPart += "\t\t\tfunction upVolume() {\n";            //音量を上げる
                    //contlolPart += "\t\t\t\tv.volume = v.volume + 0.25;\n";
                    //contlolPart += "\t\t\t}\n\n";
                    //contlolPart += "\t\t\tfunction downVolume() {\n";            //音量を下げる
                    //contlolPart += "\t\t\t\tv.volume = v.volume - 0.25;\n";
                    //contlolPart += "\t\t\t}\n\n";
                    contlolPart += "\t\t\tfunction myOnLoad() {\n";
                    //現在の再生位置が変更された時
                    contlolPart += "\t\t\t\tv.addEventListener(" + '"' + "timeupdate" + '"' + ", function(){\n";
                    //	contlolPart += "\t\t\t\t\tvar myCurrentTime = v.currentTime;\n";
                    contlolPart += "\t\t\t\t\twindow.chrome.webview.postMessage(v.currentTime+" + '"' + '"' + ");\n";
                    contlolPart += "\t\t\t\t}, false);\n";            //メディアリソースの末尾に達して、再生が停止した時
                                                                      //メディアリソースの末尾に達して、再生が停止した時
                    contlolPart += "\t\t\t\tv.addEventListener(" + '"' + "ended" + '"' + ", function(){\n";
                    contlolPart += "\t\t\t\t\twindow.chrome.webview.postMessage(" + '"' + "ended" + '"' + ");\n";
                    contlolPart += "\t\t\t\t});\n";
                    //            v.addEventListener("loadeddata", function(){
                    //                playVideo();
                    //            }, false);
                    contlolPart += "\t\t\t}\n";
                    ////メディアリソースの末尾に達して、再生が停止した時
                    //contlolPart += "\t\t\tv.onended = function ( event ) {\n";
                    //contlolPart += "\t\t\t\twindow.chrome.webview.postMessage(" + '"' + "ended" + '"' + ");\n";
                    //contlolPart += "\t\t\t};\n";
                    contlolPart += "\t\t</script>\n";

                }
                contlolPart += "\t</body>\n</html>\n";
                MyLog(TAG, dbMsg);
            } catch (Exception er) {
                MyErrorLog(TAG, dbMsg, er);
            }
            return contlolPart;
        }           //Video用のタグを作成

        private string MakeImageSouce(string fileName, int webWidth, int webHeight) {
            string TAG = "[MakeImageSouce]";
            string dbMsg = TAG;
            string contlolPart = "";
            string comentStr = "";
            string[] extStrs = fileName.Split('.');
            string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
            contlolPart += "\t</head>\n";
            contlolPart += "\t<body style = " + '"' + "background-color: #000000;color:#ffffff;" + '"' + " >\n\t\t";
            if (extentionStr == ".jpg" ||
                extentionStr == ".jpeg" ||
                extentionStr == ".png" ||
                extentionStr == ".gif"
                ) {
            } else {
                /*	 ".tif", ".ico", ".bmp" };*/
                comentStr = "静止画はimgタグで読めるもののみ対応しています。";
            }
            contlolPart += "\n\t\t<img src = " + '"' + fileName + '"' + " style=" + '"' + "width:100%" + '"' + "/>\n";
            // + '"' + webWidth + '"' + " height = " + '"' + webHeight + '"' +
            contlolPart += "\t\t<div>\n\t\t\t" + comentStr + "\n\t\t</div>\n";
            MyLog(TAG, dbMsg);
            return contlolPart;
        }  //静止画用のタグを作成

        private string MakeAudioSouce(string fileName, int webWidth, int webHeight) {
            string TAG = "[MakeAudioSouce]";
            string dbMsg = TAG;
            string contlolPart = "";
            contlolPart += "\t</head>\n";
            contlolPart += "\t<body style = " + '"' + "background-color: #000000;color:#ffffff;" + '"' + " >\n\t\t";
            string comentStr = "";
            string[] extStrs = fileName.Split('.');
            string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
            string[] souceNames = fileName.Split(Path.DirectorySeparatorChar);
            string souceName = souceNames[souceNames.Length - 1];

            if (
                extentionStr == ".ogg"
                ) {
                //		contlolPart += "<div class=" + '"' + "video-container" + '"' + ">\n";
                contlolPart += "\t\t\t<audio src=" + '"' + "file://" + fileName + '"' + " controls autoplay style = " + '"' + "width:100%" + '"' + " />\n";
                comentStr = "audioタグで読み込めないファイルは対策検討中です。。";
            } else if (extentionStr == ".ra") {
                string clsId = "clsid:CFCDAA03-8BE4-11CF-B84B-0020AFBBCCFA";       //ブラウザーの ActiveX コントロール
                contlolPart += "<objec id=" + '"' + wiPlayerID + '"' +
                                    "  classid=" + '"' + clsId + '"' +
                                    " width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
                                 ">\n";
                contlolPart += "\t\t\t<param name =" + '"' + "src" + '"' + " value=" + '"' + fileName + '"' + "/>\n";
                contlolPart += "\t\t\t<param name =" + '"' + "AUTOSTART" + '"' + " value=" + '"' + "TRUEF" + '"' + "/>\n";
                //	contlolPart += "\t\t\t<param name =" + '"' + "CONTROLS" + '"' + " value=" + '"' + "All" + '"' + "/>\n"; //http://www.tohoho-web.com/wwwmmd3.htm
            } else if (extentionStr == ".wma" ||
                extentionStr == ".wvx" ||
                extentionStr == ".wax" ||
                extentionStr == ".wav" ||
                extentionStr == ".m4a" ||           //var12;MP4 オーディオ ファイル
                extentionStr == ".mp3" ||
                extentionStr == ".aac" ||
                extentionStr == ".m4a" ||           //iTurne				extentionStr == ".midi" ||           //var9;MIDI 
                extentionStr == ".mid" ||           //var9;MIDI 
                extentionStr == ".rmi" ||           //var9;MIDI 
                extentionStr == ".aif" ||           //var9;Audio Interchange File FormatI 
                extentionStr == ".aifc" ||           //var9;Audio Interchange File FormatI 
                extentionStr == ".aiff" ||           //var9;Audio Interchange File FormatI 
                extentionStr == ".au" ||           //var9;Sun Microsystems および NeXT  
                extentionStr == ".snd" ||           //var9;Sun Microsystems および NeXT  
                extentionStr == ".wav" ||           //var9;Windows 用オーディオ   
                extentionStr == ".cda" ||           //var9;CD オーディオ トラック 
                extentionStr == ".adt" ||           //var12;Windows オーディオ ファイル 
                extentionStr == ".adts" ||           //var12;Windows オーディオ ファイル 
                extentionStr == ".asx"
                ) {
                string clsId = "CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6";   //Windows Media Player9
                contlolPart += "\n\t\t<div><object id=" + '"' + wiPlayerID + '"' +
                                    "  classid =" + '"' + clsId + '"' + " style = " + '"' + "width:100%;higth :90%" + '"' + " >\n";
                contlolPart += "\t\t\t<param name =" + '"' + "url" + '"' + "value = " + '"' + "file://" + fileName + '"' + "/>\n";
                contlolPart += "\t\t\t<param name =" + '"' + "stretchToFit" + '"' + " value = true />\n";//右クリックして縮小/拡大で200％
                contlolPart += "\t\t\t<param name =" + '"' + "autoStart" + '"' + " value = " + true + "/></div>\n<br><div style=" + '"' + "top:96%" + '"' + ">";
                comentStr = "\t\t\t<pre>" + souceName + " " + " ; Windows Media Player読み込めないファイルは対策検討中です。</pre></div>\n";
                //この行が表示されない
                /*		contlolPart += "<ASX VERSION =" + '"' + "3.0"  + '"' + " >\n";
						contlolPart += "\t\t<ENTRY >\n";
						contlolPart += "\t\t\t<REF HREF =" + '"' +  fileName + '"' + " >\n";//"file://" +
						contlolPart += "\t\t\t</ENTRY >\n";
						contlolPart += "\t\t\t</ASX >\n";
						  comentStr = "ASXタグで確認中です。(Windows Media Player　がサポートしている形式)";*/
            } else {
                /* ".ra", ".flac",  }; */
                comentStr = "このファイルの再生方法は確認中です。";
            }
            contlolPart += "\t\t<div>\n\t\t\t" + comentStr + "\n\t\t</div>\n";
            MyLog(TAG, dbMsg);
            return contlolPart;
        }  //静止画用のタグを作成selectNode

        private string MakeTextSouce(string fileName, int webWidth, int webHeight) {
            string TAG = "[MakeTextSouce]";
            string dbMsg = TAG;
            string contlolPart = "";
            string comentStr = "";
            dbMsg += ",fileName=" + fileName;

            string rText = ReadTextFile(fileName, "UTF-8"); //"Shift_JIS"では文字化け発生
            dbMsg += ",rText=" + rText;

            string[] extStrs = fileName.Split('.');
            string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
            contlolPart += "\t\t<pre>\n";
            if (extentionStr == ".htm" ||
                extentionStr == ".html" ||
                extentionStr == ".xhtml" ||
                extentionStr == ".xml" ||
                extentionStr == ".rss" ||
                extentionStr == ".xml" ||
                extentionStr == ".css" ||
                extentionStr == ".js" ||
                extentionStr == ".vbs" ||
                extentionStr == ".cgi" ||
                extentionStr == ".php"
                ) {
                rText = rText.Replace("<", "&lt;");
                rText = rText.Replace(">", "&gt;");
                contlolPart += rText;
            } else if (extentionStr == ".txt") {
                contlolPart += "\t\t\t" + rText + "\n";
            } else {
                comentStr = "このファイルの表示方法は確認中です。";
            }
            contlolPart += "\t\t</pre>\n";
            contlolPart += "\t\t<div>\n\t\t\t" + comentStr + "\n\t\t</div>\n";
            MyLog(TAG, dbMsg);
            return contlolPart;
        }  //Text用のタグを作成		

        private string MakeApplicationeSouce(string fileName, int webWidth, int webHeight) {
            string TAG = "[MakeApplicationeSouce]";
            string dbMsg = TAG;
            string contlolPart = "";
            string comentStr = "";
            string[] extStrs = fileName.Split('.');
            string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
            if (extentionStr == ".wmx" ||        //ver9:Windows Media Player スキン 
                extentionStr == ".wms" ||        //ver9:Windows Media Player スキン  
                extentionStr == ".wmz" ||     //ver9:Windows Media Player スキン  
                extentionStr == ".wms" ||     //ver9:Windows Media Player スキン  
                extentionStr == ".m3u" ||   //MPEGだがrealPlayyerのプレイリスト
                extentionStr == ".wmd"     //ver9:Windows Media Download パッケージ   
                ) {
                string clsId = "CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6";   //Windows Media Player9
                contlolPart += "\n\t\t<object classid =" + '"' + clsId + '"' + " style = " + '"' + "width:100%" + '"' + " >\n";
                contlolPart += "\t\t\t<param name =" + '"' + "url" + '"' + "value = " + '"' + "file://" + fileName + '"' + "/>\n";
                contlolPart += "\t\t\t<param name =" + '"' + "stretchToFit" + '"' + " value = true />\n";//右クリックして縮小/拡大で200％
                contlolPart += "\t\t\t<param name =" + '"' + "autoStart" + '"' + " value = " + true + "/>\n";
                comentStr = "Windows Media Player9読み込めないファイルは対策検討中です。";
            } else {
                comentStr = "このファイルの再生方法は確認中です。";
            }
            contlolPart += "\t\t<div>\n\t\t\t" + comentStr + "\n\t\t</div>\n";
            MyLog(TAG, dbMsg);
            return contlolPart;
        }  //アプリケーション用のタグを作成
           //        //windows media Player////////////////////////////////////////////////////////////Flash//
           //        #region WMPBlock
           //        /// <summary>
           //        /// windows media Playerを生成
           //        /// </summary>
           //        /// <param name="fileName"></param>
           //        private void MakeWMP(string fileName)
           //        {
           //            string TAG = "[MakeWMP]" + fileName;
           //            string dbMsg = TAG;
           //            try
           //            {
           //                InitPlayerPane("WMP");
           //                //if (this.playerWebBrowser != null)
           //                //{
           //                //    this.MediaPlayerPanel.Controls.Remove(this.playerWebBrowser);
           //                //    this.playerWebBrowser = null;
           //                //}
           //                if (this.mediaPlayer == null)
           //                {
           //                    this.mediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
           //                    ((System.ComponentModel.ISupportInitialize)(this.mediaPlayer)).BeginInit();
           //                    //this.SuspendLayout();
           //                    //this.MediaPlayerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top
           //                    //                                                                     | System.Windows.Forms.AnchorStyles.Bottom)
           //                    //                                                                     | System.Windows.Forms.AnchorStyles.Left)
           //                    //                                                                     | System.Windows.Forms.AnchorStyles.Right)));
           //                    //this.MediaPlayerPanel.BackColor = System.Drawing.SystemColors.Control;
           //                    this.MediaPlayerPanel.Controls.Add(this.mediaPlayer);
           //                    //this.MediaPlayerPanel.Controls.Add(this.progresPanel);
           //                    //this.MediaPlayerPanel.Location = new System.Drawing.Point(0, 0);
           //                    //this.MediaPlayerPanel.Name = "MediaPlayerPanel";
           //                    //this.MediaPlayerPanel.Size = new System.Drawing.Size(this.MediaPlayerPanel.Width, this.MediaPlayerPanel.Height);
           //                    //this.MediaPlayerPanel.TabIndex = 0;
           //                    this.mediaPlayer.uiMode = "none";
           //                    this.mediaPlayer.Enabled = true;
           //                    this.mediaPlayer.Location = new System.Drawing.Point(0, 0);
           //                    this.mediaPlayer.Name = "mediaPlayer";
           //                    this.mediaPlayer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer.OcxState")));
           //                    this.mediaPlayer.Size = new System.Drawing.Size(this.MediaPlayerPanel.Width, this.MediaPlayerPanel.Height);
           //                    this.mediaPlayer.TabIndex = 5;
           //                    ((System.ComponentModel.ISupportInitialize)(this.mediaPlayer)).EndInit();
           //                    this.ResumeLayout(false);
           //                    this.mediaPlayer.settings.autoStart = false;                    //ファイル読込後の自動再生
           //                    VolBar.Maximum = 100;
           //                    this.mediaPlayer.OpenStateChange += new AxWMPLib._WMPOCXEvents_OpenStateChangeEventHandler(this.WMP_OpenStateChange);
           //                    this.mediaPlayer.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(this.WMP_PlayStateChange);   //再生イベントリスナー
           //                }
           //                CurrentPosition = 0;
           //                this.mediaPlayer.URL = fileName;
           //                SettingsVolum = this.mediaPlayer.settings.volume;
           //                VolBar.Value = SettingsVolum;
           //                VolLabel.Text = VolBar.Value.ToString();
           //                PlayPouseButton.PerformClick();
           //                MyLog(TAG, dbMsg);
           //            }
           //            catch (Exception er)
           //            {
           //                this.mediaPlayer = null;
           //                dbMsg += "<<以降でエラー発生>>" + er.Message;
           //                MyLog(TAG, dbMsg);
           //            }
           //        }
           //        //AxWindowsMediaPlayer  https://so-zou.jp/software/tech/programming/c-sharp/media/video/ax-windows-media-player/
           //        /// <summary>
           //        /// 再生状況で発生するイベント
           //        /// 自動送りに使用
           //        /// </summary>
           //        /// <param name="sender"></param>
           //        /// <param name="e"></param>
           //        /// https://msdn.microsoft.com/ja-jp/library/cc411009.aspx
           //        private void WMP_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
           //        {
           //            string TAG = "[WMP_PlayStateChange]";
           //            string dbMsg = TAG;
           //            try
           //            {
           //                string rStr = PlayTitolLabel.Text + "\n";
           //                dbMsg += e.newState + ";";
           //                switch (e.newState)
           //                {
           //                    case 0:
           //                        //          rStr += "Undefined;Windows Media Player の状態が定義されません。"; 
           //                        dbMsg += "Undefined;Windows Media Player の状態が定義されません。";
           //                        break;
           //                    case 1:
           //                        dbMsg += "Stopped;現在のメディア クリップの再生が停止されています。";
           //                        break;
           //                    case 2:
           //                        dbMsg += "Paused;現在のメディア クリップの再生が一時停止されています。メディアを一時停止した場合は、再生が同じ位置から再開されます。";
           //                        break;
           //                    case 3:
           //                        dbMsg += "Playing;現在のメディア クリップは再生中です。";
           //                        break;
           //                    case 4:
           //                        dbMsg += "ScanForward;現在のメディア クリップは早送り中です";
           //                        break;
           //                    case 5:
           //                        dbMsg += "ScanReverse;現在のメディア クリップは巻き戻し中です。";
           //                        break;
           //                    case 6:
           //                        dbMsg += "Buffering;現在のメディア クリップはサーバーからの追加情報を取得中です。";
           //                        break;
           //                    case 7:
           //                        dbMsg += "Waiting;接続は確立されましたが、サーバーがビットを送信していません。セッションの開始を待機中です。";
           //                        break;
           //                    case 8:
           //                        dbMsg += "MediaEnded;メディアの再生が完了し、最後の位置にあります。";
           //                        PlayPouseButton.PerformClick();
           //                        plNextBbutton.PerformClick();
           //                        break;
           //                    case 9:
           //                        dbMsg += "Transitioning;新しいメディアを準備中です。";
           //                        break;
           //                    case 10:
           //                        dbMsg += "Ready;再生を開始する準備ができています。";
           //                        if (this.mediaPlayer.playState != WMPLib.WMPPlayState.wmppsPlaying)
           //                        {
           //                            PlayPouseButton.PerformClick();
           //                        }
           //                        break;
           //                }
           //                //      PlayTitolLabel.Text = (rStr);
           //                MyLog(TAG, dbMsg);
           //            }
           //            catch (Exception er)
           //            {
           //                dbMsg += "<<以降でエラー発生>>" + er.Message;
           //                MyLog(TAG, dbMsg);
           //            }
           //        }

        //        /// <summary>
        //        /// 
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        /// http://blog.code-life.net/blog/2011/09/05/how-to-use-windows-media-player-activex-controll-3/
        //        /// https://msdn.microsoft.com/ja-jp/library/cc411001.aspx
        //        private void WMP_OpenStateChange(object sender, AxWMPLib._WMPOCXEvents_OpenStateChangeEvent e)
        //        {
        //            string TAG = "[WMP_OpenStateChange]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += e.newState + ";";
        //                switch (e.newState)
        //                {
        //                    case 0:
        //                        dbMsg += "Undefined;WindowsMediaPlayerの状態が定義されていません。";
        //                        //            PlayTitolLabel.Text = ("Undefined;WindowsMediaPlayerの状態が定義されていません");
        //                        break;
        //                    case 1:
        //                        dbMsg += "PlaylistChanging;新しい再生リストが読み込まれようとしています。";
        //                        break;
        //                    case 2:
        //                        dbMsg += "PlaylistLocating;Windows Media Player が再生リストを探しています。再生リストは、ローカル (データベースまたはテキスト ファイル) でも、リモートでもかまいません。";
        //                        break;
        //                    case 3:
        //                        dbMsg += "PlaylistConnecting;再生リストに接続中です。";
        //                        break;
        //                    case 4:
        //                        dbMsg += "PlaylistLoading;再生リストが検出され、現在取り込んでいます";
        //                        break;
        //                    case 5:
        //                        dbMsg += "PlaylistOpening;再生リストは取得済みで、現在解析され、読み込み中です。";
        //                        break;
        //                    case 6:
        //                        dbMsg += "PlaylistOpenNoMedia;再生リストは開いています";
        //                        break;
        //                    case 7:
        //                        dbMsg += "PlaylistChanged;新しい再生リストが currentPlaylist に割り当てられました。";
        //                        break;
        //                    case 8:
        //                        dbMsg += "MediaChanging;新しいメディアが読み込まれようとしています。";
        //                        break;
        //                    case 9:
        //                        dbMsg += "MediaLocating;Windows Media Player がメディア ファイルを検索中です。ファイルは、ローカルでもリモートでもかまいません。";
        //                        break;
        //                    case 10:
        //                        dbMsg += "MediaConnecting;メディアを保持しているサーバーに接続中です。";
        //                        break;
        //                    case 11:
        //                        dbMsg += "MediaLoading;メディアが検出され、現在取得中です。";
        //                        break;
        //                    case 12:
        //                        dbMsg += "MediaOpening;メディアは取得済みで、現在開いているところです。";
        //                        break;
        //                    case 13:
        //                        dbMsg += "MediaOpen;メディアは現在開いています";
        //                        break;
        //                    case 14:
        //                        dbMsg += "BeginCodecAcquistion;コーデックの取得を開始してす";
        //                        break;
        //                    case 15:
        //                        dbMsg += "EndCodecAcquisition;コーデックの取得が完了しました。";
        //                        break;
        //                    case 16:
        //                        dbMsg += "BeginLicenseAcquisition;DRM 保護付きのコンテンツを再生するライセンスを取得中です。";
        //                        break;
        //                    case 17:
        //                        dbMsg += "EndLicenseAcquisition;DRM 保護付きのコンテンツを再生するライセンスを取得しました。";
        //                        break;
        //                    case 18:
        //                        dbMsg += "BeginIndividualization;DRM 個別化を開始しました。";
        //                        break;
        //                    case 19:
        //                        dbMsg += "EndIndividualization;DRM 個別化は完了しました。";
        //                        break;
        //                    case 20:
        //                        dbMsg += "MediaWaiting;メディアを待機中です。";
        //                        break;
        //                    case 21:
        //                        dbMsg += "OpeningUnknownURL;不明な種類の URL を開いています。";
        //                        break;
        //                    default:
        //                        break;
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                this.mediaPlayer = null;
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        #endregion

        //        //プレイヤーコントロール///////////////////////////////////////////////////////////////プレイヤー作成///		
        //        #region Player Contorole
        //        /// <summary>
        //        /// 再生/一時停止ボタンのクリック
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void PlayPouseButton_Click(object sender, EventArgs e)
        //        {
        //            string TAG = "[PlayPouseButton_Click]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                if (this.mediaPlayer != null)
        //                {
        //                    dbMsg += "status=" + this.mediaPlayer.status;
        //                    if (this.mediaPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
        //                    {
        //                        isPlay = false;
        //                        PlayPouseButton.BackgroundImage = AWSFileBroeser.Properties.Resources.pl_r_btn;    //pouse
        //                        if (this.mediaPlayer != null)
        //                        {
        //                            this.mediaPlayer.Ctlcontrols.pause();               //.controls.pause();      //AxWMPLib       Ctlcontrols                                           //再生する
        //                        }
        //                        CurrentPositionTimer.Stop();
        //                    }
        //                    else
        //                    {                                                                           //2
        //                        isPlay = true;
        //                        PlayPouseButton.BackgroundImage = AWSFileBroeser.Properties.Resources.pousebtn;     //play	pl_r_btn	
        //                        if (this.mediaPlayer != null)
        //                        {
        //                            this.mediaPlayer.Ctlcontrols.play();                //AxWMPLib     Ctlcontrols                                   //再生する
        //                        }
        //                        CurrentPositionTimer.Start();
        //                    }
        //                    PlayTitolLabel.Text = this.mediaPlayer.status;
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }

        //        }

        //        /// <summary>
        //        /// TrackBar操作で再生ポジションを変更
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        /// 目盛りの外観の設定   http://docs.grapecity.com/help/pluspak-winforms-8/GcTrackBar_TickStyle.html
        //        private void MediaPositionTrackBar_Scroll(object sender, EventArgs e)
        //        {
        //            string TAG = "[MediaPositionTrackBar_Scroll]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",mediaPlayer=" + mediaPlayer.Ctlcontrols.currentPosition;
        //                dbMsg += ",TrackBar=" + MediaPositionTrackBar.Value;
        //                mediaPlayer.Ctlcontrols.currentPosition = (double)MediaPositionTrackBar.Value;
        //                dbMsg += ">>" + mediaPlayer.Ctlcontrols.currentPosition;
        //                dbMsg += "/" + CurrentMediaDuration;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// タイマーで現在ポジションを取得
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        /// http://blog.code-life.net/blog/2011/09/03/how-to-use-windows-media-player-activex-controll-2/
        //        private void CurrentPositionTimer_Tick(object sender, EventArgs e)
        //        {
        //            string TAG = "[CurrentPositionTimer_Tick]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                if (CurrentMediaDuration != this.mediaPlayer.currentMedia.duration)
        //                {
        //                    CurrentMediaDuration = this.mediaPlayer.currentMedia.duration;
        //                    dbMsg += ",duration=" + CurrentMediaDuration;
        //                    dbMsg += ",/60=" + CurrentMediaDuration / 60;
        //                    dbMsg += ",%60=" + CurrentMediaDuration % 60;
        //                    TimeSpan ts = new TimeSpan(0, (int)CurrentMediaDuration / 60, (int)CurrentMediaDuration % 60);
        //                    EndTime.Text = ts.ToString();                                       //DateTime.ParseExact(mediaPlayer.currentMedia.duration.ToString(), "HH:mm:ss", null).ToString();       //"HHmmss"
        //                    MediaPositionTrackBar.Maximum = (int)CurrentMediaDuration;          //最大値と
        //                    MediaPositionTrackBar.TickFrequency = (int)CurrentMediaDuration / 10; //目盛間隔
        //                }

        //                double NowPosition = mediaPlayer.Ctlcontrols.currentPosition;
        //                dbMsg += ",Player=" + NowPosition;
        //                if (CurrentPosition < (int)NowPosition)
        //                {
        //                    CurrentPosition = NowPosition;
        //                    CarentTime.Text = this.mediaPlayer.Ctlcontrols.currentPositionString.ToString();      //AxWMPLib
        //                    MediaPositionTrackBar.Value = (int)CurrentPosition;
        //                    dbMsg += ",TrackBar=" + MediaPositionTrackBar.Value;
        //                    MediaPositionTrackBar.Value = (int)CurrentPosition;
        //                    dbMsg += ">>" + MediaPositionTrackBar.Value;
        //                    dbMsg += "/" + CurrentMediaDuration;
        //                    PlayTitolLabel.Text = this.mediaPlayer.status + "[" + this.mediaPlayer.ClientSize.Width + "×" + this.mediaPlayer.ClientSize.Height + "]";
        //                }
        //                //        MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        private void VolBar_Scroll(object sender, EventArgs e)
        //        {
        //            string TAG = "[VolBar_Scroll]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                TrackBar tb = (TrackBar)sender;
        //                //		SettingsVolum = tb.Value;
        //                dbMsg += ",Value=" + tb.Value;
        //                this.mediaPlayer.settings.volume = tb.Value;
        //                VolLabel.Text = tb.Value.ToString();
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        private void VolLabel_Click(object sender, EventArgs e)
        //        {
        //            string TAG = "[VolLabel_Click]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",SettingsVolum=" + SettingsVolum;
        //                dbMsg += ",mediaPlayer=" + this.mediaPlayer.settings.volume;
        //                if (0 < this.mediaPlayer.settings.volume)
        //                {
        //                    SettingsVolum = this.mediaPlayer.settings.volume;
        //                    this.mediaPlayer.settings.volume = 0;
        //                }
        //                else
        //                {
        //                    this.mediaPlayer.settings.volume = SettingsVolum;
        //                }
        //                VolBar.Value = this.mediaPlayer.settings.volume;
        //                dbMsg += ",Value=" + VolBar.Value;
        //                VolLabel.Text = VolBar.Value.ToString();
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        #endregion


        //        /////////////////////////////////////////////////プレイヤーコントロール/////windows media Player//
        //        ////各プレイヤーの生成/////////////////////////////////////////////////////////////////ファイル操作///
        //        #region Player Common

        //        private void InitPlayerPane(string contPlayer)
        //        {
        //            string TAG = "[InitPlayerPane]" + contPlayer;
        //            string dbMsg = TAG;
        //            try
        //            {
        //                if (this.playerWebBrowser != null && !contPlayer.Equals("web"))
        //                {
        //                    this.MediaPlayerPanel.Controls.Remove(this.playerWebBrowser);
        //                    this.playerWebBrowser = null;
        //                    dbMsg += ">webを削除";
        //                }
        //                if (this.mediaPlayer != null && !contPlayer.Equals("WMP"))
        //                {
        //                    this.MediaPlayerPanel.Controls.Remove(this.mediaPlayer);
        //                    this.mediaPlayer = null;
        //                    dbMsg += ">WMPを削除";
        //                }
        //                if (this.SFPlayer != null && !contPlayer.Equals("FLP"))
        //                {
        //                    this.MediaPlayerPanel.Controls.Remove(this.SFPlayer);
        //                    this.SFPlayer = null;
        //                    dbMsg += ">FLPを削除";
        //                }

        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                this.mediaPlayer = null;
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// 各再生動作に入る前のファイル有無チェックとプレイヤーの振り分け
        //        /// プレイリストは読み込み動作へ
        //        /// </summary>
        //        /// <param name="fileName"></param>
        //        /// 
        //        private void ToView(string fileName)
        //        {
        //            string TAG = "[ToView]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                fileName = checKLocalFile(fileName);
        //                dbMsg += ",fileName=" + fileName;
        //                System.IO.FileInfo fi = new System.IO.FileInfo(fileName);

        //                if (fi.Extension.Equals(".m3u"))      //fileName.Contains(".m3u")
        //                {
        //                    dbMsg += ">プレイリスト";
        //                    ReadPlayList(fileName);
        //                    AddPlaylistComboBox(fileName);
        //                    int selectIndex = PlaylistComboBox.Items.IndexOf(fileName); //PlaylistComboBox.Items.IndexOf(fileName);     //PlaylistComboBox.Items.Count;
        //                    dbMsg += ",selectIndex=" + selectIndex;
        //                    PlaylistComboBox.SelectedIndex = selectIndex;
        //                    appSettings.CurrentList = fileName;
        //                }
        //                else if (fileName != "")
        //                {
        //                    dbMsg += ">ファイル再生";
        //                    CheckDelPlayListItem(fileName, true);

        //                    //			this.mediaPlayer = null;
        //                    //			this.playerWebBrowser = null;
        //                    PlayTitolLabel.Text = fi.Name;
        //                    dbMsg += ",Extension=" + fi.Extension;

        //                    if (-1 < Array.IndexOf(mpFiles, fi.Extension))
        //                    {        //windows media Player
        //                        MakeWMP(fileName);
        //                        //ファイルまたはアセンブリ 'AxInterop.WMPLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'、
        //                        //またはその依存関係の 1 つが読み込めませんでした。
        //                        //厳密な名前付きのアセンブリが必要です。 (HRESULT からの例外:0x80131044)

        //                    }
        //                    else if (fi.Extension.Equals(".flv") || fi.Extension.Equals(".swf") || fi.Extension.Equals(".f4v"))
        //                    {
        //                        MakeFlash(fileName);
        //                    }
        //                    else
        //                    {
        //                        MakeWebSouce(fileName);
        //                    }
        //                    appSettings.CurrentFile = fileName;
        //                    //			WriteSetting();
        //                }
        //                /*			} else {
        //								string playListName = PlaylistComboBox.Text;
        //								DialogResult result = MessageBox.Show(playListName + "を今読み直す場合は「はい」\n" +
        //									"後で読み直す(コンボボックス切替など)場合は「いいえ」を選択して下さい。",
        //									fileName + "削除後の処理",
        //									MessageBoxButtons.OKCancel,
        //									MessageBoxIcon.Exclamation,
        //									MessageBoxDefaultButton.Button1);                   //メッセージボックスを表示する
        //								if (result == DialogResult.OK) {                   //何が選択されたか調べる
        //									dbMsg += "「はい」が選択されました";
        //									PlayListReWrite(playListName);
        //								} else if (result == DialogResult.Cancel) {
        //									dbMsg += "「キャンセル」が選択されました";

        //								}*/
        //                //		}
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception e)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + e.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }
        //        #endregion

        //        /// <summary>
        //        ///リサイズ時の再描画 ////////////////////////////////////////////////////////////////////////////////
        //        /// </summary>
        //        /// <param name="e"></param>
        //        protected override void OnPaint(PaintEventArgs e)
        //        {
        //            string TAG = "[OnPaint]";
        //            string dbMsg = TAG;
        //            base.OnPaint(e);
        //            if (typeName.Text != "")
        //            {      //rExtension.Text !="" &&
        //                if (plaingItem == "")
        //                {
        //                    if (fileNameLabel.Text.Contains(@":\"))
        //                    {
        //                        plaingItem = fileNameLabel.Text;
        //                    }
        //                    else
        //                    {
        //                        plaingItem = passNameLabel.Text + Path.DirectorySeparatorChar + fileNameLabel.Text;
        //                    }
        //                }
        //                plaingItem = checKLocalFile(plaingItem);
        //                ToView(plaingItem);
        //            }
        //            MyLog(TAG, dbMsg);
        //        }           //リサイズ時の再描画

        //        private void ReSizeViews(object sender, EventArgs e)
        //        {
        //            string TAG = "[ReSizeViews]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                //		Size size = Form1.ScrollRectangle.Size; //webBrowser1.Document.Bodyだとerror! Body is null;
        //                //	var leftPWidth = 405;
        //                dbMsg += "[" + this.Width + "×" + this.Height + "]";
        //                dbMsg += ",leftTop=" + FileBrowserSplitContainer.Height + ",Center=" + FileBrowserCenterSplitContainer.Height;
        //                //		splitContainer1.Panel1.Width = leftPWidth;
        //                //	splitContainerLeftTop.Height = 60;
        //                //	splitContainerCenter.Panel1.Height = this.Height-(60+80);            //_Panel2.
        //                //	splitContainerCenter.Panel2.Height = 80;            //_Panel2.
        //                //		splitContainerCenter.Width = leftPWidth;
        //                dbMsg += ">>2=" + FileBrowserSplitContainer.Height + ">>Center=" + FileBrowserCenterSplitContainer.Height;
        //                /*		dbMsg += ",continuousPlayCheck=" + continuousPlayCheckBox.Checked;
        //						if (continuousPlayCheckBox.Checked) {
        //							viewSplitContainer.Width = playListWidth;
        //							PlayListsplitContainer.Height = fileTree.Bottom;
        //							dbMsg += ",playLis[" + playListWidth + "×" + PlayListsplitContainer.Height;
        //						}*/
        //                if (plaingItem == "")
        //                {
        //                    if (fileNameLabel.Text.Contains(@":\"))
        //                    {
        //                        plaingItem = fileNameLabel.Text;
        //                    }
        //                    else
        //                    {
        //                        plaingItem = passNameLabel.Text + Path.DirectorySeparatorChar + fileNameLabel.Text;
        //                    }
        //                }
        //                plaingItem = checKLocalFile(plaingItem);
        //                ToView(plaingItem);
        //                //			WriteSetting();
        //                this.MediaControlPanel.Width = this.MediaPlayerSplitContainer.Width;
        //                dbMsg += ",MediaControlPanel=" + this.MediaControlPanel.Width;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }//表示サイズ変更

        //        /// <summary>
        //        /// フルパス名で順次Nodeを開き,指定されたフルパス名に該当するNodeを返す
        //        /// </summary>
        //        /// <param name="tree"></param>
        //        /// <param name="fullName"></param>
        //        /// <returns></returns>
        //        public TreeNode FindTreeNode(TreeView tree, string fullName)
        //        {
        //            string TAG = "[FindTreeNode]";
        //            string dbMsg = TAG;
        //            TreeNode retNode = new TreeNode();
        //            try
        //            {
        //                dbMsg += "fullName=" + fullName;
        //                string[] findNames = fullName.Split(Path.DirectorySeparatorChar);
        //                string findName = "";
        //                TreeNodeCollection rNodeCollection = tree.Nodes;
        //                int nCount = rNodeCollection.Count;
        //                dbMsg += "、nCount=" + nCount;

        //                for (int i = 0; i < findNames.Length; i++)
        //                {
        //                    if (i == 0)
        //                    {
        //                        findName += findNames[i] + Path.DirectorySeparatorChar;
        //                    }
        //                    else
        //                    {
        //                        findName += Path.DirectorySeparatorChar + findNames[i];
        //                    }
        //                    if (findName.Contains(@":\\\"))
        //                    {
        //                        findName = findName.Replace(@":\\\", @":\\");
        //                    }
        //                    dbMsg += "\n(find;" + i + "/" + findNames.Length + "階層)" + findName;
        //                    for (int j = 0; j < nCount; j++)
        //                    {
        //                        dbMsg += "(" + j + "/" + nCount + ")";
        //                        TreeNode cNode = rNodeCollection[j];
        //                        dbMsg += cNode.FullPath;
        //                        if (cNode.FullPath == findName)
        //                        {
        //                            retNode = cNode;
        //                            FolderItemListUp(findName, retNode);      //TreeNodeを再構築
        //                                                                      //		retNode.Expand();
        //                            rNodeCollection = retNode.Nodes;
        //                            nCount = rNodeCollection.Count;
        //                            break;
        //                        }
        //                    }
        //                    if (retNode.FullPath == fullName)
        //                    {
        //                        //			MyLog(TAG, dbMsg);
        //                        return retNode;
        //                    }
        //                }
        //                //		MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return retNode;
        //        }

        //        //ファイルTree・ファイルリスト共通//////////////////////////////////////////////////////////////////
        //        /// <summary>
        //        /// ファイルもしくはフォルダが選択された時の処理
        //        /// </summary>
        //        /// <param name="fullPathName"></param>
        //        private void FileItemSelect(string fullPathName)
        //        {
        //            string TAG = "[FileItemSelect]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                typeName.Text = "";
        //                mineType.Text = "";
        //                lsFullPathName = fullPathName;
        //                dbMsg += ",fullPathName=" + fullPathName;
        //                FileInfo fi = new FileInfo(fullPathName);
        //                String infoStr = ",Exists;";
        //                infoStr += fi.Exists;
        //                string fullName = fi.FullName;
        //                infoStr += ",絶対パス;" + fullName;
        //                infoStr += ",親ディレクトリ;" + fi.Directory;// 
        //                string passNameStr = fi.DirectoryName + "";    //親ディレクトリ名
        //                if (passNameStr == "")
        //                {
        //                    passNameStr = fullName;
        //                }
        //                infoStr += ">>" + passNameStr;
        //                passNameLabel.Text = passNameStr;    //親ディレクトリ名
        //                string fileNameStr = fi.Name + "";//ファイル名= selectItem;
        //                if (fileNameStr == "")
        //                {
        //                    fileNameStr = fullName;
        //                }
        //                fileNameLabel.Text = fileNameStr;//ファイル名= selectItem;
        //                lastWriteTime.Text = fi.LastWriteTime.ToString();//更新
        //                creationTime.Text = fi.CreationTime.ToString();//作成
        //                lastAccessTime.Text = fi.LastAccessTime.ToString();//アクセス
        //                rExtension.Text = fi.Extension.ToString();//拡張子
        //                dbMsg += ",infoStr=" + infoStr;                             //infoStr=,Exists;False,拡張子;作成;2012/11/04 3:56:33,アクセス;2012/11/04 3:56:33,絶対パス;I:\Dtop,親ディレクトリ;I:\

        //                string fileAttributes = fi.Attributes.ToString();
        //                dbMsg += ",Attributes=" + fileAttributes;
        //                //	dbMsg += ",Directory.Exists=" + Directory.Exists( fullName );                             //infoStr=,Exists;False,拡張子;作成;2012/11/04 3:56:33,アクセス;2012/11/04 3:56:33,絶対パス;I:\Dtop,親ディレクトリ;I:\
        //                名称変更ToolStripMenuItem.Visible = true;
        //                if (copySouce != "" || cutSouce != "")
        //                {
        //                    ペーストToolStripMenuItem.Visible = true;
        //                    コピーToolStripMenuItem.Visible = false;
        //                    if (cutSouce != "")
        //                    {
        //                        カットToolStripMenuItem.Visible = false;
        //                    }
        //                }
        //                else
        //                {
        //                    ペーストToolStripMenuItem.Visible = false;
        //                    コピーToolStripMenuItem.Visible = true;
        //                    カットToolStripMenuItem.Visible = true;
        //                }
        //                削除ToolStripMenuItem.Visible = true;
        //                mineType.Text = "";
        //                if (fileAttributes.Contains("Directory"))
        //                {
        //                    dbMsg += ",Directoryを選択";
        //                    フォルダ作成ToolStripMenuItem.Visible = true;
        //                    他のアプリケーションで開くToolStripMenuItem.Visible = false;
        //                    if (fileNameStr == passNameStr)
        //                    {         //if (fileNameLabel.Text == passNameLabel.Text) {
        //                        dbMsg += ",ドライブを選択";
        //                        名称変更ToolStripMenuItem.Visible = false;
        //                        コピーToolStripMenuItem.Visible = false;
        //                        カットToolStripMenuItem.Visible = false;
        //                        削除ToolStripMenuItem.Visible = false;
        //                        元に戻す.Visible = false;
        //                    }
        //                    passNameStr = fullPathName;
        //                    typeName.Text = "フォルダ";
        //                    mineType.Text = fileAttributes.Replace("Directory", "");        //systemなどの他属性が有れば記載
        //                    if (mineType.Text == "")
        //                    {                                   //何の記載も無いままなら
        //                        dbMsg += "；内容確認";
        //                    }
        //                    if (passNameStr != @"C:\" && 0 < PlaylistComboBox.Items.Count)
        //                    {            //実際のリストが書き込まれて2行以上ないと書き換える度に読み込みが発生する
        //                        PlaylistComboBox.Items[0] = passNameStr;
        //                        FilelistView.Focus();
        //                    }
        //                    DirectoryInfo di = new DirectoryInfo(fullPathName);
        //                    int itemCount = 0;
        //                    try
        //                    {
        //                        dbMsg += ",GetDirectories=" + di.GetDirectories().Count();
        //                        itemCount = di.GetDirectories().Count();
        //                        dbMsg += ",GetFiles=" + di.GetFiles().Length;
        //                        itemCount += di.GetFiles().Length;
        //                    }
        //                    catch (System.UnauthorizedAccessException se)
        //                    {
        //                        dbMsg += "<<GetDirectoriesでエラー発生>>" + se.Message;
        //                        MyLog(TAG, dbMsg);
        //                    }
        //                    if (0 < itemCount)
        //                    {
        //                        fileLength.Text = itemCount + "アイテム";
        //                    }
        //                    else
        //                    {
        //                        fileLength.Text = "取得不能";
        //                    }
        //                }
        //                else
        //                {        //ファイルの時はArchive
        //                    dbMsg += ",ファイルを選択";
        //                    fileLength.Text = fi.Length.ToString();//ファイルサイズ
        //                    if (rExtension.Text != "")
        //                    {
        //                        //					fileLength.Text = fi.Length.ToString();//ファイルサイズ
        //                        typeName.Text = GetFileTypeStr(passNameStr);
        //                        他のアプリケーションで開くToolStripMenuItem.Visible = true;
        //                        dbMsg += ",Checked=" + continuousPlayCheckBox.Checked;
        //                        //	if (continuousPlayCheckBox.Checked) {                   //連続再生中
        //                        dbMsg += ",nowLPlayList=" + nowLPlayList;
        //                        if (!nowLPlayList.Contains(".m3u"))
        //                        {               //プレイリスト再生中でなければ
        //                            dbMsg += ",再生動作";
        //                            ToView(fullPathName);
        //                        }
        //                        else
        //                        {
        //                            dbMsg += ",書換え";
        //                            AddPlaylistComboBox(fi.Directory.FullName);
        //                        }
        //                        このファイルを再生ToolStripMenuItem.Visible = true;                     //プレイリストへボタン表示
        //                        PlaylistComboBox.Items[0] = fi.DirectoryName;
        //                    }
        //                    //		appSettings.CurrentFile = passNameStr;               //ファイルが選択される度に書換
        //                    //		WriteSetting();
        //                }
        //                //		ReExpandNode(passNameStr);
        //                FileListVewDrow(passNameStr);
        //                if (typeName.Text == "video" || typeName.Text == "audio")
        //                {
        //                    continuousPlayCheckBox.Visible = true;                 //連続再生中チェックボックス表示
        //                                                                           //splitContainer2.Panel1Collapsed = false;                 //playlistPanelを開く
        //                }
        //                else
        //                {
        //                    continuousPlayCheckBox.Visible = false;                 //連続再生中チェックボックス非表示
        //                    continuousPlayCheckBox.Checked = false;
        //                    //	splitContainer2.Panel1Collapsed = true;             //playlistPanelを閉じる
        //                }
        //                //	appSettings.CurrentFile = passNameStr;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        //ファイルTree操作//////////////////////////////////////////////////////////////ファイルTree・ファイルリスト共通//
        //        //		System.IO.FileInfo fCpoy = null;
        //        //		System.IO.FileInfo fMove = null;

        //        /// <summary>
        //        ///Nodeを書き直して再び開く ///////////////////////////////////
        //        /// </summary>
        //        /// MakeNewFolder,DelFiles.TargetReName,PeastSelecter,
        //        public void ReExpandNode(string targetFile)
        //        {
        //            string TAG = "[ReExpandNode]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",targetFile=" + targetFile;
        //                System.IO.FileInfo fi = new System.IO.FileInfo(targetFile);
        //                string openFolder = fi.DirectoryName;
        //                dbMsg += ",openFolder(Directory)=" + openFolder;
        //                string Attributes = fi.Attributes.ToString();
        //                dbMsg += ",Attributes=" + Attributes;
        //                if (openFolder == null || Attributes.Contains("Directory"))
        //                {                         //ドライブルートの場合
        //                    openFolder = targetFile;
        //                }
        //                dbMsg += ">>" + openFolder;
        //                TreeNode SelectedNode = FindTreeNode(fileTree, openFolder); // fileTree.SelectedNode;
        //                dbMsg += ",SelectedNode=" + SelectedNode.FullPath;
        //                TreeNode openNode = SelectedNode.Parent;
        //                dbMsg += ",openNode=" + openNode;
        //                if (openNode == null)
        //                {                             //ドライブルート
        //                    openNode = SelectedNode;
        //                }
        //                else if (openNode.FullPath != openFolder)
        //                {
        //                    openNode = SelectedNode;
        //                }
        //                dbMsg += ">openNode>" + openNode.FullPath;
        //                //		openNode.Collapse();
        //                FolderItemListUp(openFolder, openNode);      //TreeNodeを再構築
        //                openNode.Expand();
        //                fileTree.SelectedNode = openNode;
        //                fileTree.Focus();
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// 新規フォルダの作成
        //        /// </summary>
        //        /// <param name="fullPath"></param>
        //        public void MakeNewFolder(string fullPath)
        //        {
        //            string TAG = "[MakeNewFolder]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += fullPath;
        //                System.IO.FileInfo fi = new System.IO.FileInfo(fullPath);
        //                string Attributes = fi.Attributes.ToString();
        //                if (Attributes.Contains("Directory"))
        //                {            //フォルダ
        //                }
        //                else
        //                {                                            //以外が指定されたら
        //                    fullPath = fi.DirectoryName;                    //そのファイルのデレクトリ
        //                    dbMsg += ">>" + fullPath;
        //                }
        //                fullPath = fullPath + Path.DirectorySeparatorChar + "新しいフォルダ";
        //                System.IO.Directory.CreateDirectory(fullPath);
        //                ReExpandNode(fullPath);
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "でエラー発生" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// 指定されたものを削除
        //        /// </summary>
        //        /// <param name="sourceName">ファイルもしくはフォルダ名</param>
        //        /// <param name="isTrash">trueでゴミ箱　/　falseで完全削除</param>
        //        public void DelFiles(List<string> DragURLs, bool isTrash)
        //        {
        //            string TAG = "[DelFiles]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",元=" + DragURLs.Count + "件を削除;isTrash=" + isTrash;
        //                string farstName = DragURLs[0];
        //                System.IO.FileInfo fi = new System.IO.FileInfo(farstName);
        //                string rewriteFolder = fi.Directory.FullName;
        //                TreeNode rewriteNode = FindTreeNode(fileTree, rewriteFolder);// SelectedNode;
        //                string MessageStr = "";//	fileName + "を" + playListName + ""
        //                foreach (string sourceName in DragURLs)
        //                {
        //                    MessageStr += sourceName + "\n";
        //                }
        //                MessageStr += "を削除します。";

        //                DialogResult result = MessageBox.Show(MessageStr,
        //                    fi.DirectoryName + "から",
        //                    MessageBoxButtons.OKCancel,
        //                    MessageBoxIcon.Exclamation,
        //                    MessageBoxDefaultButton.Button1);                   //メッセージボックスを表示する
        //                if (result == DialogResult.OK)
        //                {                   //何が選択されたか調べる
        //                    dbMsg += "「はい」が選択されました";
        //                    Microsoft.VisualBasic.FileIO.RecycleOption recycleOption = RecycleOption.DeletePermanently;         //ファイルまたはディレクトリを完全に削除します。 既定モード。
        //                    元に戻す.Visible = false;
        //                    if (isTrash)
        //                    {
        //                        recycleOption = RecycleOption.SendToRecycleBin;                                                //ファイルまたはディレクトリの送信、 ごみ箱します。																												   //			元に戻す.Visible = true;
        //                    }

        //                    foreach (string sourceName in DragURLs)
        //                    {
        //                        dbMsg += ",元=" + sourceName;
        //                        if (File.Exists(sourceName))
        //                        {
        //                            dbMsg += ",ファイル";
        //                            FileSystem.DeleteFile(sourceName, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.DoNothing);        //UIOption.AllDialogs 削除前のダイアログ表示
        //                                                                                                                                          //もしくは	System.IO.File.Delete( sourceName );             //フォルダ"C:\TEST"を削除する
        //                        }
        //                        else if (Directory.Exists(sourceName))
        //                        {
        //                            dbMsg += ",フォルダ";
        //                            FileSystem.DeleteDirectory(sourceName, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.DoNothing);
        //                            //もしくは	System.IO.Directory.Delete( sourceName, true );   //true;エラーを無視して削除？
        //                        }
        //                    }

        //                    dbMsg += ",再表示=" + rewriteFolder;
        //                    if (rewriteNode != null)
        //                    {
        //                        dbMsg += ",rewriteNode=" + rewriteNode.FullPath;
        //                        ReExpandNode(rewriteFolder);
        //                        FolderItemListUp(rewriteFolder, rewriteNode);      //TreeNodeを再構築
        //                    }
        //                    FileListVewDrow(rewriteFolder);
        //                }
        //                else
        //                {
        //                    dbMsg += "「キャンセル」が選択されました";
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "でエラー発生" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            //https://dobon.net/vb/dotnet/file/directorycreate.html
        //        }

        //        /// <summary>
        //        /// FileInfoのMoveToで移動/名称変更
        //        /// </summary>
        //        /// <param name="sourceName"></param>
        //        /// <param name="destName"></param>
        //        public void MoveMyFile(string sourceName, string destName)
        //        {
        //            string TAG = "[MoveMyFile]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",元=" + sourceName + ",先=" + destName;
        //                System.IO.FileInfo fi = new System.IO.FileInfo(sourceName);   //変更元のFileInfoのオブジェクトを作成します。 @"C:\files1\sample1.txt" 
        //                fi.MoveTo(destName);                                           //MoveToメソッドで移動先を指定してファイルを移動します。@"D:\files2\sample2.txt"
        //                                                                               // http://www.openreference.org/articles/view/329
        //                                                                               //	fi = null;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //                throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
        //            }
        //        }

        //        /// <summary>
        //        ///  System.IO.Directoryでフォルダを作成
        //        /// </summary>
        //        /// <param name="sourceName"></param>
        //        /// <param name="destName"></param>
        //        public void MoveFolder(string sourceName, string destName)
        //        {
        //            string TAG = "[MoveFolder]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",元=" + sourceName + ",先=" + destName;
        //                //https://dobon.net/vb/dotnet/file/directorycreate.html
        //                /*			string[] dirs = System.IO.Directory.GetFiles( sourceName, "*", System.IO.SearchOption.AllDirectories );
        //								dbMsg += ",中身は" + dirs.Length + "フォルダ" ;
        //								foreach (string dir in dirs) {
        //									string newSouce = dir;
        //									dbMsg += "," + newSouce;
        //								}*/

        //                //Directoryクラスを使用する方法;中身移動せず
        //                /*			System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory( destName );   //フォルダ"C:\TEST\SUB"を作成する
        //							System.IO.Directory.Move( sourceName, destName );               //フォルダ"C:\1"を"C:\2\SUB"に移動（名前を変更）する
        //							string[] files = System.IO.Directory.GetFiles( di.FullName, "*", System.IO.SearchOption.AllDirectories );
        //							dbMsg += ",di（" + di.FullName + "）に" + files.Length + "件";//
        //							System.IO.Directory.Delete( sourceName, true );             //フォルダ"C:\TEST"を削除する
        //							*/
        //                //DirectoryInfoクラスを使用する方法;中身移動せず
        //                /*		System.IO.DirectoryInfo di = new System.IO.DirectoryInfo( sourceName ); //@"C:\TEST\SUB"；DirectoryInfoオブジェクトを作成する
        //						string[] files = System.IO.Directory.GetFiles( di.FullName, "*", System.IO.SearchOption.AllDirectories );
        //						dbMsg += ",di（" + di.FullName + "）に" + files.Length + "件";//
        //						di.Create();                                                           //フォルダ"C:\TEST\SUB"を作成する
        //						System.IO.DirectoryInfo subDir = di.CreateSubdirectory( "1" );     //サブフォルダを作成する☆subDirには、フォルダ"C:\TEST\SUB\1"のDirectoryInfoオブジェクトが入る
        //						files = System.IO.Directory.GetFiles( subDir.FullName, "*", System.IO.SearchOption.AllDirectories );
        //						dbMsg += ",di（" + subDir.FullName + "）に" + files.Length + "件";
        //						subDir.MoveTo( destName );                                           //フォルダ"C:\TEST\SUB\1"を"C:\TEST\SUB\2"に移動する☆subDirの内容は、"C:\TEST\SUB\2"のものに変わる
        //						di.Delete( true );                                                  //フォルダ"C:\TEST\SUB"を根こそぎ削除する☆trueにしないと中身が有った場合にエラー発生
        //						*/

        //                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(sourceName); //@"C:\TEST\SUB"；DirectoryInfoオブジェクトを作成する	
        //                                                                                      //FileSystemを使用:参照設定に"Microsoft.VisualBasic.dll"が追加されている必要がある
        //                FileSystem.CreateDirectory(destName);                     //フォルダdestを作成する
        //                string[] dirs = System.IO.Directory.GetFiles(destName, "*", System.IO.SearchOption.AllDirectories);
        //                dbMsg += ",中身は" + dirs.Length + "フォルダ";
        //                FileSystem.MoveDirectory(sourceName, destName, true);    //sourceをdestに移動する☆第3項にTrueを指定すると、destが存在する時、上書きする
        //                                                                         //		FileSystem.MoveDirectory( sourceName, destName, UIOption.AllDialogs, UICancelOption.DoNothing );//sourceをdestに移動する
        //                                                                         //進行状況ダイアログとエラーダイアログを表示する☆ユーザーがキャンセルしても例外OperationCanceledExceptionをスローしない
        //                dirs = System.IO.Directory.GetFiles(destName, "*", System.IO.SearchOption.AllDirectories);
        //                dbMsg += ">>" + dirs.Length + "件";
        //                di.Delete(true);                                                  //フォルダ"C:\TEST\SUB"を根こそぎ削除する☆trueにしないと中身が有った場合にエラー発生
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "でエラー発生" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            //https://dobon.net/vb/dotnet/file/directorycreate.html
        //        }

        //        /// <summary>
        //        /// ファイル名/フォルダ名変更(ListViewのラベル変更から)
        //        /// ☆FileSystem.RenameFileを使用
        //        /// ☆入力ダイアログのtextinputで拡張子の書き換え回避
        //        /// </summary>
        //        /// <param name="destName"></param>
        //        public void TargetReName(string destName)
        //        {
        //            string TAG = "[TargetReName]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += " , destName=" + destName;
        //                ListViewItem selectItem = FilelistView.FocusedItem;

        //                string selectItemStr = selectItem.Text;
        //                dbMsg += " , selectItem=" + selectItemStr;
        //                if (!destName.Contains(@":\"))
        //                {                        //ドライブ選択でなければ		passNameStr != selectItem
        //                    if (selectItemStr != passNameLabel.Text)
        //                    {
        //                        selectItemStr = passNameLabel.Text + Path.DirectorySeparatorChar + selectItemStr;
        //                        selectItemStr = checKLocalFile(selectItemStr);

        //                        dbMsg += ">>" + selectItemStr;  // selectItem=media2.flv>>M:\sample/media2.flv,選択；ペースト,
        //                    }
        //                    System.IO.FileInfo fi = new System.IO.FileInfo(selectItemStr);   //変更元のFileInfoのオブジェクトを作成します。 
        //                    string passNameStr = fi.DirectoryName;
        //                    dbMsg += " , passNameStr=" + passNameStr;
        //                    string titolStr = selectItemStr + "の名称変更";
        //                    string msgStr = "元の名称\n" + selectItemStr;
        //                    dbMsg += ",titolStr=" + titolStr + ",msgStr=" + msgStr;

        //                    InputDialog f = new InputDialog(msgStr, titolStr, destName);
        //                    if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //                    {
        //                        destName = f.ResultText;
        //                        FilelistView.FocusedItem.Text = destName;
        //                        dbMsg += ",元=" + selectItemStr + ",先=" + destName;
        //                        string renewName = passNameStr + Path.DirectorySeparatorChar + destName;
        //                        renewName = checKLocalFile(renewName);

        //                        if (File.Exists(selectItemStr))
        //                        {
        //                            dbMsg += ">>ファイル名変更>" + renewName;
        //                            FileSystem.RenameFile(selectItemStr, destName);     //ファイル名の変更（同じフォルダへの移動）のみ可
        //                                                                                //	MoveMyFile(selectItem, renewName);
        //                        }
        //                        else if (Directory.Exists(selectItemStr))
        //                        {
        //                            dbMsg += ">>フォルダ名変更>" + renewName;
        //                            FileSystem.RenameDirectory(selectItemStr, destName);
        //                            //	MoveFolder(selectItem, renewName);
        //                        }
        //                        ReExpandNode(passNameStr);
        //                        FileListVewDrow(passNameStr);
        //                        //TreeViewのノードのテキストをユーザーが編集  https://dobon.net/vb/dotnet/control/tvlabeledit.html
        //                    }
        //                    else
        //                    {
        //                        dbMsg += ">>Cancel";
        //                    }
        //                }
        //                else
        //                {
        //                    string titolStr = selectItem + "の名称は変更できません";
        //                    string msgStr = "ドライブ名称は変更できません";
        //                    DialogResult result = MessageBox.Show(msgStr, titolStr,
        //                        MessageBoxButtons.OK,
        //                        MessageBoxIcon.Exclamation,
        //                        MessageBoxDefaultButton.Button1);                  //メッセージボックスを表示する
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// ファイルもしくはフォルダをコピーする
        //        /// </summary>
        //        /// <param name="sourceName">コピー元</param>
        //        /// <param name="destName">コピー先</param>
        //        public void FilesCopy(string sourceName, string destName)
        //        {
        //            string TAG = "[FilesCopy]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",元=" + sourceName;
        //                FileInfo sourceInfo = new FileInfo(sourceName);
        //                dbMsg += ",sourceInfo=" + sourceInfo.FullName;              //ドライブ名～拡張子
        //                string sourceExtension = sourceInfo.Extension;
        //                dbMsg += ",sourceExtension=" + sourceExtension;
        //                dbMsg += ",先=" + destName;
        //                FileInfo destInfo = new FileInfo(destName);
        //                dbMsg += ",destInfo:FullName=" + destInfo.FullName;
        //                if (Directory.Exists(destName))
        //                {
        //                    dbMsg += ";フォルダ";
        //                    destName = destName + Path.DirectorySeparatorChar + sourceInfo.Name;// + sourceInfo.Extension;
        //                    destName = checKLocalFile(destName);
        //                }
        //                else
        //                {
        //                    dbMsg += ";ファイル";
        //                    destName = destInfo.DirectoryName + Path.DirectorySeparatorChar + sourceInfo.Name;// + sourceInfo.Extension;
        //                    destName = checKLocalFile(destName);
        //                }
        //                dbMsg += ">destName>" + destName;
        //                destInfo = new FileInfo(destName);

        //                /*	string[] souceNames = sourceName.Split(Path.DirectorySeparatorChar);
        //					string souceEnd = souceNames[souceNames.Length - 1];
        //					destName += Path.DirectorySeparatorChar + souceEnd;
        //					dbMsg += ">>" + destName;*/
        //                if (File.Exists(sourceName))
        //                {      //File.Exists(sourceName)
        //                    dbMsg += ">>ファイルコピー";
        //                    if (destInfo.Exists || sourceName == destName)
        //                    {
        //                        //	string[] extStrs = destName.Split('.');
        //                        //	string souceEnd = extStrs[];            //extStrs.Length - 2
        //                        if (destName.Contains(sourceExtension))
        //                        {
        //                            destName = destName.Replace(sourceExtension, "のコピー") + sourceExtension;// extStrs[extStrs.Length - 1];
        //                        }
        //                        else
        //                        {
        //                            destName = destName + "のコピー";
        //                        }
        //                        dbMsg += ">>" + destName;
        //                    }
        //                    //		FileSystem.CopyFile( sourceName, destName );                  //"C:\test\1.txt"を"C:\test\2.txt"にコピーする
        //                    //		FileSystem.CopyFile( sourceName, destName, true );	//"C:\test\2.txt"がすでに存在している場合は、これを上書きする
        //                    //		FileSystem.CopyFile( sourceName, destName, UIOption.OnlyErrorDialogs );                    //エラーの時、ダイアログを表示する
        //                    //		FileSystem.CopyFile( sourceName, destName,UIOption.AllDialogs );                    //進行状況ダイアログと、エラーダイアログを表示する
        //                    FileSystem.CopyFile(sourceName, destName, UIOption.AllDialogs, UICancelOption.DoNothing);
        //                    //進行状況ダイアログやエラーダイアログでキャンセルされても例外をスローしない
        //                    //UICancelOption.DoNothingを指定しないと、例外OperationCanceledExceptionが発生
        //                }
        //                else if (Directory.Exists(sourceName))
        //                {
        //                    dbMsg += ">>フォルダコピー";
        //                    if (Directory.Exists(destName))
        //                    {
        //                        destName = destName + "のコピー";
        //                        dbMsg += ">>" + destName;
        //                    }
        //                    FileSystem.CopyDirectory(sourceName, destName, UIOption.AllDialogs, UICancelOption.DoNothing);
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "でエラー発生" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// ファイルもしくはフォルダをコピーする
        //        /// </summary>
        //        /// <param name="sourceName">コピー元</param>
        //        /// <param name="destName">コピー先</param>
        //        public void FilesMove(string sourceName, string destName)
        //        {
        //            string TAG = "[FilesMove]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",元=" + sourceName + ",先=" + destName;
        //                string[] souceNames = sourceName.Split(Path.DirectorySeparatorChar);
        //                string souceEnd = souceNames[souceNames.Length - 1];
        //                destName += Path.DirectorySeparatorChar + souceEnd;
        //                dbMsg += ">>" + destName;
        //                if (File.Exists(sourceName))
        //                {
        //                    dbMsg += ">>ファイル";
        //                    MoveMyFile(sourceName, destName);
        //                }
        //                else if (Directory.Exists(sourceName))
        //                {
        //                    dbMsg += ">>フォルダ";
        //                    MoveFolder(sourceName, destName);
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        public static void CopyDirectory(string sourceDirName, string destDirName)
        //        {
        //            string TAG = "[CopyDirectory]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",元=" + sourceDirName + ",先=" + destDirName;
        //                if (!System.IO.Directory.Exists(destDirName))
        //                {                                                           //コピー先のディレクトリがないときは
        //                    System.IO.Directory.CreateDirectory(destDirName);                                                      //作る
        //                    System.IO.File.SetAttributes(destDirName, System.IO.File.GetAttributes(sourceDirName));              //属性もコピー
        //                }

        //                if (destDirName[destDirName.Length - 1] != System.IO.Path.DirectorySeparatorChar)
        //                {
        //                    destDirName = destDirName + System.IO.Path.DirectorySeparatorChar;                                      //コピー先のディレクトリ名の末尾に"\"をつける
        //                }

        //                string[] files = System.IO.Directory.GetFiles(sourceDirName);
        //                foreach (string file in files)
        //                {
        //                    System.IO.File.Copy(file, destDirName + System.IO.Path.GetFileName(file), true);                     //コピー元のディレクトリにあるファイルをコピー
        //                }

        //                string[] dirs = System.IO.Directory.GetDirectories(sourceDirName);
        //                foreach (string dir in dirs)
        //                {
        //                    CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir));          //コピー元のディレクトリにあるディレクトリについて、再帰的に呼び出す
        //                }

        //                //		MyLog( dbMsg );
        //            }
        //            catch (Exception er)
        //            {
        //                Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// コピーかカットかを判定してペースト動作へ
        //        /// </summary>
        //        /// <param name="copySouce"></param>
        //        /// <param name="cutSouce"></param>
        //        /// <param name="peastFor"></param>
        //        public void PeastSelecter(string copySouce, string cutSouce, string peastFor)
        //        {
        //            string TAG = "[PeastSelecter]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",copy=" + copySouce + ",cut=" + cutSouce + ",先=" + peastFor;
        //                string fullPath = null;
        //                foreach (string tItem in DragURLs)
        //                {
        //                    if (copySouce != "")
        //                    {
        //                        FilesCopy(tItem, peastFor);
        //                    }
        //                    else if (cutSouce != "")
        //                    {
        //                        FilesMove(tItem, peastFor);
        //                    }
        //                    fullPath = tItem;
        //                }
        //                ReExpandNode(peastFor);
        //                FileListVewDrow(peastFor);
        //                if (cutSouce != "")
        //                {
        //                    cutSouce = "";
        //                }
        //                コピーToolStripMenuItem.Visible = true;
        //                カットToolStripMenuItem.Visible = true;
        //                ペーストToolStripMenuItem.Visible = false;
        //                dbMsg += ">copy=" + copySouce + ",cut=" + cutSouce + ",先=" + peastFor + ">";
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        public void DropPeast(string copySouce, string cutSouce, string dragSouceUrl, string dropSouceUrl)
        //        {
        //            string TAG = "[DropPeast]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",copy=" + copySouce + ",cut=" + cutSouce + ",元=" + dragSouceUrl + ",先=" + dropSouceUrl;
        //                System.IO.FileInfo dragFi = new System.IO.FileInfo(dragSouceUrl);
        //                System.IO.FileInfo dropFi = new System.IO.FileInfo(dropSouceUrl);
        //                if (cutSouce != "")
        //                {
        //                    string dragRoot = "";
        //                    if (dragFi.Directory != null)
        //                    {                 //ドライブルートはDirectoryが無い
        //                        dragRoot = dragFi.Directory.Root.ToString();
        //                    }
        //                    else
        //                    {
        //                        dragRoot = dragFi.FullName.ToString();
        //                    }
        //                    dbMsg += ",root=" + dragRoot;
        //                    string dropRoot = "";
        //                    if (dropFi.Directory != null)
        //                    {
        //                        dropRoot = dropFi.Directory.Root.ToString();
        //                    }
        //                    else
        //                    {
        //                        dropRoot = dropFi.FullName.ToString();
        //                    }
        //                    dbMsg += ">>" + dropRoot;
        //                    if (dragRoot != dropRoot)
        //                    {
        //                        dbMsg += ";copyに変更;";
        //                        copySouce = cutSouce;
        //                        cutSouce = "";
        //                    }
        //                }
        //                if (copySouce != "")
        //                {
        //                    dbMsg += ",copy=" + copySouce;
        //                }
        //                if (cutSouce != "")
        //                {
        //                    dbMsg += ",cut=" + cutSouce;
        //                }
        //                dbMsg += ",peast先=" + dropSouceUrl;
        //                PeastSelecter(copySouce, cutSouce, dropSouceUrl);

        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// 割り付けられたアプリケーションを起動する
        //        /// </summary>
        //        /// <param name="sourceName"></param>
        //        public void SartApication(string sourceName)
        //        {
        //            string TAG = "[SartApication]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",元=" + sourceName;
        //                System.Diagnostics.Process p = System.Diagnostics.Process.Start(sourceName);
        //                dbMsg += ",MainWindowTitle=" + p.MainWindowTitle;
        //                dbMsg += ",ModuleName=" + p.MainModule.ModuleName;
        //                dbMsg += ",ProcessName=" + p.ProcessName;
        //                MyLog(TAG, dbMsg);                                             //何故かここのLogが出ない
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////ファイル操作//
        //        /// <summary>
        //        /// fileTreeのクリック結果
        //        /// 右クリックされたアイテムからフルパスをグローバル変数に設定
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        public void FilelistBoxMouseUp(object sender, MouseEventArgs e)
        //        {
        //            string TAG = "[FilelistBoxMouseUp]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                int selsctLevel = -1;
        //                string seieNodeName = "";
        //                string fiAttributes = "";
        //                Point pos = fileTree.PointToScreen(e.Location);
        //                dbMsg += ",pos=" + pos;
        //                TreeNode flRightItem = fileTree.GetNodeAt(e.Location);            //e.X, e.Y)
        //                if (flRightItem != null)
        //                {
        //                    //			fileTree.SelectedNode = flRightItem;                 // アイテムを選択
        //                    int SelectedID = flRightItem.Index;
        //                    dbMsg += ",SelectedID=" + SelectedID;
        //                    flRightClickItemUrl = flRightItem.FullPath;
        //                    dbMsg += ",flRightClickItemUrl=" + flRightClickItemUrl;
        //                    seieNodeName = flRightClickItemUrl.Replace(@":\\", @":\");
        //                    selsctLevel = flRightItem.Level;
        //                    dbMsg += " , selsctLevel=" + selsctLevel;
        //                    FileInfo fi = new FileInfo(seieNodeName);
        //                    fiAttributes = fi.Attributes.ToString();
        //                }
        //                else
        //                {
        //                    //			FilelistView.GetChildAtPoint
        //                }

        //                if (this.FormBorderStyle == FormBorderStyle.None && this.WindowState == FormWindowState.Maximized)
        //                {                //フルスクリーン
        //                    通常サイズに戻すToolStripMenuItem.Visible = true;
        //                }
        //                else
        //                {            //	this.FormBorderStyle = FormBorderStyle.Sizable; //this.WindowState = FormWindowState.Normal;              //通常サイズに戻す
        //                    通常サイズに戻すToolStripMenuItem.Visible = false;
        //                }

        //                dbMsg += " , seieNodeName=" + seieNodeName;
        //                if (e.Button == System.Windows.Forms.MouseButtons.Right && seieNodeName != "")
        //                {          // 右クリックされた？
        //                    titolToolStripMenuItem.Text = seieNodeName;
        //                    フォルダ作成ToolStripMenuItem.Visible = false;
        //                    名称変更ToolStripMenuItem.Visible = false;
        //                    カットToolStripMenuItem.Visible = false;
        //                    コピーToolStripMenuItem.Visible = false;
        //                    ペーストToolStripMenuItem.Visible = false;
        //                    削除ToolStripMenuItem.Visible = false;
        //                    プレイリストに追加ToolStripMenuItem.Visible = false;
        //                    プレイリストを作成ToolStripMenuItem.Visible = false;
        //                    元に戻す.Visible = false;
        //                    他のアプリケーションで開くToolStripMenuItem.Visible = false;
        //                    再生ToolStripMenuItem.Visible = false;
        //                    if (selsctLevel == 0)
        //                    {
        //                        dbMsg += ">>ドライブルート";
        //                        フォルダ作成ToolStripMenuItem.Visible = true;
        //                    }
        //                    else if (fiAttributes.Contains("Directory"))
        //                    {
        //                        dbMsg += ">>フォルダ";
        //                        フォルダ作成ToolStripMenuItem.Visible = true;
        //                        カットToolStripMenuItem.Visible = true;
        //                        コピーToolStripMenuItem.Visible = true;
        //                        削除ToolStripMenuItem.Visible = true;
        //                        プレイリストに追加ToolStripMenuItem.Visible = true;
        //                        プレイリストを作成ToolStripMenuItem.Visible = true;
        //                    }
        //                    else
        //                    {
        //                        dbMsg += ">>単体ファイル";
        //                        名称変更ToolStripMenuItem.Visible = true;
        //                        カットToolStripMenuItem.Visible = true;
        //                        コピーToolStripMenuItem.Visible = true;
        //                        削除ToolStripMenuItem.Visible = true;
        //                        プレイリストに追加ToolStripMenuItem.Visible = true;
        //                        プレイリストを作成ToolStripMenuItem.Visible = true;
        //                        このファイルを再生ToolStripMenuItem.Visible = true;                     //プレイリストへボタン表示
        //                    }
        //                    if (copySouce != "" || cutSouce != "")
        //                    {
        //                        ペーストToolStripMenuItem.Visible = true;
        //                    }
        //                    if (-1 < selsctLevel && (-1 < pos.X || -1 < pos.Y))
        //                    {                               //エリア内にマウスポイントが拾えていたら
        //                        fileTreeContextMenuStrip.Show(pos);                     // コンテキストメニューを表示
        //                    }
        //                    else
        //                    {
        //                        dbMsg += ">>範囲外";
        //                    }
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// コピーもしくはカット元配列を作成する
        //        /// </summary>
        //        /// <param name="senderName"></param>
        //        private void DragListMake(string senderName)
        //        {
        //            string TAG = "[DragListMake]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += senderName + "から";
        //                DragURLs = new List<string>();
        //                if (senderName == FilelistView.Name)
        //                {
        //                    for (int i = 0; i < FilelistView.SelectedItems.Count; ++i)
        //                    {
        //                        dbMsg += "(" + i + ")";
        //                        ListViewItem itemxs = FilelistView.SelectedItems[i];
        //                        string SelectedItems = FilelistView.SelectedItems[i].Name;     //(dragSouc;0)Url;M:\\sample\123.flv
        //                        dbMsg += SelectedItems;
        //                        DragURLs.Add(SelectedItems);
        //                    }
        //                    dbMsg += ">>" + DragURLs.Count + "件";
        //                }
        //                else if (senderName == fileTree.Name)
        //                {
        //                    TreeNode selectNode = fileTree.SelectedNode;
        //                    dbMsg += ".selectNode=" + selectNode.FullPath;
        //                    DragURLs.Add(selectNode.FullPath);
        //                }
        //                else
        //                {
        //                    dbMsg += ".flRightClickItemUrl=" + flRightClickItemUrl;
        //                    DragURLs.Add(flRightClickItemUrl);
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //                throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
        //            }
        //        }

        //        /// <summary>
        //        /// FileTreeとFileViewの共通ショートカット
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FileBrowser_KeyUp(string senderName, string fullPath, KeyEventArgs e)
        //        {
        //            string TAG = "[FileBrowser_KeyUp]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",senderName=" + senderName;
        //                DragListMake(senderName);
        //                dbMsg += ",fullPath=" + fullPath;
        //                dbMsg += ", e.KeyCode=" + e.KeyCode;
        //                if (fullPath != null)
        //                {
        //                    if (e.KeyCode == Keys.C && e.Control)
        //                    {
        //                        dbMsg += ";コピー;";
        //                        copySouce = fullPath;
        //                    }
        //                    else if (e.KeyCode == Keys.X && e.Control)
        //                    {
        //                        dbMsg += "；カット";
        //                        cutSouce = fullPath;
        //                    }
        //                    else if (e.KeyCode == Keys.V && e.Control)
        //                    {
        //                        dbMsg += "；ペースト";
        //                        PeastSelecter(copySouce, cutSouce, fullPath);
        //                    }
        //                    else if (e.KeyCode == Keys.Delete)
        //                    {
        //                        dbMsg += ";Delete;";
        //                        DelFiles(DragURLs, true);
        //                    }
        //                    /*	} else {
        //							string mgsbTitol = "";
        //							if (e.KeyCode == Keys.C) {     //e.KeyCode == Keys.Control &&
        //								mgsbTitol += "コピーが指定されました";
        //							} else if (e.KeyCode == Keys.X) {      //e.KeyCode == Keys.Control &&
        //								mgsbTitol += "カットが指定されました";
        //							} else if (e.KeyCode == Keys.V) {
        //								mgsbTitol += "ペーストが指定されました";
        //							} else if (e.KeyCode == Keys.Delete) {
        //								mgsbTitol += "削除が指定されました";
        //							}
        //							DialogResult result = MessageBox.Show("ファイルもしくはフォルダを選択して下さい。",
        //																	mgsbTitol,
        //																	MessageBoxButtons.OK,
        //																	MessageBoxIcon.Exclamation,
        //																	MessageBoxDefaultButton.Button1);                   //メッセージボックスを表示する
        //		*/
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //                throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
        //            }
        //        }

        //        /// <summary>
        //        /// failreeが選択されている時に同時に押されているキーの有無を判定する
        //        /// F2が押されていたらラベル編集に入る
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FileTree_KeyUp(object sender, KeyEventArgs e)
        //        {
        //            string TAG = "[FileTree_KeyUp]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                TreeView tv = (TreeView)sender;
        //                if (tv.SelectedNode != null)
        //                {
        //                    string fullPath = tv.SelectedNode.FullPath;
        //                    dbMsg += ",fullPath=" + fullPath;       //M:\\DL\DL\新しいフォルダになってる
        //                    if (e.KeyCode == Keys.F2 && tv.SelectedNode != null && tv.LabelEdit)
        //                    {              //F2キーが離されたときは、フォーカスのあるアイテムの編集を開始
        //                        dbMsg += ";名称変更;";
        //                        tv.SelectedNode.BeginEdit();
        //                    }
        //                    else if (e.KeyCode == Keys.N && e.Shift && e.Control && tv.SelectedNode != null)
        //                    {
        //                        dbMsg += "；フォルダ作成";       //M:\\DL\DL\新しいフォルダになってる
        //                        MakeNewFolder(fullPath);
        //                    }
        //                    else if (e.KeyCode == Keys.N && e.Shift && e.Control && tv.SelectedNode != null)
        //                    {
        //                    }
        //                    else
        //                    {
        //                        FileBrowser_KeyUp(fileTree.Name, fullPath, e);
        //                    }
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //                throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
        //            }
        //        }

        //        /// <summary>
        //        /// ファイルTreeとリストの右クリックで開くコンテキストメニュークリック後の処理
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        public void FileTreeContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        //        {
        //            string TAG = "[FileTreeContextMenuStrip_ItemClicked]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "flRightClickItemUrl=" + flRightClickItemUrl;
        //                string senderName = flRightClickItemUrl;
        //                string selectItem = "";
        //                if (FilelistView.FocusedItem != null)
        //                {
        //                    selectItem = FilelistView.FocusedItem.Name;
        //                    senderName = FilelistView.Name;
        //                }
        //                else if (fileTree.SelectedNode != null)
        //                {
        //                    TreeNode selectNode = fileTree.SelectedNode;
        //                    selectItem = selectNode.FullPath;
        //                    senderName = fileTree.Name;
        //                }
        //                dbMsg += " ,senderName=" + senderName;
        //                DragListMake(senderName);
        //                dbMsg += " ,selectItem=" + selectItem;

        //                dbMsg += ",ClickedItem=" + e.ClickedItem.Name;                             //e=		常にSystem.Windows.Forms.TreeViewEventArgs,
        //                string clickedMenuItem = e.ClickedItem.Name.Replace("ToolStripMenuItem", "");
        //                dbMsg += ">>" + clickedMenuItem;
        //                dbMsg += " , selectItem=" + selectItem;
        //                if (selectItem == "")
        //                {
        //                    if (selectItem != passNameLabel.Text)
        //                    {
        //                        selectItem = passNameLabel.Text;
        //                        dbMsg += ">>" + selectItem;  // selectItem=media2.flv>>M:\sample/media2.flv,選択；ペースト,
        //                    }
        //                }
        //                string selectFullName = flRightClickItemUrl;
        //                fileTreeContextMenuStrip.Close();                                           //☆ダイアログが出ている間、メニューが表示されっぱなしになるので強制的に閉じる

        //                switch (clickedMenuItem)
        //                {                                           // クリックされた項目の Name を判定します。 
        //                    case "フォルダ作成":
        //                        dbMsg += ",選択；フォルダ作成=" + selectItem;       //M:\\DL\DL\新しいフォルダになってる
        //                        MakeNewFolder(selectItem);
        //                        break;

        //                    case "名称変更":
        //                        dbMsg += ",選択；名称変更=" + selectItem;
        //                        TargetReName(selectItem);
        //                        break;

        //                    case "カット":
        //                        cutSouce = selectItem;
        //                        dbMsg += ",選択；カット" + cutSouce;
        //                        DragListMake(senderName);
        //                        break;

        //                    case "コピー":
        //                        copySouce = selectItem;
        //                        dbMsg += ",選択；コピー" + copySouce;
        //                        DragListMake(senderName);
        //                        break;

        //                    case "ペースト":
        //                        dbMsg += ",選択；ペースト";
        //                        PeastSelecter(copySouce, cutSouce, selectItem);
        //                        break;


        //                    case "このファイルを再生":
        //                        //dbMsg += selectFullName;
        //                        PlayFromFileBrousert(selectItem);      //plRightClickItemUrl
        //                        break;

        //                    case "削除":
        //                        dbMsg += ",選択；削除;" + selectItem;
        //                        DelFiles(DragURLs, true);
        //                        break;

        //                    case "元に戻す":
        //                        dbMsg += ",選択；元に戻す";
        //                        元に戻す.Visible = false;
        //                        break;

        //                    case "他のアプリケーションで開く":
        //                        dbMsg += ",選択；他のアプリケーションで開く";
        //                        SartApication(selectItem);
        //                        break;

        //                    case "プレイリストに追加":
        //                        dbMsg += ",選択；プレイリストに追加；" + selectFullName;
        //                        string[] PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };
        //                        dbMsg += ",PLArray=" + PLArray.Length + "件";
        //                        if (PLArray.Length < 1)
        //                        {
        //                            AddPlayListFromFile(selectFullName);
        //                        }
        //                        break;

        //                    /*			case "プレイリストを作成":
        //									dbMsg += ",選択；プレイリストを作成；" + selectFullName;
        //									MakePlayList(selectFullName);
        //									break;*/
        //                    case "再生ToolStripMenuItem":
        //                        dbMsg += ",選択；再生；" + selectFullName;
        //                        break;

        //                    case "通常サイズに戻す":
        //                        dbMsg += ",選択；通常サイズに戻す";
        //                        this.FormBorderStyle = FormBorderStyle.Sizable;                         //通常サイズに戻す
        //                        this.WindowState = FormWindowState.Normal;
        //                        if (!IsWriteSysMenu)
        //                        {   //システムメニューを追記した
        //                            ReWriteSysMenu();
        //                        }
        //                        break;

        //                    default:
        //                        break;
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "でエラー発生" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>




        //        private void FileTree_Click(object sender, EventArgs e)
        //        {
        //            string TAG = "[FileTree_Click]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                TreeView tv = (TreeView)sender;
        //                TreeNode selectedNode = tv.SelectedNode;
        //                dbMsg += " ,SelectedNode=" + selectedNode.FullPath;
        //                FileTreeItemSelect(selectedNode);
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// ダブルクリックで選択ノードに合わせた動作分岐へ
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FileTree_DoubleClick(object sender, EventArgs e)
        //        {
        //            string TAG = "[FileTree_DoubleClick]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                TreeView tv = (TreeView)sender;
        //                TreeNode selectedNode = tv.SelectedNode;
        //                dbMsg += " ,SelectedNode=" + selectedNode.FullPath;
        //                FileTreeItemSelect(selectedNode);
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// fileTreeのアイテムを開く前の処理
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        //        {
        //            string TAG = "[TreeView1_BeforeExpand]";
        //            string dbMsg = TAG;
        //            //	dbMsg += "sender=" + sender;
        //            //	dbMsg += "e=" + e;
        //            try
        //            {
        //                //	TreeNode tn = e.Node;//, tn2;
        //                //	string sarchDir = tn.Text;//展開するノードのフルパスを取得		FullPath だとM:\\DL
        //                //	dbMsg += ",sarchDir=" + sarchDir;
        //                /*		string motoPass = passNameLabel.Text + "";
        //						dbMsg += ",motoPass=" + motoPass;
        //						if (motoPass != "") {
        //							sarchDir = motoPass + sarchDir;// + Path.DirectorySeparatorChar
        //						} else if (0 < motoPass.IndexOf( ":", StringComparison.OrdinalIgnoreCase )) {
        //							sarchDir = tn.Text;
        //						}
        //						dbMsg += ">sarchDir>" + sarchDir;
        //						passNameLabel.Text = sarchDir;
        //						*/
        //                //20170825		tn.Nodes.Clear();
        //                //	FolderItemListUp( sarchDir, tn );

        //                /*
        //								tn.Nodes.Clear();
        //								di = new DirectoryInfo( sarchDir );//ディレクトリ一覧を取得
        //								//string sarchDir = di.Name;
        //								MyLog( dbMsg );
        //								foreach (FileInfo fi in di.GetFiles(  )) {
        //									tn2 = new TreeNode( fi.Name, 3, 3 );
        //									string rfileName = fi.Name;
        //									rfileName = rfileName.Replace( sarchDir,"" );
        //									dbMsg += ",rfileName=" + rfileName;
        //									tn.Nodes.Add( rfileName );
        //								}
        //								MyLog( dbMsg );
        //								foreach (DirectoryInfo d2 in di.GetDirectories(  )) {
        //									tn2 = new TreeNode( d2.Name, 1, 2 );
        //									string rdirectoryName = d2.Name;
        //									 rdirectoryName = rdirectoryName.Replace( sarchDir + Path.DirectorySeparatorChar, "" );
        //									dbMsg += ",rdirectoryName=" + rdirectoryName;
        //									tn.Nodes.Add( rdirectoryName );
        //									FolderItemListUp( d2.Name, tn2 );
        //									//	tn2.Nodes.Add( "..." );
        //								}
        //								*/
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
        //            }
        //        }       //ノードを展開しようとしているときに発生するイベント

        //        /// <summary>
        //        /// FileTreeのアイテムクリック
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>	
        //        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)//NodeMouseClickが利かなかった
        //        {
        //            string TAG = "[TreeView1_AfterSelect]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                //		FileTreeItemSelect(e.Node);
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// ラベル編集モードに入ったら強制的にTargetReNameへ
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FileTree_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        //        {

        //        }

        //        /// <summary>
        //        /// fileTreeのノードがドラッグされた時
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FileTree_ItemDrag(object sender, ItemDragEventArgs e)
        //        {
        //            string TAG = "[FileTree_ItemDrag]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                TreeView tv = (TreeView)sender;
        //                mouceDownPoint = Control.MousePosition;
        //                mouceDownPoint = tv.PointToClient(mouceDownPoint);//ドラッグ開始時のマウスの位置をクライアント座標に変換
        //                dbMsg += "(mouceDownPoint;" + mouceDownPoint.X + "," + mouceDownPoint.Y + ")";      //(mouceDownPoint;735,-39)
        //                                                                                                    //	dragSouceIDP = tv.IndexFromPoint(mouceDownPoint);//マウス下のListBoxのインデックスを得る
        //                                                                                                    //	dbMsg += "(Pointから;" + dragSouceIDP + ")";
        //                                                                                                    ////////////////////////////////////////////////////////////////////////////////////////////////
        //                cutSouce = "";       //カットするアイテムのurl
        //                copySouce = "";      //コピーするアイテムのurl
        //                dragFrom = tv.Name;
        //                dragNode = (TreeNode)e.Item;
        //                tv.SelectedNode = dragNode;
        //                dragSouceIDl = tv.SelectedNode.Index; //draglist.SelectedIndex;
        //                dbMsg += "(dragSouc;" + dragSouceIDl + ")";     //(dragSouc;0)Url;M:\\sample\123.flv
        //                dragSouceUrl = tv.SelectedNode.FullPath; // draglist.SelectedValue.ToString();
        //                dbMsg += "dragSouceUrl;" + dragSouceUrl;
        //                DragURLs = new List<string>();
        //                //	for (int i = 0; i < lv.SelectedItems.Count; ++i) {
        //                //		dbMsg += "(" + i + ")";
        //                //		ListViewItem itemxs = lv.SelectedItems[i];
        //                //		string SelectedItems = lv.SelectedItems[i].Name;     //(dragSouc;0)Url;M:\\sample\123.flv
        //                //		dbMsg += SelectedItems;
        //                DragURLs.Add(dragSouceUrl);
        //                //	}

        //                tv.Focus();
        //                DDEfect = tv.DoDragDrop(dragNode, DragDropEffects.All);       //e.Item
        //                                                                              //		DDEfect = tv.DoDragDrop(dragSouceUrl, DragDropEffects.All);       //e.Item
        //                b_dragSouceUrl = "";
        //                dbMsg += "のドラッグを開始";
        //                if ((DDEfect & DragDropEffects.Move) == DragDropEffects.Move)
        //                {
        //                    cutSouce = tv.SelectedNode.FullPath;       //カットするアイテムのurl
        //                }
        //                ////////////////////////////////////////////////////////////////////////////////////////////////
        //                PlayListMouseDownNo = tv.SelectedNode.Index; //3?	draglist.SelectedIndex;
        //                dbMsg += " (Down;" + PlayListMouseDownNo + ")";     //(Down;0)M:\\sample\123.flv
        //                PlayListMouseDownValue = tv.SelectedNode.FullPath; //draglist.SelectedValue.ToString();
        //                dbMsg += PlayListMouseDownValue;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        private void FileTree_MouseMove(object sender, MouseEventArgs e)
        //        {
        //            string TAG = "[FileTree_MouseMove]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                /*			dbMsg += "(MovePoint;" + e.X + "," + e.Y + ")";
        //							Point movePoint = new Point(e.X, e.Y);
        //							movePoint = fileTree.PointToClient(movePoint);//ドラッグ開始時のマウスの位置をクライアント座標に変換
        //							dbMsg += ">>(" + movePoint.X + "," + movePoint.Y + ")";

        //							//	dbMsg += "Button=" + e.Button;
        //							//	if (e.Button == System.Windows.Forms.MouseButtons.Left) {        //左ボタン
        //							dbMsg += "(DownPoint;" + mouceDownPoint.X + "," + mouceDownPoint.Y + ")";
        //							if (mouceDownPoint != Point.Empty) {
        //								int filrTreeRight = fileTree.Left + fileTree.Width;
        //								//		filrTreeRight = draglist.PointToClient(filrTreeRight,e.Y);
        //								dbMsg += "filrTreeRight=" + filrTreeRight;
        //								if (filrTreeRight < movePoint.X) {    //PlayListに入った	|| playListBoxLeft < e.X
        //									if (-1 < dragSouceIDP) {
        //										playListBox.DoDragDrop(playListBox.Items[dragSouceIDP].ToString(), DragDropEffects.Move);//ドラッグスタート
        //										dbMsg += ">>DoDragDrop";
        //										mouceDownPoint = Point.Empty;
        //									}
        //									MyLog(TAG, dbMsg);
        //								}
        //							}
        //							//	}       //if (e.Button == System.Windows.Forms.MouseButtons.Left)*/
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// FileTreからドラッグしている時
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FileTree_DragOver(object sender, DragEventArgs e)
        //        {
        //            string TAG = "[FileTree_DragOver]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "dragFrom=" + dragFrom;
        //                dbMsg += ",dragSouceUrl=" + dragSouceUrl;
        //                dbMsg += ",DDEfect=" + DDEfect;
        //                if (dragFrom == fileTree.Name)
        //                {
        //                    dbMsg += "(MovePoint;" + e.X + "," + e.Y + ")";
        //                    Point movePoint = new Point(e.X, e.Y);
        //                    movePoint = fileTree.PointToClient(movePoint);//ドラッグ開始時のマウスの位置をクライアント座標に変換
        //                    dbMsg += ">>(" + movePoint.X + "," + movePoint.Y + ")";
        //                    TreeView tv = fileTree;// (TreeView)sender;
        //                    string dragSouce = "";
        //                    dragSouce = tv.SelectedNode.FullPath;
        //                    dbMsg += " ,dragSouce=" + dragSouce;
        //                    if ((e.KeyState & 8) == 8 && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
        //                    {
        //                        dbMsg += " , Ctrlキーが押されている>>Copy";//Ctrlキーが押されていればCopy//"8"はCtrlキーを表す
        //                        copySouce = dragSouce;      //コピーするアイテムのurl
        //                        e.Effect = DragDropEffects.Copy;
        //                    }
        //                    else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
        //                    {
        //                        dbMsg += " , 何も押されていない>>Move";
        //                        cutSouce = dragSouce;     //カットするアイテムのurl
        //                        e.Effect = DragDropEffects.Move;
        //                    }
        //                    DDEfect = e.Effect;
        //                    if (copySouce != "")
        //                    {
        //                        dbMsg += ",copy=" + copySouce;
        //                    }
        //                    if (cutSouce != "")
        //                    {
        //                        dbMsg += ",cut=" + cutSouce;
        //                    }
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// Dropを受け入れる
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FileTree_DragEnter(object sender, DragEventArgs e)
        //        {
        //            string TAG = "[FileTree_DragEnter]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "dragFrom=" + dragFrom;
        //                dbMsg += ",dragSouceUrl=" + dragSouceUrl;
        //                dbMsg += ",DDEfect=" + DDEfect;
        //                dbMsg += "'(=" + e.Effect + ")";            //FilelistViewからだとNoneになる
        //                if (DDEfect == DragDropEffects.Move)
        //                {
        //                    cutSouce = dragSouceUrl;
        //                    dbMsg += ">cutSouce>" + cutSouce;
        //                }
        //                else if (DDEfect == DragDropEffects.Copy)
        //                {
        //                    copySouce = dragSouceUrl;
        //                    dbMsg += ">copySouce>" + copySouce;
        //                }

        //                e.Effect = DDEfect;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// ドロップされたとき
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FileTree_DragDrop(object sender, DragEventArgs e)
        //        {
        //            string TAG = "[FileTree_DragDrop]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                TreeView tv = (TreeView)sender;
        //                dbMsg += "dragFrom=" + dragFrom;
        //                dbMsg += ",dragSouceUrl=" + dragSouceUrl;
        //                dbMsg += ",DDEfect=" + DDEfect;
        //                dbMsg += " , Effect(" + e.Effect + ")" + e.Effect.ToString();
        //                if (e.Effect != DragDropEffects.None && dragFrom != "")
        //                {
        //                    dbMsg += ">Drop開始>";
        //                    dbMsg += ">source=null";
        //                    fileTreeDropNode = tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y))); //ドロップ先のTreeNodeを取得する
        //                    tv.SelectedNode = fileTreeDropNode;
        //                    dbMsg += " Drop先は" + tv.SelectedNode.FullPath;
        //                    string dropSouce = fileTreeDropNode.FullPath.ToString();
        //                    dbMsg += ",dropSouce=" + dropSouce;
        //                    DropPeast(copySouce, cutSouce, dragSouceUrl, dropSouce);
        //                    /*表示だけの書き換えなら
        //						TreeNode cln = ( TreeNode ) source.Clone();                             //ドロップされたNodeのコピーを作成
        //						target.Nodes.Add( cln );												//Nodeを追加
        //						target.Expand();														//ドロップ先のNodeを展開
        //						tv.SelectedNode = cln;                                                  //追加されたNodeを選択
        //					*/
        //                    if (dragFrom == fileTree.Name)
        //                    {                //同じtreeviewの中で
        //                        if (e.Effect.ToString() == "Move")
        //                        {        //カット指定なら
        //                            cutSouce = fileTree.SelectedNode.FullPath;       //カットするアイテムのurl
        //                            dbMsg += " , 移動した時は、ドラッグしたノード=" + dragNode.Name.ToString();             //移動先に書き換わる
        //                            string dragNodeName = cutSouce.Replace(@":\\", @":\");
        //                            dbMsg += " , dragNodeName=" + dragNodeName + " を削除";
        //                            TreeNode dragParentNode = dragNode.Parent;
        //                            dbMsg += " , " + fileTreeDropNode + " を選択";
        //                            fileTree.Nodes.Remove(dragNode);
        //                            fileTree.SelectedNode = fileTreeDropNode;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    dbMsg += ">Drop中断";
        //                }
        //                e.Effect = DragDropEffects.None;
        //                fileTreeDropNode = null;
        //                dragFrom = "";
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// あるTreeNodeが別のTreeNodeの子ノードか調べる
        //        /// https://dobon.net/vb/dotnet/control/tvdraganddrop.html
        //        /// </summary>
        //        /// <param name="parentNode">親ノードか調べるTreeNode</param>
        //        /// <param name="childNode">子ノードか調べるTreeNode</param>
        //        /// <returns>子ノードの時はTrue</returns>
        //        private bool IsChildNode(TreeNode parentNode, TreeNode childNode)
        //        {      //private static ?
        //            string TAG = "[IsChildNode]";
        //            string dbMsg = TAG;
        //            bool retBool = true;
        //            try
        //            {
        //                dbMsg += "parentNode=" + parentNode.FullPath.ToString();
        //                dbMsg += " / childNode=" + childNode.FullPath.ToString();
        //                if (childNode.Parent == parentNode)
        //                {
        //                    retBool = true;
        //                }
        //                else if (childNode.Parent != null)
        //                {
        //                    retBool = IsChildNode(parentNode, childNode.Parent);
        //                }
        //                else
        //                {
        //                    retBool = false;
        //                }
        //                dbMsg += ">retBool=" + retBool;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return retBool;
        //        }

        //        /*	http://blog.ahh.jp/?p=1426
        //			//FileListVew///////////////////////////////////////////////////////////fileTreeの操作//
        //		/*
        //		 設定備考
        //		 ・HeaderStyle をCkickableで_ColumnClickが有効になる
        //		 ・showGrupe をfalseにしないと一行目に線とdefalutの文字が入る
        //			 */
        //        /// <summary>
        //        /// FileListVewの更新
        //        /// http://study-csharp.blogspot.jp/2012/08/c-listview.html	
        //        /// </summary>
        //        /// <param name="sarchDir"></param>
        //        public void FileListVewDrow(string sarchDir)
        //        {
        //            string TAG = "[FileListVewDrow]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "sarchDir=" + sarchDir;
        //                DirectoryInfo di = new DirectoryInfo(sarchDir);
        //                string fiAttributes = di.Attributes.ToString();
        //                dbMsg += ",Exists=" + di.Exists;
        //                dbMsg += ",Attributes=" + fiAttributes;
        //                //ReadOnly, Directory
        //                //Hidden, System, Directory, ReparsePoint, NotContentIndexed
        //                dbMsg += ",Root=" + di.Root;
        //                progresPanel.Visible = true;
        //                ProgressTitolLabel.Text = sarchDir;
        //                //	progresPanel.Left = -200;
        //                //	progresPanel.Top = 50;
        //                if ((di.Attributes & System.IO.FileAttributes.System) != System.IO.FileAttributes.System || sarchDir == di.Root.ToString())
        //                { //	if (!fiAttributes.Contains("System")) {			//エクスプローラーでは非表示
        //                    FilelistView.Items.Clear();
        //                    string[] files = Directory.GetFiles(sarchDir);
        //                    if (files != null)
        //                    {
        //                        int pCount = 0;
        //                        int barMax = files.Length;
        //                        progressBar1.Maximum = barMax;
        //                        foreach (string fileName in files)
        //                        {
        //                            pCount++;
        //                            progressBar1.Value = pCount;
        //                            dbMsg += "\n(" + progressBar1.Value + "/" + progressBar1.Maximum + ")" + fileName;
        //                            prgMessageLabel.Text = fileName;
        //                            FileInfo fi = new FileInfo(fileName);
        //                            dbMsg += ",DirectoryName=" + fi.DirectoryName;
        //                            dbMsg += ",Directory=" + fi.Directory.ToString();
        //                            dbMsg += ",Root=" + fi.Directory.Root.ToString();
        //                            dbMsg += ",Attributes=" + fi.Attributes;
        //                            dbMsg += ",Exists=" + fi.Exists;
        //                            string extentionStr = fi.Extension.ToLower(); //"." + extStrs[extStrs.Length - 1].ToLower();
        //                            dbMsg += ",拡張子=" + extentionStr;
        //                            if (-1 < Array.IndexOf(systemFiles, extentionStr) ||
        //                                0 < fileName.IndexOf("BOOTNXT", StringComparison.OrdinalIgnoreCase) ||
        //                                0 < fileName.IndexOf("-ms", StringComparison.OrdinalIgnoreCase) ||
        //                                0 < fileName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase)
        //                                )
        //                            {
        //                            }
        //                            else
        //                            {
        //                                int iconType = 2;
        //                                if (-1 < Array.IndexOf(videoFiles, extentionStr))
        //                                {
        //                                    iconType = 3;
        //                                }
        //                                else if (-1 < Array.IndexOf(imageFiles, extentionStr))
        //                                {
        //                                    iconType = 4;
        //                                }
        //                                else if (-1 < Array.IndexOf(audioFiles, extentionStr))
        //                                {
        //                                    iconType = 5;
        //                                }
        //                                else if (-1 < Array.IndexOf(textFiles, extentionStr))
        //                                {
        //                                    iconType = 2;
        //                                }
        //                                dbMsg += ",iconType=" + iconType;
        //                                string rfileName = fileName.Replace(fi.Directory.Root.ToString(), fi.Directory.Root.ToString() + Path.DirectorySeparatorChar);
        //                                dbMsg += ",file=" + rfileName;

        //                                ListViewItem lvi;
        //                                lvi = FilelistView.Items.Add(fi.Name);
        //                                lvi.Name = rfileName;
        //                                lvi.ImageIndex = iconType;                  //イメージを使用する	http://blog.hiros-dot.net/?p=2433
        //                                dbMsg += ",fi.Length=" + fi.Length;
        //                                float Length = (float)fi.Length;//new double(fi.Length);
        //                                                                //			Length = Length / (1024 * 1024);
        //                                dbMsg += ",Length=" + Length;
        //                                string LengthStr = fi.Length.ToString();        // string.Format("{0:f4}\r\n", fi.Length/1000 );
        //                                if (1000000 < Length)
        //                                {
        //                                    Length = fi.Length / (1024 * 1024);
        //                                    LengthStr = Math.Round(Length, 2, MidpointRounding.AwayFromZero) + "MB";  //
        //                                }
        //                                else if (1000 < Length)
        //                                {
        //                                    Length = fi.Length / 1024;
        //                                    LengthStr = Math.Round(Length, 2, MidpointRounding.AwayFromZero) + "KB";
        //                                }
        //                                dbMsg += ",LengthStr=" + LengthStr;
        //                                lvi.SubItems.Add(LengthStr);
        //                                lvi.SubItems.Add(fi.LastWriteTime.ToString());
        //                            }
        //                            ProgressMaxLabel.Text = progressBar1.Maximum.ToString();
        //                            progCountLabel.Text = progressBar1.Value.ToString();
        //                            progresPanel.Update();
        //                        }
        //                    }

        //                    string[] folderes = Directory.GetDirectories(sarchDir);//
        //                    if (folderes != null)
        //                    {
        //                        int barMax = folderes.Length;
        //                        progressBar1.Maximum = barMax;

        //                        int pCount = 0;
        //                        foreach (string directoryName in folderes)
        //                        {
        //                            pCount++;
        //                            progressBar1.Value = pCount;
        //                            dbMsg += "\n(" + progressBar1.Value + "/" + progressBar1.Maximum + ")" + directoryName;
        //                            prgMessageLabel.Text = directoryName;
        //                            di = new DirectoryInfo(directoryName);
        //                            fiAttributes = di.Attributes.ToString();
        //                            dbMsg += ",Attributes=" + fiAttributes;
        //                            if (
        //                                //	-1 < directoryName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
        //                                //	-1 < directoryName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase)|| 
        //                                (di.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System
        //                            //			|| (di.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden
        //                            )
        //                            {
        //                            }
        //                            else
        //                            {
        //                                string rdirectoryName = directoryName.Replace(di.Root.ToString(), di.Root.ToString() + Path.DirectorySeparatorChar); //sarchDir, "");// + 
        //                                                                                                                                                     //		rdirectoryName = rdirectoryName.Replace(Path.DirectorySeparatorChar + "", "");
        //                                dbMsg += ",Attributes=" + di.Attributes;
        //                                //		Attributes	Hidden | System | Directory | ReparsePoint | NotContentIndexed	System.IO.FileAttributes

        //                                dbMsg += ",Exists=" + di.Exists;
        //                                int itemCount = 0;
        //                                try
        //                                {
        //                                    dbMsg += ",GetDirectories=" + di.GetDirectories().Count();
        //                                    itemCount = di.GetDirectories().Count();
        //                                    dbMsg += ",GetFiles=" + di.GetFiles().Length;
        //                                    itemCount += di.GetFiles().Length;
        //                                }
        //                                catch (System.UnauthorizedAccessException se)
        //                                {
        //                                    dbMsg += "<<GetDirectoriesでエラー発生>>" + se.Message;
        //                                    MyLog(TAG, dbMsg);
        //                                    //	throw System.UnauthorizedAccessException;
        //                                }

        //                                if (0 < itemCount)
        //                                {
        //                                    ListViewItem lvi;
        //                                    lvi = FilelistView.Items.Add(di.Name);
        //                                    lvi.Name = rdirectoryName;
        //                                    lvi.ImageIndex = 1;                                             //folder_close_icon.png
        //                                    lvi.SubItems.Add(itemCount + "アイテム");
        //                                    dbMsg += ",LastWriteTime=" + di.LastWriteTime.ToString();
        //                                    lvi.SubItems.Add(di.LastWriteTime.ToString());
        //                                }
        //                                else
        //                                {
        //                                    //				lvi.SubItems.Add("取得不能");
        //                                }
        //                            }
        //                            ProgressMaxLabel.Text = progressBar1.Maximum.ToString();
        //                            progCountLabel.Text = progressBar1.Value.ToString();
        //                            progresPanel.Update();
        //                        }           //ListBox1に結果を表示する
        //                    }
        //                }
        //                else
        //                {
        //                    DialogResult result = MessageBox.Show("このフォルダはアクセスできません。\nAttributes;" + fiAttributes,
        //                        sarchDir,
        //                        MessageBoxButtons.OK,
        //                        MessageBoxIcon.Exclamation,
        //                        MessageBoxDefaultButton.Button1);                   //メッセージボックスを表示する
        //                    if (result == DialogResult.OK)
        //                    {                   //何が選択されたか調べる
        //                    }

        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (UnauthorizedAccessException UAEx)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + UAEx.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (PathTooLongException PathEx)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + PathEx.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            progresPanel.Visible = false;
        //        }

        //        /// <summary>
        //        /// ListVeiwのヘッダークリック
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        public void FilelistView_ColumnClick(object sender, ColumnClickEventArgs e)
        //        {
        //            string TAG = "[FilelistView_ColumnClick]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                ListView lv = (ListView)sender;
        //                int tColumn = e.Column;
        //                if (listViewItemSorter == null)
        //                {
        //                    listViewItemSorter = new ListViewItemComparer();
        //                    listViewItemSorter.ColumnModes = new ListViewItemComparer.ComparerMode[] {
        //                                        ListViewItemComparer.ComparerMode.String,
        //                                        ListViewItemComparer.ComparerMode.Integer,
        //                                        ListViewItemComparer.ComparerMode.DateTime
        //                                };
        //                    FilelistView.ListViewItemSorter = listViewItemSorter;               //ListViewItemSorterを指定する
        //                    dbMsg += "ListViewItemComparer生成";
        //                }
        //                else
        //                {
        //                    int bColumn = listViewItemSorter.Column;
        //                    dbMsg += ",現在=" + bColumn + "列目";
        //                    SortOrder bOrder = listViewItemSorter.Order;
        //                    SortOrder sOrder = bOrder;//				default(SortOrder);
        //                    dbMsg += ",Order=" + bOrder;
        //                    dbMsg += ".Mode=" + listViewItemSorter.Mode;
        //                    ListViewItemComparer.ComparerMode sMode = default(ListViewItemComparer.ComparerMode);
        //                    dbMsg += ">指定;" + tColumn + "列目";
        //                    listViewItemSorter.Column = tColumn;           //①クリックされた列を設定
        //                    lv.Sort();                                      //②並び替える
        //                                                                    //type2;ここでListViewItemComparerの作成と設定
        //                                                                    /*									if (tColumn == bColumn) {
        //																												if (bOrder == SortOrder.Descending) {
        //																													sOrder = SortOrder.Ascending;
        //																												} else if (bOrder == SortOrder.Ascending || bOrder == SortOrder.None) {
        //																													sOrder = SortOrder.Descending;
        //																												}
        //																												dbMsg += ",Order=" + sOrder;
        //																												listViewItemSorter.Order = sOrder;
        //																												//		lv.Sorting = sOrder;
        //																											}
        //												*/
        //                                                                    /*				switch (tColumn) {
        //																					case 0:
        //																						sMode = ListViewItemComparer.ComparerMode.String;
        //																						break;
        //																					case 1:
        //																						sMode = ListViewItemComparer.ComparerMode.Integer;
        //																						break;
        //																					case 2:
        //																						sMode = ListViewItemComparer.ComparerMode.DateTime;
        //																						break;
        //																				}
        //																				dbMsg += ",sMode=" + sMode;
        //																				listViewItemSorter.Mode = sMode;
        //																				FilelistView.ListViewItemSorter = new ListViewItemComparer(tColumn, sOrder, sMode);
        //																		*/        //				lv.Sort();              //②並び替える
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        private void FileViewItemSelect(string selectItem, string senderName)
        //        {
        //            string TAG = "[FileViewItemSelect]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",senderName=" + senderName;
        //                dbMsg += ",selectItem=" + selectItem;
        //                lsFullPathName = selectItem;
        //                dbMsg += ",fullPathName=" + lsFullPathName;
        //                FileInfo fi = new FileInfo(lsFullPathName);
        //                string fullName = fi.FullName;
        //                dbMsg += ",絶対パス;fullName=" + fullName;
        //                dbMsg += ",親ディレクトリ;" + fi.DirectoryName;// Directory
        //                string passNameStr = fi.DirectoryName + "";    //親ディレクトリ名
        //                if (passNameStr == "")
        //                {
        //                    passNameStr = fullName;
        //                }
        //                dbMsg += ">>" + passNameStr;
        //                string fileAttributes = fi.Attributes.ToString();
        //                dbMsg += ",Attributes=" + fileAttributes;
        //                if (fileAttributes.Contains("Directory"))
        //                {
        //                    dbMsg += ",Directoryを選択";
        //                    //		ReExpandNode(fullName);
        //                    //			FileListVewDrow(fullName);
        //                }
        //                else
        //                {
        //                    このファイルを再生ToolStripMenuItem.Visible = true;                     //プレイリストへボタン表示
        //                }
        //                //		appSettings.CurrentFile = lsFullPathName;
        //                //		WriteSetting();
        //                FileItemSelect(lsFullPathName);
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// クリックしたアイテムに合わせた動作へ
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FilelistView_MouseUp(object sender, MouseEventArgs e)
        //        {
        //            string TAG = "[FilelistView_MouseUp]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                string selectItem = lsFullPathName;
        //                ListView lv = (ListView)sender;
        //                if (dragFrom != lv.Name)
        //                {
        //                    ListViewItem FocusedItem = lv.FocusedItem;                           //フォーカスのあるアイテムのTextを表示する
        //                    if (FocusedItem != null)
        //                    {
        //                        dbMsg += ",FocusedItem=" + FocusedItem.Text;
        //                        fileNameLabel.Text = FocusedItem.Text;
        //                        selectItem = FocusedItem.Name;
        //                        flRightClickItemUrl = selectItem;
        //                    }
        //                    dbMsg += ",selectItem=" + selectItem;
        //                }

        //                if (e.Button == System.Windows.Forms.MouseButtons.Right)
        //                {          // 右クリックでコンテキストメニュー表示
        //                    Point pos = lv.PointToScreen(e.Location);
        //                    dbMsg += ",pos=" + pos;
        //                    ListView.SelectedListViewItemCollection SelectedItems = lv.SelectedItems;
        //                    dbMsg += ",SelectedItems=" + SelectedItems.Count + "件";
        //                    if (0 == SelectedItems.Count)
        //                    {
        //                        FileInfo fi = new FileInfo(flRightClickItemUrl);
        //                        flRightClickItemUrl = fi.DirectoryName;
        //                    }
        //                    titolToolStripMenuItem.Text = flRightClickItemUrl;
        //                    フォルダ作成ToolStripMenuItem.Visible = false;
        //                    名称変更ToolStripMenuItem.Visible = false;
        //                    カットToolStripMenuItem.Visible = false;
        //                    コピーToolStripMenuItem.Visible = false;
        //                    ペーストToolStripMenuItem.Visible = false;
        //                    削除ToolStripMenuItem.Visible = false;
        //                    このファイルを再生ToolStripMenuItem.Visible = false;                     //プレイリストへボタン表示
        //                    プレイリストに追加ToolStripMenuItem.Visible = false;
        //                    プレイリストを作成ToolStripMenuItem.Visible = false;
        //                    元に戻す.Visible = false;
        //                    他のアプリケーションで開くToolStripMenuItem.Visible = false;
        //                    再生ToolStripMenuItem.Visible = false;
        //                    if (1 == SelectedItems.Count)
        //                    {
        //                        名称変更ToolStripMenuItem.Visible = true;
        //                        カットToolStripMenuItem.Visible = true;
        //                        コピーToolStripMenuItem.Visible = true;
        //                        削除ToolStripMenuItem.Visible = true;
        //                        プレイリストに追加ToolStripMenuItem.Visible = true;
        //                        プレイリストを作成ToolStripMenuItem.Visible = true;
        //                        string SelectedItem = SelectedItems[0].Name;
        //                        FileInfo fi = new FileInfo(SelectedItem);
        //                        if (fi.Attributes.ToString().Contains("Directory"))
        //                        {
        //                            フォルダ作成ToolStripMenuItem.Visible = true;
        //                        }
        //                        else
        //                        {
        //                            このファイルを再生ToolStripMenuItem.Visible = true;                     //プレイリストへボタン表示
        //                        }
        //                    }
        //                    else if (1 < SelectedItems.Count)
        //                    {
        //                        カットToolStripMenuItem.Visible = true;
        //                        コピーToolStripMenuItem.Visible = true;
        //                        削除ToolStripMenuItem.Visible = true;
        //                        プレイリストに追加ToolStripMenuItem.Visible = true;
        //                        プレイリストを作成ToolStripMenuItem.Visible = false;
        //                    }
        //                    else if (SelectedItems.Count < 1)
        //                    {
        //                        フォルダ作成ToolStripMenuItem.Visible = true;
        //                    }
        //                    if (copySouce != "" || cutSouce != "")
        //                    {
        //                        ペーストToolStripMenuItem.Visible = true;
        //                    }
        //                    fileTreeContextMenuStrip.Show(pos);                     // コンテキストメニューを表示
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        private void FilelistView_DoubleClick(object sender, EventArgs e)
        //        {
        //            string TAG = "[FilelistView_DoubleClick]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                ListView lv = (ListView)sender;
        //                if (dragFrom != lv.Name)
        //                {
        //                    ListViewItem FocusedItem = lv.FocusedItem;                           //フォーカスのあるアイテムのTextを表示する
        //                    dbMsg += ",FocusedItem=" + FocusedItem.Text;
        //                    fileNameLabel.Text = FocusedItem.Text;
        //                    string selectItem = FocusedItem.Name;
        //                    dbMsg += ",selectItem=" + selectItem;
        //                    dbMsg += ",(このファイルを再生ToolStripMenuItem.Visible=" + このファイルを再生ToolStripMenuItem.Visible;
        //                    FileViewItemSelect(selectItem, FilelistView.Name);
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// ラベル編集へ
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FilelistView_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        //        {
        //            string TAG = "[FilelistView_BeforeLabelEdit]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                ListView lv = (ListView)sender;
        //                string destName = lv.FocusedItem.Text;          //.SelectedItems[0].ToString();          //.FullPath;
        //                dbMsg += ",destName=" + destName;
        //                TargetReName(destName);
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //                throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
        //            }
        //        }

        //        /// <summary>
        //        /// FilelistViewのItemがドラッグされた時
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FilelistView_ItemDrag(object sender, ItemDragEventArgs e)
        //        {
        //            string TAG = "[FilelistView_ItemDrag]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dragFrom = FilelistView.Name;
        //                cutSouce = "";       //カットするアイテムのurl
        //                copySouce = "";      //コピーするアイテムのurl
        //                ListView lv = (ListView)sender;
        //                dragFrom = lv.Name;
        //                mouceDownPoint = Control.MousePosition;
        //                mouceDownPoint = lv.PointToClient(mouceDownPoint);//ドラッグ開始時のマウスの位置をクライアント座標に変換
        //                dbMsg += "(mouceDownPoint;" + mouceDownPoint.X + "," + mouceDownPoint.Y + ")";      //(mouceDownPoint;735,-39)
        //                ListViewItem FocusedItem = lv.FocusedItem;                           //フォーカスのあるアイテムのTextを表示する
        //                dragSouceIDl = FocusedItem.Index; //draglist.SelectedIndex;
        //                dbMsg += dragFrom + "(dragSouc;" + dragSouceIDl + ")から";     //(dragSouc;0)Url;M:\\sample\123.flv
        //                dragSouceUrl = FocusedItem.Name; // draglist.SelectedValue.ToString();
        //                dragSouceUrl = checKLocalFile(dragSouceUrl);
        //                dbMsg += "dragSouceUrl;" + dragSouceUrl;
        //                DragURLs = new List<string>();
        //                for (int i = 0; i < lv.SelectedItems.Count; ++i)
        //                {
        //                    dbMsg += "(" + i + ")";
        //                    ListViewItem itemxs = lv.SelectedItems[i];
        //                    string SelectedItems = lv.SelectedItems[i].Name;     //(dragSouc;0)Url;M:\\sample\123.flv
        //                    dbMsg += SelectedItems;
        //                    DragURLs.Add(SelectedItems);
        //                }
        //                dbMsg += ">>" + DragURLs.Count + "件";     //(dragSouc;0)Url;M:\\sample\123.flv

        //                lv.Focus();
        //                DDEfect = lv.DoDragDrop(dragSouceUrl, DragDropEffects.All);       //e.Item
        //                                                                                  //		DDEfect = tv.DoDragDrop(dragSouceUrl, DragDropEffects.All);       //e.Item
        //                b_dragSouceUrl = "";
        //                dbMsg += "のドラッグを開始";
        //                if ((DDEfect & DragDropEffects.Move) == DragDropEffects.Move)
        //                {
        //                    cutSouce = FocusedItem.Name;        //カットするアイテムのurl
        //                }
        //                ////////////////////////////////////////////////////////////////////////////////////////////////
        //                PlayListMouseDownNo = dragSouceIDl; //3?	draglist.SelectedIndex;
        //                dbMsg += " (Down;" + PlayListMouseDownNo + ")";     //(Down;0)M:\\sample\123.flv
        //                PlayListMouseDownValue = dragSouceUrl;  //draglist.SelectedValue.ToString();
        //                dbMsg += PlayListMouseDownValue;

        //                dbMsg += "dragFrom=" + dragFrom;
        //                dbMsg += ",dragSouceUrl=" + dragSouceUrl;
        //                dbMsg += ",DDEfect=" + DDEfect;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        private void FilelistView_DragOver(object sender, DragEventArgs e)
        //        {
        //            string TAG = "[FilelistView_DragOver]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                copySouce = "";
        //                cutSouce = "";
        //                dbMsg += "dragFrom=" + dragFrom;
        //                dbMsg += ",dragSouceUrl=" + dragSouceUrl;
        //                dbMsg += ",DDEfect=" + DDEfect;
        //                dbMsg += "(MovePoint;" + e.X + "," + e.Y + ")";
        //                Point movePoint = new Point(e.X, e.Y);
        //                movePoint = FilelistView.PointToClient(movePoint);//ドラッグ開始時のマウスの位置をクライアント座標に変換
        //                dbMsg += ">>(" + movePoint.X + "," + movePoint.Y + ")";
        //                if (dragFrom == FilelistView.Name)
        //                {              //ドラッグされているデータがTreeNodeか調べる
        //                    ListView lv = (ListView)sender;
        //                    string dragSouce = lv.FocusedItem.Name;
        //                    dbMsg += " ,FocusedItemName=" + dragSouce;
        //                    copySouce = "";
        //                    cutSouce = "";
        //                    if ((e.KeyState & 8) == 8 && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
        //                    {
        //                        dbMsg += " , Ctrlキーが押されている>>Copy";//Ctrlキーが押されていればCopy//"8"はCtrlキーを表す
        //                        copySouce = dragSouce;      //コピーするアイテムのurl
        //                        e.Effect = DragDropEffects.Copy;
        //                    }
        //                    else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
        //                    {
        //                        dbMsg += " , 何も押されていない>>Move";
        //                        cutSouce = dragSouce;     //カットするアイテムのurl
        //                        e.Effect = DragDropEffects.Move;
        //                    }
        //                    else
        //                    {
        //                        cutSouce = dragSouce;     //カットするアイテムのurl
        //                        e.Effect = DragDropEffects.None;
        //                    }
        //                    DDEfect = e.Effect;
        //                    if (copySouce != "")
        //                    {
        //                        dbMsg += ",copy=" + copySouce;
        //                    }
        //                    if (cutSouce != "")
        //                    {
        //                        dbMsg += ",cut=" + cutSouce;
        //                    }
        //                }
        //                DDEfect = e.Effect;
        //                if (DDEfect == DragDropEffects.None)
        //                {
        //                    //				MyLog(TAG, dbMsg);
        //                }
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// 範囲から外れたらFilelistViewのDragDropを破棄
        //        /// ☆ドラッグ アンド ドロップ操作中にキーボードまたはマウス ボタンの状態に変更があると発生
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        /// 
        //        private void FilelistView_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        //        {
        //            string TAG = "[FilelistView_QueryContinueDrag]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                ListView lv = (ListView)sender;
        //                if (lv != null)
        //                {
        //                    if ((e.KeyState & 2) == 2)
        //                    {                //"2"はマウスの右ボタンを表す
        //                        dbMsg += "マウスの右ボタンでドラッグをキャンセル";
        //                        e.Action = DragAction.Cancel;
        //                    }
        //                    else if ((e.KeyState & 1) == 1)
        //                    {             //左ボタンがクリックされている時だけ処理開始
        //                        Point moucePoint = Control.MousePosition;
        //                        moucePoint = lv.PointToClient(moucePoint);//ドラッグ開始時のマウスの位置をクライアント座標に変換
        //                        dbMsg += "(moucePoint;" + moucePoint.X + "," + moucePoint.Y + ")";      //(mouceDownPoint;735,-39)
        //                        if (moucePoint != Point.Empty)
        //                        {
        //                            int FilelistViewRight = FilelistView.Left + FilelistView.Width;
        //                            dbMsg += ",FilelistView左右=" + FilelistView.Left + "～" + FilelistViewRight;
        //                            dbMsg += "上下=" + FilelistView.Top + "～" + FilelistView.Bottom;
        //                            dbMsg += ",dragSouceUrl=" + dragSouceUrl;
        //                            dbMsg += ",DDEfect=" + DDEfect;
        //                            if (FilelistViewRight < moucePoint.X)
        //                            {
        //                                e.Action = DragAction.Cancel;
        //                                if (b_dragSouceUrl != dragSouceUrl)
        //                                {
        //                                    dbMsg += ">playListBoxへ>";
        //                                    playListBox.DoDragDrop(dragSouceUrl, DragDropEffects.Copy);//ドラッグスタートし直し
        //                                }
        //                            }
        //                            else if (moucePoint.X < FilelistView.Left)
        //                            {
        //                                dbMsg += ">fileTreeへ>";
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    e.Action = DragAction.Cancel;
        //                }
        //                if (dbMsg.Contains("へ>"))
        //                {
        //                    MyLog(TAG, dbMsg);
        //                }
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        private void FilelistView_DragEnter(object sender, DragEventArgs e)
        //        {
        //            string TAG = "[FilelistView_DragEnter]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "dragFrom=" + dragFrom;
        //                dbMsg += ",dragSouceUrl=" + dragSouceUrl;
        //                dbMsg += "'(=" + e.Effect + ")";
        //                dbMsg += ",DDEfect=" + DDEfect;
        //                if (dragFrom == fileTree.Name)
        //                {
        //                    cutSouce = dragSouceUrl;
        //                    DDEfect = DragDropEffects.Move;
        //                    dbMsg += ">>" + DDEfect;
        //                    dbMsg += ">>" + cutSouce;
        //                }
        //                e.Effect = DDEfect;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        private void FilelistView_DragDrop(object sender, DragEventArgs e)
        //        {
        //            string TAG = "[FilelistView_DragDrop]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "dragFrom=" + dragFrom;
        //                dbMsg += ",dragSouceUrl=" + dragSouceUrl;
        //                dbMsg += ",DDEfect=" + DDEfect;
        //                dbMsg += " , Effect(" + e.Effect + ")" + e.Effect.ToString();
        //                if (e.Effect != DragDropEffects.None && dragFrom != "")
        //                {
        //                    ListView lv = (ListView)sender;
        //                    string farstName = lv.Items[0].Name;
        //                    System.IO.FileInfo fi = new System.IO.FileInfo(farstName);
        //                    Point moucePoint = Control.MousePosition;
        //                    moucePoint = lv.PointToClient(moucePoint);//ドラッグ開始時のマウスの位置をクライアント座標に変換
        //                    dbMsg += "(moucePoint;" + moucePoint.X + "," + moucePoint.Y + ")";      //(mouceDownPoint;735,-39)
        //                    Point ePoint = lv.PointToClient(new Point(e.X, e.Y));
        //                    dbMsg += "(ePoint;" + ePoint.X + "," + ePoint.Y + ")";      //(mouceDownPoint;735,-39)
        //                    ListViewItem dropItem = lv.GetItemAt(ePoint.X, ePoint.Y);
        //                    string dropSouce = fi.DirectoryName;       //lv.Items[0].Name;
        //                    dbMsg += ",dropSouce(表示中のフォルダ)=" + dropSouce;

        //                    if (dropItem != null)
        //                    {             //アイテム以外にドロップされた
        //                        dropSouce = dropItem.Name;       //lv.Items[0].Name;
        //                    }
        //                    fi = new System.IO.FileInfo(dropSouce);   //変更元のFileInfoのオブジェクトを作成します。 @"C:\files1\sample1.txt" 
        //                    string dropParent = fi.DirectoryName;
        //                    string fileAttributes = fi.Attributes.ToString();
        //                    dbMsg += ",Drop先の属性=" + fileAttributes;
        //                    if (fileAttributes.Contains("Directory"))
        //                    {
        //                    }
        //                    else
        //                    {
        //                        dropSouce = dropParent;
        //                    }
        //                    if (copySouce == "" && DDEfect == DragDropEffects.Copy)
        //                    {
        //                        copySouce = dragSouceUrl;
        //                        dbMsg += ">>" + copySouce;
        //                    }
        //                    else if (cutSouce == "" && DDEfect == DragDropEffects.Move)
        //                    {
        //                        cutSouce = dragSouceUrl;
        //                        dbMsg += ">>" + cutSouce;
        //                    }
        //                    DropPeast(copySouce, cutSouce, dragSouceUrl, dropSouce);
        //                }
        //                else
        //                {
        //                    dbMsg += ">Drop中断";
        //                }
        //                e.Effect = DragDropEffects.None;
        //                fileTreeDropNode = null;
        //                dragSouceUrl = "";
        //                dragFrom = "";
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// ショートカットキー処理
        //        /// F2；ラベル編集へ
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void FilelistView_KeyUp(object sender, KeyEventArgs e)
        //        {
        //            string TAG = "[FilelistView_KeyUp]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                ListView lv = (ListView)sender;
        //                dbMsg += "KeyCode=" + e.KeyCode;
        //                if (lv.FocusedItem != null)
        //                {
        //                    string fullPath = lv.FocusedItem.Name;
        //                    dbMsg += ";" + fullPath;
        //                    if (e.KeyCode == Keys.F2 && lv.LabelEdit)
        //                    {               //F2キーが離されたときは、フォーカスのあるアイテムの編集を開始
        //                        lv.FocusedItem.BeginEdit();
        //                        /*		} else if (e.KeyCode == Keys.Delete) {
        //									dbMsg += "をDelete;";
        //									DelFiles(DragURLs, true);*/
        //                    }
        //                    else if (e.KeyCode == Keys.N && e.Shift && e.Control)
        //                    {
        //                        dbMsg += "にフォルダ作成";
        //                        MakeNewFolder(fullPath);
        //                    }
        //                    else
        //                    {
        //                        FileBrowser_KeyUp(lv.Name, fullPath, e);
        //                    }
        //                }
        //                else
        //                {
        //                    dbMsg += ";FocusedItem無し";
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //                throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
        //            }
        //        }

        //        /// <summary>
        //        /// パス名ラベルのクリック>>上の階層を表示
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void PassNameLabel_Click(object sender, EventArgs e)
        //        {
        //            string TAG = "[PassNameLabel_Click]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                string passName = passNameLabel.Text;
        //                dbMsg += ",passName" + passName;
        //                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(passName);
        //                if (di.Exists)
        //                {
        //                    dbMsg += ",Root=" + di.Root.Name;
        //                    if (di.Name != di.Root.Name)
        //                    {      //ドライブルートでなければ
        //                        string passNameStr = di.Parent.FullName;
        //                        dbMsg += ",ParentName=" + passNameStr;
        //                        //		ReExpandNode(passNameStr);
        //                        FileListVewDrow(passNameStr);
        //                        passNameLabel.Text = passNameStr;
        //                        PlaylistComboBox.Items[0] = passNameStr;
        //                        di = new System.IO.DirectoryInfo(passNameStr);
        //                        FileInfo[] files = di.GetFiles();
        //                        dbMsg += ",ParentName" + files[0].Name;
        //                        fileNameLabel.Text = files[0].Name;
        //                    }
        //                }

        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }


        //        /// <summary>
        //        /// プレイリストの表示とwebのリサイズ
        //        /// </summary>
        //        /// 読出し元	ContinuousPlayCheckBox_CheckedChanged、UpDirListup
        //        private void SetPlayListItems(string carrentDir, string type)
        //        {
        //            string TAG = "[SetPlayListItems]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "Checked=" + continuousPlayCheckBox.Checked;
        //                if (continuousPlayCheckBox.Checked)
        //                {
        //                    viewSplitContainer.Panel1Collapsed = false;//リストエリアを開く
        //                                                               //		viewSplitContainer.Width = playListWidth;
        //                    progresPanel.Visible = true;
        //                    //	dbMsg += ";;playList準備；既存;" + PlayListBoxItem.Count + "件";
        //                    PlayListBoxItem = new List<PlayListItems>();
        //                    string valuestr = PlayListBoxItem.Count.ToString();
        //                    string titolStr = "指定フォルダ；" + carrentDir + "から" + type + "をリストアップ";
        //                    int nowToTal = CurrentItemCount(passNameLabel.Text);
        //                    dbMsg += ";nowToTal=" + nowToTal + "件";
        //                    if (0 == nowToTal)
        //                    {
        //                        nowToTal = 100;
        //                        dbMsg += ">>" + nowToTal + "件";
        //                    }
        //                    progressBar1.Maximum = nowToTal;
        //                    ProgressTitolLabel.Text = titolStr;
        //                    progCountLabel.Text = valuestr;                     //確認
        //                    targetCountLabel.Text = "0";                        //リストアップ
        //                    prgMessageLabel.Text = "リストアップ開始";
        //                    ProgressMaxLabel.Text = nowToTal.ToString();        //Max
        //                                                                        //		pDialog = new ProgressDialog(titolStr, maxvaluestr, valuestr);
        //                                                                        //		pDialog.ShowDialog(this);										//プログレスダイアログ表示
        //                    listUpDir = carrentDir;             //プレイリストにリストアップするデレクトリ
        //                    System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(listUpDir);
        //                    //		string parentDir = dirInfo.Parent.FullName.ToString();
        //                    //		dbMsg += ",Parent=" + parentDir;
        //                    string rootStr = dirInfo.Root.ToString();
        //                    dbMsg += ",Root=" + rootStr;
        //                    dbMsg += "；Dir;;Attributes=" + dirInfo.Attributes;
        //                    plPosisionLabel.Text = "リストアップ中";
        //                    plPosisionLabel.Update();
        //                    //再帰しながらプレイリストを作成/////////////////////
        //                    PlayListBoxItem = ListUpFiles(listUpDir, type);
        //                    /////////////////////再帰しながらプレイリストを作成//
        //                    progresPanel.Visible = false;
        //                    playListBox.DisplayMember = "NotationName";
        //                    playListBox.ValueMember = "FullPathStr";
        //                    if (PlayListBoxItem != null)
        //                    {
        //                        playListBox.DataSource = PlayListBoxItem;
        //                        dbMsg += ";PlayListBoxItem= " + PlayListBoxItem.Count + "件";
        //                        dbMsg += ",plaingItem= " + plaingItem;
        //                        string selStr = Path2titol(plaingItem);           //タイトル(ファイル名)だけを抜き出し
        //                        dbMsg += "、再生中=" + selStr;
        //                        int plaingID = playListBox.FindString(selStr);    //リスト内のインデックスを引き当て
        //                        dbMsg += "; " + plaingID + "件目";
        //                        dbMsg += ",plaingID=" + plaingID;
        //                        if (-1 < plaingID)
        //                        {
        //                            //	playListBox.SetSelected( plaingID, true );
        //                            playListBox.SelectedIndex = plaingID;           //リスト上で選択
        //                            dbMsg += "," + (playListBox.SelectedIndex + 1).ToString();
        //                            dbMsg += "/" + PlayListBoxItem.Count.ToString();
        //                            dbMsg += "," + playListBox.SelectedValue.ToString();
        //                            PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), playListBox.SelectedValue.ToString());
        //                        }
        //                        //	PlaylistComboBox.Items[0] = carrentDir;
        //                        //			PlaylistComboBox.SelectedIndex = 0;
        //                    }
        //                }
        //                else
        //                {
        //                    //			viewSplitContainer.Panel1Collapsed = true;
        //                    playListBox.Items.Clear();
        //                }
        //                //		ToView( fileNameLabel.Text );
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// プレイリストで選択されているアイテムをプレイヤーに送る
        //        /// </summary>
        //        /// <param name="plaingItem"></param>
        //        private void PlayFromPlayList(string plaingItem)
        //        {
        //            string TAG = "[PlayFromPlayList]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "(" + playListBox.SelectedIndex + ")" + playListBox.Text;
        //                //	plaingItem = playListBox.SelectedValue.ToString();
        //                dbMsg += ";plaingItem=" + plaingItem;
        //                lsFullPathName = plaingItem;
        //                PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), plaingItem);
        //                ToView(plaingItem);
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// プレイリストからの再生動作
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void PlayListBox_Select(object sender, System.EventArgs e)
        //        {
        //            string TAG = "[PlayListBox_Select]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                if (playListBox.SelectedItems != null)
        //                {
        //                    int seleCount = playListBox.SelectedItems.Count;
        //                    dbMsg += seleCount + "項目を選択";
        //                    if (0 < seleCount)
        //                    {
        //                        //	plaingItem = playListBox.SelectedItems[0].ToString();
        //                        //	} else {
        //                        dbMsg += "(" + playListBox.SelectedIndex + ")" + playListBox.Text;
        //                        plaingItem = playListBox.SelectedValue.ToString();
        //                        dbMsg += ";plaingItem=" + plaingItem;
        //                        //				このファイルを再生ToolStripMenuItem.Visible = true;                     //ファイルブラウザで選択されたアイテムを再生

        //                        PlayFromPlayList(plaingItem);
        //                        /*		lsFullPathName = plaingItem;
        //								PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), plaingItem);
        //								ToView(plaingItem);*/
        //                    }
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// プレイリストのラベルを更新する
        //        /// </summary>
        //        /// <param name="posisionStr">最上部のカウンタ数字</param>
        //        /// <param name="wrUrl">二つ、一つ上のフォルダ名</param>
        //        private void PlayListLabelWrigt(string posisionStr, string wrUrl)
        //        {
        //            string TAG = "[PlayListLabelWrigt]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                int plaingID = playListBox.SelectedIndex;
        //                dbMsg += "(" + plaingID + ")" + playListBox.Text;
        //                plPosisionLabel.Text = "";
        //                plPosisionLabel.Text = posisionStr;
        //                string[] souceNames = wrUrl.Split(Path.DirectorySeparatorChar);
        //                grarnPathLabel.Text = "";
        //                if (3 < souceNames.Length)
        //                {
        //                    grarnPathLabel.Text = souceNames[souceNames.Length - 3];
        //                }
        //                parentPathLabel.Text = "";
        //                if (2 < souceNames.Length)
        //                {
        //                    parentPathLabel.Text = souceNames[souceNames.Length - 2];
        //                }
        //                PlayListsplitContainer.Panel2.Update();
        //                //		MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// プレイリストの次へボタンをクリック
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void PlNextBbutton_Click(object sender, EventArgs e)
        //        {
        //            string TAG = "[PlNextBbutton_Click]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                int plaingID = playListBox.SelectedIndex;
        //                dbMsg += "(" + plaingID + ")" + playListBox.Text;
        //                plaingID++;
        //                if ((PlayListBoxItem.Count - 1) < plaingID)
        //                {
        //                    plaingID = 0;
        //                }
        //                playListBox.ClearSelected();
        //                playListBox.SelectedIndex = plaingID;           //リスト上で選択
        //                plaingItem = playListBox.SelectedValue.ToString();
        //                dbMsg += ">>(" + plaingID + ")" + playListBox.Text;
        //                dbMsg += ";plaingItem=" + plaingItem;
        //                lsFullPathName = plaingItem;
        //                PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), playListBox.SelectedValue.ToString());
        //                ToView(plaingItem);
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// プレイリストの前へボタンをクリック
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void PlRewButton_Click(object sender, EventArgs e)
        //        {
        //            string TAG = "[PlRewButton_Click]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                int plaingID = playListBox.SelectedIndex;
        //                dbMsg += "(" + plaingID + ")" + playListBox.Text;
        //                plaingID--;
        //                if (plaingID < 0)
        //                {
        //                    plaingID = (PlayListBoxItem.Count - 1);
        //                }
        //                playListBox.ClearSelected();
        //                playListBox.SelectedIndex = plaingID;           //リスト上で選択
        //                plaingItem = playListBox.SelectedValue.ToString();
        //                dbMsg += ">>(" + plaingID + ")" + playListBox.Text;
        //                dbMsg += ";plaingItem=" + plaingItem;
        //                lsFullPathName = plaingItem;
        //                PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), playListBox.SelectedValue.ToString());
        //                ToView(plaingItem);
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }

        //        }


        //        /// <summary>
        //        /// プレイリストのコンテキストメニュ
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void PlayListContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        //        {
        //            string TAG = "[PlayListContextMenuStrip_ItemClicked]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",ClickedItem=" + e.ClickedItem.Name;                             //e=		常にSystem.Windows.Forms.TreeViewEventArgs,
        //                string clickedMenuItem = e.ClickedItem.Name.Replace("plToolStripMenuItem", "");         //他のコンテキストメニューと同じNameは使えないのでプレイリストはplを付ける
        //                dbMsg += ">>" + clickedMenuItem;
        //                PlayListContextMenuStrip.Close();                                           //☆ダイアログが出ている間、メニューが表示されっぱなしになるので強制的に閉じる
        //                FileInfo fi = new FileInfo(plRightClickItemUrl);
        //                switch (clickedMenuItem)
        //                {                                           // クリックされた項目の Name を判定します。 
        //                    case "ファイルブラウザで選択":
        //                        dbMsg += ",選択；ファイルブラウザで選択=" + plRightClickItemUrl;
        //                        string passNameStr = fi.Directory.ToString();
        //                        dbMsg += ",fi.Directory=" + passNameStr;
        //                        FindTreeNode(fileTree, passNameStr);

        //                        FileListVewDrow(passNameStr);
        //                        passNameLabel.Text = passNameStr;
        //                        string findName = fi.Name.ToString();
        //                        dbMsg += ",fi.Name=" + findName;
        //                        fileNameLabel.Text = findName;
        //                        int tIndex = FilelistView.FindItemWithText(findName).Index;
        //                        dbMsg += "," + tIndex + "番目";
        //                        FilelistView.Items[tIndex].Focused = true;
        //                        FilelistView.Items[tIndex].Selected = true;
        //                        FilelistView.Focus();
        //                        break;
        //                    case "エクスプローラーで開く":
        //                        dbMsg += ",選択；エクスプローラーで開く；" + plRightClickItemUrl;
        //                        if (fi.Exists)
        //                        {
        //                            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
        //                            psi.FileName = fi.DirectoryName;            //デレクトリ fi.DirectoryName
        //                            dbMsg += ",psi.FileName=" + psi.FileName;
        //                            psi.Verb = "explore";
        //                            System.Diagnostics.Process.Start(psi);  //オプションに"/e"を指定してエクスプローラ式で開く	https://dobon.net/vb/dotnet/process/openexplore.html
        //                        }
        //                        else
        //                        {
        //                            DialogResult result = MessageBox.Show("該当するファイルが有りません。",
        //                                                                    fi.FullName,
        //                                                                    MessageBoxButtons.YesNo,
        //                                                                    MessageBoxIcon.Exclamation,
        //                                                                    MessageBoxDefaultButton.Button1);                   //メッセージボックスを表示する
        //                        }
        //                        break;
        //                    case "削除":
        //                        dbMsg += ",選択；削除；" + plRightClickItemUrl;
        //                        DelFromPlayList(PlaylistComboBox.Text, plIndex);
        //                        break;
        //                    case "他のアプリケーションで開く":
        //                        dbMsg += ",選択；他のアプリケーションで開く；" + plRightClickItemUrl;
        //                        SartApication(plRightClickItemUrl);
        //                        /*もしくは　https://dobon.net/vb/dotnet/process/openfile.html
        //						 	System.Diagnostics.Process p =System.Diagnostics.Process.Start(fi.FullName);
        //							p.WaitForExit();
        //						 */
        //                        break;
        //                    case "プレイリストに追加":
        //                        dbMsg += ",選択；プレイリストに追加；" + plRightClickItemUrl;
        //                        AddPlayListFromFile(plRightClickItemUrl);
        //                        break;

        //                    case "プレイリストを作成":
        //                        dbMsg += ",選択；プレイリストを作成；" + plRightClickItemUrl;
        //                        //ここから他のメソッドを呼べない？？
        //                        break;

        //                    case "通常サイズに戻す":
        //                        dbMsg += ",選択；通常サイズに戻す";
        //                        this.FormBorderStyle = FormBorderStyle.Sizable;                         //通常サイズに戻す
        //                        this.WindowState = FormWindowState.Normal;
        //                        if (!IsWriteSysMenu)
        //                        {   //システムメニューを追記した
        //                            ReWriteSysMenu();
        //                        }
        //                        break;

        //                    default:
        //                        break;
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }



        //        /// <summary>
        //        /// 表示しているプレイリストのURLを読み込んでプレイリストファイルの更新
        //        /// playListBox.Items.Remove(fileName);
        //        /// </summary>
        //        /// <param name="playList"></param>
        //        private void PlayListReWrite(string playList)
        //        {
        //            string TAG = "[Files2PlayListIndex]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += playList + "を書き直し";
        //                dbMsg += playListBox.Items.Count + "件";
        //                string rText = "";

        //                foreach (PlayListItems item in playListBox.Items)
        //                {
        //                    string FullPathStr = item.FullPathStr;          //M:\\\\sample\123.flv
        //                    if (FullPathStr != "")
        //                    {
        //                        string uriPath = FullPathStr;
        //                        Uri urlObj = new Uri(FullPathStr);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
        //                        if (urlObj.IsFile)
        //                        {                     //変換するURIがファイルを表していることを確認する
        //                            uriPath = urlObj.AbsoluteUri;
        //                            uriPath = uriPath.Replace("://", ":/");// + "\r\n";
        //                        }
        //                        rText += uriPath + "\r\n";
        //                    }
        //                    else
        //                    {
        //                        dbMsg += " 空白行";
        //                    }
        //                }
        //                dbMsg += ">>" + rText.Length + "文字";
        //                System.IO.StreamWriter sw = new System.IO.StreamWriter(playList, false, new UTF8Encoding(true));     // BOM付き
        //                sw.Write(rText);
        //                sw.Close();
        //                dbMsg += ">Exists=" + File.Exists(playList);
        //                if (PlaylistComboBox.Text == playList)
        //                {
        //                    ReadPlayList(playList);             //	再読込み
        //                }

        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// url指定でPlayListからアイテム削除
        //        /// </summary>
        //        /// <param name="fileName"></param>
        //        /// <param name="playListName"></param>
        //        public void DelPlayListItem(string fileName, string playListName)
        //        {
        //            string TAG = "[DelPlayListItem]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                int startCount = playListBox.Items.Count;
        //                dbMsg += startCount + "件";
        //                int seleCount = playListBox.SelectedItems.Count;
        //                dbMsg += seleCount + "項目を選択";
        //                dbMsg += "," + playListBox.SelectedItems[0] + "～" + playListBox.SelectedItems[playListBox.SelectedItems.Count - 1];
        //                dbMsg += playListName + " から　" + fileName + " を削除";
        //                PlayListItems PLI = PlayListBoxItem.Find(x => x.FullPathStr.Contains(fileName));        //☆List<T>内検索
        //                string NotationName = PLI.NotationName;
        //                dbMsg += ";" + NotationName + "は";
        //                int delPosition = playListBox.FindString(PLI.NotationName);
        //                dbMsg += delPosition + "番目";
        //                if (-1 < delPosition)
        //                {
        //                    DelFromPlayList(playListName, delPosition);         //☆	playListBox.Items.Remove(fileName);では消せない
        //                }
        //                int endCount = playListBox.Items.Count;
        //                dbMsg += endCount + "件";
        //                if (delPosition < playListBox.Items.Count)
        //                {
        //                    playListBox.SelectedIndex = delPosition;                //削除した次の項目を選択
        //                }
        //                else
        //                {
        //                    playListBox.SelectedIndex = playListBox.Items.Count - 1;                //削除した次の項目を選択
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception e)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + e.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        public void CheckDelPlayListItem(string fileName, bool isMsg = false)
        //        {
        //            string TAG = "[CheckDelPlayListItem]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                string playListName = PlaylistComboBox.Text;
        //                if (fileName != "" && playListName != "")
        //                {
        //                    dbMsg += playListName + " の　" + fileName + " を確認";
        //                    if (playListName != fileName)
        //                    {
        //                        if (!File.Exists(fileName))
        //                        {
        //                            if (isMsg)
        //                            {
        //                                DialogResult result = MessageBox.Show(fileName + "を" + playListName + "から削除します。",
        //                                    fileName + "が読み込めません。",
        //                                    MessageBoxButtons.YesNo,
        //                                    MessageBoxIcon.Exclamation,
        //                                    MessageBoxDefaultButton.Button1);                   //メッセージボックスを表示する
        //                                if (result == DialogResult.Yes)
        //                                {                   //何が選択されたか調べる
        //                                    dbMsg += "「はい」が選択されました";
        //                                    DelPlayListItem(fileName, playListName);
        //                                }
        //                                else if (result == DialogResult.Cancel)
        //                                {
        //                                    dbMsg += "「キャンセル」が選択されました";
        //                                }
        //                            }
        //                            else
        //                            {
        //                                DelPlayListItem(fileName, playListName);
        //                            }
        //                        }
        //                    }
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception e)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + e.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        private void MakePlayListSelectItems(string senderName)
        //        {
        //            string TAG = "[MakePlayListSelectItems]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += senderName + "から";
        //                DragURLs = new List<string>();
        //                if (senderName == FilelistView.Name)
        //                {
        //                    for (int i = 0; i < FilelistView.SelectedItems.Count; ++i)
        //                    {
        //                        dbMsg += "(" + i + ")";
        //                        ListViewItem itemxs = FilelistView.SelectedItems[i];
        //                        string SelectedItems = FilelistView.SelectedItems[i].Name;     //(dragSouc;0)Url;M:\\sample\123.flv
        //                        dbMsg += SelectedItems;
        //                        DragURLs.Add(SelectedItems);
        //                    }
        //                    dbMsg += ">>" + DragURLs.Count + "件";
        //                }
        //                else if (senderName == fileTree.Name)
        //                {
        //                    TreeNode selectNode = fileTree.SelectedNode;
        //                    dbMsg += ".selectNode=" + selectNode.FullPath;
        //                    DragURLs.Add(selectNode.FullPath);
        //                }
        //                else
        //                {
        //                    dbMsg += ".flRightClickItemUrl=" + flRightClickItemUrl;
        //                    DragURLs.Add(flRightClickItemUrl);
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //                throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
        //            }
        //        }

        //        /// <summary>
        //        /// 使用中のリストのソースを読込み、実在しないファイルを削除して再読込み
        //        /// </summary>
        //        public void CheckDelPlayListItemAll()
        //        {
        //            string TAG = "[CheckDelPlayListItemAll]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                string playListName = PlaylistComboBox.Text;
        //                string SelectedtName = playListBox.SelectedItem.ToString();
        //                dbMsg += playListName + " を確認";
        //                progresPanel.Visible = true;
        //                ProgressTitolLabel.Text = playListName + " を確認";

        //                string rText = ReadTextFile(playListName, "UTF-8"); //"Shift_JIS"では文字化け発生		UTF-8
        //                dbMsg += ",rText=" + rText.Length + "文字";
        //                string[] items = System.Text.RegularExpressions.Regex.Split(rText, "\r\n");
        //                dbMsg += ",rText=" + items.Length + "件";
        //                List<string> stringList = new List<string>();
        //                stringList.AddRange(items);//配列→List
        //                int startCount = stringList.Count;
        //                dbMsg += ",startCount=" + startCount + "件";
        //                progressBar1.Maximum = startCount;
        //                ProgressMaxLabel.Text = progressBar1.Maximum.ToString();        //Max

        //                for (int i = stringList.Count - 1; 0 < i; i--)
        //                {
        //                    dbMsg += "\n(" + i + ")";
        //                    string FullPathStr = stringList[i];
        //                    dbMsg += FullPathStr;
        //                    if (FullPathStr != "")
        //                    {
        //                        Uri urlObj = new Uri(FullPathStr);
        //                        if (urlObj.IsFile)
        //                        {             //Uriオブジェクトがファイルを表していることを確認する
        //                            FullPathStr = urlObj.LocalPath + Uri.UnescapeDataString(urlObj.Fragment);       //Windows形式のパス表現に変換する
        //                            dbMsg += ">>" + FullPathStr;
        //                        }
        //                        if (!File.Exists(FullPathStr))
        //                        {
        //                            dbMsg += "削除";
        //                            stringList.RemoveAt(i);
        //                        }
        //                        else
        //                        {
        //                            string rType = GetFileTypeStr(FullPathStr);
        //                            if (rType == "video" || rType == "audio")
        //                            {
        //                            }
        //                            else
        //                            {
        //                                dbMsg += "削除";
        //                                stringList.RemoveAt(i);
        //                            }
        //                        }
        //                    }

        //                    int remainCount = startCount - i;
        //                    progressBar1.Value = remainCount;
        //                    progCountLabel.Text = progressBar1.Value.ToString();                   //確認
        //                    prgMessageLabel.Text = "(" + remainCount + ")" + FullPathStr;
        //                    progresPanel.Update();
        //                }
        //                progresPanel.Visible = false;
        //                int endCount = stringList.Count;
        //                dbMsg += ",endCount=" + endCount + "件";
        //                rText = "";
        //                foreach (string lItem in stringList)
        //                {
        //                    rText += lItem + "\r\n";
        //                }
        //                dbMsg += ">>" + rText.Length + "文字";
        //                System.IO.StreamWriter sw = new System.IO.StreamWriter(playListName, false, new UTF8Encoding(true));     // BOM付き
        //                sw.Write(rText);
        //                sw.Close();
        //                dbMsg += ">Exists=" + File.Exists(playListName);
        //                //	if (PlaylistComboBox.Text == playListName) {
        //                ReadPlayList(playListName);             //	再読込み
        //                dbMsg += ",SelectedtName=" + SelectedtName;
        //                int selIndex = playListBox.Items.IndexOf(SelectedtName);
        //                dbMsg += "は" + selIndex + "番目";
        //                if (selIndex < 0)
        //                {
        //                    selIndex = 0;
        //                }
        //                playListBox.SelectedIndex = selIndex;
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception e)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + e.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// 汎用プレイリストの読み込みとリスト作成
        //        /// </summary>
        //        /// <param name="fileName"></param>
        //        private void ReadPlayList(string fileName)
        //        {
        //            string TAG = "[ReadPlayList]" + fileName;
        //            string dbMsg = TAG;
        //            try
        //            {
        //                AddPlaylistComboBox(fileName);
        //                //	ComboBoxAddItems(PlaylistComboBox, fileName);
        //                if (0 < playListBox.Items.Count)
        //                {
        //                    dbMsg += ",処理前=" + playListBox.Items.Count + "件";
        //                    playListBox.DataSource = null;
        //                }
        //                string rText = ReadTextFile(fileName, "UTF-8"); //"Shift_JIS"では文字化け発生
        //                                                                //	dbMsg += ",rText=" + rText;
        //                                                                //	rText = rText.Replace('/', Path.DirectorySeparatorChar);
        //                string[] files = System.Text.RegularExpressions.Regex.Split(rText, "\r\n");
        //                if (files != null)
        //                {
        //                    viewSplitContainer.Panel1Collapsed = false;//リストエリアを開く //		viewSplitContainer.Width = playListWidth;
        //                    progresPanel.Visible = true;
        //                    dbMsg += ";;playList準備；既存;" + PlayListBoxItem.Count + "件";
        //                    PlayListBoxItem = new List<PlayListItems>();
        //                    string valuestr = PlayListBoxItem.Count.ToString();
        //                    int listCount = 0;
        //                    targetCountLabel.Text = listCount.ToString();                    //確認
        //                    int nowToTal = files.Length;
        //                    dbMsg += ";nowToTal;" + nowToTal + "件";
        //                    progressBar1.Maximum = nowToTal;
        //                    progCountLabel.Text = valuestr;                     //確認
        //                    targetCountLabel.Text = "0";                        //リストアップ
        //                    prgMessageLabel.Text = "リストアップ開始";
        //                    ProgressMaxLabel.Text = nowToTal.ToString();        //Max
        //                                                                        //		pDialog = new ProgressDialog(titolStr, maxvaluestr, valuestr);保留；プログレスダイアログ
        //                                                                        //		pDialog.ShowDialog(this);										//プログレスダイアログ表示
        //                    dbMsg += "ファイル=" + files.Length + "件";
        //                    string rFileName = "";
        //                    foreach (string plFileName in files)
        //                    {
        //                        listCount++;
        //                        string prgMsg = "(" + listCount + ")" + plFileName;
        //                        //	dbMsg += "\n(" + listCount + ")" + plFileName;
        //                        if (plFileName != "")
        //                        {
        //                            FileInfo fi = new FileInfo(fileName);
        //                            if (fi.Exists)
        //                            {                     //変換するURIがファイルを表していることを確認する☆読み込み時にリロードのループになる
        //                                Uri urlObj = new Uri(plFileName);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
        //                                if (PlayListBoxItem.Count == 0)
        //                                {
        //                                    plaingItem = plFileName;
        //                                    dbMsg += ",一行目=" + plaingItem;
        //                                    //		Uri urlObj = new Uri(plaingItem);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
        //                                    if (urlObj.IsFile)
        //                                    {                     //変換するURIがファイルを表していることを確認する
        //                                        plaingItem = urlObj.LocalPath + Uri.UnescapeDataString(urlObj.Fragment);                          //Windows形式のパス表現に変換する
        //                                        dbMsg += "  >> " + plaingItem;
        //                                    }
        //                                    string type = GetFileTypeStr(plaingItem);
        //                                    dbMsg += "、type＝> " + type;
        //                                    string titolStr = "プレイリスト：" + fileName + "（" + type + "）をリストアップ";
        //                                    ProgressTitolLabel.Text = titolStr;
        //                                }
        //                                string winPath = plFileName;
        //                                if (urlObj.IsFile)
        //                                {                     //変換するURIがファイルを表していることを確認する
        //                                    winPath = urlObj.LocalPath + Uri.UnescapeDataString(urlObj.Fragment);                          //Windows形式のパス表現に変換する
        //                                    dbMsg += "  ,winPath= " + winPath;
        //                                    string[] pathStrs = winPath.Split(Path.DirectorySeparatorChar);
        //                                    winPath = winPath.Replace((":" + Path.DirectorySeparatorChar), ":" + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar);
        //                                    winPath = checKLocalFile(winPath);
        //                                    dbMsg += "Path=" + winPath;
        //                                    string wrTitol = Path2titol(winPath);//Path2titol2(plFileName, "/");
        //                                    dbMsg += ",Titol=" + wrTitol;
        //                                    PlayListItems pli = new PlayListItems(wrTitol, winPath);
        //                                    PlayListBoxItem.Add(pli);
        //                                    prgMsg += ">>" + pathStrs[pathStrs.Length - 1];
        //                                }
        //                                else
        //                                {
        //                                    prgMsg += "はファイルURIではありません。";
        //                                    dbMsg += "はファイルURIではありません。";
        //                                }
        //                            }
        //                            else
        //                            {
        //                                prgMsg += "は正常に読み込めません。";
        //                                dbMsg += "は正常に読み込めません。";
        //                            }
        //                            targetCountLabel.Text = listCount.ToString();                    //確認
        //                        }
        //                        else
        //                        {
        //                            prgMsg += " >> 処理スキップ";
        //                            dbMsg += "処理スキップ";
        //                        }
        //                        int checkCount = Int32.Parse(progCountLabel.Text) + 1;                          //pDialog.GetProgValue() + 1;
        //                        dbMsg += ",vCount=" + checkCount;
        //                        progCountLabel.Text = checkCount.ToString();                   //確認
        //                                                                                       //		progCountLabel.Update();
        //                        if (progressBar1.Maximum < checkCount)
        //                        {
        //                            progressBar1.Maximum = checkCount + 10;
        //                            ProgressMaxLabel.Text = progressBar1.Maximum.ToString();        //Max
        //                                                                                            //					ProgressMaxLabel.Update();
        //                        }
        //                        progressBar1.Value = checkCount;
        //                        PlayListLabelWrigt(listCount.ToString(), plFileName);
        //                        //	pDialog.RedrowPDialog(checkCount.ToString(),  maxvaluestr, nowCount.ToString(), wrTitol);   保留；プログレスダイアログ更新
        //                        rFileName = plFileName;
        //                        prgMessageLabel.Text = prgMsg;      //pathStrs[pathStrs.Length - 1];
        //                        progresPanel.Update();              //パネル全体をアップデート
        //                    }
        //                    typeName.Text = GetFileTypeStr(rFileName);
        //                    typeName.Update();
        //                }
        //                progresPanel.Visible = false;
        //                playListBox.DisplayMember = "NotationName";
        //                playListBox.ValueMember = "FullPathStr";
        //                playListBox.DataSource = PlayListBoxItem;
        //                dbMsg += ";PlayListBoxItem= " + PlayListBoxItem.Count + "件";
        //                dbMsg += " , plaingItem= " + plaingItem;
        //                string selStr = Path2titol(plaingItem);//Path2titol2(plFileName, "/");          //タイトル(ファイル名)だけを抜き出し
        //                dbMsg += "、再生中=" + selStr;
        //                int plaingID = playListBox.FindString(selStr);    //リスト内のインデックスを引き当て
        //                dbMsg += "; " + plaingID + "件目";
        //                dbMsg += ",plaingID=" + plaingID;
        //                if (-1 < plaingID)
        //                {
        //                    playListBox.SelectedIndex = plaingID;                               //リスト上で選択
        //                    PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), playListBox.SelectedValue.ToString());
        //                }

        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// ファイル選択ダイアログからプレイリストファイルを選ぶ
        //        /// </summary>
        //        /// <param name="addFileName"></param>
        //        private void AddPlayListFromFile(string addFileName)
        //        {
        //            string TAG = "[AddPlayListFromFile]" + addFileName;
        //            string dbMsg = TAG;
        //            try
        //            {
        //                //Windows API Code Packの CommonOpenFileDialogを使用		//☆標準はOpenFileDialog
        //                CommonOpenFileDialog ofd = new CommonOpenFileDialog();              //OpenFileDialogクラスのインスタンスを作成☆
        //                ofd.Title = "プレイリストを選択してください";              //タイトルを設定する
        //                                                            //	ofd.FileName = "default.m3u";                          //はじめのファイル名を指定する
        //                                                            //はじめに「ファイル名」で表示される文字列を指定する


        //                string initialDirectory = @"C:\";
        //                if (passNameLabel.Text != "")
        //                {
        //                    initialDirectory = passNameLabel.Text;
        //                }
        //                ofd.InitialDirectory = initialDirectory;              //はじめに表示されるフォルダを指定する
        //                ofd.RestoreDirectory = true;                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
        //                CommonFileDialogComboBox OFDcomboBox = new CommonFileDialogComboBox();
        //                OFDcomboBox.Items.Add(new CommonFileDialogComboBoxItem("先頭に挿入"));
        //                OFDcomboBox.Items.Add(new CommonFileDialogComboBoxItem("末尾に追加"));
        //                OFDcomboBox.SelectedIndex = 0;
        //                ofd.Controls.Add(OFDcomboBox);

        //                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
        //                {        //OpenFileDialogでは == DialogResult.OK)
        //                    nowLPlayList = ofd.FileName;
        //                    dbMsg += ",選択されたファイル名=" + nowLPlayList;
        //                    appSettings.CurrentList = nowLPlayList;
        //                    AddPlaylistComboBox(nowLPlayList);
        //                    //		ComboBoxAddItems(PlaylistComboBox, nowLPlayList);
        //                    /*		string[] PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };
        //							dbMsg += ",PLArray=" + PLArray.Length + "件";
        //							if (Array.IndexOf(PLArray, nowLPlayList) < 0) {         //既に登録されているリストでなければ
        //								PlaylistComboBox.Items.Add(nowLPlayList);
        //								appSettings.PlayLists = nowLPlayList;
        //								WriteSetting();
        //							}*/

        //                    //			label1.Text = ofd.FileName;
        //                    //			label2.Text = OFDcomboBox.Items[OFDcomboBox.SelectedIndex].Text;
        //                }

        //                dbMsg += ",PlayListFileNames=" + PlaylistComboBox.Items.Count + "件";
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// プレイリストに一行追加
        //        /// </summary>
        //        /// <param name="playList"></param>
        //        /// <param name="addRecord"></param>
        //        /// <param name="insarPosition"></param>
        //        private List<string> Item2PlayListIndex(List<string> stringList, string addRecord, int insarPosition)
        //        {
        //            string TAG = "[Item2PlayListIndex]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                string uriPath = addRecord;
        //                uriPath = uriPath.Replace(Path.DirectorySeparatorChar, '/');
        //                uriPath = "file://" + uriPath;              //	urlObj.AbsoluteUrでは文字化け発生
        //                dbMsg += ",uriPath=" + uriPath;
        //                dbMsg += ",stringList=" + stringList.Count + "件";
        //                stringList.Insert(insarPosition, uriPath);
        //                dbMsg += ">>" + stringList.Count + "件";
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return stringList;
        //        }

        //        private List<string> Files2PlayListIndexBody(List<string> itemList, string addFiles, int insarPosition)
        //        {
        //            string TAG = "[Files2PlayListIndexBody]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += insarPosition + "/" + itemList.Count + "へ" + addFiles;
        //                FileInfo fi = new FileInfo(addFiles);
        //                string fileAttributes = fi.Attributes.ToString();
        //                dbMsg += ",fileAttributes=" + fileAttributes;

        //                if (fileAttributes.Contains("Directory"))
        //                {
        //                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(lsFullPathName);
        //                    string[] files = Directory.GetFiles(addFiles);
        //                    foreach (string fileName in files)
        //                    {
        //                        dbMsg += ",fileName=" + fileName;
        //                        FileInfo fi2 = new FileInfo(fileName);
        //                        if (1024 < fi2.Length)
        //                        {
        //                            itemList = Item2PlayListIndex(itemList, fileName, insarPosition);
        //                            insarPosition++;
        //                        }
        //                        else
        //                        {
        //                            dbMsg += ",サイズ不足" + fi.Length;
        //                        }
        //                    }

        //                    string[] folderes = Directory.GetDirectories(addFiles);
        //                    if (folderes != null)
        //                    {
        //                        foreach (string foldereName in folderes)
        //                        {
        //                            if (-1 < foldereName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
        //                                -1 < foldereName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase))
        //                            {
        //                            }
        //                            else
        //                            {
        //                                /*		string rdirectoryName = directoryName.Replace(addRecord, "");// + 
        //										rdirectoryName = rdirectoryName.Replace(Path.DirectorySeparatorChar + "", "");
        //										dbMsg += ",foler=" + rdirectoryName;*/
        //                                itemList = Files2PlayListIndexBody(itemList, foldereName, insarPosition);
        //                                insarPosition++;
        //                            }
        //                        }           //ListBox1に結果を表示する
        //                    }
        //                }
        //                else
        //                {
        //                    if (1024 < fi.Length)
        //                    {
        //                        itemList = Item2PlayListIndex(itemList, addFiles, insarPosition);
        //                    }
        //                    else
        //                    {
        //                        dbMsg += ",サイズ不足" + fi.Length;
        //                    }
        //                }

        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return itemList;
        //        }

        //        private void Files2PlayListIndex(string playList, string addFiles, int insarPosition)
        //        {
        //            string TAG = "[Files2PlayListIndex]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += playList + "へ" + addFiles + "を" + insarPosition + "から";
        //                string rText = ReadTextFile(playList, System.Text.Encoding.UTF8.ToString()); //"Shift_JIS"では文字化け発生,	"UTF-8"では%E5%9B%9E/[%E5
        //                                                                                             //	string rText = ReadTextFile(playList, "UTF-8"); //"Shift_JIS"では文字化け発生
        //                dbMsg += "　rText=" + rText.Length + "文字";
        //                string[] items = System.Text.RegularExpressions.Regex.Split(rText, "\r\n");
        //                dbMsg += " ,rText=" + items.Length + "件";
        //                List<string> stringList = new List<string>();
        //                stringList.AddRange(items);//配列→List
        //                dbMsg += ",stringList=" + stringList.Count + "件";
        //                dbMsg += ",drag=" + DragURLs.Count + "件";
        //                foreach (string aFiles in DragURLs)
        //                {
        //                    dbMsg += "(" + insarPosition + "へ)" + aFiles;
        //                    stringList = Files2PlayListIndexBody(stringList, aFiles, insarPosition);
        //                    insarPosition++;
        //                }
        //                dbMsg += ">>" + stringList.Count + "件";
        //                rText = "";
        //                foreach (string lItem in stringList)
        //                {
        //                    if (lItem != "")
        //                    {
        //                        rText += lItem + "\r\n";
        //                    }
        //                    else
        //                    {
        //                        dbMsg += " 空白行";
        //                    }
        //                }
        //                dbMsg += ">>" + rText.Length + "文字";
        //                System.IO.StreamWriter sw = new System.IO.StreamWriter(playList, false, new UTF8Encoding(true));     // BOM付き
        //                sw.Write(rText);
        //                sw.Close();
        //                dbMsg += ">Exists=" + File.Exists(playList);
        //                if (PlaylistComboBox.Text == playList)
        //                {
        //                    ReadPlayList(playList);             //	再読込み
        //                }

        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// アイテムを一つプレイリストに追記
        //        /// </summary>
        //        /// <param name="addList"></param>
        //        /// <param name="addRecord"></param>
        //        /// <param name="toTop"></param>
        //        /// <returns></returns>
        //        private string Item2PlayListBody(string addList, string addRecord, bool toTop)
        //        {
        //            string TAG = "[Item2PlayListBody]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                string Type = GetFileTypeStr(addRecord);
        //                string uriPath = MakePlayListRecordBody(addRecord, Type);
        //                /*
        //				uriPath = addRecord.Replace(Path.DirectorySeparatorChar, '/');
        //				uriPath = "file://" + addRecord;
        //				Uri urlObj = new Uri(addRecord);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
        //				if (urlObj.IsFile) {                     //変換するURIがファイルを表していることを確認する
        //					uriPath = urlObj.AbsoluteUri;
        //					uriPath = uriPath.Replace("://", ":/");// + "\r\n";
        //				}
        //								string[] files = System.Text.RegularExpressions.Regex.Split(rText, "\r\n");
        //								plaingItem = files[0];
        //								dbMsg += ",一行目=" + plaingItem;
        //								*/
        //                dbMsg += ",uriPath=" + uriPath;
        //                addList = addList.Replace("\r\n\r\n", "\r\n");
        //                if (toTop)
        //                {
        //                    addList = uriPath + "\r\n" + addList;
        //                }
        //                else
        //                {
        //                    addList = addList + "\r\n" + uriPath;
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return addList;
        //        }

        //        /// <summary>
        //        /// プレイリスト追加の再帰部分
        //        /// </summary>
        //        /// <param name="addList"></param>
        //        /// <param name="addFiles"></param>
        //        /// <param name="toTop"></param>
        //        /// <returns></returns>
        //        private string AddOne2PlayListBody(string addList, string addFiles, bool toTop)
        //        {
        //            string TAG = "[AddOne2PlayListBody]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += addFiles + "をtoTop=" + toTop;
        //                FileInfo fi = new FileInfo(addFiles);
        //                string fileAttributes = fi.Attributes.ToString();
        //                dbMsg += ",fileAttributes=" + fileAttributes;
        //                if (fileAttributes.Contains("Directory"))
        //                {
        //                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(addFiles);           //lsFullPathName
        //                    string[] files = Directory.GetFiles(addFiles);
        //                    foreach (string fileName in files)
        //                    {
        //                        dbMsg += ",fileName=" + fileName;
        //                        addList = Item2PlayListBody(addList, fileName, toTop);
        //                    }

        //                    string[] folderes = Directory.GetDirectories(addFiles);
        //                    if (folderes != null)
        //                    {
        //                        foreach (string directoryName in folderes)
        //                        {
        //                            if (-1 < directoryName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
        //                                -1 < directoryName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase))
        //                            {
        //                            }
        //                            else
        //                            {
        //                                string rdirectoryName = directoryName.Replace(addFiles, "");// + 
        //                                rdirectoryName = rdirectoryName.Replace(Path.DirectorySeparatorChar + "", "");
        //                                dbMsg += ",foler=" + rdirectoryName;
        //                                addList = AddOne2PlayListBody(addList, directoryName, toTop);

        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    addList = Item2PlayListBody(addList, addFiles, toTop);
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return addList;
        //        }

        //        /// <summary>
        //        /// 指定したプレイリストも先頭か末尾にアイテムを追加
        //        /// </summary>
        //        /// <param name="fileName"></param>
        //        /// <param name="addFiles"></param>
        //        /// <param name="topBottom"></param>
        //        private void AddOne2PlayList(string fileName, string addFiles, bool toTop)
        //        {
        //            string TAG = "[AddOne2PlayList]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += fileName + "へ" + addFiles + "をtoTop=" + toTop;
        //                string rText = ReadTextFile(fileName, "UTF-8"); //"Shift_JIS"では文字化け発生
        //                                                                //	dbMsg += ",rText=" + rText;
        //                                                                //	rText = rText.Replace('/', Path.DirectorySeparatorChar);
        //                rText = AddOne2PlayListBody(rText, addFiles, toTop);

        //                MyLog(TAG, dbMsg);
        //                System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, false, new UTF8Encoding(true));     // BOM付き
        //                sw.Write(rText);
        //                sw.Close();
        //                dbMsg += ">Exists=" + File.Exists(fileName);
        //                if (PlaylistComboBox.Text == fileName)
        //                {
        //                    ReadPlayList(fileName);             //	再読込み
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// プレイリストから指定した位置のアイテムを削除する
        //        /// ☆文字照合では同じアイテムを全て消してしまうので位置指定
        //        /// 複数選択の場合はクリックしたポイントが渡されるので選択範囲確認
        //        /// 選択状態にならずに削除メニューが選ばれる場合があるので、指定ポジションを削除
        //        /// </summary>
        //        /// <param name="playList"></param>
        //        /// <param name="delPosition"></param>
        //        private void DelFromPlayList(string playList, int delPosition)
        //        {       //, string deldRecordp
        //            string TAG = "[DelFromPlayList]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                int startCount = playListBox.Items.Count;
        //                dbMsg += ",playList=" + playList + "(開始時" + startCount + "件中";
        //                int seleCount = playListBox.SelectedItems.Count;
        //                dbMsg += "(選択" + seleCount + "項目)";
        //                int seleStarPosition = -1;
        //                int seleEndPosition = delPosition;
        //                List<int> plSelects = new List<int>();
        //                for (int i = 0; i < playListBox.SelectedItems.Count; ++i)
        //                {
        //                    //if (playListBox.GetSelected(i)) {
        //                    plSelects.Add(playListBox.SelectedIndex);
        //                    if (seleStarPosition == -1)
        //                    {
        //                        seleStarPosition = playListBox.SelectedIndex;
        //                        dbMsg += "(" + seleStarPosition + ")";// + playListBox.SelectedItems[seleStarPosition];
        //                    }
        //                    //	}else
        //                    if (-1 < seleStarPosition && seleStarPosition <= seleEndPosition)
        //                    {
        //                        seleEndPosition = playListBox.SelectedIndex;
        //                        dbMsg += "～(" + seleEndPosition + ")" + playListBox.Items[seleEndPosition];
        //                    }

        //                }
        //                if (seleStarPosition == -1)
        //                {
        //                    seleStarPosition = delPosition;
        //                    dbMsg += ">>(" + seleStarPosition + ")";    // + playListBox.Items[seleStarPosition];
        //                }
        //                string rText = ReadTextFile(playList, "UTF-8"); //"Shift_JIS"では文字化け発生
        //                                                                //	dbMsg += ",rText=" + rText;
        //                                                                //	rText = rText.Replace('/', Path.DirectorySeparatorChar);
        //                dbMsg += ",rText=" + rText.Length + "文字";
        //                /*		string uriPath = deldRecordp;
        //							Uri urlObj = new Uri(deldRecordp);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
        //							if (urlObj.IsFile) {                     //変換するURIがファイルを表していることを確認する
        //								uriPath = urlObj.AbsoluteUri;
        //								uriPath = uriPath.Replace("://", ":/");
        //							}
        //							uriPath = uriPath + "\r\n";
        //							dbMsg +=  " uriPath" + uriPath;
        //						//	rText = rText.Replace(uriPath, "");*/
        //                string[] items = System.Text.RegularExpressions.Regex.Split(rText, "\r\n");
        //                dbMsg += ",rText=" + items.Length + "件";
        //                List<string> stringList = new List<string>();
        //                stringList.AddRange(items);//配列→List
        //                dbMsg += ",stringList=" + stringList.Count + "件";
        //                dbMsg += ",plSelects=" + plSelects.Count + "件";
        //                foreach (int seleIndex in plSelects)
        //                {
        //                    dbMsg += " (" + seleIndex + ")";
        //                    string deldRecord = stringList[seleIndex];
        //                    dbMsg += deldRecord;
        //                    stringList.RemoveAt(seleIndex);
        //                }
        //                /*			for (int i = seleStarPosition; i <= seleEndPosition; ++i) {
        //								dbMsg += "(" + i+")";
        //								string deldRecord = stringList[delPosition+1];
        //								dbMsg += ",deldRecordp=" + deldRecord;
        //								stringList.RemoveAt(delPosition);
        //							}*/
        //                dbMsg += ",stringList=" + stringList.Count + "件";
        //                rText = "";
        //                foreach (string lItem in stringList)
        //                {
        //                    if (lItem != "")
        //                    {
        //                        rText += lItem + "\r\n";
        //                    }
        //                }
        //                dbMsg += ">>" + rText.Length + "文字";
        //                System.IO.StreamWriter sw = new System.IO.StreamWriter(playList, false, new UTF8Encoding(true));     // BOM付き
        //                sw.Write(rText);
        //                sw.Close();
        //                dbMsg += ">Exists=" + File.Exists(playList);
        //                if (PlaylistComboBox.Text == playList)
        //                {
        //                    ReadPlayList(playList);                                             //	再読込み
        //                    playListBox.SelectionMode = SelectionMode.One;                      //1:単一選択に
        //                    if (delPosition < playListBox.Items.Count)
        //                    {
        //                        playListBox.SelectedIndex = delPosition;
        //                    }
        //                    else
        //                    {
        //                        playListBox.SelectedIndex = playListBox.Items.Count - 1;
        //                    }
        //                }

        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// fileタイプを判定して該当すればwindowsフルパスをURlに書き換える
        //        /// </summary>
        //        /// <param name="addRecord"></param>
        //        /// <param name="Type"></param>
        //        /// <returns></returns>
        //        private string MakePlayListRecordBody(string addRecord, string Type)
        //        {
        //            string TAG = "[MakePlayListRecordBody]";
        //            string dbMsg = TAG;
        //            string uriPath = "";
        //            try
        //            {
        //                dbMsg += addRecord + ";Type=" + Type;
        //                string rType = GetFileTypeStr(addRecord);
        //                if (rType == Type)
        //                {
        //                    uriPath = addRecord.Replace(Path.DirectorySeparatorChar, '/');
        //                    uriPath = "file://" + uriPath;
        //                    /*		Uri urlObj = new Uri("file://" + addRecord);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
        //							if (urlObj.IsFile) {                                             //変換するURIがファイルを表していることを確認する
        //								addRecord = urlObj.AbsoluteUri;                        //Windows形式のパスをURIに変換	 urlObj.AbsoluteUriでは文字化けする
        //								addRecord = addRecord.Replace("://", ":/") + "\r\n";
        //								dbMsg += "  >> " + addRecord;
        //							}*/
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return uriPath;
        //        }

        //        /// <summary>
        //        ///指定されたファイル/フォルダから新規プレイリストを作成する
        //        /// </summary>
        //        /// <param name="addFiles"></param>
        //        /// <param name="Type"></param>
        //        /// <returns></returns>
        //        private string MakePlayListRecprd(string addFiles, string Type)
        //        {
        //            string TAG = "[MakePlayListRecprd]";
        //            string dbMsg = TAG;
        //            string addRecord = "";
        //            try
        //            {
        //                dbMsg += addFiles + ";Type=" + Type;
        //                FileInfo fi = new FileInfo(addFiles);
        //                string fileAttributes = fi.Attributes.ToString();
        //                dbMsg += ",fileAttributes=" + fileAttributes;
        //                if (fileAttributes.Contains("Directory"))
        //                {
        //                    string[] files = Directory.GetFiles(addFiles);        //		sarchDir	"C:\\\\マイナンバー.pdf"	string	☆sarchDir = "\\2013.m3u"でフルパスになっていない
        //                    foreach (string fileName in files)
        //                    {
        //                        dbMsg += ",fileName=" + fileName;
        //                        addRecord += MakePlayListRecordBody(fileName, Type) + "\r\n";
        //                    }

        //                    string[] directries = Directory.GetDirectories(addFiles);//
        //                    if (directries != null)
        //                    {
        //                        foreach (string foldereName in directries)
        //                        {
        //                            if (-1 < foldereName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
        //                                -1 < foldereName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase))
        //                            {
        //                            }
        //                            else
        //                            {
        //                                dbMsg += ",foler=" + foldereName;
        //                                addRecord += MakePlayListRecprd(foldereName, Type);
        //                            }
        //                        }           //ListBox1に結果を表示する
        //                    }
        //                }
        //                else
        //                {                    //単一のファイル名
        //                    addRecord = MakePlayListRecordBody(addFiles, Type) + "\r\n";
        //                }
        //                addRecord = addRecord.Replace("\r\n\r\n", "\r\n");
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return addRecord;
        //        }

        //        /// <summary>
        //        ///プレイリスト作成
        //        /// </summary>
        //        /// <param name="addFiles"></param>
        //        /// <param name="Type"></param>
        //        private void MakePlayList(string addFiles, string Type)
        //        {
        //            string TAG = "[MakePlayList]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ",addRecord=" + addFiles + "からプレイリスト作成;Type=" + Type;
        //                string addRecord = MakePlayListRecprd(addFiles, Type);
        //                dbMsg += ">>" + addRecord;
        //                if (addRecord.Length < 1)
        //                {
        //                    //メッセージボックスを表示する
        //                    DialogResult result = MessageBox.Show(addFiles + "に" + Type + "は有りませんでした。", "検索結果",
        //                        MessageBoxButtons.OK,
        //                        MessageBoxIcon.Exclamation,
        //                        MessageBoxDefaultButton.Button1);
        //                }
        //                else
        //                {
        //                    string initialDirectory = appSettings.CurrentList;
        //                    if (initialDirectory == "")
        //                    {
        //                        initialDirectory = appSettings.CurrentFile;
        //                    }
        //                    SaveFileDialog sfd = new SaveFileDialog();              //SaveFileDialogクラスのインスタンスを作成
        //                    sfd.FileName = "新しいプレイリスト.m3u";              //はじめのファイル名を指定する
        //                    sfd.InitialDirectory = initialDirectory;                          //				//はじめに表示されるフォルダを指定する
        //                    sfd.Filter = "プレイリスト(*.m3u)|*.m3u|すべてのファイル(*.*)|*.*";               //[ファイルの種類]に表示される選択肢を指定する//指定しない（空の文字列）の時は、現在のディレクトリが表示される
        //                    sfd.FilterIndex = 1;                //[ファイルの種類]ではじめに選択されるものを指定する
        //                    sfd.Title = "新しいプレイリストの作成";                //タイトルを設定する
        //                    sfd.RestoreDirectory = true;                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
        //                    sfd.OverwritePrompt = true;             //既に存在するファイル名を指定したとき警告する//デフォルトでTrueなので指定する必要はない
        //                    sfd.CheckPathExists = true;             //存在しないパスが指定されたとき警告を表示する//デフォルトでTrueなので指定する必要はない
        //                    if (sfd.ShowDialog() == DialogResult.OK)
        //                    {              //ダイアログを表示する
        //                        dbMsg += " ,FileName= " + sfd.FileName;
        //                        System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName, false, new UTF8Encoding(true));
        //                        sw.Write(addRecord);                        //ファイルに書き込む
        //                        sw.Close();                     //閉じる
        //                    }
        //                    viewSplitContainer.Panel1Collapsed = false;//リストエリアを開く
        //                    ComboBoxAddItems(PlaylistComboBox, sfd.FileName);
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }//形式に合わせたhtml作成

        //        /// <summary>
        //        /// ファイルセレクトからプレイリストを選択する
        //        /// </summary>
        //        public void SelectPlayList()
        //        {
        //            string TAG = "[SelecPlayList]";// + fileName;
        //            string dbMsg = TAG;
        //            try
        //            {
        //                string initialDirectory = "";
        //                if (appSettings.CurrentFile != "")
        //                {
        //                    FileInfo fi = new FileInfo(nowLPlayList);
        //                    string passNameStr = fi.Directory.ToString();
        //                    dbMsg += ",Directory=" + passNameStr;
        //                    initialDirectory = passNameStr;
        //                }
        //                else if (passNameLabel.Text != "")
        //                {
        //                    initialDirectory = passNameLabel.Text;
        //                }
        //                string initialFile = "*.m3u";

        //                string[] PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };		playerUrl
        //                dbMsg += ",PLArray=" + PLArray.Length + "件";
        //                if (0 < PLArray.Length)
        //                {
        //                    nowLPlayList = PLArray[0].ToString();// PlayListFileNames[PlayListFileNames.Count()-1].ToString();
        //                    dbMsg += ",nowLPlayList=" + nowLPlayList;
        //                    string[] iDirectorys = nowLPlayList.Split(Path.DirectorySeparatorChar);
        //                    initialFile = iDirectorys[iDirectorys.Length - 1];
        //                    dbMsg += ",initialFile=" + initialFile;
        //                    initialDirectory = nowLPlayList.Replace(initialFile, "");
        //                    dbMsg += ",initialDirectory=" + initialDirectory;
        //                }

        //                OpenFileDialog ofd = new OpenFileDialog();              //OpenFileDialogクラスのインスタンスを作成
        //                ofd.FileName = initialFile;                          //はじめのファイル名を指定する
        //                                                                     //はじめに「ファイル名」で表示される文字列を指定する
        //                ofd.InitialDirectory = initialDirectory;              //はじめに表示されるフォルダを指定する
        //                                                                      //指定しない（空の文字列）の時は、現在のディレクトリが表示される
        //                ofd.Filter = "プレイリスト(*.m3u)|*.m3u|すべてのファイル(*.*)|*.*";               //[ファイルの種類]に表示される選択肢を指定する		"HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";  
        //                                                                                    //指定しないとすべてのファイルが表示される
        //                                                                                    //	ofd.FilterIndex = 2;                //[ファイルの種類]ではじめに選択されるものを指定する
        //                                                                                    //2番目の「すべてのファイル」が選択されているようにする
        //                ofd.Title = "プレイリストを選択してください";              //タイトルを設定する
        //                ofd.RestoreDirectory = true;                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
        //                ofd.CheckFileExists = true;             //存在しないファイルの名前が指定されたとき警告を表示する
        //                                                        //デフォルトでTrueなので指定する必要はない
        //                ofd.CheckPathExists = true;             //存在しないパスが指定されたとき警告を表示する
        //                                                        //デフォルトでTrueなので指定する必要はない

        //                if (ofd.ShowDialog() == DialogResult.OK)
        //                {              //ダイアログを表示する
        //                    nowLPlayList = ofd.FileName;                                               //	string fileName= ofd.FileName;
        //                    dbMsg += ",選択されたファイル名=" + nowLPlayList;
        //                    appSettings.CurrentList = nowLPlayList;
        //                    AddPlaylistComboBox(nowLPlayList);
        //                    //	ComboBoxAddItems(PlaylistComboBox, nowLPlayList);
        //                    if (passNameLabel.Text == "")
        //                    {
        //                        FileInfo fi = new FileInfo(nowLPlayList);
        //                        string passNameStr = fi.Directory.ToString();
        //                        dbMsg += ",Directory=" + passNameStr;
        //                        passNameLabel.Text = passNameStr;
        //                    }

        //                }
        //                PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };		playerUrl
        //                dbMsg += ">>PLArray=" + PLArray.Length + "件";
        //                viewSplitContainer.Panel1Collapsed = false;//リストエリアを開く

        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }

        //        }

        //        private List<String> SarchExtFilsBody(string carrentDir, string sarchExtention, List<String> PlayListFileNames)
        //        {
        //            string TAG = "[SarchExtFilsBody]" + sarchExtention;
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "carrentDir=" + carrentDir + ",sarchExtention=" + sarchExtention;
        //                string sarchDir = carrentDir;
        //                string[] files = Directory.GetFiles(sarchDir);
        //                int listCount = -1;
        //                int tCount = PlayListFileNames.Count();
        //                int nowCount = PlayListFileNames.Count;
        //                int checkCount = progressBar1.Value;
        //                dbMsg += "/PlayListBoxItem; " + PlayListBoxItem.Count + "件";
        //                string wrTitol = "";
        //                //		int nowToTal = CurrentItemCount(sarchDir);      // サブディレクトリ内のファイルもカウントする場合	, SearchOption.AllDirectories
        //                //		dbMsg += ",このデレクトリには" + nowToTal + "件";
        //                int barMax = progressBar1.Maximum;
        //                dbMsg += ",progressMax=" + barMax + "件";
        //                /*	if (barMax < nowToTal) {
        //						progressBar1.Maximum = nowToTal;
        //						ProgressMaxLabel.Text = progressBar1.Maximum.ToString();        //Max
        //						ProgressMaxLabel.Update();
        //					}*/
        //                if (files != null)
        //                {
        //                    dbMsg += "ファイル=" + files.Length + "件";
        //                    foreach (string plFileName in files)
        //                    {
        //                        listCount++;
        //                        dbMsg += "\n(" + listCount + ")" + plFileName;
        //                        string[] pathStrs = plFileName.Split(Path.DirectorySeparatorChar);
        //                        System.IO.FileAttributes attr = System.IO.File.GetAttributes(plFileName);
        //                        if ((attr & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden)
        //                        {
        //                            dbMsg += ">>Hidden";
        //                        }
        //                        else if ((attr & System.IO.FileAttributes.System) == System.IO.FileAttributes.System)
        //                        {
        //                            dbMsg += ">>System";
        //                        }
        //                        else if (-1 == Array.IndexOf(systemFiles, plFileName))
        //                        {
        //                            string[] extStrs = plFileName.Split('.');
        //                            string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
        //                            dbMsg += "拡張子=" + extentionStr;
        //                            if (extentionStr == sarchExtention)
        //                            {
        //                                dbMsg += ",Titol=" + wrTitol;
        //                                PlayListFileNames.Add(plFileName);
        //                                tCount = PlayListFileNames.Count();//Int32.Parse(targetCountLabel.Text) + 1;
        //                                targetCountLabel.Text = tCount.ToString();                    //確認
        //                                targetCountLabel.Update();
        //                                //				prgMessageLabel.Text = plFileName;// pathStrs[pathStrs.Length - 1];
        //                            }
        //                            //		prgMessageLabel.Text = plFileName;       //StackOverflowException
        //                            //		prgMessageLabel.Update();
        //                        }
        //                        checkCount = progressBar1.Value + 1;// Int32.Parse(progCountLabel.Text) + 1;                          //pDialog.GetProgValue() + 1;
        //                        dbMsg += ",checkCount=" + checkCount.ToString();

        //                        progCountLabel.Update();
        //                        if (progressBar1.Maximum < checkCount)
        //                        {
        //                            progressBar1.Maximum = checkCount + 100;
        //                            ProgressMaxLabel.Text = (progressBar1.Maximum - 100).ToString();        //Max
        //                                                                                                    //		ProgressMaxLabel.Update();
        //                        }
        //                        progressBar1.Value = checkCount;
        //                        //	progresPanel.Update();
        //                        //	PlayListLabelWrigt(tCount.ToString(), plFileName);
        //                        //	pDialog.RedrowPDialog(checkCount.ToString(),  maxvaluestr, nowCount.ToString(), wrTitol);   保留；プログレスダイアログ更新
        //                    }
        //                }
        //                prgMessageLabel.Text = carrentDir;       //StackOverflowException
        //                progCountLabel.Text = checkCount.ToString();                   //$exception	{"種類 'System.StackOverflowException' の例外がスローされました。"}	System.StackOverflowException
        //                progresPanel.Update();

        //                string[] folderes = Directory.GetDirectories(sarchDir);//
        //                if (folderes != null)
        //                {
        //                    foreach (string directoryName in folderes)
        //                    {
        //                        System.IO.FileAttributes attr = System.IO.File.GetAttributes(directoryName);
        //                        if ((attr & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden)
        //                        {
        //                            dbMsg += ">>Hidden";
        //                        }
        //                        else if ((attr & System.IO.FileAttributes.System) == System.IO.FileAttributes.System)
        //                        {
        //                            dbMsg += ">>System";
        //                        }
        //                        else if (0 < Array.IndexOf(systemFiles, directoryName))
        //                        {
        //                        }
        //                        else
        //                        {
        //                            SarchExtFilsBody(directoryName, sarchExtention, PlayListFileNames);        //再帰
        //                        }
        //                    }
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return PlayListFileNames;
        //        }

        //        /// <summary>
        //        /// 指定した拡張子のファイルをリストアップ
        //        /// </summary>
        //        /// <param name="sarchExtention"></param>
        //        private void SarchExtFils(string sarchExtention)
        //        {
        //            string TAG = "[sarchExtFils]" + sarchExtention;
        //            string dbMsg = TAG;
        //            try
        //            {
        //                int dCount = 0;
        //                //	int fCount = 0;
        //                dbMsg += ",driveNames=" + PlayListFileNames.Count + "件";

        //                if (0 < PlayListFileNames.Count)
        //                {
        //                    PlayListFileNames = new List<String>();
        //                }
        //                progresPanel.Visible = true;
        //                prgMessageLabel.Text = "リストアップ開始";
        //                progressBar1.Maximum = 100;
        //                progressBar1.Value = 0;

        //                ProgressTitolLabel.Text = "プレイリスト " + sarchExtention + "を抽出";
        //                foreach (DriveInfo drive in DriveInfo.GetDrives())
        //                { /////http://www.atmarkit.co.jp/fdotnet/dotnettips/557driveinfo/driveinfo.html
        //                    dCount++;
        //                    string driveNames = drive.Name; // ドライブ名
        //                    dbMsg += ",driveNames=" + driveNames;
        //                    if (drive.IsReady)
        //                    { // 使用可能なドライブのみ
        //                        PlayListFileNames = SarchExtFilsBody(driveNames, sarchExtention, PlayListFileNames);
        //                        progresPanel.Update();
        //                        progresPanel.Focus();
        //                    }
        //                }
        //                progresPanel.Visible = false;
        //                dbMsg += ",=" + PlayListFileNames.Count() + "件";
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }


        //        /// <summary>
        //        /// 指定したComboBoxのアイテムをstring[]で返す
        //        /// </summary>
        //        /// <param name="readComboBox"></param>
        //        /// <param name="startCount"></param>
        //        /// <returns></returns>
        //        public string[] ComboBoxItems2StrArray(ComboBox readComboBox, int startCount)
        //        {
        //            string TAG = "[ComboBoxItems2StrArray]";// + fileName;
        //            string dbMsg = TAG;
        //            string[] PLArray = { };       //new string[1]ではnullが一つ入る
        //            try
        //            {
        //                int ArraySize = readComboBox.Items.Count;
        //                dbMsg += ",Items=" + ArraySize + "件";
        //                if (0 < ArraySize)
        //                {
        //                    PLArray = new string[ArraySize - 1];// { PlaylistComboBox.Items.Contains() };
        //                    for (int i = startCount; i <= ArraySize - startCount; i++)
        //                    {
        //                        dbMsg += "(" + i + ")";
        //                        string addItem = readComboBox.Items[i].ToString();
        //                        dbMsg += addItem;
        //                        PLArray[i - startCount] = addItem;
        //                    }
        //                    dbMsg += ",PLArray=" + PLArray.Length + "件";
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return PLArray;
        //        }

        //        private void PlayListBox_Click(object sender, EventArgs e)
        //        {
        //            string TAG = "[PlayListBox_Click]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "(" + playListBox.SelectedIndex + ")" + playListBox.Text;
        //                plaingItem = playListBox.SelectedValue.ToString();
        //                dbMsg += ";plaingItem=" + plaingItem;
        //                PlayFromPlayList(plaingItem);
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }

        //        }



        //        private void PlayListBox_KeyDown(object sender, KeyEventArgs e)
        //        {
        //            /*		string TAG = "[PlayListBox_KeyDown]";
        //					string dbMsg = TAG;
        //					try {
        //						draglist.SelectionMode = SelectionMode.One;
        //						if (e.KeyCode == Keys.ShiftKey) {
        //							draglist.SelectionMode = SelectionMode.MultiExtended;
        //						}
        //						MyLog(TAG, dbMsg);
        //					} catch (Exception er) {
        //						dbMsg += "<<以降でエラー発生>>" + er.Message;
        //						MyLog(TAG, dbMsg);
        //					}
        //		*/
        //        }

        //        private void PlayListBox_KeyUp(object sender, KeyEventArgs e)
        //        {
        //            string TAG = "[playListBox_KeyUp]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "KeyCode=" + e.KeyCode;
        //                ListBox lb = (ListBox)sender;
        //                ListBox.SelectedObjectCollection SelectedItems = lb.SelectedItems;
        //                if (SelectedItems != null)
        //                {
        //                    dbMsg += "SelectedItems=" + SelectedItems.Count + "件";
        //                    dbMsg += "SelectedIndices=" + lb.SelectedIndices.Count + "件";
        //                    string fullPath = lb.SelectedValue.ToString();
        //                    dbMsg += ";" + fullPath;
        //                    if (e.KeyCode == Keys.Delete)
        //                    {
        //                        foreach (int plIndex in lb.SelectedIndices)
        //                        {
        //                            dbMsg += ";plIndex=" + plIndex;
        //                            DelFromPlayList(PlaylistComboBox.Text, plIndex);
        //                        }

        //                        /*						foreach (PlayListItems sitem in SelectedItems) {
        //													fullPath = sitem.FullPathStr;
        //													dbMsg += "を削除";
        //													plIndex = playListBox.Items.IndexOf(fullPath);
        //												//	plIndex = playListBox.SelectedIndex;             //プレイリスト上のマウス座標から選択すべきアイテムのインデックスを取得
        //													dbMsg += ";plIndex=" + plIndex;
        //													DelFromPlayList(PlaylistComboBox.Text, plIndex);
        //												}*/
        //                    }
        //                }
        //                else
        //                {
        //                    dbMsg += ";FocusedItem無し";
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //                throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
        //            }
        //        }

        //        /*
        //		<M3U／WPL共通＞
        //		¡最大ディレクトリ階層 ：8階層
        //		¡最大フォルダ名／最大ファイル名文字数 ：半角28文字
        //		¡フォルダ名／ファイル名使用可能文字 ：A〜Z（全角／半角）、0〜9（全角／半角）、
        //		_（アンダースコア）、全角漢字（JIS 第2水準まで）、
        //		ひらがな、カタカナ（全角／半角）
        //		¡最大プレイリストファイル数 ：30
        //		¡1プレイリストファイル中の最大ファイル数 ：100

        //		*/
        //        //システムメニュー///////////////////////////////////////////////////////////playList//
        //        private void ReWriteSysMenu()
        //        {
        //            string TAG = "[ReWriteSysMenu]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                IntPtr hSysMenu = GetSystemMenu(this.Handle, false);

        //                MENUITEMINFO item1 = new MENUITEMINFO();                //メニュー要素はMENUITEMINFO構造体に値を設定
        //                item1.cbSize = (uint)Marshal.SizeOf(item1);             //構造体のサイズ
        //                item1.fMask = MIIM_FTYPE;                               //この構造体で設定するメンバを指定
        //                item1.fType = MFT_SEPARATOR;                            //
        //                InsertMenuItem(hSysMenu, 5, true, ref item1);           //①メニューのハンドル②識別子または位置③uItem パラメータの意味④メニュー項目の情報

        //                MENUITEMINFO item20 = new MENUITEMINFO();
        //                item20.cbSize = (uint)Marshal.SizeOf(item20);
        //                item20.fMask = MIIM_STRING | MIIM_ID;
        //                item20.wID = MENU_ID_20;                                 //メニュー要素を識別するためのID
        //                item20.dwTypeData = "ファイルブラウザ";
        //                InsertMenuItem(hSysMenu, 6, true, ref item20);

        //                MENUITEMINFO item60 = new MENUITEMINFO();
        //                item60.cbSize = (uint)Marshal.SizeOf(item60);
        //                item60.fMask = MIIM_STRING | MIIM_ID;
        //                item60.wID = MENU_ID_60;
        //                item60.dwTypeData = "プレイリスト";
        //                InsertMenuItem(hSysMenu, 7, true, ref item60);

        //                MENUITEMINFO item99 = new MENUITEMINFO();
        //                item99.cbSize = (uint)Marshal.SizeOf(item60);
        //                item99.fMask = MIIM_STRING | MIIM_ID;
        //                item99.wID = MENU_ID_99;
        //                item99.dwTypeData = "バージョン情報";
        //                InsertMenuItem(hSysMenu, 8, true, ref item99);

        //                /*			IntPtr hMenu = GetSystemMenu(this.Handle, 0);           // タイトルバーのコンテキストメニューを取得
        //							AppendMenu(hMenu, MF_SEPARATOR, 0, string.Empty);           // セパレータとメニューを追加
        //							AppendMenu(hMenu, MF_STRING, MF_BYCOMMAND, "プレイリスト");
        //							AppendMenu(hMenu, MF_SEPARATOR, 0, string.Empty);           // セパレータとメニューを追加
        //							AppendMenu(hMenu, MF_STRING, MF_BYCOMMAND, "バージョン情報");
        //							*/
        //                IsWriteSysMenu = true;    //システムメニューを追記した
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// システムメニューの動作
        //        /// </summary>
        //        /// <param name="m"></param>
        //        protected override void WndProc(ref Message m)
        //        {
        //            string TAG = "[WndProc]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "Message=" + m.ToString();
        //                base.WndProc(ref m);
        //                dbMsg += ",m.Msg=" + m.Msg.ToString();
        //                if (m.Msg == WM_SYSCOMMAND)
        //                {
        //                    uint menuid = (uint)(m.WParam.ToInt32() & 0xffff);
        //                    dbMsg += ",menuid=" + menuid.ToString();

        //                    switch (menuid)
        //                    {
        //                        case MENU_ID_20:
        //                            dbMsg += ",FileBrowserSplitContainer=" + baseSplitContainer.Panel1Collapsed;
        //                            if (baseSplitContainer.Panel1Collapsed)
        //                            {
        //                                baseSplitContainer.Panel1Collapsed = false;//ファイルブラウザを開く
        //                            }
        //                            else
        //                            {
        //                                baseSplitContainer.Panel1Collapsed = true;//ファイルブラウザを閉じる
        //                            }
        //                            break;
        //                        case MENU_ID_60:
        //                            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;              //コンテキストメニューを表示する座標
        //                            dbMsg += "(" + p.X + "," + p.Y + ")";
        //                            if (PlaylistComboBox.Text.Contains(".m3u"))
        //                            {
        //                                上の階層をリストアップLCToolStripMenuItem.Visible = false;
        //                                読めないファイルを削除LCToolStripMenuItem.Visible = true;
        //                            }
        //                            else
        //                            {
        //                                上の階層をリストアップLCToolStripMenuItem.Visible = true;
        //                                読めないファイルを削除LCToolStripMenuItem.Visible = false;
        //                            }
        //                            dbMsg += ",viewSplitContainer=" + viewSplitContainer.Panel1Collapsed;
        //                            if (viewSplitContainer.Panel1Collapsed)
        //                            {                   //既に開いていたら
        //                                プレイリスト表示LCToolStripMenuItem.Text = "プレイリスト表示";
        //                            }
        //                            else
        //                            {
        //                                プレイリスト表示LCToolStripMenuItem.Text = "プレイリストを閉じる";
        //                            }
        //                            this.ListContextMenuStrip.Show(p.X, p.Y);             //指定した画面上の座標位置にコンテキストメニューを表示する
        //                            break;
        //                        case MENU_ID_99:
        //                            string ver = Application.ProductVersion;
        //                            //	System.Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        //                            //		System.Diagnostics.FileVersionInfo ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);         //1.2.0.4"
        //                            dbMsg += ",ver=" + ver.ToString();      //ver.ProductVersion.ToString()
        //                            MessageBox.Show(ver.ToString(),                 //H:\develop\dnet\M3UPlayerilebrowser\Properties\AssemblyInfo.cs の[assembly: AssemblyFileVersion( "1.2.0.4" )]
        //                            "バージョン情報", MessageBoxButtons.OK,
        //                            MessageBoxIcon.Information);
        //                            break;
        //                    }
        //                }
        //                //					MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        private void Form1_KeyUp(object sender, KeyEventArgs e)
        //        {
        //            string TAG = "[Form1_KeyUp]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += ", e.KeyCode=" + e.KeyCode;
        //                switch (e.KeyCode)
        //                {
        //                    case Keys.Escape:
        //                        this.FormBorderStyle = FormBorderStyle.Sizable;                         //通常サイズに戻す
        //                        this.WindowState = FormWindowState.Normal;
        //                        break;
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        //設定///////////////////////////////////////////////////////////システムメニュー//
        //        //		https://dobon.net/vb/dotnet/programing/storeappsettings.html
        //        /// <summary>
        //        /// プリファレンスの変更イベント
        //        /// </summary>
        //        /// <param name="sender"></param>
        //        /// <param name="e"></param>
        //        private void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        //        {
        //            string TAG = "[Default_SettingChanging]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "変更=" + e.SettingName;
        //                dbMsg += " を" + e.NewValue.ToString() + "に";
        //                //変更しようとしている設定が"Text"のとき
        //                if (e.SettingName == "CurrentFile")
        //                {
        //                    //設定しようとしている値を取得
        //                    string str = e.NewValue.ToString();
        //                    if (str.Length > 10)
        //                    {
        //                        //変更をキャンセルする
        //                        e.Cancel = true;
        //                    }
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// config書込み
        //        /// </summary>
        //        private void WriteSetting()
        //        {
        //            string TAG = "[WriteSetting]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "configFileName=" + configFileName;
        //                System.IO.FileInfo fi = new System.IO.FileInfo(configFileName);
        //                /*	if (fi.Exists) {
        //						fi.Delete();
        //					}
        //	*/
        //                dbMsg += " , CurrentFile=" + appSettings.CurrentFile;
        //                if (appSettings.CurrentFile != "")
        //                {
        //                    fi = new System.IO.FileInfo(appSettings.CurrentFile);   //変更元のFileInfoのオブジェクトを作成します。 @"C:\files1\sample1.txt" 
        //                    FileAttributes Attributes = fi.Attributes;
        //                    if (Attributes.ToString().Contains("Directory"))
        //                    {
        //                        appSettings.CurrentFile = "";
        //                    }
        //                }
        //                dbMsg += " , CurrentList=" + appSettings.CurrentList;
        //                dbMsg += " , PlaylistComboBox=" + PlaylistComboBox.Items.Count + "件";
        //                dbMsg += " , PlayLists=" + appSettings.PlayLists.Length + "件";
        //                if (appSettings.PlayLists.Length < PlaylistComboBox.Items.Count)
        //                {
        //                    List<string> PlArray = new List<string>();
        //                    foreach (string fileName in PlaylistComboBox.Items)
        //                    {
        //                        dbMsg += "(" + PlArray.Count + ")" + fileName;
        //                        if (fileName.Contains(".m3u"))
        //                        {
        //                            PlArray.Add(fileName);
        //                        }
        //                    }
        //                    appSettings.PlayLists = PlArray.ToArray();
        //                }
        //                dbMsg += ">>" + appSettings.PlayLists.Length + "件";

        //                /*	if (appSettings.PlayLists != null) {
        //						dbMsg += " , PlayLists=" + appSettings.PlayLists.Length + "件";
        //						dbMsg += " =" + appSettings.PlayLists.ToString();
        //					}*/
        //                dbMsg += " , FormBorderStyle=" + this.FormBorderStyle;
        //                dbMsg += " , FormBorderStyle=" + this.WindowState;
        //                if (this.FormBorderStyle == FormBorderStyle.None || this.WindowState == FormWindowState.Maximized)
        //                {                //フルスクリーン
        //                    appSettings.IsFullScreene = true;
        //                }
        //                else
        //                {            //	this.FormBorderStyle = FormBorderStyle.Sizable; //this.WindowState = FormWindowState.Normal;              //通常サイズに戻す
        //                    appSettings.IsFullScreene = false;
        //                }
        //                appSettings.FormWidth = this.Width;
        //                dbMsg += " ,[FormWidth=" + appSettings.FormWidth;
        //                appSettings.FormHight = this.Height;
        //                dbMsg += "×" + appSettings.FormHight + "]";
        //                dbMsg += " , baseSplitContainer.Panel1Collapsed=" + baseSplitContainer.Panel1Collapsed; //ファイルブラウザを開くfalse	閉じるtrue
        //                appSettings.FileBrowserVisible = !baseSplitContainer.Panel1Collapsed;
        //                dbMsg += " >FileBrowserVisible>" + appSettings.FileBrowserVisible; //ファイルブラウザを開くfalse	閉じるtrue
        //                dbMsg += ",viewSplitContainer.Panel1Collapsed=" + viewSplitContainer.Panel1Collapsed;
        //                appSettings.PlayListVisible = !viewSplitContainer.Panel1Collapsed;        //リストエリアを開くファイルブラウザを開くfalse	閉じる閉じるtrue
        //                dbMsg += ">playListVisible>" + appSettings.PlayListVisible;
        //                dbMsg += " , SoundVolume=" + appSettings.SoundVolume;

        //                //＜XMLファイルに書き込む＞
        //                System.Xml.Serialization.XmlSerializer serializer1 = new System.Xml.Serialization.XmlSerializer(typeof(Settings));       //XmlSerializerオブジェクトを作成
        //                                                                                                                                         //書き込むオブジェクトの型を指定する
        //                System.IO.StreamWriter sw = new System.IO.StreamWriter(configFileName, false, new System.Text.UTF8Encoding(false));     //ファイルを開く（UTF-8 BOM無し）
        //                serializer1.Serialize(sw, appSettings);                                                                                 //シリアル化し、XMLファイルに保存する
        //                sw.Close();                                                                                                             //閉じる

        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        //        /// <summary>
        //        /// config読込み
        //        /// </summary>
        //        private void ReadSetting()
        //        {
        //            string TAG = "[ReadSetting]";
        //            string dbMsg = TAG;
        //            try
        //            {
        //                dbMsg += "configFileName=" + configFileName;
        //                if (File.Exists(configFileName))
        //                {
        //                    //＜XMLファイルから読み込む＞
        //                    System.Xml.Serialization.XmlSerializer serializer2 = new System.Xml.Serialization.XmlSerializer(typeof(Settings));   //XmlSerializerオブジェクトの作成
        //                    System.IO.StreamReader sr = new System.IO.StreamReader(configFileName, new System.Text.UTF8Encoding(false));        //ファイルを開く
        //                    appSettings = (Settings)serializer2.Deserialize(sr);                                                                    //XMLファイルから読み込み、逆シリアル化する
        //                    sr.Close();                                                                                                         //閉じる

        //                    playerUrl = System.Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);       //=C:\Users\博臣\Videos 
        //                    System.IO.FileInfo fi = new System.IO.FileInfo(playerUrl);   //変更元のFileInfoのオブジェクトを作成します。 @"C:\files1\sample1.txt" 
        //                                                                                 //			playerUrl = fi.DirectoryName;
        //                                                                                 //			fi = new System.IO.FileInfo(playerUrl);   //変更元のFileInfoのオブジェクトを作成します。 @"C:\files1\sample1.txt" 
        //                    dbMsg += " , playerUrl=" + playerUrl;
        //                    dbMsg += " , CurrentFile=" + appSettings.CurrentFile;
        //                    if (appSettings.CurrentFile != "")
        //                    {
        //                        playerUrl = appSettings.CurrentFile.ToString();
        //                        fi = new System.IO.FileInfo(playerUrl);   //変更元のFileInfoのオブジェクトを作成します。 @"C:\files1\sample1.txt" 
        //                        FileAttributes Attributes = fi.Attributes;
        //                        if (Attributes.ToString().Contains("Directory"))
        //                        {
        //                        }
        //                        else
        //                        {
        //                            playerUrl = appSettings.CurrentFile.ToString();
        //                        }
        //                    }
        //                    string passNameStr = fi.DirectoryName;
        //                    dbMsg += " , Attributes=" + fi.Attributes.ToString();
        //                    if (fi.Attributes.ToString().Contains("Directory"))
        //                    {
        //                        passNameStr = playerUrl;
        //                    }
        //                    dbMsg += " , passNameStr=" + passNameStr;
        //                    PlaylistComboBox.Items.Add(passNameStr);                            //最初のアイテムは選択ファイルを強制的に書き込む
        //                                                                                        //	ComboBoxAddItems(PlaylistComboBox, passNameStr);
        //                                                                                        //			PlaylistComboBox.Items.Add(passNameStr);                              //前回読みファイルのフォルダをデフォルトに
        //                    FileListVewDrow(passNameStr);
        //                    fileNameLabel.Text = playerUrl;
        //                    passNameLabel.Text = passNameStr;// fileNameLabel.Text.Replace(fi.DirectoryName, "");

        //                    dbMsg += " , PlayLists=" + appSettings.PlayLists.Length + "件";
        //                    //	if (0 < appSettings.PlayLists.Length) {
        //                    foreach (string fileName in appSettings.PlayLists)
        //                    {
        //                        dbMsg += "(" + PlaylistComboBox.Items.Count + ")" + fileName;
        //                        AddPlaylistComboBox(fileName);
        //                        /*			string feName = checKLocalFile(fileName);
        //									//			if (fileName.Contains(@":\\") || fileName.Contains(@":\\\")) {
        //									//			} else {
        //									int alradyIndex = PlaylistComboBox.Items.IndexOf(feName);
        //								dbMsg += ".alradyIndex=" + alradyIndex;
        //								if (alradyIndex < 0) {                                              //同名アイテムが無ければ
        //										PlaylistComboBox.Items.Add(feName);                           //追記
        //									}
        //									//			}*/
        //                    }
        //                    //	}
        //                    dbMsg += ">PlaylistComboBox=>" + PlaylistComboBox.Items[0].ToString();
        //                    dbMsg += " , CurrentList=" + appSettings.CurrentList;
        //                    if (appSettings.CurrentList != "")
        //                    {
        //                        nowLPlayList = appSettings.CurrentList.ToString();
        //                        int lcIndex = PlaylistComboBox.Items.IndexOf(nowLPlayList);
        //                        dbMsg += ">lcIndex=" + lcIndex;
        //                        if (-1 < lcIndex)
        //                        {
        //                            PlaylistComboBox.SelectedIndex = lcIndex;
        //                        }
        //                        else
        //                        {
        //                            PlaylistComboBox.SelectedIndex = 0;
        //                        }
        //                        if (appSettings.CurrentList != "")
        //                        {                        //プレイリスト
        //                            viewSplitContainer.Panel1Collapsed = false;
        //                        }
        //                        else
        //                        {
        //                            viewSplitContainer.Panel1Collapsed = true;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        PlaylistComboBox.SelectedIndex = 0;
        //                    }
        //                    PlaylistComboBox.Focus();

        //                    dbMsg += " , FormWidth=" + appSettings.FormWidth;
        //                    if (-1 < appSettings.FormWidth)
        //                    {
        //                        this.Width = appSettings.FormWidth;
        //                    }
        //                    dbMsg += " , FormHight=" + appSettings.FormHight;
        //                    if (-1 < appSettings.FormHight)
        //                    {
        //                        this.Height = appSettings.FormHight;
        //                    }
        //                    dbMsg += " , IsFullScreene=" + appSettings.IsFullScreene;
        //                    if (appSettings.IsFullScreene)
        //                    {                           //フルスクリーンにする
        //                        this.FormBorderStyle = FormBorderStyle.None; //タイトルバーが消されて元サイズに戻せなくなる
        //                        this.WindowState = FormWindowState.Maximized;
        //                    }
        //                    else
        //                    {
        //                        this.FormBorderStyle = FormBorderStyle.Sizable;                         //通常サイズに戻す
        //                        this.WindowState = FormWindowState.Normal;
        //                    }

        //                    dbMsg += " , FileBrowserVisible=" + appSettings.FileBrowserVisible;
        //                    if (appSettings.FileBrowserVisible)
        //                    {
        //                        baseSplitContainer.Panel1Collapsed = false;//ファイルブラウザを開く
        //                    }
        //                    else
        //                    {
        //                        baseSplitContainer.Panel1Collapsed = true;//ファイルブラウザを閉じる
        //                    }

        //                    dbMsg += ",playListVisible=" + appSettings.PlayListVisible;
        //                    if (appSettings.PlayListVisible)
        //                    {
        //                        viewSplitContainer.Panel1Collapsed = false;//リストエリアを開く
        //                    }
        //                    else
        //                    {
        //                        viewSplitContainer.Panel1Collapsed = true;//リストエリアを閉じる
        //                    }

        //                    dbMsg += " , SoundVolume=" + appSettings.SoundVolume;
        //                    if (-1 < appSettings.SoundVolume)
        //                    {
        //                    }


        //                }
        //                else
        //                {
        //                    appSettings = new Settings();
        //                }
        //                MyLog(TAG, dbMsg);
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }


        //        /// <summary>
        //        /// 設定の管理クラス
        //        /// </summary>
        //        public class Settings
        //        {
        //            public string currentFile;                          //再生していたファイル
        //            public string currentList;                          //再生していたリスト
        //            public string[] playLists;                          //利用可能なプレイリスト
        //            public bool isFullScreene = false;                  //フルスクリーン表示
        //            public int formWidth = 1560;                        //表示幅
        //            public int formHight = 750;                         //表示高さ
        //            public bool fileBrowserVisible = true;              //ファイルブラウザ表示
        //            public bool playListVisible = true;                 //プレイリスト表示
        //            public long soundVolume = 50;                       //音量設定　0 to 100.


        //            public string CurrentFile
        //            {
        //                get { return currentFile; }
        //                set { currentFile = value; }
        //            }
        //            public string CurrentList
        //            {
        //                get { return currentList; }
        //                set { currentList = value; }
        //            }
        //            public string[] PlayLists
        //            {
        //                get { return playLists; }
        //                set { playLists = value; }
        //            }

        //            public int IndexOfPlayLists(string word)
        //            {
        //                string TAG = "[IndexOfPlayLists]";
        //                string dbMsg = TAG;
        //                int index = -1;
        //                try
        //                {
        //                    for (int i = 0; i < playLists.Length; ++i)
        //                    {
        //                        dbMsg += "(" + PlayLists.Count() + ")" + playLists[i];
        //                        if (playLists[i].Equals(word))
        //                        {
        //                            index = i;
        //                        }
        //                    }
        //                    Console.WriteLine(dbMsg);
        //                }
        //                catch (Exception er)
        //                {
        //                    dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                    Console.WriteLine(dbMsg);
        //                }
        //                return index;
        //            }

        //            public Settings()
        //            {
        //                currentFile = @"C:\Users";
        //                playLists = new string[1] { "" };
        //            }

        //            public bool IsFullScreene
        //            {
        //                get { return isFullScreene; }
        //                set { isFullScreene = value; }
        //            }

        //            public int FormWidth
        //            {
        //                get { return formWidth; }
        //                set { formWidth = value; }
        //            }

        //            public int FormHight
        //            {
        //                get { return formHight; }
        //                set { formHight = value; }
        //            }

        //            public bool FileBrowserVisible
        //            {
        //                get { return fileBrowserVisible; }
        //                set { fileBrowserVisible = value; }
        //            }

        //            public bool PlayListVisible
        //            {
        //                get { return playListVisible; }
        //                set { playListVisible = value; }
        //            }

        //            public long SoundVolume
        //            {
        //                get { return soundVolume; }
        //                set { soundVolume = value; }
        //            }

        //        }

        //        //その他///////////////////////////////////////////////////////////設定//
        //        /// <summary>
        //        /// フルパスを示す文字列からコンテンツのタイトルになる文字列を抜き出す
        //        /// </summary>
        //        /// <param name="pathStr">パスを示す文字列</param>
        //        /// <returns>タイトル</returns>
        //        private string Path2titol(string pathStr)
        //        {
        //            string TAG = "[Path2titol]";
        //            string dbMsg = TAG;
        //            string retStr = "";
        //            try
        //            {
        //                dbMsg += "pathStr=" + pathStr;
        //                string[] names = pathStr.Split(Path.DirectorySeparatorChar);           //
        //                retStr = names[names.Length - 1];
        //                dbMsg += ",retStr=" + retStr;
        //                string[] names2 = retStr.Split('.');
        //                retStr = names2[0];
        //                dbMsg += " >>" + retStr;
        //                //		MyLog( dbMsg );
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //            return retStr;
        //        }

        //        private void GetFileListByType(string type)
        //        {
        //            string TAG = "[GetFileListByType]" + type;
        //            string dbMsg = TAG;
        //            try
        //            {
        //                //		MyLog( dbMsg );
        //            }
        //            catch (Exception er)
        //            {
        //                dbMsg += "<<以降でエラー発生>>" + er.Message;
        //                MyLog(TAG, dbMsg);
        //            }
        //        }

        /// <summary>
        /// プロパティ変更通知を行うBindableBase
        /// ；ヘルパなしでRaisePropertyChangedの代りにNotifyPropertyChangedを使う
        /// </summary>

        //public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        void RequeryCommands() {
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            //throw new NotImplementedException();
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// プロパティ変更通知を行うBindableBase
        /// ；ヘルパなしでRaisePropertyChangedの代りにNotifyPropertyChangedを使う
        /// </summary>

        //    public event PropertyChangedEventHandler PropertyChanged;
        //private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
        //    if (PropertyChanged != null) {
        //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}


        //デバッグツール///////////////////////////////////////////////////////////その他//
        Boolean debug_now = true;

        public static void MyLog(string TAG, string dbMsg) {
            dbMsg = "[MainViewModel]" + dbMsg;
            //dbMsg = "[" + MethodBase.GetCurrentMethod().Name + "]" + dbMsg;
            CS_Util Util = new CS_Util();
            Util.MyLog(TAG, dbMsg);
        }

        public static void MyErrorLog(string TAG, string dbMsg, Exception err) {
            dbMsg = "[MainViewModel]" + dbMsg;
            CS_Util Util = new CS_Util();
            Util.MyErrorLog(TAG, dbMsg, err);
        }

        public MessageBoxResult MessageShowWPF(String titolStr, String msgStr,
                                                                        MessageBoxButton buttns,
                                                                        MessageBoxImage icon
                                                                        ) {
            CS_Util Util = new CS_Util();
            return Util.MessageShowWPF(msgStr, titolStr, buttns, icon,MyView);
        }


        //http://www.usefullcode.net/2016/03/index.html

        //↓属性設定が無いとエラーになります
        /// <summary>WebView2に読み込ませるためのJsで実行する関数を保持させたクラス</summary>
        [ClassInterface(ClassInterfaceType.AutoDual)]
        [ComVisible(true)]
        public class JsToCs {
            public void MessageShow(string strText) {
                MessageBox.Show("Jsからの呼び出し>" + strText);
            }
        }
    }
}
