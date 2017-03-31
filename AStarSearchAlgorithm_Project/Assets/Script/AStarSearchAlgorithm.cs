using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{    
    public Vector2 pos;
    public int x { get { return (int)pos.x; } }
    public int y { get { return (int)pos.y; } }
    public int H_Value;
    public int G_Value;
    public int F_Value { get { return H_Value + G_Value; } }
    public Node Parent;
    public Transform Self;
    public NODE_STATE_ENUM State;
}

public enum NODE_STATE_ENUM
{
    NOTHING,    //Can move node
    CHECKED,    //Checked node
    CLOSE,      //Can't move node
    START,      //Start Node
    END,        //End Node
}

public class AStarSearchAlgorithm : MonoBehaviour {

    public Node StartNode;
    public Node EndNode;
    private Node[,] Map = new Node[11,11];  //11x11
    
	// Use this for initialization
	void Start () {
        Debug.Log("Map one: " + Map.GetLength(0) + " Map two: " + Map.GetLength(1));
        NodeInit();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objHit = hit.transform;
                Debug.Log(objHit.name);

                //set close node
                Node _node = Map[(int)Math.Truncate(objHit.position.x) + 5, (int)Math.Truncate(objHit.position.z) + 5];
                if(_node.State == NODE_STATE_ENUM.NOTHING)
                    ChangeNodeState(_node, NODE_STATE_ENUM.CLOSE);
                else if(_node.State == NODE_STATE_ENUM.CLOSE)
                    ChangeNodeState(_node, NODE_STATE_ENUM.NOTHING);
            }
        }
    }

    private void NodeInit() {

        Transform[] nodeList = this.gameObject.transform.GetComponentsInChildren<Transform>();

        for (int x = 0; x < nodeList.Length; ++x) {
            int Xpos = (int)Math.Truncate(nodeList[x].position.x) + 5;
            int Ypos = (int)Math.Truncate(nodeList[x].position.z) + 5;
            Map[Xpos, Ypos] = new Node { pos = new Vector2(Xpos, Ypos) , Self = nodeList[x]};              
        }

        //set start node & end node
        ChangeNodeState(Map[0, 0], NODE_STATE_ENUM.START);
        ChangeNodeState(Map[10,10], NODE_STATE_ENUM.END);

    }

    private void ChangeNodeState(Node _node , NODE_STATE_ENUM _State) {
        _node.State = _State;

        Renderer NodeRDR = _node.Self.GetComponent<Renderer>();
        switch (_State) {
            case NODE_STATE_ENUM.NOTHING:
                NodeRDR.material.color = Color.white;
                break;
            case NODE_STATE_ENUM.CHECKED:
                NodeRDR.material.color = Color.gray;
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
                NodeRDR.material.color = Color.red;
                break;
        }
    }

    private List<Node> GetNearNode(Node _node) {
        List < Node > nearNode = new List<Node>();
        return nearNode;
    }

    private void FindAStarPath(Node _startNode, Node _EndNode) {
        List<Node> openSet = new List<Node>();
        HashSet<Node> closeSet = new HashSet<Node>();
        openSet.Add(_startNode);

        while (openSet.Count > 0) {
            Node tempNode = openSet[0];
            for (int i = 1; i < openSet.Count; ++i) {
                if (openSet[i].F_Value < tempNode.F_Value || (openSet[i].F_Value == tempNode.H_Value && openSet[i].H_Value < tempNode.H_Value)) {
                    tempNode = openSet[i];
                }
            }

            openSet.Remove(tempNode);
            closeSet.Add(tempNode);

            if (tempNode == _EndNode) {
                return;
            }



        }
    }

}


