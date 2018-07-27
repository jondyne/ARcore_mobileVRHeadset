using UnityEngine;

// Note: adapted from http://wiki.unity3d.com/index.php/FramesPerSecond

public class FramesPerSecond : MonoBehaviour {
    #region Editor public fields

    [SerializeField]
    float updateInterval = 0.5f;

    #endregion

    #region Public properties

    #endregion

    #region Private fields

    float accumulator;
    int frames = 0;
    float timeRemaining;
    float displayFramesPerSecond;

    #endregion

    #region Unity methods

    void Start() {
        timeRemaining = updateInterval;
    }

    void OnGUI() {
        const float boundary = 20f;
        const float labelHeight = 60f;
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(boundary, Screen.height - labelHeight - boundary, 400, labelHeight),
                  $"FPS: {displayFramesPerSecond:F2}");
    }

    void Update() {
        timeRemaining -= Time.deltaTime;
        accumulator += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeRemaining <= 0.0) {
            // Interval ended - update presentable FPS value and start new interval
            displayFramesPerSecond = accumulator / frames;

            timeRemaining = updateInterval;
            accumulator = 0f;
            frames = 0;
        }
    }

    #endregion
}
