using UnityEngine;

namespace ClientPlugin;

static class PowerUps {
	public static bool Allowed(Upgrade id) => (id) switch {
		Upgrade.BouncePlus => ArchipelagoState.current.Unlocked(PoolableType.Banana),
		Upgrade.AerialDuration => ArchipelagoState.current.Unlocked(PoolableType.Glider) || ArchipelagoState.current.Unlocked(PoolableType.Jetpack),
		Upgrade.JetpackPower => ArchipelagoState.current.Unlocked(PoolableType.Jetpack),
		Upgrade.CalliPlus => ArchipelagoState.current.Unlocked(PoolableType.Booster),
		Upgrade.KiaraDuration => ArchipelagoState.current.Unlocked(PoolableType.Kiara),
		Upgrade.KroniiPlus => ArchipelagoState.current.Unlocked(PoolableType.Kronii),
		Upgrade.MumeiPlus => ArchipelagoState.current.Unlocked(PoolableType.Mumei),
		Upgrade.LuckyDice => ArchipelagoState.current.Unlocked(PoolableType.Monopoly),
		Upgrade.FaunaSlap => ArchipelagoState.current.Unlocked(PoolableType.Fauna),
		Upgrade.GuraRawr => ArchipelagoState.current.Unlocked(PoolableType.Gura),
		Upgrade.InaPortal => ArchipelagoState.current.Unlocked(PoolableType.Ina),
		Upgrade.KaelaBonk => ArchipelagoState.current.downboostUnlocked,
		Upgrade.KoboBonk => ArchipelagoState.current.downboostUnlocked,
		Upgrade.Vertical => ArchipelagoState.current.Unlocked(Upgrade.Horizontal),
		Upgrade.TripleSoda => ArchipelagoState.current.Unlocked(Upgrade.DoubleSoda),
        _ => ArchipelagoState.current.Unlocked(id)
	};
}