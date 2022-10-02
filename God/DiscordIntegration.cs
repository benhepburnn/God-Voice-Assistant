using System;
using System.Linq;
using Discord;
using Discord.Audio;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace God {
	class DiscordIntegration {

		private DiscordClient _client;
		private IAudioClient _vClient;

		public DiscordIntegration() { // setup
			_client = new DiscordClient();

			_client.Log.Message += (s, e) => Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}"); // Log errors/info to console
			Debug.WriteLine("New Discord client created");

			_client.UsingAudio(x => // Opens an AudioConfigBuilder so we can configure our AudioService
			{
				x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
			});

			//_client.ExecuteAndWait(async () => {
			Connect();
				//_client.SetGame("oliver is a big faggot");
			//});
		}

		private async Task Connect() {
			await _client.Connect("ENTER TOKEN HERE", TokenType.Bot);

			var voiceChannel = _client.FindServers("gay").FirstOrDefault().FindChannels("suh dude").FirstOrDefault(); // Finds the first VoiceChannel on the server 'Music Bot Server'

			_vClient = await _client.GetService<AudioService>() // We use GetService to find the AudioService that we installed earlier. In previous versions, this was equivelent to _client.Audio()
					.Join(voiceChannel); // Join the Voice Channel, and return the IAudioClient.
		}

		public void PlaySong (string songQuery) {
			Debug.WriteLine("playing song");

			_client.FindServers("gay").FirstOrDefault().TextChannels.FirstOrDefault().SendMessage(";;skip");
			_client.FindServers("gay").FirstOrDefault().TextChannels.FirstOrDefault().SendMessage(";;play " + songQuery);
			_client.FindServers("gay").FirstOrDefault().TextChannels.FirstOrDefault().SendMessage(";;play 1");
			_client.FindServers("gay").FirstOrDefault().TextChannels.FirstOrDefault().SendMessage(";;join");
		}
	}
}
