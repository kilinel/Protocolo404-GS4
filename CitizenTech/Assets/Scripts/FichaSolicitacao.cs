using TMPro;
using UnityEngine;

public class FichaSolicitacao : MonoBehaviour
{
    public enum TipoEncaminhamento
    {
        Nenhum,
        Regular,
        Assistido,
        Regularizacao
    }

    public bool Carimbada { get; private set; }

    public void RegistrarCarimbo()
    {
        if (isActiveAndEnabled)
            Carimbada = true;
    }

    [Header("Campo da ficha")]
    public TMP_Text textoEncaminhamento;

    [Header("Som de preenchimento")]
    public AudioSource audioEscrita;
    public AudioClip somEscrita;

    public TipoEncaminhamento Escolhido { get; private set; }

    private void OnEnable()
    {
        Carimbada = false;
        Escolhido = TipoEncaminhamento.Nenhum;

        if (textoEncaminhamento != null)
            textoEncaminhamento.text = "Não preenchido";
    }

    public void EscolherRegular()
    {
        Preencher(
            TipoEncaminhamento.Regular,
            "Prosseguir pelo portal"
        );
    }

    public void EscolherAssistido()
    {
        Preencher(
            TipoEncaminhamento.Assistido,
            "Atendimento assistido presencial"
        );
    }

    public void EscolherRegularizacao()
    {
        Preencher(
            TipoEncaminhamento.Regularizacao,
            "Regularização cadastral"
        );
    }

    private void Preencher(
        TipoEncaminhamento opcao,
        string texto)
    {
        if (!isActiveAndEnabled)
            return;

        if (textoEncaminhamento == null)
        {
            Debug.LogError(
                "Preencha o campo Texto Encaminhamento na ficha.",
                this
            );
            return;
        }

        if (Escolhido == opcao)
            return;

        Escolhido = opcao;
        textoEncaminhamento.text = texto;

        if (audioEscrita != null && somEscrita != null)
        {
            audioEscrita.Stop();
            audioEscrita.PlayOneShot(somEscrita);
        }
    }
}