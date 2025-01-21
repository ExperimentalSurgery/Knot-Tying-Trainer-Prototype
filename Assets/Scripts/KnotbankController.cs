using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnotbankController : MonoBehaviour
{

    public Transform knotBankBase;
    public SkinnedMeshRenderer rubberBands;
    public Renderer baseRenderer;
    public Renderer caseRenderer;
    public Light tensionLED;
    public Light contactLED;

    private int tmpInt;

    private int contactVal;

    private float yBase;

    private Material baseMatInst;
    private Material caseMatInst;

    private Color offGreen = new Color(0.8f, 1f, 0.8f);
    private Color offRed = new Color(1f, 0.8f, 0.8f);

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 90;
        yBase = knotBankBase.localPosition.y;

        baseMatInst = new Material(baseRenderer.material);
        baseRenderer.material = baseMatInst;

        caseMatInst = new Material(caseRenderer.material);
        caseRenderer.material = caseMatInst;
    }

    // Update is called once per frame
    void Update()
    {		
        if (contactVal == 0) 
        { 
            rubberBands.SetBlendShapeWeight(0, 100);
            baseMatInst.color = offGreen;
            contactLED.color = Color.green;
        }
		else
        { 
            rubberBands.SetBlendShapeWeight(0, 0);
            baseMatInst.color = Color.white;
            contactLED.color = Color.black;
        }
    }

    // Invoked when a line of data is received from the serial device.
    void OnMessageArrived(string msg)
    {
        string[] data = msg.Split(';');
        Debug.Log($"Contact: {data[0]} - Tension Grams: {data[1]}");

        if (int.TryParse(data[1], out tmpInt))
        {
            knotBankBase.localPosition = new Vector3(knotBankBase.localPosition.x, yBase - ((tmpInt - 20) / 200000f), knotBankBase.localPosition.z);
            caseMatInst.color = Color.Lerp(offGreen, offRed, Map(tmpInt - 20, 0, -330, 0f, 1f));
            tensionLED.color = Color.Lerp(Color.green, Color.red, Map(tmpInt - 20, 0, -330, 0f, 1f));
        }

        if (int.TryParse(data[0], out tmpInt))
            contactVal = tmpInt;
    }

    // Invoked when a connect/disconnect event occurs. The parameter 'success'
    // will be 'true' upon connection, and 'false' upon disconnection or
    // failure to connect.

    void OnConnectionEvent(bool success)
    {
        if (success)
            Debug.Log("Connection established");
        else
            Debug.Log("Connection attempt failed or disconnection detected");
    }

    float Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
