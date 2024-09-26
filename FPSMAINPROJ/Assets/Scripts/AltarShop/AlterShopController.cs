using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class AlterShopController : MonoBehaviour
{
    [SerializeField] Transform armStartPoint;
    
    [SerializeField] GameObject emptyArm;
    [SerializeField] GameObject tempestArm;
    [SerializeField] GameObject darknessArm;
    [SerializeField] GameObject floodsArm;
    [SerializeField] float changeWaitTime;

    public GameObject activeArm;
    GameObject activeArmInstance;

    
    bool changingArm;
    public bool weaponArmActive = false;
    public bool pickedWeaponUp = false;
    void Start()
    {
        activeArm = Instantiate(emptyArm, armStartPoint.position, emptyArm.transform.rotation);
        activeArm.transform.SetParent(armStartPoint.transform);
        
        weaponArmActive = false;
    }

    private void Update()
    {
        if(!changingArm && pickedWeaponUp)
        {
            StartCoroutine(changeArm(emptyArm));
            pickedWeaponUp = false;
            weaponArmActive = false;
        }
    }

    public IEnumerator changeArm(GameObject armToSpawn)
    {
        
        changingArm = true;

        Vector3 startPos = armStartPoint.localPosition;
        Vector3 lowerPos = startPos + new Vector3(0f, -1.3f, 0);

        float swapTime = changeWaitTime * 0.5f;
        float elapsed = 0f;

        while (elapsed < swapTime)
        {
            armStartPoint.localPosition  = Vector3.Lerp(startPos, lowerPos, elapsed / swapTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        activeArm.transform.localPosition = lowerPos;


        if(armStartPoint.transform.childCount > 0)
        {
            Transform currentArm = armStartPoint.GetChild(0);
            Destroy(currentArm.gameObject);
        }
        
        
        GameObject newArm = Instantiate(armToSpawn, armStartPoint.position, armToSpawn.transform.localRotation);
        newArm.transform.SetParent(armStartPoint.transform);
        activeArm = newArm;
        activeArm.SetActive(true); 

        elapsed = 0f;

        while (elapsed < swapTime)
        {
            armStartPoint.localPosition = Vector3.Lerp(armStartPoint.localPosition, startPos, elapsed / swapTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        changingArm = false;
    }




    public void hekaTempest()
    {
        if (gameManager.gameInstance.playerScript.inventory.gemCount() >= 35 && !gameManager.gameInstance.playerWeapon.hasTempest && !changingArm && !weaponArmActive) 
        {
            
            activeArm = tempestArm;
            StartCoroutine(changeArm(tempestArm));
            gameManager.gameInstance.playerScript.inventory.takeGems(35);
            gameManager.gameInstance.PointCount -= 35;
            weaponArmActive = true;
            
        }
        else if (gameManager.gameInstance.playerScript.inventory.gemCount() < 35)
        {
            StartCoroutine(FlashNoGems());
        }
        else
        {
            //UI for arm already active. 
        }


    }

    public void pharoahsEclipse()
    {
        if (gameManager.gameInstance.playerScript.inventory.gemCount() >= 25 && !gameManager.gameInstance.playerWeapon.hasEclipse && !changingArm && !weaponArmActive) 
        {
            //if player has gems amount
            if (!gameManager.gameInstance.playerWeapon.hasEclipse)
            {
                
                activeArm = darknessArm;
                StartCoroutine(changeArm(darknessArm));
                gameManager.gameInstance.playerScript.inventory.takeGems(20);
                gameManager.gameInstance.PointCount -= 20;
                weaponArmActive = true;
            }
        }
        else if (gameManager.gameInstance.playerScript.inventory.gemCount() < 20)
        {
            StartCoroutine(FlashNoGems());
        }
        else
        {
            //UI for arm already active. 
        }

    }

    public void nilesWrath()
    {
        if (gameManager.gameInstance.playerScript.inventory.gemCount() >= 25 && !gameManager.gameInstance.playerWeapon.hasFloods && !changingArm && !weaponArmActive) 
        {
            //if player has gems amount
            if (!gameManager.gameInstance.playerWeapon.hasFloods)
            {
                
                activeArm = floodsArm;
                StartCoroutine(changeArm(floodsArm));
                gameManager.gameInstance.playerScript.inventory.takeGems(25);
                gameManager.gameInstance.PointCount -= 25;
                weaponArmActive = true;
            }
        }
        else if (gameManager.gameInstance.playerScript.inventory.gemCount() < 25)
        {
            StartCoroutine(FlashNoGems());
        }
        else
        {
            //UI for arm already active. 
        }
    }

    public void healPlayer()
    {
        if (gameManager.gameInstance.playerScript.inventory.gemCount() >= 5 && gameManager.gameInstance.playerScript.playerHP != gameManager.gameInstance.playerScript.HPorig) 
        {
            float healthAmount = 25;

            float currentHP = gameManager.gameInstance.playerScript.playerHP;
            float maxHp = gameManager.gameInstance.playerScript.HPorig;

            if (maxHp - currentHP < healthAmount)
            {

                healthAmount = maxHp - currentHP;
            }

            gameManager.gameInstance.playerScript.recieveHP(healthAmount);
            gameManager.gameInstance.playerScript.inventory.takeGems(5);
            gameManager.gameInstance.PointCount -= 5;
        }
        else
            StartCoroutine(FlashNoGems());
    }

    IEnumerator FlashNoGems()
    {
        gameManager.gameInstance.NoGems.GameObject().SetActive(true);
        yield return new WaitForSeconds(0.5f);
        gameManager.gameInstance.NoGems.GameObject().SetActive(false);
    }
}
