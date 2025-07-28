using System;
using System.Linq;
using Archipelago.MultiClient.Net.Json;
using EncryptString;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ClientPlugin;

static class HarmonyPatches {
	[HarmonyPatch(typeof(GameManager), "Update")]
	[HarmonyPrefix]
	static void Tick() => Plugin.Tick();

	[HarmonyPatch(typeof(SpawnSequencer), "GetNextObstacle")]
	[HarmonyPrefix]
	static bool ObjectSpawner(SpawnSequencer __instance, ref SpawnSequencer.SequenceItem __result, int chunkNumber, GameZone zone) {

		if (!Plugin.o1pd) {
			Plugin.o1pd = true;
			Plugin.On1Play();
		}

		Plugin.SetSpaceItemMode(SpaceItemMode.Received);

		var mode = GameManager.Instance.CurrentGameMode;
		if (
			(
				mode == GameMode.Classic &&
				ArchipelagoState.current.Unlocked(PoolableType.Banana) &&
				ArchipelagoState.current.Unlocked(PoolableType.Booster)
			) || (
				mode == GameMode.Baeless &&
				ArchipelagoState.current.Unlocked(PoolableType.Banana) &&
				ArchipelagoState.current.Unlocked(PoolableType.Booster) &&
				ArchipelagoState.current.Unlocked(PoolableType.Mumei) &&
				ArchipelagoState.current.Unlocked(PoolableType.Kronii) &&
				ArchipelagoState.current.Unlocked(PoolableType.Kiara)
			) || (
				(mode == GameMode.Standard || mode == GameMode.Chaos) &&
				zone == GameZone.Normal &&
				ArchipelagoState.current.Unlocked(PoolableType.Banana) &&
				ArchipelagoState.current.Unlocked(PoolableType.Booster) &&
				ArchipelagoState.current.Unlocked(PoolableType.Mumei) &&
				ArchipelagoState.current.Unlocked(PoolableType.Kronii) &&
				ArchipelagoState.current.Unlocked(PoolableType.Kiara) &&
				ArchipelagoState.current.Unlocked(PoolableType.Strawberry)
			) || (
				zone == GameZone.Ocean &&
				ArchipelagoState.current.Unlocked(PoolableType.Banana) &&
				ArchipelagoState.current.Unlocked(PoolableType.Fauna) &&
				ArchipelagoState.current.Unlocked(PoolableType.Gura) &&
				ArchipelagoState.current.Unlocked(PoolableType.Ina) &&
				ArchipelagoState.current.Unlocked(PoolableType.Kiara) &&
				ArchipelagoState.current.Unlocked(PoolableType.Strawberry)
			)
		) {
			return true;
		}

		__instance.AdjustDifficultyParams(chunkNumber);

		__result = new() {
			SpawnIndex = __instance.spawnedObjectCount,
			SpawnType = Spawner.Next(__instance, mode, zone)
		};

		__instance.spawnedObjectCount++;

		return false;
	}

	[HarmonyPatch(typeof(SpawnSequencer), "GetNextAerialObstacle")]
	[HarmonyPostfix]
	static void AerialObjectSpawner(ref PoolableType __result) {
		if (!ArchipelagoState.current.Unlocked(__result))
			__result = PoolableType.NONE;
	}

	[HarmonyPatch(typeof(GamesaveHandler), "Start")]
	[HarmonyPrefix]
	static bool InterceptGameLoad(GamesaveHandler __instance) {
		if (ArchipelagoState.current == null) {
			try {
				Plugin.AddTicker(new ArchipelagoConnectUI(__instance));
			} catch (Exception err) {
				Plugin.Log.LogFatal(err);
				Application.Quit();
			}

			return false;
		}

		GameManager.Instance.debugUnlockModes = true;
		Plugin.LoadData();
		return true;
	}

    [HarmonyPatch(typeof(GamesaveHandler), "Start")]
    [HarmonyPostfix]
    static void DontMessUpDavidsStats() {
		if (ArchipelagoState.current != null) {
			ArchipelagoState.current.save ??= new();
			Component.FindObjectOfType<PrivacySettingManager>().SetConsentDataCollection(false);
		}
	}

