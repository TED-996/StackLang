﻿using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace StackLang.Ide.ViewModel {
	public class ViewModelLocator {
		static ViewModelLocator() {
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

			SimpleIoc.Default.Register<MainViewModel>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
			"CA1822:MarkMembersAsStatic",
			Justification = "This non-static member is needed for data binding purposes.")]
		public MainViewModel Main {
			get {
				return ServiceLocator.Current.GetInstance<MainViewModel>();
			}
		}

		public static void Cleanup() {
		}
	}
}