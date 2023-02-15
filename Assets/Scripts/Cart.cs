using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cart : MonoBehaviour
{
  SpriteRenderer sr;
  [SerializeField]
  public bool paused = false;

  public float mass = 1f;

  float gravity = 0.00001f * 9.81f;

  public float KE;
  public float PE;
  public float TE;
  public Vector3 netForce;
  public Vector3 acceleration;
  public float accel;
  public Vector3 velocity = new Vector3(1f, 0);
  public float vel;
  public float releaseHeight;
  [SerializeField]
  bool tooHigh = true;
  [SerializeField]
  bool positiveVel = true;

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

  public void StepSim()
  {
    StartCoroutine(StepCoroutine());

  }

  IEnumerator StepCoroutine()
  {
    StartSim();

    yield return new WaitForSeconds(0.01f);
    PauseSim();

  }

  public void RestartSim()
  {
    tooHigh = true;
    positiveVel = true;
    transform.position = track.trackPoints[0];
    GameObject.Find("Wall").transform.position = track.trackPoints[track.trackPoints.Count - 1];
    releaseHeight = transform.position.y;
    TE = mass * 9.81f * releaseHeight + 0.02f;
    velocity = Vector3.zero;
    vel = 0f;
    acceleration = Vector3.zero;
    accel = 0f;
    PauseSim();
    GameObject[] flags = GameObject.FindGameObjectsWithTag("Flag");
    foreach (GameObject flag in flags)
    {
      flag.GetComponent<Flag>().Reset();
    }
  }

  public void Hide()
  {
    GameObject.Find("Wall").GetComponent<SpriteRenderer>().enabled = false;
    sr.enabled = false;
  }

  public void Show()
  {
    GameObject.Find("Wall").GetComponent<SpriteRenderer>().enabled = true;
    sr.enabled = true;
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

    sr = GetComponent<SpriteRenderer>();
    velocityText = GameObject.Find("Velocity").GetComponent<Text>();
    accelerationText = GameObject.Find("Acceleration").GetComponent<Text>();
    KEText = GameObject.Find("KE").GetComponent<Text>();
    PEText = GameObject.Find("PE").GetComponent<Text>();
    TEText = GameObject.Find("TE").GetComponent<Text>();
    RAText = GameObject.Find("RA").GetComponent<Text>();
    Time.timeScale = 1f;
    track = GameObject.FindGameObjectWithTag("Track Manager").GetComponent<TrackManager>();
    if (track.trackPoints.Count > 0)
      transform.position = track.trackPoints[0];
    PauseSim();
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    velocityText.text = "Velocity: " + velocity.magnitude.ToString("F2") + " m/s";
    accelerationText.text = "Acceleration: " + acceleration.magnitude.ToString("F2") + " m/s^2";
    KEText.text = "Kinetic Energy: " + KE.ToString("F2") + " j";
    PEText.text = "Potential Energy: " + PE.ToString("F2") + " j";
    TEText.text = "Total Energy: " + TE.ToString("F2") + " j";
    RAText.text = "Initial Drop: " + releaseHeight.ToString("F2") + " m";

    if (!paused && track.trackPoints.Count > 0)
    {
      // get rotation
      float theta = track.GetClosestPointSlope(transform.position);
      transform.rotation = Quaternion.Euler(0, 0, -180 + Mathf.Rad2Deg * theta);

      // get prev, current, and next point
      Vector3[] closestPoints = track.GetClosestPoints(transform.position);
      Vector3 currentPoint = closestPoints[1];

      // if the cart is too high it'll fall back, change the velocity's sign and start going the other way
      if (transform.position.y >= releaseHeight && !tooHigh)
      {
        tooHigh = true;
        positiveVel = !positiveVel;
      }
      if (transform.position.y < releaseHeight && tooHigh)
      {
        tooHigh = false;
      }

      // calculate speed from kinetic energy
      PE = mass * 9.81f * transform.position.y;
      KE = Mathf.Abs(TE - PE);
      vel = Mathf.Sqrt((2 * KE) / mass);

      if (positiveVel)
      {
        transform.position += vel * Vector3.Normalize(closestPoints[2] - closestPoints[0]) * Time.fixedDeltaTime;
      }
      else
      {
        transform.position -= vel * Vector3.Normalize(closestPoints[2] - closestPoints[0]) * Time.fixedDeltaTime;
      }


      if ((Vector3.SqrMagnitude(closestPoints[1] - closestPoints[2]) < 0.0001f && positiveVel) || 
          (Vector3.SqrMagnitude(closestPoints[0] - closestPoints[1]) < 0.0001f && !positiveVel))
      {
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        accel = 0;
      }



    }
    else if (!paused)
    {
      velocity = Vector3.zero;
      acceleration = Vector3.zero;
    }
  }
}
