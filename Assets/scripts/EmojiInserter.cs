using UnityEngine;
using TMPro;

public class EmojiInserter : MonoBehaviour
{
    public TMP_InputField field;

    public void Append(string emoji)   // type the emoji in the button's OnClick box
    {
        field.text += emoji;
    }
}