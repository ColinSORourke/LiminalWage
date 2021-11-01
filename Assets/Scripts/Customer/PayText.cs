using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class PayText : TextMeshManager
{
    public void Construct(Color color, int pay)
    {
        base.Construct();
        ChangeColor(color);
        ChangeText(pay);
    }
}
