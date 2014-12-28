using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using StackLang.Ide.Model;
using TestingMvvmLight.Helpers;

namespace StackLang.Ide.ViewModel {
	public class MainViewModel : ViewModelBase {
		InterpreterModel interpreter;
		DebuggerModel debugger;

		ObservableCollection<EditorTabViewModel> _editorTabViewModels = new ObservableCollection<EditorTabViewModel>();
		public ObservableCollection<EditorTabViewModel> EditorTabViewModels {
			get { return _editorTabViewModels; }
			set {
				if (_editorTabViewModels == value) {
					return;
				}
				_editorTabViewModels = value;
				RaisePropertyChanged();
			}
		}

		RelayCommand addTabCommand;
		public RelayCommand AddTabCommand {
			get {
				return addTabCommand ?? (addTabCommand = new RelayCommand(() => 
					EditorTabViewModels.Add(new EditorTabViewModel())));
			}
		}

		public MainViewModel() {
			EditorTabViewModels.CollectionChanged += OnEditorTabsCollectionChanged;
			EditorTabViewModels.Add(new EditorTabViewModel());
		}

		void OnEditorTabsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			foreach (EditorTabViewModel vm in e.NewItems.NotNullCollection()) {
				vm.RequestRemove += OnEditorTabRemove;
			}
			foreach (EditorTabViewModel vm in e.OldItems.NotNullCollection()) {
				vm.RequestRemove -= OnEditorTabRemove;
			}
		}

		void OnEditorTabRemove(object s, EventArgs e) {
			EditorTabViewModels.Remove((EditorTabViewModel) s);
		}
	}
}