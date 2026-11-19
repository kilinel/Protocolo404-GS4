using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class FluxoInicial : MonoBehaviour
{
    public const string ChaveIntroducao = "Protocolo404.IntroducaoVista.v1";

    [Serializable]
    public class Trecho
    {
        public Sprite imagem;
        [TextArea(2, 6)] public string texto;
    }

    [Header("Objetos da interface")]
    public GameObject painelIntroducao;
    public GameObject painelMenu;
    public GameObject painelCircular;
    public GameObject painelGameplay;
    public GameObject painelFimPrototipo;
    public Image imagemIntroducao;
    public TMP_Text textoIntroducao;
    public Button botaoContinuar;

    [Header("Aúdio")]
    public AudioSource audioDigitando;
    public AudioSource audioClique;
    public AudioClip somClique;




    [Header("Uma imagem e um texto por trecho")]
    [Min(1)] public float letrasPorSegundo = 35;
    public Trecho[] trechos =
    {
        new Trecho { texto = "O ano é 2032." },
        new Trecho { texto = "A prefeitura mudou a forma de atender. Agora, quase tudo começa pelo portal. A promessa era resolver as coisas sem sair de casa." },
        new Trecho { texto = "Mas nem todo mundo tem internet, um celular compatível ou facilidade para usar o sistema." },
        new Trecho { texto = "Então, eles se prepararam criando a Central do Cidadão: um posto público para orientar e ajudar quem precisa de atendimento presencial." },
        new Trecho { texto = "E adivinha?" },
        new Trecho { texto = "Hoje é meu primeiro dia trabalhando nela." }
    };

    private int trechoAtual;
    private bool digitando;
    private Coroutine animacao;
    private bool partidaIniciada;

    private void Start()
    {
        if (painelIntroducao == null || painelMenu == null || painelCircular == null ||
            imagemIntroducao == null || textoIntroducao == null || botaoContinuar == null)
        {
            Debug.LogError("FluxoInicial: preencha os seis objetos da interface no Inspector.", this);
            enabled = false;
            return;
        }

        if (PlayerPrefs.GetInt(ChaveIntroducao, 0) == 1)
            MostrarMenu();
        else
            ReverIntroducao();
    }

    public void ReverIntroducao()
    {
        PararAnimacao();
        painelMenu.SetActive(false);
        painelCircular.SetActive(false);
        painelIntroducao.SetActive(true);
        trechoAtual = 0;
        MostrarTrecho();
    }

    public void Continuar()
    {
        if (!painelIntroducao.activeSelf || digitando)
            return;

        trechoAtual++;
        if (trechos != null && trechoAtual < trechos.Length)
        {
            MostrarTrecho();
            return;
        }

        // Salvar só ao concluir evita pular uma introdução interrompida.
        PlayerPrefs.SetInt(ChaveIntroducao, 1);
        PlayerPrefs.Save();
        MostrarMenu();
    }

    private void MostrarTrecho()
    {
        PararAnimacao();
        if (trechos == null || trechos.Length == 0)
        {
            Debug.LogWarning("FluxoInicial: adicione pelo menos um trecho da introdução.", this);
            MostrarMenu();
            return;
        }

        Trecho trecho = trechos[trechoAtual];
        imagemIntroducao.sprite = trecho == null ? null : trecho.imagem;
        imagemIntroducao.enabled = imagemIntroducao.sprite != null;
        imagemIntroducao.preserveAspect = true;
        textoIntroducao.text = trecho == null ? "" : trecho.texto ?? "";
        textoIntroducao.maxVisibleCharacters = 0;
        botaoContinuar.gameObject.SetActive(false);
        digitando = true;
        animacao = StartCoroutine(RevelarTexto());
    }

    private IEnumerator RevelarTexto()
    {
        textoIntroducao.ForceMeshUpdate();
        int total = textoIntroducao.textInfo.characterCount;

        if (total > 0 && audioDigitando != null)
            audioDigitando.Play();

        float visiveis = 0;
        while (visiveis < total)
        {
            visiveis += Mathf.Max(1, letrasPorSegundo) * Time.unscaledDeltaTime;
            textoIntroducao.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(visiveis));
            yield return null;
        }

        if (audioDigitando != null)
            audioDigitando.Stop();

        textoIntroducao.maxVisibleCharacters = total;
        digitando = false;
        botaoContinuar.gameObject.SetActive(true);
        animacao = null;
    }

    public void MostrarMenu()
    {
        if (partidaIniciada)
        {
            ReiniciarPrototipo();
            return;
        }
        PararAnimacao();
        painelGameplay.SetActive(false);
        painelIntroducao.SetActive(false);
        painelCircular.SetActive(false);
        painelMenu.SetActive(true);
        painelFimPrototipo.SetActive(false);
    }

    public void Jogar()
    {
        PararAnimacao();
        painelIntroducao.SetActive(false);
        painelMenu.SetActive(false);
        painelCircular.SetActive(true);
    }

    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    [ContextMenu("Save/Limpar somente introdução vista")]
    public void LimparIntroducaoVista()
    {
        PlayerPrefs.DeleteKey(ChaveIntroducao);
        PlayerPrefs.Save();
    }

    private void PararAnimacao()
    {
        if (audioDigitando != null)
            audioDigitando.Stop();

        if (animacao != null)
            StopCoroutine(animacao);
        animacao = null;
        digitando = false;
    }

    private void OnDisable()
    {
        PararAnimacao();
    }

    public void TocarClique()
    {
        if (audioClique != null && somClique != null)
            audioClique.PlayOneShot(somClique);
    }

    public void IniciarExpediente()
    {
        partidaIniciada = true;
        PararAnimacao();
        painelIntroducao.SetActive(false);
        painelMenu.SetActive(false);
        painelCircular.SetActive(false);
        painelGameplay.SetActive(true);
    }

    public void MostrarFimPrototipo()
    {
        PararAnimacao();

        painelIntroducao.SetActive(false);
        painelMenu.SetActive(false);
        painelCircular.SetActive(false);
        painelGameplay.SetActive(false);

        painelFimPrototipo.SetActive(true);
    }

    public void ReiniciarPrototipo()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
