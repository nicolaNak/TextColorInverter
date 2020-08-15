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

    private bool _initRectCheck = false;

    private int _iter = 0;


    private Vector3 _textPosition;

    public RenderTexture CameraTexture;

    private Texture2D _textureData;

    private Rect _readData;

    private Color _colorCheck;

    private Color _textUpdatedColour;

    private RectTransform _positionCheckImage;

    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();

        GameObject newCheck = new GameObject("PositionImageCheck");
        newCheck.AddComponent<Image>();
        newCheck.GetComponent<Image>().color = new Color(Color.magenta.r, Color.magenta.g, Color.magenta.b, 0.3f);
        newCheck.GetComponent<Image>().raycastTarget = false;
        newCheck.transform.parent = transform;
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
        if (_iter <= 10)
        {
            _adjustedWidth = _textPosition.x + _text.renderedWidth; //max x value
            _adjustedHeight =_textPosition.y - _text.renderedHeight; //min y value

            _textPosition = _text.rectTransform.anchoredPosition3D;

            _checkXValues = new Vector2Int(Mathf.RoundToInt(_textPosition.x), Mathf.RoundToInt(_adjustedWidth));
            _checkYValues = new Vector2Int(Mathf.RoundToInt(_adjustedHeight), Mathf.RoundToInt(_textPosition.y));

            _iter++;

            //_positionCheck.transform.SetParent(_text.transform.parent, false); 
            //TODO: make image check have parent, move parent to match position and size of text, then change image to rendered width & height
           // _positionCheck.GetComponent<RectTransform>().anchorMin = _text.rectTransform.anchorMin;
            //_positionCheck.GetComponent<RectTransform>().anchorMax = _text.rectTransform.anchorMax;
           // _positionCheck.GetComponent<RectTransform>().sizeDelta = _text.rectTransform.sizeDelta;
            //_positionCheck.GetComponent<RectTransform>().anchoredPosition3D = _textPosition;

            _positionCheckImage.transform.SetParent(_text.transform, false);
            //_positionCheck.GetComponent<RectTransform>().anchorMin = _text.rectTransform.anchorMin;
            //_positionCheck.GetComponent<RectTransform>().anchorMax = _text.rectTransform.anchorMax;
            _positionCheckImage.sizeDelta = new Vector2(_text.renderedWidth, _text.renderedHeight);
            //X pos adjusted by taking parent position and subtracting rendered width to bring in line with top of UI element
            //Y pos adjusted by splitting parent height and subtracting split of rendered height to line up with left side of UI element
            _positionCheckImage.anchoredPosition3D = new Vector3((Mathf.Abs(_textPosition.x) - _text.renderedWidth), (_text.rectTransform.sizeDelta.y * 0.5f - _text.renderedHeight * 0.5f), 0f);
            //can this be done without parent object?

        }

        RenderTexture.active = CameraTexture;

        _textureData.ReadPixels(_readData, 0, 0);
        _textureData.Apply();

        RenderTexture.active = null;

        _text.color = UpdateTextColor(false);
        //_checkImage.color = _textUpdatedColour;
        
    }

    private Color UpdateTextColor(bool greyscale, bool inverse = true)
    {
        Color colorCheck = Color.white;
        Color updatedColor = Color.black;

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
            }
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

        return updatedColor;
    }

    void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        //GameObject.Destroy(myLine, duration);
    }
}
