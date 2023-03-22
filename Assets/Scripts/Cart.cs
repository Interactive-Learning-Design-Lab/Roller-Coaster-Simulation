using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cart : MonoBehaviour
{
  SpriteRenderer sr;
  [SerializeField]
  public bool paused = false;
  public float mu = .8f;
  public float mass = 1f;

  float gravity = 0.00001f * 9.81f;
  public bool slowDown = false;
  public float friction = 1f;
  public float initialTotal;
  public float KE;
  public float PE;
  public float TE;
  public float HE;
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
  Slider KESlider;
  Text HEText;
  Slider HESlider;
  Text PEText;
  Slider PESlider;
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

    yield return new WaitForSecondsRealtime(0.01f);
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
      transform.position = track.trackPoints[2];
      GameObject.Find("Wall").transform.position = track.trackPoints[track.trackPoints.Count - 1];
      releaseHeight = track.trackPoints[0].y;
      TE = mass * (9.81f * releaseHeight + 0.00001f);
      initialTotal = TE;
      PE = TE;
      KE = 0;
      HE = 0;
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
      float theta = track.GetClosestPointSlope(transform.position);
      transform.rotation = Quaternion.Euler(0, 0, -180 + Mathf.Rad2Deg * theta);
      Show();
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
    GameObject.Find("MText").GetComponent<Text>().text = "Mass: " + mass.ToString("F2") + " kg";
    RestartSim();
  }

  public void SetFriction(float friction)
  {
    this.mu = friction;
    GameObject.Find("FText").GetComponent<Text>().text = "Friction: " + (mu/100f).ToString("F2");
    RestartSim();
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
    KESlider = GameObject.Find("KESlider").GetComponent<Slider>();
    HEText = GameObject.Find("HE").GetComponent<Text>();
    HESlider = GameObject.Find("HESlider").GetComponent<Slider>();
    PEText = GameObject.Find("PE").GetComponent<Text>();
    PESlider = GameObject.Find("PESlider").GetComponent<Slider>();
    TEText = GameObject.Find("TE").GetComponent<Text>();
    RAText = GameObject.Find("RA").GetComponent<Text>();
    Time.timeScale = 4f;
    track = GameObject.FindGameObjectWithTag("Track Manager").GetComponent<TrackManager>();
    if (track.trackPoints.Count > 0)
      transform.position = track.trackPoints[2];
    RestartSim();
  }


  float round(float v) {
    return Mathf.Max(0.00f, Mathf.Round(v * 100) / 100f);
  }
  // Update is called once per frame
  void FixedUpdate()
  {
    float d_ReleaseHeight = round(releaseHeight);
    float d_totalEnergy = round(mass * 9.81f * d_ReleaseHeight);
    
    float d_PE = round(mass * 9.81f * transform.position.y);
    if (d_PE <= 0.02)
    {
      d_PE = 0;
    }

    float d_HE = round(HE / (initialTotal - PE) * (d_totalEnergy - d_PE));
    float d_KE = round(d_totalEnergy - (d_PE + d_HE));

    if (d_HE != d_HE || d_HE <= 0.02f)
    {
      d_HE = 0;
    }

    if (d_KE != d_KE || d_KE <= 0.02f)
    {
      d_KE = 0;
    }
    
    // if (d_HE <= 0.01f || d_KE <= 0.01f)
    {
      d_PE = d_totalEnergy - (d_HE + d_KE);
    }
    
    // float d_velocity = Mathf.Sqrt((2 * d_KE) / mass);



    velocityText.text = "Velocity: " + vel.ToString("F2") + " m/s";
    accelerationText.text = "Acceleration: " + acceleration.magnitude.ToString("F2") + " m/s^2";
    KEText.text = "Kinetic Energy: " + d_KE.ToString("F2") + " j";
    PEText.text = "Potential Energy: " + d_PE.ToString("F2") + " j";
    HEText.text = "Thermal Energy: " + d_HE.ToString("F2") + " j";
    TEText.text = "Total Energy: " + d_totalEnergy.ToString("F2") + " j";
    RAText.text = "Initial Drop: " + d_ReleaseHeight.ToString("F2") + " m";
    float maxVal = 0;
    if (initialTotal >= 0.1f)
    {
      maxVal = initialTotal;
      KESlider.value = KE;
      PESlider.value = PE;
      HESlider.value = HE;
    } else {
      KESlider.value = 0;
      PESlider.value = 0;
      HESlider.value = 0;
      maxVal = 1f;
    }
    KESlider.maxValue = initialTotal;
    PESlider.maxValue = initialTotal;
    HESlider.maxValue = initialTotal;


    for (int i = 0; i < 1; i++)
    {
      if (!paused && track.trackPoints.Count > 0 && (!slowDown || vel > 0.01f))
      {

        if (transform.position.x >= track.trackPoints[track.trackPoints.Count - 31].x)
        {
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
          else
          {
            final = transform.position;
          }


          int q = 0;
          while (deltaTime < Time.fixedDeltaTime)
          {
            // get prev, current, and next point
            Vector3[] closestPoints = track.GetClosestPoints((Vector3)final);
            if (closestPoints[1] == closestPoints[2])
            {
              vel = 0;
              break;
            }
            initial = closestPoints[1];

            // if the cart is too high it'll fall back, change the velocity's sign and start going the other way
            if (transform.position.y + 0.007f >= releaseHeight && !tooHigh)
            {
              tooHigh = true;
              positiveVel = !positiveVel;
            }
            if (transform.position.y + 0.009f< releaseHeight && tooHigh)
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
            float tvel = vel;
            PE = mass * 9.81f * transform.position.y;// * friction;
            if (PE > TE)
            {
              // tooHigh = true;
              vel = lastValidVel;
              final = initial;
              PE = TE - KE;

            }
            else
            {
              KE = TE - PE;
              vel = Mathf.Sqrt((2 * KE) / mass);
              if (slowDown && !tooHigh)
              {
                TE = PE + KE - mu * 4 * vel * Time.fixedDeltaTime;
                HE += mu * 4 * vel * Time.fixedDeltaTime;
              }
            }

            acceleration = Vector3.right * (tvel-vel)/Time.fixedDeltaTime;
            lastValidVel = vel;


            // find the time it takes to get to the next point
            deltaTime += Vector3.Magnitude((Vector3)final - initial) / vel * 5f;
            if (q++ > 50) { /* HE += KE; KE = 0;vel = 0*/;if (slowDown) vel = 0; break; }

          }
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

        PauseSim();
        HE += KE;
        KE = 0;
        vel = 0;
      }
    }
  }
}
