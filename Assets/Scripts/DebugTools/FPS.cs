using UnityEngine;

public class FPS : MonoBehaviour
{
	private const float CounterHeightScreenProportion = 0.02f;
	private const float MillisecondsPerSecond = 1000.0f;
	private float deltaTime;

	private void Update() => 
		this.deltaTime += (Time.unscaledDeltaTime - this.deltaTime) * 0.1f;

	private void OnGUI()
	{
		var width = Screen.width;
		var height = Screen.height;

		GUIStyle style = new GUIStyle();

		var rect = new Rect(0, 0, width, height * CounterHeightScreenProportion);
		
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = (int) (height * CounterHeightScreenProportion);
		style.normal.textColor = new Color(0, 0, 0, 1f);
		
		var milliseconds = deltaTime * MillisecondsPerSecond;
		var fps = 1.0f / deltaTime;
		var fpsTextReading = string.Format("{0:0.0} ms ({1:0.} fps)", milliseconds, fps);
		
		GUI.Label(rect, fpsTextReading, style);
	}
}