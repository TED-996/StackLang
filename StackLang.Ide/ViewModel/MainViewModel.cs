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
		readonly ExecutionAreaModel executionAreaModel;
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

				SettingsViewModel.IoModel = (value == null ? null : value.IoSettingsModel);

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

		SettingsViewModel _settingsViewModel = new SettingsViewModel();
		public SettingsViewModel SettingsViewModel {
			get { return _settingsViewModel; }
			set {
				if (_settingsViewModel == value) {
					return;
				}
				_settingsViewModel = value;
				RaisePropertyChanged();
			}
		}

		DebugAreaViewModel _debugAreaViewModel;
		public DebugAreaViewModel DebugAreaViewModel {
			get { return _debugAreaViewModel; }
			set {
				if (_debugAreaViewModel == value) {
					return;
				}
				_debugAreaViewModel = value;
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
				return openFileCommand ?? (openFileCommand = new RelayCommand(OpenTab));
			}
		}

		RelayCommand saveCommand;
		public RelayCommand SaveCommand {
			get {
				return saveCommand ?? (saveCommand = new RelayCommand(
					() => SaveTab(false),
					() => SelectedTabViewModel != null));
			}
		}

		RelayCommand saveAsCommand;

		public RelayCommand SaveAsCommand {
			get {
				return saveAsCommand ?? (saveAsCommand = new RelayCommand(
					() => SaveTab(true),
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
					() => SelectedTabViewModel != null && !interpreter.ExecutionRunning && !debugger.ExecutionRunning));
			}
		}

		RelayCommand abortCommand;
		public RelayCommand AbortCommand {
			get {
				return abortCommand ?? (abortCommand = new RelayCommand(Abort,
					() => interpreter.ExecutionRunning || debugger.ExecutionRunning));
			}
		}

		RelayCommand debugCommand;
		public RelayCommand DebugCommand {
			get {
				return debugCommand ?? (debugCommand = new RelayCommand(Debug, 
					() => SelectedTabViewModel != null && !interpreter.ExecutionRunning && !debugger.ExecutionRunning));
			}
		}

		RelayCommand stepCommand;
		public RelayCommand StepCommand {
			get {
				return stepCommand ?? (stepCommand = new RelayCommand(debugger.Step,
					() => debugger.ExecutionRunning && !debugger.StepRunning));
			}
		}

		public MainViewModel() {
			EditorTabViewModels.CollectionChanged += OnEditorTabsCollectionChanged;
			AddTab(new EditorTabViewModel());

			executionAreaModel = ExecutionAreaViewModel.Model;
			outputAreaModel = OutputAreaViewModel.Model;
			interpreter = new InterpreterModel(outputAreaModel);
			debugger = new DebuggerModel(outputAreaModel);

			debugger.DebugStart += OnDebugStart;
			debugger.DebugEnd += OnDebugEnd;
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

		void OpenTab() {
			EditorTabViewModel newTab;
			try {
				newTab = EditorTabViewModel.Open();
			}
			catch (FileException ex) {
				outputAreaModel.WriteLine(ex.ToString());
				return;
			}
			if (newTab != null) {
				AddTab(newTab);
			}
		}

		void SaveTab(bool saveAs) {
			try {
				SelectedTabViewModel.Save(saveAs);
			}
			catch (FileException ex) {
				outputAreaModel.WriteLine(ex.ToString());
			}
		}

		void Run() {
			executionAreaModel.Clear();

			try {
				interpreter.InputManager = SelectedTabViewModel.IoSettingsModel.GetInputManager(executionAreaModel);
				interpreter.OutputManager = SelectedTabViewModel.IoSettingsModel.GetOutputManager(executionAreaModel);
			}
			catch (FileException ex) {
				outputAreaModel.WriteLine(ex.ToString());
				return;
			}

			interpreter.Run(SelectedTabViewModel.Text);
		}

		void Debug() {
			executionAreaModel.Clear();

			try {
				debugger.InputManager = SelectedTabViewModel.IoSettingsModel.GetInputManager(executionAreaModel);
				debugger.OutputManager = SelectedTabViewModel.IoSettingsModel.GetOutputManager(executionAreaModel);
			}
			catch (FileException ex) {
				outputAreaModel.WriteLine(ex.ToString());
				return;
			}

			debugger.Start(SelectedTabViewModel);
		}

		void OnDebugStart(object sender, EventArgs e) {
			DebugAreaViewModel = new DebugAreaViewModel(debugger);
		}
		
		void OnDebugEnd(object sender, EventArgs e) {
			DebugAreaViewModel = null;
		}

		void Abort() {
			if (interpreter.ExecutionRunning) {
				interpreter.Abort();
			}
			else if (debugger.ExecutionRunning) {
				debugger.Abort();
			}
		}
	}
}