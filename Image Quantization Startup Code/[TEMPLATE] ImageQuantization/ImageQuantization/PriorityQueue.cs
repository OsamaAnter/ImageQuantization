using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Priority_Queue
{
    public class FastPriorityQueueNode
    {
        /// <summary>
        /// The Priority to insert this node at.
        /// Should not be manually edited once the node has been enqueued - use queue.UpdatePriority() instead
        /// </summary>
        public float Priority { get; protected internal set; }

        /// <summary>
        /// Represents the current position in the queue
        /// </summary>
        public int IndexQueue { get; internal set; }
    }

    
    public sealed class FastPriorityQueue<T>   where T : FastPriorityQueueNode
    {
        private int _numNodes;
        private T[] Nodes;

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        public FastPriorityQueue(int NodesMax)
        {

            if (NodesMax <= 0)
            {
                throw new InvalidOperationException("New queue size cannot be smaller than 1");
            }


            _numNodes = 0;
            Nodes = new T[NodesMax + 1];
        }

        /// <summary>
        /// Returns the number of nodes in the queue.
        /// O(1)
        /// </summary>
        public int Count
        {
            get
            {
                return _numNodes;
            }
        }


        /// <summary>
        /// Returns (in O(1)!) whether the given node is in the queue.  O(1)
        /// </summary>

        public bool Contains(T node)
        {

            if (node == null)
            {
                throw new ArgumentNullException("Node NULL");
            }
            if (node.IndexQueue < 0 || node.IndexQueue >= Nodes.Length)
            {
                throw new InvalidOperationException("node.QueueIndex has been corrupted.");
            }


            return (Nodes[node.IndexQueue] == node);
        }

        /// <summary>
        /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// If the queue is full, the result is undefined.
        /// If the node is already enqueued, the result is undefined.
        /// O(log n)
        /// </summary>

        public void Enqueue(T node, float priority)
        {

            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (_numNodes >= Nodes.Length - 1)
            {
                throw new InvalidOperationException("Queue is full - node cannot be added: " + node);
            }
            if (Contains(node))
            {
                throw new InvalidOperationException("Node is already enqueued: " + node);
            }


            node.Priority = priority;
            _numNodes++;
            Nodes[_numNodes] = node;
            node.IndexQueue = _numNodes;
            CascadeUp(Nodes[_numNodes]);
        }

        private void Swap(T node1, T node2)
        {
            //Swap the nodes
            Nodes[node1.IndexQueue] = node2;
            Nodes[node2.IndexQueue] = node1;

            //Swap their indicies
            int temp = node1.IndexQueue;
            node1.IndexQueue = node2.IndexQueue;
            node2.IndexQueue = temp;
        }

        //Performance appears to be slightly better when this is NOT inlined o_O
        private void CascadeUp(T node)
        {
            //aka Heapify-up
            int parent = node.IndexQueue / 2;
            while (parent >= 1)
            {
                T parentNode = Nodes[parent];
                if (HasHigherPriority(parentNode, node))
                    break;

                //Node has lower priority value, so move it up the heap
                Swap(node, parentNode); 

                parent = node.IndexQueue / 2;
            }
        }

        private void CascadeDown(T node)
        {
            //aka Heapify-down
            T newParent;
            int finalQueueIndex = node.IndexQueue;
            while (true)
            {
                newParent = node;
                int childLeftIndex = 2 * finalQueueIndex;

                //Check if the left-child is higher-priority than the current node
                if (childLeftIndex > _numNodes)
                {
                    //This could be placed outside the loop, but then we'd have to check newParent != node twice
                    node.IndexQueue = finalQueueIndex;
                    Nodes[finalQueueIndex] = node;
                    break;
                }

                T childLeft = Nodes[childLeftIndex];
                if (HasHigherPriority(childLeft, newParent))
                {
                    newParent = childLeft;
                }

                //Check if the right-child is higher-priority than either the current node or the left child
                int childRightIndex = childLeftIndex + 1;
                if (childRightIndex <= _numNodes)
                {
                    T childRight = Nodes[childRightIndex];
                    if (HasHigherPriority(childRight, newParent))
                    {
                        newParent = childRight;
                    }
                }

                //If the children has(smaller) priority, swap and continue cascading
                if (newParent != node)
                {
                    //Move new parent to its new index.  node will be moved once, at the end
                    //Doing it this way is one less assignment operation than calling Swap()
                    Nodes[finalQueueIndex] = newParent;

                    int temp = newParent.IndexQueue;
                    newParent.IndexQueue = finalQueueIndex;
                    finalQueueIndex = temp;
                    continue;
                }
                else
                {
                    //See note above
                    node.IndexQueue = finalQueueIndex;
                    Nodes[finalQueueIndex] = node;
                    break;
                }
            }
        }

        /// <summary>
        /// Returns true if 'higher' has higher priority than 'lower', false otherwise.
        /// Note that calling HasHigherPriority(node, node) (ie. both arguments the same node) will return false
        /// </summary>

        private bool HasHigherPriority(T higher, T lower)
        {
            return (higher.Priority < lower.Priority);
        }

        /// <summary>
        /// Removes the head of the queue and returns it.
        /// If queue is empty, result is undefined
        /// O(log n)
        /// </summary>
        public T Dequeue()
        {
            T Returnelement;
            if (_numNodes <= 0)
            {
                throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
            }

            if (!IsValidQueue())
            {
                throw new InvalidOperationException("Queue has been corrupted");
            }

            Returnelement = Nodes[1];
            Remove(Returnelement);
            return Returnelement;
        }

       
        /// <summary>
        /// This method must be called on a node every time its priority changes while it is in the queue.  
        /// <b>Forgetting to call this method will result in a corrupted queue!</b>
        /// Calling this method on a node not in the queue results in undefined behavior
        /// O(log n)
        /// </summary>

        public void UpdatePriority(T Nodeinput, float priority)
        {

            if (Nodeinput == null)
            {
                throw new ArgumentNullException("node NULL");
            }
            if (Contains(Nodeinput) == false)
            {
                throw new InvalidOperationException(" node not enqueued: " + Nodeinput);
            }

            Nodeinput.Priority = priority;
            OnNodeUpdated(Nodeinput);
        }

        private void OnNodeUpdated(T NodeNew)
        {
            //Bubble the updated node up or down as appropriate
            int IndexParent = NodeNew.IndexQueue / 2;
            T parentNode = Nodes[IndexParent];

            if (IndexParent > 0 && HasHigherPriority(NodeNew, parentNode))
            {
                CascadeUp(NodeNew);
            }
            else
            {
                // CascadeDown will be called if parentNode == node (that is, node is the root)
                CascadeDown(NodeNew);
            }
        }

        /// <summary>
        /// Removes a node from the queue.  The node does not need to be the head of the queue.  
        /// If the node is not in the queue, the result is undefined.  If unsure, check Contains() first
        /// O(log n)
        /// </summary>
        public void Remove(T node)
        {
            T LastNode;
            if (node == null)
            {
                throw new ArgumentNullException("node NULL");
            }
            if (Contains(node) == false)
            {
                throw new InvalidOperationException(" node not enqueued: " + node);
            }

            //If the node is already the last node, we can remove it immediately
            if (node.IndexQueue == _numNodes)
            {
                Nodes[_numNodes] = null;
                _numNodes-=1;
                return;
            }

            //Swap the node with the last node
            LastNode = Nodes[_numNodes];
            Swap(node, LastNode);
            Nodes[_numNodes] = null;
            _numNodes-=1;

            //Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
            OnNodeUpdated(LastNode);
        }

        public IEnumerator<T> GetEnumerator()
        {
            int counter = 1;
            while ( counter <= _numNodes)
            { 
                yield return Nodes[counter];
                counter++;
            }
                
        }

        /// <summary>
        /// <b>Should not be called in production code.</b>
        /// Checks to make sure the queue is still in a valid state.  Used for testing/debugging the queue.
        /// </summary>
        public bool IsValidQueue()
        {
            int length = Nodes.Length;
            for (int i = 1; i < length; i++)
            {
                if (Nodes[i] != null)
                {
                    int leftChildIndex = i*2 ;
                    if (leftChildIndex < Nodes.Length && Nodes[leftChildIndex] != null )
                        if (HasHigherPriority(Nodes[leftChildIndex], Nodes[i]))
                        {
                             return false;
                        }
                       

                    int RightChildIndex = leftChildIndex + 1;
                    if (RightChildIndex < Nodes.Length && Nodes[RightChildIndex] != null )
                        if(HasHigherPriority(Nodes[RightChildIndex], Nodes[i]))
                        {
                            return false;
                        }
                }
            }
            return true;
        }
    }
}