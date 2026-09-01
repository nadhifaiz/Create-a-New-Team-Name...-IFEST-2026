using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [Header("World Boundaries (Batas Layar)")]
    [Tooltip("Titik kordinat X paling kiri (tempat awan muncul)")]
    public float startPosX = -12f;
    [Tooltip("Titik kordinat X paling kanan (tempat awan menghilang dan balik lagi)")]
    public float endPosX = 12f;

    [Header("Movement Settings")]
    public float speed = 2.0f;
    [Tooltip("Jumlah awan yang mau disebar di layar")]
    public int numberOfClouds = 3;

    [Header("Cloud Prefab")]
    public GameObject cloudPrefab;

    private Transform[] clouds;

    void Start()
    {
        if (cloudPrefab == null)
        {
            Debug.LogError("Prefab awan belum dimasukkan!");
            return;
        }

        clouds = new Transform[numberOfClouds];

        // Menghitung jarak antar awan agar saat awal game, 
        // mereka tidak menumpuk dan sudah tersebar rata di layar.
        float totalDistance = endPosX - startPosX;
        float spacing = totalDistance / numberOfClouds;

        for (int i = 0; i < numberOfClouds; i++)
        {
            GameObject newCloud = Instantiate(cloudPrefab, transform);

            // Sebar awan pertama di kiri, awan kedua di tengah, dst.
            float startX = startPosX + (spacing * i);

            // Tetap menggunakan tinggi Y bawaan dari prefab asli
            newCloud.transform.position = new Vector3(startX, cloudPrefab.transform.position.y, transform.position.z);

            clouds[i] = newCloud.transform;
        }
    }

    void Update()
    {
        if (clouds == null || clouds.Length == 0) return;

        foreach (Transform cloud in clouds)
        {
            // Gerakkan awan ke kanan setiap frame
            cloud.Translate(Vector3.right * speed * Time.deltaTime);

            // Jika awan sudah menyentuh / melewati ujung kanan dunia (endPosX)
            if (cloud.position.x >= endPosX)
            {
                // Teleportasi awan tersebut balik ke ujung kiri (startPosX)
                cloud.position = new Vector3(startPosX, cloud.position.y, cloud.position.z);
            }
        }
    }
}