using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class SineWaveText : MonoBehaviour
{
    public float waveAmplitude = 5f;
    public float waveFrequency = 1f;
    public float waveSpeed = 2f;

    private TextMeshProUGUI tmp;
    private Mesh mesh;
    private Vector3[] vertices;

    private HashSet<int> wavyIndices = new HashSet<int>();

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        StartCoroutine(AnimateVertexWave());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator AnimateVertexWave()
    {
        while (true)
        {
            tmp.ForceMeshUpdate();
            TMP_TextInfo textInfo = tmp.textInfo;
            mesh = tmp.mesh;
            vertices = mesh.vertices;

            float time = Time.time * waveSpeed;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int vertexIndex = charInfo.vertexIndex;
                Vector3 offset = Vector3.zero;

                if (wavyIndices.Contains(i))
                {
                    offset.y = Mathf.Sin(time + i * waveFrequency) * waveAmplitude;
                }

                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] += offset;
                }
            }

            mesh.vertices = vertices;
            tmp.canvasRenderer.SetMesh(mesh);

            yield return new WaitForSeconds(0.016f); // ~60fps
        }
    }

    /// <summary>
    /// Must be called after dialogueText.text is set and before Enable().
    /// It strips [wave] tags and stores animated character indices.
    /// </summary>
    public void PrepareWaveText(string rawText)
    {
        wavyIndices.Clear();

        // Track output string and char index mapping
        List<int> indicesToWave = new List<int>();
        System.Text.StringBuilder cleanText = new System.Text.StringBuilder();
        bool inWave = false;

        for (int i = 0, outputIndex = 0; i < rawText.Length; i++)
        {
            if (rawText.Substring(i).StartsWith("[wave]"))
            {
                inWave = true;
                i += "[wave]".Length - 1;
                continue;
            }
            else if (rawText.Substring(i).StartsWith("[/wave]"))
            {
                inWave = false;
                i += "[/wave]".Length - 1;
                continue;
            }

            if (inWave) indicesToWave.Add(outputIndex);
            cleanText.Append(rawText[i]);
            outputIndex++;
        }

        tmp.text = cleanText.ToString();

        foreach (var idx in indicesToWave)
            wavyIndices.Add(idx);
    }
}
