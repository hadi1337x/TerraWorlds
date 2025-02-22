using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingBarController : MonoBehaviour
{
    [Header("UI References")]
    public Slider loadingSlider;
    public GameObject afterLoad;
    public TMP_Text guestBTN;

    [Header("Connection Settings")]
    public ClientConn clientConn;

    [Header("Loading Settings")]
    public float fillSpeed = 1f;

    private bool isLoading = true;

    void Start()
    {
        if (loadingSlider == null)
        {
            Debug.LogError("Loading Slider not assigned!");
        }

        if (clientConn == null)
        {
            clientConn = FindObjectOfType<ClientConn>();
        }
    }

    void Update()
    {
        if (isLoading)
        {
            if (clientConn != null && clientConn.IsConnected())
            {
                if (loadingSlider.value < loadingSlider.maxValue)
                {
                    loadingSlider.value += fillSpeed * Time.deltaTime;
                }
                else
                {
                    isLoading = false;
                    Debug.Log("Loading complete!");
                }
            }
        }
        if (Mathf.Approximately(loadingSlider.value, loadingSlider.maxValue) || loadingSlider.value >= loadingSlider.maxValue)
        {
            SceneManager.LoadScene("MainMenu");
            /*
            loadingSlider.gameObject.SetActive(false);
            afterLoad.gameObject.SetActive(true);
            guestBTN.text = "Play as "+clientConn.myGuestName;*/
        }
    }
}
