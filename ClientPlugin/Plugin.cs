using System.Text;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using Archipelago.MultiClient.Net.Json;

namespace ClientPlugin;

[BepInPlugin("moe.shinten.deliveringhopearchipelago", "Archipelago", "0.1.0")]
public class Plugin : BasePlugin
{
    internal static Sprite archipelagoIcon;
    internal static new ManualLogSource Log;
    static readonly List<Ticker> tickers = [], tickersToAdd = [];

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Harmony.CreateAndPatchAll(typeof(HarmonyPatches));
    }

    internal static void LoadData() {
        L("Loading sprite data");
        var iconData = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Archipelago.png"));
        Texture2D iconTexture = new(2, 2, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(iconTexture, iconData);
        archipelagoIcon = Sprite.Create(iconTexture, new(0, 0, iconTexture.width, iconTexture.height), new());
    }

    /*
    internal static bool o1pd = false;
    public static void On1Play() {
        static string TL(string key) => Translator.Instance.GetLocalized(key);
        //static string TLA(IEnumerable<string> keys) => string.Join("//", keys.Select(TL));

        Dictionary<string, string> map = new();
        foreach (var key in Translator.Instance.localizedTexts.Keys)
            map.Add(key, TL(key));
        File.WriteAllText("z:\\tmp\\dh_dump", JObject.FromObject(map).ToJSON());
    }*/

    internal static void Tick() {
        tickers.AddRange(tickersToAdd);
        tickersToAdd.Clear();

        foreach (var ticker in tickers)
            ticker.Tick();
        
        tickers.RemoveAll(ticker => ticker.removalQueued);
    }

    public static void AddTicker(Ticker ticker) => tickersToAdd.Add(ticker);

    public static void AddTickerUniqueType(Ticker ticker) {
        var type = ticker.GetType();
        if (!tickers.Concat(tickersToAdd).Any(t => t.GetType() == type && !t.removalQueued))
            AddTicker(ticker);
    }

    public static void Schedule(Action action, float delay = 0) => AddTicker(new ScheduledAction(action, delay));

    public static void L(object o) => Log.LogInfo(o);

    static void SetSpaceAchievements(bool[] flags) {
		for (int i = 0; i < 5; i++) {
			GamesaveHandler.Instance.AchievementsUnlocked[AchievementID.SUPER_DivorcePapers + i] = flags[i];
		}
	}

    internal static SpaceItemMode spaceItemMode = SpaceItemMode.NotYetInitiated;
	public static void SetSpaceItemMode(SpaceItemMode mode) {
		if (mode != spaceItemMode) {
			spaceItemMode = mode;
			SetSpaceAchievements(mode == SpaceItemMode.Sent ? ArchipelagoState.current.save.spaceItemsSent : Items.spaceItemsReceived);
		}
	}

    public static void ForceUpdateSpaceItems() {
        var mode = spaceItemMode;
        spaceItemMode = SpaceItemMode.NotYetInitiated;
        SetSpaceItemMode(mode);
    }
}
