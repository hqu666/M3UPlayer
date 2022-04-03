using System;
using System.Collections.Generic;
using System.IO;

//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Calendar.v3;
//using Google.Apis.Drive.v3;

namespace M3UPlayer {
	class Constant {
		public static bool debugNow = true;
		public static bool errorCheckNow = true;
		public static int MyFontSize = 12;

        public static string WebStratUrl = "https://www.yahoo.co.jp/?fr=top_ga1_ext1_bookmark";         //webViewのデフォルト表示ページ
		public static string CurrentFolder = "";                    //現在の対象フォルダ
		public static string RootFolderURL;
		public static double SoundValue = 0.5;
		public static string ForwardCBComboSelected = "120";
		
		/// <summary>
		/// webに書き込むプレイヤー名
		/// </summary>
		public static string PlayerName = "wiPlayer";
		/// <summary>
		/// webソースの書き出し先
		/// </summary>
		public static string currentDirectory = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "souce.html";


		////認証情報/API キー
		//// https://console.cloud.google.com/apis/dashboard?authuser=3&project=kyokuto4
		//public static string ApplicationName = "kyokuto4";                                        //	アプリケーション名
		//public static string APIKey = "AIzaSyAnJ-QXa9dqQr644u7jn_3-sxSr3XL_z60";
		////OAuth 2.0 クライアント ID
		//public static string CliantId = "912719822179-n9hvcs7tr9pqgn8mns7pdl5njo54gpe1.apps.googleusercontent.com";    //クライアント ID
		//public static string CliantSeacret = "aGVZ_mfTKJq8WFf5spDOOiHi";    //クライアント シークレット
		//      public static string DriveId;
		public static string GoogleLogInPage =@"https://accounts.google.com/signin/v2/identifier?service=mail&flowName=GlifWebSignIn&flowEntry=ServiceLogin";
		public static string GoogleAcountMSG = "";			//"YourGoogleAcount@gmail.com";

  //      public static UserCredential MyCalendarCredential;
		//public static CalendarService MyCalendarService;
		//public static UserCredential MyDriveCredential;
		//public static DriveService MyDriveService;

		//public static IList<Google.Apis.Calendar.v3.Data.Event> GCalenderEvent;//カレンダーの予定
		//public static Google.Apis.Calendar.v3.Data.Event eventItem;
		public static string CalenderSummary = "abcbdffghaiklnm@gmail.com";   //Googleアカウント
		public static string CalenderOtherView = "https://calendar.google.com/calendar/r/";            //週別/日別への切替

        public static string HierarchyFileName = "MyHierarchy";            //アイテムの階層管理スプレッドシート
        public static string HierarchyFileID = "";                           //アイテムの階層管理スプレッドシート
        public static string RootFolderName = "ProductionSchedule";            //保存先サーバのルートフォルダ
        public static string RootFolderID = "";
		public static string TopFolderName = "ProductionSchedule";                                        //	最上位フォルダ KSクラウド
		public static string TopFolderID = "";
		public static string MakeFolderName = null;         //作成するファイルの格納フォルダ
		public static String parentFolderId;
		public static string LocalPass = "";            //送信元PCフォルダ

		//public static IList<Google.Apis.Drive.v3.Data.File> GDriveFiles;
		//public static IDictionary<string, Google.Apis.Drive.v3.Data.File> GDriveFolders;
		//public static IList<Google.Apis.Drive.v3.Data.File> GDriveFolderMembers;
		//public static IList<Google.Apis.Drive.v3.Data.File> GDriveSelectedFiles;
		//public static string GoogleDriveMime_Folder = "application/vnd.google-apps.folder";

		///// <summary>
		///// GoogleのIDで定義されたEventColor
		///// </summary>
		//public struct GoogleEventColor {
		//	public string id;
		//	public string name;
		//	public System.Windows.Media.Color rgb;  //= System.Windows.Media.Color.FromRgb(0x00, 0xFF, 0x00);

		//	public GoogleEventColor(string id, string name, System.Windows.Media.Color rgb)
		//	{
		//		this.id = id;
		//		this.name = name;
		//		this.rgb = rgb;
		//	}
		//}
		//public static IList<GoogleEventColor> googleEventColor;

		///// <summary>
		///// 受注No　:　GoogleEventに無い追加項目
		///// </summary>
		//public static string orderNumber = "";
		///// <summary>
		///// 管理番号　:　GoogleEventに無い追加項目
		///// </summary>
		//public static string managementNumber = "";
		///// <summary>
		///// 得意先　:　GoogleEventに無い追加項目
		///// </summary>
		//public static string customerName =""; 

		/// <summary>
		/// PCのファイル管理
		/// </summary>
		public struct LocalFile {
			public string fullPass;
			public string name;
			public string parent;
			public bool isFolder;

			public LocalFile(string fullPass, string name, string parent, bool isFolder)
			{
				this.fullPass = fullPass;
				this.name = name;
				this.parent = parent;
				this.isFolder = isFolder;
			}
		}
		public static IList<LocalFile> sendFiles = null;             //送信元PCのファイルリスト

		public static IList<string> selectFiles = null;             //送信元PCのファイルリスト
	}
}

/*
 このアプリに必要なパッケージ
 パッケージマネージャコンソールから
Install-Package Google.Apis.Drive.v3
Install-Package Google.Apis.Calendar.v3 
 */
