using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PauseScreen : MonoBehaviour
{
    public Button resume;
    public Button quit; 
    public GameObject canvas;
    public bool ispaused = false;

    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
       
    }

    [ContextMenu("test pause menu")]
    void ToggleMenu()
    {
      ispaused = !ispaused;
        canvas.SetActive(ispaused);
    }
}
