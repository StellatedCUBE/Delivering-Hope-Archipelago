using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClientPlugin;

class ArchipelagoConnectUI : Ticker {
	const bool SKIP = false;

	readonly static string[] tags = ["DeathLink"];
	readonly List<GameObject> deactivated = [];
	readonly GamesaveHandler gsh;
	readonly NamedPipeServerStream server;
	readonly Process ui;
	readonly List<byte> message = [];
	readonly Transform canvas;
	readonly TMP_FontAsset font;
	readonly string configFile = Path.Combine(Application.persistentDataPath, "archipelago.cfg");
	float age = 0;
	bool connected = false;
	bool maybeHasMessage = false;
	bool shownMessage = false;
	byte[] activeRead = null;
	GameObject popupNotice;

	public ArchipelagoConnectUI(GamesaveHandler gsh) {
		this.gsh = gsh;

		StringBuilder pipeNameBuilder = new(12);

		for (int i = 0; i < 12; i++)
			pipeNameBuilder.Append((char)('a' + UnityEngine.Random.Range(0, 25)));

		var pipeName = pipeNameBuilder.ToString() + "_hope";

		server = new(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
		server.BeginWaitForConnection(res => {
			Plugin.L("Pipe established");
			connected = true;
			server.EndWaitForConnection(res);
		}, null);

		foreach (var canvas in Canvas.FindObjectsOfType<Canvas>()) {
			if (canvas.gameObject.name == "LoadingCanvas") {
				this.canvas = canvas.transform;
				font = canvas.GetComponentInChildren<TextMeshProUGUI>().font;
			}

			for (int i = 0; i < canvas.transform.GetChildCount(); i++)
				Deactivate(canvas.transform.GetChild(i).gameObject);
		}
		
		string defaultHost = "archipelago.gg";
		string defaultPort = "38281";
		string defaultSlot = "";
		string defaultPassword = "";

		try {
			var config = File.ReadAllLines(configFile);
			defaultHost = config[0].Trim();
			defaultPort = config[1].Trim();
			defaultSlot = config[2].Trim();
			defaultPassword = config[3].Trim();
		} catch {}

		string exe = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ConnectUI.exe");

		if (!SKIP && File.Exists(exe)) {
			ui = Process.Start(
				exe, $@"{pipeName} /{defaultHost} {defaultPort} /{
					Convert.ToBase64String(Encoding.UTF8.GetBytes(defaultSlot))} /{Convert.ToBase64String(Encoding.UTF8.GetBytes(defaultPassword))}"
			);
		} else {
			shownMessage = true;
			popupNotice = new();
			popupNotice.transform.SetParent(canvas, false);
			var tmp = popupNotice.AddComponent<TextMeshProUGUI>();
			tmp.SetText("Popup not found.\nCheck if it was eaten\nby your antivirus.");
			tmp.color = Color.white;
			tmp.enableWordWrapping = false;
			tmp.alignment = TextAlignmentOptions.Center;
			tmp.font = font;
		}

		if (SKIP) {
			Plugin.AddTicker(new Debug());
			Plugin.Schedule(() => TryConnect(defaultHost, int.Parse(defaultPort), defaultSlot, defaultPassword), 1);
		}
	}

    public override void Tick() {
		if (SKIP)
			return;

		if (!shownMessage)
        	age += Time.deltaTime;

		if (!connected && age > 120) {
			Plugin.Log.LogFatal("Timeout connecting to Archipelago settings UI");
			Application.Quit();
			return;
		}

		if ((connected && !server.IsConnected) || ui.HasExited) {
			Application.Quit();
			return;
		}

		if (connected && !shownMessage) {
			shownMessage = true;
			popupNotice = new();
			popupNotice.transform.SetParent(canvas, false);
			var tmp = popupNotice.AddComponent<TextMeshProUGUI>();
			tmp.SetText("Continue in popup");
			tmp.color = Color.white;
			tmp.enableWordWrapping = false;
			tmp.alignment = TextAlignmentOptions.Center;
			tmp.font = font;
		}

		if (connected && activeRead == null) {
			activeRead = new byte[64];
			server.BeginRead(activeRead, 0, 64, res => {
				message.AddRange(activeRead.Take(server.EndRead(res)));
				activeRead = null;
				maybeHasMessage = true;
			}, null);
		}

		if (maybeHasMessage) {
			maybeHasMessage = false;
			int length = message.IndexOf(0);
			if (length >= 0) {
				Plugin.L("Received pipe message");

				var items = Encoding.UTF8.GetString([.. message.Take(length)]).Split('\n');
				message.RemoveRange(0, length + 1);

				File.WriteAllLines(configFile, items);
				TryConnect(items[0], int.Parse(items[1]), items[2], items[3] == "" ? null : items[3]);
			}
		}
    }

	void TryConnect(string host, int port, string slot, string password) {
		Plugin.L($"Creating Archipelago session {slot}@{host}:{port}");

		var session = ArchipelagoSessionFactory.CreateSession(host, port);

		session.Socket.ErrorReceived += (error, _) => Plugin.Log.LogError(error);

		LoginResult result;
		try {
			result = session.TryConnectAndLogin("Delivering Hope", slot, ItemsHandlingFlags.AllItems, tags: tags, password: password);
		} catch (Exception e) {
			result = new LoginFailure(e.GetBaseException().Message);
		}

		if (result.Successful) {
			Plugin.L("Archipelago session connected");

			ArchipelagoState.current = new(session, ((LoginSuccessful)result).SlotData);
			Plugin.AddTicker(ArchipelagoState.current);

			Remove();
			if (server.IsConnected)
				server.Disconnect();

			RestoreGame();
		} else {
			var failure = (LoginFailure)result;

			Plugin.L("Archipelago server rejected connection:");
			foreach (var error in failure.Errors)
				Plugin.L("  " + error);

			SendError("Failed to connect to Archipelago room\0");
		}
	}

	void SendError(string error) {
		var buffer = Encoding.UTF8.GetBytes(error);
		server.Write(buffer, 0, buffer.Length);
		server.Flush();
	}

	void Deactivate(GameObject gameObject) {
		if (gameObject.activeSelf) {
			gameObject.active = false;
			deactivated.Add(gameObject);
		}
	}

	void RestoreGame() {
		GameObject.Destroy(popupNotice);

		gsh.Start();

		foreach (var gameObject in deactivated) {
			gameObject.active = true;
		}

		Component.FindObjectOfType<LoadingSceneHandler>().Awake();
		Component.FindObjectOfType<SoundManager>().PlayTitleBGM();
	}
}