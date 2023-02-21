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
    heightText.text = "Height: " + _height.ToString("F2") + " m";
    EditTrack();
  }

  public void setWidth(float _width)
  {
    width = _width + "";
    widthText.text = "Width: " + _width.ToString("F2") + " m";
    EditTrack();
  }

  public void CreateTrack()
  {
    if (GameObject.Find("Cart").GetComponent<Cart>().paused)
    {
      float w = float.Parse(width);
      float h = float.Parse(height);

      if (w > 0 && h > 0)
      {
        Track newTrack = Instantiate(trackPrefab, Vector3.zero, Quaternion.identity).GetComponent<Track>();
        newTrack.setProperties(w, h, (TrackType)type);
        UpdateTracks();

      }
    }
  }

  public void EditTrack()
  {
    if (selected != null)
    {
      float w = float.Parse(width);
      float h = float.Parse(height);

      if (w > 0 && h > 0)
      {

        selected.setProperties(w, h, (TrackType)type);
        selected.Update();
        UpdateTracks();
      }
    }
  }

  public void AddFlag()
  {
    Instantiate(flagPrefab, new Vector3(Camera.main.transform.position.x, 1, -3), Quaternion.identity);
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
    lineRenderer = GetComponent<LineRenderer>();
    heightText = GameObject.Find("HText").GetComponent<Text>();
    widthText = GameObject.Find("WText").GetComponent<Text>();
    _instance = this;
    UpdateTracks();

  }

  public static TrackManager GetInstance()
  {
    return _instance;
  }

  public static void UpdateTracks()
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
    Debug.Log(tracks.Count);


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


    // For each track add points to trackPoints
    for (int i = 0; i < tracks.Count; i++)
    {
      if (i > 0)
      {
        Vector3 head = tracks[i].GetComponent<LineRenderer>().GetPosition(0);
        LineRenderer tailTrack = tracks[i - 1].GetComponent<LineRenderer>();
        Vector3 tail = tailTrack.GetPosition(tailTrack.positionCount - 1);

        if (Vector3.SqrMagnitude(head - tail) > 0.1f)
        {
          // Debug.LogError("Tracks too far apart");
          GetInstance().trackPoints = new List<Vector3>();
          GameObject.Find("Cart").GetComponent<Cart>().Hide();
          break;
        }

        if (head.x <= tail.x)
        {
          // Debug.LogError("Tracks cannot coincide");
          GetInstance().trackPoints = new List<Vector3>();
          GameObject.Find("Cart").GetComponent<Cart>().Hide();
          break;
        }
      }


      LineRenderer trackRenderer = tracks[i].GetComponent<LineRenderer>();
      Vector3[] positions = new Vector3[trackRenderer.positionCount];
      trackRenderer.GetPositions(positions);

      GetInstance().trackPoints.AddRange(positions);
      GameObject.Find("Cart").GetComponent<Cart>().Show();


    }


    
    List<Vector3> points = GetInstance().trackPoints;
    if (points.Count > 0)
    {
      Vector3 end = points[points.Count - 1];

      for (int i = 1; i < 20; i++)
      {
        GetInstance().trackPoints.Add(end + new Vector3(i/20f, 0, 0));
      }
    }
  }
}
