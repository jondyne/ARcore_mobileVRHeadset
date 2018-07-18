using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public sealed class SeeThroughController : MonoBehaviour {
    #region Editor public fields

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

            Renderer.Mode = ARRenderMode.MaterialAsBackground;
            Renderer.BackgroundMaterial = material;

            if (ARSubsystemManager.cameraSubsystem != null) {
                ARSubsystemManager.cameraSubsystem.Material = material;
            }
        }
    }

    public SeeThroughRenderer Renderer { get; private set; }

    #endregion

    #region Private fields

    bool cameraSetupThrewException;
    Camera currentCamera;

    #endregion

    #region Unity methods

    void Start() {
        currentCamera = GetComponent<Camera>();

        Material.SetFloat("_FOV", fov);
        Material.SetFloat("_Disparity", disparity);

//        var alpha = (webCamTexture.height / (float) Screen.height) *
//                    ((Screen.width * 0.5f) / webCamTexture.width);
        // TODO: Understand _Alpha calculation ↑ better, 0.5 is needed but on S7 it's 0.66
        Material.SetFloat("_Alpha", 0.5f);

        StartRenderer();
    }

    #endregion

    #region Camera handling

    void StartRenderer() {
        Debug.Log("Starting ARBackgroundRenderer");

        Renderer = new SeeThroughRenderer {
            Mode = ARRenderMode.MaterialAsBackground,
            Camera = currentCamera,
            BackgroundMaterial = Material
        };

        var cameraSubsystem = ARSubsystemManager.cameraSubsystem;
        if (cameraSubsystem != null) {
            cameraSubsystem.Camera = currentCamera;
            cameraSubsystem.Material = Material;
        }

        ARSubsystemManager.cameraFrameReceived += OnCameraFrameReceived;
    }

    void SetupCameraIfNecessary() {
        Renderer.Mode = ARRenderMode.MaterialAsBackground;

        if (Renderer.BackgroundMaterial != Material) {
            Material = Material;
        }
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs) {
        SetupCameraIfNecessary();
    }

    #endregion
}
