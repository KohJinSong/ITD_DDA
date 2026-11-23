using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class Note : MonoBehaviour
{
    [Header("Views (children of the note's Canvas)")]
    public GameObject collapsedView;   // small bubble
    public GameObject expandedView;    // big card with full text + heart

    [Header("Heart")]
    public TMP_Text heartCountText;
    public int hearts = 0;
    public GameObject heartObject;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip popClip;

    [Header("Text")]
    public TMP_Text previewText;   // the short text in CollapsedView
    public TMP_Text fullText;      // the long text in ExpandedView

    [Header("Drag")]
    public float holdToDragTime = 0.4f;   // hold this long to start dragging

    // private state — inside the class
    bool isExpanded;
    Vector3 baseScale;
    Vector3 heartTextBaseScale;
    Coroutine punchRoutine;
    Coroutine heartRoutine;
    bool pressed, dragging;
    float pressTime;
    Plane dragPlane;
    Vector2 pos;
    

    public void BeginPress()              // wire to Select Entered
    {
        pressed = true;
        dragging = false;
        pressTime = Time.time;
    }

    public void EndPress()                // wire to Select Exited
    {
        pressed = false;
        if (dragging) { dragging = false; return; }  // was a drag → keep new spot
        OnTapped();                                   // quick tap → expand/collapse
    }

    void Update()
    {
        if (!pressed) return;

        if (!dragging && Time.time - pressTime > holdToDragTime)
        {
            dragging = true;
            // invisible wall facing the camera, passing through the note
            dragPlane = new Plane(-Camera.main.transform.forward, transform.position);
        }

        if (dragging)
        {
            Vector2 pos = Mouse.current != null ? Mouse.current.position.ReadValue()
                                                : Touchscreen.current.primaryTouch.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(pos);
            if (dragPlane.Raycast(ray, out float enter))
                transform.position = ray.GetPoint(enter);
        }
    }


    void Awake()
    {
        baseScale = transform.localScale;   // remembered once, never changes
        if (heartCountText != null)
            heartTextBaseScale = heartCountText.transform.localScale;
    }

    void Start()
    {
        collapsedView.SetActive(true);
        expandedView.SetActive(false);
        if (heartObject != null) heartObject.SetActive(false);   // hidden until expanded
        UpdateHeartText();
    }

    public void OnTapped()
    {
        isExpanded = !isExpanded;
        collapsedView.SetActive(!isExpanded);
        expandedView.SetActive(isExpanded);
        if (heartObject != null) heartObject.SetActive(isExpanded);
        PlayPop();

        if (punchRoutine != null) StopCoroutine(punchRoutine);
        punchRoutine = StartCoroutine(Punch());
    }

    public void AddHeart()
    {
        hearts++;
        UpdateHeartText();
        PlayPop();

        if (heartRoutine != null) StopCoroutine(heartRoutine);
        heartRoutine = StartCoroutine(PunchHeartText());
    }

    public void SetText(string title, string info)
    {
        Debug.Log($"SetText: '{title}' preview={(previewText == null ? "NULL" : "wired")} full={(fullText == null ? "NULL" : "wired")}");
        if (previewText != null) previewText.text = title;
        if (fullText != null)
            fullText.text = string.IsNullOrWhiteSpace(info) ? title : info;
    }

    void UpdateHeartText()
    {
        if (heartCountText != null)
            heartCountText.text = hearts.ToString();
    }

    void PlayPop()
    {
        if (audioSource != null && popClip != null)
            audioSource.PlayOneShot(popClip);
    }

    System.Collections.IEnumerator Punch()
    {
        transform.localScale = baseScale * 1.15f;
        yield return new WaitForSeconds(0.08f);
        transform.localScale = baseScale;   // absolute restore — can't grow forever
        punchRoutine = null;
    }

    System.Collections.IEnumerator PunchHeartText()
    {
        if (heartCountText != null)
        {
            heartCountText.transform.localScale = heartTextBaseScale * 1.3f;
            yield return new WaitForSeconds(0.08f);
            heartCountText.transform.localScale = heartTextBaseScale;
        }
        heartRoutine = null;
    }
}