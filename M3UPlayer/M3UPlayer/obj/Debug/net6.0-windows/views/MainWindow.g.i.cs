﻿#pragma checksum "..\..\..\..\views\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "3524E37A4EDF95491853A2D7DBAEDEE7F9E968C7"
//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

using M3UPlayer;
using M3UPlayer.ViewModels;
using M3UPlayer.Views;
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
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 230 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid PlayList;
        
        #line default
        #line hidden
        
        
        #line 308 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.Popup popup1;
        
        #line default
        #line hidden
        
        
        #line 319 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock popup_text;
        
        #line default
        #line hidden
        
        
        #line 353 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image MuteBtImage;
        
        #line default
        #line hidden
        
        
        #line 362 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider SoundSlider;
        
        #line default
        #line hidden
        
        
        #line 401 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ControlHideBT;
        
        #line default
        #line hidden
        
        
        #line 422 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid FrameGrid;
        
        #line default
        #line hidden
        
        
        #line 424 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Web.WebView2.Wpf.WebView2 webView;
        
        #line default
        #line hidden
        
        
        #line 446 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid PlayerContlolGR;
        
        #line default
        #line hidden
        
        
        #line 455 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button PlayBt;
        
        #line default
        #line hidden
        
        
        #line 464 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image PlayBtImage;
        
        #line default
        #line hidden
        
        
        #line 521 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox RewCB;
        
        #line default
        #line hidden
        
        
        #line 534 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider PositionSL;
        
        #line default
        #line hidden
        
        
        #line 835 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ForwardCB;
        
        #line default
        #line hidden
        
        
        #line 858 "..\..\..\..\views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ForwardBT;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.3.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/M3UPlayer;component/views/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\views\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.3.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 18 "..\..\..\..\views\MainWindow.xaml"
            ((M3UPlayer.Views.MainWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.PlayList = ((System.Windows.Controls.DataGrid)(target));
            
            #line 236 "..\..\..\..\views\MainWindow.xaml"
            this.PlayList.MouseMove += new System.Windows.Input.MouseEventHandler(this.PlayList_MouseMove);
            
            #line default
            #line hidden
            
            #line 237 "..\..\..\..\views\MainWindow.xaml"
            this.PlayList.GiveFeedback += new System.Windows.GiveFeedbackEventHandler(this.PlayListBox_GiveFeedback);
            
            #line default
            #line hidden
            
            #line 245 "..\..\..\..\views\MainWindow.xaml"
            this.PlayList.PreviewDragEnter += new System.Windows.DragEventHandler(this.PlayList_PreviewDragOver);
            
            #line default
            #line hidden
            
            #line 246 "..\..\..\..\views\MainWindow.xaml"
            this.PlayList.PreviewDrop += new System.Windows.DragEventHandler(this.PlayList_PreviewDrop);
            
            #line default
            #line hidden
            
            #line 247 "..\..\..\..\views\MainWindow.xaml"
            this.PlayList.DragEnter += new System.Windows.DragEventHandler(this.PlayList_DragEnter);
            
            #line default
            #line hidden
            
            #line 248 "..\..\..\..\views\MainWindow.xaml"
            this.PlayList.Drop += new System.Windows.DragEventHandler(this.PlayList_Drop);
            
            #line default
            #line hidden
            
            #line 249 "..\..\..\..\views\MainWindow.xaml"
            this.PlayList.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.PlayList_MouseDown);
            
            #line default
            #line hidden
            
            #line 250 "..\..\..\..\views\MainWindow.xaml"
            this.PlayList.MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.PlayList_MouseUp);
            
            #line default
            #line hidden
            return;
            case 3:
            this.popup1 = ((System.Windows.Controls.Primitives.Popup)(target));
            return;
            case 4:
            this.popup_text = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.MuteBtImage = ((System.Windows.Controls.Image)(target));
            return;
            case 6:
            this.SoundSlider = ((System.Windows.Controls.Slider)(target));
            
            #line 369 "..\..\..\..\views\MainWindow.xaml"
            this.SoundSlider.AddHandler(System.Windows.Controls.Primitives.Thumb.DragCompletedEvent, new System.Windows.Controls.Primitives.DragCompletedEventHandler(this.SoundSlider_DragCompleted));
            
            #line default
            #line hidden
            return;
            case 7:
            this.ControlHideBT = ((System.Windows.Controls.Button)(target));
            return;
            case 8:
            this.FrameGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 9:
            this.webView = ((Microsoft.Web.WebView2.Wpf.WebView2)(target));
            return;
            case 10:
            this.PlayerContlolGR = ((System.Windows.Controls.Grid)(target));
            return;
            case 11:
            this.PlayBt = ((System.Windows.Controls.Button)(target));
            return;
            case 12:
            this.PlayBtImage = ((System.Windows.Controls.Image)(target));
            return;
            case 13:
            this.RewCB = ((System.Windows.Controls.ComboBox)(target));
            
            #line 530 "..\..\..\..\views\MainWindow.xaml"
            this.RewCB.DropDownClosed += new System.EventHandler(this.RewCB_DropDownClosed);
            
            #line default
            #line hidden
            return;
            case 14:
            this.PositionSL = ((System.Windows.Controls.Slider)(target));
            
            #line 542 "..\..\..\..\views\MainWindow.xaml"
            this.PositionSL.AddHandler(System.Windows.Controls.Primitives.Thumb.DragStartedEvent, new System.Windows.Controls.Primitives.DragStartedEventHandler(this.PositionSL_DragStarted));
            
            #line default
            #line hidden
            
            #line 543 "..\..\..\..\views\MainWindow.xaml"
            this.PositionSL.AddHandler(System.Windows.Controls.Primitives.Thumb.DragCompletedEvent, new System.Windows.Controls.Primitives.DragCompletedEventHandler(this.PositionSL_DragCompleted));
            
            #line default
            #line hidden
            return;
            case 15:
            this.ForwardCB = ((System.Windows.Controls.ComboBox)(target));
            
            #line 844 "..\..\..\..\views\MainWindow.xaml"
            this.ForwardCB.DropDownClosed += new System.EventHandler(this.ForwardCB_DropDownClosed);
            
            #line default
            #line hidden
            return;
            case 16:
            this.ForwardBT = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

