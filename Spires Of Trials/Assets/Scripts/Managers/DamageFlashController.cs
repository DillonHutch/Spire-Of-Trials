using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DamageFlashController : MonoBehaviour
{
    
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private Image flashImage;

    private void OnEnable()
    {
        EventManager.Instance.StartListening<int>("takeDamageEvent", OnTakeDamage);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening<int>("takeDamageEvent", OnTakeDamage);
    }

    private void OnTakeDamage(int dmg)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashImage.color = new Color(1, 0, 0, 0.5f);
        yield return new WaitForSeconds(flashDuration);
        while (flashImage.color.a > 0)
        {
            var c = flashImage.color;
            c.a -= fadeSpeed * Time.deltaTime;
            flashImage.color = c;
            yield return null;
        }
    }
}
