using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ExternalJsonLoader : MonoBehaviour
{
    // declare a delegate type that specifies the signature of the event handler
    public delegate void JsonLoadedEventHandler(object sender, string message);
    // declare a static event of the StringEventHandler delegate type
    public static event JsonLoadedEventHandler JsonLoadedEvent;

    [Header("References")] public GameObject loadingBar;
    public GameObject prefab;

    private void Start()
    {
        ExternalJsonLoader.JsonLoadedEvent += HandleStaticEvent;
        loadingBar.SetActive(false);
    }

    // TODO this will be used in other classes which parse the data
    // define a static method that handles the event
    public static void HandleStaticEvent(object sender, string message)
    {
        Debug.Log($"Static event triggered with message: {message}");
    }

    private IEnumerator OutputRoutine(string url) {
        // Debug.Log(url);
        // var loader = new WWW(url);
        // yield return loader;
        //
        // JsonLoadedEvent(null, loader.text);
        
        
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success) {
            // JsonLoadedEvent(null, www.downloadHandler.text);
            SerializeJson(www);
            loadingBar.SetActive(false);
        }
        else {
            // Error occurred while loading the file
            Debug.LogError("Error loading file: " + www.error);
            loadingBar.SetActive(false);
        }
    }
    
    // Called from UI
    public void LoadJson() {
        loadingBar.SetActive(true);
        var paths = StandaloneFileBrowser.OpenFilePanel("Choose JSON file", "", new [] {
            new ExtensionFilter("Json Files", "json" ),
        }, false);
        if (paths.Length > 0) {
            StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }

    private void SerializeJson(UnityWebRequest webRequest)
    {
        // If we have perf problems
        // using (Stream s = new MemoryStream(webRequest.downloadHandler.text))
        // using (StreamReader sr = new StreamReader(s))
        // using (JsonReader reader = new JsonTextReader(sr))
        // {
        //     JsonSerializer serializer = new JsonSerializer();
        //
        //     // read the json from a stream
        //     // json size doesn't matter because only a small piece is read at a time from the HTTP request
        //     Person p = serializer.Deserialize<Person>(reader);
        // }


        RawDataHolder holder = JsonConvert.DeserializeObject<RawDataHolder>(webRequest.downloadHandler.text);
        Debug.Log(holder.edges.Count);
        Debug.Log(holder.vertices.Count);
        long count = 0;
        foreach (var vertice in holder.vertices)
        {
            Instantiate(prefab, new Vector3(count++, 0f, 0f), Quaternion.identity);
        }

        count = 0;
        foreach (var edges in holder.edges)
        {
            Instantiate(prefab, new Vector3(count++, 2f, 0f), Quaternion.identity);
        }
    }
}
