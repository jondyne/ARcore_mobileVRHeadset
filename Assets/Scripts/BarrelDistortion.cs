using UnityEngine;

public class BarrelDistortion : MonoBehaviour {
    #region Editor public fields

    [SerializeField]
    Material distortionMaterial;

    #endregion

    #region Public properties

    #endregion

    #region Private fields

    #endregion

    #region Unity methods

    void Start() {
        distortionMaterial.SetFloat("_FOV", 1.6f);
        distortionMaterial.SetFloat("_Disparity", 0f);
        distortionMaterial.SetFloat("_Alpha", 0.5f);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(src, null, distortionMaterial);
    }

    #endregion
}
