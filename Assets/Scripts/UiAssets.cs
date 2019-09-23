using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiAssets : MonoBehaviour
{
    private static UiAssets _i;
    
    public PointsPopup PointsPopup;
    
    public static UiAssets i
    {
        get
        {
            if (_i == null)
            {
                _i = Instantiate(Resources.Load<UiAssets>("UIAssets"));
            }
            
            return _i;
        }
    }

}