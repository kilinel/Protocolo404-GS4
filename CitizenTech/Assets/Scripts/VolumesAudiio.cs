using UnityEngine;
using UnityEngine.Audio;


public class VolumesAudiio : MonoBehaviour
{
    public AudioMixer mixer;

    public void Geral(float valor) => Aplicar("VolumeGeral", valor);
    public void SFX(float valor) => Aplicar("VolumeSFX", valor);
    public void Musica(float valor) => Aplicar("VolumeMusica", valor);
    public void Dialogos(float valor) => Aplicar("VolumeDialogos", valor);


    private void Aplicar(string parametro, float valor)
    {


        if (mixer == null)
        {
            Debug.LogError("Arraste o MixerPrincipal para o campo Mixer.", this);
            return;
        }


        valor = Mathf.Clamp01(valor);
        float decibeis = valor <= 0 ? -80f : Mathf.Log10(valor) * 20f;

        if (!mixer.SetFloat(parametro, decibeis))
            Debug.LogError("Parâmetro de áudio não encontrado: " + parametro, this);
    }
}
