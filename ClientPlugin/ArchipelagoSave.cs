using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Json;

namespace ClientPlugin;

public class ArchipelagoSave {
	[JsonIgnore]
	public bool firstPlay = true;
	[JsonIgnore]
	public bool dirty = false;

	public int lastItemIndex = 0;
	public bool? deathLink = null;
	public List<long> checks = [];
	//public int freeSpaceFlights = 0;
	public bool[] spaceItemsSent = new bool[5];
	public int queuedHopeStones = 0;
	public float bestDistanceStandard = 0;
	public float bestDistanceClassic = 0;
	public float bestDistanceBaeless = 0;
	public float bestDistanceGacha = 0;
	public float bestDistanceSpace = 0;

	public void UpdateDistanceRecord(GameMode mode, float distance) {
		switch (mode) {
			case GameMode.Standard:
				bestDistanceStandard = Math.Max(bestDistanceStandard, distance);
				break;

			case GameMode.Classic:
				bestDistanceClassic = Math.Max(bestDistanceClassic, distance);
				break;

			case GameMode.Baeless:
				bestDistanceBaeless = Math.Max(bestDistanceBaeless, distance);
				break;

			case GameMode.Chaos:
				bestDistanceGacha = Math.Max(bestDistanceGacha, distance);
				break;

			case GameMode.Space:
				bestDistanceSpace = Math.Max(bestDistanceSpace, distance);
				break;
		}
	}
}