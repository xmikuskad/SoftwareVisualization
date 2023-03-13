using System.Collections;
using SFB;
using UnityEngine;
using UnityEngine.Networking;

public class ExternalJsonLoader : MonoBehaviour
{
    private bool isLoading = false;
    private DataLoaderJob dataLoaderJob;

    [Header("References")] public GameObject loadingBar;
    public GameObject prefab;
    public GameObject loadBtn;

    private void Start()
    {
        SetLoading(false);
    }

    private void Update()
    {
        processJobs();
    }
    
    // Load file from disk
    private IEnumerator OutputRoutine(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            dataLoaderJob = new DataLoaderJob();
            dataLoaderJob.text = www.downloadHandler.text;
            dataLoaderJob.Start(); // Don't touch any data in the job class after you called Start until IsDone is true.
        }
        else
        {
            // Error occurred while loading the file
            Debug.LogError("Error loading file: " + www.error);
            SetLoading(false);
        }
    }

    // Called from UI btn
    public void LoadJson()
    {
        SetLoading(true);
        var paths = StandaloneFileBrowser.OpenFilePanel("Choose JSON file", "", new[]
        {
            new ExtensionFilter("Json Files", "json"),
        }, false);
        if (paths.Length > 0)
        {
            StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }

    // Manages loading screen
    public void SetLoading(bool status)
    {
        isLoading = status;
        loadingBar.SetActive(status);
        loadBtn.SetActive(!status);
    }

    // Takes care of async jobs and handling
    private void processJobs()
    {
        if (dataLoaderJob != null)
        {
            if (dataLoaderJob.Update())
            {
                // Alternative to the OnFinished callback
                SingletonManager.Instance.dataManager.LoadData(dataLoaderJob.dataHolder);
                dataLoaderJob = null;
                SetLoading(false);
            }
        }
    }
}
