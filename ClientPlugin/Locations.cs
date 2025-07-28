using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.UI;

namespace ClientPlugin;

static class Locations {
	const long LOCATION_TYPE_ACHIEVEMENT = 0;
	public const long LOCATION_TYPE_SHOP = 1;
	public const long LOCATION_TYPE_SPACE = 2;
	const long LOCATION_TYPE_FUSION = 3;

	static Dictionary<long, ScoutedItemInfo> locationScoutData = null;
	public static Dictionary<string, long> shopLocations = [];
	
	public static void Scout(ArchipelagoSession session) {
		session.Locations.ScoutLocationsAsync(
			HintCreationPolicy.None,
			[.. session.Locations.AllLocations]
		).ContinueWith(scoutData => {
			locationScoutData = scoutData.Result;
			session.Locations.CompleteLocationChecks([.. locationScoutData.Keys.Where(Checked)]);
		});
	}

	public static void OnAchievement(AchievementID achievement) {
		Plugin.L($"Got achievement {achievement}");

		if (!Has(achievement))
			return;

		var location = (long)achievement;
		ArchipelagoState.current.Check(location);

		if (!AchievementManager.Instance)
			return;

		var scout = locationScoutData[location];
		var aso = AchievementManager.Instance.GetAchievement(achievement);
		PopupHandler.Popup(
			aso.Title,
			scout.Player.Slot == ArchipelagoState.current.Me ? $"Found {scout.ItemDisplayName}" : $"Sent {scout.ItemDisplayName} to {scout.Player.Name}",
			() => {
				var icon = Items.GetIcon(scout);
				return icon == Plugin.archipelagoIcon ? aso.Icon : icon;
			},
			aso.NotifColor
		);
	}

	public static bool Has(long location) => locationScoutData.ContainsKey(location);
	public static bool Has(AchievementID achievement) => Has((LOCATION_TYPE_ACHIEVEMENT << 8) | (int)achievement);
	public static bool Has(Buyable shopItem) => Has((LOCATION_TYPE_SHOP << 8) | (int)shopItem);

    public static bool Checked(long location) => (location >> 8) switch {
        LOCATION_TYPE_ACHIEVEMENT => GamesaveHandler.Instance.GetAchievement((AchievementID)(location & 255)),
		LOCATION_TYPE_SPACE => ArchipelagoState.current.save.spaceItemsSent[location & 255],
        _ => ArchipelagoState.current.save.checks.Contains(location),
    };

	public static void CheckExtra(long location) {
		if (!Checked(location)) {
			ArchipelagoState.current.save.checks.Add(location);
			ArchipelagoState.current.Check(location);
		}
	}

	public static void SetUpShop(Il2CppStructArray<Buyable> shop) {
		if (locationScoutData == null) {
			Plugin.Schedule(() => SetUpShop(shop), 0.25f);
			return;
		}

		foreach (var item in shop) {
			long location = (LOCATION_TYPE_SHOP << 8) | (int)item;
			if (locationScoutData.TryGetValue(location, out var scout)) {
				var so = CurrencyItemManager.Instance.GetBuyable(item);
				so.Sprite = Items.GetIcon(scout);
				so.NameKey = string.Concat(scout.ItemDisplayName[0].ToString().ToUpperInvariant(), scout.ItemDisplayName.AsSpan(1)).Replace('<', '«');
				so.DescKey = $@"<color=#ee0065>{so.NameKey}</color>
{(scout.Player.Slot == ArchipelagoState.current.Me ? "Your" : $"{scout.Player.Name}'s")} {(scout.Flags) switch {
					ItemFlags.Advancement | ItemFlags.NeverExclude => "useful progression",
					ItemFlags.Advancement => "progression",
					ItemFlags.NeverExclude => "useful",
					ItemFlags.Trap => "trap",
					_ => "filler"
				}} item.";
				so.CategoryKey = Items.GetCategory(scout);

				shopLocations[so.Unlock == UnlockID.NONE ? so.RecipeUnlock.ToString() : so.Unlock.ToString()] = location;
			}
		}
	}

	public static bool Buy(string key) {
		if (shopLocations.TryGetValue(key, out var location)) {
			CheckExtra(location);
			var scout = locationScoutData[location];
			PopupHandler.Popup(
				"Archipelago",
				scout.Player.Slot == ArchipelagoState.current.Me ? "Bought " + scout.ItemDisplayName : $"Sent {scout.ItemDisplayName} to {scout.Player.Name}",
				Items.GetIcon(scout)
			);
			return true;
		}

		return false;
	}

