using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cart : MonoBehaviour
{
  public float mass = 5f;

  float gravity = 9.81f;

  public Vector3 netForce;
  public Vector3 acceleration;
  public Vector3 velocity;

  Text velocityText;
  Text accelerationText;

  [SerializeField]
  bool onTrack = false;

  [SerializeField]
  float tolerance = 0.5f;

  TrackManager track;


  // Start is called before the first frame update
  void Start()
  {
    velocityText = GameObject.Find("Velocity").GetComponent<Text>();
    accelerationText = GameObject.Find("Acceleration").GetComponent<Text>();
    Time.timeScale = 0.2f;
    track = GameObject.FindGameObjectWithTag("Track Manager").GetComponent<TrackManager>();
    transform.position = track.trackPoints[0];
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    velocityText.text = "Velocity: " + velocity.magnitude;
    accelerationText.text = "Acceleration: " + acceleration.magnitude;
    // for (int i = 0; i < 1 && track.trackPoints.Count > 0; i++)
    if (track.trackPoints.Count > 0)
    {
      netForce = Vector3.zero;

      float theta = track.GetClosestPointSlope(transform.position);

      transform.rotation = Quaternion.Euler(0, 0, -180 + Mathf.Rad2Deg * theta);


      Vector3 currentPoint = track.GetClosestPoints(transform.position)[1];



      Vector3 normalForce = transform.up * mass * gravity * Mathf.Cos(theta);
      Debug.DrawRay(transform.position, normalForce, Color.red);
      netForce += normalForce;


      Vector3 weight = mass * gravity * Vector3.down;
      Debug.DrawRay(transform.position, weight);

      netForce += weight;
      Debug.DrawRay(transform.position, netForce, Color.green);


      // apply forces
      acceleration = 1 * netForce / mass;
      velocity += acceleration * Time.fixedDeltaTime;
      Debug.Log(theta);

      Vector3[]? closest = null;
      int k = 10;
      for (int j = 0; j < k; j++)
      {
        closest = track.GetClosestPoints(transform.position);
        transform.position = Vector3.MoveTowards(transform.position, closest[2], velocity.magnitude * 1 / k * Time.fixedDeltaTime);
        closest = track.GetClosestPoints(transform.position);

      }


      if (closest != null && Vector3.SqrMagnitude(closest[1] - currentPoint) > 0.0001f
      && Vector3.SqrMagnitude(closest[1] - transform.position) < tolerance)
      {
        transform.position = closest[1];
        Debug.Log("teleported");
      }
      // Quaternion.Euler(0, 0, -theta) * Vector3.right * Vector3.Magnitude(velocity);
      Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, -theta) * Vector3.right * Vector3.Magnitude(velocity));

      if (Vector3.SqrMagnitude(closest[1] - closest[2]) < 0.0001f){
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
      }
    }
  }
}
