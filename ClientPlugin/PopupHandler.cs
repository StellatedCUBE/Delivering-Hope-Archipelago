using System;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Json;
using UnityEngine;

namespace ClientPlugin;

class PopupHandler : Ticker
{
	public const AchievementID POPUP_DATA_OFFSET = (AchievementID)65536;

	static int popupIndex = 0;
	static float cooldown;
	static readonly AchievementSO[] popupData = new AchievementSO[64];
	static readonly Func<Sprite>[] icons = new Func<Sprite>[64];
	static AchievementHandlerUI achievementHandlerUI;
	static Vector2 iconBasePosition, iconBaseSize;
	static bool popupsAllowed = false;

	public static void Popup(DeathLink deathLink) => Popup(
		"Death Link",
		string.IsNullOrEmpty(deathLink.Cause) ? "From " + deathLink.Source : deathLink.Cause,
		() => Plugin.archipelagoIcon,
		Color.red
	);

	public static void Popup(string heading, string body, Sprite icon, Color border = default) => Popup(heading, body, () => icon, border);

	public static void Popup(string heading, string body, Func<Sprite> icon, Color border = default) {
		if (!popupsAllowed && ArchipelagoState.current.save.firstPlay)
			return;
		
		Plugin.L($"{heading}: {body}");

		popupData[popupIndex] ??= ScriptableObject.CreateInstance<AchievementSO>();

		if (popupData[popupIndex].Id != AchievementID.NONE)
			return;

		popupData[popupIndex].Title = heading;
		popupData[popupIndex].Description = body;
		popupData[popupIndex].Id = AchievementID.HopeAscended;
		popupData[popupIndex].NotifColor = border == default ? Color.white : border;
		icons[popupIndex] = icon;

		popupIndex = (popupIndex + 1) & 63;
	}

	public static AchievementSO GetAndClear(int i) {
		popupData[i].Id = AchievementID.NONE;
		popupData[i].Icon = icons[i]();
		return popupData[i];
	}
	
    public override void Tick() {
		if (!achievementHandlerUI) {
			if (achievementHandlerUI = Component.FindObjectOfType<AchievementHandlerUI>()) {
				Items.LoadObjectIconTransforms();
				
				if (iconBaseSize == default) {
					Items.LoadIcons();
					iconBasePosition = achievementHandlerUI.iconImg.transform.localPosition;
					iconBaseSize = achievementHandlerUI.iconImg.GetComponent<RectTransform>().sizeDelta;
					popupsAllowed = true;
				}

				cooldown = 0.5f;
			}
			return;
		}

		if (achievementHandlerUI.busyShowing) {
			var size = achievementHandlerUI.iconImg.sprite.rect.size;
			if (size.x > size.y) {
				achievementHandlerUI.iconImg.transform.localScale = new(1, size.y / size.x, 1);
				achievementHandlerUI.iconImg.transform.localPosition = iconBasePosition + new Vector2(0, Mathf.Lerp(iconBaseSize.y / -2, 0, size.y / size.x));
			} else {
				achievementHandlerUI.iconImg.transform.localScale = new(size.x / size.y, 1, 1);
				achievementHandlerUI.iconImg.transform.localPosition = iconBasePosition + new Vector2(Mathf.Lerp(iconBaseSize.x / 2, 0, size.x / size.y), 0);
			}
		} else {
			achievementHandlerUI.iconImg.transform.localPosition = iconBasePosition;
			achievementHandlerUI.iconImg.transform.localScale = Vector3.one;
		}

		if (cooldown > 0) {
			cooldown -= Time.deltaTime;
			return;
		}

        for (int i = 0; i < 64; i++) {
			var data = popupData[i];
			if (data == null)
				break;
			
			if (data.Id == AchievementID.HopeAscended) {
				data.Id = AchievementID.SeeTheWorld;
				achievementHandlerUI.EnqueueNotification((AchievementID)(i + POPUP_DATA_OFFSET));
			}
		}
    }
}