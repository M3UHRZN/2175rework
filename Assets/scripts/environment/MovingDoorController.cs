using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MovingDoorController : MonoBehaviour
{
    [Tooltip("Hareket ettirilecek parça. Boşsa bu nesne kullanılır.")]
    public Transform hedefParca;
    [Tooltip("Yerel uzayda kapalı pozisyon.")]
    public Vector3 kapaliKonum;
    [Tooltip("Yerel uzayda açık pozisyon.")]
    public Vector3 acikKonum = new Vector3(0f, 3f, 0f);
    [Tooltip("Kapı başlangıçta açık mı?")]
    public bool baslangictaAcik;
    [Tooltip("Açılış/kapanış süresi (saniye).")]
    public float hareketSuresi = 1f;
    [Tooltip("Hareket eğrisi (0-1 arası).")]
    public AnimationCurve hareketEgrisi = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    Coroutine aktifRut;
    bool acik;

    void Reset()
    {
        hedefParca = transform;
        kapaliKonum = Vector3.zero;
        acikKonum = new Vector3(0f, 3f, 0f);
        hareketSuresi = 1f;
    }

    void Awake()
    {
        if (!hedefParca)
            hedefParca = transform;

        if (kapaliKonum == Vector3.zero)
            kapaliKonum = hedefParca.localPosition;
    }

    void Start()
    {
        SetDurum(baslangictaAcik, true);
    }

    public void Ac()
    {
        SetDurum(true);
    }

    public void Kapat()
    {
        SetDurum(false);
    }

    public void Degistir()
    {
        SetDurum(!acik);
    }

    public void SetDurum(bool hedefDurum)
    {
        SetDurum(hedefDurum, false);
    }

    void SetDurum(bool hedefDurum, bool instant)
    {
        if (!hedefParca)
            return;

        if (acik == hedefDurum && !instant)
            return;

        acik = hedefDurum;

        if (aktifRut != null)
            StopCoroutine(aktifRut);

        if (instant || hareketSuresi <= 0f)
        {
            hedefParca.localPosition = acik ? acikKonum : kapaliKonum;
            aktifRut = null;
        }
        else
        {
            aktifRut = StartCoroutine(HareketRutini(acik ? acikKonum : kapaliKonum));
        }
    }

    IEnumerator HareketRutini(Vector3 hedefKonum)
    {
        Vector3 baslangic = hedefParca.localPosition;
        float zaman = 0f;
        float sure = Mathf.Max(0.01f, hareketSuresi);

        while (zaman < sure)
        {
            zaman += Time.deltaTime;
            float t = Mathf.Clamp01(zaman / sure);
            float egriliT = hareketEgrisi != null ? hareketEgrisi.Evaluate(t) : t;
            hedefParca.localPosition = Vector3.LerpUnclamped(baslangic, hedefKonum, egriliT);
            yield return null;
        }

        hedefParca.localPosition = hedefKonum;
        aktifRut = null;
    }

    public bool Acik => acik;
}
