using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeGeneration : MonoBehaviour
{
    public bool premade;

    public enum DungeonBuild { square, circle };

    public GameObject roomPrefab;
    public GameObject corridorHPrefab;
    public GameObject corridorVPrefab;
    private List<GameObject> possiblePaths = new List<GameObject>();
    private GameObject chosenPath;
    private GameObject startPoint;
    private GameObject endPoint;

    private List<GameObject> dungeonRooms = new List<GameObject>();
    private List<GameObject> triggeredRooms = new List<GameObject>();
    private List<GameObject> dungeonCorridors = new List<GameObject>();

    public int length;
    public int height;

    public Material red;
    public Material yellow;
    public Material blue;
    public Material green;

    // Use this for initialization
    void Start ()
    {
        BuildDungeon();
        //AddDungeonDetails();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(0);
        }
    }

    //Makes a basic maze
    void BuildDungeon()
    {
        if (!premade)
        {
            CreateBase();
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.tag == "Room")
            {
                dungeonRooms.Add(transform.GetChild(i).gameObject);
            }
            else if (transform.GetChild(i).gameObject.tag == "Corridor")
            {
                dungeonCorridors.Add(transform.GetChild(i).gameObject);
            }
        }

        for (int i = 0; i <= dungeonRooms.Count; i++)
        {
            if (i == 0)
            {
                ChooseStartPoint();
            }
            else if (i < dungeonRooms.Count && i > 0)
            {
                FindPossiblePaths();
            }
            else if (i == dungeonRooms.Count)
            {
                CleanupDungeon();
            }
        }
    }

    void CreateBase()
    {
        //Places Room Locations
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject room = Instantiate(roomPrefab,gameObject.transform);
                room.transform.position = new Vector3(0 + (10 * i), 0, 0 - (10 * j));
            }
        }

        //Places Horizontal Corridors
        for (int i = 0; i < length - 1; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject room = Instantiate(corridorHPrefab, gameObject.transform);
                room.transform.position = new Vector3(5 + (10 * i), 0, 0 - (10 * j));
            }
        }
        //Places Vertical Corridors
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < height -1; j++)
            {
                GameObject room = Instantiate(corridorVPrefab, gameObject.transform);
                room.transform.position = new Vector3(0 + (10 * i), 0, -5 - (10 * j));
            }
        }
    }

    void ChooseStartPoint()
    {
        int r = Random.Range(0, dungeonRooms.Count);
        triggeredRooms.Add(dungeonRooms[r]);
        startPoint = dungeonRooms[r];
    }

    void FindPossiblePaths()
    {
        possiblePaths.Clear();

        foreach (GameObject g in triggeredRooms)
        {
            foreach (Collider c in Physics.OverlapSphere(g.transform.position,5))
            {
                if (c.gameObject.tag == "Corridor")
                {
                    if (c.gameObject.GetComponent<CorridorData>().triggered == false)
                    {
                        if (possiblePaths.Contains(c.gameObject) == false)
                        {
                            possiblePaths.Add(c.gameObject);
                        }
                    }
                }
            }
        }
        FindShortestPath();
    }

    void FindShortestPath()
    {
        chosenPath = null;

        foreach (GameObject g in possiblePaths)
        {
            if (chosenPath == null)
            {
                chosenPath = g;
            }
            else if (g.GetComponent<CorridorData>().value < chosenPath.GetComponent<CorridorData>().value)
            {
                chosenPath = g;
            }
            else if (g.GetComponent<CorridorData>().value == chosenPath.GetComponent<CorridorData>().value)
            {
                int r = Random.Range(0, 2);
                if (r==1)
                {
                    chosenPath = g;
                }
            }
        }
        DeterminePathViability();
    }

    void DeterminePathViability()
    {
        GameObject unusedRoom = null;

        foreach (Collider c in Physics.OverlapSphere(chosenPath.transform.position,5))
        {
            if (c.gameObject.tag == "Room")
            {
                if (triggeredRooms.Contains(c.gameObject) == false)
                {
                    unusedRoom = c.gameObject;
                }
            }
        }

        if (unusedRoom == null)
        {
            chosenPath.SetActive(false);
            FindPossiblePaths();
        }
        else
        {
            chosenPath.GetComponent<CorridorData>().triggered = true;
            triggeredRooms.Add(unusedRoom);

            if (triggeredRooms.Count == dungeonRooms.Count)
            {
                endPoint = unusedRoom;
            }
        }
    }

    void CleanupDungeon()
    {
        foreach (GameObject g in dungeonCorridors)
        {
            if (g.GetComponent<CorridorData>().triggered == false)
            {
                g.SetActive(false);
            }
        }
    }

    //Gives some of the rooms in the dungeon set functions
    void AddDungeonDetails()
    {
        //AddStartAndEnd();

        AddItemRooms();
    }

    void AddStartAndEnd()
    {
        GameObject start = GameObject.CreatePrimitive(PrimitiveType.Cube);
        start.transform.position = startPoint.transform.position;
        start.transform.parent = transform;
        start.GetComponent<Renderer>().material = blue;
        start.transform.localScale = new Vector3(2, 2, 2);
        startPoint.GetComponent<RoomData>().used = true;

        GameObject end = GameObject.CreatePrimitive(PrimitiveType.Cube);
        end.transform.position = endPoint.transform.position;
        end.transform.parent = transform;
        end.GetComponent<Renderer>().material = green;
        end.transform.localScale = new Vector3(2, 2, 2);
        endPoint.GetComponent<RoomData>().used = true;
    }

    void AddItemRooms()
    {
        foreach (GameObject g in dungeonRooms)
        {
            if (g.GetComponent<RoomData>().used == false)
            {
                List<GameObject> nearbyCorridors = new List<GameObject>();
                foreach (Collider c in Physics.OverlapSphere(g.transform.position, 5))
                {
                    if (c.gameObject.tag == "Corridor")
                    {
                        nearbyCorridors.Add(c.gameObject);
                    }
                }

                if (nearbyCorridors.Count == 1)
                {
                    GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    item.transform.position = g.transform.position;
                    item.transform.parent = transform;
                    item.GetComponent<Renderer>().material = yellow;
                    item.transform.localScale = new Vector3(2, 2, 2);
                }
            }
        }
    }
}