	[HarmonyPatch(typeof(SettingsPanelHandlerV5), "ShowPrivacyPage")]
    [HarmonyPrefix]
    static bool CancelCall() => false;

	[HarmonyPatch(typeof(SoundManager), "PlayTitleBGM")]
	[HarmonyPrefix]
	static bool NoBGMDuringLogin() => ArchipelagoState.current != null;

	static void UpdateSaveFile(ref string path) {
		path = Path.Combine(
			Path.GetDirectoryName(path),
			ArchipelagoState.current.savePrefix + Path.GetFileName(path)
		);
	}

	[HarmonyPatch(typeof(File), "WriteAllText")]
	[HarmonyPrefix]
	static void UpdateSaveFile1(ref string path, ref string contents) {
		if (path.StartsWith(Application.persistentDataPath) && !path.Contains(ArchipelagoState.current.savePrefix)) {
			if (path.EndsWith("savedata.ddd") && ArchipelagoState.current.save != null) {
				var jo = JObject.FromJSON(contents);
				jo.Merge(JObject.FromObject(ArchipelagoState.current.save, true));
				contents = jo.ToJSON();
			}

			UpdateSaveFile(ref path);
		}
	}

	[HarmonyPatch(typeof(File), "ReadAllText")]
	[HarmonyPatch(typeof(File), "Exists")]
	[HarmonyPrefix]
	static void UpdateSaveFile2(ref string path) {
		if (path.StartsWith(Application.persistentDataPath) && !path.Contains(ArchipelagoState.current.savePrefix)) {
			UpdateSaveFile(ref path);
		}
	}

	[HarmonyPatch(typeof(File), "ReadAllText")]
	[HarmonyPostfix]
	static void UpdateSaveFile3(string path, string __result) {
		if (path.EndsWith("savedata.ddd")) {
			ArchipelagoState.current.save = JObject.FromJSON(__result).ToObject<ArchipelagoSave>();
			ArchipelagoState.current.save.firstPlay = false;
		}
	}

	[HarmonyPatch(typeof(StringCipher), "Encrypt")]
	[HarmonyPrefix]
	static bool DisableEncryption1(ref string __result, string plainText) {
		__result = plainText;
		return false;
	}

	[HarmonyPatch(typeof(StringCipher), "Decrypt")]
	[HarmonyPrefix]
	static bool DisableEncryption2(ref string __result, string cipherText) {
		__result = cipherText;
		return false;
	}

	[HarmonyPatch(typeof(AchievementManager), "GetAchievement")]
	[HarmonyPrefix]
	static bool GetPopupData(ref AchievementSO __result, AchievementID id) {
		if (id >= PopupHandler.POPUP_DATA_OFFSET) {
			__result = PopupHandler.GetAndClear((int)id - (int)PopupHandler.POPUP_DATA_OFFSET);
			return false;
		}

		return true;
	}

	[HarmonyPatch(typeof(Translator), "GetLocalized")]
	[HarmonyPostfix]
	static void AllowUnlocalizedText(ref string __result, string textKey) {
		if (__result == "ERR") {
			__result = textKey;
		}
	}

	[HarmonyPatch(typeof(AchievementHandlerUI), "ShowAchievement")]
	[HarmonyPrefix]
	static bool CancelPopups(AchievementHandlerUI __instance, AchievementID id) {
		if (id >= PopupHandler.POPUP_DATA_OFFSET || (!Locations.Has(id) && !id.ToString().StartsWith("SUPER_"))) {
			return true;
		} else {
			__instance.busyShowing = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(GamesaveHandler), "SetAchievement")]
	[HarmonyPrefix]
	static void OnAchievement(GamesaveHandler __instance, AchievementID id) {
		if (id >= AchievementID.SUPER_DivorcePapers)
			Locations.SpaceCheck((int)id - (int)AchievementID.SUPER_DivorcePapers);
		else if (!__instance.GetAchievement(id))
			Locations.OnAchievement(id);
	}

