using System;
using System.Collections;
using System.IO;
using UnityEngine;

// Saves preview pictures of the running game to Downloads/RiverRun_Files (used by the RiverSnapshot test).
public class RiverSnapshotTaker : MonoBehaviour
{
	const int Width = 1280, Height = 720;
	public bool Finished { get; private set; }

	IEnumerator Start()
	{
		Camera cam = Camera.main;
		RenderTexture rt = new RenderTexture(Width, Height, 24);
		Canvas canvas = FindObjectOfType<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.worldCamera = cam;
		canvas.planeDistance = 1;

		RiverGameManager manager = FindObjectOfType<RiverGameManager>();
		RiverSpawner spawner = FindObjectOfType<RiverSpawner>();
		Canoe canoe = FindObjectOfType<Canoe>();

		string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads/RiverRun_Files");
		Directory.CreateDirectory(folder);

		yield return new WaitForSeconds(0.5f);
		Save(cam, rt, Path.Combine(folder, "preview_title.png"));

		manager.StartGame();
		spawner.enabled = false;
		typeof(Canoe).GetField("safeTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(canoe, 999f);
		spawner.Spawn(spawner.boxPrefab, -2.5f, 22);
		spawner.Spawn(spawner.rockPrefab, -0.9f, 23.5f);
		spawner.Spawn(spawner.logPrefab, 2f, 38);
		spawner.Spawn(spawner.hippoPrefab, -1.5f, 52);
		spawner.Spawn(spawner.boxPrefab, 3.5f, 55);
		spawner.Spawn(spawner.fastWaterPrefab, 0.5f, 70);
		yield return new WaitForSeconds(0.4f);
		Save(cam, rt, Path.Combine(folder, "preview_river.png"));

		yield return new WaitForSeconds(1.6f);
		Save(cam, rt, Path.Combine(folder, "preview_hippo_and_current.png"));

		manager.elapsed = manager.sunsetSeconds * 0.8f;
		spawner.Spawn(spawner.boxPrefab, 1f, 20);
		spawner.Spawn(spawner.rockPrefab, -2f, 30);
		yield return new WaitForSeconds(0.2f);
		Save(cam, rt, Path.Combine(folder, "preview_sunset.png"));

		manager.distance = manager.villageDistance - 30f;
		typeof(RiverGameManager).GetProperty("Boxes").SetValue(manager, 11);
		spawner.enabled = true;
		yield return new WaitForSeconds(0.1f);
		spawner.enabled = false;
		yield return new WaitForSeconds(1.0f);
		Save(cam, rt, Path.Combine(folder, "preview_village.png"));
		yield return new WaitUntil(() => !manager.IsPlaying);
		Save(cam, rt, Path.Combine(folder, "preview_win.png"));
		Finished = true;
	}

	static void Save(Camera cam, RenderTexture rt, string path)
	{
		cam.targetTexture = rt;
		cam.Render();
		RenderTexture.active = rt;
		Texture2D tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
		tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
		tex.Apply();
		RenderTexture.active = null;
		cam.targetTexture = null;
		File.WriteAllBytes(path, tex.EncodeToPNG());
		Debug.Log("RIVER-SNAPSHOT " + path);
	}
}
