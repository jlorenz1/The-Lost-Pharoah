using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    
    [SerializeField] public GameObject mainMenu;
    [SerializeField] public GameObject optionsMenu;
    [SerializeField] public GameObject creditsMenu;

    [SerializeField] public AudioMixer audioMixer;

    [SerializeField] public Slider sensSlider;
    [SerializeField] public Slider masterVolSlider;
    [SerializeField] public Slider playerVolSlider;
    [SerializeField] public Slider SFXVolSlider;
    [SerializeField] public Slider musicVolSlider;

    public void Awake()
    {
        sensSlider.value = PlayerPrefs.GetInt("sens");
        masterVolSlider.value = PlayerPrefs.GetFloat("MasterVol", 1f);
        SFXVolSlider.value = PlayerPrefs.GetFloat("SFXVol", 1f);
        musicVolSlider.value = PlayerPrefs.GetFloat("MusicVol", 1f);


        masterVolSlider.onValueChanged.AddListener(onMasterSliderChange);
        SFXVolSlider.onValueChanged.AddListener(onSFXSliderChange);
        musicVolSlider.onValueChanged.AddListener(onMusicSliderChange);

    }

    public void Start()
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolSlider.value) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(SFXVolSlider.value) * 20);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolSlider.value) * 20);
    }
    public void Update()
    {
        if(optionsMenu.activeSelf)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                saveSettings();
                optionsMenu.SetActive(false);
                mainMenu.SetActive(true);
            }
        }
        else if(creditsMenu.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                creditsMenu.SetActive(false);
                mainMenu.SetActive(true);
            }
        }

        PlayerPrefs.SetInt("sens", (int)sensSlider.value);

    }
    public void play()
    {
        SceneManager.LoadScene(1);
    }

    public void onSensSliderChange(float value)
    {
        PlayerPrefs.SetInt("sens", (int)value);

        PlayerPrefs.Save();
    }

    public void onMasterSliderChange(float value)
    {
        Debug.Log("Master Volume Slider Value: " + value);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20); //log10 for decibles 
        saveSettings();
    }
    public void onSFXSliderChange(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20); //log10 for decibles 
        saveSettings() ;
    }

    public void onMusicSliderChange(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20); //log10 for decibles 
        saveSettings();
    }

    public void saveSettings()
    {
        PlayerPrefs.SetInt("sens", (int)sensSlider.value);
        PlayerPrefs.SetFloat("MasterVol", masterVolSlider.value);
        PlayerPrefs.SetFloat("SFXVol", SFXVolSlider.value);
        PlayerPrefs.SetFloat("MusicVol", musicVolSlider.value);

        PlayerPrefs.Save();
    }
}
