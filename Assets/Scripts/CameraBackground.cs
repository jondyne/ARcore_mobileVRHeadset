using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

// NOTE: Modified version of ARFoundation's ARCameraBackground, fixing the material override bug.

public sealed class CameraBackground : MonoBehaviour {
    #region Editor public fields

    [SerializeField]
    bool overrideMaterial;

    [SerializeField]
    Material material;

    [SerializeField]
    float fov = 1.6f;

    [SerializeField]
    float disparity;

    #endregion

    #region Public properties

    public Material Material {
        get { return material; }
        set {
            material = value;

            BackgroundRenderer.mode = ARRenderMode.MaterialAsBackground;
            BackgroundRenderer.backgroundMaterial = material;

            if (ARSubsystemManager.cameraSubsystem != null) {
                ARSubsystemManager.cameraSubsystem.Material = material;
            }
        }
    }

    public ARBackgroundRenderer BackgroundRenderer { get; private set; }

    #endregion

    #region Private fields

    bool cameraHasBeenSetup;
    bool cameraSetupThrewException;
    bool shaderRatioConfigured;
    Camera currentCamera;

    WebCamTexture webCamTexture;

    #endregion

    #region Unity methods

    void Start() {
        currentCamera = GetComponent<Camera>();

        if (!overrideMaterial) {
            material = null;
        }

        foreach (var webCamDevice in WebCamTexture.devices) {
            if (!webCamDevice.isFrontFacing) {
                webCamTexture = new WebCamTexture(webCamDevice.name);
                webCamTexture.Play();
                break;
            }
        }

        if (webCamTexture == null) {
            Debug.LogWarning("No back-facing camera found, using first available.");
            webCamTexture = new WebCamTexture();
        }

        Material.SetFloat("_FOV", fov);
        Material.SetFloat("_Disparity", disparity);
    }

    void Update() {
        if (!shaderRatioConfigured && webCamTexture.width > 100) {
            // Alpha is the pixel density ratio of width over height, needed for displaying the
            // final image without skew
            Debug.Log("WebCamTexture has initialized, dimensions: " +
                      $"{webCamTexture.width}x{webCamTexture.height}");
            var alpha = webCamTexture.height / (float) Screen.height * Screen.width * 0.5f /
                        webCamTexture.width;
            Debug.Log($"Setting shader pixel density ratio to {alpha}");
            Material.SetFloat("_Alpha", alpha);

            shaderRatioConfigured = true;
            webCamTexture.Stop();
            webCamTexture = null;

            StartBackgroundRenderer();
        }
    }

    #endregion

    #region Camera handling

    void StartBackgroundRenderer() {
        Debug.Log("Starting ARBackgroundRenderer");

        BackgroundRenderer = new ARBackgroundRenderer {
            mode = ARRenderMode.StandardBackground,
            camera = currentCamera
        };

        NotifyCameraSubsystem();
        ARSubsystemManager.cameraFrameReceived += OnCameraFrameReceived;
        ARSubsystemManager.systemStateChanged += OnSystemStateChanged;
    }

    void SetupCameraIfNecessary() {
        if (cameraHasBeenSetup) {
            BackgroundRenderer.mode = ARRenderMode.MaterialAsBackground;
        }

        if (overrideMaterial) {
            if (BackgroundRenderer.backgroundMaterial != Material) {
                Material = Material;
            }

            return;
        }

        var cameraSubsystem = ARSubsystemManager.cameraSubsystem;
        if (cameraSetupThrewException || cameraHasBeenSetup || (cameraSubsystem == null)) {
            return;
        }

        // Try to create a material from the plugin's provided shader.
        var shaderName = "";
        if (!cameraSubsystem.TryGetShaderName(ref shaderName)) {
            return;
        }

        var shader = Shader.Find(shaderName);
        if (shader == null) {
            // If an exception is thrown, then something is irrecoverably wrong.
            // Set this flag so we don't try to do this every frame.
            cameraSetupThrewException = true;

            throw new InvalidOperationException(
                $"Could not find shader named \"{shaderName}\" required for video overlay on " +
                $"camera subsystem named \"{cameraSubsystem.SubsystemDescriptor.id}\".");
        }

        Material = new Material(shader);
        cameraHasBeenSetup = (Material != null);
        NotifyCameraSubsystem();
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs) {
        SetupCameraIfNecessary();
    }

    void NotifyCameraSubsystem() {
        var cameraSubsystem = ARSubsystemManager.cameraSubsystem;
        if (cameraSubsystem != null) {
            cameraSubsystem.Camera = currentCamera;
            cameraSubsystem.Material = Material;
        }
    }

    void OnSystemStateChanged(ARSystemStateChangedEventArgs eventArgs) {
//        if (eventArgs.state < ARSystemState.SessionInitializing && BackgroundRenderer != null) {
//            BackgroundRenderer.mode = ARRenderMode.StandardBackground;
//        }
    }

    #endregion
}
