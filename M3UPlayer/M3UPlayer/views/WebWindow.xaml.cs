using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using Google.Apis.Drive.v3.Data;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

using M3UPlayer.ViewModels;
using M3UPlayer.Properties;

namespace M3UPlayer.Views
{
    /// <summary>
    /// W_1.xaml の相互作用ロジック
    /// </summary>
    public partial class WebWindow : Window
    {

        public ViewModels.WsbViewModel VM;

        public static RoutedCommand InjectScriptCommand = new RoutedCommand();
        public static RoutedCommand NavigateWithWebResourceRequestCommand = new RoutedCommand();
        public static RoutedCommand DOMContentLoadedCommand = new RoutedCommand();
        public static RoutedCommand GetCookiesCommand = new RoutedCommand();
        public static RoutedCommand AddOrUpdateCookieCommand = new RoutedCommand();
        public static RoutedCommand DeleteCookiesCommand = new RoutedCommand();
        public static RoutedCommand DeleteAllCookiesCommand = new RoutedCommand();
        /// <summary>
        /// 現在遷移中
        /// </summary>
        bool _isNavigating = false;

        string urlStr = "";
        bool _isShowHttpsAlart = false;

        public WebWindow()
        {
            InitializeComponent();
            InitializeAsync();
            webView.NavigationStarting += EnsureHttps;
            if (VM ==null)
            {
                VM = new ViewModels.WsbViewModel();
            }
            this.DataContext = VM;
            this.Loaded += WebViewloaded;
        }

        /// <summary>
        /// 読込み終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebViewloaded(object sender, RoutedEventArgs e)
        {
            //ViewとViewModelの紐付け
            VM.MyView = this;
            //	((ViewModels.MainViewModel)this.DataContext).MyView =this;
            // ウィンドウのサイズを復元
            RecoverWindowBounds();

        }

        /// <summary>
        /// ウィンドウの位置・サイズを復元します。
        /// </summary>
        void RecoverWindowBounds()
        {
            string TAG = "RecoverWindowBounds";
            string dbMsg = "";
            try
            {
                var settings = Settings.Default;
                dbMsg += "(" + settings.WsbViewLeft + "," + settings.WsbViewTop + ")";
                // 左
                if (settings.WsbViewLeft >= 0 &&
                    (settings.WsbViewLeft + settings.WindowWidth) < SystemParameters.VirtualScreenWidth)
                {
                    Left = settings.WsbViewLeft;
                }
                // 上
                if (settings.WsbViewTop >= 0 &&
                    (settings.WsbViewTop + settings.WsbViewHeight) < SystemParameters.VirtualScreenHeight) { Top = settings.WsbViewTop; }
                dbMsg += "[" + settings.WsbViewWidth + "×" + settings.WsbViewHeight + "]" + settings.WsbViewMaximized;
                // 幅
                if (settings.WsbViewWidth > 0 &&
                    settings.WsbViewWidth <= SystemParameters.WorkArea.Width) { Width = settings.WsbViewWidth; }
                // 高さ
                if (settings.WsbViewHeight > 0 &&
                    settings.WsbViewHeight <= SystemParameters.WorkArea.Height) { Height = settings.WsbViewHeight; }
                // 最大化
                if (settings.WsbViewMaximized)
                {
                    // ロード後に最大化
                    Loaded += (o, e) => WindowState = WindowState.Maximized;
                }
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }

        }

