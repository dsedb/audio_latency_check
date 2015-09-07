using UnityEngine;
using System.Collections;

public class SoundPlayer : MonoBehaviour {

	public float frequency = 440f;
	public float gain = 1f;

	const float sampling_frequency_ = 48000f;
	private float phase_;
	private bool playing_ = false;

	public void startPlay()
	{
		playing_ = true;
	}

	public void stopPlay()
	{
		playing_ = false;
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		if (!playing_)
			return;

		float increment = frequency * 2 * Mathf.PI / sampling_frequency_;
		
		for (var i = 0; i < data.Length; i = i + channels)
		{
			phase_ += increment;
			data[i] = gain*Mathf.Sin(phase_);
			if (channels >= 2) {
				for (var j = 1; j < channels; ++j) {
					data[i+j] = data[i];
				}
			}
			phase_ = Mathf.Repeat(phase_, 2 * Mathf.PI);
		}
	}
}