    [HarmonyPatch(typeof(ShopManager), "IsValidForActiveGameZone")]
    [HarmonyPostfix]
    static void StripShop1(ref bool __result, PowerupSO pwr) => __result = __result && PowerUps.Allowed(pwr.UpgradeId);

	static int shopRealLastHopeUsesLeft, shopRealDivorceUsesLeft;
	static bool shopRealHasBlocker, shopRealHasHotSauce, shopRealHasRebirth;
	static float shopRealSuperHotSauceChance;
	[HarmonyPatch(typeof(ShopManager), "GetShopOptions")]
	[HarmonyPrefix]
	static void SetUpShopBlocking(ShopManager __instance) {
		shopRealLastHopeUsesLeft = __instance.lastHopeUsesLeft;
		shopRealDivorceUsesLeft = __instance.divorceUsesLeft;
		shopRealHasBlocker = __instance.hasBlocker;
		shopRealHasHotSauce = __instance.hasHotSauce;
		shopRealSuperHotSauceChance = __instance.superHotSauceChance;
		shopRealHasRebirth = __instance.hasRebirth;

		if (!PowerUps.Allowed(Upgrade.LastHope))
			__instance.lastHopeUsesLeft = 1;
		if (!PowerUps.Allowed(Upgrade.DivorcePapers))
			__instance.divorceUsesLeft = 1;
		if (!PowerUps.Allowed(Upgrade.RatBlocker))
			__instance.hasBlocker = true;
		if (!PowerUps.Allowed(Upgrade.HotSauce))
			__instance.hasHotSauce = true;
		else if (!PowerUps.Allowed(Upgrade.VeryHotSauce))
			__instance.superHotSauceChance = -1;
		if (!PowerUps.Allowed(Upgrade.Rebirth))
			__instance.hasRebirth = true;
	}

	[HarmonyPatch(typeof(ShopManager), "GetShopOptions")]
	[HarmonyPostfix]
	static void RestoreShopState(List<ShopUpgradeOption> __result, ShopManager __instance) {
		__instance.lastHopeUsesLeft = shopRealLastHopeUsesLeft;
		__instance.divorceUsesLeft = shopRealDivorceUsesLeft;
		__instance.hasBlocker = shopRealHasBlocker;
		__instance.hasHotSauce = shopRealHasHotSauce;
		__instance.superHotSauceChance = shopRealSuperHotSauceChance;
		__instance.hasRebirth = shopRealHasRebirth;

		foreach (var option in __result)
			if (option.Level == 1 && thisRunBaseLevels.TryGetValue(option.Power.UpgradeId, out int baseLevel))
				option.Level = baseLevel + 1;
	}

	[HarmonyPatch(typeof(ShopManager), "GetSomeOptions")]
	[HarmonyPostfix]
	static void StripShop2(
		ref List<ShopUpgradeOption> __result,
		ShopManager __instance,
		List<ShopUpgradeOption> list,
		int count,
		List<ShopUpgradeOption> alreadyInList
	) {
		List<ShopUpgradeOption> @new = new(__result.Count);
		bool dirty = false;

		foreach (var option in __result) {
			if (PowerUps.Allowed(option.Power.UpgradeId))
				@new.Add(option);
			else
				dirty = true;
		}

		if (dirty) {
			List<ShopUpgradeOption> newAIL = new(alreadyInList.Count + __result.Count);
			foreach (var ailItem in alreadyInList)
				newAIL.Add(ailItem);
			foreach (var newItem in __result)
				newAIL.Add(newItem);
			foreach (var nextAttemptItem in __instance.GetSomeOptions(list, count - @new.Count, newAIL))
				@new.Add(nextAttemptItem);
			__result = @new;
		}
	}

