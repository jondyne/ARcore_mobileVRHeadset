using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARFoundation;

public class SpawnOnPlane : MonoBehaviour, IPointerClickHandler {
    #region Editor public fields

    [SerializeField]
    Camera mainCamera;

    [SerializeField]
    GameObject crosshairPrefab;

    [SerializeField]
    GameObject prefabToSpawn;

    [SerializeField]
    ARSessionOrigin sessionOrigin;

    [SerializeField]
    float scaleMinimum = 0.2f;

    [SerializeField]
    float scaleMaximum = 1f;

    #endregion

    #region Public properties

    #endregion

    #region Private fields

    Vector3 screenCenter;
    GameObject crosshair;
    readonly List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    #endregion

    #region Unity methods

    void Start() {
        screenCenter = new Vector3(mainCamera.pixelWidth / 2, mainCamera.pixelHeight / 2, 0f);

        crosshair = Instantiate(crosshairPrefab);
        crosshair.SetActive(false);
    }

    void Update() {
        var ray = mainCamera.ScreenPointToRay(screenCenter);
        if (sessionOrigin.Raycast(ray, raycastHits, TrackableType.PlaneWithinPolygon)) {
            var pose = raycastHits[0].pose;

            crosshair.SetActive(true);
            crosshair.transform.position = pose.position;
            crosshair.transform.up = pose.up;
        } else {
            crosshair.SetActive(false);
        }
    }

    #endregion

    #region Interaction

    public void OnPointerClick(PointerEventData eventData) {
        var ray = mainCamera.ScreenPointToRay(screenCenter);
        if (sessionOrigin.Raycast(ray, raycastHits, TrackableType.PlaneWithinPolygon)) {
            var pose = raycastHits[0].pose;
            var spawnedObject = Instantiate(prefabToSpawn, pose.position, pose.rotation);

            var forward = -mainCamera.transform.forward;
            forward.y = 0f;
            spawnedObject.transform.forward = forward.normalized;

            var randomScale = Random.Range(scaleMinimum, scaleMaximum);
            spawnedObject.transform.localScale = Vector3.one * randomScale;
        }
    }

    #endregion
}
