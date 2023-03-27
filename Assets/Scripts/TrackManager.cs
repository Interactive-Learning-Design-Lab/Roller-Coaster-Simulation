using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackManager : MonoBehaviour
{
  public List<Vector3> trackPoints = new List<Vector3>();
  LineRenderer lineRenderer;

  [SerializeField]
  public GameObject connectorPrefab;
  public GameObject trackPrefab;
  public GameObject flagPrefab;
  private static TrackManager _instance;
  Text widthText;
  Text heightText;

  Vector3[] closestPoints;
  // float lastCached = -1f;

  public static int type;
  public static string height;
  public static string width;
  public static Track selected;
  public static GameObject selectedFlag;
  public static int trackCount;
  public static int errCount = 0;

  public void SetType(int _type)
  {
    type = _type;
    EditTrack();
  }

  public void SetCreationType(int _type)
  {
    type = _type;
  }

  public void SetHeight(string _height)
  {
    height = _height;
  }

  public void setWidth(string _width)
  {
    width = _width;
  }

  public void SetHeight(float _height)
  {
    height = _height + "";
    heightText.text = "Height: " + (_height).ToString("F2") + " m";
    EditTrack();
  }

  public void setWidth(float _width)
  {
    width = _width + "";
    widthText.text = "Width: " + (_width).ToString("F2") + " m";
    EditTrack();
  }



  public void CreateTrack(int trackType)
  {

    if (GameObject.Find("Cart").GetComponent<Cart>().paused)
    {
      if (trackCount < 4) {

        Track newTrack = Instantiate(trackPrefab, Vector3.zero + Vector3.right * Random.Range(-1f,1f) * 3f, Quaternion.identity).GetComponent<Track>();
        trackCount++;
        selected = newTrack;
        Track.editPanel.SetActive(true);
        width = "2";
        height = "2";
        Track.heightSlider.value = 2;
        Track.widthSlider.value = 2;
        heightText.text = "Height: 2.00 m";
        widthText.text = "Width: 2.00 m";
        newTrack.setProperties(2, 2, (TrackType)trackType);
        StartCoroutine(WaitAndUpdate());
      } else {
        Error("Too many tracks");
      }

    } else {
      Error("Pause to create track");
    }
  }

  IEnumerator WaitAndUpdate() {
    yield return new WaitForSecondsRealtime(0.01f);
    UpdateTracks();
  }

  public void EditTrack()
  {
    if (selected != null)
    {
      // height = GameObject.Find("Height Input").GetComponent<Slider>().value.ToString("F2");
      // width = GameObject.Find("Width Input").GetComponent<Slider>().value.ToString("F2");
      // type = GameObject.Find("Type").GetComponent<Dropdown>().value;

      float w = float.Parse(width);
      float h = float.Parse(height);

      if (w > 0 && h > 0)
      {

        selected.setProperties(w, h, selected.type);
        selected.Update();
        UpdateTracks();
      }
    }
  }

  public void AddFlag()
  {
    selectedFlag = Instantiate(flagPrefab, new Vector3(Camera.main.transform.position.x + Random.Range(-1f,1f) * 3f, 3 + Random.Range(-1f,1f), -3), Quaternion.identity);
    StartCoroutine(InstantiateFlag(selectedFlag));
  }

  IEnumerator InstantiateFlag(GameObject newFlag) {
    yield return new WaitForSecondsRealtime(0.01f);
    newFlag.GetComponent<Flag>().Create();
  }

  public void Delete()
  {
    if (selected != null)
    {
      selected.Delete();
      // GameObject.Destroy(selected.gameObject);
      UpdateTracks();

    }

  }

  public void DeleteFlag() 
  {
    if (selectedFlag != null) {
      selectedFlag.GetComponent<Flag>().Delete();
    }
  }

  // void OnDrawGizmos()
  // {
  //   // Vector3[] closestPoints = GetClosestPoints(transform.position);
  //   Gizmos.color = Color.red;
  //   Gizmos.DrawSphere(closestPoints[0], 0.05f);
  //   Gizmos.color = Color.white;
  //   Gizmos.DrawSphere(closestPoints[1], 0.05f);
  //   Gizmos.color = Color.yellow;
  //   Gizmos.DrawSphere(closestPoints[2], 0.05f);
  // }


  public Vector3[] GetClosestPoints(Vector3 position)
  {
    float shortestDist = float.PositiveInfinity;
    int closest = 0;

    for (int i = 0; i < trackPoints.Count; i++)
    {
      float sqDist = Vector3.SqrMagnitude(position - trackPoints[i]);
      // Debug.Log("position");
      // Debug.Log(position);
      //       Debug.Log("trackPoints["+i+"]");
      // Debug.Log(trackPoints[i]);
      if (sqDist < shortestDist)
      {
        shortestDist = sqDist;
        closest = i;
      }
    }

    // lastCached = Time.frameCount;

    Vector3[] closestPoints = new Vector3[3];
    closestPoints[1] = trackPoints[closest];

    if (closest > 0)
      closestPoints[0] = trackPoints[closest - 1];
    else
      closestPoints[0] = trackPoints[closest];

    if (closest < trackPoints.Count - 1)
      closestPoints[2] = trackPoints[closest + 1];
    else
      closestPoints[2] = trackPoints[closest];
    this.closestPoints = closestPoints;

    return closestPoints;
  }

  public float GetClosestPointSlope(Vector3 position)
  {
    Vector3[] closest;

    // if (Time.frameCount == lastCached)
    // {
    //   closest = closestPoint;
    // }
    // else
    //{
    closest = GetClosestPoints(position);
    //}

    Vector3 delta = closest[2] - closest[1];
    return Mathf.Atan2(delta.y, delta.x);
  }

  void Start()
  {
    trackCount = GameObject.FindGameObjectsWithTag("Track").Length;
    lineRenderer = GetComponent<LineRenderer>();

    heightText = Track.heightText;
    widthText = Track.widthText;
    _instance = this;
    UpdateTracks();

  }

  public static TrackManager GetInstance()
  {
    return _instance;
  }

  public static void UpdateTracks()
  {
    if (trackCount > 0)
    {
      // FillGaps();
      GetAllPoints();
      // Debug.Log(GetInstance());
      // Debug.Log("Reset points");
      GetInstance().lineRenderer.positionCount = 0;
      // Debug.Log(GetInstance().trackPoints.Count);
      GetInstance().lineRenderer.positionCount = GetInstance().trackPoints.Count;
      GetInstance().lineRenderer.SetPositions(GetInstance().trackPoints.ToArray());

      GameObject.Find("Cart").GetComponent<Cart>().RestartSim();
    } else {
      GameObject.Find("Cart").GetComponent<Cart>().Hide();
    }
  }

  public static void FillGaps()
  {
    // Get rid of all previously generated connectors
    GameObject[] oldConnectors = GameObject.FindGameObjectsWithTag("Connector");

    for (var i = 0; i < oldConnectors.Length; i++)
    {
      oldConnectors[i].GetComponent<Track>().Delete();
    }

    // Find all tracks in scene
    List<GameObject> tracks = new List<GameObject>(GameObject.FindGameObjectsWithTag("Track"));
    tracks.Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));



    for (int i = 1; i < tracks.Count; i++)
    {
      Vector3 head = tracks[i].GetComponent<LineRenderer>().GetPosition(0);
      LineRenderer tailTrack = tracks[i - 1].GetComponent<LineRenderer>();
      Vector3 tail = tailTrack.GetPosition(tailTrack.positionCount - 1);

      Vector3 midPoint = (head.x + tail.x) / 2 * Vector3.right;
      float width = head.x - tail.x;
      float height = head.y - tail.y;

      TrackType trackType = TrackType.up;

      // check if / or \ connector is needed
      if (height < 0)
      {
        height = -height;
        trackType = TrackType.down;
      }

      Track newTrack = Instantiate(GetInstance().connectorPrefab, midPoint, Quaternion.identity).GetComponent<Track>();

      newTrack.setProperties(width - 0.1f, height, trackType);
    }
  }

  // Get all track points in scene and add to trackPoints
  public static void GetAllPoints()
  {
    // Find all tracks in scene
    List<GameObject> tracks = new List<GameObject>(GameObject.FindGameObjectsWithTag("Track"));
    tracks.AddRange(new List<GameObject>(GameObject.FindGameObjectsWithTag("Connector")));
    tracks.Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));

    GetInstance().trackPoints = new List<Vector3>();


    errCount = 0;
    bool coincide = false;
    bool tooFar = false;
    // For each track add points to trackPoints
    for (int i = 0; i < tracks.Count; i++)
    {
      if (i > 0)
      {
        Vector3 head = tracks[i].GetComponent<LineRenderer>().GetPosition(0);
        LineRenderer tailTrack = tracks[i - 1].GetComponent<LineRenderer>();
        Vector3 tail = tailTrack.GetPosition(tailTrack.positionCount - 1);

        if (head.x <= tail.x)
        {
          // Error("Tracks cannot coincide");
          coincide = true;
          GetInstance().trackPoints = new List<Vector3>();
          GameObject.Find("Cart").GetComponent<Cart>().Hide();
          coincide = true;
          errCount++;
          break;
        }

        if (Vector3.SqrMagnitude(head - tail) > 0.1f)
        {
          // Error("Tracks are too far apart");
          tooFar = true;
          GetInstance().trackPoints = new List<Vector3>();
          GameObject.Find("Cart").GetComponent<Cart>().Hide();
          errCount++;
          break;
        }
      }


      LineRenderer trackRenderer = tracks[i].GetComponent<LineRenderer>();
      Vector3[] positions = new Vector3[trackRenderer.positionCount];
      trackRenderer.GetPositions(positions);

      GetInstance().trackPoints.AddRange(positions);
      GameObject.Find("Cart").GetComponent<Cart>().Show();


    }

    if (errCount <= 0)
    {
      HideError();
    }
    else if (coincide || errCount > 1)
    {
      Error("Tracks are too close", 0);
    }
    else if (tooFar)
    {
      Error("Tracks are too far apart", 0);
    }


    List<Vector3> points = GetInstance().trackPoints;
    if (points.Count > 0)
    {
      Vector3 end = points[points.Count - 1];

      for (int i = 1; i < 30; i++)
      {
        GetInstance().trackPoints.Add(end + new Vector3(i / 20f, 0, 0));
      }
    }
  }

  public static void Error(string err, float duration = 5f)
  {
    GameObject panel = GameObject.Find("ErrorPanel");
    panel.GetComponent<Image>().enabled = true;
    panel.transform.GetChild(0).GetComponent<Text>().text = err;
    _instance.StartCoroutine(ShowError(err, duration));
  }

  public static IEnumerator ShowError (string err, float duration) {
    yield return new WaitForSecondsRealtime(duration);
    if (duration > float.Epsilon)
    HideError();
  }
  public static void HideError()
  {
    GameObject panel = GameObject.Find("ErrorPanel");
    panel.GetComponent<Image>().enabled = false;
    panel.transform.GetChild(0).GetComponent<Text>().text = "";
  }
}