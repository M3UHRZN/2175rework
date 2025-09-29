using UnityEngine;

public class SimpleLightController : MonoBehaviour
{
    [Tooltip("Kontrol edilecek Işık bileşeni. Boş bırakılırsa aynı nesnede aranır.")]
    public Light hedefIsik;
    [Tooltip("Açık/Kapalı durumuna göre etkinleştirilecek bir obje (isteğe bağlı).")]
    public GameObject hedefGoruntu;
    [Tooltip("Başlangıçta ışık açık olsun mu?")]
    public bool baslangictaAcik;

    bool acik;

    void Awake()
    {
        if (!hedefIsik)
            hedefIsik = GetComponentInChildren<Light>();
        if (!hedefGoruntu && hedefIsik)
            hedefGoruntu = hedefIsik.gameObject;

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

    public void SetDurum(bool acikDurum)
    {
        SetDurum(acikDurum, false);
    }

    void SetDurum(bool acikDurum, bool force)
    {
        if (!force && acik == acikDurum)
            return;

        acik = acikDurum;

        if (hedefIsik)
            hedefIsik.enabled = acik;

        if (hedefGoruntu)
            hedefGoruntu.SetActive(acik);
    }

    public bool Acik => acik;
}
