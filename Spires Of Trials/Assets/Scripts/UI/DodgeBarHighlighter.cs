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
            float alpha = activePositions.Contains(i) ? 1f : 0f;

            // Update main Image
            SetImageAlpha(positionHighlights[i], alpha);

            // Update child SpriteRenderers
            foreach (SpriteRenderer childSprite in positionHighlights[i].GetComponentsInChildren<SpriteRenderer>(true))
            {
                SetSpriteAlpha(childSprite, alpha);
            }
        }
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
        }
    }

    private void SetSpriteAlpha(SpriteRenderer sprite, float alpha)
    {
        if (sprite != null)
        {
            Color color = sprite.color;
            sprite.color = new Color(color.r, color.g, color.b, alpha);
        }
    }
}
