using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    GameObject epf;
    GameObject manager;

    List<GameObject>  edges  = new List<GameObject> ();
    List<SpringJoint> joints = new List<SpringJoint>();

    public Material orange;
    public Material white;

    public int spiked;
  
    void Start(){
        spiked = 0;
        transform.GetChild(0).GetComponent<TextMesh>().text = name;
        manager = GameObject.Find("Manager");
    }
  
    void Update(){    
        int i = 0;
        foreach (GameObject edge in edges){
          edge.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
          SpringJoint sj = joints[i];
          GameObject target = sj.connectedBody.gameObject;
          edge.transform.LookAt(target.transform);
          Vector3 ls = edge.transform.localScale;
          ls.z = Vector3.Distance(transform.position, target.transform.position);
          edge.transform.localScale = ls;
          edge.transform.position = new Vector3((transform.position.x+target.transform.position.x)/2,
                            (transform.position.y+target.transform.position.y)/2,
                            (transform.position.z+target.transform.position.z)/2);
          i++;
        }

        if (spiked > 0)
        {
            GetComponent<Renderer>().material = white;
        } else
        {
            GetComponent<Renderer>().material = orange;
        }
    }

    private void OnMouseOver()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        manager.GetComponent<Manager>().neuron_buffer[int.Parse(gameObject.name)].spiked = 1;

    }

    public void SetEdgePrefab(GameObject epf){
        this.epf = epf;
    }
  
    public void AddEdge(Node n){
        SpringJoint sj = gameObject.AddComponent<SpringJoint> ();  
        sj.autoConfigureConnectedAnchor = false;
        sj.anchor = new Vector3(0,0.5f,0);
        sj.connectedAnchor = new Vector3(0,0,0);    
        sj.enableCollision = true;
        sj.connectedBody = n.GetComponent<Rigidbody>();
        GameObject edge = Instantiate(this.epf, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
        edges.Add(edge);
        joints.Add(sj);
    }
    
}
