using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Director : MonoBehaviour
{
    Canvas canvas;
    Player player;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        canvas = FindObjectOfType<Canvas>();
        player = FindObjectOfType<Player>();
        refreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void refreshUI()
    {
        canvas.transform.Find("Funds/Value").GetComponent<TextMeshProUGUI>().text = player.getFunds().ToString();
    }
}
