using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ImageReader : MonoBehaviour
{
    [SerializeField]
    RawImage testImage;

    async void Start()
    {
        string resourcesPath = Application.streamingAssetsPath + "/Screenshots";

        if (!Directory.Exists(resourcesPath))
        Directory.CreateDirectory(resourcesPath);

        ScreenCapture.CaptureScreenshot(resourcesPath + "/Test.png");
        Debug.Log("Скриншот сделан и сохранён в: " + resourcesPath);

        await Task.Delay(500);

        Texture2D texture = new Texture2D(Screen.width / 2, Screen.height / 2);
        var loadingData = File.ReadAllBytes(resourcesPath + "/Test.png");
        texture.LoadImage(loadingData);

        if (texture == null) throw new NullReferenceException();

        testImage.GetComponent<RectTransform>().sizeDelta = new Vector2(
            texture.width / 2, 
            texture.height / 2
        );
        
        testImage.texture = texture;
    }

    void Update()
    {
        
    }
}
