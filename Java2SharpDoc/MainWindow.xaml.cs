using System.ComponentModel;
using System.Windows;
using Java2SharpDoc.Models;
using Java2SharpDoc.Services;

namespace Java2SharpDoc
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly Converter converter;
		private readonly ViewModel viewModel;

		public MainWindow()
		{
			InitializeComponent();
			
			converter = new Converter();
			viewModel = new ViewModel();
			viewModel.PropertyChanged += ModelOnPropertyChanged;

			DataContext = viewModel;
		}

		private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(ViewModel.InputText))
			{
				return;
			}

			(string doc, string attrib) = converter.Convert(viewModel.InputText);
		    viewModel.OutputDocText = doc;
		    viewModel.OutputAttribText = attrib;
		}
	}
}
