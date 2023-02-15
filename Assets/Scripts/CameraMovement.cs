using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

  [SerializeField]
  float minZoom = 1f;
  [SerializeField]
  float maxZoom = 10f;
  [SerializeField]
  float sensitivity = 2f;
  Vector3 cameraPosition;
  Vector3 mousePositionOnScreen;
  Vector3 mousePositionOnScreen1;
  Vector3 camPos1;
  Vector3 mouseOnWorld;
  Vector3 camDragBegin;
  Vector3 camDragNext;
  Camera cam;

  void Start()
  {
    cam = GetComponent<Camera>();
    cameraPosition = cam.transform.position;
    mousePositionOnScreen = new Vector3();
    mousePositionOnScreen1 = new Vector3();
    camPos1 = new Vector3();
    mouseOnWorld = new Vector3();
  }

  void Update()
  {
    if (Input.GetAxis("Mouse ScrollWheel") != 0)
    {
      mousePositionOnScreen = mousePositionOnScreen1;
      mousePositionOnScreen1 = Input.mousePosition;
      if (Vector3.Distance(mousePositionOnScreen, mousePositionOnScreen1) == 0)
      {
        float fov = cam.orthographicSize;
        fov -= Input.GetAxis("Mouse ScrollWheel") * sensitivity;
        fov = Mathf.Clamp(fov, minZoom, maxZoom);
        cam.orthographicSize = fov;
        Vector3 mouseOnWorld1 = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 posDiff = mouseOnWorld - mouseOnWorld1;
        Vector3 camPos = cam.transform.position;
        cam.transform.position = new Vector3(camPos.x + posDiff.x, fov - 0.5f, camPos.z);
      }
      else
      {
        mouseOnWorld = cam.ScreenToWorldPoint(Input.mousePosition);
      }
    }

    if (Input.GetMouseButtonDown(1))
    {
      camDragBegin = Input.mousePosition;
      camPos1 = cam.transform.position;
    }

    if (Input.GetMouseButton(1))
    {
      if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
      {

        camDragNext = Input.mousePosition;
        Vector3 screenDelta = camDragBegin - camDragNext;
        Vector2 screenSize = ScaleScreenToWorldSize(cam.aspect, cam.orthographicSize, cam.scaledPixelWidth, cam.scaledPixelHeight, screenDelta.x, screenDelta.y);

        Vector3 camPosMove = new Vector3(camPos1.x + screenSize.x, camPos1.y, camPos1.z);
        cam.transform.position = camPosMove;
      }
    }
  }

  Vector2 ScaleScreenToWorldSize(float camAspect, float camSize, float camScreenPixelWidth, float camScreenPixelHeight, float screenW, float screenH)
  {
    float cameraWidth = camAspect * camSize * 2f;
    float cameraHeght = camSize * 2f;
    float screenWorldW = screenW * (cameraWidth / camScreenPixelWidth);
    float screenWorldH = screenH * (cameraHeght / camScreenPixelHeight);

    return new Vector2(screenWorldW, screenWorldH);
  }
}