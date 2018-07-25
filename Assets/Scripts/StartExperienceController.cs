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
        ARSubsystemManager.systemStateChanged += ArSubsystemStateChanged;
    }

    void OnDisable() {
        ARSubsystemManager.systemStateChanged -= ArSubsystemStateChanged;
    }

    #endregion

    #region AR handling

    void ArSubsystemStateChanged(ARSystemStateChangedEventArgs eventArgs) {
        Debug.Log($"AR system state changed to {eventArgs.state}");

        if (!initialized && eventArgs.state == ARSystemState.SessionTracking) {
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
