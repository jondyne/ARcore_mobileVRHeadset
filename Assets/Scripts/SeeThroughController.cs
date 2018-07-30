using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public sealed class SeeThroughController : MonoBehaviour {
    #region Editor public fields

    [SerializeField]
    Camera seeThroughCamera;

    [SerializeField]
    Camera renderingCamera;

    [SerializeField]
    Material backgroundMaterial;

#if UNITY_EDITOR
    [SerializeField]
    Material debugBackgroundMaterial;
#endif

    #endregion

    #region Public properties

    public Material BackgroundMaterial {
        get { return backgroundMaterial; }
        set {
            backgroundMaterial = value;

            seeThroughRenderer.Mode = ARRenderMode.MaterialAsBackground;
            seeThroughRenderer.BackgroundMaterial = backgroundMaterial;

            if (ARSubsystemManager.cameraSubsystem != null) {
                ARSubsystemManager.cameraSubsystem.Material = backgroundMaterial;
            }
        }
    }

    #endregion

    #region Private fields

    SeeThroughRenderer seeThroughRenderer;

#if UNITY_EDITOR
    WebCamTexture webCamTexture;
#endif

    #endregion

    #region Unity methods

    void Start() {
#if UNITY_EDITOR
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();

        debugBackgroundMaterial.SetTexture("_MainTex", webCamTexture);

        seeThroughRenderer = new SeeThroughRenderer(seeThroughCamera, debugBackgroundMaterial);
#else
        seeThroughRenderer = new SeeThroughRenderer(seeThroughCamera, backgroundMaterial);
#endif

        var cameraSubsystem = ARSubsystemManager.cameraSubsystem;
        if (cameraSubsystem != null) {
            cameraSubsystem.Camera = seeThroughCamera;
            cameraSubsystem.Material = BackgroundMaterial;
        }

        ARSubsystemManager.cameraFrameReceived += OnCameraFrameReceived;
    }

    void OnGUI() {
        const int labelHeight = 60;
        const int sliderWidth = 200;
        const int buttonSize = 80;
        const int boundary = 20;
        const float arFovIncrement = 0.001f;

        GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 40;
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

        GUI.Label(new Rect(boundary, boundary, 400, labelHeight),
                  $"AR FOV: {renderingCamera.fieldOfView:F3}");

        if (GUI.Button(new Rect(boundary, boundary + labelHeight, buttonSize, buttonSize), "-")) {
            renderingCamera.fieldOfView -= arFovIncrement;
        }

        renderingCamera.fieldOfView = GUI.HorizontalSlider(
            new Rect(boundary + buttonSize, boundary + labelHeight, sliderWidth, labelHeight),
            renderingCamera.fieldOfView, 35f, 42f);

        if (GUI.Button(
            new Rect(boundary + buttonSize + sliderWidth, boundary + labelHeight, buttonSize,
                     buttonSize), "+")) {
            renderingCamera.fieldOfView += arFovIncrement;
        }
    }

    #endregion

    #region Camera handling

    void SetupCameraIfNecessary() {
        seeThroughRenderer.Mode = ARRenderMode.MaterialAsBackground;

        if (seeThroughRenderer.BackgroundMaterial != BackgroundMaterial) {
            BackgroundMaterial = BackgroundMaterial;
        }
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs) {
        SetupCameraIfNecessary();
    }

    #endregion
}