        /// <summary>
        /// ウィンドウの位置・サイズを保存します。
        /// </summary>
        void SaveWindowBounds()
        {
            string TAG = "SaveWindowBounds";
            string dbMsg = "";
            try
            {
                dbMsg += "(" + Left + "," + Top + ")[" + Width + "×" + Height + "]" + WindowState;
                var settings = Settings.Default;
                settings.WsbViewMaximized = WindowState == WindowState.Maximized;
                WindowState = WindowState.Normal; // 最大化解除
                settings.WsbViewLeft = Left;
                settings.WsbViewTop = Top;
                settings.WsbViewWidth = Width;
                settings.WsbViewHeight = Height;
                settings.Save();
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // ウィンドウのサイズを保存
            string TAG = "Window_Closing";
            string dbMsg = "";
            try
            {
                SaveWindowBounds();
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 明示的な初期化
        /// </summary>
        async void InitializeAsync()
        {
            string TAG = "InitializeAsync";
            string dbMsg = "";
            try
            {
                await webView.EnsureCoreWebView2Async(null);
                //		webView.CoreWebView2.WebMessageReceived += UpdateAddressBar;
                //ホストからメッセージを印刷するためのハンドラーを登録する web コンテンツに、スクリプトを挿入
                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");
                //ホストに URL をポストする web コンテンツにスクリプトを挿入
                //				await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.addEventListener(\'message\', event => alert(event.data));");
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// 読込んだページのURLにアドレスバーを更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void UpdateAddressBar(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            string TAG = "UpdateAddressBar";
            string dbMsg = "";
            try
            {
                String uri = args.TryGetWebMessageAsString();
                dbMsg += ",uri=" + uri;
                addressBar.Text = uri;
                webView.CoreWebView2.PostWebMessageAsString(uri);
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        private void this_loaded(object sender, RoutedEventArgs e)
        {
            string TAG = "this_loaded";
            string dbMsg = "";
            try
            {
                //			topPanel.Visibility = Visibility.Hidden;
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// アドレスバーパネルの表示制御
        /// </summary>
        private void VisibleTop()
        {
            string TAG = "VisibleTop";
            string dbMsg = "";
            try
            {
                bool doIt = true;
                if (webView == null)
                {
                    dbMsg += ">>webView == null ";
                    doIt = false;
                }
                if (webView.CoreWebView2 != null)
                {
                    dbMsg += ">CoreWebView2 != null";
                    doIt = false;
                }
                dbMsg += ">doIt= " + doIt;
                if (doIt)
                {
                    topPanel.Visibility = Visibility.Visible;
                }
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        /// <summary>
        /// HTTPS 以外のサイトに移動したときにユーザーに通知。
        /// スクリプトが web コンテンツに挿入されるように関数を変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void EnsureHttps(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            string TAG = "EnsureHttps";
            string dbMsg = "";
            try
            {
                VisibleTop();
                String uri = args.Uri;
                dbMsg += ",uri=" + uri;
                if (!uri.StartsWith("https://") && _isShowHttpsAlart)
                {
                    webView.CoreWebView2.ExecuteScriptAsync($"alert('{uri} は安全な遷移先ではありません, https で始るURLをご利用ください')");
                    args.Cancel = true;
                }
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        void NewCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            new MainWindow().Show();
        }

        void CloseCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //	this.Close();
        }

        void BackCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = webView != null && webView.CanGoBack;
        }

        void ForwardCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = webView != null && webView.CanGoForward;
        }

        void RefreshCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = webView != null && webView.CoreWebView2 != null && !_isNavigating;
        }

        void BrowseStopCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = webView != null && webView.CoreWebView2 != null && _isNavigating;
        }

        void CoreWebView2RequiringCmdsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = webView != null && webView.CoreWebView2 != null;
        }

        double ZoomStep()
        {
            if (webView.ZoomFactor < 1)
            {
                return 0.25;
            }
            else if (webView.ZoomFactor < 2)
            {
                return 0.5;
            }
            else
            {
                return 1;
            }
        }

        void IncreaseZoomCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = webView != null;
        }

        void IncreaseZoomCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            webView.ZoomFactor += ZoomStep();
        }

        void DecreaseZoomCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (webView != null) && (webView.ZoomFactor - ZoomStep() > 0.0);
        }

        void DecreaseZoomCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            webView.ZoomFactor -= ZoomStep();
        }

