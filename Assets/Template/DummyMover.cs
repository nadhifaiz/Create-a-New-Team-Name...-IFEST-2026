using UnityEngine;

public class DummyMover : MonoBehaviour
{
    [Header("Pengaturan Gerak")]
    public float speed = 2f;      // Seberapa cepat bolak-baliknya
    public float distance = 3f;   // Seberapa jauh jarak bolak-baliknya

    private Vector2 startPos;

    void Start()
    {
        // Simpan posisi awal saat game dimulai
        startPos = transform.position;
    }

    void Update()
    {
        // Kalkulasi posisi baru menggunakan gelombang Sinus
        float newX = startPos.x + Mathf.Sin(Time.time * speed) * distance;

        // Terapkan ke posisi object
        transform.position = new Vector2(newX, startPos.y);
    }
}