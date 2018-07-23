using System.Collections;
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

    [SerializeField]
    Material barrelDistortionMaterial;

#if UNITY_EDITOR
    [SerializeField]
    Material debugBackgroundMaterial;
#endif

    [SerializeField]
    [Range(1f, 2f)]
    float fov = 1f;

    [SerializeField]
    float disparity;

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
        barrelDistortionMaterial.SetFloat("_FOV", fov);
        barrelDistortionMaterial.SetFloat("_Disparity", disparity);

//        var alpha = (webCamTexture.height / (float) Screen.height) *
//                    ((Screen.width * 0.5f) / webCamTexture.width);
        // TODO: Understand _Alpha calculation ↑ better, 0.5 is needed but on S7 it's 0.66
        barrelDistortionMaterial.SetFloat("_Alpha", 0.5f);

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
        const int boundary = 20;
        GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 40;

        GUI.Label(new Rect(boundary, boundary, 400, labelHeight), "AR FOV");
        renderingCamera.fieldOfView = GUI.HorizontalSlider(
            new Rect(boundary, boundary + labelHeight, 200, labelHeight),
            renderingCamera.fieldOfView, 30f, 50f);

        GUI.Label(new Rect(Screen.width - boundary - 200, boundary, 200, labelHeight), "FOV");
        fov = GUI.HorizontalSlider(
            new Rect(Screen.width - boundary - 200, boundary + labelHeight, 200, labelHeight), fov,
            1.0f, 2.0f);
        barrelDistortionMaterial.SetFloat("_FOV", fov);

        GUI.Label(
            new Rect(Screen.width - boundary - 200, Screen.height - labelHeight * 2 - boundary, 200,
                     labelHeight), "Disparity");
        disparity =
            GUI.HorizontalSlider(
                new Rect(Screen.width - boundary - 200, Screen.height - labelHeight - boundary, 200,
                         labelHeight), disparity, 0.0f, 0.3f);
        barrelDistortionMaterial.SetFloat("_Disparity", disparity);
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
