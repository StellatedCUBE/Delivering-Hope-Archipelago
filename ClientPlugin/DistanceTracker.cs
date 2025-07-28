using UnityEngine;

namespace ClientPlugin;

class DistanceTracker : Ticker {
	readonly PlayScreenManager psm;
	readonly GameMode mode;

	public DistanceTracker() {
		psm = PlayScreenManager.Instance;
		mode = GameManager.Instance.CurrentGameMode;
	}

    public override void Tick() {
        if (psm) 
			ArchipelagoState.current.save.UpdateDistanceRecord(mode, psm.playerDistance);
		else
			Remove();
    }
}