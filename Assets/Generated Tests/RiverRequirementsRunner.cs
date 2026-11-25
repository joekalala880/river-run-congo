using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Play-mode checks for River Run: Congo. Results are logged with the prefix "RIVER-CHECK".
public class RiverRequirementsRunner : MonoBehaviour
{
	int passed, failed;
	RiverGameManager manager;
	RiverSpawner spawner;
	Canoe canoe;

	IEnumerator Start()
	{
		DontDestroyOnLoad(gameObject);
		Find();

		// ---- Title screen ----
		yield return null;
		Check("Starts on the title screen (HUD hidden, river not moving forward)",
			manager.CurrentState == RiverGameManager.State.Title && manager.titlePanel.activeSelf && !manager.hud.activeSelf && manager.Speed == 0);
		Check("Rules fit about a 3 minute trip (" + manager.villageDistance / canoe.normalSpeed + " s at normal speed, sun sets at " + manager.sunsetSeconds + " s)",
			manager.villageDistance / canoe.normalSpeed < manager.sunsetSeconds && manager.villageDistance / canoe.normalSpeed > 120);

		manager.StartGame();
		spawner.enabled = false;
		yield return null;
		ClearRiver();
		Check("Enter starts the trip (HUD shown)", manager.IsPlaying && manager.hud.activeSelf && !manager.titlePanel.activeSelf);

		// ---- Moving ----
		float d0 = manager.distance;
		yield return new WaitForSeconds(0.5f);
		Check("Canoe always moves forward with the river (" + (manager.distance - d0).ToString("0.0") + " m in 0.5 s)", Near(manager.distance - d0, 7.5f, 1.5f));

		float x0 = canoe.transform.position.x;
		canoe.testSteer = 1;
		yield return new WaitForSeconds(0.3f);
		canoe.testSteer = 0;
		Check("Steer right (x " + x0.ToString("0.0") + " -> " + canoe.transform.position.x.ToString("0.0") + ")", canoe.transform.position.x > x0 + 1f);
		SetX(10);
		yield return null;
		Check("Canoe stays on the river (x = " + canoe.transform.position.x.ToString("0.0") + ")", canoe.transform.position.x <= canoe.bounds + 0.01f);
		SetX(0);

		// ---- Paddle hard / energy / slow down ----
		canoe.testBoost = true;
		yield return new WaitForSeconds(0.5f);
		Check("Space = paddle hard: faster (" + manager.Speed + " m/s) and uses energy (" + canoe.Energy.ToString("0") + ")", manager.Speed == canoe.boostSpeed && canoe.Energy < canoe.maxEnergy - 10);
		yield return new WaitForSeconds(3.5f);
		Check("Out of energy: back to normal speed (" + manager.Speed + " m/s), bar gray", !canoe.CanBoost && manager.Speed == canoe.normalSpeed);
		canoe.testBoost = false;
		float e0 = canoe.Energy;
		yield return new WaitForSeconds(1f);
		Check("Energy comes back while resting (" + e0.ToString("0") + " -> " + canoe.Energy.ToString("0") + ")", canoe.Energy > e0 + 10);
		yield return new WaitUntil(() => canoe.CanBoost);

		canoe.testSlow = true;
		yield return null;
		Check("S = slow down (" + manager.Speed + " m/s)", manager.Speed == canoe.slowSpeed);
		canoe.testSlow = false;
		yield return null;

		// ---- Supply box ----
		GameObject box = spawner.Spawn(spawner.boxPrefab, 0, 6);
		yield return new WaitForSeconds(0.8f);
		Check("Grabbing a supply box adds 1 (" + manager.Boxes + ") and the box is gone", manager.Boxes == 1 && box == null && manager.boxesText.text == "Supplies: 1 / 10");

		// ---- Rock ----
		spawner.Spawn(spawner.rockPrefab, 0, 6);
		yield return new WaitForSeconds(0.8f);
		Check("Hitting a rock breaks the canoe a little (health " + manager.Health + ")", manager.Health == 2 && canoe.IsSafe);
		spawner.Spawn(spawner.rockPrefab, 0, 6);
		yield return new WaitForSeconds(0.6f);
		Check("Short safe time after a hit (health still " + manager.Health + ")", manager.Health == 2);
		yield return new WaitUntil(() => !canoe.IsSafe);
		ClearRiver();

		// ---- Log ----
		SetHealth(3);
		SetX(1.2f);
		spawner.Spawn(spawner.logPrefab, 0, 6);
		yield return new WaitForSeconds(0.8f);
		Check("A log across the river hits the canoe even off-center (health " + manager.Health + ")", manager.Health == 2);
		yield return new WaitUntil(() => !canoe.IsSafe);
		SetX(0);
		ClearRiver();

		// ---- Hippo ----
		SetHealth(3);
		RiverObject hippo = spawner.Spawn(spawner.hippoPrefab, 0, 45).GetComponent<RiverObject>();
		yield return null;
		yield return null;
		Check("Hippo hides under the water at first (y = " + hippo.transform.position.y.ToString("0.0") + ")", hippo.transform.position.y < -1.5f && !hippo.HippoIsUp);
		yield return new WaitUntil(() => hippo == null || hippo.HippoIsUp);
		yield return new WaitForSeconds(0.5f);
		Check("Hippo pops up in front of the canoe (y = " + (hippo == null ? "gone" : hippo.transform.position.y.ToString("0.0")) + ")", hippo != null && hippo.transform.position.y > -0.3f);
		yield return new WaitForSeconds(1.2f);
		Check("Hitting a hippo hurts the canoe (health " + manager.Health + ")", manager.Health == 2);
		yield return new WaitUntil(() => !canoe.IsSafe);
		ClearRiver();
		SetHealth(3);

		// ---- Fast water ----
		SetX(0);
		GameObject water = spawner.Spawn(spawner.fastWaterPrefab, 0, 0);
		water.transform.localScale = new Vector3(5, 0.02f, 200);   // long strip, so it stays under the canoe
		float before = canoe.transform.position.x;
		yield return new WaitForSeconds(0.5f);
		Check("Fast water pushes the canoe sideways (x " + before.ToString("0.0") + " -> " + canoe.transform.position.x.ToString("0.0") + ")", canoe.transform.position.x > before + 1f);
		Destroy(water);
		SetX(0);

		// ---- Sun ----
		manager.elapsed = 0;
		yield return null;
		Color dayColor = manager.sun.color;
		float dayHeight = manager.sun.transform.eulerAngles.x;
		manager.elapsed = manager.sunsetSeconds * 0.9f;
		yield return null;
		Color lateColor = manager.sun.color;
		Check("The sun is the timer: it goes down and the light turns red (green " + dayColor.g.ToString("0.00") + " -> " + lateColor.g.ToString("0.00") + ")",
			lateColor.g < dayColor.g - 0.3f && manager.sun.transform.eulerAngles.x < dayHeight - 30);
		Check("Sky gets dark near the end", manager.cam.backgroundColor.grayscale < 0.4f);
		Check("Warning when it is getting late (\"" + manager.warningText.text + "\")", manager.warningText.text.Contains("Hurry"));
		manager.elapsed = 0;

		// ---- Spawner ----
		SetSafe(30);
		spawner.enabled = true;
		manager.distance += 600;
		yield return null;
		var kinds = new HashSet<RiverObject.Kind>();
		var boxes = new List<Transform>();
		var dangers = new List<Transform>();
		foreach (RiverObject r in FindObjectsOfType<RiverObject>())
		{
			kinds.Add(r.kind);
			if (r.kind == RiverObject.Kind.SupplyBox) boxes.Add(r.transform);
			if (r.IsDanger) dangers.Add(r.transform);
		}
		int risky = 0;
		foreach (Transform b in boxes)
			foreach (Transform d in dangers)
				if (Vector3.Distance(new Vector3(b.position.x, 0, b.position.z), new Vector3(d.position.x, 0, d.position.z)) < 2.5f) { risky++; break; }
		Check("River makes rocks, logs, hippos, boxes and fast water",
			kinds.Contains(RiverObject.Kind.Rock) && kinds.Contains(RiverObject.Kind.Log) && kinds.Contains(RiverObject.Kind.Hippo) &&
			kinds.Contains(RiverObject.Kind.SupplyBox) && kinds.Contains(RiverObject.Kind.FastWater));
		Check("Some boxes are right next to danger (" + risky + " of " + boxes.Count + ")", risky >= 1);
		ClearRiver();

		// ---- Village dock and win ----
		manager.distance = manager.villageDistance - 70;
		yield return null;
		yield return null;
		bool dock = false;
		foreach (RiverObject r in FindObjectsOfType<RiverObject>())
			if (r.kind == RiverObject.Kind.VillageDock) dock = true;
		Check("Village dock shows up at the end of the trip", dock);
		SetBoxes(12);
		manager.distance = manager.villageDistance - 1f;
		yield return new WaitForSeconds(0.3f);
		Check("WIN: reach the village before dark with 10+ boxes (\"" + manager.endTitle.text + "\")",
			manager.CurrentState == RiverGameManager.State.Won && manager.endPanel.activeSelf && manager.endText.text.Contains("12 / 10"));
		yield return null;
		Check("Everything stops after the end (speed " + manager.Speed + ")", manager.Speed == 0);

		// ---- Lose: not enough boxes ----
		yield return Restart();
		SetBoxes(4);
		manager.distance = manager.villageDistance - 1f;
		yield return new WaitForSeconds(0.3f);
		Check("LOSE: reach the village with fewer than 10 boxes (\"" + manager.endTitle.text + "\")",
			manager.CurrentState == RiverGameManager.State.Lost && manager.endTitle.text == "NOT ENOUGH SUPPLIES");

		// ---- Lose: dark ----
		yield return Restart();
		manager.elapsed = manager.sunsetSeconds - 0.1f;
		yield return new WaitForSeconds(0.3f);
		Check("LOSE: it gets dark before you reach the village (\"" + manager.endTitle.text + "\")",
			manager.CurrentState == RiverGameManager.State.Lost && manager.endTitle.text == "NIGHT FELL");

		// ---- Lose: canoe broken ----
		yield return Restart();
		manager.Damage();
		manager.Damage();
		manager.Damage();
		yield return null;
		Check("LOSE: the canoe loses all 3 health (\"" + manager.endTitle.text + "\")",
			manager.CurrentState == RiverGameManager.State.Lost && manager.endTitle.text == "YOUR CANOE BROKE");
		Check("End screen says how to play again", manager.endText.text.Contains("Press R"));

		manager.PlayAgain();
		yield return null;
		yield return null;
		Find();
		Check("Play again goes back to a fresh trip (health " + manager.Health + ", boxes " + manager.Boxes + ")",
			manager.CurrentState == RiverGameManager.State.Title && manager.Health == 3 && manager.Boxes == 0);

		Debug.Log("RIVER-CHECK DONE: " + passed + " passed, " + failed + " failed");
		Destroy(gameObject);
	}

