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
    Camera currentCamera;

    #endregion

    #region Unity methods

    void Awake() {
        currentCamera = GetComponent<Camera>();
        BackgroundRenderer = new ARBackgroundRenderer();
        if (!overrideMaterial)
            material = null;
    }

    void OnEnable() {
        BackgroundRenderer.mode = ARRenderMode.MaterialAsBackground;
        BackgroundRenderer.camera = currentCamera;

        NotifyCameraSubsystem();
        ARSubsystemManager.cameraFrameReceived += OnCameraFrameReceived;
        ARSubsystemManager.systemStateChanged += OnSystemStateChanged;
    }

    void OnDisable() {
        BackgroundRenderer.mode = ARRenderMode.StandardBackground;
        ARSubsystemManager.cameraFrameReceived -= OnCameraFrameReceived;
        ARSubsystemManager.systemStateChanged -= OnSystemStateChanged;
        cameraHasBeenSetup = false;
        cameraSetupThrewException = false;

        // Tell the camera subsystem to stop doing work
        var cameraSubsystem = ARSubsystemManager.cameraSubsystem;
        if (cameraSubsystem != null) {
            if (cameraSubsystem.Camera == currentCamera)
                cameraSubsystem.Camera = null;

            if (cameraSubsystem.Material == Material)
                cameraSubsystem.Material = null;
        }
    }

    #endregion

    #region Camera handling

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
        if (eventArgs.state < ARSystemState.SessionInitializing && BackgroundRenderer != null) {
            BackgroundRenderer.mode = ARRenderMode.StandardBackground;
        }
    }

    #endregion
}