	[HarmonyPatch(typeof(ShopCanvasHandler), "ShowShop")]
	[HarmonyPrefix]
	static void HandlePowerupShop(ShopCanvasHandler __instance) {
		__instance.availableWings = ArchipelagoState.current.rerollingUnlocked ? PlayScreenManager.Instance.availableWings : 0;
		
		if (__instance.rerollBtn.transform.localPosition.y < -65536 && ArchipelagoState.current.rerollingUnlocked)
			__instance.rerollBtn.transform.localPosition += new Vector3(0, 131072, 0);
		else if (__instance.rerollBtn.transform.localPosition.y > -65536 && !ArchipelagoState.current.rerollingUnlocked)
			__instance.rerollBtn.transform.localPosition -= new Vector3(0, 131072, 0);
		
		if (__instance.fuseBtn.transform.localPosition.y < -65536 && ArchipelagoState.current.fusionUnlocked)
			__instance.fuseBtn.transform.localPosition += new Vector3(0, 131072, 0);
		else if (__instance.fuseBtn.transform.localPosition.y > -65536 && !ArchipelagoState.current.fusionUnlocked)
			__instance.fuseBtn.transform.localPosition -= new Vector3(0, 131072, 0);
	}
	
	[HarmonyPatch(typeof(ShopCanvasHandler), "HasRerollPotion")]
	[HarmonyPostfix]
	static void RequireRerollingForRerollPotion(ref bool __result) => __result = __result && ArchipelagoState.current.rerollingUnlocked;

	[HarmonyPatch(typeof(ShopManager), "CanFusePowerups")]
	[HarmonyPostfix]
	static void CanFuse(ref bool __result) => __result = __result && ArchipelagoState.current.fusionUnlocked;

	/*[HarmonyPatch(typeof(GamesaveHandler), "GetSpaceFlights")]
	[HarmonyPrefix]
	static bool GetSpaceFlights(ref int __result) {
		__result = 3 - ArchipelagoState.current.save.freeSpaceFlights;
		return false;
	}

	[HarmonyPatch(typeof(GameManager), "NotifySpaceStarted")]
	[HarmonyPrefix]
	static void UseFreeFlight() => Plugin.Schedule(() => {
		if (ArchipelagoState.current.save.freeSpaceFlights > 0)
			ArchipelagoState.current.save.freeSpaceFlights--;
	}, 0.5f);*/

	[HarmonyPatch(typeof(GamesaveHandler), "SetUnlock")]
	[HarmonyPrefix]
	static bool BlockCosmetic1(UnlockID id) {
		if (Items.processingCosmeticItem) {
			Items.processingCosmeticItem = false;
			return true;
		}

		var name = id.ToString();
		if (Locations.Buy(name))
			return false;

		if (!(name.StartsWith("ACC_") || name.StartsWith("TRAIL_")) || !ArchipelagoState.current.slotData.cosmeticsAreItems)
			return true;

		return false;
	}

	[HarmonyPatch(typeof(PlayScreenManager), "EquipUnlockedAccesory")]
	[HarmonyPrefix]
	static bool BlockCosmetic2() => !ArchipelagoState.current.slotData.cosmeticsAreItems || Items.processingCosmeticItem;

	[HarmonyPatch(typeof(AchievementHandlerUI), "ShowUnlock")]
	[HarmonyPrefix]
	static bool BlockCosmeticPopup(AchievementHandlerUI __instance, UnlockID unlock) {
		if (
			unlock != UnlockID.EQUIP_SecondSlot &&
			(!(unlock.ToString().StartsWith("ACC_") || unlock.ToString().StartsWith("TRAIL_")) || !ArchipelagoState.current.slotData.cosmeticsAreItems)
		) {
			return true;
		}
		
		__instance.busyShowing = false;
		return false;
	}

	[HarmonyPatch(typeof(MythKiaraPanelV5), "PopulateBuyableList")]
	[HarmonyPrefix]
	static void SetUpShop(MythKiaraPanelV5 __instance) {
		Plugin.SetSpaceItemMode(SpaceItemMode.Received);

		if (ArchipelagoState.current.slotData.cosmeticsAreItems) {
			__instance.shopItemsToDisplay = __instance.shopItemsToDisplay.Where(i => !i.ToString().StartsWith("ACC_")).ToArray();
		}

		Locations.SetUpShop(__instance.shopItemsToDisplay);
	}

