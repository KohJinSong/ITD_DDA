using UnityEngine;

public class HeartButton : MonoBehaviour
{
    public Note note;   // drag the parent note here

    public void OnTapped()
    {
        Debug.Log("HEART TAPPED");
        if (note != null) note.AddHeart();
    }
}