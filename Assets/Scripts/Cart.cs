using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class Cart : MonoBehaviour
{
  SpriteRenderer sr;
  [SerializeField]
  public bool paused = true;
  public bool atStart = true;
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
  public float d_PE = 0;
  public float d_HE = 0;
  public float d_KE = 0;
  public float d_totalEnergy = 0;
  public Vector3 lastVel = Vector3.zero;
  public Vector3 velVector = Vector3.zero;
  public static string tracks = "";

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
  public Material frictionRoad;

  [SerializeField]
  bool onTrack = false;

  [SerializeField]
  float tolerance = 0.5f;

  public TrackManager track;
  private float theta;
  public Vector3 d_acc;
  public Vector3 d_vel;
  private Vector3 d_previous_vel;

  public void StartSim()
  {
    pauseButton.SetActive(true);
    startButton.SetActive(false);
    paused = false;
    track.Exit();
    TrackManager.HideError();
  }

  public void PauseSim()
  {
    pauseButton.SetActive(false);
    startButton.SetActive(true);
    paused = true;
    TrackManager.HideError();
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
    PauseSim();
    atStart = true;

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

      GameObject[] flags = GameObject.FindGameObjectsWithTag("Flag");
      foreach (GameObject flag in flags)
      {
        flag.GetComponent<Flag>().Reset();
      }
      float theta = track.GetClosestPointSlope(transform.position);
      transform.rotation = Quaternion.Euler(0, 0, -180 + Mathf.Rad2Deg * theta);
      Show();
    }
    else
    {
      releaseHeight = 0;
    }
    float d_ReleaseHeight = round(releaseHeight);
    d_totalEnergy = round(mass * 9.81f * d_ReleaseHeight);
    d_PE = d_totalEnergy;
    d_acc = Vector3.zero;
    d_KE = 0f;
    d_HE = 0f;
    d_previous_vel = Vector3.zero;
    d_vel = Vector3.zero;


    velocityText.text = "Velocity: " + d_vel.magnitude.ToString("F2", CultureInfo.InvariantCulture) + " m/s";
    // accelerationText.text = "Acceleration: " + d_acc.magnitude.ToString("F2", CultureInfo.InvariantCulture) + " m/s^2";
    KEText.text = "Kinetic Energy: " + d_KE.ToString("F2", CultureInfo.InvariantCulture) + " J";
    PEText.text = "Potential Energy: " + d_PE.ToString("F2", CultureInfo.InvariantCulture) + " J";
    HEText.text = "Thermal Energy: " + d_HE.ToString("F2", CultureInfo.InvariantCulture) + " J";
    TEText.text = "Total Energy: " + d_totalEnergy.ToString("F2", CultureInfo.InvariantCulture) + " J";
    RAText.text = "Initial Drop: " + d_ReleaseHeight.ToString("F2", CultureInfo.InvariantCulture) + " m";

    float maxVal = 0;
    if (d_totalEnergy >= 0.1f)
    {
      maxVal = d_totalEnergy;
      PESlider.maxValue = maxVal;
      PESlider.value = d_PE;
    }
    else
    {
      PESlider.maxValue = maxVal;
      PESlider.value = 0;
      maxVal = 1f;
    }
    KESlider.value = 0;
    HESlider.value = 0;

    KESlider.maxValue = 1f;
    HESlider.maxValue = 1f;

  }

  public void Hide()
  {
    PauseSim();
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
    transform.localScale = new Vector3(Mathf.Pow(mass, 0.1771838201f)*0.75f, Mathf.Pow(mass, 0.1771838201f)*0.75f, transform.localScale.z);
    GameObject.Find("MText").GetComponent<Text>().text = "Mass: " + mass.ToString("F2", CultureInfo.InvariantCulture) + " kg";
    RestartSim();
  }

  public void SetFriction(float friction)
  {
    this.mu = friction;
    GameObject.Find("FText").GetComponent<Text>().text = "Friction: " + (mu / 100f).ToString("F2", CultureInfo.InvariantCulture);
    frictionRoad.color = (Mathf.Round(friction * 100f) > 0f) ? 
      Color.HSVToRGB(7/360f, .49f, 1f) : 
      Color.HSVToRGB(127/360f, .49f, 1f);
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
    // accelerationText = GameObject.Find("Acceleration").GetComponent<Text>();
    KEText = GameObject.Find("KE").GetComponent<Text>();
    KESlider = GameObject.Find("KESlider").GetComponent<Slider>();
    HEText = GameObject.Find("HE").GetComponent<Text>();
    HESlider = GameObject.Find("HESlider").GetComponent<Slider>();
    PEText = GameObject.Find("PE").GetComponent<Text>();
    PESlider = GameObject.Find("PESlider").GetComponent<Slider>();
    TEText = GameObject.Find("TE").GetComponent<Text>();
    RAText = GameObject.Find("RA").GetComponent<Text>();
    // Time.timeScale = 4f;
    track = GameObject.FindGameObjectWithTag("Track Manager").GetComponent<TrackManager>();
    if (track.trackPoints.Count > 0)
      transform.position = track.trackPoints[2];
    RestartSim();
  }


  float round(float v)
  {
    return Mathf.Max(0.00f, Mathf.Round(v * 100f) / 100f);
  }

  void OnDrawGizmos()
  {
    // Gizmos.color = Color.red;
    // Gizmos.DrawLine(transform.position, transform.position - velVector);
    // Gizmos.color = Color.green;
    // Gizmos.DrawLine(transform.position, transform.position - lastVel);
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    float d_ReleaseHeight = round(releaseHeight);
    if (!paused)
    {
      atStart = false;

      d_totalEnergy = round(mass * 9.81f * d_ReleaseHeight);

      d_PE = round(mass * 9.81f * transform.position.y);
      if (transform.position.y <= 0.02f)
      {
        d_PE = 0;
      }

      d_HE = round(HE / (initialTotal - PE) * (d_totalEnergy - d_PE));
      d_KE = round(d_totalEnergy - (d_PE + d_HE));

      if (d_HE != d_HE || d_HE <= 0.02f)
      {
        d_HE = 0;
      }

      if (d_KE != d_KE || d_KE <= 0.02f)
      {
        d_KE = 0;
        d_vel = Vector3.zero;
      }

      {
        d_PE = d_totalEnergy - (d_HE + d_KE);
      }

      velocityText.text = "Velocity: " + d_vel.magnitude.ToString("F2", CultureInfo.InvariantCulture) + " m/s";
      KEText.text = "Kinetic Energy: " + d_KE.ToString("F2", CultureInfo.InvariantCulture) + " J";
      PEText.text = "Potential Energy: " + d_PE.ToString("F2", CultureInfo.InvariantCulture) + " J";
      HEText.text = "Thermal Energy: " + d_HE.ToString("F2", CultureInfo.InvariantCulture) + " J";
      TEText.text = "Total Energy: " + d_totalEnergy.ToString("F2", CultureInfo.InvariantCulture) + " J";
      RAText.text = "Initial Drop: " + d_ReleaseHeight.ToString("F2", CultureInfo.InvariantCulture) + " m";

      float maxVal = 0;
      if (d_totalEnergy >= 0.1f)
      {
        maxVal = d_totalEnergy;
        KESlider.value = d_KE;
        PESlider.value = d_PE;
        HESlider.value = d_HE;
      }
      else
      {
        KESlider.value = 0;
        PESlider.value = 0;
        HESlider.value = 0;
        maxVal = 1f;
      }

      KESlider.maxValue = maxVal;
      PESlider.maxValue = maxVal;
      HESlider.maxValue = maxVal;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
      bool atEnd = (Vector3.SqrMagnitude(transform.position - track.trackPoints[track.trackPoints.Count - 1]) <= Vector3.kEpsilon * Vector3.kEpsilon);
      string frontendJSON = "{" +
                          "\"acceleration\": " + d_acc.magnitude.ToString("F2", CultureInfo.InvariantCulture) +
                          ", \"velocity\": " + d_vel.magnitude.ToString("F2", CultureInfo.InvariantCulture) +
                          ", \"position\": " + TrackManager.formatVector(transform.position) +
                          ", \"kinetic_energy\": " + d_KE.ToString("F2", CultureInfo.InvariantCulture) +
                          ", \"potential_energy\": " + d_PE.ToString("F2", CultureInfo.InvariantCulture) +
                          ", \"thermal_energy\": " + d_HE.ToString("F2", CultureInfo.InvariantCulture) +
                          ", \"total_energy\": " + d_totalEnergy.ToString("F2", CultureInfo.InvariantCulture) +
                          ", \"initial_drop\": " + d_ReleaseHeight.ToString("F2", CultureInfo.InvariantCulture) +
                          ", \"mass\": " + mass.ToString("F2", CultureInfo.InvariantCulture) +
                          ", \"friction\": " + (mu / 100f).ToString("F2", CultureInfo.InvariantCulture) +
                          ", \"at_end\": " + atEnd.ToString().ToLower() +
                          ", \"tracks\" : [" + tracks + "]" +
                          "}";
      WebGLPluginJS.PassTextParam(frontendJSON);
#endif


    for (int i = 0; i < 1; i++)
    {
      if (!paused && track.trackPoints.Count > 0 && (!slowDown || vel > 0.01f))
      {
        if (transform.position.x >= track.trackPoints[track.trackPoints.Count - 31].x)
        {
          slowDown = true;
        }
        // get rotation
        theta = track.GetClosestPointSlope(transform.position);

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
          while (deltaTime < Time.fixedUnscaledDeltaTime)
          {
            // get prev, current, and next point
            Vector3[] closestPoints = track.GetClosestPoints((Vector3)final);

            // if current and next point are the same, this means cart hit the wall 
            if (Vector3.SqrMagnitude(closestPoints[1] - closestPoints[2]) < Vector3.kEpsilonNormalSqrt)
            {
              // transfer all velocity to acceleration
              d_acc = vel * transform.right / deltaTime;
              vel = 0;
              break;
            }

            // set initial to current point
            initial = closestPoints[1];

            // if the cart is too high it'll fall back, change the velocity's sign and start going the other way
            if (initial.y + 0.009f >= releaseHeight && !tooHigh)
            {
              tooHigh = true;
              positiveVel = !positiveVel;
            }
            // detect when cart is no longer too high
            if (initial.y + 0.03f < releaseHeight && tooHigh)
            {
              tooHigh = false;
            }

            // choose the next point to go to (either previous or next point in track)
            if (positiveVel)
            {
              final = closestPoints[2];
            }
            else
            {
              final = closestPoints[0];
            }

            // calculate actual potential energy (PE) from height
            PE = mass * 9.81f * transform.position.y;
            // if PE calculated from height > total energy (TE) something is wrong
            if (PE > TE && !slowDown)
            {
              // revert to last valid velocity to avoid climbing further up
              vel = lastValidVel;
              // fix PE value to equal what it should equal (TE - kinetic energy)
              PE = TE - KE;
            }
            else
            {
              // if cart is slowing down and not too high
              if (slowDown && !tooHigh)
              {
                // start turning velocity into thermal energy (HE)
                HE += mu * 4 * vel * Time.fixedUnscaledDeltaTime;
              }
              // recalculate KE in case HE is increased
              KE = TE - (PE + HE);
              // if KE <= 0, this means car stopped
              if (KE <= 0)
              {
                // set KE, vel to 0
                KE = 0;
                vel = 0;
                // transfer all remaining energy to heat
                HE = TE - PE;
                d_HE += d_KE;
                // reset acc and KE
                d_acc = Vector3.zero;
                d_KE = 0;
                d_vel = Vector3.zero;
                UpdateDynamic();
                PauseSim();
                break;
              }
              else
              {

                vel = Mathf.Sqrt((2 * KE) / mass);
                UpdateDynamic();
              }
            }

            // now the current velocity is the last valid velocity
            lastValidVel = vel;

            // find the total time it takes to get to the next point
            deltaTime += Vector3.Magnitude((Vector3)final - initial) / vel;
            duration = deltaTime;

            if (q++ > 100)
            { /* HE += KE; KE = 0;vel = 0*/
              if (slowDown)
                vel = 0;
              else
              {
                if (!positiveVel)
                {
                  final = closestPoints[2];
                }
                else
                {
                  final = closestPoints[0];
                }
                // transform.position = (Vector3) final;
                // duration *= 2f * vel;
                // deltaTime *= 2f * vel;
                // Debug.Log("break while");
                break;
                // duration = Time.fixedDeltaTime;
              }
              // break;
            }
          }
        }

        Vector3 target = Vector3.Lerp(transform.position, (Vector3)final, Time.fixedUnscaledDeltaTime / duration);
        if (target.y < releaseHeight)
          transform.position = target;
        deltaTime -= Time.fixedUnscaledDeltaTime;
        d_vel = Mathf.Sqrt((2 * KE) / mass) * transform.right;
        d_acc = (d_vel - d_previous_vel) / duration;
        d_previous_vel = d_vel;

        Debug.DrawRay(transform.position, transform.position + d_acc, Color.blue);
        Debug.DrawRay(transform.position, transform.position + d_vel, Color.green);
        // if ((Vector3.SqrMagnitude(closestPoints[1] - closestPoints[2]) < 0.0001f && positiveVel) || 
        //     (Vector3.SqrMagnitude(closestPoints[0] - closestPoints[1]) < 0.0001f && !positiveVel))
        // {
        //   velocity = Vector3.zero;
        //   acceleration = Vector3.zero;
        //   accel = 0;
        // }
        // if (Vector3.SqrMagnitude((finalVelocity - initial) / Time.fixedDeltaTime) > float.Epsilon * float.Epsilon) {
        //   acceleration = (finalVelocity - initial) / Time.fixedDeltaTime / Time.timeScale / 5f; 
        // }
      }
      else if (!paused)
      {

        PauseSim();

        HE += KE;
        KE = 0;
        vel = 0;

        d_acc = Vector3.zero;
        d_HE += d_KE;
        d_KE = 0;
        d_vel = Vector3.zero;
        UpdateDynamic();
      }
    }
  }

  void UpdateDynamic()
  {
    velocityText.text = "Velocity: " + d_vel.magnitude.ToString("F2", CultureInfo.InvariantCulture) + " m/s";
    // accelerationText.text = "Acceleration: " + d_acc.magnitude.ToString("F2", CultureInfo.InvariantCulture) + " m/s^2";
    KEText.text = "Kinetic Energy: " + d_KE.ToString("F2", CultureInfo.InvariantCulture) + " J";
    PEText.text = "Potential Energy: " + d_PE.ToString("F2", CultureInfo.InvariantCulture) + " J";
    HEText.text = "Thermal Energy: " + d_HE.ToString("F2", CultureInfo.InvariantCulture) + " J";
    KESlider.value = d_KE;
    HESlider.value = d_HE;

    float maxVal = 0;
    if (d_totalEnergy >= 0.1f)
    {
      maxVal = d_totalEnergy;
    }
    else
    {
      maxVal = 1f;
    }

    KESlider.maxValue = maxVal;
    HESlider.maxValue = maxVal;
  }
}
