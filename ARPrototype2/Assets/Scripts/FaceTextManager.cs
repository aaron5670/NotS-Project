using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
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
            textMesh.text = "Hello World, In need for medical information";
            try
            {
                await GetImageAsync();
            }
            catch
            {
                textMesh.text = "Image saving failed";
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
            outputDimensions = new Vector2Int(cpuImage.width / 2, cpuImage.height / 2),

            // Color image format.
            outputFormat = TextureFormat.ARGB32,

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

        Debug.Log(rawData);

        // Copy the image data into the texture.
        m_Texture.LoadRawTextureData(rawData);
        m_Texture.Apply();

        // Making a PNG
        byte[] m_Texture_Png = m_Texture.EncodeToPNG();

        GetMedicalDataOfPersonFromPicture(m_Texture_Png);

        // Need to dispose the request to delete resources associated
        // with the request, including the raw data.
        request.Dispose();
    }

    public void GetMedicalDataOfPersonFromPicture(byte[] image)
    {
        //Store picture in gallery
        string name = string.Format("{0}_Capture{1}_{2}.png", Application.productName, "{0}", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        NativeGallery.SaveImageToGallery(image, Application.productName, name);

        textMesh.text = "Picture saved in gallery";

        //TO DO Send picture to FR and get medical data
        //Send screencapture to facial recognition
        //Get Medical data
        //Set Medical data in textMesh
    }
}
