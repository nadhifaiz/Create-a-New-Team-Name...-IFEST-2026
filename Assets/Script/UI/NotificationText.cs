using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NotificationText : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Jarak dari luar layar (Gunakan nilai negatif jika ingin muncul dari kanan)")]
    [SerializeField] private float slideDistance = 300f;
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private float stayDuration = 1.5f;

    [Header("Stacking Settings")]
    [Tooltip("Jarak ke bawah untuk setiap notifikasi yang menumpuk")]
    [SerializeField] private float ySpacing = 50f;

    private TMP_Text textComponent;
    private Vector3 basePosition;
    private float timer = 0f;

    // State Animasi
    private enum State { SlidingIn, Staying, SlidingOut }
    private State currentState = State.SlidingIn;

    // KUNCI STACKING: Menyimpan semua notifikasi yang sedang aktif
    private static List<NotificationText> activeNotifications = new List<NotificationText>();

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        // 1. Simpan posisi tetap yang ditentukan saat Instantiate di HUD
        basePosition = transform.position;

        // 2. Lempar posisi awal ke luar layar (titik start animasi)
        transform.position = new Vector3(basePosition.x - slideDistance, basePosition.y, basePosition.z);

        // 3. Daftarkan diri ke daftar notifikasi aktif
        activeNotifications.Add(this);
    }

    private void Update()
    {
        // --- LOGIKA POSISI MENUMPUK (STACKING) ---
        // Cari urutan ke berapa teks ini di dalam list
        int index = activeNotifications.IndexOf(this);

        // Target Y akan turun sejauh ySpacing dikali urutannya
        float targetY = basePosition.y - (index * ySpacing);

        // --- LOGIKA ANIMASI STATE ---
        timer += Time.deltaTime;
        float targetX = basePosition.x; // Default: Berada di posisi asli

        if (currentState == State.SlidingIn)
        {
            if (timer >= slideDuration)
            {
                currentState = State.Staying;
                timer = 0f;
            }
        }
        else if (currentState == State.Staying)
        {
            if (timer >= stayDuration)
            {
                currentState = State.SlidingOut;
                timer = 0f;
            }
        }
        else if (currentState == State.SlidingOut)
        {
            targetX = basePosition.x - slideDistance; // Target kembali ke luar layar

            // Opsional: Fade Out warna agar lebih halus hilangnya
            if (textComponent != null)
            {
                Color c = textComponent.color;
                c.a = Mathf.Lerp(c.a, 0f, Time.deltaTime * 10f);
                textComponent.color = c;
            }

            if (timer >= slideDuration)
            {
                // Selesai -> Hapus dari antrean dan hancurkan
                activeNotifications.Remove(this);
                Destroy(gameObject);
                return;
            }
        }

        // --- EKSEKUSI PERGERAKAN (SMOOTH LERP) ---
        // Vector3.Lerp membuat pergerakan sangat mulus seperti pegas
        Vector3 targetPosition = new Vector3(targetX, targetY, basePosition.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
    }

    private void OnDestroy()
    {
        // Pengaman: Pastikan terhapus dari list jika hancur secara paksa (misal ganti scene)
        if (activeNotifications.Contains(this))
        {
            activeNotifications.Remove(this);
        }
    }
}