using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BarGraph : MonoBehaviour {
	public GameObject blockPrefab_;
	const int BLOCK_NUM = 16;
	const float min_y = -32f;
	const float max_y = 32f;
	const float block_height = (max_y - min_y)/BLOCK_NUM;

	private List<GameObject> block_list_;
	private RingBuffer<float> ring_buffer_;

	// Use this for initialization
	void Start () {
		ring_buffer_ = new RingBuffer<float>(64*256 /* capacity */);
		block_list_ = new List<GameObject> ();
		for (var i = 0; i < BLOCK_NUM; ++i) {
			var go = Instantiate (blockPrefab_) as GameObject;
			var trfm = go.transform;
			trfm.SetParent (this.transform);
			trfm.localScale = new Vector3(1f, block_height, 1f);
			trfm.position = new Vector3(0f, block_height*i, 0f);
			block_list_.Add(go);
		}
	}

	public void put(float value)
	{
		ring_buffer_.Add(value);
	}

	// Update is called once per frame
	void Update () {
		int total_num = ring_buffer_.Capacity;
		int unit_num = total_num / BLOCK_NUM;
		int top = 0;
		foreach (var block in block_list_) {
			//var sprite = block.GetComponent<Sprite>();
			var transform = block.transform;
			float value = 0f;
			for (int i = top; i < unit_num + top; ++i) {
				float v = i < ring_buffer_.Count ? Mathf.Abs (ring_buffer_.getElemR(i)) : 0f;
				if (value < v) {
					value = v;
				}
			}
			top += unit_num;
			transform.localScale = new Vector3(Mathf.Min (value, 0.5f)*64f+0.1f, block_height, 1f);
			var color = new Color(1f, 1f, 1f);
			if (value > 0.5f) {
				color.b = 2f - value*2f;
				color.g = color.b;
			}
			block.GetComponent<SpriteRenderer>().color = color;
		}
	}
}
