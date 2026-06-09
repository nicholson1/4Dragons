using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  
/// <summary>
/// I removed this from the GameManager prefab for now as it's also handled in InputHandler.
/// We might revisit this class if we want a cursor switch system
/// </summary>
public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTex;    
    void Awake()
    {
        Cursor.SetCursor(cursorTex, Vector2.zero, CursorMode.ForceSoftware);
    }
}
