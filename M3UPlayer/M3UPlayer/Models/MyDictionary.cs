using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace M3UPlayer.Models {
	internal class MyDictionary : ICloneable {
        /// <summary>
        /// 項目名
        /// </summary>
        private string _key;
        public string Key {
            get => _key;
            set {
                if (_key == value)
                    return;
                _key = value;
                RaisePropertyChanged("Key");
            }
        }

        private string _Value;
        public string Value {
            get => _Value;
            set {
                if (_Value == value)
                    return;
                _Value = value;
                RaisePropertyChanged("Value");
            }
        }


        object ICloneable.Clone() {
            return new MyDictionary() {
                Key = this.Key,
                Value = this.Value,
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
