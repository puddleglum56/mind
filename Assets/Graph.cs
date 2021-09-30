using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{

  public TextAsset file;
  public GameObject nodepf;
  public GameObject edgepf; 
  public float width;
  public float length;
  public float height;
  
    void Start()
    {
		LoadGMLFromFile(file);
	}

    void Update(){}

    void LoadGMLFromFile(TextAsset f){
      string[] lines = f.text.Split('\n');
      int currentobject = -1; // 0 = graph, 1 = node, 2 = edge
      int stage = -1; // 0 waiting to open, 1 = waiting for attribute, 2 = waiting for id, 3 = waiting for label, 4 = waiting for source, 5 = waiting for target
      Node n = null;
      Dictionary<string,Node> nodes = new Dictionary<string,Node>();
      foreach (string line in lines){
	string l = line.Trim();
	string [] words = l.Split(' ');
	foreach(string word in words){
	  if (word == "graph" && stage == -1) {
	    currentobject = 0;
	  }
	  if (word == "node" && stage == -1) {
	    currentobject = 1;
	    stage = 0;	    
	  }
	  if (word == "edge" && stage == -1) {
	    currentobject = 2;
	    stage = 0;	    
	  }
	  if (word == "[" && stage == 0 && currentobject == 2){
	    stage = 1;;
	  }
	  if (word == "[" && stage == 0 && currentobject == 1){
	    stage = 1;
	    GameObject go = Instantiate(nodepf, new Vector3(Random.Range(-width/2, width/2), Random.Range(-length/2, length/2), Random.Range(-height/2, height/2)), Quaternion.identity);
	    n = go.GetComponent<Node>();
	    n.transform.parent = transform;
	    n.SetEdgePrefab(edgepf);
	    continue;
	  }
	  if (word == "]"){
	    stage = -1;
	  }
	  if (word == "id" && stage == 1 && currentobject == 1){
	    stage = 2;
	    continue;
	  }
	  if (word == "label" && stage == 1 && currentobject == 1){
	    stage = 3;
	    continue;
	  }
	  if (stage == 2){
	    nodes.Add(word, n);
	    stage = 1;
	    break;
	  }
	  if (stage == 3){
	    n.name = word;
	    stage = 1;
	    break;
	  }
	  if (word == "source" && stage == 1 && currentobject == 2){
	    stage = 4;
	    continue;
	  }
	  if (word == "target" && stage == 1 && currentobject == 2){
	    stage = 5;
	    continue;
	  }
	  if (stage == 4){
	    n = nodes[word];
	    stage = 1;
	    break;
	  }
	  if (stage == 5){
	    n.AddEdge(nodes[word]);
	    stage = 1;
	    break;
	  }
	}
      }
    }
    
}
