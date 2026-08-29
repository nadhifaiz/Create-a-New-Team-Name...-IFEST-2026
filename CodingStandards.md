# 📖 Unity C# Coding Standards

Dokumen ini berisi standar penulisan kode untuk seluruh programmer dalam proyek. Tujuannya adalah menjaga kode tetap konsisten, mudah dibaca, mudah dipelihara, dan mempermudah kolaborasi tim.

---

# 1. General Rules

## Wajib

✅ Gunakan Bahasa Inggris untuk seluruh kode.

✅ Gunakan nama yang deskriptif dan mudah dipahami.

✅ Terapkan Single Responsibility Principle (1 script = 1 tanggung jawab utama).

✅ Selalu gunakan Access Modifier (`private`, `public`, `protected`).

✅ Gunakan SerializeField untuk variabel yang ingin ditampilkan di Inspector.

✅ Pisahkan logic berdasarkan sistem.

---

## Dilarang

❌ Nama variabel tidak jelas.

```csharp
int x;
float a;
GameObject obj;
```

❌ Nama method yang tidak menjelaskan fungsi.

```csharp
void Do()
{
}

void Test()
{
}
```

❌ Hardcoded value tanpa alasan.

```csharp
transform.position += Vector3.right * 7.3f;
```

Gunakan:

```csharp
[SerializeField] private float moveSpeed = 7.3f;
```

---

# 2. File Naming

Nama file HARUS sama dengan nama class utama.

Benar:

```csharp
PlayerMovement.cs

public class PlayerMovement
{
}
```

Salah:

```csharp
Movement.cs

public class PlayerMovement
{
}
```

---

# 3. Class Naming

Gunakan PascalCase.

Format:

```csharp
ClassName
```

Contoh:

```csharp
PlayerMovement
EnemyAI
GameManager
InventorySystem
AudioManager
```

---

# 4. Method Naming

Gunakan PascalCase.

Format:

```csharp
Verb + Object
```

Contoh:

```csharp
MovePlayer()
AttackEnemy()
LoadGame()
SaveData()
PlaySound()
OpenInventory()
```

Hindari:

```csharp
Move()
Do()
Test()
Function1()
```

---

# 5. Variable Naming

Gunakan camelCase.

Format:

```csharp
variableName
```

Contoh:

```csharp
moveSpeed
jumpForce
currentHealth
maxHealth
inventorySlots
```

---

# 6. Private Variable

Gunakan camelCase.

```csharp
private float moveSpeed;
private Rigidbody playerRb;
private Animator animator;
```

---

# 7. SerializeField

Gunakan format:

```csharp
[SerializeField] private float moveSpeed;
[SerializeField] private Animator animator;
[SerializeField] private GameObject projectilePrefab;
```

Jangan:

```csharp
public float moveSpeed;
```

Kecuali memang harus diakses class lain.

---

# 8. Constant Naming

Gunakan UPPER_SNAKE_CASE.

```csharp
private const float MAX_SPEED = 10f;
private const int MAX_HEALTH = 100;
```

---

# 9. Enum Naming

Gunakan PascalCase.

```csharp
public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack
}
```

---

# 10. Boolean Naming

Gunakan awalan:

```csharp
is
has
can
should
```

Contoh:

```csharp
isGrounded
isDead
hasWeapon
canAttack
shouldRespawn
```

Jangan:

```csharp
grounded
dead
weapon
```

---

# 11. Property Naming

Gunakan PascalCase.

```csharp
public int Health { get; private set; }

public bool IsDead { get; private set; }
```

---

# 12. Interface Naming

Gunakan awalan I.

```csharp
public interface IDamageable
{
    void TakeDamage(int amount);
}
```

Contoh lain:

```csharp
IInteractable
ISaveable
IMovable
```

---

# 13. Event Naming

Gunakan format:

```csharp
OnSomethingHappened
```

Contoh:

```csharp
OnPlayerDied
OnGameOver
OnLevelCompleted
```

---

# 14. Script Structure

Urutan penulisan:

```csharp
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables

    [SerializeField] private float moveSpeed;

    private Rigidbody rb;

    #endregion

    #region Unity Methods

    private void Awake()
    {

    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {

    }

    #endregion

    #region Public Methods

    public void EnableMovement()
    {

    }

    #endregion

    #region Private Methods

    private void MovePlayer()
    {

    }

    #endregion
}
```

---

# 15. Bracket Style

Gunakan Allman Style.

