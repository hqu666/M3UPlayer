using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace M3UPlayer
{
	class CS_Util {

		/*
	   * サブディレクトリも含め全てのファイル名を取得する関数
	   * 
	   * 引用元：【C#】ドライブ直下からのファイルリスト取得について
	   *        https://qiita.com/OneK/items/8b0d02817a9f2a2fbeb0
	   */
		public List<string> GetAllFiles(String DirPath) {
			string TAG = "GetAllFiles";
			string dbMsg = "[CS_Util]";
			List<String> lstStr = new List<String>();    // 取得したファイル名を格納するためのリスト
			String[] strBuff;   // ファイル名とディレクトリ名取得用

			try {
				// ファイル名取得
				strBuff = Directory.GetFiles(DirPath, "*.*");        // 探索範囲がルートフォルダで時間が掛かるため、テキスト形式のファイルのみ探索
				foreach (String file in strBuff) {
					lstStr.Add(file);
					dbMsg += "\r\n[" + lstStr.Count + "]" + file;
				}
				// ディレクトリ名の取得
				strBuff = Directory.GetDirectories(DirPath);
				foreach (String directory in strBuff) {
					List<String> lstBuff = GetAllFiles(directory);    // 取得したディレクトリ名を引数にして再帰
					lstBuff.ForEach(delegate (String str) {
						lstStr.Add(str);
						dbMsg += "\r\n[" + lstStr.Count + "]" + str;
					});
				}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
			// 取得したファイル名リストを呼び出し元に返す
			return lstStr;
			//System.ArgumentException: String cannot be of zero length. (Parameter 'oldValue')at System.String.Replace(String oldValue, String newValue)
		}

		/// <summary>
		/// カラーコードの文字列をSystem.Windows.MediaのColorに変換する
		/// </summary>
		public Color ColorStr2Color(string colorcode)
		{
			string TAG = "IsForegroundWhite";
			string dbMsg = "[CS_Util]";
			Color col= Color.FromArgb(255, (byte)0, (byte)0, (byte)0);
			try {
				int redInt = int.Parse(colorcode.Substring(1, 2), NumberStyles.HexNumber);
				int greenInt = int.Parse(colorcode.Substring(3, 2), NumberStyles.HexNumber);
				int blueInt = int.Parse(colorcode.Substring(5, 2), NumberStyles.HexNumber);
				col = Color.FromArgb(255, (byte)redInt, (byte)greenInt, (byte)blueInt);
				if (colorcode.Length == 6) {
					dbMsg += ",r=" + redInt + ",g=" + greenInt + ",b=" + blueInt;
				} else {
					dbMsg += ">ARGB>";
					int alphaInt = int.Parse(colorcode.Substring(1, 2), NumberStyles.HexNumber);
					dbMsg += ",alpha=" + alphaInt;
					redInt = int.Parse(colorcode.Substring(3, 2), NumberStyles.HexNumber);
					greenInt = int.Parse(colorcode.Substring(5, 2), NumberStyles.HexNumber);
					blueInt = int.Parse(colorcode.Substring(7, 2), NumberStyles.HexNumber);
					dbMsg += ",r=" + redInt + ",g=" + greenInt + ",b=" + blueInt;
					col = Color.FromArgb((byte)alphaInt,(byte)redInt, (byte)greenInt, (byte)blueInt);
				}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
			return col;
		}

		/// <summary>
		/// 背景色に応じて文字色が白で良ければTrueを返す
		/// </summary>
		/// <param name="colorcode"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		public bool IsForegroundWhite(string colorcode, int limit = 128)
		{
			string TAG = "IsForegroundWhite";
			string dbMsg = "[CS_Util]";
			bool retBool = false;
			try {
				dbMsg += ",colorcode=" + colorcode;
				if(6<colorcode.Length) {
					if (colorcode.StartsWith("#")) {
						int redInt = int.Parse(colorcode.Substring(1, 2), NumberStyles.HexNumber);
						int greenInt = int.Parse(colorcode.Substring(3, 2), NumberStyles.HexNumber);
						int blueInt = int.Parse(colorcode.Substring(5, 2), NumberStyles.HexNumber);
						if (colorcode.Length == 6) {
							dbMsg += ",r=" + redInt + ",g=" + greenInt + ",b=" + blueInt;
						} else {
							dbMsg += ">ARGB>";
							int alphaInt = int.Parse(colorcode.Substring(1, 2), NumberStyles.HexNumber);
							dbMsg += ",alpha=" + alphaInt;
							redInt = int.Parse(colorcode.Substring(3, 2), NumberStyles.HexNumber);
							greenInt = int.Parse(colorcode.Substring(5, 2), NumberStyles.HexNumber);
							blueInt = int.Parse(colorcode.Substring(7, 2), NumberStyles.HexNumber);
							dbMsg += ",r=" + redInt + ",g=" + greenInt + ",b=" + blueInt;
						}
						int Judgment = ((redInt * 299) + (greenInt * 587) + (blueInt * 114)) / 1000;
						dbMsg += ",Judg=" + Judgment;
						dbMsg += " : " + limit;
						if (Judgment < limit) {
							retBool = true;
						}
						dbMsg += ">>" + retBool;
					} else {
						dbMsg += ">>カラーコードではない";
					}
	 			}else{
					dbMsg += ">>文字数不足";
				}
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyErrorLog(TAG, dbMsg, er);
			}
			return retBool;
		}

		/// <summary>
		/// デバッグログ
		/// 出荷時は　debugNow　をfalseに
		/// </summary>
		/// <param name="TAG"></param>
		/// <param name="dbMsg"></param>
		public void MyLog(String TAG, String dbMsg)
		{
#if DEBUG
			Debug.Print(TAG + " : " + dbMsg);
			//Constant.debugNow = true;
#endif
		}

		public void MyErrorLog(String TAG, String dbMsg, Exception err)
		{
			Debug.Print(TAG + " : " + dbMsg + "でエラー発生;" + err);
        }

        public MessageBoxResult MessageShowWPF(String msgStr,
												String titolStr = null,
												MessageBoxButton buttns = MessageBoxButton.OK,
												MessageBoxImage icon = MessageBoxImage.None,
												Window window = null)
		{
			String TAG = "MessageShowWPF";
			String dbMsg = "開始";
			MessageBoxResult result = 0;
			try {
				dbMsg = "titolStr=" + titolStr;
				dbMsg += "mggStr=" + msgStr;
				//メッセージボックスを表示する		https://docs.microsoft.com/ja-jp/dotnet/api/system.windows.messagebox?view=netcore-3.1
				if (titolStr == null) {
					result = MessageBox.Show(window,msgStr);
				} else if (icon == MessageBoxImage.None) {
					result = MessageBox.Show(window,msgStr, titolStr, buttns );
				} else {
					result = MessageBox.Show(window,msgStr, titolStr, buttns, icon);
				}
				dbMsg += ",result=" + result;
				MyLog(TAG, dbMsg);
			} catch (Exception er) {
				MyLog(TAG, dbMsg + "で" + er.ToString());
			}
			return result;
		}

		/// <summary>
		/// ダイアログ表示
		/// </summary>
		/// <param name="titolStr">タイトル</param>
		/// <param name="msgStr">メッセージ</param>
		/// 		/// http://furuya02.hatenablog.com/entry/20140426/1398477869
		/// <param name="titolStr"></param>
		/// <param name="msgStr"></param>
		/// <param name="buttns">MessageBoxButtons.OKCancel</param>
		/// <param name="icon">MessageBoxIcon.Exclamation</param>
		/// <param name="defaultButton">MessageBoxDefaultButton.Button2</param>
		/// <returns></returns>
		//public DialogResult MessageShow(String titolStr, String msgStr, MessageBoxButtons buttns, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		//{
		//	String TAG = "MessageShow";
		//	String dbMsg = "開始";
		//	DialogResult result = 0;
		//	try
		//	{
		//		dbMsg = "titolStr=" + titolStr;
		//		dbMsg += "mggStr=" + msgStr;

		//		//メッセージボックスを表示する
		//		result = MessageBox.Show(msgStr, titolStr, buttns, icon, defaultButton);
		//		dbMsg += ",result=" + result;
		//		MyLog(TAG, dbMsg);
		//	}
		//	catch (Exception er)
		//	{
		//		MyLog(TAG, dbMsg + "で" + er.ToString());
		//	}
		//	return result;
		//}
	}
}