	[HarmonyPatch(typeof(MythScreensHandlerV5), "ShowShop")]
	[HarmonyPrefix]
	static bool FixShopIconStretch() {
		if (!ArchipelagoState.current.shopUnlocked)
			return false;

		ArchipelagoState.current.ViewShop();

		foreach (var msbui5 in Component.FindObjectsOfType<MythShopBuyableUIV5>()) {
			var icon = msbui5.transform.Find("IconGroup/IconMask/Icon");
			if (icon && icon.GetChildCount() == 0 && Locations.Has(msbui5.BuyableItem.BuyableId)) {
				var iim = icon.GetComponent<Image>();
				var size = iim.sprite.rect.size;
				
				if (size.x != size.y) {
					GameObject go = new();
					go.transform.SetParent(icon, false);

					var im = go.AddComponent<Image>();
					im.sprite = iim.sprite;
					iim.color = Color.clear;

					var rt = go.GetComponent<RectTransform>();

					if (size.x > size.y) {
						rt.anchorMin = new(0, Mathf.Lerp(0.5f, 0, size.y / size.x));
						rt.anchorMax = new(1, Mathf.Lerp(0.5f, 1, size.y / size.x));
					} else {
						rt.anchorMin = new(Mathf.Lerp(0.5f, 0, size.x / size.y), 0);
						rt.anchorMax = new(Mathf.Lerp(0.5f, 1, size.x / size.y), 1);
					}

					rt.sizeDelta = Vector2.zero;
				}
			}
		}

		return true;
	}

	[HarmonyPatch(typeof(GamesaveHandler), "SetRecipeUnlock")]
	[HarmonyPrefix]
	static bool BuyItem(RecipeUnlock recipe) => Items.processingRecipe || !Locations.Buy(recipe.ToString());

	[HarmonyPatch(typeof(MythKiaraPanelV5), "GetBuyableAvailableState")]
	[HarmonyPrefix]
	static bool CheckItemAvailability(ref bool __result, RecipeUnlock recipeId, UnlockID unlockId) {
		if (Locations.shopLocations.TryGetValue(recipeId == RecipeUnlock.NONE ? unlockId.ToString() : recipeId.ToString(), out var location)) {
			__result = !Locations.Checked(location) && !ArchipelagoState.current.Checked(location);
			return false;
		}

		return true;
	}

    [HarmonyPatch(typeof(SpaceUpgradeBox), "Start")]
    [HarmonyPrefix]
    static void SetUpSpaceUpgradeBox(SpaceUpgradeBox __instance) {
		ArchipelagoState.current.ViewSpace();
		Locations.SetUpSpaceUpgradeBox(__instance, __instance.PointsRequired / 1800);
	}

    [HarmonyPatch(typeof(SpaceUnlocksListUI), "Start")]
    [HarmonyPrefix]
    static void SetUpSpaceItemList(SpaceUnlocksListUI __instance) {
		DeathLinkStop();
		Plugin.SetSpaceItemMode(SpaceItemMode.Sent);
		ArchipelagoState.current.FlushStoneQueue();
		Locations.SetUpSpaceIcons(__instance.transform);
	}

	static Vector2 spaceNextUnlockBaseSize;
	static Vector3 spaceNextUnlockBasePosition;
	[HarmonyPatch(typeof(SpaceBottomUI), "Start")]
	[HarmonyPostfix]
	static void FixNextUnlockStretch1(SpaceBottomUI __instance) {
		spaceNextUnlockBaseSize = __instance.nextImage.GetComponent<RectTransform>().sizeDelta;
		spaceNextUnlockBasePosition = __instance.nextImage.transform.localPosition;
	}

