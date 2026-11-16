using UnityEngine;
using UnityEngine.EventSystems;

public class DocumentoArrastavel : MonoBehaviour, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{

    private Carimbo sistemaCarimbo;
    private RectTransform meuRectTransform;

    [Header("Visuais deste documento")]
    public GameObject visualBalcao;
    public GameObject visualInspecao;

    [Header("Áreas da cena")]
    public RectTransform areaBalcao;
    public RectTransform areaInspecao;
    public RectTransform areaGuiche;

    private RectTransform documento;
    private RectTransform pai;

    private Vector3 posicaoAnterior;
    private Vector3 distanciaDoMouse;
    private bool estavaNaInspecao;
    private bool naInspecao;
    private bool arrastando;

    [Header("Feedback visual")]
    public GameObject destaqueInspecao;

    private void Awake()
    {
        documento = GetComponent<RectTransform>();
        pai = transform.parent as RectTransform;
    }

    void Start()
    {
        sistemaCarimbo = FindAnyObjectByType<Carimbo>();
        meuRectTransform = GetComponent<RectTransform>();
    }
    public void OnBeginDrag(PointerEventData evento)
    {
        if (documento == null || pai == null ||
            visualBalcao == null || visualInspecao == null ||
            areaBalcao == null || areaInspecao == null ||
            areaGuiche == null)
        {
           
            return;
        }

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
            pai, evento.position, evento.pressEventCamera,
            out Vector3 mouse))
            return;

        posicaoAnterior = documento.position;
        estavaNaInspecao = naInspecao;
        distanciaDoMouse = documento.position - mouse;
        arrastando = true;

        if (destaqueInspecao != null)
            destaqueInspecao.SetActive(true);

        pai.SetAsLastSibling();
        documento.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData evento)
    {
        if (!arrastando)
            return;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            pai, evento.position, evento.pressEventCamera,
            out Vector3 mouse))
        {
            documento.position = mouse + distanciaDoMouse;
        }

        TrocarVisual(EstaDentro(areaInspecao, evento));
    }

    public void OnEndDrag(PointerEventData evento)
    {
        if (!arrastando)
            return;

       
        OnDrag(evento);
        arrastando = false;

        if (destaqueInspecao != null)
            destaqueInspecao.SetActive(false);

        if (EstaDentro(areaInspecao, evento))
        {
            TrocarVisual(true);
        }
        else if (EstaDentro(areaGuiche, evento))
        {
            // Devolvido ao cidadão.
            gameObject.SetActive(false);
        }
        else if (EstaDentro(areaBalcao, evento))
        {
            TrocarVisual(false);
        }
        else
        {
            documento.position = posicaoAnterior;
            TrocarVisual(estavaNaInspecao);
        }
    }

    private bool EstaDentro(
        RectTransform area,
        PointerEventData evento)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            area, evento.position, evento.pressEventCamera
        );
    }

    private void TrocarVisual(bool inspecao)
    {
        naInspecao = inspecao;
        visualBalcao.SetActive(!inspecao);
        visualInspecao.SetActive(inspecao);
    }

    private void OnDisable()
    {
        if(destaqueInspecao != null)
            destaqueInspecao.SetActive(false);
        if (!arrastando)
            return;

        arrastando = false;
        documento.position = posicaoAnterior;
        TrocarVisual(estavaNaInspecao);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!naInspecao || eventData.button != PointerEventData.InputButton.Left || eventData.dragging)
            return;

        RectTransform rectInspecao = visualInspecao.GetComponent<RectTransform>();

        bool clicouNoVisualInspecao = RectTransformUtility.RectangleContainsScreenPoint(
               rectInspecao,
               eventData.position,
               eventData.pressEventCamera
        );

        if (clicouNoVisualInspecao && sistemaCarimbo != null)
        {
            sistemaCarimbo.AplicarCarimboNaFolha(rectInspecao);
        }

    }
}