using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;

namespace StackLang.Ide.Helpers {
	public sealed class AvalonEditBehavior : Behavior<TextEditor> {
		public static readonly DependencyProperty BindableTextProperty =
			DependencyProperty.Register("BindableText", typeof(string), typeof(AvalonEditBehavior),
			new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TextPropertyChangedCallback));

		public string BindableText {
			get { return (string)GetValue(BindableTextProperty); }
			set { SetValue(BindableTextProperty, value); }
		}

		protected override void OnAttached() {
			base.OnAttached();
			if (AssociatedObject != null) {
				AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
			}
		}

		protected override void OnDetaching() {
			base.OnDetaching();
			if (AssociatedObject != null) {
				AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
			}
		}

		void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs) {
			var textEditor = sender as TextEditor;
			if (textEditor != null) {
				if (textEditor.Document != null)
					BindableText = textEditor.Document.Text;
			}
		}

		static void TextPropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e) {
			TextEditor editor = ((AvalonEditBehavior)o).AssociatedObject;
			if (editor != null && editor.Document != null) {
				int caretOffset = editor.CaretOffset;
				editor.Document.Text = e.NewValue.ToString();
				editor.CaretOffset = caretOffset;
			}
		}
	}

	public static class MvvmAvalonMakeover {
		public static readonly DependencyProperty LineTransformersProperty = DependencyProperty.RegisterAttached(
			"LineTransformers", typeof (IList<DocumentColorizingTransformer>), typeof (MvvmAvalonMakeover),
			new PropertyMetadata(default(IList<DocumentColorizingTransformer>), LineTransformersChangedCallback));

		static void LineTransformersChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e) {
			TextEditor editor = (TextEditor) o;
			if (editor != null && editor.Document != null) {
				IList<IVisualLineTransformer> originalTransformers = editor.TextArea.TextView.LineTransformers;
				foreach (DocumentColorizingTransformer transformer in ((IList<DocumentColorizingTransformer>)e.NewValue).
					Where(transformer => !originalTransformers.Contains(transformer))) {
					originalTransformers.Add(transformer);
				}
			}
		}

		public static void SetLineTransformers(DependencyObject element, IList<DocumentColorizingTransformer> value) {
			element.SetValue(LineTransformersProperty, value);
		}

		public static IList<DocumentColorizingTransformer> GetLineTransformers(DependencyObject element) {
			return (IList<DocumentColorizingTransformer>) element.GetValue(LineTransformersProperty);
		}
	}
}