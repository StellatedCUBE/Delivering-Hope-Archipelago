using System.Collections.Generic;

namespace ClientPlugin;

static class Spawner {
	static int jailPosition;
	static bool jailJustSpawnedNone;

	public static PoolableType Next(SpawnSequencer spawnSequencer, GameMode mode, GameZone zone) {
		PoolableType @object = PoolableType.NONE;

		if (mode == GameMode.TheCell) {
			if (spawnSequencer.spawnedObjectCount == 0) {
				jailPosition = 0;
				jailJustSpawnedNone = false;
			}

			@object = spawnSequencer.jailSpawnArray[jailPosition++ % spawnSequencer.jailSpawnArray.Length];
			if (!ArchipelagoState.current.Unlocked(@object == PoolableType.Mococo ? PoolableType.Fuwawa : @object)) {
				if (jailJustSpawnedNone) {
					do {
						@object = spawnSequencer.jailSpawnArray[jailPosition++ % spawnSequencer.jailSpawnArray.Length];
					} while (!ArchipelagoState.current.Unlocked(@object == PoolableType.Mococo ? PoolableType.Fuwawa : @object));
				} else {
					@object = PoolableType.NONE;
				}
			}
			jailJustSpawnedNone = @object == PoolableType.NONE;
		}
		
		else if (spawnSequencer.spawnedObjectCount > 0) {
			if (zone == GameZone.Ocean)
				mode = GameMode.TheCell;
			
			List<PoolableType> available = (mode) switch {
				GameMode.Baeless => [
					PoolableType.Soda,
					PoolableType.Banana,
					PoolableType.Booster,
					PoolableType.Kronii,
					PoolableType.Mumei,
					PoolableType.Kiara
				],
				GameMode.Classic => [
					PoolableType.Soda,
					PoolableType.Banana,
					PoolableType.Bush,
					PoolableType.Tree,
					PoolableType.Booster
				],
				GameMode.TheCell => [
					PoolableType.Soda,
					PoolableType.Banana,
					PoolableType.Tofu,
					PoolableType.Strawberry,
					PoolableType.Tree,
					PoolableType.Gura,
					PoolableType.Ina,
					PoolableType.Fauna,
					PoolableType.Kiara
				],
				_ => [
					PoolableType.Soda,
					PoolableType.Banana,
					PoolableType.Bush,
					PoolableType.Strawberry,
					PoolableType.Tree,
					PoolableType.Booster,
					PoolableType.Kronii,
					PoolableType.Mumei,
					PoolableType.Kiara
				],
			};

			float baseLen = available.Count;
			available.RemoveAll(o => !ArchipelagoState.current.Unlocked(o));
			float scale = available.Count / baseLen;

			var osl = new int[23];

			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Kiara, spawnSequencer.lastKiaraSpawned, spawnSequencer.kiaraMax);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Tofu, spawnSequencer.lastTofuSpawned, spawnSequencer.tofuMax * scale);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Bush, spawnSequencer.lastBushSpawned, spawnSequencer.bushMax * scale);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Soda, spawnSequencer.lastSodaSpawned, spawnSequencer.sodaMax * scale);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Tree, spawnSequencer.lastTreeSpawned, spawnSequencer.treeMax * scale);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Fauna, spawnSequencer.lastFaunaSpawned, spawnSequencer.faunaMax * scale);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Gura, spawnSequencer.lastGuraSpawned, spawnSequencer.guraMax * scale);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Ina, spawnSequencer.lastInaSpawned, spawnSequencer.inaMax * scale);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Booster, spawnSequencer.lastSpringSpawned, spawnSequencer.springMax * scale);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Mumei, spawnSequencer.lastMumeiSpawned, spawnSequencer.mumeiMax * scale);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Kronii, spawnSequencer.lastKroniiSpawned, spawnSequencer.kroniiMax * scale);
			CheckMust(osl, spawnSequencer, available, ref @object, PoolableType.Banana, spawnSequencer.lastBananaSpawned, spawnSequencer.bananaMax * scale);
			osl[(int)PoolableType.Strawberry] = spawnSequencer.spawnedObjectCount - spawnSequencer.lastStrawberrySpawned;

			if (@object == PoolableType.NONE) {
				var mins = new float[osl.Length];

				mins[(int)PoolableType.Kiara] = spawnSequencer.kiaraMin;
				mins[(int)PoolableType.Tofu] = scale * spawnSequencer.tofuMin;
				mins[(int)PoolableType.Bush] = scale * spawnSequencer.bushMin;
				mins[(int)PoolableType.Soda] = scale * spawnSequencer.sodaMin;
				mins[(int)PoolableType.Tree] = scale * spawnSequencer.treeMin;
				mins[(int)PoolableType.Fauna] = scale * spawnSequencer.faunaMin;
				mins[(int)PoolableType.Gura] = scale * spawnSequencer.guraMin;
				mins[(int)PoolableType.Ina] = scale * spawnSequencer.inaMin;
				mins[(int)PoolableType.Booster] = scale * spawnSequencer.springMin;
				mins[(int)PoolableType.Mumei] = scale * spawnSequencer.mumeiMin;
				mins[(int)PoolableType.Kronii] = scale * spawnSequencer.kroniiMin;
				mins[(int)PoolableType.Banana] = scale * spawnSequencer.bananaMin;
				mins[(int)PoolableType.Strawberry] = scale * spawnSequencer.strawberrySpacingMin;

				for (int i = 0; i < osl.Length; i++)
					if (osl[i] > spawnSequencer.spawnedObjectCount)
						osl[i] = int.MaxValue;
				
				available.RemoveAll(o => mins[(int)o] > osl[(int)o]);
				available.Add(PoolableType.NONE);

				@object = available[UnityEngine.Random.Range(0, available.Count)];
				if (@object == PoolableType.Strawberry && UnityEngine.Random.value > spawnSequencer.strawberryChance)
					@object = PoolableType.NONE;
			}
		}

		switch (@object) {
			case PoolableType.Tree:
			case PoolableType.BaeOcean:
				spawnSequencer.lastTreeSpawned = spawnSequencer.spawnedObjectCount;
				break;

			case PoolableType.Bush:
				spawnSequencer.lastBushSpawned = spawnSequencer.spawnedObjectCount;
				break;

			case PoolableType.Soda:
			case PoolableType.LargeSoda:
			case PoolableType.DoubleSoda:
			case PoolableType.TripleSoda:
				spawnSequencer.lastSodaSpawned = spawnSequencer.spawnedObjectCount;
				break;

			case PoolableType.Booster:
				spawnSequencer.lastSpringSpawned = spawnSequencer.spawnedObjectCount;
				break;

			case PoolableType.Banana:
				spawnSequencer.lastBananaSpawned = spawnSequencer.spawnedObjectCount;
				break;
			
			case PoolableType.Kronii:
				spawnSequencer.lastKroniiSpawned = spawnSequencer.spawnedObjectCount;
				break;

			case PoolableType.Mumei:
				spawnSequencer.lastMumeiSpawned = spawnSequencer.spawnedObjectCount;
				break;

			case PoolableType.Kiara:
				spawnSequencer.lastKiaraSpawned = spawnSequencer.spawnedObjectCount;
				break;

			case PoolableType.Ina:
				spawnSequencer.lastInaSpawned = spawnSequencer.spawnedObjectCount;
				break;

			case PoolableType.Gura:
				spawnSequencer.lastGuraSpawned = spawnSequencer.spawnedObjectCount;
				break;

			case PoolableType.Fauna:
				spawnSequencer.lastFaunaSpawned = spawnSequencer.spawnedObjectCount;
				break;

			case PoolableType.Tofu:
				spawnSequencer.lastSodaSpawned = spawnSequencer.spawnedObjectCount;
				break;
			
			case PoolableType.Strawberry:
			case PoolableType.Strawberry_x2:
				spawnSequencer.lastStrawberrySpawned = spawnSequencer.spawnedObjectCount;
				break;
		}

		if (zone == GameZone.Ocean && @object == PoolableType.Tree)
			@object = PoolableType.BaeOcean;
		else if (@object == PoolableType.Strawberry && GamesaveHandler.Instance.GetAchievement(AchievementID.SUPER_Strawberry))
			@object = PoolableType.Strawberry_x2;
		else if (@object == PoolableType.Soda) {
			if (zone == GameZone.Normal) {
				if (UnityEngine.Random.value < FlightParamsManager.Instance.doubleSodaChance)
					@object = PoolableType.DoubleSoda;
				else if (UnityEngine.Random.value < FlightParamsManager.Instance.largeSodaChance)
					@object = PoolableType.LargeSoda;
			} else if (UnityEngine.Random.value < FlightParamsManager.Instance.tripleSodaChance)
				@object = PoolableType.TripleSoda;
		}

		return @object;
	}

	static void CheckMust(int[] osl, SpawnSequencer spawnSequencer, List<PoolableType> available, ref PoolableType @object, PoolableType type, int last, float max) {
		if (available.Contains(type) && (osl[(int)type] = spawnSequencer.spawnedObjectCount - last) >= max && @object == PoolableType.NONE)
			@object = type;
	}
}