using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using UnityEngine;
using UnityEngine.UI;

namespace ClientPlugin;

static class Items {

	const long ITEM_TYPE_GAME_MODE = 0;
	const long ITEM_TYPE_OBJECT = 1;
	const long ITEM_TYPE_ABILITY = 2;
	const long ITEM_TYPE_RECIPE = 3;
	const long ITEM_TYPE_POWERUP = 4;

	public const long ITEM_TYPE_SPACE = 10;
	public const long ITEM_TYPE_PROGRESSIVE = 11;

	const long ITEM_TYPE_FILLER = 50;

	const long ITEM_TYPE_COSMETIC = 100;
	const long ITEM_TYPE_COSMETIC_HEAD = 100;
	const long ITEM_TYPE_COSMETIC_BODY = 101;
	const long ITEM_TYPE_COSMETIC_TRAIL = 102;

	public static bool processingCosmeticItem = false, processingRecipe = false;

	static readonly Dictionary<long, Sprite> iconData = [];
	public static bool HasIconData => iconData.Count > 8;
	static readonly Transform[] gameModeIcons = new Transform[6];
	static readonly Transform[] objectIcons = new Transform[31];
	static Transform ratMasks;
	static readonly PoolableType[] ratTypes = [
		PoolableType.Booster,
		PoolableType.Mumei,
		PoolableType.Ina,
		PoolableType.Kiara,
		PoolableType.Kronii,
		PoolableType.Gura,
		PoolableType.Fauna
	];

	public static readonly bool[] spaceItemsReceived = new bool[5];

	static int itemsHandled = 0;

