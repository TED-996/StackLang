using System;
using GalaSoft.MvvmLight.CommandWpf;
using StackLang.Core;
using StackLang.Ide.Helpers;
using StackLang.Ide.MVVMEnhancements;

namespace StackLang.Ide.ViewModel {
	public class WatchViewModel : ViewModelBaseEnhanced {
		public readonly WatchType Type;
		public readonly int WatchAddress;

		IStackObject _stackObject;
		public IStackObject StackObject {
			get { return _stackObject; }
			set {
				if (_stackObject == value) {
					return;
				}
				_stackObject = value;
				RaisePropertyChanged();
				RaisePropertyChanged("DisplayValue");
			}
		}

		public string DisplayValue {
			get { return GetDisplayValue(); }
		}

		string _name = "";
		public string Name {
			get { return _name; }
			set {
				if (_name == value) {
					return;
				}
				_name = value;
				RaisePropertyChanged();
			}
		}

		bool _closeEnabled = true;
		public bool CloseEnabled {
			get { return _closeEnabled; }
			set {
				if (_closeEnabled == value) {
					return;
				}
				_closeEnabled = value;
				RaisePropertyChanged();
			}
		}

		public RelayCommand RemoveCommand {
			get { return GetRelayCommand(RaiseRequestRemove); }
		}

		public WatchViewModel(WatchType newType, int newAddress = 0, SnapshotWrapper snapshot = null) {
			Type = newType;
			WatchAddress = newAddress;
			Name = (Type == WatchType.Register
				? "Register"
				: "M: " + WatchAddress);

			if (snapshot != null) {
				UpdateWatch(snapshot);
			}
		}

		string GetDisplayValue() {
			return StackObject == null ? "null" : StackObject.GetPrintedValue();
		}

		public void UpdateWatch(SnapshotWrapper snapshot) {
			StackObject = (Type == WatchType.Register
				? snapshot.Register
				: snapshot.Memory[WatchAddress]);
		}

		public event EventHandler RequestRemove;
		public void RaiseRequestRemove() {
			EventHandler handler = RequestRemove;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}
		}

		public enum WatchType {
			Register,
			Address
		}
	}
}