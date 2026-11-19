using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextoDigitado : MonoBehaviour
{
    public TMP_Text texto;
    public Button botaoAoTerminar;
    public AudioSource audioTyping;

    [Min(1)] public float letrasPorSegundo = 35;

    private bool iniciouAudio;

    private void OnEnable()
    {
        if (texto == null || botaoAoTerminar == null)
        {
           
            return;
        }

        texto.maxVisibleCharacters = 0;
        botaoAoTerminar.gameObject.SetActive(false);

        StartCoroutine(Digitar());
    }

    private IEnumerator Digitar()
    {
        
        yield return null;

        texto.ForceMeshUpdate();
        int total = texto.textInfo.characterCount;

        if (total > 0 && audioTyping != null &&
            audioTyping.clip != null)
        {
            iniciouAudio = true;
            audioTyping.Play();
        }

        float quantidade = 0;

        while (quantidade < total)
        {
            quantidade += Mathf.Max(1, letrasPorSegundo)
                        * Time.unscaledDeltaTime;

            texto.maxVisibleCharacters =
                Mathf.Min(total, Mathf.FloorToInt(quantidade));

            yield return null;
        }

        texto.maxVisibleCharacters = total;
        PararSom();

        botaoAoTerminar.gameObject.SetActive(true);
    }

    private void PararSom()
    {
        if (iniciouAudio && audioTyping != null)
            audioTyping.Stop();

        iniciouAudio = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        PararSom();
    }
}