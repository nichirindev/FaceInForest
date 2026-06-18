using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TMP_Text fpsText;
    public float updateInterval = 0.5f;

    private float deltaTime;
    private float timer;
    private int frameCount;

    void Update()
    {
        frameCount++;
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            float fps = frameCount / timer;
            fpsText.text = Mathf.RoundToInt(fps).ToString() + " FPS";
            frameCount = 0;
            timer = 0f;
        }
    }
}
