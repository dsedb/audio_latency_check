using UnityEngine;
using System.Collections;

public class GraphManager : MonoBehaviour {
	public Material lineMat;
	public Material guideLineMat;

	private float gauge_value_ = 1f;
	private RingBuffer<float> ring_buffer_;

	// Use this for initialization
	void Start () {
		ring_buffer_ = new RingBuffer<float>(64*256 /* capacity */);
	}
	
	public void setGauge(float value)
	{
		gauge_value_ = value;
	}

	public void put(float value)
	{
		ring_buffer_.Add(value);
	}

	private void draw_graph()
	{
		lineMat.SetPass(0);
		float scale = 128.0f;
		int num = ring_buffer_.Count;
		float x_step = 0.0015f;
		float x = 0.0f - num*x_step;
        GL.PushMatrix();
		GL.MultMatrix (transform.localToWorldMatrix);
		GL.Begin(GL.LINES);
		for (var i = 0; i < num-1; ++i) {
			if (i%4 == 0) {
				GL.Vertex3(x,        ring_buffer_[i+0]*scale, 0);
				GL.Vertex3(x+x_step, ring_buffer_[i+1]*scale, 0);
			}
			x += x_step;
		}
		GL.End();
		guideLineMat.SetPass(0);
		GL.Begin(GL.LINES);
		GL.Vertex3(-num*x_step,  gauge_value_*scale, 0);
		GL.Vertex3(0f,           gauge_value_*scale, 0);
		GL.Vertex3(-num*x_step, -gauge_value_*scale, 0);
		GL.Vertex3(0f,          -gauge_value_*scale, 0);
		GL.End();
		GL.PopMatrix();
	}

	void OnRenderObject ()
	{
		draw_graph();
	}
}