        async void InjectScriptCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //var dialog = new TextInputDialog(
            //    title: "Inject Script",
            //    description: "Enter some JavaScript to be executed in the context of this page.",
            //    defaultInput: "window.getComputedStyle(document.body).backgroundColor");
            //if (dialog.ShowDialog() == true)
            //{
            //    string scriptResult = await webView.ExecuteScriptAsync(dialog.Input.Text);
            //    //MessageBox.Show(this, scriptResult, "Script Result");
            //}
        }

        async void GetCookiesCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //List<CoreWebView2Cookie> cookieList = await webView.CoreWebView2.CookieManager.GetCookiesAsync("https://www.bing.com");
            //StringBuilder cookieResult = new StringBuilder(cookieList.Count + " cookie(s) received from https://www.bing.com\n");
            //for (int i = 0; i < cookieList.Count; ++i) {
            //	CoreWebView2Cookie cookie = webView.CoreWebView2.CookieManager.CreateCookieWithSystemNetCookie(cookieList[i].ToSystemNetCookie());
            //	cookieResult.Append($"\n{cookie.Name} {cookie.Value} {(cookie.IsSession ? "[session cookie]" : cookie.Expires.ToString("G"))}");
            //}
            //MessageBox.Show(this, cookieResult.ToString(), "GetCookiesAsync");
        }

        void AddOrUpdateCookieCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //CoreWebView2Cookie cookie = webView.CoreWebView2.CookieManager.CreateCookie("CookieName", "CookieValue", ".bing.com", "/");
            //webView.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
        }

        void DeleteAllCookiesCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //		webView.CoreWebView2.CookieManager.DeleteAllCookies();
        }

        void DeleteCookiesCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //			webView.CoreWebView2.CookieManager.DeleteCookiesWithDomainAndPath("CookieName", ".bing.com", "/");
        }

        void DOMContentLoadedCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            //webView.CoreWebView2.DOMContentLoaded += (object sender, CoreWebView2DOMContentLoadedEventArgs arg) => {
            //	_ = webView.ExecuteScriptAsync("let " +
            //							  "content=document.createElement(\"h2\");content.style.color=" +
            //							  "'blue';content.textContent= \"This text was added by the " +
            //							  "host app\";document.body.appendChild(content);");
            //};
            webView.NavigateToString(@"<!DOCTYPE html><h1>DOMContentLoaded sample page</h1><h2>The content below will be added after DOM content is loaded </h2>");
        }

        private CoreWebView2Environment _coreWebView2Environment;

        async void NavigateWithWebResourceRequestCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            // Need CoreWebView2Environment
            if (_coreWebView2Environment == null)
            {
                //	_coreWebView2Environment = webView.CoreWebView2.Environment;
            }

            //// Prepare post data as UTF-8 byte array and convert it to stream
            //// as required by the application/x-www-form-urlencoded Content-Type
            //var dialog = new TextInputDialog(
            //    title: "NavigateWithWebResourceRequest",
            //    description: "Specify post data to submit to https://www.w3schools.com/action_page.php.");
            //if (dialog.ShowDialog() == true)
            //{
            //    string postDataString = "input=" + dialog.Input.Text;
            //    UTF8Encoding utfEncoding = new UTF8Encoding();
            //    byte[] postData = utfEncoding.GetBytes(
            //        postDataString);
            //    MemoryStream postDataStream = new MemoryStream(postDataString.Length);
            //    postDataStream.Write(postData, 0, postData.Length);
            //    postDataStream.Seek(0, SeekOrigin.Begin);
            //    //CoreWebView2WebResourceRequest webResourceRequest =
            //    //  _coreWebView2Environment.CreateWebResourceRequest(
            //    //	"https://www.w3schools.com/action_page.php",
            //    //	"POST",
            //    //	postDataStream,
            //    //	"Content-Type: application/x-www-form-urlencoded\r\n");
            //    //webView.CoreWebView2.NavigateWithWebResourceRequest(webResourceRequest);
            //}
        }

        void GoToPageCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = webView != null && !_isNavigating;
        }

        private static void OnShowNextWebResponseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainWindow window = (MainWindow)d;
            if ((bool)e.NewValue)
            {
                //			window.webView.CoreWebView2.WebResourceResponseReceived += window.CoreWebView2_WebResourceResponseReceived;
            }
            else
            {
                //			window.webView.CoreWebView2.WebResourceResponseReceived -= window.CoreWebView2_WebResourceResponseReceived;
            }
        }

        public static readonly DependencyProperty ShowNextWebResponseProperty = DependencyProperty.Register(
            nameof(ShowNextWebResponse),
            typeof(Boolean),
            typeof(MainWindow),
            new PropertyMetadata(false, OnShowNextWebResponseChanged));

        public bool ShowNextWebResponse {
            get => (bool)this.GetValue(ShowNextWebResponseProperty);
            set => this.SetValue(ShowNextWebResponseProperty, value);
        }

        ///////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Backボタン
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        void BackCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            webView.GoBack();
        }

        /// <summary>
        /// Forwardボタン
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        void ForwardCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            webView.GoForward();
        }

        /// <summary>
        /// Refreshボタン
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        void RefreshCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            webView.Reload();
        }

        /// <summary>
        /// stopボタン
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        void BrowseStopCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            webView.Stop();
        }

        /// <summary>
        /// Goボタン
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        async void GoToPageCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            // Setting webView.Source will not trigger a navigation if the Source is the same
            // as the previous Source.  CoreWebView.Navigate() will always trigger a navigation.
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Navigate((string)e.Parameter);
        }

        // イベントで設定される物 ///////////////////////////////////////////////////////////////////////////////////////
        /* ViewModelに配置済み ///////////////////////////////////////////////////////////////////////////////////////
		void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
		private void WebView_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
		private void WebView_Loaded(object sender, RoutedEventArgs e)
		// ViewModelに配置済み /////////////////////////////////////////////////////////////////////////////////////*/
        //以下は初回読み込み時のみ発生した

        /// <summary>
        /// コンテンツ読込み開始時のみ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            string TAG = "NavigationStarting";
            string dbMsg = "";
            try
            {
                Microsoft.Web.WebView2.Wpf.WebView2 wv2 = (Microsoft.Web.WebView2.Wpf.WebView2)sender;
                string tUri = wv2.Source.AbsoluteUri;
                dbMsg += "AbsoluteUri=" + tUri;
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        private void WebView_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            string TAG = "SourceUpdated";
            string dbMsg = "";
            try
            {
                Microsoft.Web.WebView2.Wpf.WebView2 wv2 = (Microsoft.Web.WebView2.Wpf.WebView2)sender;
                string tUri = wv2.Source.AbsoluteUri;
                dbMsg += "AbsoluteUri=" + tUri;
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        private void WebView_ContentLoading(object sender, CoreWebView2ContentLoadingEventArgs e)
        {
            string TAG = "ContentLoading";
            string dbMsg = "";
            try
            {
                Microsoft.Web.WebView2.Wpf.WebView2 wv2 = (Microsoft.Web.WebView2.Wpf.WebView2)sender;
                string tUri = wv2.Source.AbsoluteUri;
                dbMsg += "AbsoluteUri=" + tUri;
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string TAG = "WebMessageReceived";
            string dbMsg = "";
            try
            {
                Microsoft.Web.WebView2.Wpf.WebView2 wv2 = (Microsoft.Web.WebView2.Wpf.WebView2)sender;
                string tUri = wv2.Source.AbsoluteUri;
                dbMsg += "AbsoluteUri=" + tUri;
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }
        private void WebView_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            string TAG = "RequestBringIntoView";
            string dbMsg = "";
            try
            {
                Microsoft.Web.WebView2.Wpf.WebView2 wv2 = (Microsoft.Web.WebView2.Wpf.WebView2)sender;
                string tUri = wv2.Source.AbsoluteUri;
                dbMsg += "AbsoluteUri=" + tUri;
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        private void WebView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            string TAG = "TargetUpdated";
            string dbMsg = "";
            try
            {
                Microsoft.Web.WebView2.Wpf.WebView2 wv2 = (Microsoft.Web.WebView2.Wpf.WebView2)sender;
                string tUri = wv2.Source.AbsoluteUri;
                dbMsg += "AbsoluteUri=" + tUri;
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }

        private void WebView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string TAG = "MouseUp";
            string dbMsg = "";
            try
            {
                Microsoft.Web.WebView2.Wpf.WebView2 wv2 = (Microsoft.Web.WebView2.Wpf.WebView2)sender;
                string tUri = wv2.Source.AbsoluteUri;
                dbMsg += "AbsoluteUri=" + tUri;
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }
        private void WebView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string TAG = "MouseLeftButtonDown";
            string dbMsg = "";
            try
            {
                Microsoft.Web.WebView2.Wpf.WebView2 wv2 = (Microsoft.Web.WebView2.Wpf.WebView2)sender;
                string tUri = wv2.Source.AbsoluteUri;
                dbMsg += "AbsoluteUri=" + tUri;
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        private void WebView_GotMouseCapture(object sender, MouseEventArgs e)
        {
            string TAG = "GotMouseCapture";
            string dbMsg = "";
            try
            {
                Microsoft.Web.WebView2.Wpf.WebView2 wv2 = (Microsoft.Web.WebView2.Wpf.WebView2)sender;
                string tUri = wv2.Source.AbsoluteUri;
                dbMsg += "AbsoluteUri=" + tUri;
                MyLog(TAG, dbMsg);
            }
            catch (Exception er)
            {
                MyErrorLog(TAG, dbMsg, er);
            }
        }


        //async void CoreWebView2_WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        //{
        //	ShowNextWebResponse = false;

        //	CoreWebView2WebResourceRequest request = e.Request;
        //	CoreWebView2WebResourceResponseView response = e.Response;

        //	string caption = "Web Resource Response Received";
        //	// Start with capacity 64 for minimum message size
        //	StringBuilder messageBuilder = new StringBuilder(64);
        //	string HttpMessageContentToString(System.IO.Stream content) => content == null ? "[null]" : "[data]";
        //	void AppendHeaders(IEnumerable headers)
        //	{
        //		foreach (var header in headers) {
        //			messageBuilder.AppendLine($"  {header}");
        //		}
        //	}

        //	// Request
        //	messageBuilder.AppendLine("Request");
        //	messageBuilder.AppendLine($"URI: {request.Uri}");
        //	messageBuilder.AppendLine($"Method: {request.Method}");
        //	messageBuilder.AppendLine("Headers:");
        //	AppendHeaders(request.Headers);
        //	messageBuilder.AppendLine($"Content: {HttpMessageContentToString(request.Content)}");
        //	messageBuilder.AppendLine();

        //	// Response
        //	messageBuilder.AppendLine("Response");
        //	messageBuilder.AppendLine($"Status: {response.StatusCode}");
        //	messageBuilder.AppendLine($"Reason: {response.ReasonPhrase}");
        //	messageBuilder.AppendLine("Headers:");
        //	AppendHeaders(response.Headers);
        //	try {
        //		Stream content = await response.GetContentAsync();
        //		messageBuilder.AppendLine($"Content: {HttpMessageContentToString(content)}");
        //	} catch (System.Runtime.InteropServices.COMException) {
        //		messageBuilder.AppendLine($"Content: [failed to load]");
        //	}

        //	MessageBox.Show(messageBuilder.ToString(), caption);
        //}
        //////////////////////////////////////////////////////////////////////////////////////////
        public static void MyLog(string TAG, string dbMsg)
        {
            dbMsg = "[WebWindow]" + dbMsg;
            //dbMsg = "[" + MethodBase.GetCurrentMethod().Name + "]" + dbMsg;
            CS_Util Util = new CS_Util();
            Util.MyLog(TAG, dbMsg);
        }

        public static void MyErrorLog(string TAG, string dbMsg, Exception err)
        {
            dbMsg = "[WebWindow]" + dbMsg;
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
