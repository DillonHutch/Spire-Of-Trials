using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages the dodge bar UI highlights, allowing specific positions to be highlighted
/// when an enemy attack is about to land, and clearing them when necessary.
/// </summary>
public class DodgeBarHighlighter : MonoBehaviour
{

    #region Fields
    /// <summary>
    /// Array of UI Image components representing different dodge bar positions.
    /// These should be assigned in the Unity Inspector.
    /// </summary>
    public Image[] positionHighlights;

    /// <summary>
    /// Tracks which positions are currently highlighted.
    /// Uses a HashSet to ensure each position is only stored once.
    /// </summary>
    private HashSet<int> activePositions = new HashSet<int>();

    #endregion

    #region Highlight Management

    /// <summary>
    /// Highlights a specific dodge bar position by making it fully visible.
    /// </summary>
    /// <param name="position">The index of the position to highlight.</param>
    public void HighlightPosition(int position)
    {
        // Ensure the position index is within valid bounds
        if (position < 0 || position >= positionHighlights.Length) return;

        // Add the position to the active highlights set
        activePositions.Add(position);

        // Update the UI to reflect the changes
        UpdateHighlights();
    }

    /// <summary>
    /// Clears the highlight from a specific dodge bar position.
    /// </summary>
    /// <param name="position">The index of the position to remove from highlights.</param>
    public void ClearHighlight(int position)
    {
        // Ensure the position index is within valid bounds
        if (position < 0 || position >= positionHighlights.Length) return;

        // Remove the position from the active highlights set
        activePositions.Remove(position);

        // Update the UI to reflect the changes
        UpdateHighlights();
    }

    #endregion

    #region UI Update Methods

    /// <summary>
    /// Updates the UI by setting the correct alpha (transparency) for each dodge bar position.
    /// Positions in the active set will be fully visible, others will be transparent.
    /// </summary>
    private void UpdateHighlights()
    {
        // Loop through all dodge bar positions
        for (int i = 0; i < positionHighlights.Length; i++)
        {
            // Determine the alpha value: 1 for active positions, 0 for inactive
            float alpha = activePositions.Contains(i) ? 1f : 0f;

            // Update the main UI Image
            SetImageAlpha(positionHighlights[i], alpha);

            // Update any child SpriteRenderers (for additional visual effects)
            foreach (SpriteRenderer childSprite in positionHighlights[i].GetComponentsInChildren<SpriteRenderer>(true))
            {
                SetSpriteAlpha(childSprite, alpha);
            }
        }
    }

    /// <summary>
    /// Sets the transparency of a UI Image.
    /// </summary>
    /// <param name="img">The Image component to modify.</param>
    /// <param name="alpha">The desired transparency level (0 to 1).</param>
    private void SetImageAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
        }
    }

    /// <summary>
    /// Sets the transparency of a SpriteRenderer (used for child effects).
    /// </summary>
    /// <param name="sprite">The SpriteRenderer component to modify.</param>
    /// <param name="alpha">The desired transparency level (0 to 1).</param>
    private void SetSpriteAlpha(SpriteRenderer sprite, float alpha)
    {
        if (sprite != null)
        {
            Color color = sprite.color;
            sprite.color = new Color(color.r, color.g, color.b, alpha);
        }
    }

    #endregion
}
