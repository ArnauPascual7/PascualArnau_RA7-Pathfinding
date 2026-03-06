using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Button _nextButton;

    public int Size;
    public BoxCollider2D Panel;
    public GameObject startToken;
    public GameObject endToken;

    public GameObject tokenBlue;
    public GameObject tokenYellow;
    public GameObject tokenRed;
    public GameObject tokenGreen;

    //private int[,] GameMatrix; //0 not chosen, 1 player, 2 enemy de momento no hago nada con esto
    private Node[,] NodeMatrix;
    private int startPosx, startPosy;
    private int endPosx, endPosy;

    private bool _continue = false;

    void Awake()
    {
        Instance = this;
        //GameMatrix = new int[Size, Size];
        Calculs.CalculateDistances(Panel, Size);
    }
    private void Start()
    {
        /*for(int i = 0; i<Size; i++)
        {
            for (int j = 0; j< Size; j++)
            {
                GameMatrix[i, j] = 0;
            }
        }*/
        
        startPosx = Random.Range(0, Size);
        startPosy = Random.Range(0, Size);
        do
        {
            endPosx = Random.Range(0, Size);
            endPosy = Random.Range(0, Size);
        } while(endPosx== startPosx || endPosy== startPosy);

        //GameMatrix[startPosx, startPosy] = 2;
        //GameMatrix[startPosx, startPosy] = 1;
        NodeMatrix = new Node[Size, Size];
        CreateNodes();

        Instantiate(startToken, Calculs.CalculatePoint(startPosx, startPosy), Quaternion.identity);
        Instantiate(endToken, Calculs.CalculatePoint(endPosx, endPosy), Quaternion.identity);

        //_currentNode = NodeMatrix[startPosx, startPosy];

        _nextButton.onClick.AddListener(OnNextPressed);
        StartCoroutine(RunAStar());
    }

    private void OnNextPressed()
    {
        _continue = true;
    }

    /*private void Update()
    {
        if (!_end)
        {
            GetNextNode(_currentNode);
        }
    }

    private void GetNextNode(Node node)
    {
        if (node == NodeMatrix[endPosx, endPosy])
        {
            _end = true;
            return;
        }

        float minCost = 1000;
        int i = -1;

        foreach (var way in node.WayList)
        {
            //Instantiate(blue, way.NodeDestiny.RealPosition, Quaternion.identity);

            if (way.Cost + way.ACUMulatedCost + way.NodeDestiny.Heuristic < minCost)
            {
                minCost = way.Cost + way.ACUMulatedCost + way.NodeDestiny.Heuristic;
                i = node.WayList.IndexOf(way);
            }
        }

        _currentNode = node.WayList[i].NodeDestiny;
        Instantiate(blue, node.WayList[i].NodeDestiny.RealPosition, Quaternion.identity);
    }*/

    private IEnumerator RunAStar()
    {
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Dictionary<Node, Way> bestWayToNode = new Dictionary<Node, Way>();

        Dictionary<Node, GameObject> panelTokens = new Dictionary<Node, GameObject>();

        Node startNode = NodeMatrix[startPosx, startPosy];
        Node endNode = NodeMatrix[endPosx, endPosy];

        Way startWay = new Way(null, 0);
        startWay.TotalACUMulatedWayCost = 0;
        bestWayToNode[startNode] = startWay;

        openList.Add(startNode);
        panelTokens[startNode] = Instantiate(tokenBlue, startNode.RealPosition, Quaternion.identity);

        while (openList.Count > 0)
        {
            _continue = false;
            yield return new WaitUntil(() => _continue);

            Node current = openList[0];
            foreach (Node n in openList)
            {
                float fCurrent = bestWayToNode[current].TotalACUMulatedWayCost + current.Heuristic;
                float fN = bestWayToNode[n].TotalACUMulatedWayCost + n.Heuristic;
                if (fN < fCurrent)
                    current = n;
            }

            if (panelTokens.ContainsKey(current))
                Destroy(panelTokens[current]);
            panelTokens[current] = Instantiate(tokenYellow, current.RealPosition, Quaternion.identity);

            Debug.Log($"Current Node: ({current.PositionX}, {current.PositionY}) Total Cost: ({bestWayToNode[current].TotalACUMulatedWayCost + current.Heuristic})");

            _continue = false;
            yield return new WaitUntil(() => _continue);

            if (current == endNode)
            {
                ReconstructPath(endNode, panelTokens);
                yield break;
            }

            openList.Remove(current);
            closedList.Add(current);

            if (panelTokens.ContainsKey(current))
                Destroy(panelTokens[current]);
            panelTokens[current] = Instantiate(tokenRed, current.RealPosition, Quaternion.identity);

            float currentACUMulatedCost = bestWayToNode[current].TotalACUMulatedWayCost;

            foreach (Way way in current.NeighborWaysList)
            {
                Node neighbor = way.WayNodeDestiny;

                if (closedList.Contains(neighbor)) continue;

                float wayTotalACUMulatedCost = currentACUMulatedCost + way.WayCost;

                if (!openList.Contains(neighbor))
                {
                    openList.Add(neighbor);
                    if (panelTokens.ContainsKey(neighbor))
                        Destroy(panelTokens[neighbor]);
                    panelTokens[neighbor] = Instantiate(tokenBlue, neighbor.RealPosition, Quaternion.identity);
                }
                else if (wayTotalACUMulatedCost >= bestWayToNode[neighbor].TotalACUMulatedWayCost)
                {
                    continue;
                }

                way.TotalACUMulatedWayCost = wayTotalACUMulatedCost;
                bestWayToNode[neighbor] = way;
                neighbor.NodeParent = current;
            }
        }
    }

    private void ReconstructPath(Node endNode, Dictionary<Node, GameObject> nodeVisuals)
    {
        Node current = endNode;
        List<Node> path = new List<Node>();

        while (current != null)
        {
            path.Add(current);
            current = current.NodeParent;
        }

        path.Reverse();

        foreach (Node node in path)
        {
            if (nodeVisuals.ContainsKey(node))
                Destroy(nodeVisuals[node]);
            Instantiate(tokenGreen, node.RealPosition, Quaternion.identity);
        }
    }

    public void CreateNodes()
    {
        for(int i=0; i<Size; i++)
        {
            for(int j=0; j<Size; j++)
            {
                NodeMatrix[i, j] = new Node(i, j, Calculs.CalculatePoint(i,j));
                NodeMatrix[i, j].Heuristic = Calculs.CalculateHeuristic(NodeMatrix[i,j],endPosx,endPosy);
            }
        }
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                SetWays(NodeMatrix[i, j], i, j);
            }
        }
        //DebugMatrix();
    }

    public void SetWays(Node node, int x, int y)
    {
        node.NeighborWaysList = new List<Way>();
        if (x>0)
        {
            node.NeighborWaysList.Add(new Way(NodeMatrix[x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.NeighborWaysList.Add(new Way(NodeMatrix[x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(x<Size-1)
        {
            node.NeighborWaysList.Add(new Way(NodeMatrix[x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.NeighborWaysList.Add(new Way(NodeMatrix[x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if(y>0)
        {
            node.NeighborWaysList.Add(new Way(NodeMatrix[x, y - 1], Calculs.LinearDistance));
        }
        if (y<Size-1)
        {
            node.NeighborWaysList.Add(new Way(NodeMatrix[x, y + 1], Calculs.LinearDistance));
            if (x>0)
            {
                node.NeighborWaysList.Add(new Way(NodeMatrix[x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x<Size-1)
            {
                node.NeighborWaysList.Add(new Way(NodeMatrix[x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }
    }

    public void DebugMatrix()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                //Instantiate(token, NodeMatrix[i, j].RealPosition, Quaternion.identity);

                Debug.Log("Element (" + j + ", " + i + ")");
                Debug.Log("Position " + NodeMatrix[i, j].RealPosition);
                Debug.Log("Heuristic " + NodeMatrix[i, j].Heuristic);

                /*Debug.Log("Ways: ");
                foreach (var way in NodeMatrix[i, j].WayList)
                {
                    Debug.Log(" (" + way.NodeDestiny.PositionX + ", " + way.NodeDestiny.PositionY + ")");
                }*/
            }
        }
    }
}
