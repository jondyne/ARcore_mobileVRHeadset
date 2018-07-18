using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public sealed class SeeThroughController : MonoBehaviour {
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

            Renderer.mode = ARRenderMode.MaterialAsBackground;
            Renderer.backgroundMaterial = material;

            if (ARSubsystemManager.cameraSubsystem != null) {
                ARSubsystemManager.cameraSubsystem.Material = material;
            }
        }
    }

    public SeeThroughRenderer Renderer { get; private set; }

    #endregion

    #region Private fields

    bool cameraHasBeenSetup;
    bool cameraSetupThrewException;
    Camera currentCamera;

//    bool shaderRatioConfigured;
//    WebCamTexture webCamTexture;

    #endregion

    #region Unity methods

    void Start() {
        currentCamera = GetComponent<Camera>();

        if (!overrideMaterial) {
            material = null;
        }

//        foreach (var webCamDevice in WebCamTexture.devices) {
//            if (!webCamDevice.isFrontFacing) {
//                webCamTexture = new WebCamTexture(webCamDevice.name);
//                webCamTexture.Play();
//                break;
//            }
//        }
//
//        if (webCamTexture == null) {
//            Debug.LogWarning("No back-facing camera found, using first available.");
//            webCamTexture = new WebCamTexture();
//        }

        Material.SetFloat("_FOV", fov);
        Material.SetFloat("_Disparity", disparity);

        // TODO: Understand _Alpha calculation better, 0.5 is needed but on S7 it's 0.66
        Material.SetFloat("_Alpha", 0.5f);

        StartRenderer();
    }

//    void Update() {
//        if (!shaderRatioConfigured && webCamTexture.width > 100) {
//            // Alpha is the pixel density ratio of width over height, needed for displaying the
//            // final image without skew
//            Debug.Log("WebCamTexture has initialized, dimensions: " +
//                      $"{webCamTexture.width}x{webCamTexture.height}");
//            var alpha = (webCamTexture.height / (float) Screen.height) *
//                        ((Screen.width * 0.5f) / webCamTexture.width);
//            Debug.Log($"Setting shader pixel density ratio to {alpha}, screen: {Screen.width}x{Screen.height}");
//            Material.SetFloat("_Alpha", alpha);
//
//            shaderRatioConfigured = true;
//            webCamTexture.Stop();
//            webCamTexture = null;
//
//            StartRenderer();
//        }
//    }

    #endregion

    #region Camera handling

    void StartRenderer() {
        Debug.Log("Starting ARBackgroundRenderer");

        Renderer = new SeeThroughRenderer {
            mode = ARRenderMode.MaterialAsBackground,
            camera = currentCamera
        };

        NotifyCameraSubsystem();
        ARSubsystemManager.cameraFrameReceived += OnCameraFrameReceived;
        ARSubsystemManager.systemStateChanged += OnSystemStateChanged;
    }

    void SetupCameraIfNecessary() {
        if (cameraHasBeenSetup) {
            Renderer.mode = ARRenderMode.MaterialAsBackground;
        }

        if (overrideMaterial) {
            if (Renderer.backgroundMaterial != Material) {
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
