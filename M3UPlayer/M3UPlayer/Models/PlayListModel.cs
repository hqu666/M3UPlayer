using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;

namespace M3UPlayer.Models
{
    public class PlayListModel : ICloneable
    {
        /// <summary>
        /// Url
        /// </summary>
        private string _UrlStr;
        public string UrlStr
        {
            get => _UrlStr;
            set {
                if (_UrlStr == value)
                    return;
                _UrlStr = value;
                RaisePropertyChanged("UrlStr");

                string[] urls = UrlStr.Split(Path.DirectorySeparatorChar);
                if (urls.Length < 2) {
                    urls = UrlStr.Split('/');
                }
                if (2 < urls.Length) {
                    this.ParentDir = urls[urls.Length - 2];
				} else {
                    this.ParentDir = null;
                }
                if (3 < urls.Length) {
                    this.GranDir = urls[urls.Length - 3];
                } else {
                    this.GranDir = null;
                }
                //string[] remains = _UrlStr.Split(this.ParentDir);
                //this.GranDir = remains[0];

                this.fileName = Path.GetFileName(UrlStr);                  //  urls[urls.Length - 1];
				if (this.fileName == null || this.fileName.Length == 0) {
                    this.extentionStr = null;
                    this.Summary = null;
				} else {
                    this.extentionStr = Path.GetExtension(UrlStr);
                    this.Summary = this.fileName.Replace(this.extentionStr, "");
                }


            }
        }


        /// <summary>
        /// 要約・表記
        /// </summary>
        private string? _Summary;
        public string? Summary
        {
            get => _Summary;
            set {
                if (_Summary == value)
                    return;
                _Summary = value;
                RaisePropertyChanged("Summary");
            }
        }

        /// <summary>
        /// 3階層以上
        /// </summary>
        private string? _GranDir;
        public string? GranDir {
            get => _GranDir;
            set {
                if (_GranDir == value)
                    return;
                _GranDir = value;
                RaisePropertyChanged("GranDir");
            }
        }

        /// <summary>
        /// 親階層
        /// </summary>
        private string? _ParentDir;
        public string? ParentDir {
            get => _ParentDir;
            set {
                if (_ParentDir == value)
                    return;
                _ParentDir = value;
                RaisePropertyChanged("ParentDir");
            }
        }

        /// <summary>
        /// ファイル名
        /// </summary>
        private string? _fileName;
        public string? fileName {
            get => _fileName;
            set {
                if (_fileName == value)
                    return;
                _fileName = value;
                RaisePropertyChanged("fileName");
            }
        }

        private string? _extentionStr;
        public string? extentionStr {
            get => _extentionStr;
            set {
                if (_extentionStr == value)
                    return;
                _extentionStr = value;
                RaisePropertyChanged("extentionStr");
            }
        }


        private bool _ActionFlag;
		public bool ActionFlag {
			get => _ActionFlag;
			set {
				if (_ActionFlag == value)
					return;
				_ActionFlag = value;
                RaisePropertyChanged("ActionFlag");
            }
        }


		object ICloneable.Clone()
		{
            return new PlayListModel() {
                UrlStr = this.UrlStr,
                Summary = this.Summary,
                ActionFlag = this.ActionFlag,
                GranDir = this.GranDir,
                ParentDir = this.ParentDir,
                fileName = this.fileName,
                extentionStr = this.extentionStr,
            };
		}

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }

    public class PlayListCollection : ObservableCollection<PlayListModel>
    {
        public PlayListCollection()
        {
        }
    }
}
