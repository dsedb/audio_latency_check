using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Measure : MonoBehaviour {

	private SoundPlayer sound_player_;
	private MicrophoneFetcher microphone_fetcher_;
	private Button button_start_;
	private Button button_stop_;
	private Text info_text_;
	private Text message_text_;
	private Coroutine coroutine_;

	void Start()
	{
		sound_player_ = GameObject.Find("Main Camera").GetComponent<SoundPlayer>();
		microphone_fetcher_ = GameObject.Find("microphone").GetComponent<MicrophoneFetcher>();
		button_start_ = GameObject.Find("ButtonStart").GetComponent<Button>();
		button_stop_ = GameObject.Find("ButtonStop").GetComponent<Button>();
		info_text_ = GameObject.Find("InfoText").GetComponent<Text>();
		message_text_ = GameObject.Find("MessageText").GetComponent<Text>();
		showInfo ();
	}
	void showInfo()
	{
		string info = "";
		foreach (var device in Microphone.devices) {
			info += device + "\n";
		}
		info_text_.text = info;
	}
		
	void Update()
	{
		showInfo ();
	}	

	IEnumerator play()
	{
		message_text_.text = "measuring:";

		int times_to_try = 10;
		var results = new List<float>();

		int tried = 0;
		for (var i = 0; i < 99; ++i) {
			float start_time = Time.time;
			microphone_fetcher_.beginAnalysis();
			sound_player_.startPlay();
			yield return new WaitForSeconds(0.5f);
			sound_player_.stopPlay();
			float latency;
			bool success = microphone_fetcher_.endAnalysis(start_time, out latency);
			if (success) {
				results.Add(latency);
				++tried;
				message_text_.text = message_text_.text + "o";
				if (tried >= times_to_try) {
					break;
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
		results.Sort ();
		if (results.Count > 2)
			results.RemoveAt (0);
		if (results.Count > 1)
			results.RemoveAt (results.Count-1);
		if (results.Count > 0) {
			float ave = 0f;
			foreach (float value in results) {
				ave += value;
			}
			ave /= (float)results.Count;
			message_text_.text = string.Format ("latency: {0} msec", ave * 1000f);
		}
		OnPressedStop();
	}

	public void OnPressedStart()
	{
		button_start_.interactable = false;
		button_stop_.interactable = true;
		coroutine_ = StartCoroutine(play());
	}

	public void OnPressedStop()
	{
		if (coroutine_ != null) {
			StopCoroutine(coroutine_);
			coroutine_ = null;
		}
		sound_player_.stopPlay();
		button_start_.interactable = true;
		button_stop_.interactable = false;
	}

}
