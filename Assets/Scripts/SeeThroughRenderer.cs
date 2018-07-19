using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

// Note: Heavily modified version of Unity's ARBackgroundRenderer.

public class SeeThroughRenderer {
    #region Public properties

    public Material BackgroundMaterial {
        get { return backgroundMaterial; }
        set {
            if (backgroundMaterial == value) {
                return;
            }

            RemoveCommandBuffersIfNeeded();
            backgroundMaterial = value;
            ReapplyCommandBuffersIfNeeded();
        }
    }

    public Texture BackgroundTexture {
        get { return backgroundTexture; }
        set {
            if (backgroundTexture == value) {
                return;
            }

            RemoveCommandBuffersIfNeeded();
            backgroundTexture = value;
            ReapplyCommandBuffersIfNeeded();
        }
    }

    public Camera Camera {
        get { return seeThroughCamera != null ? seeThroughCamera : Camera.main; }
        set {
            if (seeThroughCamera == value) {
                return;
            }

            RemoveCommandBuffersIfNeeded();
            seeThroughCamera = value;
            ReapplyCommandBuffersIfNeeded();
        }
    }

    public ARRenderMode Mode {
        get { return renderMode; }
        set {
            if (value == renderMode) {
                return;
            }

            renderMode = value;
            switch (renderMode) {
                case ARRenderMode.StandardBackground:
                    DisableBackgroundRendering();
                    break;
                case ARRenderMode.MaterialAsBackground:
                    EnableBackgroundRendering();
                    break;
                default:
                    throw new Exception("Unhandled render mode.");
            }
        }
    }

    #endregion

    #region Private fields

    Camera seeThroughCamera;
    Texture backgroundTexture;
    Material backgroundMaterial;
    Texture arTexture;
    Material arMaterial;
    ARRenderMode renderMode = ARRenderMode.StandardBackground;
    CommandBuffer commandBuffer;
    CameraClearFlags savedCameraClearFlags = CameraClearFlags.Skybox;

    #endregion

    #region Initialization

    public SeeThroughRenderer(Camera seeThroughCamera, RenderTexture arTexture,
                              Material backgroundMaterial, Material arMaterial) {
        this.seeThroughCamera = seeThroughCamera;
        this.arTexture = arTexture;
        this.backgroundMaterial = backgroundMaterial;
        renderMode = ARRenderMode.MaterialAsBackground;

        EnableBackgroundRendering();
    }

    #endregion

    #region Rendering

    void EnableBackgroundRendering() {
        if (backgroundMaterial == null) {
            return;
        }

        savedCameraClearFlags = Camera.clearFlags;
        Camera.clearFlags = CameraClearFlags.Depth;

        commandBuffer = new CommandBuffer();

        var source = backgroundTexture;
        if (source == null && backgroundMaterial.HasProperty("_MainTex")) {
            source = backgroundMaterial.GetTexture("_MainTex");
        }

        commandBuffer.Blit(source, BuiltinRenderTextureType.CameraTarget, backgroundMaterial);
        Camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
        Camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, commandBuffer);
    }

    void DisableBackgroundRendering() {
        if (commandBuffer == null) {
            return;
        }

        Camera.clearFlags = savedCameraClearFlags;
        Camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
        Camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, commandBuffer);
    }

    void ReapplyCommandBuffersIfNeeded() {
        if (renderMode != ARRenderMode.MaterialAsBackground) {
            return;
        }

        EnableBackgroundRendering();
    }

    void RemoveCommandBuffersIfNeeded() {
        if (renderMode != ARRenderMode.MaterialAsBackground) {
            return;
        }

        DisableBackgroundRendering();
    }

    #endregion
}
