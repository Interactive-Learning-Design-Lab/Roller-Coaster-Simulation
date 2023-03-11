using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flag : MonoBehaviour
{
  Vector3 closest = new Vector3(-100, 0, 0);
  float smallestDistance = float.PositiveInfinity;

  public Vector3 acceleration;
  public float velocity;
  public float PE;
  public float KE;
  public float TE;
  public float HE;

  public Text velocityText;
  public Text accelerationText;
  public Text PEText;
  public Text KEText;
  public Text TEText;
  public Text HEText;

  static GameObject selected;
  // Drag & pan helper vars
  Plane dragPlane;
  Vector3 offset;
  Camera mainCam;

  Transform cart;
  Cart cartScript;

  Color flagColor;
  static int colorIndex = 0;
  public void Reset() {
    closest = new Vector3(-100, 0, 0);
    smallestDistance = float.PositiveInfinity;
  }
  // Start is called before the first frame update
  void Start()
  {
    flagColor = Color.HSVToRGB((++colorIndex % 12f) / 12f, .7f, .9f);
    GetComponent<SpriteRenderer>().color = flagColor;
    velocityText = GameObject.Find("Flag Velocity").GetComponent<Text>();
    accelerationText = GameObject.Find("Flag Acceleration").GetComponent<Text>();
    KEText = GameObject.Find("Flag KE").GetComponent<Text>();
    PEText = GameObject.Find("Flag PE").GetComponent<Text>();
    TEText = GameObject.Find("Flag TE").GetComponent<Text>();
    HEText = GameObject.Find("Flag HE").GetComponent<Text>();

    cart = GameObject.Find("Cart").transform;

    cartScript = cart.GetComponent<Cart>();
    mainCam = Camera.main;
    // hitbox = GetComponent<BoxCollider2D>();
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)) {
      if (selected == gameObject){
        selected = null;
        velocityText.text = "Velocity: ";
        accelerationText.text = "Acceleration: ";
        KEText.text = "Kinetic Energy: ";
        HEText.text = "Thermal Energy: ";
        PEText.text = "Potential Energy: ";
        TEText.text = "Total Energy: ";
        Destroy(gameObject);
      }
      
    }
    // Debug.Log(smallestDistance + " "  + Vector3.SqrMagnitude(transform.position - cart.position));
    // Debug.Log(smallestDistance > Vector3.SqrMagnitude(transform.position - cart.position));
    if (Vector3.SqrMagnitude(transform.position - cart.position) < smallestDistance)
    {
      
      closest = cart.position;
      acceleration = cartScript.acceleration;
      velocity = cartScript.vel;
      PE = cartScript.PE;
      KE = cartScript.KE;
      TE = cartScript.TE;
      HE = cartScript.HE;

      smallestDistance = Vector3.SqrMagnitude(transform.position - cart.position);



    }
    if(selected == gameObject) {
      velocityText.text = "Velocity: " + velocity.ToString("F2") + " m/s";
      accelerationText.text = "Acceleration: " + acceleration.magnitude.ToString("F2") + " m/s^2";
    KEText.text = "Kinetic Energy: " + (Mathf.Max(0,KE)).ToString("F2") + " j";
    PEText.text = "Potential Energy: " + (Mathf.Max(0,PE)).ToString("F2") + " j";
    HEText.text = "Thermal Energy: " + (Mathf.Max(0,HE)).ToString("F2") + " j";
    TEText.text = "Total Energy: " + (Mathf.Max(0,TE)).ToString("F2") + " j";
    }
  }

  void OnMouseDown()
  {
    selected = gameObject;
    dragPlane = new Plane(mainCam.transform.forward, transform.position);
    Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);

    float planeDist;
    dragPlane.Raycast(camRay, out planeDist);
    offset = transform.position - camRay.GetPoint(planeDist);
  }

  void OnMouseDrag()
  {
    Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);

    float planeDist;
    dragPlane.Raycast(camRay, out planeDist);
    transform.position = new Vector3(camRay.GetPoint(planeDist).x + offset.x, camRay.GetPoint(planeDist).y + offset.y, camRay.GetPoint(planeDist).z + offset.z);

  }
}
