using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public enum InventoryPos //for inventory
{
    Slot1 = 0,
    Slot2 = 1,
    Slot3 = 2,
}
public class PlayerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] Renderer model;

    [Header("Doging")]
    [SerializeField] Collider playerCollider;
    [SerializeField] float dodgeDuration;
    [SerializeField] float dodgeCooldown;
    [SerializeField] float dodgeDistance;
    private bool canDodge = true;

    [Header("PLAYER VARIABLES")]
    [SerializeField] float speed;
    [SerializeField] int sprintMod;
    //[SerializeField] int crouchSpeed;
    float crouchSpeed;
    [SerializeField] float maxSprintTimer;
    [SerializeField] float maxSprintWaitTimer;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;
    [SerializeField] public int playerHP;
    int HPorig;

    // Weapon Variables for player
    [Header("WEAPON VARIABLES")]
    [SerializeField] int shootDamage;
    [SerializeField] int shootRate;
    [SerializeField] int shootDistance;
    [SerializeField] public Transform weaponSpawn;
    public Weapon weapon;

    // climbing video variables
    [Header("Reference")]
    [SerializeField] LayerMask whatToClimb;

    [Header("Climbing")]
    [SerializeField] float climbSpeed;
    [SerializeField] float maxClimbTimer;
    private float climbTimer;

    private bool isClimbing;

    [Header("Dectection")]
    [SerializeField] float detectionLength;
    [SerializeField] float sphereCastRadius;
    [SerializeField] float maxWallLookAngle;
    private float WallLookAngle;

    private RaycastHit wallHit;
    private bool wallFront;
    // End of climbiing video variables


    // Sliding video variables
    [Header("Sliding")]
    [SerializeField] float maxSlideTime;
    [SerializeField] float slideForce;
    float slideTimer;

    public float slideYScale;
    float startingYScale;
    float startingYPOS;
    int controllerHeightOrgi;

    bool isSliding;

    [Header("Input")]
    private KeyCode slideKey = KeyCode.LeftControl;
    float horizontalInput;
    float verticalInput;
    // End of Sliding video variables


    [Header("Interact")]
    [SerializeField] int pickupDis;
    [SerializeField] LayerMask ignoreMask;
    private Ray interactRay;
    private RaycastHit interactHit;
    bool isPickedUp;
    bool inProcess;
    bool doorOpen = false;
    public inventoryObject inventory;
    private float sprintTimer;

    Vector3 move;
    Vector3 playerVel;

    int jumpCount;

    bool isSprinting;
    bool onSprintCoolDown;
    bool isCrouching;

    float originalSpeed;
    public int damage;
    public bool hasItems;
    // Start is called before the first frame update
    void Start()
    {
        HPorig = playerHP;
        damage  = shootDamage;
        sprintTimer = maxSprintTimer;
        originalSpeed = speed;
        crouchSpeed = speed / 2;
        startingYScale = transform.localScale.y;
        controllerHeightOrgi = ((int)controller.height);
        updatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        if (!onSprintCoolDown && !isCrouching)
            sprint();
        sprintTimerUpdate();

        wallCheck();
        stateMachine();
        
        interact();
        useItemFromInv();
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
            climbTimer = maxClimbTimer;

        }
        if (isSliding)
            slideMovement();

        if (isClimbing) 
            climbingMovement();

        move = Input.GetAxis("Vertical") * transform.forward +
            Input.GetAxis("Horizontal") * transform.right;
        controller.Move(move * speed * Time.deltaTime);

        horizontalInput = Input.GetAxis("Vertical");
        verticalInput = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;

        if(Input.GetKeyDown(slideKey) && !isSprinting)
        {
            startCrouch();
        }
        else if (Input.GetKeyUp(slideKey))
        {
            stopCrouch();
        }
        if (Input.GetButtonDown("Dodge") && canDodge)
        {
            UnityEngine.Debug.LogError("Dodge input detected");
            StartCoroutine(PerformDodge());
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint") || sprintTimer <= 0)
        {
            if (sprintTimer <= 0)
                onSprintCoolDown = true;
            //speed /= sprintMod;
            speed = originalSpeed;
            isSprinting = false;
            return;
        }

    }
    void stateMachine()
    {
        if (wallFront && Input.GetKey(KeyCode.W) && WallLookAngle < maxWallLookAngle)
        {
            if (isSliding)
                stopSlide();

            if (!isClimbing && climbTimer > 0) startClimb();

            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) endClimbing();

        }
        else
        {
            if (isClimbing) endClimbing();
        }

        if (isSprinting)
        {
            if (Input.GetKeyDown(slideKey))
            {
                startingYPOS = transform.position.y;
                startSlide();
            }
            if (Input.GetKeyUp(slideKey) && isSliding)
            {
                stopSlide();
            }
        }

    }
    void wallCheck()
    {
        //wallFront tell if there is a wall in front
        //                            ( starting postion,                   Radius,              Directions,    location of the infomation, sphereCast length,  the layerMask)
        wallFront = Physics.SphereCast(Camera.main.transform.position, sphereCastRadius, Camera.main.transform.forward, out wallHit, detectionLength, whatToClimb);
        WallLookAngle = Vector3.Angle(Camera.main.transform.forward, -wallHit.normal);


    }
    void startClimb()
    {
        isClimbing = true;
    }
    void climbingMovement()
    {
        playerVel = new Vector3(playerVel.x, climbSpeed, playerVel.z);

    }

    void endClimbing()
    {
        isClimbing = false;
    }
    void startSlide()
    {
        controller.height = slideYScale;
        model.transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        isSliding = true;
        slideTimer = maxSlideTime;
    }

    void slideMovement()
    {
        Vector3 inputDir = transform.forward * verticalInput + transform.right * horizontalInput;

        speed += slideForce;

        slideTimer -= Time.deltaTime;

        if (slideTimer <= 0)
            stopSlide();

    }

    void stopSlide()
    {
        controller.height = controllerHeightOrgi;
        model.transform.localScale = new Vector3(transform.localScale.x, startingYScale, transform.localScale.z);
        speed = originalSpeed;
        isSliding = false;
    }

    void startCrouch()
    {
        controller.height = slideYScale;
        model.transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        isCrouching = true;
        speed = crouchSpeed;
    }
    void stopCrouch()
    {
        controller.height = controllerHeightOrgi;
        model.transform.localScale = new Vector3(transform.localScale.x, startingYScale, transform.localScale.z);
        isCrouching = false;
        speed = originalSpeed;
    }

    // IDamage Player Damage
    public void takeDamage(int amountOfDamageTaken)
    {
        // Subtract the amount of current damage from player HP
        playerHP -= amountOfDamageTaken;
        StartCoroutine(damageFeedback());
        updatePlayerUI();
        if(playerHP <= 0)
        {
            gameManager.gameInstance.loseScreen();
        }
        
    }
    public void recieveHP(int amount)
    {
        playerHP += amount;
    }

    IEnumerator damageFeedback()
    {
        gameManager.gameInstance.flashDamage.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.gameInstance.flashDamage.SetActive(false);
    }
    void sprintTimerUpdate()
    {
        if (isSprinting)
        {
            sprintTimer -= Time.deltaTime;
        }

        if (onSprintCoolDown && Input.GetButtonUp("Sprint"))
        {
            StartCoroutine(waitTimer());
        }

        if(onSprintCoolDown && Input.GetButtonDown("Sprint"))
        {
            StopAllCoroutines();
            //StopCoroutine(waitTimer());
        }

    }

    IEnumerator waitTimer()
    {
        yield return new WaitForSeconds(maxSprintWaitTimer);
        sprintTimer = maxSprintTimer;
        onSprintCoolDown = false;
    }

    public void updatePlayerUI()
    {
        gameManager.gameInstance.playerHPBar.fillAmount = (float)playerHP / HPorig;
    }

    public void interact()
    {
        RaycastHit hit;

        if(Physics.Raycast(gameManager.gameInstance.MainCam.transform.position, gameManager.gameInstance.MainCam.transform.forward, out hit, pickupDis, ~ignoreMask))
        {
            if (hit.collider.gameObject.CompareTag("pickup"))
            {
                gameManager.gameInstance.playerInteract.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(startInteract());
                }
            }
            else
            {
                gameManager.gameInstance.playerInteract.SetActive(false);
            }
        }
    }

    public IEnumerator startInteract()
    {
        RaycastHit hit;

        if (Physics.Raycast(gameManager.gameInstance.MainCam.transform.position, gameManager.gameInstance.MainCam.transform.forward, out hit, pickupDis, ~ignoreMask))
        {
            pickup pickup = hit.collider.GetComponent<pickup>();
            if (pickup != null)
            {
                inventory.AddItem(pickup.item, 1);
                Destroy(hit.collider.gameObject);
            }
            if (pickup.item.type == itemType.Default || pickup.item.type == itemType.Rune)
            {
                checkForRequiredItems();
            }
        }
        yield return new WaitForSeconds(1);
        gameManager.gameInstance.playerInteract.SetActive(false);
    }

    public void useItemFromInv()
    {
        RaycastHit hit;

        if(Physics.Raycast(gameManager.gameInstance.MainCam.transform.position, gameManager.gameInstance.MainCam.transform.forward, out hit, pickupDis, ~ignoreMask))
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                //using heal
                inventory.useItem(itemType.Bandage);
            }
            else if(Input.GetKeyDown(KeyCode.E))
            {
                if (hit.collider != null)
                {
                    var door = hit.collider.GetComponent<doorScript>();
                    if (door != null)
                    {
                        inventory.useItem(itemType.Key);
                    }
                    else
                    {
                        var bossInteraction = hit.collider.GetComponent<bossInteraction>();
                        if (bossInteraction != null)
                        {
                            bossInteraction.spawnBoss();
                            Destroy(hit.collider);
                        }
                        else
                        {
                            UnityEngine.Debug.Log("not spawning");
                        }
                    }
                }
            }
        }
    }

    public void checkForRequiredItems()
    {
        bool hasRunes = false;
        bool hasLighter = false;

        for (int i = 0; i < inventory.containerForInv.Count; i++)
        {
            if (inventory.containerForInv[i].pickup.type == itemType.Rune)
            {
                

                if (inventory.containerForInv[i].amount >= 3)
                {
                    hasRunes = true;
                    StartCoroutine(gameManager.gameInstance.requiredItemsUI(("Collected all Runes!"), 2f));
                    if(hasLighter)
                    {
                        StartCoroutine(gameManager.gameInstance.requiredItemsUI(("Collected all Runes and Lighter, Find the ritual sight to spawn the boss!" ), 6f));

                    }
                    UnityEngine.Debug.Log(hasRunes);
                }
                else
                {
                    UnityEngine.Debug.Log("Not enough runes");

                    StartCoroutine(gameManager.gameInstance.requiredItemsUI((inventory.containerForInv[i].amount.ToString() + " runes in Inventory"), 2f));
                    //set UI active coroutine 
                }
            }
            else if (inventory.containerForInv[i].pickup.type == itemType.Default)
            {
           
                if (inventory.containerForInv[i].amount >= 1)
                {
                    hasLighter = true;
                    StartCoroutine(gameManager.gameInstance.requiredItemsUI(("Collected Lighter!"), 2f));
                    if(hasRunes)
                    {
                        StartCoroutine(gameManager.gameInstance.requiredItemsUI(("Collected Lighter and have " + inventory.containerForInv[i].amount.ToString()) + " Runes!", 2f));
                    }
                    UnityEngine.Debug.Log(hasLighter);
                }
                else
                {
                    StartCoroutine(gameManager.gameInstance.requiredItemsUI(("Doesnt have lighter!"), 2f));
                    UnityEngine.Debug.Log("Doesnt have lighter");
                    //set UI active coroutine 
                }
            }
        }
        if (hasRunes && hasLighter)
        {
            UnityEngine.Debug.Log("can now spawn boss");
            hasItems = true;
           
        }
        else
        {
            UnityEngine.Debug.Log("required items missing");

            hasItems = false;
        }
    }

    public void OnApplicationQuit()
    {
        //set inventory back to the original starting loadout.
        for(int i = 0; i < inventory.containerForInv.Count; i++)
        {
            inventory.containerForInv.Clear();
        }
            
    }
    // Things Added by Jamauri 
    public void SetSpeed(float Modifier)
    {
        speed = Modifier;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public void SetJumpCount(int Modifier)
    {
        jumpMax = Modifier;
    }

    public int GetJumpCount()
    {
        return jumpMax;
    }


    private IEnumerator PerformDodge()
    {
        // Start the dodge
        canDodge = false;

        // Disable the collider
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // Shift left
        Vector3 dodgeDirection = transform.right * -dodgeDistance;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + dodgeDirection;

        float elapsedTime = 0f;
        while (elapsedTime < dodgeDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / dodgeDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is exactly the target
        transform.position = targetPosition;

        // Re-enable the collider
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        // Wait for the cooldown duration
        yield return new WaitForSeconds(dodgeCooldown);

        // End the dodge cooldown
        canDodge = true;
    }
}
