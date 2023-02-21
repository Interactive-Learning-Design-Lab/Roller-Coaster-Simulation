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
  public bool slowDown = false;
  public float friction = 1f;
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
  [SerializeField]
  bool waiting = false;
  float deltaTime = 0;
  float duration = 0;
  Vector3 initial;
  Vector3? final = null;
  float lastValidVel;

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
    if (track.trackPoints.Count > 0)
    {
      friction = 1;
      slowDown = false;
      waiting = false;
      deltaTime = 0;
      duration = 0;
      final = null;
      waiting = false;
      tooHigh = true;
      positiveVel = true;
      transform.position = track.trackPoints[0];
      GameObject.Find("Wall").transform.position = track.trackPoints[track.trackPoints.Count - 1];
      releaseHeight = transform.position.y;
      TE = mass * 9.81f * releaseHeight + 0.0001f;
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
    Time.timeScale = 4f;
    track = GameObject.FindGameObjectWithTag("Track Manager").GetComponent<TrackManager>();
    if (track.trackPoints.Count > 0)
      transform.position = track.trackPoints[0];
    PauseSim();
  }

  // Update is called once per frame
  void FixedUpdate()
  {

    velocityText.text = "Velocity: " + vel.ToString("F2") + " m/s";
    accelerationText.text = "Acceleration: " + acceleration.magnitude.ToString("F2") + " m/s^2";
    KEText.text = "Kinetic Energy: " + KE.ToString("F2") + " j";
    PEText.text = "Potential Energy: " + PE.ToString("F2") + " j";
    TEText.text = "Total Energy: " + TE.ToString("F2") + " j";
    RAText.text = "Initial Drop: " + releaseHeight.ToString("F2") + " m";


    for (int i = 0; i < 10; i++)
    {
      if (!paused && track.trackPoints.Count > 0 && (!slowDown || vel > 0.25f))
      {
        if(transform.position.x > track.trackPoints[track.trackPoints.Count - 30].x) {
          slowDown = true;
        }
        // get rotation
        float theta = track.GetClosestPointSlope(transform.position);
        transform.rotation = Quaternion.Euler(0, 0, -180 + Mathf.Rad2Deg * theta);

        if (deltaTime <= 0)
        {
          if (final != null)
          {
            transform.position = (Vector3)final;
          }
          // get prev, current, and next point
          Vector3[] closestPoints = track.GetClosestPoints(transform.position);
          Vector3 currentPoint = closestPoints[1];

          // if the cart is too high it'll fall back, change the velocity's sign and start going the other way
          if (transform.position.y + 0.007f >= releaseHeight && !tooHigh)
          {
            tooHigh = true;
            positiveVel = !positiveVel;
          }
          if (transform.position.y + 0.007f < releaseHeight && tooHigh)
          {
            tooHigh = false;
          }

          // choose the next point
          if (positiveVel)
          {
            final = closestPoints[2];
          }
          else
          {
            final = closestPoints[0];
          }
          
          
          if (slowDown){
            friction = friction * (1 - 0.08f);
          // calculate speed from kinetic energy
          // PE = mass * 9.81f * transform.position.y * friction;
          // TE *= 1 - (0.99f * Time.fixedDeltaTime);
          }
          
          PE = mass * 9.81f * transform.position.y;// * friction;
          if (PE > TE) {
            tooHigh = true;
            vel = lastValidVel;
            final = initial;
          } else {
            KE = TE - PE;
            vel = Mathf.Sqrt((2 * KE) / mass) * friction*friction;
            if (slowDown)
              KE = 0.5f * mass * vel * vel;
          }
          
          lastValidVel = vel;

          initial = closestPoints[1];

          // find the time it takes to get to the next point
          deltaTime = Vector3.Magnitude((Vector3)final - initial) / vel * 50f ;
          duration = deltaTime;
        }

        
        Vector3 target = Vector3.Lerp(transform.position, (Vector3)final, Time.fixedDeltaTime / duration);
        if (target.y < releaseHeight)
        transform.position = target;
        deltaTime -= Time.fixedDeltaTime;
        // if ((Vector3.SqrMagnitude(closestPoints[1] - closestPoints[2]) < 0.0001f && positiveVel) || 
        //     (Vector3.SqrMagnitude(closestPoints[0] - closestPoints[1]) < 0.0001f && !positiveVel))
        // {
        //   velocity = Vector3.zero;
        //   acceleration = Vector3.zero;
        //   accel = 0;
        // }
      }
      else if (!paused)
      {
        KE = 0;
        vel = 0;
      }
    }
  }
}
