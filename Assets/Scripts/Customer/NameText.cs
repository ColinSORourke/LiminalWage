using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class NameText : TextMeshManager
{
    public void Construct(Color color, string name)
    {
        base.Construct();
        ChangeColor(color);
        ChangeText(name);
    }
}
