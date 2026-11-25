using UnityEngine;

// Puts rocks, logs, hippos, supply boxes and fast water in the river ahead of the canoe.
// Things are placed by distance (every few meters), so paddling hard does not make the river easier.
public class RiverSpawner : MonoBehaviour
{
	public RiverGameManager manager;

	[Header("Prefabs")]
	public GameObject rockPrefab;
	public GameObject logPrefab;
	public GameObject hippoPrefab;
	public GameObject boxPrefab;
	public GameObject fastWaterPrefab;
	public GameObject treePrefab;
	public GameObject villageDockPrefab;

	[Header("Spawning")]
	public float spawnZ = 80f;
	public float metersBetweenRows = 13f;
	public float riverHalfWidth = 4.5f;
	public float metersBetweenTrees = 9f;

	float nextRow;
	float nextTree;
	bool dockPlaced;

	void Start()
	{
		nextRow = 25f;
		// Trees along both banks from the start, so the scene looks full on the title screen
		for (float z = -10f; z < spawnZ; z += metersBetweenTrees)
			SpawnTrees(z);
		nextTree = metersBetweenTrees;
	}

	void Update()
	{
		if (!manager.IsPlaying)
			return;

		float d = manager.distance;

		while (d >= nextTree)
		{
			SpawnTrees(spawnZ - (d - nextTree));
			nextTree += metersBetweenTrees;
		}

		// The village dock appears at the end of the trip
		if (!dockPlaced && manager.Remaining <= spawnZ)
		{
			dockPlaced = true;
			Spawn(villageDockPrefab, 0, manager.Remaining);
		}

		// Keep the last part before the dock clear
		while (d >= nextRow && manager.Remaining > spawnZ + 15f)
		{
			SpawnRow(spawnZ - (d - nextRow));
			nextRow += metersBetweenRows * Random.Range(0.8f, 1.2f);
		}
	}

	void SpawnRow(float z)
	{
		float roll = Random.value;
		float x = Random.Range(-riverHalfWidth, riverHalfWidth);

		if (roll < 0.26f)
		{
			Spawn(rockPrefab, x, z);
			// Sometimes a second rock, to make a narrow gap
			if (Random.value < 0.4f)
				Spawn(rockPrefab, Mathf.Clamp(x + (x > 0 ? -3.4f : 3.4f), -riverHalfWidth, riverHalfWidth), z + Random.Range(-1f, 1f));
		}
		else if (roll < 0.42f)
		{
			// Logs lie across the river and block part of it
			Spawn(logPrefab, Random.Range(-riverHalfWidth + 1.5f, riverHalfWidth - 1.5f), z);
		}
		else if (roll < 0.56f)
		{
			Spawn(hippoPrefab, x, z);
		}
		else if (roll < 0.82f)
		{
			// Supply box, often right next to danger (risk and reward)
			Spawn(boxPrefab, x, z);
			if (Random.value < 0.5f)
			{
				float side = x > 0 ? -1.6f : 1.6f;
				Spawn(Random.value < 0.6f ? rockPrefab : hippoPrefab, Mathf.Clamp(x + side, -riverHalfWidth, riverHalfWidth), z + 1.5f);
			}
		}
		else if (roll < 0.92f)
		{
			GameObject water = Spawn(fastWaterPrefab, Random.Range(-1.5f, 1.5f), z);
			water.GetComponent<RiverObject>().pushSpeed *= Random.value < 0.5f ? -1f : 1f;
		}
		// else: an empty stretch of river, a short rest
	}

	void SpawnTrees(float z)
	{
		Spawn(treePrefab, -7.2f - Random.value * 1.5f, z + Random.Range(-2f, 2f));
		Spawn(treePrefab, 7.2f + Random.value * 1.5f, z + Random.Range(-2f, 2f));
	}

	public GameObject Spawn(GameObject prefab, float x, float z)
	{
		GameObject go = Instantiate(prefab);
		Vector3 p = go.transform.position;
		p.x = x;
		p.z = z;
		go.transform.position = p;
		go.GetComponent<RiverObject>().manager = manager;
		if (prefab == treePrefab)
			go.transform.localScale *= Random.Range(0.8f, 1.3f);
		return go;
	}
}
