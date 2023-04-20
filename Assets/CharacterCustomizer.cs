using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomizer : MonoBehaviour
{
    [SerializeField] private GameObject[] Faces;
    [SerializeField] private GameObject[] Hair;

    private int hairIndex = 0;
    private int faceIndex = 0;

    public void FaceButton(int direction)
    {
        Faces[faceIndex].SetActive(false);
        faceIndex += direction;
        if (faceIndex > Faces.Length - 1 )
        {
            faceIndex = 0;
        }else if (faceIndex < 0)
        {
            faceIndex = Faces.Length - 1;
        }
        Faces[faceIndex].SetActive(true);
    }
    
    public void HairButton(int direction)
    {
        Hair[hairIndex].SetActive(false);
        hairIndex += direction;
        if (hairIndex > Hair.Length - 1 )
        {
            hairIndex = 0;
        }else if (hairIndex < 0)
        {
            hairIndex = Hair.Length - 1;
        }
        Hair[hairIndex].SetActive(true);
    }
    
}
