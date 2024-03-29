using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class Flag : MonoBehaviour
{
  Vector3 closest = new Vector3(-100, 0, 0);
  float smallestDistance = float.PositiveInfinity;

  public float acceleration;
  public float velocity;
  public float PE;
  public float KE;
  public float TE;
  public float HE;
  public float railHeight;

  public static TrackManager track; 
  public static Text velocityText;
  public static Text accelerationText;
  public static Text PEText;
  public static Text KEText;
  public static Text heightText;
  public static Text TEText;
  public static Text HEText;
  public static GameObject flagValues = null;

  // Drag & pan helper vars
  Plane dragPlane;
  Vector3 offset;
  public Vector2 panelOffset = Vector2.zero;
  private Vector3 dragStart;
  Camera mainCam;

  Transform cart;
  Cart cartScript;

  public Color flagColor;
  static int colorIndex = 0;
  public void Reset()
  {

    closest = new Vector3(-100, 0, 0);
    smallestDistance = float.PositiveInfinity;
    acceleration = 0;
    velocity = 0;
    PE = 0;
    KE = 0;
    TE = 0;
    HE = 0;
  }
  // Start is called before the first frame update
  void Start()
  {

    flagColor = Color.HSVToRGB((++colorIndex % 12f) / 12f, .7f, .9f);
    GetComponent<SpriteRenderer>().color = flagColor;
    if (velocityText == null)
    {
      velocityText = GameObject.Find("Flag Velocity").GetComponent<Text>();
    }
    if (track == null){
        track = GameObject.FindGameObjectWithTag("Track Manager").GetComponent<TrackManager>();

    }
    // if (accelerationText == null)
    // {
    //   accelerationText = GameObject.Find("Flag Acceleration").GetComponent<Text>();
    // }
    if (KEText == null)
    {
      KEText = GameObject.Find("Flag KE").GetComponent<Text>();
    }
    if (heightText == null)
    {
      heightText = GameObject.Find("Flag Height").GetComponent<Text>();
    }
    if (PEText == null)
    {
      PEText = GameObject.Find("Flag PE").GetComponent<Text>();
    }
    if (TEText == null)
    {
      TEText = GameObject.Find("Flag TE").GetComponent<Text>();
    }
    if (HEText == null)
    {
      HEText = GameObject.Find("Flag HE").GetComponent<Text>();
    }

    cart = GameObject.Find("Cart").transform;

    cartScript = cart.GetComponent<Cart>();
    mainCam = Camera.main;
    if (flagValues == null)
    {
      flagValues = GameObject.Find("Flag Values");
      flagValues.SetActive(false);
    }
    // hitbox = GetComponent<BoxCollider2D>();
  }

  public void Create()
  {
    if (cartScript.paused)
      flagValues.SetActive(false);

  }

  public void Delete()
  {

    flagValues.SetActive(false);
    TrackManager.selectedFlag = null;
    // velocityText.text = "Velocity: ";
    // accelerationText.text = "Acceleration: ";
    // KEText.text = "Kinetic Energy: ";
    // HEText.text = "Thermal Energy: ";
    // PEText.text = "Potential Energy: ";
    // TEText.text = "Total Energy: ";
    Destroy(gameObject);
  }

  // void OnDrawGizmos()
  // {
  //   Gizmos.DrawSphere(track.GetClosestPoints(transform.position)[1], 0.1f);
  // }
  float round(float v)
  {
    return Mathf.Max(0.00f, Mathf.Round(v * 100f) / 100f);
  }
  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
    {
      if (TrackManager.selectedFlag == gameObject)
      {
        Delete();
      }

    }
    // Debug.Log(smallestDistance + " "  + Vector3.SqrMagnitude(transform.position - cart.position));
    // Debug.Log(smallestDistance > Vector3.SqrMagnitude(transform.position - cart.position));
    float dist = (transform.position + 3 * Vector3.forward - cart.position).sqrMagnitude;
    if (!cartScript.paused && dist < smallestDistance)
    {

      if (Mathf.Sqrt(dist) < .2f)
      {
        closest = cart.position;
        PE = round(cartScript.mass * 9.81f * round(railHeight));
        acceleration = cartScript.d_acc.magnitude;
        TE = round(cartScript.mass * 9.81f * round(cartScript.releaseHeight));
        if (transform.position.x >= cartScript.track.trackPoints[track.trackPoints.Count - 31].x) {
          velocity = cartScript.d_vel.magnitude;
          KE = cartScript.d_KE;
          HE = cartScript.d_HE;
          PE = cartScript.d_PE;
          if (transform.position.x >= cartScript.track.trackPoints[track.trackPoints.Count - 1].x) {
            velocity = 0;
            KE = 0;
            HE = TE - PE;
          }
        } else {
          KE = TE - PE;
          HE = 0;
          velocity = Mathf.Sqrt(2 * KE / cartScript.mass);
        }
        
      }
      smallestDistance = dist;
    }

    if (TrackManager.selectedFlag == gameObject)
    {
      velocityText.text = "Velocity: " + velocity.ToString("F2", CultureInfo.InvariantCulture) + " m/s";
      // accelerationText.text = "Acceleration: " + (acceleration).ToString("F2", CultureInfo.InvariantCulture) + " m/s^2";

      KEText.text = "Kinetic Energy: " + KE.ToString("F2", CultureInfo.InvariantCulture) + " J";
      PEText.text = "Potential Energy: " + PE.ToString("F2", CultureInfo.InvariantCulture) + " J";
      HEText.text = "Thermal Energy: " + HE.ToString("F2", CultureInfo.InvariantCulture) + " J";
      TEText.text = "Total Energy: " + TE.ToString("F2", CultureInfo.InvariantCulture) + " J";
    }
  }

  void OnDrawGizmos(){
    // Gizmos.DrawSphere(cartScript.track.trackPoints[track.trackPoints.Count - 31], 0.1f);
  }
  void OnMouseDown()
  {
    railHeight = round(track.GetClosestPoints(transform.position)[1].y);
    heightText.text = "Rail Height: " + railHeight.ToString("F2", CultureInfo.InvariantCulture) + " m";

    dragPlane = new Plane(mainCam.transform.forward, transform.position);
    Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);

    float planeDist;
    dragPlane.Raycast(camRay, out planeDist);
    offset = transform.position - camRay.GetPoint(planeDist);
    dragStart = camRay.GetPoint(planeDist);
    if (cartScript.paused)
    {
      flagValues.SetActive(true);
      flagValues.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.425f) - new Vector3(panelOffset.x, panelOffset.y);
      flagValues.GetComponent<Draggable>().flag = this;

      TrackManager.selectedFlag = gameObject;
    }
    else
    {
      TrackManager.Error("Pause to select flag");
    }
  }

  void OnMouseDrag()
  {
    
    railHeight = round(track.GetClosestPoints(transform.position)[1].y);
    heightText.text = "Rail Height: " + railHeight.ToString("F2", CultureInfo.InvariantCulture) + " m";
    flagValues.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.425f) - new Vector3(panelOffset.x, panelOffset.y);

    Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);

    float planeDist;
    dragPlane.Raycast(camRay, out planeDist);
    if (cartScript.paused && (cartScript.atStart || TrackManager.errCount > 0))
    {
      transform.position = new Vector3(camRay.GetPoint(planeDist).x + offset.x, camRay.GetPoint(planeDist).y + offset.y, camRay.GetPoint(planeDist).z + offset.z);
    }
    else
    {
      if (Vector2.SqrMagnitude(dragStart - camRay.GetPoint(planeDist)) > .25f)
        TrackManager.Error("Restart to move flag");
    }
  }
}
