using UnityEngine;

public class BarrelDistortion : MonoBehaviour {
    #region Editor public fields

    [SerializeField]
    Material distortionMaterial;

    [SerializeField]
    [Range(0.5f, 1.5f)]
    float fov = 1f;

    #endregion

    #region Public properties

    #endregion

    #region Private fields

    #endregion

    #region Unity methods

    void Start() {
//        var alpha = (webCamTexture.height / (float) Screen.height) *
//                    ((Screen.width * 0.5f) / webCamTexture.width);
        // TODO: Understand _Alpha calculation ↑ better, 0.5 is needed but on S7 it's 0.66
        distortionMaterial.SetFloat("_Alpha", 0.5f);

        distortionMaterial.SetFloat("_FOV", fov);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(src, null, distortionMaterial);
    }

    void OnGUI() {
        const int boundary = 20;
        const int labelHeight = 60;
        const int sliderWidth = 200;
        const int buttonSize = 80;
        const float fovIncrement = 0.001f;

        GUI.skin.label.alignment = TextAnchor.MiddleRight;

        GUI.Label(new Rect(Screen.width - boundary - 300, boundary, 300, labelHeight),
                  $"FOV: {fov:F3}");

        if (GUI.Button(
            new Rect(Screen.width - boundary - 2 * buttonSize - sliderWidth, boundary + labelHeight,
                     buttonSize, buttonSize), "-")) {
            fov -= fovIncrement;
        }

        fov = GUI.HorizontalSlider(
            new Rect(Screen.width - boundary - buttonSize - sliderWidth, boundary + labelHeight,
                     sliderWidth, labelHeight), fov, 0.5f, 1.5f);
        distortionMaterial.SetFloat("_FOV", fov);

        if (GUI.Button(
            new Rect(Screen.width - boundary - buttonSize, boundary + labelHeight, buttonSize,
                     buttonSize), "+")) {
            fov += fovIncrement;
        }
    }

    #endregion
}
