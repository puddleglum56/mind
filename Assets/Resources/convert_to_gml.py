import networkx as nx
import sys

in_adjlist = sys.argv[1]
out_gml = in_adjlist.replace('.csv','.txt') 

g = nx.read_adjlist(in_adjlist)
nx.write_gml(g, out_gml)
