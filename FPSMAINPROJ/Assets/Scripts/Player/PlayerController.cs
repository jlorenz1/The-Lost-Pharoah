
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
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
    private bool invincible = false;

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
    [SerializeField] public float playerHP;
    [SerializeField] Light flashLight;
    private bool isLit = false;

    public float HPorig;
    float Sprintorig;

    // Weapon Variables for player
    [Header("WEAPON VARIABLES")]
    [SerializeField] public List<weaponStats> gunList = new List<weaponStats>();
    [SerializeFeild] public GameObject gunModel;
    [SerializeField] public GameObject muzzleFlash;
    [SerializeField] public GameObject casingEffect;
    [SerializeField] public Transform muzzleFlashTransform;
    [SerializeField] public Transform casingSpawnTransform;
    [SerializeField] float shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDistance;
    [SerializeField] LayerMask canBeShotMask;
    [SerializeField] Transform gunTransform;
    bool isReloading;
    private weaponStats currGun;
  
    //public Weapon weapon;
    //private float lastShotTime;
    //private bool weaponCanShoot;


    [SerializeField] Collider meleeWeapon;

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
    float meeleDuration;
    bool isSliding;

    [Header("Input")]
    private KeyCode slideKey = KeyCode.LeftControl;
    float horizontalInput;
    float verticalInput;
    private float sprintTimer;


    public inventoryObject inventory;


    [Header("Sounds")]
    public AudioClip[] interactSounds;
    [Range(0, 1)][SerializeField] public float interactVol;
    public AudioClip[] flashlightSounds;
    [Range(0, 1)][SerializeField] public float flashlightVol;
    public AudioClip[] jumpSounds;
    [Range(0, 1)][SerializeField] public float jumpVol;
    public AudioClip[] stepSounds;
    [Range(0, 1)][SerializeField] public float stepVol;
    public AudioClip[] slideSounds;
    [Range(0, 1)][SerializeField] public float slideVol;
    public AudioClip[] hurtSounds;
    [Range(0, 1)][SerializeField] public float hurtVol;

    // Weapon Vars
    //public Weapon primaryWeapon;
    //public Weapon secondaryWeapon;
    //public Weapon activeWeapon;

    Vector3 move;
    Vector3 playerVel;

    int jumpCount;
    int selectedGun;
    public bool isSprinting;
    public bool isShooting;
    bool onSprintCoolDown;
    bool isCrouching;
    bool canMelee;
    float originalSpeed;
    public float damage;
    public bool hasItems;
    bool isPlayingSound;
    private Coroutine waitTime;
    // Start is called before the first frame update
    void Start()
    {
        HPorig = playerHP;
        damage = shootDamage;
        sprintTimer = maxSprintTimer;
        originalSpeed = speed;
        crouchSpeed = speed / 3;
        startingYScale = transform.localScale.y;
        controllerHeightOrgi = ((int)controller.height);
        updatePlayerUI();
        meeleDuration = 2;
        canMelee = true;
        flashLight.gameObject.SetActive(false);

        //lastShotTime = -shootRate; // Allows immediate shooting
        //weaponCanShoot = true;
        //activeWeapon = primaryWeapon;
        //EquipWeapon(activeWeapon);
    }



    // Update is called once per frame
    void Update()
    {
       
        if (!onSprintCoolDown && !isCrouching)
            sprint();
        sprintTimerUpdate();

        wallCheck();
        stateMachine();

        if(!gameManager.gameInstance.gameIsPaused)
        {
            movement();
            selectGun();
        }

        if (currGun != null)
        {

            if (Input.GetKeyDown(KeyCode.R) && !currGun.magazines[currGun.currentMagazineIndex].weaponFull())
            {
                if (!isReloading)
                {
                    isReloading = true;
                    StartCoroutine(reload());
                }
            }
        }
        if (gunList.Count >= 1)
        {
            displayAmmo();
        }
    }

    IEnumerator shoot()
    {

        if (selectedGun >= 0 && selectedGun < gunList.Count)
        {
            if (gunList[selectedGun].magazines[gunList[selectedGun].currentMagazineIndex].currentAmmoCount >= 1 && !isReloading)
            {
                UnityEngine.Debug.Log(gunList[selectedGun].magazines[gunList[selectedGun].currentMagazineIndex].currentAmmoCount);
                gunList[selectedGun].magazines[gunList[selectedGun].currentMagazineIndex].currentAmmoCount--;
                isShooting = true;
                //StartCoroutine(flashMuzzel());
                AudioManager.audioInstance.playAudio(gunList[selectedGun].shootSound[Random.Range(0, gunList[selectedGun].shootSound.Length)], gunList[selectedGun].shootVol);
                Instantiate(muzzleFlash, muzzleFlashTransform.position, Quaternion.identity);
                Instantiate(casingEffect, casingSpawnTransform.position, casingSpawnTransform.rotation);

                RaycastHit hit;

                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDistance, ~canBeShotMask))
                {
                    IDamage dmg = hit.collider.GetComponent<IDamage>();
                    if (dmg != null)
                    {
                        float actualDamage = shootDamage; // Start with the base damage

                        // Example condition to modify the damage
                        if (hit.collider.CompareTag("Zombie Head"))
                        {
                            actualDamage *= 2.0f; // Double damage for headshots
                        }
                        else if (hit.collider.CompareTag("Zombie Body"))
                        {
                            actualDamage *= 1.0f; // Normal damage for body shots
                        }
                        else if (hit.collider.CompareTag("Zombie Legs"))
                        {
                            actualDamage *= 0.5f; // Reduced damage for leg shots
                        }

                        // Apply the modified damage
                        dmg.takeDamage(actualDamage);
                        Instantiate(gunList[selectedGun].zombieHitEffect, hit.point, Quaternion.identity);
                    }
                    else
                    {
                        Instantiate(gunList[selectedGun].hitEffect, hit.point, Quaternion.identity);
                    }

                }
                yield return new WaitForSeconds(shootRate);

                isShooting = false;
            }
            else
            {
                UnityEngine.Debug.Log("need to reload");

            }
        }
        else
            UnityEngine.Debug.LogError("selectedGun index out of range");



    }

    IEnumerator reload()
    {
        
        //StartCoroutine(fillWhileReloading());
        yield return new WaitForSeconds(currGun.reloadTime);
        //checks if there are mags to reload with
        
        if(currGun.currentMagazineIndex + 1 < currGun.magazines.Length)
        {
            currGun.currentMagazineIndex++;
            currGun.magazines[currGun.currentMagazineIndex].currentAmmoCount = currGun.magazines[currGun.currentMagazineIndex].magazineCapacity;
        }
        else
        {
            UnityEngine.Debug.Log("No more mags!");
        }
        isReloading = false;   
    
    }

    //public IEnumerator fillWhileReloading()
    //{
    //    float elapsedTime = 0f;
    //    float startingFill = gameManager.gameInstance.ammoCircle.fillAmount;
    //    //if (qteSuccess == false)
    //    //{
    //    //    reloadTime = gunStats.reloadAnimation.length;
    //    //}
    //    while (elapsedTime < gunList[selectedGun].reloadTime)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        float fillAmount = Mathf.Lerp(startingFill, 1f, elapsedTime / gunList[selectedGun].reloadTime);
    //        gameManager.gameInstance.ammoCircle.fillAmount = fillAmount;
    //        yield return null;
    //    }
    //    gameManager.gameInstance.ammoCircle.fillAmount = 1f;
    //}

    public void displayAmmo()
    {
        int amountToDisplay = 0;
        gameManager.gameInstance.ammoCount.text = gunList[selectedGun].magazines[gunList[selectedGun].currentMagazineIndex].currentAmmoCount.ToString("F0");
        for(int i = 0; i < gunList[selectedGun].magazines.Length; i++)
        {
            amountToDisplay += gunList[selectedGun].magazines[i].currentAmmoCount;
        }
        UnityEngine.Debug.Log(amountToDisplay);
        gameManager.gameInstance.maxAmmoCount.text = amountToDisplay.ToString("F0");
        //gameManager.gameInstance.ammoCircle.fillAmount = (float)gunList[selectedGun].magazines[gunList[selectedGun].currentMagazineIndex].currentAmmoCount / (float)gunList[selectedGun].magazines[gunList[selectedGun].currentMagazineIndex].magazineCapacity;
    }

    //IEnumerator flashMuzzel()
    //{
    //    muzzleFlash.SetActive(true);
    //    yield return new WaitForSeconds(0.5f);
    //    muzzleFlash.SetActive(false);
    //}

    //void FireWeapon()
    //{
    //    // Check if enough time has passed before last shot
    //    if (Time.time - lastShotTime < shootRate) return;

    //    // Update the last fire time
    //    lastShotTime = Time.time;

    //    ShootRaycastBullet();

    //    StartCoroutine(ResetShootingState(shootRate));
    //}

    //void ShootRaycastBullet()
    //{
    //    Ray reticleRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
    //    RaycastHit hit;

    //    if (Physics.Raycast(reticleRay, out hit, shootDistance, canBeShotMask))
    //    {
    //        // Apply damage
    //        IDamage target = hit.collider.gameObject.GetComponent<IDamage>();
    //        if (target != null)
    //        {
    //            target.takeDamage(shootDamage);
    //        }

    //        // VFX effects


    //        UnityEngine.Debug.Log("Hit " + hit.collider.name + " for " + shootDamage + " damage.");
    //    }

    //    // Trigger shooting system (muzzle flash, sounds, etc)

    //}


    //IEnumerator ResetShootingState(float weaponFireRate)
    //{
    //    yield return new WaitForSeconds(weaponFireRate);
    //    isShooting = false;
    //    weaponCanShoot = true;
    //}

    //void EquipWeapon(Weapon activeWeapon)
    //{

    //    if (activeWeapon == null)
    //    {
    //        UnityEngine.Debug.LogWarning("Weapon to equip is null");
    //        return;
    //    }



    //    if (primaryWeapon != null)
    //        primaryWeapon.gameObject.SetActive(false);

    //    if (secondaryWeapon != null)
    //        secondaryWeapon.gameObject.SetActive(false);

    //    activeWeapon.gameObject.SetActive(true);
    //}

    //void HandleWeaponSwitching()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1) && activeWeapon != primaryWeapon)
    //    {
    //        activeWeapon = primaryWeapon;
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Alpha2) && activeWeapon != secondaryWeapon)
    //    {
    //        activeWeapon = secondaryWeapon;
    //    }

    //    EquipWeapon(activeWeapon);
    //}

    void movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
            climbTimer = maxClimbTimer;

        }
        //if (isSliding)
        //    slideMovement();

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
            AudioManager.audioInstance.playAudio(jumpSounds[Random.Range(0, jumpSounds.Length)], jumpVol);
        }
        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;


        if (Input.GetKeyDown(slideKey) && !isSprinting)
        {
            startCrouch();
        }
        else if (Input.GetKeyUp(slideKey))
        {
            stopCrouch();
        }

        if (controller.isGrounded && move.magnitude > 0.2f && !isPlayingSound)// && !isSliding)
            StartCoroutine(playStep());

        if (Input.GetButtonDown("Dodge") && canDodge)
        {
            UnityEngine.Debug.Log("Dodge input detected");
            StartCoroutine(PerformDodge());
        }


        if (Input.GetButtonDown("Melee") && canMelee)
        {
            UnityEngine.Debug.Log("Melee input detected");
            StartCoroutine(PerformMelee());
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (inventory.hasItem(itemType.flashlight))
            {
                enableFlashLight();
            }
            else
            {
                StartCoroutine(gameManager.gameInstance.requiredItemsUI("Do not have flashlight! Check your campsite for it!", 3.0f));
            }
        }

       
        if (Input.GetButton("Fire1") && gunList.Count > 0 && !isShooting)
        {
            StartCoroutine(shoot());
        }
        
        else
        {
            UnityEngine.Debug.Log("no gun");
        }
     
        //if (Input.GetButtonDown("Reload"))

        //{
        //    activeWeapon.TempReload();
        //}


        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //   activeWeapon.HandleFireModeSwitching();
        //}

        //    //  activeWeapon.HandleFireModeSwitching();
        //    activeWeapon.displayAmmo();


    }

    IEnumerator playStep()
    {
        isPlayingSound = true;

        AudioManager.audioInstance.playAudio(stepSounds[Random.Range(0, stepSounds.Length)], stepVol);

        if (!isSprinting && !isCrouching)
            yield return new WaitForSeconds(0.45f);
        else
            yield return new WaitForSeconds(0.3f);

        if (isCrouching)
            yield return new WaitForSeconds(0.5f);


        isPlayingSound = false;
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
    void sprintTimerUpdate()
    {
        if (isSprinting)
            gameManager.gameInstance.SprintBarBoarder.transform.GameObject().SetActive(true);

        gameManager.gameInstance.playerSprintBar.color = Color.white;
        gameManager.gameInstance.playerSprintBar.fillAmount = sprintTimer / maxSprintTimer;
        if (isSprinting)
        {
            sprintTimer -= Time.deltaTime;
        }

        if (Input.GetButtonUp("Sprint"))
        {
            waitTime = StartCoroutine(waitTimer());
        }
        if (!onSprintCoolDown && Input.GetButtonDown("Sprint"))
        {
            if (waitTime != null)
            {
                StopCoroutine(waitTime);
                waitTime = null;
            }

        }

    }

    IEnumerator waitTimer()
    {
        float startingFill = gameManager.gameInstance.playerSprintBar.fillAmount;
        while (sprintTimer < maxSprintTimer)
        {
            if (!onSprintCoolDown)
                gameManager.gameInstance.playerSprintBar.color = Color.white;
            else
            {
                gameManager.gameInstance.playerSprintBar.color = new Color(Color.grey.r, Color.grey.g, Color.grey.b, 0.3f);
            }



            sprintTimer += Time.deltaTime;
            float fillAmount = Mathf.Lerp(startingFill, 1f, sprintTimer / maxSprintTimer);
            gameManager.gameInstance.playerSprintBar.fillAmount = fillAmount;
            yield return null;
        }
        gameManager.gameInstance.playerSprintBar.fillAmount = 1f;
        onSprintCoolDown = false;

        yield return new WaitForSeconds(.3f);
        gameManager.gameInstance.SprintBarBoarder.transform.GameObject().SetActive(false);

    }

    void stateMachine()
    {
        if (wallFront && Input.GetKey(KeyCode.W) && WallLookAngle < maxWallLookAngle)
        {
            //if (isSliding)
            //    stopSlide();

            if (!isClimbing && climbTimer > 0) startClimb();

            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) endClimbing();

        }
        else
        {
            if (isClimbing) endClimbing();
        }

        //if (isSprinting)
        //{
        //    if (Input.GetKeyDown(slideKey))
        //    {
        //        startingYPOS = transform.position.y;
        //        startSlide();
        //    }
        //    if (Input.GetKeyUp(slideKey) && isSliding)
        //    {
        //        stopSlide();
        //    }
        //}

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
    //void startSlide()
    //{
    //    controller.height = slideYScale;
    //    model.transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
    //    isSliding = true;
    //    slideTimer = maxSlideTime;
    //}

    //void slideMovement()
    //{
    //    Vector3 inputDir = transform.forward * verticalInput + transform.right * horizontalInput;
    //    if (!isPlayingSound)
    //        StartCoroutine(slideAud());
    //    speed += slideForce;

    //    slideTimer -= Time.deltaTime;

    //    if (slideTimer <= 0)
    //        stopSlide();

    //}

    //IEnumerator slideAud()
    //{
    //    isPlayingSound = true;
    //    AudioManager.audioInstance.playAudio(slideSounds[Random.Range(0, slideSounds.Length)], slideVol);
    //    yield return new WaitForSeconds(.05f);
    //    isPlayingSound = false;
    //}

    //void stopSlide()
    //{
    //    controller.height = controllerHeightOrgi;
    //    model.transform.localScale = new Vector3(transform.localScale.x, startingYScale, transform.localScale.z);
    //    speed = originalSpeed;
    //    isSliding = false;
    //}

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
    public void takeDamage(float amountOfDamageTaken)
    {
        if(!invincible)
        {
            // Subtract the amount of current damage from player HP
            playerHP -= amountOfDamageTaken;
            AudioManager.audioInstance.playAudio(hurtSounds[Random.Range(0, hurtSounds.Length)], hurtVol);
            StartCoroutine(damageFeedback());
            updatePlayerUI();
            if (playerHP <= 0)
            {
                gameManager.gameInstance.loseScreen();
            }

        }
        

    }
    public void recieveHP(float amount)
    {
        playerHP += amount;
        updatePlayerUI();
    }

    IEnumerator damageFeedback()
    {
        gameManager.gameInstance.flashDamage.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.gameInstance.flashDamage.SetActive(false);
    }

    public void updatePlayerUI()
    {
        gameManager.gameInstance.playerHPBar.fillAmount = playerHP / HPorig;
        gameManager.gameInstance.playerSprintBar.fillAmount = sprintTimer / maxSprintTimer;
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
        invincible = true;

        // Disable the collider
            //if (playerCollider != null)
            //{
            //    playerCollider.enabled = false;
            //}

        float elapsedTime = 0f;
        while (elapsedTime < dodgeDuration)
        {
            //Vector3 inputDir = transform.forward * verticalInput + transform.right * horizontalInput;
            speed += dodgeDistance;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        speed = originalSpeed;

        // Re-enable the collider
            //if (playerCollider != null)
            //{
            //    playerCollider.enabled = true;
            //}
        invincible = false;

        // Wait for the cooldown duration
        yield return new WaitForSeconds(dodgeCooldown);

        // End the dodge cooldown
        canDodge = true;
    }


    private IEnumerator PerformMelee()
    {
        canMelee = false;


        meleeWeapon.enabled = true;
        float elapsedTime = 0f;
        while (elapsedTime < meeleDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        meleeWeapon.enabled = false;

        yield return new WaitForSeconds(1f);

        canMelee = true;
    }

    void enableFlashLight()
    {
        isLit = !isLit;
        flashLight.gameObject.SetActive(isLit);
        AudioManager.audioInstance.playAudio(flashlightSounds[Random.Range(0, flashlightSounds.Length)], flashlightVol);
    }

    public void getWeaponStats(weaponStats gun)
    {
        currGun = gun;
        gunList.Add(gun);
        selectedGun = gunList.Count - 1;
        shootDamage = gun.shootDamage;
        shootDistance = gun.shootingDistance;
        shootRate = gun.shootRate;

        gunModel.GetComponent<MeshFilter>().sharedMesh = gun.gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gun.gunModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void selectGun()
    {
        if(Input.GetAxis("Mouse ScrollWheel") > 0 && selectedGun < gunList.Count - 1)
        {
           
            selectedGun++;
            changeGun();
        }
        else if(Input.GetAxis("Mouse ScrollWheel") < 0 && selectedGun > 0)
        {
            
            selectedGun--;
            changeGun();
        }
    
    }
    void changeGun()
    {
        currGun = gunList[selectedGun];
        shootDamage = gunList[selectedGun].shootDamage;
        shootRate = gunList[selectedGun].shootRate;
        shootDistance = gunList[selectedGun].shootingDistance;
        gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[selectedGun].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[selectedGun].gunModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

}