	[HarmonyPatch(typeof(SpaceBottomUI), "UpdateUpgradeVisual")]
	[HarmonyPostfix]
	static void FixNextUnlockStretch2(SpaceBottomUI __instance) {
		var size = __instance.nextImage.sprite.rect.size;
		if (size.x > size.y) {
			__instance.nextImage.transform.localScale = new(1, size.y / size.x, 1);
		} else {
			__instance.nextImage.transform.localScale = new(size.x / size.y, 1, 1);
		}
	}

	[HarmonyPatch(typeof(SpacePause), "ShowPowerInfo")]
	[HarmonyPostfix]
	static void FixSpaceIconPauseStretch(SpacePause __instance) {
		var size = __instance.powerIcon.sprite.rect.size;

		GameObject go;
		Image im;
		if (__instance.powerIcon.transform.GetChildCount() == 0) {
			go = new();
			go.transform.SetParent(__instance.powerIcon.transform, false);
			im = go.AddComponent<Image>();
		} else {
			go = __instance.powerIcon.transform.GetChild(0).gameObject;
			im = go.GetComponent<Image>();
		}

		im.sprite = __instance.powerIcon.sprite;
		__instance.powerIcon.color = Color.clear;

		var rt = go.GetComponent<RectTransform>();

		if (size.x > size.y) {
			rt.anchorMin = new(0, Mathf.Lerp(0.5f, 0, size.y / size.x));
			rt.anchorMax = new(1, Mathf.Lerp(0.5f, 1, size.y / size.x));
		} else {
			rt.anchorMin = new(Mathf.Lerp(0.5f, 0, size.x / size.y), 0);
			rt.anchorMax = new(Mathf.Lerp(0.5f, 1, size.x / size.y), 1);
		}

		rt.sizeDelta = Vector2.zero;
	}

	static readonly System.Collections.Generic.Dictionary<Upgrade, int> thisRunBaseLevels = [];
	[HarmonyPatch(typeof(ShopManager), "ResetUpgrades")]
	[HarmonyPostfix]
	static void ApplyBasePowers() {
		foreach (var pair in ArchipelagoState.current.baseLevels) {
			var so = PowerupManager.Instance.GetPowerup(pair.Key);
			int level = Math.Min(pair.Value, so.MaxLevel - 1);
			thisRunBaseLevels[pair.Key] = level;
			if (so.ValidZones.Contains(GameZone.Normal)) {
				for (int i = 1; i <= level; i++) {
					FlightParamsManager.Instance.HandleUpgradeGained(null, new() {
						Zone = GameZone.Normal,
						Powerup = so,
						Level = i
					});
				}
			}
			if (so.ValidZones.Contains(GameZone.Ocean)) {
				for (int i = 1; i <= level; i++) {
					FlightParamsManager.Instance.HandleUpgradeGained(null, new() {
						Zone = GameZone.Ocean,
						Powerup = so,
						Level = i
					});
				}
			}
		}
	}

	[HarmonyPatch(typeof(ShopManager), "AddUpgrade")]
	[HarmonyPrefix]
	static void PreAddUpgrade(ShopManager __instance, Upgrade up) {
		if (thisRunBaseLevels.TryGetValue(up, out var level))
			__instance.AcquiredUpgradesActiveZone.TryAdd(up, level);	
	}

	[HarmonyPatch(typeof(PowerupIconsBar), "HandleFuseUpgradeGained")]
	[HarmonyPrefix]
	static void OnFuse(ShopManager.UpgradeData upgradeData) => Locations.Fusion(upgradeData.Powerup);

	[HarmonyPatch(typeof(PlayScreenManager), "HandleStopPowering")]
	[HarmonyPrefix]
	static void OnYeet() {
		DeathLinkStart();
		Plugin.AddTickerUniqueType(new DistanceTracker());
	}

	[HarmonyPatch(typeof(GameManager), "NotifySpaceStarted")]
	[HarmonyPrefix]
	static void DeathLinkStart() => ArchipelagoState.current.deathLinkConnected = ArchipelagoState.current.deathLinkEnabled;

	[HarmonyPatch(typeof(GameManager), "ResetGameFromPause")]
	[HarmonyPrefix]
	static void DeathLinkStop() => ArchipelagoState.current.deathLinkConnected = false;

