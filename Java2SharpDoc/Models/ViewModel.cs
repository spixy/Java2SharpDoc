using System.ComponentModel;

namespace Java2SharpDoc.Models
{
	public class ViewModel : INotifyPropertyChanged
	{
		private string _inputText;
		private string _outputText;

		public event PropertyChangedEventHandler PropertyChanged;

		public string InputText
		{
			get => _inputText;
			set
			{
				_inputText = value;
				OnPropertyChanged(nameof(InputText));
			}
		}

		public string OutputText
		{
			get => _outputText;
			set
			{
				_outputText = value;
				OnPropertyChanged(nameof(OutputText));
			}
		}

		private void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
