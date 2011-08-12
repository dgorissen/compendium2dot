using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace Compendium2Dot {

    /**
     * Generate a graphviz file from a compendium XML export
     * Simply call with the path to the xml file.
     * 
     * Disclaimer: Written as a relative newbie to both LINQ and C# :)
     * 
     * TODO: 
     *  - take into account deleted nodes
     *  - causes a stackoverflow exception with very big maps :/
     *  
     * Author: Dirk Gorissen <dirk.gorissen@soton.ac.uk>
     * Copyright: LGPL
     */
    class Compendium2Dot {
        
        static void Main(string[] args) {
            if (args.Length == 1) {
                Console.WriteLine("Running on " + args[0] + "...");
                genDot(args[0]);
            }
            else {
                throw new Exception("Invalid number of parameters, must pass the path to the xml file");
            }
        }

        private static string handleNodes(XDocument xmlDoc, string rootView) {
            String s = "";
            var views = from view in xmlDoc.Descendants("view")
                        join node in xmlDoc.Descendants("node") on view.Attribute("noderef").Value equals node.Attribute("id").Value
                        where view.Attribute("viewref").Value == rootView
                        select new {
                            viewref = rootView,
                            noderef = node.Attribute("id").Value,
                            label = node.Attribute("label").Value,
                            type = node.Attribute("type").Value
                        };


            //LinkedList<String> done = new LinkedList<string>();

            foreach (var v in views) {
                //if the current node is a map
                if (v.type.Equals("2")) {
                    //if (done.Contains(v.noderef)) {
                    //    continue;
                    //}
                    //else {
                        //done.AddLast(v.noderef);
                        s += "subgraph cluster_" + v.noderef + "{\n";
                        s += "label=\"" + v.label + "\";\n";
                        //create a node inside each map that represents the map (so other nodes can link to it)
                        s += v.noderef + " [label=\"map_" + v.label + "\", shape=point, style=filled, fillcolor=blue];\n";
                        //recurse to support nested clusters
                        s += handleNodes(xmlDoc, v.noderef);
                        s += "}\n";
                    //}
                }
                else {
                    s += v.noderef + " [label=\"" + v.label + "\"";
                    switch (v.type) {
                        case "1":
                            s += ",fillcolor=green4, style=filled";
                            break;
                        case "3":
                            s += ",fillcolor=lightblue, style=filled";
                            break;
                        case "4":
                            s += ",fillcolor=yellow, style=filled";
                            break;
                        case "5":
                            s += ",fillcolor=orange, style=filled";
                            break;
                        case "6":
                            s += ",fillcolor=green, style=filled";
                            break;
                        case "7":
                            s += ",fillcolor=red, style=filled";
                            break;
                        case "8":
                            s += ",fillcolor=magenta, style=filled";
                            break;
                        case "9":
                            s += ",fillcolor=gray, style=filled";
                            break;
                        case "10":
                            s += ",fillcolor=lightgray, style=filled";
                            break;
                        default:
                            break;

                    }
                    s += "]\n";
                }
            }

            return s;
        }

        private static void genDot(string xmlFile) {
            XDocument xmlDoc = XDocument.Load(xmlFile);
            TextWriter tw = new StreamWriter(xmlFile + ".dot");

            tw.WriteLine("digraph " + Path.GetFileNameWithoutExtension(xmlFile) + " {");
            tw.WriteLine("graph [fontname=arial, overlap=false, compound=true, concentrate=true, overlap=false, fontsize=11];");
            tw.WriteLine("node [fontname=arial, fontsize=11];");

            String rootView = xmlDoc.Root.Attribute("rootview").Value;

            String s = handleNodes(xmlDoc, rootView);

            tw.WriteLine(s);
            
            //Just do it in three queries already
            var fromlinks = from link in xmlDoc.Descendants("link")
                            join node in xmlDoc.Descendants("node") on link.Attribute("from").Value equals node.Attribute("id").Value
                            join node2 in xmlDoc.Descendants("node") on link.Attribute("to").Value equals node2.Attribute("id").Value
                            select new {
                                fromNode = link.Attribute("from").Value,
                                fromlabel = node.Attribute("label").Value,
                                fromtype = node.Attribute("type").Value,
                                toNode = link.Attribute("to").Value,
                                tolabel = node2.Attribute("label").Value,
                                totype = node2.Attribute("type").Value
                            };

            
         
            foreach (var link in fromlinks) {
                //The link between cluster things dont seem to work -> commented out and replaced by dummy nodes (see above)
                
                if (link.fromtype.Equals("2") && link.totype.Equals("2")) {
                    //Console.WriteLine("Map to Map: " + link.fromlabel + " -> " + link.tolabel);
                    //tw.WriteLine(link.fromNode + " -> " + link.toNode + " [ltail=cluster_" + link.fromNode + ",lhead=cluster_" + link.toNode + "];");
                }
                else if (link.fromtype.Equals("2") && !link.totype.Equals("2")) {
                    //Console.WriteLine("Map to Node: " + link.fromlabel + " -> " + link.tolabel);
                    //tw.WriteLine(link.fromNode + " -> " + link.toNode + " [ltail=cluster_" + link.fromNode + "];");
                }
                else if (!link.fromtype.Equals("2") && link.totype.Equals("2")) {
                    //Console.WriteLine("Node to Map: " + link.fromlabel + " -> " + link.tolabel);
                    //tw.WriteLine(link.fromNode + " -> " + link.toNode + " [lhead=cluster_" + link.toNode + "];");
                }
                else {
                    //Console.WriteLine("Node to Node: " + link.fromlabel + " -> " + link.tolabel);
                    //tw.WriteLine(link.fromNode + " -> " + link.toNode + ";");
                }

                tw.WriteLine(link.fromNode + " -> " + link.toNode + ";");
            }


            tw.WriteLine("}");
            tw.Close();

            Console.WriteLine("Done...");
            Console.WriteLine("-- Press Enter to quit ---");
            Console.ReadLine();
        }
    }
}
