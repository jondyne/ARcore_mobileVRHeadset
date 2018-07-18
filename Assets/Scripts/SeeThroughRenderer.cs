using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

// Note: Heavily modified version of Unity's ARBackgroundRenderer.

public class SeeThroughRenderer {
    #region Public properties

    public event Action backgroundRendererChanged = null;

    /// <summary>
    ///   <para>The Material used for AR rendering.</para>
    /// </summary>
    public Material backgroundMaterial {
        get { return this.m_BackgroundMaterial; }
        set {
            if ((UnityEngine.Object) this.m_BackgroundMaterial == (UnityEngine.Object) value)
                return;
            this.RemoveCommandBuffersIfNeeded();
            this.m_BackgroundMaterial = value;
            if (this.backgroundRendererChanged != null)
                this.backgroundRendererChanged();
            this.ReapplyCommandBuffersIfNeeded();
        }
    }

    /// <summary>
    ///   <para>An optional Texture used for AR rendering. If this property is not set then the texture set in XR.ARBackgroundRenderer._backgroundMaterial as "_MainTex" is used.</para>
    /// </summary>
    public Texture backgroundTexture {
        get { return this.m_BackgroundTexture; }
        set {
            if ((bool) ((UnityEngine.Object) (this.m_BackgroundTexture = value)))
                return;
            this.RemoveCommandBuffersIfNeeded();
            this.m_BackgroundTexture = value;
            if (this.backgroundRendererChanged != null)
                this.backgroundRendererChanged();
            this.ReapplyCommandBuffersIfNeeded();
        }
    }

    /// <summary>
    ///   <para>An optional Camera whose background rendering will be overridden by this class. If this property is not set then the main Camera in the scene is used.</para>
    /// </summary>
    public Camera camera {
        get {
            return !((UnityEngine.Object) this.m_Camera != (UnityEngine.Object) null)
                ? Camera.main
                : this.m_Camera;
        }
        set {
            if ((UnityEngine.Object) this.m_Camera == (UnityEngine.Object) value)
                return;
            this.RemoveCommandBuffersIfNeeded();
            this.m_Camera = value;
            if (this.backgroundRendererChanged != null)
                this.backgroundRendererChanged();
            this.ReapplyCommandBuffersIfNeeded();
        }
    }

    /// <summary>
    ///   <para>When set to XR.ARRenderMode.StandardBackground (default) the camera is not overridden to display the background image. Setting this property to XR.ARRenderMode.MaterialAsBackground will render the texture specified by XR.ARBackgroundRenderer._backgroundMaterial and or XR.ARBackgroundRenderer._backgroundTexture as the background.</para>
    /// </summary>
    public ARRenderMode mode {
        get { return this.m_RenderMode; }
        set {
            if (value == this.m_RenderMode)
                return;
            this.m_RenderMode = value;
            switch (this.m_RenderMode) {
                case ARRenderMode.StandardBackground:
                    this.DisableARBackgroundRendering();
                    break;
                case ARRenderMode.MaterialAsBackground:
                    this.EnableARBackgroundRendering();
                    break;
                default:
                    throw new Exception("Unhandled render mode.");
            }

            if (this.backgroundRendererChanged == null)
                return;
            this.backgroundRendererChanged();
        }
    }

    #endregion

    #region Private fields

    Camera m_Camera = (Camera) null;
    Material m_BackgroundMaterial = (Material) null;
    Texture m_BackgroundTexture = (Texture) null;
    ARRenderMode m_RenderMode = ARRenderMode.StandardBackground;
    CommandBuffer m_CommandBuffer = (CommandBuffer) null;
    CameraClearFlags m_CameraClearFlags = CameraClearFlags.Skybox;

    #endregion

    #region Rendering

    protected bool EnableARBackgroundRendering() {
        if ((UnityEngine.Object) this.m_BackgroundMaterial == (UnityEngine.Object) null)
            return false;
        Camera camera = !((UnityEngine.Object) this.m_Camera != (UnityEngine.Object) null)
            ? Camera.main
            : this.m_Camera;
        if ((UnityEngine.Object) camera == (UnityEngine.Object) null)
            return false;
        this.m_CameraClearFlags = camera.clearFlags;
        camera.clearFlags = CameraClearFlags.Depth;
        this.m_CommandBuffer = new CommandBuffer();
        Texture source = this.m_BackgroundTexture;
        if ((UnityEngine.Object) source == (UnityEngine.Object) null &&
            this.m_BackgroundMaterial.HasProperty("_MainTex"))
            source = this.m_BackgroundMaterial.GetTexture("_MainTex");
        this.m_CommandBuffer.Blit(
            source, (RenderTargetIdentifier) BuiltinRenderTextureType.CameraTarget,
            this.m_BackgroundMaterial);
        camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, this.m_CommandBuffer);
        camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, this.m_CommandBuffer);
        return true;
    }

    /// <summary>
    ///   <para>Disables AR background rendering. This method is called internally but can be overridden by users who wish to subclass XR.ARBackgroundRenderer to customize handling of AR background rendering.</para>
    /// </summary>
    protected void DisableARBackgroundRendering() {
        if (this.m_CommandBuffer == null)
            return;
        Camera camera = this.m_Camera ?? Camera.main;
        if ((UnityEngine.Object) camera == (UnityEngine.Object) null)
            return;
        camera.clearFlags = this.m_CameraClearFlags;
        camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, this.m_CommandBuffer);
        camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, this.m_CommandBuffer);
    }

    private bool ReapplyCommandBuffersIfNeeded() {
        if (this.m_RenderMode != ARRenderMode.MaterialAsBackground)
            return false;
        this.EnableARBackgroundRendering();
        return true;
    }

    private bool RemoveCommandBuffersIfNeeded() {
        if (this.m_RenderMode != ARRenderMode.MaterialAsBackground)
            return false;
        this.DisableARBackgroundRendering();
        return true;
    }

    #endregion
}