	static int achievements;
	[HarmonyPatch(typeof(PlayScreenManager), "CheckEndGameAchievements")]
	[HarmonyPrefix]
	static void StoreAchievementCount() {
		achievements = 0;
		foreach (var pair in GamesaveHandler.Instance.achievementsUnlocked)
			if (pair.Value && pair.Key < AchievementID.SUPER_DivorcePapers)
				achievements++;
	}

	[HarmonyPatch(typeof(PlayScreenManager), "CheckEndGameAchievements")]
	[HarmonyPostfix]
	static void CheckSendDeathLink() {
		if (ArchipelagoState.current.deathLinkConnected && !PlayScreenManager.Instance.isBaelessTimeup) {
			foreach (var pair in GamesaveHandler.Instance.achievementsUnlocked)
				if (pair.Value && pair.Key < AchievementID.SUPER_DivorcePapers)
					achievements--;
			
			if (achievements == 0)
				ArchipelagoState.current.SendDeathLink(PlayScreenManager.Instance.thePlayer.IsDead ? DeathReason.Bae : DeathReason.Stop);
		}
	}

	[HarmonyPatch(typeof(MyUtils), "FormatFloatToMS")]
	[HarmonyPrefix]
	static bool AllowInfiniteTime(ref string __result, float totalSecondsF) {
		if (totalSecondsF > 1e7f) {
			__result = "--:--.--";
			return false;
		}

		return true;
	}

	[HarmonyPatch(typeof(SpaceGameover), "ShowGameoverScreen")]
	[HarmonyPrefix]
	static void CheckSendSpaceDeathLink() {
		ArchipelagoState.current.save.dirty = true;
		if (ArchipelagoState.current.deathLinkConnected)
			ArchipelagoState.current.SendDeathLink(ArchipelagoState.current.asteroidHitTime > 0 ? DeathReason.Asteroid : DeathReason.NoFuel);
	}

	[HarmonyPatch(typeof(SpaceSpawn), "SlowPlayer")]
	[HarmonyPrefix]
	static void NoteAsteroidHit(float slowFactor) {
		if (slowFactor == 0.5f)
			ArchipelagoState.current.asteroidHitTime = 5;
	}

	[HarmonyPatch(typeof(InstructionCanvasHandler), "Start")]
	[HarmonyPrefix]
	static void NotMyFirstRodeo(InstructionCanvasHandler __instance) {
		__instance.transform.Find("RegularInstr/Trees").localPosition += new Vector3(0, -65536, 0);
		__instance.transform.Find("RegularInstr/Boost").localPosition += new Vector3(0, -65536, 0);
		__instance.transform.Find("RegularInstr/Soda").localPosition += new Vector3(0, -65536, 0);
		__instance.transform.Find("RegularInstr/HowFar").localPosition += new Vector3(0, -65536, 0);
		__instance.dontTouchBaeClassic.localPosition += new Vector3(0, -65536, 0);
		__instance.dontTouchBaeNew.localPosition += new Vector3(0, -65536, 0);
	}

	[HarmonyPatch(typeof(GameModeSelectUIV5), "SelectModeClassic")]
	[HarmonyPrefix]
	static bool CanClassic() => ArchipelagoState.current.Unlocked(GameMode.Classic);
	[HarmonyPatch(typeof(GameModeSelectUIV5), "SelectModeBaeless")]
	[HarmonyPrefix]
	static bool CanBaeless() => ArchipelagoState.current.Unlocked(GameMode.Baeless);
	[HarmonyPatch(typeof(GameModeSelectUIV5), "SelectModeGacha")]
	[HarmonyPrefix]
	static bool CanGacha() => ArchipelagoState.current.Unlocked(GameMode.Chaos);
	[HarmonyPatch(typeof(GameModeSelectUIV5), "SelectModeJail")]
	[HarmonyPrefix]
	static bool CanTheCell() => ArchipelagoState.current.Unlocked(GameMode.TheCell);
	[HarmonyPatch(typeof(GameModeSelectUIV5), "SelectModeSpace")]
	[HarmonyPrefix]
	static bool CanSpace() => ArchipelagoState.current.Unlocked(GameMode.Space);