	public static void HandleIncoming(ItemInfo item) {
		Plugin.L($"Got item {item.ItemName} ({item.ItemId})");

		if (item.ItemGame != "Delivering Hope") {
			Plugin.Log.LogWarning("AAAAH");
			return;
		}

		bool @new = ArchipelagoState.current.save.lastItemIndex < ++itemsHandled;
		
		if (@new) {
			if (item.Player.Slot != ArchipelagoState.current.Me)
				PopupHandler.Popup(string.Concat(item.ItemName[0].ToString().ToUpperInvariant(), item.ItemName.AsSpan(1)), $"Received from {item.Player.Name}",
					() => GetIcon(item), item.ItemId >> 8 < ITEM_TYPE_FILLER ? UnityEngine.Color.yellow : UnityEngine.Color.white);
			ArchipelagoState.current.save.lastItemIndex = itemsHandled;
			ArchipelagoState.current.save.dirty = true;
		}

		switch (item.ItemId >> 8) {
			case ITEM_TYPE_GAME_MODE:
				ArchipelagoState.current.Unlock((GameMode)(item.ItemId & 255));
				ShowHideGameModeIcons();
				break;

			case ITEM_TYPE_OBJECT:
				ArchipelagoState.current.Unlock((PoolableType)(item.ItemId & 255));
				PositionObjectIcons();
				break;
			
			case ITEM_TYPE_ABILITY:
				switch (item.ItemId & 255) {
					case 0:
						ArchipelagoState.current.downboostUnlocked = true;
						break;
					case 1:
						ArchipelagoState.current.beachUnlocked = true;
						break;
					case 2:
						ArchipelagoState.current.fusionUnlocked = true;
						break;
					case 3:
						ArchipelagoState.current.shopUnlocked = true;
						break;
					case 4:
						ArchipelagoState.current.labUnlocked = true;
						break;
					case 10:
						ArchipelagoState.current.rerollingUnlocked = true;
						break;
					case 11:
						processingCosmeticItem = true;
						GamesaveHandler.Instance.SetUnlock(UnlockID.EQUIP_SecondSlot);
						break;
				}
				break;

			case ITEM_TYPE_RECIPE:
				processingRecipe = true;
				GamesaveHandler.Instance.SetRecipeUnlock((RecipeUnlock)(item.ItemId & 255), true, false);
				processingRecipe = false;
				if (GamesaveHandler.Instance.GetRecipeUnlockCount() > 6)
					GamesaveHandler.Instance.SetAchievement(AchievementID.AncientKnowledge);
				break;

			case ITEM_TYPE_POWERUP:
				ArchipelagoState.current.Unlock((Upgrade)(item.ItemId & 255));
				break;

			case ITEM_TYPE_SPACE:
				spaceItemsReceived[item.ItemId & 255] = true;
				Plugin.ForceUpdateSpaceItems();
				break;

			case ITEM_TYPE_PROGRESSIVE:
				switch (item.ItemId & 255) {
					case 100:
						if (ArchipelagoState.current.Unlocked(PoolableType.Strawberry)) {
							spaceItemsReceived[2] = true;
							Plugin.ForceUpdateSpaceItems();
						} else {
							ArchipelagoState.current.Unlock(PoolableType.Strawberry);
						}
						break;
					case 101:
						ArchipelagoState.current.Unlock(ArchipelagoState.current.Unlocked(Upgrade.HotSauce) ? Upgrade.VeryHotSauce : Upgrade.HotSauce);
						break;
					case 102:
						if (ArchipelagoState.current.Unlocked(Upgrade.LastHope)) {
							spaceItemsReceived[1] = true;
							Plugin.ForceUpdateSpaceItems();
						} else {
							ArchipelagoState.current.Unlock(Upgrade.LastHope);
						}
						break;
					case 103:
						if (ArchipelagoState.current.Unlocked(Upgrade.DivorcePapers)) {
							spaceItemsReceived[0] = true;
							Plugin.ForceUpdateSpaceItems();
						} else {
							ArchipelagoState.current.Unlock(Upgrade.DivorcePapers);
						}
						break;
					default:
						var upgrade = (Upgrade)(item.ItemId & 255);
						ArchipelagoState.current.baseLevels.TryGetValue(upgrade, out int level);
						ArchipelagoState.current.baseLevels[upgrade] = level + 1;
						break;
				}
				break;
			
			case ITEM_TYPE_FILLER:
				if (!@new)
					break;
				if (item.ItemName.EndsWith(" Hope Stones")) {
					ArchipelagoState.current.save.queuedHopeStones += int.Parse(item.ItemName.Split(' ')[0]);
					ArchipelagoState.current.FlushStoneQueue();
				} else {
					GamesaveHandler.Instance.SetCraftItemCount(
						(CraftItem)(item.ItemId & 255),
						GamesaveHandler.Instance.GetCraftItemCount((CraftItem)(item.ItemId & 255)) + 1,
						false
					);
				}
				break;

			case ITEM_TYPE_COSMETIC_HEAD:
			case ITEM_TYPE_COSMETIC_BODY:
			case ITEM_TYPE_COSMETIC_TRAIL:
				if (!@new || !ArchipelagoState.current.slotData.cosmeticsAreItems)
					break;
				foreach (var cosmetic in AccessoryManager.Instance.unlockableAccessories) {
					if (ITEM_TYPE_COSMETIC - 1 + (int)cosmetic.Category == item.ItemId >> 8 && Math.Max(Math.Max(
						(int)cosmetic.AccesoryHead, (int)cosmetic.AccesoryBody), (int)cosmetic.AccesoryTrail) == (item.ItemId & 255)) {
						processingCosmeticItem = true;
						if (PlayScreenManager._instance)
							PlayScreenManager._instance.EquipUnlockedAccesory(cosmetic.Unlock, cosmetic.Category);
						else
							GamesaveHandler.Instance.SetUnlock(cosmetic.Unlock);
						break;
					}
				}
				break;
		}
	}

	public static void LoadGameModeIcons(GameModeSelectUIV5 gmsui5) {
		for (var mode = GameMode.Standard; mode <= GameMode.Space; mode++) {
			if (gmsui5.sidebarButtonsDict.TryGetValue(SidebarPanelPage.PLAY_STANDARD + (int)mode, out var btn) && btn && btn.icon) {
				gameModeIcons[(int)mode] = btn.icon.transform;
				iconData[(ITEM_TYPE_GAME_MODE << 8) | (int)mode] = btn.onSprite;
			}
		}

		iconData[(ITEM_TYPE_ABILITY << 8) | 1] = Sprite.Create(iconData[ITEM_TYPE_GAME_MODE << 8].texture, new(298, 257, 90, 90), new());
		iconData[(ITEM_TYPE_ABILITY << 8) | 2] = Sprite.Create(iconData[ITEM_TYPE_GAME_MODE << 8].texture, new(260, 676, 76, 53), new());
		iconData[(ITEM_TYPE_ABILITY << 8) |10] = Sprite.Create(iconData[ITEM_TYPE_GAME_MODE << 8].texture, new(256, 758, 54, 55), new());
		
		ShowHideGameModeIcons();
	}

