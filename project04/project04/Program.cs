/**
 * Project04
 * CS 212A
 * Jonathan Winkle
 * Bingo program: can perform a variety of commands that involve finding the relationships between two people
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

namespace Bingo
{
    class Program
    {
        private static RelationshipGraph rg;

        // Read RelationshipGraph whose filename is passed in as a parameter.
        // Build a RelationshipGraph in RelationshipGraph rg
        private static void ReadRelationshipGraph(string filename)
        {
            rg = new RelationshipGraph();                           // create a new RelationshipGraph object

            string name = "";                                       // name of person currently being read
            int numPeople = 0;
            string[] values;
            Console.Write("Reading file " + filename + "\n");
            try
            {
                string input = System.IO.File.ReadAllText(filename);// read file
                input = input.Replace("\r", ";");                   // get rid of nasty carriage returns 
                input = input.Replace("\n", ";");                   // get rid of nasty new lines
                string[] inputItems = Regex.Split(input, @";\s*");  // parse out the relationships (separated by ;)
                foreach (string item in inputItems)
                {
                    if (item.Length > 2)                            // don't bother with empty relationships
                    {
                        values = Regex.Split(item, @"\s*:\s*");     // parse out relationship:name
                        if (values[0] == "name")                    // name:[personname] indicates start of new person
                        {
                            name = values[1];                       // remember name for future relationships
                            rg.AddNode(name);                       // create the node
                            numPeople++;
                        }
                        else
                        {
                            rg.AddEdge(name, values[1], values[0]); // add relationship (name1, name2, relationship)

                            // handle symmetric relationships -- add the other way
                            if (values[0] == "hasSpouse" || values[0] == "hasFriend")
                                rg.AddEdge(values[1], name, values[0]);

                            // for parent relationships add child as well
                            else if (values[0] == "hasParent")
                                rg.AddEdge(values[1], name, "hasChild");
                            else if (values[0] == "hasChild")
                                rg.AddEdge(values[1], name, "hasParent");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("Unable to read file {0}: {1}\n", filename, e.ToString());
            }
            Console.WriteLine(numPeople + " people read");
        }

        // Show the relationships a person is involved in
        private static void ShowPerson(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
                Console.Write(n.ToString());
            else
                Console.WriteLine("{0} not found", name);
        }

        // Show a person's friends
        private static void ShowFriends(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
            {
                Console.Write("{0}'s friends: ", name);
                List<GraphEdge> friendEdges = n.GetEdges("hasFriend");
                foreach (GraphEdge e in friendEdges)
                {
                    Console.Write("{0} ", e.To().Name);
                }
                Console.WriteLine();
            }
            else
                Console.WriteLine("{0} not found", name);
        }

        /**
         * Orphans function
         * @param: none
         * author: Jonathan Winkle
         * returns: a list of all of the people who are orphans
         */
        private static void Orphans()
        {
            Console.Write("Orphans: ");
            int totalOrphans = 0;

            // make a list of the person nodes in the graph
            List<GraphNode> personList = rg.nodes.GetRange(0, rg.nodes.Count - 1);
            foreach (GraphNode n in personList)
            {
                List<GraphEdge> parentEdges = n.GetEdges("hasParent");
                if (parentEdges.Count == 0)
                {
                    Console.Write("{0} ", n.Name);
                    totalOrphans += 1;
                }
            }
            if (totalOrphans == 0)
                Console.WriteLine("There are no orphans");
            else
            {
                Console.WriteLine();
                Console.WriteLine("There are {0} orphans", totalOrphans);
            }
        }

        /**
         * Siblings function
         * @param: one person
         * author: Jonathan Winkle
         * returns: a list of all of the siblings of the parameter person
         */
        private static void Siblings(string person)
        {
            int totalSiblings = 0;
            List<GraphNode> siblingsList = new List<GraphNode>();
            GraphNode personNode = rg.GetNode(person);
            List<GraphEdge> parentEdges = personNode.GetEdges("hasParent");

            // if the person isn't an orphan
            if (parentEdges.Count != 0)
            {
                // for each parent (to account for step-siblings)
                foreach (GraphEdge edge in parentEdges)
                {
                    // get the parent node and find its children relationships
                    GraphNode parentNode = edge.To();
                    List<GraphEdge> siblingsEdges = parentNode.GetEdges("hasChild");

                    // for every child, add it to the sibling list
                    foreach (GraphEdge siblingEdge in siblingsEdges)
                    {
                        GraphNode sibNode = siblingEdge.To();
                        if (sibNode.Name != person && !siblingsList.Contains(sibNode))
                        {
                            siblingsList.Add(sibNode);
                            totalSiblings++;
                        }
                    }
                }
                Console.WriteLine("{0} has siblings: \n", person);
                foreach (GraphNode node in siblingsList)
                {
                    Console.WriteLine("{0}\n", node.Name);
                }
            }
            else
            {
                Console.WriteLine("{0} has no siblings", person);
            }
            Console.WriteLine("{0} has {1} sibling(s)", person, totalSiblings);
        }

        /**
         * Bingo function: finds the closest connection between two people
         * @param: two people in the graph
         * author: Jonathan Winkle
         * returns: the connection between the two people
         */
        private static void Bingo(string person1, string person2)
        {
            GraphNode startNode = rg.GetNode(person1);
            GraphNode endNode = rg.GetNode(person2);

            // make sure that the nodes exist
            if (startNode != null && endNode != null)
            {
                // use a queue to do a modified BFS through the graph to find the shortest route between nodes
                // use a hashtable to keep track of visited edges
                Hashtable visitedHash = new Hashtable();
                Queue<GraphNode> BFSQueue = new Queue<GraphNode>();
                BFSQueue.Enqueue(startNode);

                // while you have nodes in the queue and the end node isn't the next one...
                while (BFSQueue.Count != 0 && BFSQueue.Peek() != endNode)
                {
                    // dequeue a node and store its children edges
                    GraphNode temporaryNode = BFSQueue.Dequeue();
                    List<GraphEdge> childEdges = temporaryNode.GetEdges();
                    foreach (GraphEdge edge in childEdges)
                    {
                        if (!visitedHash.Contains(edge.To()))
                        {
                            // store the new edges in the hashtable keeping track of edges
                            visitedHash.Add(edge.To(), temporaryNode);      
                            BFSQueue.Enqueue(edge.To());
                        }
                    }
                }
                // loop for when you've gotten to the end node
                if (BFSQueue.Count >= 1 && BFSQueue.Peek() == endNode)
                {
                    GraphNode currentNode = endNode;
                    GraphNode parentNode = endNode;
                    Stack<string> connectionStack = new Stack<string>();
                    while (currentNode != startNode)
                    {
                        parentNode = (GraphNode)visitedHash[currentNode];
                        List<GraphEdge> parentEdges = parentNode.GetEdges();
                        foreach (GraphEdge edge in parentEdges)
                        {
                            if (edge.To() == currentNode)
                            {
                                connectionStack.Push(edge.ToString());
                                break;
                            }
                        }
                        currentNode = parentNode;
                    }
                    // write out the connections one at a time by popping them off of the stack
                    while (connectionStack.Count != 0)
                        Console.Write(connectionStack.Pop() + "\n");
                }
                else
                {
                    Console.WriteLine("No relationship found between {0} and {1}", person1, person2);
                }
            }
            else
                Console.WriteLine("{0} and/or {1} were not found", person1, person2);
        }

        /**
         * Function to print out a list of all descendants
         * @param: ancestor
         * author: Jonathan Winkle
         * returns: a list of all descendants of a person
         */
        private static void Descendants(string ancestorName)
        {
            GraphNode ancestor = rg.GetNode(ancestorName);
            if (ancestor != null)
            {
                // make queue to handle new descendants being added and a hashtable dictionary to store descendants and their generation removed from the ancestor
                Queue<GraphNode> descendantsQueue = new Queue<GraphNode>();
                Hashtable descendantsHash = new Hashtable();
                descendantsQueue.Enqueue(ancestor);
                descendantsHash.Add(ancestor, 0);
                while (descendantsQueue.Count != 0)
                {
                    GraphNode temporaryNode = descendantsQueue.Dequeue();
                    List<GraphEdge> descentantEdges = temporaryNode.GetEdges("hasChild");
                    foreach (GraphEdge kid in descentantEdges)
                    {
                        // make sure there are not duplicate descendants added
                        if (!descendantsHash.Contains(kid.To()))
                        {
                            descendantsQueue.Enqueue(kid.To());
                            descendantsHash.Add(kid.To(), (int)descendantsHash[temporaryNode] + 1);
                        }

                    }
                }
                //printing results section
                int counter = 0;
                ICollection descendantKeys = descendantsHash.Keys;
                Console.WriteLine("{0} has {1} descendants", ancestorName, descendantsHash.Count - 1);
                ICollection Generations = descendantsHash.Values;
                int generationsCount = 0;
                foreach (int i in Generations)
                {
                    if (i > generationsCount)
                        generationsCount = i;
                }
                while (true)
                {
                    if (counter-1 >= generationsCount)
                        break;
                    foreach (GraphNode descendant in descendantKeys)
                    {
                        if ((int)descendantsHash[descendant] == counter)
                        {
                            // printing out starting statement with the ancestor's name
                            if (counter == 0)
                                Console.WriteLine("{0}'s descendants are:", descendant.Name);

                            // case for children of the ancestor -- one generation removed
                            else if (counter == 1)
                                Console.WriteLine("child: {0}", descendant.Name);

                            // case for grandchildren of the ancestor -- two generations removed
                            else if (counter == 2)
                                Console.WriteLine("grandchild: {0}", descendant.Name);

                            // case for any descendants more than two generations removed from the ancestor
                            else
                            {
                                string relation = "grandchild";
                                for (int i = 0; i < counter; i++)
                                    relation = "great" + relation;
                                Console.WriteLine(relation + ": {0}", descendant.Name);
                            }
                        }
                    }
                    counter++;
                }
            }
            else
            {
                Console.WriteLine("{0} not found", ancestorName);
            }
        }

        // accept, parse, and execute user commands
        private static void CommandLoop()
        {
            string command = "";
            string[] commandWords;
            Console.Write("Welcome to Harry's Dutch Bingo Parlor!\n");

            while (command != "exit")
            {
                Console.Write("\nEnter a command: ");
                command = Console.ReadLine();
                commandWords = Regex.Split(command, @"\s+");        // split input into array of words
                command = commandWords[0];

                if (command == "exit")
                    ;                                               // do nothing

                // read a relationship graph from a file
                else if (command == "read" && commandWords.Length > 1)
                    ReadRelationshipGraph(commandWords[1]);

                // show information for one person
                else if (command == "show" && commandWords.Length > 1)
                    ShowPerson(commandWords[1]);

                else if (command == "friends" && commandWords.Length > 1)
                    ShowFriends(commandWords[1]);

                // dump command prints out the graph
                else if (command == "dump")
                    rg.Dump();

                // orphans comand prints out orphans
                else if (command == "orphans")
                    Orphans();

                // siblings command prints out the siblings of a person
                else if (command == "siblings")
                    Siblings(commandWords[1]);

                // bingo command prints closest connection between two people if it exists
                else if (command == "bingo")
                    Bingo(commandWords[1], commandWords[2]);

                // descendants command prints the list of descendants a person if they exist
                else if (command == "descendants")
                    Descendants(commandWords[1]);


                // illegal command
                else
                    Console.Write("\nLegal commands: read [filename], dump, show [personname],\n  friends [personname], orphans, bingo [personname1] [personname2]\n, descendants [personname], siblings [personname], exit\n");
            }
        }

        static void Main(string[] args)
        {
            CommandLoop();
        }
    }
}