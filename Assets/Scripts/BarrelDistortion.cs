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

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(src, null, distortionMaterial);
    }

    #endregion
}
