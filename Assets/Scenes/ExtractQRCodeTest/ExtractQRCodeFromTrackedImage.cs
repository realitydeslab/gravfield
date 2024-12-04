using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using ZXing;
using TMPro;

public class ExtractQRCodeFromTrackedImage : MonoBehaviour
{
    [SerializeField]
    ARCameraBackground m_ARCameraBackground;
    RenderTexture m_RenderTexture;
    [SerializeField]
    RawImage m_rawImage;
    [SerializeField]
    TextMeshProUGUI m_resultText;


    BarcodeReader barcodeReader;

    void Start()
    {
        ResizeTexture(m_RenderTexture, Screen.width, Screen.height);

        m_rawImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height) * 0.5f;

        barcodeReader = new BarcodeReader();
    }

    public void OnTrackedImageStablized(Vector3 position, Quaternion rotation)
    {
        // Get camera image
        // Create a new command buffer
        var commandBuffer = new CommandBuffer();
        commandBuffer.name = "AR Camera Background Blit Pass";

        // Get a reference to the AR Camera Background's main texture
        // We will copy this texture into our chosen render texture
        var texture = !m_ARCameraBackground.material.HasProperty("_MainTex") ?
            null : m_ARCameraBackground.material.GetTexture("_MainTex");

        // Save references to the active render target before we overwrite it
        var colorBuffer = Graphics.activeColorBuffer;
        var depthBuffer = Graphics.activeDepthBuffer;

        // Set Unity's render target to our render texture
        Graphics.SetRenderTarget(m_RenderTexture);

        // Clear the render target before we render new pixels into it
        commandBuffer.ClearRenderTarget(true, false, Color.clear);

        // Blit the AR Camera Background into the render target
        commandBuffer.Blit(
            texture,
            BuiltinRenderTextureType.CurrentActive,
            m_ARCameraBackground.material);

        // Execute the command buffer
        Graphics.ExecuteCommandBuffer(commandBuffer);

        // Set Unity's render target back to its previous value
        Graphics.SetRenderTarget(colorBuffer, depthBuffer);

        // Read QR Code
        ReadQRCode();
    }


    void ResizeTexture(RenderTexture src, int width, int height)
    {
        if (src == null || src.width != width || src.height != height)
        {
            Debug.Log($"[{this.GetType().Name}]: Resize render texture to ({ width},{height} ))");
            m_RenderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);

            m_rawImage.texture = m_RenderTexture;
        }
    }

    void ReadQRCode()
    {
        Texture2D texture2D = new Texture2D(m_RenderTexture.width, m_RenderTexture.height, TextureFormat.RGBA32, false);

        RenderTexture temp_rt = RenderTexture.active;
        RenderTexture.active = m_RenderTexture;
        texture2D.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = temp_rt;

        Result result = barcodeReader.Decode(texture2D.GetPixels32(), texture2D.width, texture2D.height);
        if (result != null)
        {
            m_resultText.text = result.Text;
        }
        else
        {
            m_resultText.text = "Reading Failed";
        }
    }
}
