using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Xml;
using GalaSoft.MvvmLight.CommandWpf;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using StackLang.Ide.Helpers;
using StackLang.Ide.Model;
using StackLang.Ide.MVVMEnhancements;

namespace StackLang.Ide.ViewModel {
	public class MainViewModel : ViewModelBaseEnhanced {
		readonly InterpreterModel interpreter;
		readonly DebuggerModel debugger;
		readonly ExecutionAreaModel executionAreaModel;
		readonly OutputAreaModel outputAreaModel;

		readonly IHighlightingDefinition stackLangSyntaxDefinition;

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

		public RelayCommand NewTabCommand {
			get { return GetRelayCommand(() => AddTab(new EditorTabViewModel(stackLangSyntaxDefinition))); }
		}
		public RelayCommand OpenFileCommand {
			get { return GetRelayCommand(OpenTab); }
		}
		public RelayCommand SaveCommand {
			get { return GetRelayCommand(() => SaveTab(false), () => SelectedTabViewModel != null); }
		}
		public RelayCommand SaveAsCommand {
			get { return GetRelayCommand(() => SaveTab(true), () => SelectedTabViewModel != null); }
		}
		public RelayCommand CloseCommand {
			get {
				return GetRelayCommand(() => SelectedTabViewModel.RaiseRequestRemove(),
				() => SelectedTabViewModel != null);
			}
		}
		public RelayCommand RunCommand {
			get {
				return GetRelayCommand(Run,
					() => SelectedTabViewModel != null && !interpreter.ExecutionRunning && !debugger.ExecutionRunning);
			}
		}
		public RelayCommand AbortCommand {
			get { return GetRelayCommand(Abort, () => interpreter.ExecutionRunning || debugger.ExecutionRunning); }
		}
		public RelayCommand DebugCommand {
			get {
				return GetRelayCommand(Debug,
					() => SelectedTabViewModel != null && !interpreter.ExecutionRunning && !debugger.ExecutionRunning);
			}
		}
		public RelayCommand StepCommand {
			get {
				return GetRelayCommand(debugger.Step,
					() => debugger.ExecutionRunning && !debugger.StepRunning && !debugger.InContinue);
			}
		}
		public RelayCommand ContinueCommand {
			get {
				return GetRelayCommand(debugger.Continue,
					() => debugger.ExecutionRunning && !debugger.StepRunning && !debugger.InContinue);
			}
		}
		public RelayCommand PauseCommand {
			get { return GetRelayCommand(debugger.Pause, () => debugger.InContinue); }
		}

		public MainViewModel() {
			Stream defitionStream = Assembly.GetExecutingAssembly().
				GetManifestResourceStream("StackLang.Ide.Content.StackLangSyntaxHighlighting.xshd");
			if (defitionStream == null) {
				throw new ApplicationException("Definition not found in resources.");
			}

			stackLangSyntaxDefinition = HighlightingLoader.Load(new XmlTextReader(defitionStream), new HighlightingManager());

			EditorTabViewModels.CollectionChanged += OnEditorTabsCollectionChanged;
			AddTab(new EditorTabViewModel(stackLangSyntaxDefinition));

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
			EditorTabViewModel viewModel = (EditorTabViewModel)s;
			if (viewModel.InDebug) {
				debugger.Abort();
				outputAreaModel.WriteLine("Cannot close the tab currently debugged.");
				return;
			}

			EditorTabViewModels.Remove(viewModel);
		}

		void OpenTab() {
			EditorTabViewModel newTab;
			try {
				newTab = EditorTabViewModel.Open(stackLangSyntaxDefinition);
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
			debugger.Breakpoints = SelectedTabViewModel.Breakpoints;

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