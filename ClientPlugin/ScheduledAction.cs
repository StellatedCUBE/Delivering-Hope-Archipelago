using System;
using UnityEngine;

namespace ClientPlugin;

class ScheduledAction(Action action, float delay) : Ticker {
	float delay = delay;
	readonly Action action = action;

    public override void Tick() {
        delay -= Time.deltaTime;

		if (delay < 0) {
			Remove();
			action();
		}
    }
}