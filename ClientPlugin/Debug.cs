using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using TMPro;
using UnityEngine;

namespace ClientPlugin;

class Debug : Ticker {
	int i;
	//Queue<Sprite> iconTest;

    public override void Tick() {
		if ((i = i + 1 & 15) > 0)
			return;

        if (Cmd("dumptree")) {
			foreach (var t in Component.FindObjectsOfType<Transform>())
				if (!t.parent)
					PGO(t.gameObject);
		}

		/*if (iconTest != null && iconTest.Count > 0 && !Component.FindObjectOfType<AchievementHandlerUI>().busyShowing) {
			var icon = iconTest.Dequeue();
			PopupHandler.Popup("Icon Test", "icon test", icon);
		}*/
    }

	static bool Cmd(string cmd) {
		if (File.Exists("z:\\tmp\\" + cmd)) {
			File.Delete("z:\\tmp\\" + cmd);
			return true;
		}
		return false;
	}

	static void PGO(GameObject go, int depth = 0) {
		string pf = new(' ', depth * 2);
		Plugin.L(pf + go.name);
		foreach (var c in go.GetComponents<TMP_Text>())
			Plugin.L($"{pf}\"{c.text.Replace("\n", "\\n")}\"");
		foreach (var c in go.GetComponents<Component>())
			Plugin.L(pf + "." + c.GetIl2CppType().Name);
		for (int i = 0; i < go.transform.GetChildCount(); i++)
			PGO(go.transform.GetChild(i).gameObject, depth + 1);
	}
}