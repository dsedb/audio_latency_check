using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class MicrophoneFetcher : MonoBehaviour {
	AudioClip audio_clip_;

	private bool granted_ = false;
	private string device_name_;
	private int processed_samples_ = 0;
	private BarGraph bar_graph_;
	private float loud_point_time_ = 0f;
	private int loud_point_sample_ = 0;
	private int frequency_;
	private System.Diagnostics.Stopwatch sw_;

	[DllImport("__Internal")]
	private static extern void requestRecordPermission();
	[DllImport("__Internal")]
	private static extern int getRecordPermissionStatus();

	IEnumerator Start()
	{
		{
			var go = GameObject.Find ("bar_graph");
			bar_graph_ = go.GetComponent<BarGraph> ();
		}
#if UNITY_IOS && !UNITY_EDITOR
		requestRecordPermission();
		int status = getRecordPermissionStatus();
		Debug.Log("status="+status.ToString());
		if (status == 1) {
			granted_ = true;
		}
#else
		granted_ = true;
#endif
		yield return null;

		device_name_ = "";	// default
		if (Microphone.devices.Length > 0) {
			device_name_ = Microphone.devices[0];
		}
		int minFreq;
		int maxFreq;
		Microphone.GetDeviceCaps(device_name_, out minFreq, out maxFreq);
		frequency_ = Mathf.Clamp (10000, minFreq, maxFreq);
		if (frequency_ <= 0)
			frequency_ = 10000;
		audio_clip_ = Microphone.Start(device_name_, true /* loop */, 10 /* lengthSec */, frequency_);
		//Debug.Log(audio_clip_.channels);

		sw_ = new System.Diagnostics.Stopwatch();
	}

	void Update()
	{
		if (!granted_)
			return;

		const float threshold = 0.01f;

		int progress = Microphone.GetPosition (device_name_);
		var new_sample_num = progress - processed_samples_;
		if (new_sample_num > 0) {
			var buffer = new float[new_sample_num * audio_clip_.channels];
			audio_clip_.GetData (buffer, processed_samples_);
			for (var i = 0; i < new_sample_num; i += audio_clip_.channels) {
				float value = buffer[i]; // only the first channel
				// if (Mathf.Abs (value) < 0.01f)
				// 	continue;
				if (Mathf.Abs(value) > threshold && loud_point_time_ == 0) {
					loud_point_time_ = Time.time;
					loud_point_sample_ = i;
					sw_.Stop();
				}
				bar_graph_.put (value);
			}
		} else {
			bar_graph_.put (0);
		}
		processed_samples_ += new_sample_num;
	}

	public void beginAnalysis()
	{
		loud_point_time_ = 0;
		loud_point_sample_ = 0;
		sw_.Reset();
		sw_.Start();
	}

	public bool endAnalysis(float start_time,
							out float latency)
	{
		if (loud_point_time_ < start_time) {
			Debug.Log(string.Format("something wrong.. {0} < {1}", loud_point_time_, start_time));
			latency = 0f;
			return false;
		}
		// float diff_time = loud_point_time_ - start_time;
		float diff_time = (float)sw_.Elapsed.TotalSeconds;
		latency = diff_time + (float)loud_point_sample_/ (float)frequency_;
		// latency = diff_time;
		return true;
	}
}
