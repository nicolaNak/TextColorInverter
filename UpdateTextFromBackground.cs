using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

/// <summary>
/// Attach to the text you want to have updated to stand out above the background
/// Need a second AR camera as a child of the main AR camera rendering to texture. Add the texture to the script when attached as a component
/// AR systems constantly change how to access the camera view, so using a second camera and rendertexture avoids having repeat updates
/// </summary>

public class UpdateTextFromBackground : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private float _adjustedWidth;
    private float _adjustedHeight;

    Vector2Int _checkXValues;
    Vector2Int _checkYValues;

    private int _iter = 0;

    private Vector3 _textPosition;

    public RenderTexture CameraTexture;

    private Texture2D _textureData;

    private Rect _readData;

    private Color _colorCheck;

    private Color _textUpdatedColour;

    private RectTransform _positionCheckImage;

    public Image _foundColorImage;
    public Image _appliedColorImage;
    public TMPro.TextMeshProUGUI _foundColorText;
    public TMPro.TextMeshProUGUI _appliedColorText;

    public RawImage _renderTextureCheck;

    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();

        GameObject newCheck = new GameObject("PositionImageCheck");
        newCheck.AddComponent<Image>();
        newCheck.GetComponent<Image>().color = new Color(Color.magenta.r, Color.magenta.g, Color.magenta.b, 0.3f);
        newCheck.GetComponent<Image>().raycastTarget = false;
        newCheck.transform.SetParent(transform, false);
        _positionCheckImage = newCheck.GetComponent<RectTransform>();
        //align to far left
        _positionCheckImage.anchorMin = new Vector2(0f, 0.5f);
        _positionCheckImage.anchorMax = new Vector2(0f, 0.5f);

        _checkXValues = Vector2Int.zero;
        _checkYValues = Vector2Int.zero;

        _textureData = new Texture2D(CameraTexture.width, CameraTexture.height, TextureFormat.RGB24, false);

        _readData = new Rect(0, 0, CameraTexture.width, CameraTexture.height);
    }


    void Update()
    {
        if (_iter < 10)
        {
            _iter++;
        }
        if (_iter == 10)
        {
            //get position of text box
            Vector3[] cornerArray = new Vector3[4];
            _text.rectTransform.GetWorldCorners(cornerArray);

            //get rendered width & height
            Vector2 renderedSize = _text.GetRenderedValues();

            //get alignment of text
            TextAlignmentOptions alignment = _text.alignment;
            string alignmentStr = _text.alignment.ToString();

            _VerticalAlignmentOptions verticalAlignment;
            _HorizontalAlignmentOptions horizontalAlignment;

            //split vertical and horizontal alignment
            //if horizontal alignments are one word the vertical setting is top
            if (alignmentStr.Contains("Left"))
                horizontalAlignment = _HorizontalAlignmentOptions.Left;
            if (alignmentStr.Contains("Center"))
                horizontalAlignment = _HorizontalAlignmentOptions.Center;

            //if vertical alignments are one word the horizontal setting is center

           
            //if ((alignment & TextAlignmentOptions.Left) != 0) 
            //singular enum options are actually center + option, and vertical/horizontal alignment option enums don't match the regular alighment options

               

            for (int i = 0; i < 4; i++)
            {
                Debug.Log(cornerArray[i]);

            //    if(i < 3)
            //        DrawLine(cornerArray[i], cornerArray[i + 1], Color.green);
            //    else
            //        DrawLine(cornerArray[i], cornerArray[0], Color.green);
            }

            //use corner[0].x for LHS
            //if (alignment.Contains("Left") || alignment.Contains("Justified"))
            //{

            //}
            //if(alignment.Contains("Center") || alignment.Contains("GeoAligned"))

            //botton LH position
            Vector2 screenPositionLHS = LocalPositionToScreenPosition(_text.rectTransform);
            //find top RH position using width/height
            Vector2 screenPositionRHS = new Vector2(screenPositionLHS.x + _positionCheckImage.sizeDelta.x, screenPositionLHS.y + _positionCheckImage.sizeDelta.y);
            //convert to int for iteration
            _checkXValues = new Vector2Int(Mathf.RoundToInt(screenPositionLHS.x), Mathf.RoundToInt(screenPositionRHS.x));
            _checkYValues = new Vector2Int(Mathf.RoundToInt(screenPositionLHS.y), Mathf.RoundToInt(screenPositionRHS.y));

            _iter++;
        }

        RenderTexture.active = CameraTexture;

        _textureData.ReadPixels(_readData, 0, 0);
        _textureData.Apply();

        RenderTexture.active = null;

        _text.faceColor = UpdateTextColor(false);
        
    }

    private Color UpdateTextColor(bool greyscale, bool inverse = true)
    {
        Color colorCheck = Color.white;
        Color updatedColor = Color.black;

        Texture2D updatedTexture = new Texture2D(100, 100);

        if (_iter >= 10)
        {
            updatedTexture = new Texture2D(Mathf.RoundToInt(_positionCheckImage.sizeDelta.x), Mathf.RoundToInt(_positionCheckImage.sizeDelta.y));
        }

        int xIter = 0;
        int yIter = 0;
        //get size of text area this script is attached to - need to check where text is on screen, default is 0,0 at bottom right corner of screen
        for (int x = _checkXValues.x; x <= _checkXValues.y; x++)
        {
            for (int y = _checkYValues.x; y <= _checkYValues.y; y++)
            {
                colorCheck += _textureData.GetPixel(x, y);

                if (x >= _checkXValues.x || y >= _checkYValues.x)
                {
                    colorCheck *= 0.5f;
                }

                if (_iter >= 10)
                {
                    updatedTexture.SetPixel(xIter, yIter, _textureData.GetPixel(x, y));
                }

                yIter++;
            }
            xIter++;
        }

        if (greyscale)
        {
            float greyScaleAverage = (colorCheck.r + colorCheck.g + colorCheck.b) / 3f;

            updatedColor = new Color(greyScaleAverage, greyScaleAverage, greyScaleAverage);            
        }
        else
        {
            updatedColor = colorCheck;
        }

        if (inverse)
        {
            updatedColor = Color.white - updatedColor;
        }

        updatedColor.a = 1f;

        _foundColorImage.color = colorCheck;
        _appliedColorImage.color = updatedColor;

        _foundColorText.text = "R:" + System.Math.Round(colorCheck.r, 2) + " G:" + System.Math.Round(colorCheck.g, 2) + " B:" + System.Math.Round(colorCheck.b, 2);
        _appliedColorText.text = "R:" + System.Math.Round(updatedColor.r, 2) + " G:" + System.Math.Round(updatedColor.g, 2) + " B:" + System.Math.Round(updatedColor.b, 2);

        if (_iter >= 10)
        {
            updatedTexture.Apply();
            _renderTextureCheck.rectTransform.sizeDelta = _positionCheckImage.sizeDelta;
            _renderTextureCheck.texture = updatedTexture;
        }
        
        return updatedColor;
    }

    void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 5f;
        lr.endWidth = 5f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        //GameObject.Destroy(myLine, duration);
    }

    public Vector2 LocalPositionToScreenPosition(RectTransform _rectTransform)
    {
        Vector2 screenCenter = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);
        Vector2 output = (Vector2)Camera.main.WorldToScreenPoint(_rectTransform.position);// - screenCenter;

        Debug.Log(output);
        return output;
    }
}
