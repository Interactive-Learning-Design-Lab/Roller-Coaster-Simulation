using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flag : MonoBehaviour
{
  Vector3 closest = new Vector3(-100, 0, 0);
  float smallestDistance = float.PositiveInfinity;

  public Vector3 acceleration;
  public Vector3 velocity;
  public float pe;
  public float ke;

  public Text velText;
  public Text accText;
  static GameObject selected;
  // Drag & pan helper vars
  Plane dragPlane;
  Vector3 offset;
  Camera mainCam;

  Transform cart;
  Cart cartScript;

  public void Reset() {
    closest = new Vector3(-100, 0, 0);
    smallestDistance = float.PositiveInfinity;
  }
  // Start is called before the first frame update
  void Start()
  {
    velText = GameObject.Find("Flag Velocity").GetComponent<Text>();
    accText = GameObject.Find("Flag Acceleration").GetComponent<Text>();

    cart = GameObject.Find("Cart").transform;

    cartScript = cart.GetComponent<Cart>();
    mainCam = Camera.main;
    // hitbox = GetComponent<BoxCollider2D>();
  }

  // Update is called once per frame
  void Update()
  {

    // Debug.Log(smallestDistance + " "  + Vector3.SqrMagnitude(transform.position - cart.position));
    // Debug.Log(smallestDistance > Vector3.SqrMagnitude(transform.position - cart.position));
    if (Vector3.SqrMagnitude(transform.position - cart.position) < smallestDistance)
    {
      
      closest = cart.position;
      acceleration = cartScript.acceleration;
      velocity = cartScript.velocity;
      // pe = cart.pe;
      smallestDistance = Vector3.SqrMagnitude(transform.position - cart.position);
      // ke = cart.ke;


    }
    if(selected == gameObject) {
      velText.text = "Velocity: " + velocity.magnitude;
      accText.text = "Acceleration: " + acceleration.magnitude;
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
