using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
using Mirror;

public class MainMenu : MonoBehaviour
{
    public GameObject settingsTab;
    public GameObject mainMenuTab;
    public GameObject controlsTab;
    public GameObject joinTab;

    public AudioMixer audioMixer;

    public TMP_Dropdown resolutionDropdown;
    public TMP_InputField joinIpAddressInputField; // Assign in Inspector
    public NetworkManager networkManager; // Assign in Inspector

    Resolution[] resolutions;

    public void Start()
    {

        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        // Set the input field's text to "localhost"
        joinIpAddressInputField.text = "localhost";

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for(int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        mainMenuTab.SetActive(true); 
        settingsTab.SetActive(false);
        controlsTab.SetActive(false);
        joinTab.SetActive(false);
    }

    void Awake()
    {
        // Attempt to automatically find the NetworkManager if not set
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }
    }

    public void PlayGame ()
    {
        mainMenuTab.SetActive(false);
        joinTab.SetActive(true);
    }

    public void UnPlayGame()
    {
        mainMenuTab.SetActive(true);
        joinTab.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsTab.SetActive(true);
        mainMenuTab.SetActive(false);
    }

    public void CloseSettings()
    {
        mainMenuTab.SetActive(true);
        settingsTab.SetActive(false);
    }
    public void OpenControls()
    {
        controlsTab.SetActive(true);
        mainMenuTab.SetActive(false);
    }

    public void CloseControls()
    {
        controlsTab.SetActive(false);
        mainMenuTab.SetActive(true);
    }
    public void SetVolume (float volume)
    {
        audioMixer.SetFloat("Volume", volume);
    }
    
    public void SetQuality(int quality)
    {
        QualitySettings.SetQualityLevel(quality);
    }

    public void SetFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void QuitGame ()
    {
        Debug.Log("Quit");
        Application.Quit();

    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void HostGame()
    {
        networkManager.StartHost();
        UnitySceneManager.LoadScene(networkManager.onlineScene);
    }

    public void JoinGame()
    {
        if (joinIpAddressInputField != null)
        {
            networkManager.networkAddress = joinIpAddressInputField.text; // Use the IP address from the input field
            networkManager.StartClient();
            UnitySceneManager.LoadScene(networkManager.onlineScene);
        }
        else
        {
            Debug.LogError("Join IP Address Input Field is not assigned.");
        }
    }
}
