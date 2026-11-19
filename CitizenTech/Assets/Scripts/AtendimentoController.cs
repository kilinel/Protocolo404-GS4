using System;
using System.Collections;
using UnityEngine;
using TMPro;

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

    [Header("Encerramento")]
    public DocumentoArrastavel documentoRG;
    public DocumentoArrastavel documentoFicha;
    public FichaSolicitacao fichaSolicitacao;
    public FluxoInicial fluxoInicial;

    [Header("Aviso de erro")]
    public GameObject report;
    private TMP_Text reportEncaminhamento;
    private TMP_Text reportEncaminhamentoEsperado;
    private TMP_Text reportDescricao;
    public AudioSource audioReport;
    public AudioClip somReport;

    private bool rgEntregue;
    private bool fichaEntregue;
    private bool encerrando;
    private bool aguardandoAviso;

    public RectTransform pontoReport;
    private GameObject reportCriado;

    private void OnEnable()
    {
        if (retratoCidadao == null || chat == null || documentos == null)
        {
           
            return;
        }

        rgEntregue = false;
        fichaEntregue = false;
        encerrando = false;
        aguardandoAviso = false;

        if (reportCriado != null)
            Destroy(reportCriado);
       

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

    public void DocumentoEntregue(DocumentoArrastavel entregue)
    {
        if (encerrando)
            return;

        if (entregue == documentoRG)
            rgEntregue = true;

        if (entregue == documentoFicha)
            fichaEntregue = true;

        if (!rgEntregue || !fichaEntregue)
            return;

        if (fichaSolicitacao == null || fluxoInicial == null)
        {
           
            return;
        }

        bool acertou =
        fichaSolicitacao.Escolhido ==
            FichaSolicitacao.TipoEncaminhamento.Assistido
        && fichaSolicitacao.Carimbada;

        if (!acertou && (report == null || pontoReport == null))
        {
            
            return;
        }

        encerrando = true;
        StartCoroutine(EncerrarAtendimento());
    }

    private IEnumerator EncerrarAtendimento()
    {
       
        var escolhido = fichaSolicitacao.Escolhido;
        bool carimbada = fichaSolicitacao.Carimbada;

        chat.Limpar();

        Vector2 inicio = retratoCidadao.anchoredPosition;
        Vector2 destino = inicio + deslocamentoInicial;
        float tempo = 0;

        while (tempo < tempoChegada)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.SmoothStep(
                0, 1,
                Mathf.Clamp01(tempo / Mathf.Max(0.1f, tempoChegada))
            );

            retratoCidadao.anchoredPosition =
                Vector2.Lerp(inicio, destino, progresso);

            yield return null;
        }

        retratoCidadao.anchoredPosition = destino;

        if (escolhido ==
         FichaSolicitacao.TipoEncaminhamento.Assistido
     && carimbada)
        {
            fluxoInicial.MostrarFimPrototipo();
            yield break;
        }
        reportCriado = Instantiate(report, pontoReport, false);

        var rect = reportCriado.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        reportEncaminhamento = reportCriado.transform
            .Find("VisualInspecao/EncReg")
            .GetComponent<TMP_Text>();

       reportEncaminhamentoEsperado = reportCriado.transform
            .Find("VisualInspecao/EncEsperado")
            .GetComponent<TMP_Text>();

        reportDescricao = reportCriado.transform
            .Find("VisualInspecao/Desc")
            .GetComponent<TMP_Text>();

        var arraste = reportCriado.GetComponent<DocumentoArrastavel>();
        arraste.areaBalcao = documentoRG.areaBalcao;
        arraste.areaInspecao = documentoRG.areaInspecao;
        arraste.areaGuiche = documentoRG.areaGuiche;
        arraste.audioSFX = documentoRG.audioSFX;
        arraste.destaqueInspecao = documentoRG.destaqueInspecao;

        var continuar = reportCriado
    .GetComponentInChildren<UnityEngine.UI.Button>(true);

        continuar.onClick.RemoveAllListeners();
        continuar.onClick.AddListener(ContinuarDepoisDoAviso);

        string registrado;

        switch (escolhido)
        {
            case FichaSolicitacao.TipoEncaminhamento.Regular:
                registrado = "Fluxo digital regular";
                break;

            case FichaSolicitacao.TipoEncaminhamento.Regularizacao:
                registrado = "Regularização cadastral";
                break;

            default:
                registrado = "Não preenchido";
                break;
        }

        reportEncaminhamento.text =
            "Encaminhamento registrado:" + registrado;

        reportEncaminhamentoEsperado.text =
         "Encaminhamento esperado:Atendimento assistido";

        string erros = "";

        if (escolhido == FichaSolicitacao.TipoEncaminhamento.Nenhum)
        {
            erros += "A ficha foi devolvida sem encaminhamento.\n";
        }
        else if (escolhido !=
                 FichaSolicitacao.TipoEncaminhamento.Assistido)
        {
            erros += "O encaminhamento não resolve a barreira de acesso.\n";
        }

        if (!carimbada)
        {
            erros += "A ficha foi devolvida sem o carimbo do atendente.\n";
        }

        erros += "O caso exigia atendimento assistido.";

        reportDescricao.text = erros;

        aguardandoAviso = true;
        reportCriado.SetActive(true);
        pontoReport.SetAsLastSibling();

        if (audioReport != null && somReport != null)
            audioReport.PlayOneShot(somReport);
    }

    public void ContinuarDepoisDoAviso()
    {
        if (!aguardandoAviso)
            return;

        aguardandoAviso = false;
        fluxoInicial.MostrarFimPrototipo();
    }

}