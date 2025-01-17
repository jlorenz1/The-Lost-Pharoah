using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MemoryPuzzleController : MonoBehaviour
{
    public GameObject door;
    public GameObject placingDisplaysBit; //used for placing display tiles
    [SerializeField] List<GameObject> randLocations;
    bool fail, pass;

    //display tiles
    public GameObject display1;
    public GameObject display2;
    public GameObject display3;
    public GameObject display4;
    public GameObject display5;
    public GameObject display6;
    public GameObject display7;
    public GameObject display10;
    public GameObject display11;
    public GameObject display14;
    public GameObject display15;
    public GameObject display17;
    public GameObject display18;
    public GameObject display20;

    //pattern options for sequence player needs to complete
    int[] pattern1 = { 3, 6, 11, 15, 20 };
    int[] pattern2 = { 2, 5, 10, 15, 18 };
    int[] pattern3 = { 4, 7, 11, 14, 17 };
    int[] pattern4 = { 1, 5, 10, 15, 18 };

    int[] pattern; //chosen pattern

    ArrayList sequence = new ArrayList(); //the sequence the player steps in
    GameObject[] correctTitles = new GameObject[5];

    private static MemoryPuzzleController _memPuzzleInstance;
    public static MemoryPuzzleController memPuzzleInstance
    {
        get
        {
            if (_memPuzzleInstance == null)
            {
                //do nothing
            }
            return _memPuzzleInstance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (_memPuzzleInstance != null && _memPuzzleInstance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _memPuzzleInstance = this;
        }

        //randomly choosing which sequence is required this run
        int randVal = Random.Range(0, 4);
        if (randVal == 0)
        {
            pattern = pattern1;
        } else if (randVal == 1)
        {
            pattern = pattern2;
        } else if (randVal == 2)
        {
            pattern = pattern3;
        } else if (randVal == 3)
        {
            pattern = pattern4;
        }

        //displaying the sequence tiles
        //placingDisplaysBit is in there to make it easier to traverse the array
        GameObject[] displayTiles = {display1,  display2, display3, display4, display5, display6, display7, 
        placingDisplaysBit, placingDisplaysBit, display10, display11, placingDisplaysBit, placingDisplaysBit, 
        display14, display15, placingDisplaysBit, display17, display18, placingDisplaysBit, display20};

        
        int minRange = 0; //minimum range for randomly generated range
        for (int i = 0; i < 5; ++i)
        {
            //randomly select tiles, must be in ascending order
            GameObject location = randLocations[Random.Range(minRange, minRange + 2)]; //chooses between two tiles

            minRange += 2; //to move onto next set of tiles

            displayTiles[pattern[i] - 1].transform.SetPositionAndRotation(location.transform.position, location.transform.rotation);
        }


    }

    //updates sequence array and handles when player steps on incorrect tile
    public void UpdateSequence(int id, GameObject title)
    {
        int index = sequence.Count;

        //checking if player stepped on the same, correct, tile twice
        if (index > 0 && pattern[index-1] == id)
        {
            //do nothing, this way the tile isnt registered as wrong but does not add to sequence array
            Debug.Log("Tile ID: " + id + ". Previous Tile ID: " + pattern[index - 1] + ".");
        } 
        else if (pattern[index] == id) //player stepped on correct tile
        {
            fail = false;
            float newPos = title.transform.position.y - .1f;
            //lowering the titles
            title.transform.position = new Vector3(title.transform.position.x, newPos, title.transform.position.z);
            correctTitles[index] = title;
            sequence.Add(id);
        } else //player stepped on incorrect tile
        {
            fail = true;

            StartCoroutine(gameManager.gameInstance.requiredItemsUI("Wrong tile. Restart.", 3f));

            //returning all the titles back to the origional height
            if (correctTitles[0] != null)
            {
                for (int i = 0; i < index; i++)
                {
                    correctTitles[i].transform.position = new Vector3(correctTitles[i].transform.position.x, correctTitles[i].transform.position.y + .1f, correctTitles[i].transform.position.z);
                }
            }

            sequence.Clear(); //player must restart
        }


        if (sequence.Count == 5) //sequence is completed, and already checked for accuracy
        {
            pass = true;
            door.GetComponent<doorScript>().slide(); //opening door
        }
    }

    public bool getFailResults()
    {
        return fail;
    }
    public bool getPassResults()
    {
        return pass;
    }

}
