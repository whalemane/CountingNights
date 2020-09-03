using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BreathBar : MonoBehaviour
{

    private Slider bar;
    private Image barImage;
    // Start is called before the first frame update
    void Start()
    {

        bar = GetComponent<Slider>();
        bar.value = 0.9f;
        barImage = bar.fillRect.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(bar.value > 0.75f)
        {
            barImage.color = Color.red;
        }
        else if(bar.value > 0.25f)
        {
            barImage.color = Color.green;
        }
        else
        {
            barImage.color = Color.red;
        }
    }
}
