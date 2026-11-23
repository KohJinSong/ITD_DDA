using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using Firebase.Database;

public class NoteSpawner : MonoBehaviour
{
    [Header("Wiring")]
    public GameObject notePrefab;       // NotePrefab
    public GameObject addNotePanel;     // the UI panel
    public TMP_InputField titleInput;   // "Enter Title..."  → collapsed view
    public TMP_InputField infoInput;    // "Enter Info..."   → expanded view

    [Header("Layout")]
    public float radius = 0.15f;   // ring distance from marker (meters)
    public float height = 0.12f;   // float height above marker

    [Header("XRI")]
    public GameObject screenRayInteractor;

    ARTrackedImageManager imageManager;
    int noteCount = 0;


    [System.Serializable]
    public class NoteData
    {
        public string title;
        public string info;
        public int hearts;
        public float posX, posY, posZ;
        public long timestamp;
    }
    
    [Header("Firebase")]
    public string locationId = "loc1";   // one id per marker/location

    DatabaseReference db;

    void Start()
    {
        imageManager = FindFirstObjectByType<ARTrackedImageManager>();
        db = FirebaseDatabase.GetInstance(
            "https://itd-dda-5c195-default-rtdb.asia-southeast1.firebasedatabase.app/"
        ).RootReference;
    }

    public void OpenPanel()
    {
        addNotePanel.SetActive(true);
        screenRayInteractor.SetActive(false);   // world taps paused while typing
    }

    public void ClosePanel()
    {
        addNotePanel.SetActive(false);
        screenRayInteractor.SetActive(true);
    }

    public void PostNote()
    {
        string title = titleInput.text;
        if (string.IsNullOrWhiteSpace(title)) return;

        var clusterObj = GameObject.FindGameObjectWithTag("NoteCluster");
        if (clusterObj == null) { Debug.Log("Scan the marker first!"); return; }

        float angle = noteCount * 60f * Mathf.Deg2Rad;   // make sure it's 60f, not 0f
        Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);

        GameObject go = Instantiate(notePrefab, clusterObj.transform);
        go.transform.localPosition = offset;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        var note = go.GetComponentInChildren<Note>();
        if (note != null) note.SetText(title, infoInput.text);

        NoteData data = new NoteData
        {
            title = title,
            info = infoInput.text,
            hearts = 0,
            posX = offset.x, posY = offset.y, posZ = offset.z,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        db.Child("notes").Child(locationId).Push().SetRawJsonValueAsync(JsonUtility.ToJson(data));

        noteCount++;
        titleInput.text = "";
        infoInput.text = "";
        ClosePanel();
    }

    Transform GetActiveMarker()
    {
        foreach (var img in imageManager.trackables)
            if (img.trackingState == TrackingState.Tracking)
                return img.transform;
        return null;
    }
}