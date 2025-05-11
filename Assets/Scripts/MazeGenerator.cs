using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject wallPrefab, pelletPrefab, pacManPrefab, floorPrefab, fruitPrefab, ghostPrefab;

    [Header("Materials")]
    public Material wallMaterial, floorMaterial, fruitMaterial, pacManMaterial, ghostRedMaterial, ghostPinkMaterial;
    public Material[] pelletMaterials;

    [Header("Settings")]
    public int numFruits = 5;

    private int[,] layout = new int[,]
    {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,0,0,0,1},
        {1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,1},
        {1,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,1,0,0,1},
        {1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1},
        {1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,1},
        {1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,1},
        {1,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,0,1,1,1,1},
        {1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,0,0,0,1},
        {1,0,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
    };

    private List<Vector2Int> freeSpaces = new();

    void Start() => Generate();

    public void Generate()
    {
        ClearPreviousMaze();
        freeSpaces.Clear();

        int h = layout.GetLength(0), w = layout.GetLength(1);

        // Walls & pellets
        for (int z = 0; z < h; z++)
            for (int x = 0; x < w; x++)
            {
                var pos = new Vector3(x, 0, -z);
                if (layout[z, x] == 1)
                {
                    var wall = Instantiate(wallPrefab, pos, Quaternion.identity);
                    wall.tag = "Wall";
                    wall.GetComponent<Renderer>().material = wallMaterial;
                }
                else
                {
                    var pellet = Instantiate(pelletPrefab, pos + Vector3.up * .5f, Quaternion.identity);
                    pellet.tag = "Pellet";
                    var mat = pelletMaterials.Length > 0
                        ? pelletMaterials[Random.Range(0, pelletMaterials.Length)]
                        : floorMaterial;
                    pellet.GetComponent<Renderer>().material = mat;
                    freeSpaces.Add(new Vector2Int(x, z));
                }
            }

        // Pac-Man
        var pacPos = new Vector3(w/2f, .5f, -h/2f);
        var pac    = Instantiate(pacManPrefab, pacPos, Quaternion.identity);
        pac.tag    = "Player";
        pac.GetComponent<Renderer>().material = pacManMaterial;

        // Ghosts
        SpawnGhost(new Vector3(2, .5f, -2),       ghostRedMaterial);
        SpawnGhost(new Vector3(w-3, .5f, -(h-3)), ghostPinkMaterial);

        // Fruits
        GenerateFruits();

        // Floor
        if (floorPrefab)
        {
            var fp = new Vector3(w/2f-.5f, -0.01f, -h/2f+.5f);
            var f  = Instantiate(floorPrefab, fp, Quaternion.identity);
            f.transform.localScale = new Vector3(w,1,h);
            f.tag = "Floor";
            f.GetComponent<Renderer>().material = floorMaterial;
        }

        // Câmera
        if (Camera.main != null)
        {
            var cam = Camera.main.GetComponent<ThirdPersonCameraFollow>();
            if (cam != null)
                cam.target = pac.transform;  // volta a usar a property `target`
        }
    }

    void SpawnGhost(Vector3 pos, Material mat)
    {
        var g = Instantiate(ghostPrefab, pos, Quaternion.identity);
        g.tag = "Enemy";
        g.GetComponent<Renderer>().material = mat;
    }

    void GenerateFruits()
    {
        var chosen = new List<Vector2Int>();
        int tries = 0;
        while (chosen.Count < numFruits && tries < 100)
        {
            var rnd = freeSpaces[Random.Range(0, freeSpaces.Count)];
            if (!chosen.Exists(p => Vector2Int.Distance(p, rnd) < 7f))
            {
                var p = new Vector3(rnd.x, .5f, -rnd.y);
                var fr = Instantiate(fruitPrefab, p, Quaternion.identity);
                fr.tag = "Fruit";
                fr.GetComponent<Renderer>().material = fruitMaterial;
                chosen.Add(rnd);
            }
            tries++;
        }
    }

    void ClearPreviousMaze()
    {
        foreach (var tag in new[] { "Player","Enemy","Fruit","Pellet","Wall","Floor" })
            foreach (var o in GameObject.FindGameObjectsWithTag(tag))
                Destroy(o);
    }

    // exposto para a AI dos ghosts
    public int[,] GetLayout() => layout;
}
