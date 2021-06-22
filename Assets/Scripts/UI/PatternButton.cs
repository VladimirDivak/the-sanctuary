using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PatternButton : MonoBehaviour
{
    private BallCustomize _ballCustomScript;
    public Color _baseColor;
    public Color _selectedColor;
    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
        _ballCustomScript = FindObjectOfType<BallCustomize>();
    }

    public void ButtonPressed()
    {
        if(_image.color != _selectedColor)
        {
            _ballCustomScript.SetPattern(name);
            _image.color = _selectedColor;
            
            foreach(var button in GameObject.FindGameObjectsWithTag("Pattern Button").Where(x => x.name != name))
            {
                button.GetComponent<Image>().color = button.GetComponent<PatternButton>()._baseColor;
            }
        }
    }
}
