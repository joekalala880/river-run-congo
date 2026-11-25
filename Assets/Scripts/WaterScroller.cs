using UnityEngine;

// Scrolls the water texture so the river looks like it is moving (same idea as the book's TextureScroller).
public class WaterScroller : MonoBehaviour
{
	public RiverGameManager manager;
	public float idleSpeed = 3f;          // the river still flows on the title screen
	public float metersPerTile = 10f;

	Renderer rend;
	float offset;

	void Start()
	{
		rend = GetComponent<Renderer>();
	}

	void Update()
	{
		float speed = manager.IsPlaying ? manager.Speed : idleSpeed;
		offset += speed / metersPerTile * Time.deltaTime;
		rend.material.mainTextureOffset = new Vector2(0, offset);
	}
}