	public static void SetUpSpaceUpgradeBox(SpaceUpgradeBox box, int i) {
		box.pointsRequired = Mathf.CeilToInt(box.pointsRequired / ArchipelagoState.current.slotData.spaceMult);
		
		var scout = locationScoutData[(LOCATION_TYPE_SPACE << 8) | i];
		box.nameKey = (scout.Player.Slot == ArchipelagoState.current.Me ? "Your " : $"{scout.Player.Name}'s ") + scout.ItemDisplayName;
		box.icon.sprite = box.bigIcon = Items.GetIcon(scout);
		var capName = string.Concat(scout.ItemDisplayName[0].ToString().ToUpperInvariant(), scout.ItemDisplayName.AsSpan(1));
		var description = $@"{(scout.Player.Slot == ArchipelagoState.current.Me ? "Your" : $"{scout.Player.Name}'s")} {(scout.Flags) switch {
				ItemFlags.Advancement | ItemFlags.NeverExclude => "useful progression",
				ItemFlags.Advancement => "progression",
				ItemFlags.NeverExclude => "useful",
				ItemFlags.Trap => "trap",
				_ => "filler"
			}
		} item.";
		var tmpt = box.tooltip.GetComponentInChildren<TMPTranslator>();
		tmpt.SetKey($"{capName}: {description}");
		tmpt.DoTranslate();

		var pause = Component.FindObjectOfType<SpacePause>();
		
		if (i == 0) {
			SetUpSpaceIcons(GameObject.Find("PowerIconStrip").transform);
			Plugin.Schedule(() => pause.ShowPowerInfo(0));
		}

		var power = pause.powers[i];
		power.desc = description;
		power.title = capName;
		power.icon = box.bigIcon;
	}

	public static void SpaceCheck(int i) {
		ArchipelagoState.current.save.spaceItemsSent[i] = true;
		ArchipelagoState.current.save.dirty = true;
		long location = (LOCATION_TYPE_SPACE << 8) | i;
		ArchipelagoState.current.Check(location);
		var scout = locationScoutData[location];
		var aso = AchievementManager.Instance.GetAchievement(AchievementID.SUPER_DivorcePapers + i);
		PopupHandler.Popup(
			"Archipelago",
			scout.Player.Slot == ArchipelagoState.current.Me ? $"Found {scout.ItemDisplayName}" : $"Sent {scout.ItemDisplayName} to {scout.Player.Name}",
			Items.GetIcon(scout),
			aso.NotifColor
		);
	}

	public static void SetUpSpaceIcons(Transform parent) {
		if (!Items.HasIconData) {
			Plugin.Schedule(() => SetUpSpaceIcons(parent), 0.25f);
			return;
		}

		for (int i = 0; i < 5; i++) {
			var child = parent.GetChild(i);
			var iim = child.GetComponent<Image>();
			Items.SetIcon((Items.ITEM_TYPE_SPACE << 8) | i, iim.sprite);
			var icon = Items.GetIcon(locationScoutData[(LOCATION_TYPE_SPACE << 8) | i]);
			var size = icon.rect.size;
			if (size.x == size.y) {
				iim.sprite = icon;
			} else {
				iim.sprite = icon;
				iim.color = new(0, 0, 0, 0);

				GameObject go = new();
				go.transform.SetParent(child, false);
				go.transform.SetAsFirstSibling();

				var im = go.AddComponent<Image>();
				im.sprite = icon;

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

	public static void Fusion(PowerupSO result) {
		long location = (LOCATION_TYPE_FUSION << 8) | (int)result.UpgradeId;
		if (locationScoutData.TryGetValue(location, out var scout) && !Checked(location)) {
			CheckExtra(location);
			var icon = Items.GetIcon(scout);
			if (icon == Plugin.archipelagoIcon)
				icon = result.Icon ?? result.IconOcean;
			PopupHandler.Popup(
				result.name,
				scout.Player.Slot == ArchipelagoState.current.Me ? $"Found {scout.ItemDisplayName}" : $"Sent {scout.ItemDisplayName} to {scout.Player.Name}",
				icon
			);
		}
	}
}