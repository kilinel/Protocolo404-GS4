using System;
using System.Collections;
using UnityEngine;

public class AtendimentoController : MonoBehaviour
{
    [Serializable]
    public class Fala
    {
        [TextArea(2, 5)] public string texto;
        public bool jogador;

        public float largura = 300;
        public float altura = 70;
        public float duracao = 6;
    }

    [Header("Objetos da cena")]
    public RectTransform retratoCidadao;
    public ChatGuiche chat;
    public GameObject documentos;


    [Header("Chegada")]
    public Vector2 deslocamentoInicial = new Vector2(700, 0);
    [Min(0.1f)] public float tempoChegada = 0.8f;
    [Min(0)] public float pausaDepoisDaChegada = 0.5f;

    [Header("Conversa")]
    [Min(0.1f)] public float intervaloEntreFalas = 3;
    public Fala[] falas;


    [Header("Vozes")]
    public AudioSource audioVoz;
    public AudioClip[] vozesAna;
    public AudioClip[] vozesNaldir;
    private int ultimaVozAna = -1;
    private int ultimaVozNaldir = -1;


    private Vector2 posicaoFinal;
    private bool posicaoSalva;

    private void OnEnable()
    {
        if (retratoCidadao == null || chat == null || documentos == null)
        {
           
            return;
        }

        posicaoFinal = retratoCidadao.anchoredPosition;
        posicaoSalva = true;

        documentos.SetActive(false);
        chat.Limpar();

        retratoCidadao.anchoredPosition =
            posicaoFinal + deslocamentoInicial;

        StartCoroutine(AtendimentoInicial());
    }

    private IEnumerator AtendimentoInicial()
    {
        yield return Chegar();

        yield return new WaitForSecondsRealtime(
            pausaDepoisDaChegada
        );

        if (falas != null)
        {
            foreach (Fala fala in falas)
            {
                if (fala == null || string.IsNullOrWhiteSpace(fala.texto))
                    continue;

                chat.Mostrar(
                    fala.texto,
                    fala.jogador,
                    fala.largura,
                    fala.altura,
                    fala.duracao
                );

                TocarVoz(fala.jogador);

                yield return new WaitForSecondsRealtime(
                    intervaloEntreFalas
                );
            }
        }

        documentos.SetActive(true);
    }

    private IEnumerator Chegar()
    {
        Vector2 inicio = retratoCidadao.anchoredPosition;
        float tempo = 0;

        while (tempo < tempoChegada)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / Mathf.Max(0.1f, tempoChegada)
            );

            progresso = Mathf.SmoothStep(0, 1, progresso);

            retratoCidadao.anchoredPosition =
                Vector2.Lerp(inicio, posicaoFinal, progresso);

            yield return null;
        }

        retratoCidadao.anchoredPosition = posicaoFinal;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (posicaoSalva && retratoCidadao != null)
            retratoCidadao.anchoredPosition = posicaoFinal;
    }

    private void TocarVoz(bool jogador)
    {
        AudioClip[] opcoes = jogador ? vozesNaldir : vozesAna;

        if (audioVoz == null || opcoes == null || opcoes.Length == 0)
            return;

        int anterior = jogador ? ultimaVozNaldir : ultimaVozAna;
        int indice;

        if (opcoes.Length == 1)
        {
            indice = 0;
        }
        else if (anterior >= 0 && anterior < opcoes.Length)
        {
            // Sorteia entre todos, exceto o índice anterior.
            indice = UnityEngine.Random.Range(0, opcoes.Length - 1);

            if (indice >= anterior)
                indice++;
        }
        else
        {
            indice = UnityEngine.Random.Range(0, opcoes.Length);
        }

        if (opcoes[indice] == null)
            return;

        if (jogador)
            ultimaVozNaldir = indice;
        else
            ultimaVozAna = indice;

        //audioVoz.Stop();
        audioVoz.PlayOneShot(opcoes[indice]);
    }
}