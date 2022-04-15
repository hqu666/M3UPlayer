﻿#pragma checksum "..\..\..\..\views\WebWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "319B3F49A38058005631636891A66588DF224F1A"
//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Web.WebView2.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace M3UPlayer.Views {
    
    
    /// <summary>
    /// WebWindow
    /// </summary>
    public partial class WebWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 76 "..\..\..\..\views\WebWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DockPanel topPanel;
        
        #line default
        #line hidden
        
        
        #line 82 "..\..\..\..\views\WebWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonHome;
        
        #line default
        #line hidden
        
        
        #line 96 "..\..\..\..\views\WebWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox addressBar;
        
        #line default
        #line hidden
        
        
        #line 100 "..\..\..\..\views\WebWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Web.WebView2.Wpf.WebView2 webView;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.4.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/M3UPlayer;component/views/webwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\views\WebWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.4.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 14 "..\..\..\..\views\WebWindow.xaml"
            ((M3UPlayer.Views.WebWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 23 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.NewCmdExecuted);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 24 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.CloseCmdExecuted);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 25 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.BackCmdExecuted);
            
            #line default
            #line hidden
            
            #line 25 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).CanExecute += new System.Windows.Input.CanExecuteRoutedEventHandler(this.BackCmdCanExecute);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 26 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.ForwardCmdExecuted);
            
            #line default
            #line hidden
            
            #line 26 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).CanExecute += new System.Windows.Input.CanExecuteRoutedEventHandler(this.ForwardCmdCanExecute);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 27 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.RefreshCmdExecuted);
            
            #line default
            #line hidden
            
            #line 27 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).CanExecute += new System.Windows.Input.CanExecuteRoutedEventHandler(this.RefreshCmdCanExecute);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 28 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.BrowseStopCmdExecuted);
            
            #line default
            #line hidden
            
            #line 28 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).CanExecute += new System.Windows.Input.CanExecuteRoutedEventHandler(this.BrowseStopCmdCanExecute);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 29 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.GoToPageCmdExecuted);
            
            #line default
            #line hidden
            
            #line 29 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).CanExecute += new System.Windows.Input.CanExecuteRoutedEventHandler(this.GoToPageCmdCanExecute);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 30 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.IncreaseZoomCmdExecuted);
            
            #line default
            #line hidden
            
            #line 30 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).CanExecute += new System.Windows.Input.CanExecuteRoutedEventHandler(this.IncreaseZoomCmdCanExecute);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 31 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.DecreaseZoomCmdExecuted);
            
            #line default
            #line hidden
            
            #line 31 "..\..\..\..\views\WebWindow.xaml"
            ((System.Windows.Input.CommandBinding)(target)).CanExecute += new System.Windows.Input.CanExecuteRoutedEventHandler(this.DecreaseZoomCmdCanExecute);
            
            #line default
            #line hidden
            return;
            case 11:
            this.topPanel = ((System.Windows.Controls.DockPanel)(target));
            return;
            case 12:
            this.ButtonHome = ((System.Windows.Controls.Button)(target));
            return;
            case 13:
            this.addressBar = ((System.Windows.Controls.TextBox)(target));
            return;
            case 14:
            this.webView = ((Microsoft.Web.WebView2.Wpf.WebView2)(target));
            
            #line 103 "..\..\..\..\views\WebWindow.xaml"
            this.webView.NavigationStarting += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs>(this.WebView_NavigationStarting);
            
            #line default
            #line hidden
            
            #line 104 "..\..\..\..\views\WebWindow.xaml"
            this.webView.SourceUpdated += new System.EventHandler<System.Windows.Data.DataTransferEventArgs>(this.WebView_SourceUpdated);
            
            #line default
            #line hidden
            
            #line 105 "..\..\..\..\views\WebWindow.xaml"
            this.webView.ContentLoading += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2ContentLoadingEventArgs>(this.WebView_ContentLoading);
            
            #line default
            #line hidden
            
            #line 106 "..\..\..\..\views\WebWindow.xaml"
            this.webView.WebMessageReceived += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs>(this.WebView_WebMessageReceived);
            
            #line default
            #line hidden
            
            #line 107 "..\..\..\..\views\WebWindow.xaml"
            this.webView.RequestBringIntoView += new System.Windows.RequestBringIntoViewEventHandler(this.WebView_RequestBringIntoView);
            
            #line default
            #line hidden
            
            #line 108 "..\..\..\..\views\WebWindow.xaml"
            this.webView.TargetUpdated += new System.EventHandler<System.Windows.Data.DataTransferEventArgs>(this.WebView_TargetUpdated);
            
            #line default
            #line hidden
            
            #line 109 "..\..\..\..\views\WebWindow.xaml"
            this.webView.MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.WebView_MouseUp);
            
            #line default
            #line hidden
            
            #line 110 "..\..\..\..\views\WebWindow.xaml"
            this.webView.GotMouseCapture += new System.Windows.Input.MouseEventHandler(this.WebView_GotMouseCapture);
            
            #line default
            #line hidden
            
            #line 111 "..\..\..\..\views\WebWindow.xaml"
            this.webView.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.WebView_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

