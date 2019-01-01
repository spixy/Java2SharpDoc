using System.ComponentModel;

namespace Java2SharpDoc.Models
{
	public class ViewModel : INotifyPropertyChanged
	{
		private string _inputText;
		private string _outputDocText;
		private string _outputAttribText;

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

		public string OutputDocText
		{
			get => _outputDocText;
			set
			{
                _outputDocText = value;
				OnPropertyChanged(nameof(OutputDocText));
			}
		}

	    public string OutputAttribText
	    {
	        get => _outputAttribText;
	        set
	        {
                _outputAttribText = value;
	            OnPropertyChanged(nameof(OutputAttribText));
	        }
	    }

        private void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
