using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Json;
using Archipelago.MultiClient.Net.Models;
using UnityEngine;

namespace ClientPlugin;

class ArchipelagoState : Ticker {
	public static ArchipelagoState current;

	readonly ArchipelagoSession session;
	DeathLinkService deathLink;
	readonly bool[] unlockedObjects = new bool[0x24];
	readonly bool[] unlockedUpgrades = new bool[0x4B];
	readonly bool[] unlockedGameModes = new bool[6];
	public readonly Dictionary<Upgrade, int> baseLevels = [];
	public readonly string savePrefix;
	public ArchipelagoSave save = null;
	public readonly ArchipelagoSlotData slotData;
	public int Me => session.Players.ActivePlayer.Slot;
	public bool deathLinkEnabled;
	public bool downboostUnlocked = false;
	public bool beachUnlocked = false;
	public bool rerollingUnlocked = false;
	public bool fusionUnlocked = false;
	public bool shopUnlocked = false;
	public bool labUnlocked = false;
	public bool deathLinkConnected = false;
	public int asteroidHitTime = 0;
	bool shownDisconnectMessage = false;
	bool initialChecksSent = false;
	bool isGoal = false;
	volatile bool incomingDeathLink = false;
	volatile DeathLink incomingDeathLinkData;
	PlayScreenManager playScreenManager;
	PlayUIHandler playUIHandler;
	FeverZonesHandler feverZonesHandler;
	FeverZoneBarUI feverZoneBarUI;

	public ArchipelagoState(ArchipelagoSession session, JObject slotData) {
		this.session = session;
		this.slotData = slotData.ToObject<ArchipelagoSlotData>();
		savePrefix = $"archipelago-{this.slotData.gameId}-";
		deathLinkEnabled = this.slotData.deathLink;

		unlockedObjects[(int)PoolableType.Tree] = true;
		unlockedObjects[(int)PoolableType.Bush] = true;
		unlockedObjects[(int)PoolableType.Soda] = true;
		unlockedObjects[(int)PoolableType.NONE] = true;
		unlockedObjects[(int)PoolableType.BaeOcean] = true;
		unlockedObjects[(int)PoolableType.DoubleSoda] = true;
		unlockedObjects[(int)PoolableType.TripleSoda] = true;
		unlockedObjects[(int)PoolableType.LargeSoda] = true;
		unlockedObjects[(int)PoolableType.Elizabeth] = true;
		unlockedObjects[(int)PoolableType.Cecilia] = true;
		unlockedObjects[(int)PoolableType.Gigi] = true;
		unlockedObjects[(int)PoolableType.Raora] = true;

		unlockedUpgrades[(int)Upgrade.FreeCharge] = true;

		unlockedGameModes[(int)GameMode.Standard] = true;

		Plugin.AddTicker(new PopupHandler());
	}

	public override void Tick() {
		if (save == null)
			return;
		
		if (!initialChecksSent) {
			initialChecksSent = true;
			Locations.Scout(session);
			if (deathLinkEnabled = save.deathLink ?? deathLinkEnabled) {
				deathLink = session.CreateDeathLinkService();
				deathLink.OnDeathLinkReceived += data => {
					incomingDeathLinkData = data;
					incomingDeathLink = true;
				};
			}
		}

		if (!session.Socket.Connected && !shownDisconnectMessage && Plugin.archipelagoIcon) {
			shownDisconnectMessage = true;
			PopupHandler.Popup("Disconnected", "Lost connection to Archipelago. Restart game to reconnect.", Plugin.archipelagoIcon, UnityEngine.Color.red);
		}

		while (session.Items.Any()) {
			var item = session.Items.DequeueItem();
			Items.HandleIncoming(item);
		}

		if (asteroidHitTime > 0)
			asteroidHitTime--;

		if (!downboostUnlocked) {
			if (playScreenManager) {
				playScreenManager.downCharge = 0;
			} else {
				playScreenManager = Component.FindObjectOfType<PlayScreenManager>();
			}

			if (!playUIHandler && (playUIHandler = Component.FindObjectOfType<PlayUIHandler>())) {
				playUIHandler.downChargeText.transform.localScale = Vector3.zero;
			}
		} else if (playUIHandler) {
			playUIHandler.downChargeText.transform.localScale = Vector3.one;
			playUIHandler = null;
		}

		if (!beachUnlocked) {
			if (feverZonesHandler) {
				feverZonesHandler.nextFeverStartPos = feverZonesHandler.playerPos + feverZonesHandler.firstFeverDistance;
				feverZonesHandler.lastFeverEndPos = feverZonesHandler.playerPos;
			} else {
				feverZonesHandler = Component.FindObjectOfType<FeverZonesHandler>();
			}

			if (!feverZoneBarUI && (feverZoneBarUI = Component.FindObjectOfType<FeverZoneBarUI>())) {
				feverZoneBarUI.transform.localScale = Vector3.zero;
			}
		} else if (feverZoneBarUI) {
			feverZoneBarUI.transform.localScale = Vector3.one;
			feverZoneBarUI = null;
		}

		if (incomingDeathLink) {
			incomingDeathLink = false;

			if (deathLinkConnected) {
				deathLinkConnected = false;

				var player = Component.FindObjectOfType<Player>();
				if (player) {
					var pause = Component.FindObjectOfType<PauseCanvasHandler>();
					if (pause.pauseCanvasGr.alpha == 1) {
						deathLinkConnected = incomingDeathLink = true;
					} else {
						Plugin.L("Handling normal mode death link");
						PopupHandler.Popup(incomingDeathLinkData);
						if (!slotData.rebirthBlocksDeathLink)
							ShopManager.Instance.hasRebirth = false;
						var jailTime = Component.FindObjectOfType<JailStopwatch>();
						if (jailTime)
							jailTime.timeElapsed = 2e7f;
						player.PlayerHitTree(ShopManager.Instance.hasRebirth);
						PlayScreenManager.Instance.currentRunBaeHits--;
					}
				}

				else {
					var spacePlayer = Component.FindObjectOfType<SpaceIrys>();

					if (spacePlayer) {
						var pause = Component.FindObjectOfType<SpacePause>();
						if (pause.isShowing) {
							deathLinkConnected = incomingDeathLink = true;
						} else {
							Plugin.L("Handling space mode death link");
							PopupHandler.Popup(incomingDeathLinkData);
							spacePlayer.fuelLeft = 0;
							spacePlayer.OnFuelChanged.Invoke(spacePlayer, new() {
								value = 0,
								maxValue = spacePlayer.maxFuel
							});
						}
					}

					else {
						Plugin.L("Ignoring death link");
					}
				}
			}
		}

		Gamesave saveData;
		if (
			!isGoal &&
			save.bestDistanceStandard >= slotData.goalStandardDistance &&
			save.bestDistanceClassic >= slotData.goalClassicDistance &&
			save.bestDistanceBaeless >= slotData.goalBaelessDistance &&
			save.bestDistanceGacha >= slotData.goalGachaDistance &&
			(saveData = GamesaveHandler.Instance.SaveData).PlayStats[(int)SaveStat.SpacePoints] >= slotData.goalSpaceScore &&
			(
				slotData.goalCellTime >= 30000 || (
					saveData.PlayStats[(int)SaveStat.BestTimeJailx100] > 0 &&
					saveData.PlayStats[(int)SaveStat.BestTimeJailx100] <= slotData.goalCellTime
				)
			) &&
			save.bestDistanceSpace >= slotData.goalSpaceDistance &&
			slotData.goalAchievements.All(GamesaveHandler.Instance.GetAchievement)
		) {
			isGoal = true;
			Plugin.L("Goal!");
			PopupHandler.Popup(
				"Goal!",
				"You have reached your goal",
				Plugin.archipelagoIcon,
				new(1f, 0f, 0.5f)
			);
			try {
				session.SetGoalAchieved();
			} catch (ArchipelagoSocketClosedException) {}
		}

		if (save.dirty) {
			save.dirty = false;
			GamesaveHandler.Instance.saveReadWriter.WriteSave(GamesaveHandler.Instance.saveData);
		}
	}

