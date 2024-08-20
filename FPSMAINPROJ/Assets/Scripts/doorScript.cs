using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorScript : MonoBehaviour
{

    public void slide()
    {
        StartCoroutine(slideDoor(this.transform));
    }
    IEnumerator slideDoor(Transform transform)
    {
            //get door pos
            Vector3 doorPos = transform.position;
            //get desired endPos
            Vector3 endPos = doorPos - new Vector3(0, 4, 0);
            //speed to slide
            float slidespeed = 1f;
            //time it takes
            float timeToOpen = 0f;

            while (timeToOpen < slidespeed)
            {
                //lerp over the positions smoothly
                transform.position = Vector3.Lerp(doorPos, endPos, (timeToOpen / slidespeed));
                timeToOpen += Time.deltaTime;
                yield return null;
            }
        transform.position = endPos;
    }
}