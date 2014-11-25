﻿using System;
using System.IO;
using System.Threading;

namespace StackLang.Debugger {
	static class DebuggerProgram {
		static void Main(string[] args) {
			Stream stream = new FileStream((args.Length == 0 ? "defaultCode.sl" : args[0]), FileMode.Open);

			Debugger debugger = new Debugger(stream);

			debugger.Load();

			while (!debugger.ExecutionEnded) {
				debugger.Snapshot();
				debugger.Print();

				Console.WriteLine("Awaiting key. (enter to step, 'h' for help)");
				ConsoleKeyInfo keyInfo = Console.ReadKey(true);

				if (ProcessKey(keyInfo.Key, debugger)) {
					continue;
				}

				debugger.Step();
			}

			Console.WriteLine("Execution ended.");
		}

		static bool ProcessKey(ConsoleKey key, Debugger debugger) {
			if (key == ConsoleKey.Enter) {
				return false;
			}
			if (key == ConsoleKey.H) {
				Console.WriteLine("Valid keys:\n" +
				                  "- enter: step into the current instruction\n" +
				                  "- b: go to the breakpoint menu\n" +
								  "- c: continue until execution ended or breakpoint hit\n" +
				                  "- h: show this help information");
			}
			if (key == ConsoleKey.B) {
				ShowBreakpointMenu(debugger);
			}
			if (key == ConsoleKey.C) {
				Continue(debugger);
				return true;
			}
			return true;
		}

		static void Continue(Debugger debugger) {
			Console.WriteLine("Continuing execution. Press Ctrl + P to pause.");
			Thread pauseThread = new Thread(PauseThreadStart);
			pauseThread.Start(debugger);
			debugger.Continue();

			pauseThread.Abort();
		}

		static void PauseThreadStart(object debuggerObj) {
			Debugger debugger = (Debugger) debuggerObj;
			while (true) {
				lock (debugger.ExecutionLock) {
					if (Console.KeyAvailable) {
						ConsoleKeyInfo info = Console.ReadKey(true);
						if (info.Key == ConsoleKey.P && (info.Modifiers & ConsoleModifiers.Control) != 0) {
							break;
						}
					}
				}
			}
			debugger.RequestPause();
		}

		static void ShowBreakpointMenu(Debugger debugger) {
			while (true) {
				Console.WriteLine("Enter command (line to toggle breakpoint or 'h' for help)");
				string line = Console.ReadLine();
				if (string.IsNullOrEmpty(line)) {
					continue;
				}
				if (line == "h") {
					Console.WriteLine("Valid commands:\n" +
					                  "- number: line to toggle breakpoint at\n" +
					                  "- list: list all breakpoints\n" +
					                  "- back: return to previous menu");
					continue;
				}
				if (line == "list") {
					debugger.ListBreakpoints();
					continue;
				}
				if (line == "back") {
					return;
				}

				int breakpointLine;
				if (int.TryParse(line, out breakpointLine)) {
					debugger.ToggleBreakpoint(breakpointLine);
					return;
				}
			}
		}
	}
}
