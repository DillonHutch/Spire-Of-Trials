using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DodgeBarHighlighter : MonoBehaviour
{
    public Image[] positionHighlights; // Assign UI images in the Inspector
    private HashSet<int> activePositions = new HashSet<int>(); // Track highlighted positions

    public void HighlightPosition(int position)
    {
        if (position < 0 || position >= positionHighlights.Length) return;

        activePositions.Add(position);
        UpdateHighlights();
    }

    public void ClearHighlight(int position)
    {
        if (position < 0 || position >= positionHighlights.Length) return;

        activePositions.Remove(position);
        UpdateHighlights();
    }

    private void UpdateHighlights()
    {
        // Reset all highlights first
        for (int i = 0; i < positionHighlights.Length; i++)
        {
            if (activePositions.Contains(i))
            {
                positionHighlights[i].color = new Color(positionHighlights[i].color.r,
                                                        positionHighlights[i].color.g,
                                                        positionHighlights[i].color.b,
                                                        1); // Fully visible
            }
            else
            {
                positionHighlights[i].color = new Color(positionHighlights[i].color.r,
                                                        positionHighlights[i].color.g,
                                                        positionHighlights[i].color.b,
                                                        0); // Fully transparent
            }
        }
    }
}
