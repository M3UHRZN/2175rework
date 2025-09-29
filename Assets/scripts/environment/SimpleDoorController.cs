using UnityEngine;

[DisallowMultipleComponent]
public class SimpleDoorController : MonoBehaviour
{
    [Tooltip("Kapının fizik çarpışmalarını yönetecek collider. Boşsa otomatik aranır.")]
    public Collider2D kapiCollider;
    [Tooltip("Animasyon kullanıyorsanız tetiklenecek Animator.")]
    public Animator animator;
    [Tooltip("Animator üzerinde açık/kapalı durumunu temsil eden parametre adı.")]
    public string animatorBoolParam = "Open";
    [Tooltip("Kapı başlangıçta açık mı?")]
    public bool baslangictaAcik;

    bool acik;

    void Awake()
    {
        if (!kapiCollider)
            kapiCollider = GetComponentInChildren<Collider2D>();
        if (!animator)
            animator = GetComponentInChildren<Animator>();

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

        if (kapiCollider)
            kapiCollider.enabled = !acik;

        if (animator && !string.IsNullOrEmpty(animatorBoolParam))
            animator.SetBool(animatorBoolParam, acik);
    }

    public bool Acik => acik;
}