	IEnumerator Restart()
	{
		manager.PlayAgain();
		yield return null;
		yield return null;
		Find();
		manager.StartGame();
		spawner.enabled = false;
		yield return null;
		ClearRiver();
	}

	void Find()
	{
		manager = FindObjectOfType<RiverGameManager>();
		spawner = FindObjectOfType<RiverSpawner>();
		canoe = FindObjectOfType<Canoe>();
	}

	void ClearRiver()
	{
		foreach (RiverObject r in FindObjectsOfType<RiverObject>())
			if (r.kind != RiverObject.Kind.Decoration)
				Destroy(r.gameObject);
	}

	void SetX(float x)
	{
		Vector3 p = canoe.transform.position;
		p.x = x;
		canoe.transform.position = p;
	}

	void SetHealth(int h) { typeof(RiverGameManager).GetProperty("Health").SetValue(manager, h); }
	void SetBoxes(int b) { typeof(RiverGameManager).GetProperty("Boxes").SetValue(manager, b); }
	void SetSafe(float s) { typeof(Canoe).GetField("safeTimer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(canoe, s); }

	static bool Near(float a, float b, float tolerance) { return Mathf.Abs(a - b) <= tolerance; }

	void Check(string what, bool ok)
	{
		if (ok) passed++; else failed++;
		Debug.Log("RIVER-CHECK " + (ok ? "PASS " : "FAIL ") + what);
	}
}
