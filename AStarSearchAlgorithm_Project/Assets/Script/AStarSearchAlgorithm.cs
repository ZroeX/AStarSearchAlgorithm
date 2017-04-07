using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{    
    //position
    private Vector2 pos;
    public int x { get { return (int)pos.x; } }
    public int y { get { return (int)pos.y; } }

    //cost value
    public int H_Value; //to near
    public int G_Value;//to end
    public int F_Value { get { return H_Value + G_Value; } }
    
    //Another info
    public Node Parent;
    public Transform Self;
    public NODE_STATE_ENUM State;

    public Node(Vector2 _pos, Transform _self, NODE_STATE_ENUM _state) {
        pos = _pos;
        Self = _self;
        State = _state;
    }
}

public enum NODE_STATE_ENUM
{
    NOTHING,    //Can move node
    CHECKED,    //Checked node
    ANSWER,     //Answer node
    CLOSE,      //Can't move node
    START,      //Start Node
    END,        //End Node
}

public class AStarSearchAlgorithm : MonoBehaviour
{

    public Node StartNode;
    public Node EndNode;
    private Node[,] Map;

    // 10*10
    private int MapX = 10;
    private int MapY = 10;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Map one: " + MapX + " Map two: " + MapY);

        NodeInit();
    }

    private void NodeInit()
    {

        Map = new Node[MapX, MapY];
        UnityEngine.Object nodePrefab = Resources.Load("Prefab/Node");

        for (int x = 0; x < MapX; ++x)
        {
            for (int y = 0; y < MapY; ++y)
            {
                Node _Node = new Node(new Vector2(x, y)
                                     , (Instantiate(nodePrefab, new Vector3(x - (MapX / 2), 0, y - (MapY / 2)), Quaternion.identity, this.transform) as GameObject).transform
                                     , NODE_STATE_ENUM.NOTHING);
                _Node.Self.name = string.Format("({0},{1})", x, y);
                Map[x, y] = _Node;
            }
        }

        //set start node & end node
        ChangeNodeState(Map[0, 0], NODE_STATE_ENUM.START);
        ChangeNodeState(Map[MapX - 1, MapY - 1], NODE_STATE_ENUM.END);

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.transform.name);
                Vector2 nodePosition = GetNodeFromWorld(hit.transform.position);

                //set close node
                Node _node = Map[(int)nodePosition.x, (int)nodePosition.y];
                if (_node.State == NODE_STATE_ENUM.NOTHING)
                    ChangeNodeState(_node, NODE_STATE_ENUM.CLOSE);
                else if (_node.State == NODE_STATE_ENUM.CLOSE)
                    ChangeNodeState(_node, NODE_STATE_ENUM.NOTHING);
            }
        }


        if (Input.GetKeyDown(KeyCode.Space)) {
            FindAStarPath(StartNode,EndNode);
        }
    }


    private Vector2 GetNodeFromWorld(Vector3 _worldPosition)
    {
        return new Vector2(_worldPosition.x + (MapX / 2), _worldPosition.z + (MapY / 2));
    }


    //-------Stupid way...Orz----------
    /*
    private int[] GetNodeFormName(string _nodeName) {

        int strXStart = _nodeName.IndexOf("(");
        int strXLength = _nodeName.IndexOf(",");
        int XValue = Int32.Parse(_nodeName.Substring(strXStart + 1, strXLength - 1));

        int strYStart = _nodeName.IndexOf(",");
        int strYLength = _nodeName.IndexOf(")") - strYStart;
        int YValue = Int32.Parse(_nodeName.Substring(strYStart + 1, strYLength - 1));


        Debug.Log("x = " + XValue + " y = " + YValue);

        int[] tmp = new int[] { XValue, YValue };

        return tmp;
    }
    */

    private void ChangeNodeState(Node _node, NODE_STATE_ENUM _State)
    {
        _node.State = _State;

        Debug.Assert(_node != null);
        Debug.Assert(_node.Self != null);

        Renderer NodeRDR = _node.Self.GetComponent<Renderer>();
        switch (_State)
        {
            case NODE_STATE_ENUM.NOTHING:
                NodeRDR.material.color = Color.white;
                break;
            case NODE_STATE_ENUM.CHECKED:
                NodeRDR.material.color = Color.blue;
                break;
            case NODE_STATE_ENUM.ANSWER:
                NodeRDR.material.color = Color.red;
                break;
            case NODE_STATE_ENUM.CLOSE:
                NodeRDR.material.color = Color.black;
                break;
            case NODE_STATE_ENUM.START:
                StartNode = _node;
                NodeRDR.material.color = Color.green;
                break;
            case NODE_STATE_ENUM.END:
                EndNode = _node;
                NodeRDR.material.color = Color.gray;
                break;
        }
    }

    private List<Node> GetNearNode(Node _tragetNode)
    {
        List<Node> nearNode = new List<Node>();

        //the node's position near the target node
        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                if (x == 0 && y == 0) continue;//it mean Target Node;

                int checkX = _tragetNode.x + x;
                int checkY = _tragetNode.y + y;

                if (checkX >= 0 && checkX < MapX && checkY >= 0 && checkY < MapY)
                {
                    nearNode.Add(Map[checkX, checkY]);
                }
            }
        }
        return nearNode;
    }

    private int GetNodeDistance(Node _nodeA, Node _nodeB)
    {
        int dstX = Mathf.Abs(_nodeA.x - _nodeB.x);
        int dstY = Mathf.Abs(_nodeA.y - _nodeB.y);


        // 14 = oblique cost.
        // 10 = straight cost.
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    void RetracePath(Node _startNode, Node _endNode) {
        //List<Node> path = new List<Node>();

        Debug.Log("RetracePath");

        Node tempNode = _endNode;
        while (tempNode != _startNode ) {

            Debug.Assert(tempNode.Parent != null);
            Debug.Log("Ans Node :  x = " + tempNode.x + " , y = " + tempNode.y);    

           ChangeNodeState(tempNode, NODE_STATE_ENUM.ANSWER);           
           tempNode = tempNode.Parent;
        }        
    }

    //A*
    private void FindAStarPath(Node _startNode, Node _endNode)
    {
        List<Node> openSet = new List<Node>();

        openSet.Add(_startNode);

        while (openSet.Count > 0)
        {

            Node tempNode = openSet[0];

            for (int i = 1; i < openSet.Count; ++i)
            {
                if (openSet[i].F_Value < tempNode.F_Value ||
                   (openSet[i].F_Value == tempNode.H_Value && openSet[i].H_Value < tempNode.H_Value))
                {
                    tempNode = openSet[i];
                }
            }

            openSet.Remove(tempNode);
            ChangeNodeState(tempNode, NODE_STATE_ENUM.CHECKED);

            //Debug.Log("tempNode in FindAStarPath: x = " + tempNode.x + " , y = " + tempNode.y);
            Debug.Log("tempNode in FindAStarPath: G = " + tempNode.G_Value + " , H = " + tempNode.H_Value);
            
            

            if (tempNode == _endNode)
            {              
                RetracePath(_startNode, _endNode);
                return;
            }

            foreach (Node nearNode in GetNearNode(tempNode))
            {
                if (nearNode.State == NODE_STATE_ENUM.CLOSE || nearNode.State == NODE_STATE_ENUM.CHECKED)
                    continue;

                int MovementCostToNearNode = tempNode.G_Value + GetNodeDistance(tempNode, nearNode);
                bool isInOpenSet = openSet.Contains(nearNode);

                if (MovementCostToNearNode < nearNode.G_Value || !isInOpenSet)
                {
                    nearNode.G_Value = MovementCostToNearNode;
                    nearNode.H_Value = GetNodeDistance(nearNode, _endNode);
                    nearNode.Parent = tempNode;           

                    if (!isInOpenSet) 
                        openSet.Add(nearNode);                    
                }

                if (nearNode == _endNode)
                {
                    RetracePath(_startNode, _endNode);
                    return;
                }

            }
        }
    }



}


