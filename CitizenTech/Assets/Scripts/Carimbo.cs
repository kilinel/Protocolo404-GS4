using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class Carimbo : MonoBehaviour
{
    public RectTransform carimboFlutuante;
    public GameObject marcaCarimboPrefab;
    public Animator animadorCarimbo;

    public float tempoAteEncostar = .15f;
    public float tempoTotalAnimacao = .3f;

    private bool segurandoCarimbo = false;
    private bool animando = false;

    [Header("Sons")]
    public AudioSource audioSFX;
    public AudioClip somPegar;
    public AudioClip somCarimbar;
    void Update()
    {
        if (segurandoCarimbo && !animando && Mouse.current != null)
        {
            Vector2 posMouse = Mouse.current.position.ReadValue();
            carimboFlutuante.position = posMouse;

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                SoltarCarimbo();
            }
        }
    }

    public void PegarCarimbo()
    {
        if (segurandoCarimbo || animando)
            return;

        if (audioSFX != null && somPegar != null)
            audioSFX.PlayOneShot(somPegar);
        segurandoCarimbo = true;
        carimboFlutuante.gameObject.SetActive(true);
        carimboFlutuante.SetAsLastSibling();

        if (Mouse.current != null)
        {
            carimboFlutuante.position = Mouse.current.position.ReadValue();
        }
    }

    public void AplicarCarimboNaFolha(RectTransform folhaClicada)
    {
        if (!segurandoCarimbo || animando || Mouse.current == null) return;


        Vector2 posicaodoClique = Mouse.current.position.ReadValue();
        StartCoroutine(RotinaCarimbar(posicaodoClique, folhaClicada));

        
    }

    private IEnumerator RotinaCarimbar(Vector3 posicaoClique, RectTransform folhaAlvo)
    {
        animando = true;
        carimboFlutuante.position = posicaoClique;

        if (animadorCarimbo != null)
        {
            animadorCarimbo.SetTrigger("Carimbar");
        }
       

        yield return new WaitForSeconds(tempoAteEncostar);

        GameObject novaMarca = Instantiate(marcaCarimboPrefab, folhaAlvo, false);

        novaMarca.transform.position = posicaoClique + new Vector3(0f,30f,0f);

        var ficha = folhaAlvo.GetComponentInParent<FichaSolicitacao>();

        if (ficha != null)
            ficha.RegistrarCarimbo();

        if (audioSFX != null && somCarimbar != null)
            audioSFX.PlayOneShot(somCarimbar);

        float tempoRestante = Mathf.Max(0, tempoTotalAnimacao - tempoAteEncostar);
        yield return new WaitForSeconds(tempoRestante);

        animando = false;

        SoltarCarimbo();
    }

    public void SoltarCarimbo()
    {
        if (animando) return;
        segurandoCarimbo = false;
        carimboFlutuante.gameObject.SetActive(false);
    }
}