	static void ShowHideGameModeIcons() {
		for (var mode = GameMode.Classic; mode <= GameMode.Space; mode++) {
			var tf = gameModeIcons[(int)mode];
			if (tf) {
				bool unlocked = ArchipelagoState.current.Unlocked(mode);
				if (unlocked && tf.localPosition.y < -65536)
					tf.localPosition += new Vector3(0, 131072, 0);
				else if (!unlocked && tf.localPosition.y > -65536)
					tf.localPosition -= new Vector3(0, 131072, 0);
			}
		}		
	}

	public static void LoadObjectIconTransforms() {
		if (objectIcons[0] || !GameObject.Find("Chara_Calli"))
			return;
		
		ratMasks = GameObject.Find("RatMasks").transform;

		objectIcons[0] = GameObject.Find("Chara_Kaela").transform;
		objectIcons[1] = GameObject.Find("Chara_Kobo").transform;

		objectIcons[(int)PoolableType.Booster]          = GameObject.Find("Chara_Calli").transform;
		objectIcons[(int)PoolableType.Banana]           = GameObject.Find("Item_Banana").transform;
		objectIcons[(int)PoolableType.Jetpack]          = GameObject.Find("Air_Rocket").transform;
		objectIcons[(int)PoolableType.Glider]           = GameObject.Find("Air_Glider").transform;
		objectIcons[(int)PoolableType.Monopoly]         = GameObject.Find("Air_Holopoly").transform;
		objectIcons[(int)PoolableType.Kronii]           = GameObject.Find("Chara_Kronii").transform;
		objectIcons[(int)PoolableType.Mumei]            = GameObject.Find("Chara_Mumei").transform;
		objectIcons[(int)PoolableType.Kiara]            = GameObject.Find("Chara_Kiara").transform;
		//objectIcons[(int)PoolableType.Sana]             = GameObject.Find("Chara_Sana").transform;
		objectIcons[(int)PoolableType.Ina]              = GameObject.Find("Chara_Ina").transform;
		objectIcons[(int)PoolableType.Gura]             = GameObject.Find("Chara_Gura").transform;
		objectIcons[(int)PoolableType.Fauna]            = GameObject.Find("Chara_Fauna").transform;
		objectIcons[(int)PoolableType.Strawberry]       = GameObject.Find("Item_Strawberry").transform;
		objectIcons[(int)PoolableType.SupersonicRocket] = GameObject.Find("Air_Supersonic").transform;
		objectIcons[(int)PoolableType.Shiori]           = GameObject.Find("Chara_Shiori").transform;
		objectIcons[(int)PoolableType.Nerissa]          = GameObject.Find("Chara_Nerissa").transform;
		objectIcons[(int)PoolableType.Bijou]            = GameObject.Find("Chara_Bijou").transform;
		objectIcons[(int)PoolableType.Fuwawa]           = GameObject.Find("Chara_Fuwawa").transform;
		objectIcons[(int)PoolableType.Mococo]           = GameObject.Find("Chara_Mococo").transform;

		PositionObjectIcons();
	}

	public static void PositionObjectIcons() {
		if (!ratMasks)
			return;

		for (int i = -7; i < 31; i++) {
			var tf = i < 0 ? ratMasks.GetChild(~i) : objectIcons[i];
			if (tf) {
				bool unlocked = i < 0 ? ArchipelagoState.current.Unlocked(ratTypes[~i]) :
					i < 2 ? ArchipelagoState.current.downboostUnlocked :
					i == (int)PoolableType.Mococo ? ArchipelagoState.current.Unlocked(PoolableType.Fuwawa) :
					ArchipelagoState.current.Unlocked((PoolableType)i);
				if (unlocked && tf.localPosition.y < -65536)
					tf.localPosition += new Vector3(0, 131072, 0);
				else if (!unlocked && tf.localPosition.y > -65536)
					tf.localPosition -= new Vector3(0, 131072, 0);
			}
		}
	}

