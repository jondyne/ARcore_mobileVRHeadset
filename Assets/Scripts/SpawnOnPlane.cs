using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SpawnOnPlane : MonoBehaviour, IPointerClickHandler {
    #region Editor public fields

    [SerializeField]
    Camera mainCamera;

    [SerializeField]
    GameObject crosshairPrefab;

    [SerializeField]
    Color crosshairInsidePlaneColor = Color.green;

    [SerializeField]
    Color crosshairOutsidePlaneColor = Color.yellow;

    [SerializeField]
    GameObject prefabToSpawn;

    [SerializeField]
    ARRaycastManager raycastManager;

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
    Renderer crosshairRenderer;

    readonly List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    int shaderColorId;

    #endregion

    #region Unity methods

    void Start() {
        screenCenter = new Vector3(mainCamera.pixelWidth / 2, mainCamera.pixelHeight / 2, 0f);

        crosshair = Instantiate(crosshairPrefab);
        crosshairRenderer = crosshair.GetComponentInChildren<Renderer>();
        crosshair.SetActive(false);

        shaderColorId = Shader.PropertyToID("_Color");
    }

    void Update() {
        var ray = mainCamera.ScreenPointToRay(screenCenter);

        // Raycasting with only PlaneWithinInfinity always sets hitType to PlaneWithinInfinity, so
        // we need to potentially do two raycasts in order to color the crosshair properly.
        if (raycastManager.Raycast(ray, raycastHits, TrackableType.PlaneWithinBounds) ||
            raycastManager.Raycast(ray, raycastHits, TrackableType.PlaneWithinInfinity)) {
            var hit = raycastHits[0];
            var pose = hit.pose;

            crosshair.SetActive(true);
            crosshair.transform.position = pose.position;
            crosshair.transform.up = pose.up;

            if (crosshairRenderer != null) {
                var color = hit.hitType == TrackableType.PlaneWithinBounds
                    ? crosshairInsidePlaneColor
                    : crosshairOutsidePlaneColor;
                crosshairRenderer.material.SetColor(shaderColorId, color);
            }
        } else {
            crosshair.SetActive(false);
        }
    }

    #endregion

    #region Interaction

    public void OnPointerClick(PointerEventData eventData) {
        var ray = mainCamera.ScreenPointToRay(screenCenter);
        if (raycastManager.Raycast(ray, raycastHits, TrackableType.PlaneWithinBounds
            | TrackableType.PlaneWithinInfinity)) {
            
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
