using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

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
                RaisePropertyChanged();
            }
        }

		private void RaisePropertyChanged()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 要約・表記
		/// </summary>
		private string _Summary;
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


		private bool _ActionFlag;
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
            return new PlayListModel()
            {
                UrlStr = this.UrlStr,
                Summary = this.Summary,
				ActionFlag = this.ActionFlag,
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
