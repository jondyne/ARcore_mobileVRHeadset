using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;

public class StartExperienceController : MonoBehaviour, IPointerClickHandler {
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

        if (eventArgs.state == ARSystemState.SessionTracking) {
            initializeUi.SetActive(false);
            tapToSpawnUi.SetActive(true);

            StartCoroutine(HideSpawnUI());
        }
    }

    #endregion

    #region UI handling

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("Click.");
    }

    // ReSharper disable once InconsistentNaming
    IEnumerator HideSpawnUI() {
        yield return new WaitForSeconds(hideAfterSeconds);

        tapToSpawnUi.SetActive(false);
    }

    #endregion
}
