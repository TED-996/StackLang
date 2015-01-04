using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace StackLang.Ide.MVVMEnhancements {
	public class ViewModelBaseEnhanced : ViewModelBase {
		readonly Dictionary<ActionFuncPair, RelayCommand> relayCommands = new Dictionary<ActionFuncPair, RelayCommand>();

		public RelayCommand GetRelayCommand(Action execute, Func<bool> canExecute = null) {
			canExecute = canExecute ?? (() => true);
			ActionFuncPair pair = new ActionFuncPair(execute, canExecute);
			if (relayCommands.ContainsKey(pair)) {
				return relayCommands[pair];
			}
			relayCommands.Add(pair, new RelayCommand(execute, canExecute));
			return relayCommands[pair];
		}

		struct ActionFuncPair {
			public readonly Action Action;
			public readonly Func<bool> Func;

			public ActionFuncPair(Action newAction, Func<bool> newFunc) {
				Action = newAction;
				Func = newFunc;
			}
		}
	}
}