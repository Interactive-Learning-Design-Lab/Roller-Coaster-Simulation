using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cart : MonoBehaviour
{
  [SerializeField]
  bool paused = true;

  public float mass = 1f;

  float gravity = 0.00001f * 9.81f;

  public float KE;
  public float PE;
  public float TE;
  public Vector3 netForce;
  public Vector3 acceleration;
  public Vector3 velocity = new Vector3(1f, 0);
  public float releaseHeight;


  Text velocityText;
  Text accelerationText;
  Text KEText;
  Text PEText;
  Text TEText;
  Text RAText;
  GameObject startButton;
  GameObject pauseButton;

  [SerializeField]
  bool onTrack = false;

  [SerializeField]
  float tolerance = 0.5f;

  TrackManager track;

  public void StartSim()
  {
    pauseButton.SetActive(true);
    startButton.SetActive(false);
    paused = false;
  }

  public void PauseSim()
  {
    pauseButton.SetActive(false);
    startButton.SetActive(true);
    paused = true;
  }

  public void RestartSim()
  {
    transform.position = track.trackPoints[0];
    releaseHeight = transform.position.y;
    TE = mass * gravity * releaseHeight;
    velocity = Vector3.zero;
    acceleration = Vector3.zero;
    pauseButton.SetActive(true);
    startButton.SetActive(false);
    paused = false;
    GameObject[] flags = GameObject.FindGameObjectsWithTag("Flag");
    foreach (GameObject flag in flags)
    {
      flag.GetComponent<Flag>().Reset();
    }
  }

  public void SetMass(float mass)
  {
    this.mass = mass;
  }

  // Start is called before the first frame update
  void Start()
  {
    startButton = GameObject.Find("Start");
    pauseButton = GameObject.Find("Pause");
    // RestartSim();
    // PauseSim();

    velocityText = GameObject.Find("Velocity").GetComponent<Text>();
    accelerationText = GameObject.Find("Acceleration").GetComponent<Text>();
    KEText = GameObject.Find("KE").GetComponent<Text>();
    PEText = GameObject.Find("PE").GetComponent<Text>();
    TEText = GameObject.Find("TE").GetComponent<Text>();
    RAText = GameObject.Find("RA").GetComponent<Text>();
    Time.timeScale = 2f;
    track = GameObject.FindGameObjectWithTag("Track Manager").GetComponent<TrackManager>();
    transform.position = track.trackPoints[0];
    PauseSim();
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    velocityText.text = "Velocity:\n" + velocity.magnitude;
    accelerationText.text = "Acceleration:\n" + acceleration.magnitude;
    KEText.text = "Kinetic Energy:\n" + KE;
    PEText.text = "Potential Energy:\n" + PE;
    TEText.text = "Total Energy:\n" + TE;
    RAText.text = "Release Height:\n" + releaseHeight;
    // for (int i = 0; i < 1 && track.trackPoints.Count > 0; i++)
    if (!paused && track.trackPoints.Count > 0)
    {

      float theta = track.GetClosestPointSlope(transform.position);

      transform.rotation = Quaternion.Euler(0, 0, -180 + Mathf.Rad2Deg * theta);

      Vector3[] closestPoints = track.GetClosestPoints(transform.position);
      //Debug.Log((closestPoints[2]-closestPoints[0]).ToString());
      Vector3 currentPoint = closestPoints[1];
      netForce = Vector3.zero;

      Vector3 weight = mass * gravity * Vector3.down;
      Debug.Log("weight.ToString()");
      //Debug.Log(weight[1]);

      netForce += weight;
      //Debug.DrawRay(transform.position, weight * 100, Color.blue); // weight: blue


      Vector3 normalForce = weight.magnitude * Mathf.Cos(theta) * -transform.up;
      netForce += normalForce;
      //Debug.DrawRay(transform.position, normalForce * 10000, Color.red); // normal: red



      //Debug.DrawRay(transform.position, netForce * 100000, Color.white);


      // apply forces
      acceleration = (1/mass)*netForce;
      Debug.Log(acceleration.magnitude);
      //Debug.DrawRay(transform.position, acceleration * 100, Color.cyan);

      velocity += acceleration.magnitude * Mathf.Sin(theta) * transform.right;
      //Debug.DrawRay(transform.position, velocity * 100, Color.magenta);
      if (Vector3.SqrMagnitude(closestPoints[1] - closestPoints[2]) < 0.0001f)
      {
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
      }
      transform.position += velocity.magnitude * Vector3.Normalize(closestPoints[2] - closestPoints[0]);// * Time.fixedDeltaTime;
      //Debug.Log(theta);

      PE = mass * 9.81f * transform.position.y;
      KE = TE-KE; 0.5f * mass * velocity.sqrMagnitude;
      

      // Vector3[]? closest = null;
      // int k = 10;
      // for (int j = 0; j < k; j++)
      // {
      //   closest = track.GetClosestPoints(transform.position);
      //   transform.position = Vector3.MoveTowards(transform.position, closest[2], velocity.magnitude * 1 / k * Time.fixedDeltaTime);
      //   closest = track.GetClosestPoints(transform.position);

      // }


      // if (closest != null && Vector3.SqrMagnitude(closest[1] - currentPoint) > 0.0001f
      // && Vector3.SqrMagnitude(closest[1] - transform.position) < tolerance)
      // {
      //   transform.position = closest[1];
      //   Debug.Log("teleported");
      // }
      // // Quaternion.Euler(0, 0, -theta) * Vector3.right * Vector3.Magnitude(velocity);
      // Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, -theta) * Vector3.right * Vector3.Magnitude(velocity));


    }
    else if (!paused)
    {
      velocity = Vector3.zero;
      acceleration = Vector3.zero;
    }
  }
}
