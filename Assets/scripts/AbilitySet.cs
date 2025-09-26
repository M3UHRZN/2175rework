using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ability Set", fileName = "AbilitySet")]
public class AbilitySet : ScriptableObject
{
    [Header("Hareket")]
    [Tooltip("Karakter zıplayabilir mi?")]
    public bool canJump = true;

    [Tooltip("Karakter duvar zıplaması yapabilir mi?")]
    public bool canWallJump = true;

    [Tooltip("Karakter merdiven ve tırmanılabilir yüzeyleri kullanabilir mi?")]
    public bool canClimb = true;

    [Header("Sim'e Özel / Yardımcı")]
    [Tooltip("Sim enerji beslemesi yapabilir mi?")]
    public bool canSupplyPower = true;

    [Tooltip("Sim panelleri kullanabilir veya terminalleri açabilir mi?")]
    public bool canUsePanels = true;

    [Tooltip("Sim onarım aksiyonlarını tetikleyebilir mi?")]
    public bool canRepair = true;

    [Header("Koordinasyon")]
    [Tooltip("Karakter merge/split akışını tetikleyebilir mi?")]
    public bool canMerge = true;

    [Tooltip("Karakter aktif kontrolü diğer ajana devredebilir mi?")]
    public bool canSwitchCharacter = true;
}