	public void ViewShop() {
		try {
			session.Locations.ScoutLocationsAsync(
				HintCreationPolicy.CreateAndAnnounceOnce,
				[.. session.Locations.AllLocations.Where(id => id >> 8 == Locations.LOCATION_TYPE_SHOP)]
			);
		}
		catch (ArchipelagoSocketClosedException) {}
	}

	public void ViewSpace() {
		try {
			session.Locations.ScoutLocationsAsync(
				HintCreationPolicy.CreateAndAnnounceOnce,
				[.. session.Locations.AllLocations.Where(id => id >> 8 == Locations.LOCATION_TYPE_SPACE)]
			);
		}
		catch (ArchipelagoSocketClosedException) {}
	}

	public void Check(long location) {
		try {
			session.Locations.CompleteLocationChecks(location);
		}
		catch (ArchipelagoSocketClosedException) {}
	}

	public void Unlock(PoolableType objectType) => unlockedObjects[(int)objectType] = true;
	public bool Unlocked(PoolableType objectType) => unlockedObjects[(int)objectType];
	public void Unlock(Upgrade upgrade) => unlockedUpgrades[(int)upgrade] = true;
	public bool Unlocked(Upgrade upgrade) => unlockedUpgrades[(int)upgrade];
	public void Unlock(GameMode mode) => unlockedGameModes[(int)mode] = true;
	public bool Unlocked(GameMode mode) => unlockedGameModes[(int)mode];

	public bool Checked(long location) => session.Locations.AllLocationsChecked.Contains(location);

	public void FlushStoneQueue() {
		if (Plugin.spaceItemMode == SpaceItemMode.Sent && save.queuedHopeStones > 0) {
			GamesaveHandler.Instance.SetCurrency(GamesaveHandler.Instance.GetCurrency() + save.queuedHopeStones, false);
			save.queuedHopeStones = 0;
			save.dirty = true;
		}
	}

	static readonly string[][] deathLinkReasonPools = [
		[
			"came to a stop",
			"ran out of hope",
			"used up their soda reserves"
		],
		[
			"hit Bae",
			"did not have a strawberry",
			"didn't file for divorce in time",
			"left their rat repellent at home"
		],
		[
			"ran out of fuel",
			"fell back to Earth",
			"descended"
		],
		[
			"hit an asteroid",
			"did not roll a nat 3720",
			"tried to break a rock with their head"
		]
	];

	public void SendDeathLink(DeathReason reason) {
		deathLinkConnected = false;
		Plugin.L($"Sending death link ({reason})");
		var pool = deathLinkReasonPools[(int)reason];
		try {
			deathLink.SendDeathLink(
				new(session.Players.ActivePlayer.Name, $"{session.Players.ActivePlayer.Name} {pool[UnityEngine.Random.Range(0, pool.Length)]}"));
		}
		catch (ArchipelagoSocketClosedException) {}
	}
}