	public static void LoadIcons() {
		for (int i = 2; i < 30; i++)
			if (objectIcons[i])
				iconData[(ITEM_TYPE_OBJECT << 8) | i] = objectIcons[i].GetComponent<Image>().sprite;

		if (objectIcons[0])
			iconData[(ITEM_TYPE_ABILITY << 8) | 0] = objectIcons[0].GetComponent<Image>().sprite;
		
		iconData[(ITEM_TYPE_ABILITY << 8) | 3] = AchievementManager.Instance.GetAchievement(AchievementID.DeepPockets).Icon;
		iconData[(ITEM_TYPE_ABILITY << 8) | 4] = AchievementManager.Instance.GetAchievement(AchievementID.AncientKnowledge).Icon;
		iconData[(ITEM_TYPE_ABILITY << 8) |11] = CurrencyItemManager.Instance.GetBuyable(Buyable.UNL_EQUIPSLOT).Sprite;

		for (var i = RecipeUnlock.BonkPotion; i <= RecipeUnlock.RerollPotion; i++)
			iconData[(ITEM_TYPE_RECIPE << 8) | (int)i] = CurrencyItemManager.Instance.GetBuyable(Buyable.REC_BONK - (int)RecipeUnlock.BonkPotion + (int)i).Sprite;

		var pumi = PowerupManager.Instance;
		for (var i = Upgrade.Horizontal; i <= Upgrade.KoboBonk; i++) {
			var so = pumi.GetPowerup(i);
			if (so == pumi.empty)
				so = pumi.GetConsumable(i);
			iconData[(ITEM_TYPE_PROGRESSIVE << 8) | (int)i] = iconData[(ITEM_TYPE_POWERUP << 8) | (int)i] = so.Icon ?? so.IconOcean;
		}
		iconData[ITEM_TYPE_POWERUP << 8] = pumi.fusionsDict[Upgrade.Diagonal].Icon;

		iconData[(ITEM_TYPE_PROGRESSIVE << 8) | 100] = iconData[(ITEM_TYPE_OBJECT << 8) | (int)PoolableType.Strawberry];
		iconData[(ITEM_TYPE_PROGRESSIVE << 8) | 101] = iconData[(ITEM_TYPE_POWERUP << 8) | (int)Upgrade.HotSauce];
		iconData[(ITEM_TYPE_PROGRESSIVE << 8) | 102] = iconData[(ITEM_TYPE_POWERUP << 8) | (int)Upgrade.LastHope];
		iconData[(ITEM_TYPE_PROGRESSIVE << 8) | 103] = iconData[(ITEM_TYPE_POWERUP << 8) | (int)Upgrade.DivorcePapers];

		for (var i = CraftItem.MysteryFluid; i < CraftItem.Currency; i++)
			iconData[(ITEM_TYPE_FILLER << 8) | (int)i] = CraftableItemManager.Instance.GetCraftable(i).Sprite;	

		iconData[(ITEM_TYPE_FILLER << 8) | 100] = iconData[(ITEM_TYPE_FILLER << 8) | 101] =
			Sprite.Create(iconData[(ITEM_TYPE_RECIPE << 8) | (int)RecipeUnlock.RatRepellent].texture, new(2410, 2281, 250, 248), new());

		foreach (var cosmetic in AccessoryManager.Instance.unlockableAccessories)
			iconData[((ITEM_TYPE_COSMETIC - 1 + (int)cosmetic.Category) << 8) | (long)Math.Max(Math.Max(
				(int)cosmetic.AccesoryHead, (int)cosmetic.AccesoryBody), (int)cosmetic.AccesoryTrail)] = cosmetic.EquipIcon;
	}

	public static Sprite GetIcon(ItemInfo item) {
		if (item.ItemGame == "Delivering Hope" && iconData.TryGetValue(item.ItemId, out var icon))
			return icon;
		else
			return Plugin.archipelagoIcon;
	}

	public static void SetIcon(long id, Sprite icon) {
		if (!iconData.ContainsKey(id))
			iconData[id] = icon;
	}

	public static string GetCategory(ItemInfo item) => item.ItemGame == "Delivering Hope" ? (item.ItemId >> 8) switch {
		ITEM_TYPE_GAME_MODE => "Game Mode",
		ITEM_TYPE_OBJECT => char.IsUpper(item.ItemDisplayName[0]) && (item.ItemId & 255) != (int)PoolableType.Monopoly ? "Character" : "Object",
		ITEM_TYPE_ABILITY => "Unlock",
		ITEM_TYPE_RECIPE => "Recipe",
		ITEM_TYPE_SPACE => "Unlock",
		ITEM_TYPE_COSMETIC_HEAD => "Accessory",
		ITEM_TYPE_COSMETIC_BODY => "Accessory",
		ITEM_TYPE_COSMETIC_TRAIL => "Accessory",
		_ => "Archipelago Item"
	} : "Archipelago Item";
}