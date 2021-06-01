using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FaceTextManager : MonoBehaviour
{
    private TextMesh textMesh;
    private ARCameraManager cameraManager;
    private Texture2D m_Texture;

    // Start is called before the first frame update
    async void Start()
    {
        transform.position = new Vector3(transform.position.x + 0.1f, transform.position.y, transform.position.z);
        this.transform.Rotate(0, 180f, 0);

        textMesh = GetComponent<TextMesh>();
        cameraManager = GetComponentInParent<ARCameraManager>();

        if(cameraManager == null)
        {
            textMesh.text = "There is no Camera Manager";
        }
        else
        {
            textMesh.text = "Hello, Trying to get information";
            try
            {
                await GetImageAsync();
            }
            catch
            {
                textMesh.text = "failed to get information\nTry again";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x + 0.1f, transform.position.y, transform.position.z);
        this.transform.Rotate(0, 180f, 0);
    }
    
    //Processing of taking image
    public async Task GetImageAsync()
    {

        await Task.Delay(1000);
        // Get information about the device camera image.
        if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            // If successful, launch a coroutine that waits for the image
            // to be ready, then apply it to a texture.
            StartCoroutine(ProcessImage(image));

            // It's safe to dispose the image before the async operation completes.
            image.Dispose();
        }
    }

    IEnumerator ProcessImage(XRCpuImage cpuImage)
    {
        // Create the async conversion request.
        var request = cpuImage.ConvertAsync(new XRCpuImage.ConversionParams
        {
            // Use the full image.
            inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),

            // Color image format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the Y axis.
            transformation = XRCpuImage.Transformation.MirrorY
        });

        // Wait for the conversion to complete.
        while (!request.status.IsDone())
            yield return null;

        // Check status to see if the conversion completed successfully.
        if (request.status != XRCpuImage.AsyncConversionStatus.Ready)
        {
            // Something went wrong.
            Debug.LogErrorFormat("Request failed with status {0}", request.status);

            // Dispose even if there is an error.
            request.Dispose();
            yield break;
        }

        // Image data is ready. Let's apply it to a Texture2D.
        var rawData = request.GetData<byte>();

        // Create a texture if necessary.
        if (m_Texture == null)
        {
            m_Texture = new Texture2D(
                request.conversionParams.outputDimensions.x,
                request.conversionParams.outputDimensions.y,
                request.conversionParams.outputFormat,
                false);
        }

        // Copy the image data into the texture.
        m_Texture.LoadRawTextureData(rawData);
        m_Texture.Apply();

        Texture2D rotatedTexture = m_Texture;

        switch (Screen.orientation)
        {
            case ScreenOrientation.Portrait:
                rotatedTexture = rotateTexture(m_Texture, true);
                break;
            case ScreenOrientation.LandscapeLeft:
                rotatedTexture = rotateTexture(m_Texture, true);
                rotatedTexture = rotateTexture(rotatedTexture, true);
                break;
            case ScreenOrientation.LandscapeRight:
                break;
            case ScreenOrientation.PortraitUpsideDown:
                rotatedTexture = rotateTexture(m_Texture, false);
                break;
        }

        // Making a PNG
        byte[] m_Texture_jpg = rotatedTexture.EncodeToJPG();

        StorePictureInGallery(m_Texture_jpg);
        yield return GetUserData(m_Texture_jpg);

        // Need to dispose the request to delete resources associated
        // with the request, including the raw data.
        request.Dispose();
    }

    Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    public void StorePictureInGallery(byte[] imageFace)
    {
        //Store picture in gallery
        string name = string.Format("{0}_Capture{1}_{2}.jpg", Application.productName, "{0}", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        NativeGallery.SaveImageToGallery(imageFace, Application.productName, name);

        textMesh.text = "Picture saved in gallery\nWaiting for information";
    }

    IEnumerator GetUserData(byte[] imageFace)
    {
        ImageObject imageObject = new ImageObject();
        string imageString = Convert.ToBase64String(imageFace);
        imageObject.base64 = imageString;
        string jsonString = JsonUtility.ToJson(imageObject);
        Debug.Log(jsonString);

        UnityWebRequest webRequest = UnityWebRequest.Put("http://51.144.142.34:3000/image", jsonString);
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "application/json");
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("error from request?");
            Debug.Log(webRequest.error);
        }
        else
        {
            var rawJson = webRequest.downloadHandler.text;
            rawJson = "{\"Persons\":" + rawJson + "}";
            Debug.Log(rawJson);

            PatientStatus[] Patients = JsonHelper.FromJson<PatientStatus>(rawJson);
            Debug.Log("Picture send");

            if(Patients.Length < 1 || Patients[0].status == "")
            {
                textMesh.text = "No Person detected by face-api";
            }
            else if ((Patients[0].status.ToLower() == "identified"))
            {
                textMesh.text = "naam: " + Patients[0].patient.name + "\nleeftijd: " + Patients[0].patient.age + "\ngeslacht: " + Patients[0].patient.gender;
                if(Patients[0].patient.medicines.Length > 0)
                {
                    textMesh.text = textMesh.text + "\nmedicijnen: " + Patients[0].patient.medicines[0].name;
                }
                if (Patients[0].patient.care.Length > 0)
                {
                    textMesh.text = textMesh.text + "\nzorg: " + Patients[0].patient.care[0].name;
                }
            } 
            else if ((Patients[0].status.ToLower() == "not identified"))
            {
                textMesh.text = "Person not identified";
            }
            else if ((Patients[0].status.ToLower() == "no data"))
            {
                textMesh.text = "naam: " + Patients[0].info + " No data found";
            }

            //Debug.Log("Form upload complete!");
            //Debug.Log(webRequest.downloadHandler.text);
            ////Debug.Log(user.userData.image.ToString());
            //textMesh.text = userMedicalData.name + ' ' + userMedicalData.leeftijd + ' ' + userMedicalData.geslacht;
        }

        webRequest.Dispose();
    }

    IEnumerator GetTestDataUnityRequest(byte[] imageFace)
    {
        TestData testData = new TestData();
        testData.id = 25;
        testData.first_name = "Corine";
        testData.phone = "123456";
        string imageString = Convert.ToBase64String(imageFace);
        testData.image = imageString;
        Debug.Log(testData.image);
        string jsonString = JsonUtility.ToJson(testData);

        UnityWebRequest webRequest = UnityWebRequest.Put("https://myfakeapi.com/api/users/", jsonString);
        webRequest.method = "POST";
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Accept", "application/json");
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            var rawJson = webRequest.downloadHandler.text;
            TestUser user = JsonUtility.FromJson<TestUser>(rawJson);
            Debug.Log("Form upload complete!");
            Debug.Log(webRequest.downloadHandler.text);
            Debug.Log(user.userData.image.ToString());
            textMesh.text = user.userData.id + ' ' + user.userData.first_name;
        }

        webRequest.Dispose();
    }
}
