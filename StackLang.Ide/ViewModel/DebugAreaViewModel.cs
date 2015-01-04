using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GalaSoft.MvvmLight.CommandWpf;
using StackLang.Ide.Helpers;
using StackLang.Ide.Model;
using StackLang.Ide.MVVMEnhancements;

namespace StackLang.Ide.ViewModel {
	public sealed class DebugAreaViewModel : ViewModelBaseEnhanced, IDisposable {
		readonly DebuggerModel model;

		SnapshotWrapper snapshot;
		public SnapshotWrapper Snapshot {
			get { return snapshot; }
			set {
				if (snapshot == value) {
					return;
				}
				snapshot = value;
				RaisePropertyChanged();
			}
		}

		string _newWatchText = "";
		public string NewWatchText {
			get { return _newWatchText; }
			set {
				if (_newWatchText == value) {
					return;
				}
				_newWatchText = value;
				RaisePropertyChanged();
			}
		}

		ObservableCollection<WatchViewModel> _watches = new ObservableCollection<WatchViewModel>();
		public ObservableCollection<WatchViewModel> Watches {
			get { return _watches; }
			set {
				if (_watches == value) {
					return;
				}
				_watches = value;
				RaisePropertyChanged();
			}
		}

		int _maxAddress;
		public int MaxAddress {
			get { return _maxAddress; }
			set {
				if (_maxAddress == value) {
					return;
				}
				_maxAddress = value;
				RaisePropertyChanged();
			}
		}

		public RelayCommand NewWatchCommand {
			get {
				return GetRelayCommand(OnNewWatch, () => !string.IsNullOrWhiteSpace(NewWatchText));
			}
		}

		public DebugAreaViewModel(DebuggerModel newModel) {
			model = newModel;
			model.NewSnapshot += OnNewSnapshot;

			Watches.CollectionChanged += OnWatchesCollectionChanged;

			Watches.Add(new WatchViewModel(WatchViewModel.WatchType.Register) {CloseEnabled = false});
		}

		void OnWatchesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			foreach (WatchViewModel vm in e.NewItems.NotNullCollection()) {
				vm.RequestRemove += OnWatchRemove;
			}
			foreach (WatchViewModel vm in e.OldItems.NotNullCollection()) {
				vm.RequestRemove -= OnWatchRemove;
			}
		}

		void OnNewWatch() {
			int address = int.Parse(NewWatchText);
			NewWatchText = "";

			if (address < 0 || address >= MaxAddress) {
				return;
			}

			WatchViewModel existingViewModel = Watches.FirstOrDefault(watch => watch.Type == WatchViewModel.WatchType.Address
				&& watch.WatchAddress == address);

			if (existingViewModel == null) {
				Watches.Add(new WatchViewModel(WatchViewModel.WatchType.Address, address, snapshot));
			}
			else {
				Watches.Remove(existingViewModel);
			}
		}

		void OnWatchRemove(object sender, EventArgs e) {
			Watches.Remove((WatchViewModel) sender);
		}

		public void OnNewSnapshot(object sender, NewSnapshotEventArgs e) {
			Snapshot = e.SnapshotWrapper;
			MaxAddress = Snapshot.Memory.Count;

			foreach (WatchViewModel viewModel in Watches) {
				viewModel.UpdateWatch(e.SnapshotWrapper);
			}
		}

		public void Dispose() {
			model.NewSnapshot -= OnNewSnapshot;
		}
	}
}