Benar:

```csharp
if (isGrounded)
{
    Jump();
}
```

Salah:

```csharp
if (isGrounded) {
    Jump();
}
```

---

# 16. If Statement

Gunakan kurung kurawal meskipun hanya satu baris.

Benar:

```csharp
if (isGrounded)
{
    Jump();
}
```

Salah:

```csharp
if (isGrounded)
    Jump();
```

---

# 17. Komentar

Komentar digunakan untuk menjelaskan alasan, bukan menjelaskan kode yang sudah jelas.

Buruk:

```csharp
// Menambah health
health += 10;
```

Bagus:

```csharp
// Bonus health diberikan setelah menyelesaikan challenge
health += bonusHealth;
```

---

# 18. TODO Comment

Gunakan format:

```csharp
// TODO: Implement save system
// TODO: Add multiplayer support
```

---

# 19. Magic Number

Dilarang:

```csharp
damage *= 1.35f;
```

Gunakan:

```csharp
private const float CRITICAL_MULTIPLIER = 1.35f;

damage *= CRITICAL_MULTIPLIER;
```

---

# 20. Logging

Format:

```csharp
Debug.Log("[Player] Player spawned");

Debug.LogWarning("[Inventory] Inventory full");

Debug.LogError("[SaveSystem] Save failed");
```

---

# 21. Null Check

Selalu cek sebelum akses object yang bisa null.

```csharp
if (animator != null)
{
    animator.SetTrigger("Attack");
}
```

---

# 22. Git Rules

Sebelum commit:

- Tidak ada compile error.
- Tidak ada missing reference.
- Tidak ada warning baru.
- Sudah test fitur sendiri.

Commit message:

```bash
feat: add inventory system
fix: resolve enemy attack bug
refactor: improve game manager structure
ui: redesign main menu
```

---

# 23. Null Safety Rules

## Wajib Melakukan Null Check

Setiap object yang berpotensi null WAJIB diperiksa sebelum digunakan.

Benar:

```csharp
if (animator != null)
{
    animator.SetTrigger("Attack");
}
```

Salah:

```csharp
animator.SetTrigger("Attack");
```

---

## Component Check Setelah GetComponent

Benar:

```csharp
rb = GetComponent<Rigidbody>();

if (rb == null)
{
    Debug.LogError("[PlayerMovement] Rigidbody not found.");
    enabled = false;
    return;
}
```

Salah:

```csharp
rb = GetComponent<Rigidbody>();
rb.AddForce(Vector3.up);
```

---

## Serialized Reference Check

Untuk reference yang di-assign melalui Inspector.

```csharp
private void Awake()
{
    if (healthBar == null)
    {
        Debug.LogError("[PlayerUI] HealthBar reference missing.");
        enabled = false;
        return;
    }
}
```

---

## Event Invocation

Gunakan Null Conditional Operator.

Benar:

```csharp
OnPlayerDied?.Invoke();
```

Salah:

```csharp
OnPlayerDied.Invoke();
```

---

## Singleton Access

Benar:

```csharp
if (GameManager.Instance != null)
{
    GameManager.Instance.StartGame();
}
```

Salah:

```csharp
GameManager.Instance.StartGame();
```

---

## Early Return Pattern

Gunakan Early Return untuk mengurangi nested if.

Benar:

```csharp
private void Attack()
{
    if (target == null)
    {
        return;
    }

    target.TakeDamage(damage);
}
```

Salah:

```csharp
private void Attack()
{
    if (target != null)
    {
        target.TakeDamage(damage);
    }
}
```

---

# 24. Code Review Checklist

Sebelum merge ke develop:

- [ ] Naming sesuai standar
- [ ] Tidak ada code duplication
- [ ] Tidak ada hardcoded value
- [ ] Tidak ada compile error
- [ ] Sudah dites
- [ ] Komentar seperlunya
- [ ] Tidak mengubah sistem lain tanpa alasan

---

## Rule

Jika terdapat kemungkinan suatu object bernilai null, programmer WAJIB:

- Melakukan validasi.
- Memberikan log error yang jelas.
- Menghentikan proses bila diperlukan.

Tidak boleh mengasumsikan object selalu tersedia.

# Final Principle

Tulis kode seolah-olah orang yang akan me-maintain kode tersebut 6 bulan lagi adalah orang yang tidak mengenal proyek ini sama sekali.

Kode yang mudah dibaca lebih berharga daripada kode yang terlihat pintar.
