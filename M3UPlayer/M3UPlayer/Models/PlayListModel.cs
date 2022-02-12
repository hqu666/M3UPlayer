using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace M3UPlayer.Models
{
    public class PlayListModel : ICloneable
    {
        /// <summary>
        /// Url
        /// </summary>
        private string? _UrlStr;
        public string UrlStr
        {
            get => _UrlStr;
            set {
                if (_UrlStr == value)
                    return;
                _UrlStr = value;
                RaisePropertyChanged();
            }
        }

		private void RaisePropertyChanged()
		{
	//		throw new NotImplementedException();
		}

		/// <summary>
		/// 要約・表記
		/// </summary>
		private string? _Summary;
        public string Summary
        {
            get => _Summary;
            set {
                if (_Summary == value)
                    return;
                _Summary = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 3階層以上
        /// </summary>
        private string _GranDir;
        public string GranDir {
            get => _GranDir;
            set {
                if (_GranDir == value)
                    return;
                _GranDir = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 親階層
        /// </summary>
        private string _ParentDir;
        public string ParentDir {
            get => _ParentDir;
            set {
                if (_ParentDir == value)
                    return;
                _ParentDir = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// ファイル名
        /// </summary>
        private string _fileName;
        public string fileName {
            get => _fileName;
            set {
                if (_fileName == value)
                    return;
                _fileName = value;
                RaisePropertyChanged();
            }
        }

        private string _extentionStr;
        public string extentionStr {
            get => _extentionStr;
            set {
                if (_extentionStr == value)
                    return;
                _extentionStr = value;
                RaisePropertyChanged();
            }
        }


        private bool _ActionFlag;

		public event PropertyChangedEventHandler? PropertyChanged;

		public bool ActionFlag {
			get => _ActionFlag;
			set {
				if (_ActionFlag == value)
					return;
				_ActionFlag = value;
				RaisePropertyChanged();
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

	}

    public class PlayListCollection : ObservableCollection<PlayListModel>
    {
        public PlayListCollection()
        {
        }
    }
}