	static void SetGameMode(GameModeSelectUIV5 gmsui5, GameMode mode) {
		switch (mode) {
			case GameMode.Standard:
				gmsui5.SelectModeStandard();
				break;

			case GameMode.Classic:
				gmsui5.SelectModeClassic();
				break;
			
			case GameMode.Baeless:
				gmsui5.SelectModeBaeless();
				break;
			
			case GameMode.Chaos:
				gmsui5.SelectModeGacha();
				break;
			
			case GameMode.TheCell:
				gmsui5.SelectModeJail();
				break;
			
			case GameMode.Space:
				gmsui5.SelectModeSpace();
				break;
		}
	}

	[HarmonyPatch(typeof(GameModeSelectUIV5), "SelectNextGameMode")]
	[HarmonyPrefix]
	static bool NextGameMode(GameModeSelectUIV5 __instance) {
		var gm = __instance.selectedGameMode + 1;
		while (gm <= GameMode.Space && !ArchipelagoState.current.Unlocked(gm))
			gm++;
		SetGameMode(__instance, gm);
		return false;
	}

	[HarmonyPatch(typeof(GameModeSelectUIV5), "SelectPreviousGameMode")]
	[HarmonyPrefix]
	static bool PreviousGameMode(GameModeSelectUIV5 __instance) {
		if (__instance.selectedGameMode > GameMode.Standard) {
			var gm = __instance.selectedGameMode - 1;
			while (!ArchipelagoState.current.Unlocked(gm))
				gm--;
			SetGameMode(__instance, gm);
		}
		return false;
	}

	[HarmonyPatch(typeof(GameModeSelectUIV5), "Awake")]
	[HarmonyPostfix]
	static void LoadGameModeIcons(GameModeSelectUIV5 __instance) => Items.LoadGameModeIcons(__instance);

	[HarmonyPatch(typeof(MythScreensHandlerV5), "ShowLaboratory")]
	[HarmonyPrefix]
	static bool BlockLab() => ArchipelagoState.current.labUnlocked;

	[HarmonyPatch(typeof(MythScreensHandlerV5), "Start")]
	[HarmonyPrefix]
	static void ChangeButtonText() {
		if (!ArchipelagoState.current.labUnlocked) {
			var t = GameObject.Find("MythBtn_Lab").GetComponentInChildren<TMPTranslator>();
			t.SetKey("Locked");
			t.DoTranslate();
		}

		if (!ArchipelagoState.current.shopUnlocked) {
			var t = GameObject.Find("MythBtn_shop").GetComponentInChildren<TMPTranslator>();
			t.SetKey("Locked");
			t.DoTranslate();
		}
	}

	[HarmonyPatch(typeof(GamesaveHandler), "SubmitLevelResult")]
	[HarmonyPrefix]
	static void SaveResult(PlayScreenManager.GameEndData endData) {
		ArchipelagoState.current.save.UpdateDistanceRecord(endData.Mode, endData.RunDistance);
		ArchipelagoState.current.save.dirty = true;
	}

	[HarmonyPatch(typeof(GamesaveHandler), "GetBestScore")]
	[HarmonyPrefix]
	static void SplitStandardClassic(GamesaveHandler __instance, GameMode mode) => __instance.SetBestDistance(
		mode == GameMode.Standard ?
		ArchipelagoState.current.save.bestDistanceStandard :
		ArchipelagoState.current.save.bestDistanceClassic
	);

	[HarmonyPatch(typeof(SpaceWaveManager), "UpdateDistanceUI")]
	[HarmonyPostfix]
	static void UpdateSpaceDistance(SpaceWaveManager __instance) =>
		ArchipelagoState.current.save.UpdateDistanceRecord(GameMode.Space, __instance.thisFlightTime * __instance.distMulti);
}