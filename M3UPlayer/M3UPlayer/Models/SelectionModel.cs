using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;

namespace M3UPlayer.Models {
    public class SelectionModel : ICloneable {

        /// <summary>
        /// プレイリストの選択情報
        /// </summary>
        public SelectionModel() {
            ListItem = new PlayListModel();
        }


        /// <summary>
        /// プレイリストのUrl
        /// </summary>
        private string _PlayListUrlStr;
        public string PlayListUrlStr {
            get => _PlayListUrlStr;
            set {
                if (_PlayListUrlStr == value)
                    return;
                _PlayListUrlStr = value;
                RaisePropertyChanged("PlayListUrlStr");
            }
        }


        /// <summary>
        /// 選択しているインデックス
        /// </summary>
        private int _SelectedIndex;
        public int SelectedIndex {
            get => _SelectedIndex;
            set {
                if (_SelectedIndex == value)
                    return;
                _SelectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }

        /// <summary>
        /// 対象アイテムの内容
        /// </summary>
        private PlayListModel _ListItem;
        public PlayListModel ListItem {
            get => _ListItem;
            set {
                if (_ListItem == value)
                    return;
                _ListItem = value;
                RaisePropertyChanged("ListItem");
            }
        }

        object ICloneable.Clone() {
            return new SelectionModel() {
                PlayListUrlStr = this.PlayListUrlStr,
                SelectedIndex = this.SelectedIndex,
                ListItem = this.ListItem,
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}
