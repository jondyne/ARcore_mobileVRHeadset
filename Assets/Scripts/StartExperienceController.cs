using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class StartExperienceController : MonoBehaviour{
    #region Editor public fields

    [SerializeField]
    GameObject initializeUi;

    [SerializeField]
    GameObject tapToSpawnUi;

    [SerializeField]
    float hideAfterSeconds = 3f;

    #endregion

    #region Public properties

    #endregion

    #region Private fields

    bool initialized;

    #endregion

    #region Unity methods

    void Start() {
        initializeUi.SetActive(true);
        tapToSpawnUi.SetActive(false);
    }

    void OnEnable() {
        ARSession.stateChanged += ArSessionStateChanged;
    }

    void OnDisable() {
        ARSession.stateChanged -= ArSessionStateChanged;
    }

    #endregion

    #region AR handling

    void ArSessionStateChanged(ARSessionStateChangedEventArgs eventArgs) {
        Debug.Log($"AR session state changed to {eventArgs.state}");

        if (!initialized && eventArgs.state == ARSessionState.SessionTracking) {
            initializeUi.SetActive(false);
            tapToSpawnUi.SetActive(true);

            initialized = true;

            StartCoroutine(HideTapToSpawnUI());
        }
    }

    #endregion

    #region UI handling

    // ReSharper disable once InconsistentNaming
    IEnumerator HideTapToSpawnUI() {
        yield return new WaitForSeconds(hideAfterSeconds);

        tapToSpawnUi.SetActive(false);
    }

    #endregion
}
