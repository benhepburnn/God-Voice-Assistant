using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using God;

namespace God {
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window {

		MainWindow main;

		public SettingsWindow() {
			InitializeComponent();

			main = new MainWindow();

			userName.Text = Properties.Settings.Default.username;
			vrName.Text = Properties.Settings.Default.vrname;
			//vrVoice.SelectedItem = Properties.Settings.Default.voice;
			volumeSlider.Value = Properties.Settings.Default.volume;

			List<string> installedVoices = new List<string>();
			foreach (var v in main.synthesizer.GetInstalledVoices().Select(v => v.VoiceInfo)) {
				installedVoices.Add(v.Name);
			}

			vrVoice.ItemsSource = installedVoices;
			vrVoice.SelectedItem = main.synthesizer.Voice.Name;
			vrVoice.Items.Refresh();
		}

		private void close_Click(object sender, EventArgs e) {
			Properties.Settings.Default.username = userName.Text;
			Properties.Settings.Default.vrname = vrName.Text;
			Properties.Settings.Default.voice = vrVoice.SelectedValue.ToString();
			Properties.Settings.Default.volume = int.Parse(volumeSlider.Value.ToString());

			main.userName = userName.Text;
			main.voiceRecognitionName = vrName.Text.ToLower();
			main.synthesizer.SelectVoice(vrVoice.SelectedItem.ToString());
			main.synthesizer.Volume = int.Parse(volumeSlider.Value.ToString());

			Close();
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}
	}
}
