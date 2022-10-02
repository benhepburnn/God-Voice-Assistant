using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;

using System.Diagnostics;

using System.Speech.Recognition;
using System.Speech.Synthesis;

using System.Media;

using iTunesLib;

using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;

namespace God {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		public string voiceRecognitionName = Properties.Settings.Default.vrname.ToLower();
		public string userName = Properties.Settings.Default.username;
		//string voice = Properties.Settings.Default.voice;

		SpeechRecognitionEngine recognition;
		public SpeechSynthesizer synthesizer;

		int indexOfMainGrammar;

		string[] recognizedWords;
		List<string> allSongNames;

		bool recognitionEnabled = false;
		string recognitionOnSound = @"../../resource/audio/recognition_on.wav";
		string recognitionOffSound = @"../../resource/audio/recognition_off.wav";
		string recognitionUnderstoodSound = @"../../resource/audio/recognition_understood.wav";
		string recognitionNotUnderstoodSound = @"../../resource/audio/recognition_not_understood.wav";

		string appIconPath = @"../../resource/img/appIcon.ico";

		SoundPlayer soundPlayer;

		iTunesApp iTunes;
		IITTrackCollection allITunesTracks;

		DiscordIntegration discord;

		public MainWindow() {
			InitializeComponent();

			//discord = new DiscordIntegration("", 0);

			iTunes = new iTunesApp();

			//get all songs
			allITunesTracks = iTunes.LibraryPlaylist.Tracks;

			//get list of every song name
			allSongNames = new List<string>();

			foreach (IITTrack currTrack in allITunesTracks) {
				allSongNames.Add(RemoveSpecialCharacters(currTrack.Name.ToLower()));
			}

			iTunes.Quit();

			NotifyIcon appIcon = new NotifyIcon();
			appIcon.Icon = new System.Drawing.Icon(appIconPath);
			appIcon.Visible = true;
			appIcon.DoubleClick += delegate (object sender, EventArgs e) {
				this.Show();
				this.WindowState = WindowState.Normal;
			};

			//voice recognition

			soundPlayer = new SoundPlayer();

			synthesizer = new SpeechSynthesizer();
			synthesizer.Rate = -2;
			synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
			synthesizer.Volume = Properties.Settings.Default.volume;

			//detect installed voices
			foreach (var v in synthesizer.GetInstalledVoices().Select(v => v.VoiceInfo)) {
				Debug.WriteLine("Name:{0}, Gender:{1}, Age:{2}",
				  v.Description, v.Gender, v.Age);
			}

			recognition = new SpeechRecognitionEngine(new CultureInfo("en-AU"));

			recognizedWords = new string[] { "hi", "hey", "hello", "play", "itunes", "music", "in", "discord", "bye", "goodbye" };
			string[] allWords = recognizedWords;
			allWords = allWords.Concat(allSongNames).ToArray();

			Choices heyGod = new Choices();
			heyGod.Add("hey " + voiceRecognitionName);
			recognition.LoadGrammar(new Grammar(new GrammarBuilder(heyGod, 0, 1)));

			Choices wordChoices = new Choices();
			//wordChoices.Add(recognizedWords);
			wordChoices.Add(allWords);
			Grammar mainGrammar = new Grammar(new GrammarBuilder(wordChoices, 0, 10));
			recognition.LoadGrammar(mainGrammar);
			indexOfMainGrammar = recognition.Grammars.IndexOf(mainGrammar);
			recognition.Grammars[indexOfMainGrammar].Enabled = false;

			/*Choices songTitles = new Choices();
			songTitles.Add(allSongNames.ToArray());
			Grammar songTitlesGrammar = new Grammar(new GrammarBuilder(songTitles, 0, 1));
			recognition.LoadGrammar(songTitlesGrammar);*/

			//recognition.LoadGrammar(new DictationGrammar());

			recognition.SetInputToDefaultAudioDevice();
			recognition.RecognizeAsync(RecognizeMode.Multiple);

			recognition.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sr_SpeechRecognized);
			recognition.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sr_SpeechRecognitionRejected);

			//Debug

			Debug.WriteLine("Voice Recognition Name: " + voiceRecognitionName);
			Debug.WriteLine("User Name: " + userName);
			Debug.WriteLine("[{0}]", string.Join(", ", allWords));
		}

		//mic icon

		private void micIcon_Click (object sender, EventArgs e) {
			if (!recognitionEnabled) {
				soundPlayer.SoundLocation = recognitionOnSound;
				soundPlayer.Play();

				recognition.Grammars[indexOfMainGrammar].Enabled = true;
				recognitionEnabled = true;
			} else if (recognitionEnabled) {
				soundPlayer.SoundLocation = recognitionOffSound;
				soundPlayer.Play();

				recognition.Grammars[indexOfMainGrammar].Enabled = false;
				recognitionEnabled = false;
			}
		}

		// voice recognition

		private async void sr_SpeechRecognized (object sender, SpeechRecognizedEventArgs speech) {
			//conversationStack.Children.Add(new TextBlock { Text = speech.Result.Text, TextAlignment = TextAlignment.Right, FontSize = 16, FontFamily = new FontFamily("Segoe UI Light"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.White });
			//conversationScrollPanel.ScrollToBottom();

			if (!recognitionEnabled) {
				if (speech.Result.Text == "hey god") {
					conversationStack.Children.Add(new TextBlock { Text = speech.Result.Text, TextAlignment = TextAlignment.Right, FontSize = 16, FontFamily = new FontFamily("Segoe UI Light"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.White });
					conversationScrollPanel.ScrollToBottom();

					recognition.Grammars[indexOfMainGrammar].Enabled = true;
					recognitionEnabled = true;

					synthesizer.SpeakAsyncCancelAll();

					soundPlayer.SoundLocation = recognitionOnSound;
					soundPlayer.Play();
				}
			} else {
				conversationStack.Children.Add(new TextBlock { Text = speech.Result.Text, TextAlignment = TextAlignment.Right, FontSize = 16, FontFamily = new FontFamily("Segoe UI Light"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.White });
				conversationScrollPanel.ScrollToBottom();

				soundPlayer.SoundLocation = recognitionUnderstoodSound;
				soundPlayer.Play();

				await Task.Delay(600);

				string responseSpoken = "";
				string responseText = "";

				if (speech.Result.Text == "hi" || speech.Result.Text == "hello" || speech.Result.Text == "hey") {

					responseSpoken = "hello";
					responseText = "Hello.";
				} else if (speech.Result.Text == "bye" || speech.Result.Text == "goodbye") {

					responseSpoken = "seeya";
					responseText = "Seeya.";
				} else if (speech.Result.Text == "play" || speech.Result.Text == "play itunes" || speech.Result.Text == "play music") {
					iTunes = new iTunesApp();
					iTunes.Play();

					responseText = "Playing music...";
				} else if (speech.Result.Text != "play" && speech.Result.Text.Contains("play")) {
					iTunes = new iTunesApp();

					//get all songs
					allITunesTracks = iTunes.LibraryPlaylist.Tracks;

					//find desired song
					List<IITTrack> songsWithMatchingWords = new List<IITTrack>();
					bool foundSong = false;
					foreach (IITTrack currTrack in allITunesTracks) {
						if (speech.Result.Text.Contains(RemoveSpecialCharacters (currTrack.Name.ToLower()))) {
							foundSong = true;
							songsWithMatchingWords.Add(currTrack);
						}
					}

					IITTrack trackToPlay = null;
					if (songsWithMatchingWords.Count == 1) {
						trackToPlay = songsWithMatchingWords[0];
					} else {
						//string trimmedResult = Regex.Replace(speech.Result.Text, "play ", "", RegexOptions.Compiled);
						//trimmedResult = Regex.Replace(trimmedResult, " in discord", "", RegexOptions.Compiled);
						int i = 1000;
						foreach (IITTrack currTrack in songsWithMatchingWords) {
							if (LevenshteinDistance(speech.Result.Text, currTrack.Name) < i) {
								trackToPlay = currTrack;
								i = LevenshteinDistance(speech.Result.Text, currTrack.Name);
							}
						}
					}

					if (foundSong == false) {
						responseSpoken = "song not found";
						responseText = "Song not found.";
					} else {
						if (speech.Result.Text.Contains("in discord")) {
							responseText = "Playing " + trackToPlay.Name + " in Discord...";

							//discord
							discord = new DiscordIntegration();
							Thread.Sleep(4000);
							discord.PlaySong(trackToPlay.Name + " " + trackToPlay.Artist);
						} else {
							responseText = "Playing " + trackToPlay.Name + "...";
							trackToPlay.Play();
						}
					}
				}

				/*else {
					synthesizer.SpeakAsync("i don't know what the fuck you just said mate");
					conversationStack.Children.Add(new TextBlock { Text = "I don't know what the fuck you just said mate.", TextAlignment = TextAlignment.Left, FontSize = 16, FontFamily = new FontFamily("Segoe UI"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.White });
				}*/

				recognition.Grammars[indexOfMainGrammar].Enabled = false;
				recognitionEnabled = false;

				//refresh voice recognition

				recognition.SpeechRecognized -= new EventHandler<SpeechRecognizedEventArgs>(sr_SpeechRecognized);
				recognition.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sr_SpeechRecognized);

				synthesizer.SpeakAsync(responseSpoken);
				conversationStack.Children.Add(new TextBlock { Text = responseText, TextAlignment = TextAlignment.Left, FontSize = 16, FontFamily = new FontFamily("Segoe UI"), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.White });
				conversationScrollPanel.ScrollToBottom();

				if (speech.Result.Text == "hey god") {
					synthesizer.SpeakAsyncCancelAll();

					soundPlayer.SoundLocation = recognitionOnSound;
					soundPlayer.Play();
					recognition.Grammars[indexOfMainGrammar].Enabled = true;
					recognitionEnabled = true;
				}
			}
		}

		private void sr_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs speech) {
			if (recognitionEnabled) {
				soundPlayer.SoundLocation = recognitionNotUnderstoodSound;
				soundPlayer.Play();
			}
		}

		//end voice recognition

		public static string RemoveSpecialCharacters(string str) {
			string editedString;
			editedString = Regex.Replace(str, "[^a-zA-Z0-9 &]+", "", RegexOptions.Compiled);
			editedString = Regex.Replace(editedString, "[&]", "and", RegexOptions.Compiled);
			return editedString;

		}

		public static int LevenshteinDistance (string s, string t) {
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if (n == 0) {
				return m;
			}

			if (m == 0) {
				return n;
			}

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++) {
			}

			for (int j = 0; j <= m; d[0, j] = j++) {
			}

			// Step 3
			for (int i = 1; i <= n; i++) {
				//Step 4
				for (int j = 1; j <= m; j++) {
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					// Step 6
					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}

		private void close_Click (object sender, EventArgs e) {
			Hide();
		}

		//options

		private void options_Click (object sender, EventArgs e) {
			SettingsWindow settingsWindow = new SettingsWindow();
			settingsWindow.Show();
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}
	}
}
