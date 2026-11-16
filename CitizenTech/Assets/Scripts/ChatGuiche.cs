using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatGuiche : MonoBehaviour
{
    [Header("Prefab: Image na raiz e texto TMP dentro")]
    public RectTransform prefabBolha;

    [Header("Posições")]
    public float xJogador = -150;
    public float xNPC = 670;
    public float yInicial = 0;
    public float limiteY = -290;
    public float espacamento = 20;

    [Header("Cores")]
    public Color corJogador = new Color32(55, 80, 95, 255);
    public Color corNPC = new Color32(100, 75, 60, 255);

    private class Mensagem
    {
        public RectTransform bolha;
        public bool jogador;
        public float expiraEm;
    }

    private readonly List<Mensagem> mensagens = new();

    public void Mostrar(
        string texto,
        bool jogador,
        float largura = 300,
        float altura = 70,
        float duracao = 6)
    {
        if (prefabBolha == null ||
            prefabBolha.GetComponent<UnityEngine.UI.Graphic>() == null ||
            prefabBolha.GetComponentInChildren<TMP_Text>(true) == null)
        {
           
            return;
        }

        if (largura <= 0 || altura <= 0 || duracao <= 0 ||
            altura > yInicial - limiteY || espacamento < 0)
        {
          
            return;
        }

        var bolha = Instantiate(prefabBolha, transform);
        bolha.gameObject.SetActive(true);

       
        
        bolha.localScale = new Vector3(2f, 2f, 2f);
        bolha.sizeDelta = new Vector2(largura, altura);

        bolha.GetComponent<UnityEngine.UI.Graphic>().color =
            jogador ? corJogador : corNPC;

        var textoBolha = bolha.GetComponentInChildren<TMP_Text>(true);
        var rectTexto = textoBolha.rectTransform;

        bolha.anchorMin = new Vector2(0, 1);
        bolha.anchorMax = new Vector2(0, 1);
        bolha.pivot = jogador
     ? new Vector2(0, 1)   
     : new Vector2(1, 1);  

        rectTexto.localScale = Vector3.one;
        rectTexto.localRotation = Quaternion.identity;

        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.pivot = new Vector2(0.5f, 0.5f);

       
        rectTexto.offsetMin = new Vector2(12, 8);
        rectTexto.offsetMax = new Vector2(-12, -8);

        textoBolha.enableAutoSizing = false;
        textoBolha.color = Color.white;
        textoBolha.text = texto;

        mensagens.Add(new Mensagem
        {
            bolha = bolha,
            jogador = jogador,
            expiraEm = Time.unscaledTime + duracao
        });

        Organizar();
    }

    private void Update()
    {
        bool mudou = false;

        for (int i = mensagens.Count - 1; i >= 0; i--)
        {
            if (Time.unscaledTime >= mensagens[i].expiraEm)
            {
                Remover(i);
                mudou = true;
            }
        }

        if (mudou)
            Organizar();
    }

    private void Organizar()
    {
        float alturaTotal = 0;

        foreach (var mensagem in mensagens)
        {
            alturaTotal += mensagem.bolha.sizeDelta.y
                         * mensagem.bolha.localScale.y;
        }

        alturaTotal += Mathf.Max(0, mensagens.Count - 1) * espacamento;

        // Remove a mais antiga somente quando falta espaço.
        while (mensagens.Count > 0 &&
               yInicial - alturaTotal < limiteY)
        {
            alturaTotal -= mensagens[0].bolha.sizeDelta.y
                         * mensagens[0].bolha.localScale.y;

            Remover(0);

            if (mensagens.Count > 0)
                alturaTotal -= espacamento;
        }

        float y = yInicial;

        foreach (var mensagem in mensagens)
        {
            float x = mensagem.jogador ? xJogador : xNPC;

            mensagem.bolha.anchoredPosition = new Vector2(x, y);

            float alturaVisivel = mensagem.bolha.sizeDelta.y
                                * mensagem.bolha.localScale.y;

            y -= alturaVisivel + espacamento;
        }
    }

    private void Remover(int indice)
    {
        mensagens[indice].bolha.gameObject.SetActive(false);
        Destroy(mensagens[indice].bolha.gameObject);
        mensagens.RemoveAt(indice);
    }

    public void Limpar()
    {
        while (mensagens.Count > 0)
            Remover(0);
    }

    private void OnDisable()
    {
        Limpar();
    }

 
}