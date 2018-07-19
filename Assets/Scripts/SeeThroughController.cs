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
    Material arMaterial;

    [SerializeField]
    float fov = 1.6f;

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
    RenderTexture arTexture;

    #endregion

    #region Unity methods

    void Start() {
        seeThroughCamera = GetComponent<Camera>();

        backgroundMaterial.SetFloat("_FOV", fov);
        backgroundMaterial.SetFloat("_Disparity", disparity);

//        var alpha = (webCamTexture.height / (float) Screen.height) *
//                    ((Screen.width * 0.5f) / webCamTexture.width);
        // TODO: Understand _Alpha calculation ↑ better, 0.5 is needed but on S7 it's 0.66
        backgroundMaterial.SetFloat("_Alpha", 0.5f);

        arTexture = new RenderTexture(Screen.width, Screen.height, 32);
        renderingCamera.targetTexture = arTexture;

        seeThroughRenderer =
            new SeeThroughRenderer(seeThroughCamera, arTexture, backgroundMaterial, arMaterial);

        var cameraSubsystem = ARSubsystemManager.cameraSubsystem;
        if (cameraSubsystem != null) {
            cameraSubsystem.Camera = seeThroughCamera;
            cameraSubsystem.Material = BackgroundMaterial;
        }

        ARSubsystemManager.cameraFrameReceived += OnCameraFrameReceived;
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
