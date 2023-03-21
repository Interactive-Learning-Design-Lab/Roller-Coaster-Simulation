using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

// Types of tracks
public enum TrackType
{
  up,       // upward slope
  down,     // downward slope
  hill,
  circular, // circular loop
  clothoid, // clothoid loop
  fillUp,    // filler track going up
  fillDown    // filler track going down
}

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(LineRenderer))]
// [ExecuteInEditMode]
public class Track : MonoBehaviour
{
  // Track points per world unit
  [SerializeField] const float resolution = 100f;

  [SerializeField]
  LineRenderer lineRenderer;
  BoxCollider2D hitbox;
  static Cart cart;

  // Points of current track
  [SerializeField] List<Vector3> points;

  [SerializeField]
  float width;
  [SerializeField]
  float height;
  [SerializeField]
  TrackType type;

  public static Slider heightSlider;
  public static Text heightText;
  public static Slider widthSlider;
  public static Text widthText;
  public static GameObject editMenu;

  // Drag & pan helper vars
  Plane dragPlane;
  Vector3 offset;
  Camera mainCam;
  public Material selected;
  public Material normal;
  public static GameObject editPanel = null;

  public void setProperties(float width, float height, TrackType _type)
  {
    this.width = width;
    this.height = height;
    this.type = _type;
  }

  public void Delete()
  {
    editPanel.SetActive(false);
    TrackManager.selected = null;

    points = new List<Vector3>();
    lineRenderer.positionCount = 0;
    gameObject.tag = "Untagged";
    TrackManager.UpdateTracks();
    TrackManager.trackCount--;
    Destroy(gameObject);
  }

  // Initialize fields
  [ContextMenu("Initialize")]
  void Initialize()
  {

    lineRenderer = GetComponent<LineRenderer>();
    lineRenderer.material = normal;
    points = new List<Vector3>();
    mainCam = Camera.main;
    hitbox = GetComponent<BoxCollider2D>();
    cart = GameObject.Find("Cart").GetComponent<Cart>();

    if (editPanel == null) {
      heightSlider = GameObject.Find("Height Input").GetComponent<Slider>();
      widthSlider = GameObject.Find("Width Input").GetComponent<Slider>();
      heightText = GameObject.Find("HText").GetComponent<Text>();
      widthText = GameObject.Find("WText").GetComponent<Text>();
      editMenu = GameObject.Find("Edit Menu");

      editPanel = GameObject.Find("Edit Menu");
      editPanel.SetActive(false);

    }

  }

  // Generate points of track based on type of track
  [ContextMenu("Generate Points")]
  void GeneratePoints()
  {
    points = new List<Vector3>();
    switch (type)
    {
      case TrackType.down:
        for (float x = -width / 2; x <= width / 2; x += 1f / resolution)
        {
          points.Add(transform.position + new Vector3(x, DownSlope(x)));
        }
        break;

      case TrackType.up:
        for (float x = -width / 2f; x <= width / 2f; x += 1f / resolution)
        {
          points.Add(transform.position + new Vector3(x, UpSlope(x)));
        }
        break;

      case TrackType.hill:
        for (float x = -width / 2f; x <= width / 2f; x += 1f / resolution)
        {
          points.Add(transform.position + new Vector3(x, Hill(x)));
        }
        break;

      case TrackType.clothoid:
        for (float t = -Mathf.PI / 2f; t <= 3 * Mathf.PI / 2f; t += 2f * Mathf.PI / (resolution * width))
        {
          points.Add(transform.position + Clothoid(t));
        }
        break;

      case TrackType.circular:
        for (float t = -Mathf.PI / 2f - 0.6f; t < 3f * Mathf.PI / 2f + 0.6f; t += (2f * Mathf.PI + 1) / (resolution * width))
        {
          points.Add(transform.position + Circular(t));
        }
        break;

      case TrackType.fillUp:
        for (float x = -width / 2f; x <= width / 2f; x += 1f / resolution)
        {
          points.Add(transform.position + new Vector3(x, x * height / width + height / 2));
        }
        break;

      case TrackType.fillDown:
        for (float x = -width / 2f; x <= width / 2f; x += 1f / resolution)
        {
          points.Add(transform.position + new Vector3(-x, x * height / width + height / 2));
        }
        break;
    }
  }

  [ContextMenu("Render")]
  void Render()
  {
    if (TrackManager.selected == this) {
      lineRenderer.material = selected;
      gameObject.layer = 6;
    } else if (TrackManager.errCount > 0) {
      lineRenderer.material = normal;
      gameObject.layer = 3;
    } else {
      gameObject.layer = 3;
      lineRenderer.materials = new Material[0];
    }
    lineRenderer.positionCount = 0;
    lineRenderer.positionCount = points.Count;

    lineRenderer.SetPositions(points.ToArray());

    if (hitbox)
    {
      hitbox.size = new Vector2(width + 0.1f, height + 0.2f);
      hitbox.offset = new Vector2(0, height / 2);
    }
  }

  float DownSlope(float x)
  {
    return height / 2 * (1 - (float)Math.Tanh(6f * x / width));
  }

  float UpSlope(float x)
  {
    return height / 2 * (1 - (float)Math.Tanh(-6f * x / width));
  }

  float Hill(float x)
  {
    return height / (Mathf.Sqrt(0.32f * Mathf.PI)) * Mathf.Exp(-4.9f * 4.9f * x * x / (width * width));
  }

  Vector3 Clothoid(float t)
  {
    float x = width * (5 * Mathf.Cos(t) / (3 * Mathf.PI) + t / (2 * Mathf.PI) - 1 / 4f);
    float y = height * (Mathf.Sin(t) / 2 + 1 / 2f);
    float z = t / 30;
    return new Vector3(x, y, z);
  }

  Vector3 Circular(float t)
  {
    if (t < -Mathf.PI / 2)
    {
      return new Vector3(width * (t + Mathf.PI / 2), 0, -t / 30);
    }
    else if (t > 3 * Mathf.PI / 2)
    {
      return new Vector3(width * (t - 3f / 2f * Mathf.PI), 0, -t / 30);
    }
    else
    {
      return new Vector3(width * Mathf.Cos(t) / 2, height / 2 * (Mathf.Sin(t) + 1), -t / 30);
    }
  }

  void Start()
  {
    Initialize();
    GeneratePoints();
    Render();

    // TrackManager.UpdateTracks();
  }

  public void Update()
  {
    GeneratePoints();
    Render();
  }

  void OnMouseDown()
  {
    if (cart.paused)
    {
      editPanel.SetActive(true);

      TrackManager.selected = this;
      TrackManager.height = height.ToString();
      TrackManager.width = width.ToString();
      TrackManager.type = (int)type;

      heightSlider.value = height;
      widthSlider.value = width;
      heightText.text = "Height: " + height.ToString("F2") + " m";
      widthText.text = "Width: " + width.ToString("F2") + " m";
      

      dragPlane = new Plane(mainCam.transform.forward, transform.position);
      Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);

      float planeDist;
      dragPlane.Raycast(camRay, out planeDist);
      offset = transform.position - camRay.GetPoint(planeDist);
    }
  }

  void OnMouseDrag()
  {
    if (cart.paused)
    {
      Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);

      float planeDist;
      dragPlane.Raycast(camRay, out planeDist);
      transform.position = new Vector3(camRay.GetPoint(planeDist).x + offset.x, 0);

      GeneratePoints();
      Render();
      TrackManager.UpdateTracks();
    }
  }



}
