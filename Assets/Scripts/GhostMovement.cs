// GhostMovement.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GhostMovement : MonoBehaviour
{
    public float speed = 2f;
    private Transform pacman;
    private Rigidbody rb;

    private int[,] layout;
    private List<Vector2Int> path = new();
    private int currentPathIndex = 0;
    private float nodeReachThreshold = 0.1f;

    void Start()
    {
        pacman = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // evita o aviso CS0618
        var gen = Object.FindFirstObjectByType<MazeGenerator>();
        layout = gen != null ? gen.GetLayout() : null;

        InvokeRepeating(nameof(UpdatePath), 0f, 1f);
    }

    void FixedUpdate()
    {
        if (path == null || path.Count == 0 || currentPathIndex >= path.Count) return;

        Vector3 targetPos = new Vector3(path[currentPathIndex].x, transform.position.y, -path[currentPathIndex].y);
        Vector3 direction = (targetPos - transform.position).normalized;
        rb.linearVelocity = direction * speed;

        if (Vector3.Distance(transform.position, targetPos) < nodeReachThreshold)
            currentPathIndex++;
    }

    void UpdatePath()
    {
        if (pacman == null || layout == null) return;

        Vector2Int start = WorldToGrid(transform.position);
        Vector2Int end   = WorldToGrid(pacman.position);

        path = FindPath(start, end);
        currentPathIndex = 0;
    }

    Vector2Int WorldToGrid(Vector3 pos)
    {
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(-pos.z));
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        var frontier   = new PriorityQueue();
        var cameFrom   = new Dictionary<Vector2Int, Vector2Int> { [start] = start };
        var costSoFar  = new Dictionary<Vector2Int, int>          { [start] = 0 };

        frontier.Enqueue(start, 0);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current == end) break;

            foreach (var next in GetNeighbours(current))
            {
                int newCost = costSoFar[current] + 1;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    int priority   = newCost + Heuristic(next, end);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(end)) return new List<Vector2Int>();

        var path = new List<Vector2Int>();
        var node = end;
        while (node != start)
        {
            path.Add(node);
            node = cameFrom[node];
        }
        path.Reverse();
        return path;
    }

    IEnumerable<Vector2Int> GetNeighbours(Vector2Int pos)
    {
        var dirs = new[] {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1)
        };
        foreach (var d in dirs)
        {
            var np = pos + d;
            if (np.x >= 0 && np.y >= 0 &&
                np.x < layout.GetLength(1) && np.y < layout.GetLength(0) &&
                layout[np.y, np.x] == 0)
                yield return np;
        }
    }

    int Heuristic(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance?.LoseLife();
    }

    // prioridade mínima
    public class PriorityQueue
    {
        private List<(Vector2Int item, int priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(Vector2Int item, int priority)
            => elements.Add((item, priority));

        public Vector2Int Dequeue()
        {
            int best = 0;
            for (int i = 1; i < elements.Count; i++)
                if (elements[i].priority < elements[best].priority)
                    best = i;
            var result = elements[best].item;
            elements.RemoveAt(best);
            return result;
        }
    }
}
