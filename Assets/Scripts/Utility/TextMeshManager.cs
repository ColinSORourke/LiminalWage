using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Utility
{
    public class TextMeshManager : MonoBehaviour
    {
        protected TextMeshPro textMesh;

        protected void Construct()
        {
            textMesh = gameObject.GetComponent<TextMeshPro>();
        }

        public void ChangeText(string text)
        {
            if(text == null)
            {
                textMesh.text = "";
            }
            else
            {
                textMesh.text = text;
            }
        }

        public void ChangeText(int text)
        {
            textMesh.text = text.ToString();
        }

        public void ChangeColor(Color color)
        {
            if(color == null)
            {
                textMesh.color = Color.magenta;
            }
            else
            {
                textMesh.color = color;
            }
        }
    }
}