using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using StackLang.Ide.Helpers;
using StackLang.Ide.Model;

namespace StackLang.Ide.ViewModel {
	public class MainViewModel : ViewModelBase {
		readonly InterpreterModel interpreter;
		readonly DebuggerModel debugger;
		readonly ExecutionIoModel executionIoModel;
		readonly OutputAreaModel outputAreaModel;

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

		EditorTabViewModel _selectedTabViewModel;
		public EditorTabViewModel SelectedTabViewModel {
			get { return _selectedTabViewModel; }
			set {
				if (_selectedTabViewModel == value) {
					return;
				}
				_selectedTabViewModel = value;
				RaisePropertyChanged();
			}
		}

		ExecutionAreaViewModel _executionAreaViewModel = new ExecutionAreaViewModel();
		public ExecutionAreaViewModel ExecutionAreaViewModel {
			get { return _executionAreaViewModel; }
			set {
				if (_executionAreaViewModel == value) {
					return;
				}
				_executionAreaViewModel = value;
				RaisePropertyChanged();
			}
		}

		OutputAreaViewModel _outputAreaViewModel = new OutputAreaViewModel();
		public OutputAreaViewModel OutputAreaViewModel {
			get { return _outputAreaViewModel; }
			set {
				if (_outputAreaViewModel == value) {
					return;
				}
				_outputAreaViewModel = value;
				RaisePropertyChanged();
			}
		}

		RelayCommand newTabCommand;
		public RelayCommand NewTabCommand {
			get {
				return newTabCommand ?? (newTabCommand = new RelayCommand(() =>
					AddTab(new EditorTabViewModel())));
			}
		}

		RelayCommand openFileCommand;
		public RelayCommand OpenFileCommand {
			get {
				return openFileCommand ?? (openFileCommand = new RelayCommand(() => {
					EditorTabViewModel newTab = EditorTabViewModel.Open();
					if (newTab != null) {
						AddTab(newTab);
					}
				}));
			}
		}

		RelayCommand saveCommand;
		public RelayCommand SaveCommand {
			get {
				return saveCommand ?? (saveCommand = new RelayCommand(
					() => SelectedTabViewModel.Save(),
					() => SelectedTabViewModel != null));
			}
		}

		RelayCommand saveAsCommand;
		public RelayCommand SaveAsCommand {
			get {
				return saveAsCommand ?? (saveAsCommand = new RelayCommand(
					() => SelectedTabViewModel.Save(true),
					() => SelectedTabViewModel != null));
			}
		}

		RelayCommand closeCommand;
		public RelayCommand CloseCommand {
			get {
				return closeCommand ?? (closeCommand = new RelayCommand(
					() => SelectedTabViewModel.RaiseRequestRemove(),
					() => SelectedTabViewModel != null));
			}
		}

		RelayCommand runCommand;
		public RelayCommand RunCommand {
			get {
				return runCommand ?? (runCommand = new RelayCommand(Run,
					() => SelectedTabViewModel != null && !interpreter.ExecutionRunning));
			}
		}

		public MainViewModel() {
			EditorTabViewModels.CollectionChanged += OnEditorTabsCollectionChanged;
			AddTab(new EditorTabViewModel());

			executionIoModel = ExecutionAreaViewModel.Model;
			interpreter = new InterpreterModel(OutputAreaViewModel.Model);
			debugger = new DebuggerModel();
		}

		void AddTab(EditorTabViewModel viewModel) {
			EditorTabViewModels.Add(viewModel);
			SelectedTabViewModel = viewModel;
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
			EditorTabViewModels.Remove((EditorTabViewModel)s);
		}

		void Run() {
			executionIoModel.Clear();

			interpreter.InputManager = executionIoModel;
			interpreter.OutputManager = executionIoModel;

			interpreter.Run(SelectedTabViewModel.Text);
		}
	}
}