using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROTAGeneticAlgo
{
    class Program
    {
        //1 & 1 & 1 = 1
        //1 & 1 & 2 = 0
        //1 & 2 & 1 = 0
        //2 & 1 & 1 = 0
        //1 & 2 & 2 = 0
        //2 & 1 & 2 = 0
        //2 & 2 & 1 = 0
        //2 & 2 & 2 = 2
        static void Main(string[] args)
        {
            ROTAGeneticAlgo.Program program = new ROTAGeneticAlgo.Program();

            program.Start(1000);
        }

        /// <summary>
        /// Starts the algo of finding the best chromosomes that can play ROTA at top level
        /// </summary>
        public void Start(int chromCount)
        {
            // [Prerequisites] generate all posible boardgame states; there are two types of states here:
            //   1. beginning phase: when the 6 pieces are placed on the board
            //   2. second phase: after the 6 pieces are placed on the board

            Dictionary<ulong, List<BoardState>> boards = BoardState.GenerateAllBoardsForColors(Color.Red, Color.Blue);
            Console.WriteLine("{0}\n", boards.ToPrintString());

            // [Start] Generate random population of n chromosomes (suitable solutions for the problem)
            // [Fitness] Evaluate the fitness f(x) of each chromosome x in the population
            // [New population] Create a new population by repeating following steps until the new population is complete
            // [Selection] Select two parent chromosomes from a population according to their fitness (the better fitness, the bigger chance to be selected)
            // [Crossover] With a crossover probability cross over the parents to form a new offspring (children). If no crossover was performed, offspring is an exact copy of parents.
            // [Mutation] With a mutation probability mutate new offspring at each locus (position in chromosome).
            // [Accepting] Place new offspring in a new population
            // [Replace] Use new generated population for a further run of algorithm
            // [Test] If the end condition is satisfied, stop, and return the best solution in current population
            // [Loop] Go to step 2

        }
    }

    public enum Color
    {
        _Empty = 0,
        Red = 1,
        Blue = 2
    }

    public class Position
    {
        public Position(Position pos)
        {
            this.Pos = pos.Pos;
            this.Col = pos.Col;
        }
        public Position(int pos, Color col)
        {
            this.Pos = pos;
            this.Col = col;
        }
        public Color Col;
        public int Pos; // 1..8
    }
    // the cirque will hold always 8 ints, 1 for red, 2 for blue, 0 for empty
    // the center will hold always 1 int, 1 for red, 2 for blue, 0 for empty
    public sealed class BoardState
    {
        private CircularLinkedList<Position> rotaLine = null;
        private Color rotaCenter = 0;

        public BoardState()
        {
            this.rotaCenter = 0;
            this.rotaLine = new CircularLinkedList<Position>();
            // initialize the board as empty
            for (int count=1; count <= 8; count++)
            {
                this.rotaLine.AddLast(new Position(count, Color._Empty));
            }
        }

        // Copy Ctor
        public BoardState(BoardState board)
        {
            this.rotaCenter = board.rotaCenter;
            this.rotaLine = new CircularLinkedList<Position>();
            // initialize the board as a copy of the input one
            Node<Position> node = board.rotaLine.Head;
            do
            {
                this.rotaLine.AddLast(new Position(node.Value));
                node = node.Next;
            }
            while (node != board.rotaLine.Head);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            Node<Position> node = this.rotaLine.Head;
            int index = 1;
            do
            {
                if (index == 5)
                {
                    sb.AppendFormat("{0}", this.rotaCenter.ToString().ToCharArray()[0]);
                    index++;
                }
                sb.AppendFormat("{0}", node.Value.Col.ToString().ToCharArray()[0]);
                if (index % 3 == 0)
                    sb.AppendLine(string.Empty);

                node = node.Next;
                index++;
            }
            while (node != this.rotaLine.Head);
            return sb.ToString();
        }

        public static Dictionary<ulong, List<BoardState>> GenerateAllBoardsForColors(Color firstColor, Color secondColor)
        {
            Dictionary<ulong, List<BoardState>> allBoards = new Dictionary<ulong, List<BoardState>>();
            BoardState board = new BoardState();

            board.ToString();

            // place the first piece
            board.rotaCenter = firstColor;
            List<BoardState> list = new List<BoardState>();
            BoardState firstBoard = new BoardState(board);
            list.Add(firstBoard);
            allBoards.Add(firstBoard.GetBoardUniqueKey(), list);

            Color[] pieces = new Color[5]{ secondColor, firstColor, secondColor, firstColor, secondColor };

            GenerateAllBoardsForPieces(allBoards, board, pieces);

            pieces = new Color[6] { firstColor, secondColor, firstColor, secondColor, firstColor, secondColor };
            GenerateAllBoardsForPieces(allBoards, new BoardState(), pieces);
            
            return allBoards;
        }

        private static void GenerateAllBoardsForPieces(Dictionary<ulong, List<BoardState>> allBoards, BoardState board, Color[] pieces)
        {
            if (pieces.Count() <= 0)
                return;

            Color currentPiece = pieces[0];
            Color[] remainingPieces = new Color[pieces.Count() - 1];

            for (int idx = 1; idx < pieces.Count(); idx++)
                remainingPieces[idx-1] = pieces[idx];

            Node<Position> node = board.rotaLine.Head;
            do
            {
                // if empty spot then ocupy it
                if (node.Value.Col == Color._Empty)
                {
                    node.Value.Col = currentPiece;
                    BoardState newBoard = new BoardState(board);
                    ulong key = newBoard.GetBoardUniqueKey();
                    if (!allBoards.Keys.Contains(key))
                    {
                        List<BoardState> list = new List<BoardState>();
                        list.Add(newBoard);
                        allBoards.Add(key, list);
                    }
                    else
                    {
                        allBoards[key].Add(newBoard); ;
                    }
                    GenerateAllBoardsForPieces(allBoards, board, remainingPieces);
                    node.Value.Col = Color._Empty;
                }
                node = node.Next;
            }
            while (node != board.rotaLine.Head);
        }

        /// <summary>
        /// this key is calculated based on the distance between each pieces of the same color
        /// center 31
        /// pos 1, 2, 3, ...8 = 5, 7, 11, 13, 17, 19, 23, 29
        /// red = 2
        /// blue = 3
        /// empty = 1
        /// </summary>
        /// <returns></returns>
        public ulong GetBoardUniqueKey()
        {
            ulong key = 31;
            switch (this.rotaCenter)
            {
                case Color.Red: key *= 2;
                    break;
                case Color.Blue: key *= 3;
                    break;
                default: key *= 1;
                    break;
            }

            int distance = 0;
            Node<Position> node = this.rotaLine.Head;
            Color currentColor = Color._Empty;
            bool firstRed = false;
            do
            {
                if (!firstRed || node.Value.Col != Color._Empty)
                {
                    if (node.Value.Col == Color.Red)
                        firstRed = true;
                    if (node.Value.Col != currentColor)
                        currentColor = node.Value.Col;
                    switch (distance)
                    {
                        case 1:
                        case 7:
                            key *= 5;
                            break;
                        case 2:
                        case 6:
                            key *= 7;
                            break;
                        case 3:
                        case 5:
                            key *= 11;
                            break;
                        case 4: key *= 13;
                            break;
                        case 8: key *= 17;
                            break;
                        default: key *= 0; // TO DO: signal thi bug in a nicer way
                            break;
                    }
                    switch (node.Value.Col)
                    {
                        case Color.Red: key *= 2;
                            break;
                        case Color.Blue: key *= 3;
                            break;
                        default: key *= 1;
                            break;
                    }
                }
                node = node.Next;
            }
            while (node != this.rotaLine.Head);

            return key;
        }

        private int CalculateKeyForColor(Color color)
        {
            int key = 1;
            int distance = 0;
            bool firstNode = true;
            Node<Position> node = this.rotaLine.Head;
            do
            {
                if (node.Value.Col == color)
                {
                    if (firstNode)
                    {
                        firstNode = false;
                    }
                    else
                    {
                        key *= GetPrimeBasedOnDistance(distance, color);
                    }
                    distance = 0;
                }
                node = node.Next;
                distance++;
            }
            while (node != this.rotaLine.Head);

            if (firstNode)
            {
                key *= GetPrimeBasedOnDistance(distance, color);
            }
            return key;
        }

        private int GetPrimeBasedOnDistance(int distance, Color color)
        {
            /// if distance between two pieces of red  is 1, 2, ..., 6, 7, 8 then these prime numbers are used 3, 5, 7, 11, 13, 17, 19, 23
            /// if distance between two pieces of blue is 1, 2, ..., 6, 7, 8 then these prime numbers are used 29, 31, 37, 41, 43, 47, 53, 59
            if (color == Color.Red)
                switch (distance)
                {
                    case 1: return 3;
                    case 2: return 5;
                    case 3: return 7;
                    case 4: return 11;
                    case 5: return 13;
                    case 6: return 17;
                    case 7: return 19;
                    case 8: return 23;
                    default:
                        // TO DO: signale it, log it in a nicer way
                        Console.WriteLine("BUG BUG BUG! there's a bug in calculating the distance 1");
                        return 0; // this should never happen
                }
            if (color == Color.Blue)
                switch (distance)
                {
                    case 1: return 29;
                    case 2: return 31;
                    case 3: return 37;
                    case 4: return 41;
                    case 5: return 43;
                    case 6: return 47;
                    case 7: return 53;
                    case 8: return 59;
                    default:
                        // TO DO: signale it, log it in a nicer way
                        Console.WriteLine("BUG BUG BUG! there's a bug in calculating the distance 2");
                        return 0; // this should never happen
                }
            // TO DO: signale it, log it in a nicer way
            Console.WriteLine("BUG BUG BUG! there's a bug in calculating the distance 3");
            return 0; // this should never happen
        }
        /// <summary>
        /// Verify and returns this board state
        /// </summary>
        /// <returns>0 in progress or 1 if player 1 wins or 2 player 2 wins</returns>
        public int VerifyBoard()
        {
            int state = 0;
            // start with any node, 
            // 1. verify if three consecutive nodes have the same color
            // 2. verify if two oposing nodes from rotaLine has the same color with the rotaCenter
            Node<Position> node = this.rotaLine.Head;
            do
            {
                //1. look at three consecutive nodes
                state = (int)node.Value.Col & (int)node.Next.Value.Col & (int)node.Next.Value.Col;
                if (state != 0)
                    return state;

                // 2. look at the current node, oposite node from rotaLine and center
                state = (int)node.Value.Col & (int)node.Next.Next.Next.Next.Next.Value.Col & (int)rotaCenter;
                if (state != 0)
                    return state;

                node = node.Next;
            }
            while (node != this.rotaLine.Head);
            return state;
        }

        public bool Equal(BoardState board)
        {
            bool equal = true;
            if (this.rotaCenter != board.rotaCenter)
            {
                equal = false;
            }
            else
            {
                Node<Position> thisHead = this.rotaLine.Head;
                Node<Position> boardHead = board.rotaLine.Head;
                do
                {
                    if (thisHead.Value != boardHead.Value)
                    {
                        equal = false;
                        break;
                    }
                    else
                    {
                        thisHead = thisHead.Next;
                        boardHead = boardHead.Next;
                    }
                }
                while (thisHead != this.rotaLine.Head && boardHead != board.rotaLine.Head);
                if (equal)
                {   // this case should never be possible; it means we have a bug in the syste;
                    if (thisHead != this.rotaLine.Tail || boardHead != board.rotaLine.Tail)
                    {
                        // TO DO: Report this bug
                        Console.WriteLine("BUG BUG BUG! boards with different size!");
                        equal = false;
                    }
                }
            }
            return equal;
        }
        /// <summary>
        /// This function finds out if two boards are identical if one is kept fixed (board) and the other (this) is rotated up to a full cycle, 7 steps that is
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public bool SimetricallyEqual(BoardState board)
        {
            bool equal = false;

            // start rotating this board with head position
            Node<Position> thisNode = this.rotaLine.Head;
            do
            {
                Node<Position> thisPivot = thisNode;
                Node<Position> boardPivot = board.rotaLine.Head;
                bool boardsEqual = true;
                do
                {
                    if (boardPivot.Value != thisPivot.Value)
                    {
                        boardsEqual = false;
                        break;
                    }
                    else
                    {
                        thisPivot = thisPivot.Next;
                        boardPivot = boardPivot.Next;
                    }
                }
                while (boardPivot != board.rotaLine.Head && thisPivot != this.rotaLine.Head);
                if (boardsEqual && (boardPivot != board.rotaLine.Head || thisPivot != this.rotaLine.Head))
                {
                    // TO DO: Report this bug
                    Console.WriteLine("BUG BUG BUG! boards with different size!");
                    boardsEqual = false;
                }
                else if (boardsEqual)
                {
                    equal = true;
                    break;
                }
                thisNode = thisNode.Next;
            }
            while (thisNode != this.rotaLine.Head);
            return equal;
        }
    }

    /// <summary>
    /// Node used in the circular linked list
    /// </summary>
    public sealed class Node<T>
    {
        public T Value { get; set; }
        public Node<T> Next { get; internal set; }
        public Node<T> Previous { get; internal set; }
        internal Node(T item)
        {
            this.Value = item;
        }
    }

    /// <summary>
    /// Circular linked list, will be used to store the 8 out of 9 positions of the board
    /// taken from 
    /// https://navaneethkn.wordpress.com/2009/08/18/circular-linked-list/#more-197
    /// </summary>
    public sealed class CircularLinkedList<T> : ICollection<T>, IEnumerable<T>
    {
        Node<T> head = null;
        Node<T> tail = null;
        int count = 0;
        readonly IEqualityComparer<T> comparer;

        /// <summary>
        /// Initializes a new instance of <see cref="CircularLinkedList"/>
        /// </summary>
        public CircularLinkedList()
            : this(null, EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CircularLinkedList"/>
        /// </summary>
        /// <param name="collection">Collection of objects that will be added to linked list</param>
        public CircularLinkedList(IEnumerable<T> collection)
            : this(collection, EqualityComparer<T>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CircularLinkedList"/>
        /// </summary>
        /// <param name="comparer">Comparer used for item comparison</param>
        /// <exception cref="ArgumentNullException">comparer is null</exception>
        public CircularLinkedList(IEqualityComparer<T> comparer)
            : this(null, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CircularLinkedList"/>
        /// </summary>
        /// <param name="collection">Collection of objects that will be added to linked list</param>
        /// <param name="comparer">Comparer used for item comparison</param>
        public CircularLinkedList(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");
            this.comparer = comparer;
            if (collection != null)
            {
                foreach (T item in collection)
                    this.AddLast(item);
            }
        }

        /// <summary>
        /// Gets Tail node. Returns NULL if no tail node found
        /// </summary>
        public Node<T> Tail { get { return tail; } }

        /// <summary>
        /// Gets the head node. Returns NULL if no node found
        /// </summary>
        public Node<T> Head { get { return head; } }

        /// <summary>
        /// Gets total number of items in the list
        /// </summary>
        public int Count { get { return count; } }

        /// <summary>
        /// Gets the item at the current index
        /// </summary>
        /// <param name="index">Zero-based index</param>
        /// <exception cref="ArgumentOutOfRangeException">index is out of range</exception>
        public Node<T> this[int index]
        {
            get
            {
                if (index >= count || index < 0)
                    throw new ArgumentOutOfRangeException("index");
                else
                {
                    Node<T> node = this.head;
                    for (int i = 0; i < index; i++)
                        node = node.Next;
                    return node;
                }
            }
            set
            {
                if (index >= count || index < 0)
                    throw new ArgumentOutOfRangeException("index");
                else
                {
                    Node<T> node = this.head;
                    for (int i = 0; i < index; i++)
                        node = node.Next;
                    node.Value = value.Value;
                }
            }
        }

        /// <summary>
        /// Add a new item to the end of the list
        /// </summary>
        /// <param name="item">Item to be added</param>
        public void AddLast(T item)
        {
            // if head is null, then this will be the first item
            if (head == null)
                this.AddFirstItem(item);
            else
            {
                Node<T> newNode = new Node<T>(item);
                tail.Next = newNode;
                newNode.Next = head;
                newNode.Previous = tail;
                tail = newNode;
                head.Previous = tail;
            }
            ++count;
        }

        void AddFirstItem(T item)
        {
            head = new Node<T>(item);
            tail = head;
            head.Next = tail;
            head.Previous = tail;
        }

        /// <summary>
        /// Adds item to the last
        /// </summary>
        /// <param name="item">Item to be added</param>
        public void AddFirst(T item)
        {
            if (head == null)
                this.AddFirstItem(item);
            else
            {
                Node<T> newNode = new Node<T>(item);
                head.Previous = newNode;
                newNode.Previous = tail;
                newNode.Next = head;
                tail.Next = newNode;
                head = newNode;
            }
            ++count;
        }

        /// <summary>
        /// Adds the specified item after the specified existing node in the list.
        /// </summary>
        /// <param name="node">Existing node after which new item will be inserted</param>
        /// <param name="item">New item to be inserted</param>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is NULL</exception>
        /// <exception cref="InvalidOperationException"><paramref name="node"/> doesn't belongs to list</exception>
        public void AddAfter(Node<T> node, T item)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            // ensuring the supplied node belongs to this list
            Node<T> temp = this.FindNode(head, node.Value);
            if (temp != node)
                throw new InvalidOperationException("Node doesn't belongs to this list");

            Node<T> newNode = new Node<T>(item);
            newNode.Next = node.Next;
            node.Next.Previous = newNode;
            newNode.Previous = node;
            node.Next = newNode;

            // if the node adding is tail node, then repointing tail node
            if (node == tail)
                tail = newNode;
            ++count;
        }

        /// <summary>
        /// Adds the new item after the specified existing item in the list.
        /// </summary>
        /// <param name="existingItem">Existing item after which new item will be added</param>
        /// <param name="newItem">New item to be added to the list</param>
        /// <exception cref="ArgumentException"><paramref name="existingItem"/> doesn't exist in the list</exception>
        public void AddAfter(T existingItem, T newItem)
        {
            // finding a node for the existing item
            Node<T> node = this.Find(existingItem);
            if (node == null)
                throw new ArgumentException("existingItem doesn't exist in the list");
            this.AddAfter(node, newItem);
        }

        /// <summary>
        /// Adds the specified item before the specified existing node in the list.
        /// </summary>
        /// <param name="node">Existing node before which new item will be inserted</param>
        /// <param name="item">New item to be inserted</param>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is NULL</exception>
        /// <exception cref="InvalidOperationException"><paramref name="node"/> doesn't belongs to list</exception>
        public void AddBefore(Node<T> node, T item)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            // ensuring the supplied node belongs to this list
            Node<T> temp = this.FindNode(head, node.Value);
            if (temp != node)
                throw new InvalidOperationException("Node doesn't belongs to this list");

            Node<T> newNode = new Node<T>(item);
            node.Previous.Next = newNode;
            newNode.Previous = node.Previous;
            newNode.Next = node;
            node.Previous = newNode;

            // if the node adding is head node, then repointing head node
            if (node == head)
                head = newNode;
            ++count;
        }

        /// <summary>
        /// Adds the new item before the specified existing item in the list.
        /// </summary>
        /// <param name="existingItem">Existing item before which new item will be added</param>
        /// <param name="newItem">New item to be added to the list</param>
        /// <exception cref="ArgumentException"><paramref name="existingItem"/> doesn't exist in the list</exception>
        public void AddBefore(T existingItem, T newItem)
        {
            // finding a node for the existing item
            Node<T> node = this.Find(existingItem);
            if (node == null)
                throw new ArgumentException("existingItem doesn't exist in the list");
            this.AddBefore(node, newItem);
        }

        /// <summary>
        /// Finds the supplied item and returns a node which contains item. Returns NULL if item not found
        /// </summary>
        /// <param name="item">Item to search</param>
        /// <returns><see cref="Node&lt;T&gt;"/> instance or NULL</returns>
        public Node<T> Find(T item)
        {
            Node<T> node = FindNode(head, item);
            return node;
        }

        /// <summary>
        /// Removes the first occurance of the supplied item
        /// </summary>
        /// <param name="item">Item to be removed</param>
        /// <returns>TRUE if removed, else FALSE</returns>
        public bool Remove(T item)
        {
            // finding the first occurance of this item
            Node<T> nodeToRemove = this.Find(item);
            if (nodeToRemove != null)
                return this.RemoveNode(nodeToRemove);
            return false;
        }

        bool RemoveNode(Node<T> nodeToRemove)
        {
            Node<T> previous = nodeToRemove.Previous;
            previous.Next = nodeToRemove.Next;
            nodeToRemove.Next.Previous = nodeToRemove.Previous;

            // if this is head, we need to update the head reference
            if (head == nodeToRemove)
                head = nodeToRemove.Next;
            else if (tail == nodeToRemove)
                tail = tail.Previous;

            --count;
            return true;
        }

        /// <summary>
        /// Removes all occurances of the supplied item
        /// </summary>
        /// <param name="item">Item to be removed</param>
        public void RemoveAll(T item)
        {
            bool removed = false;
            do
            {
                removed = this.Remove(item);
            } while (removed);
        }

        /// <summary>
        /// Clears the list
        /// </summary>
        public void Clear()
        {
            head = null;
            tail = null;
            count = 0;
        }

        /// <summary>
        /// Removes head
        /// </summary>
        /// <returns>TRUE if successfully removed, else FALSE</returns>
        public bool RemoveHead()
        {
            return this.RemoveNode(head);
        }

        /// <summary>
        /// Removes tail
        /// </summary>
        /// <returns>TRUE if successfully removed, else FALSE</returns>
        public bool RemoveTail()
        {
            return this.RemoveNode(tail);
        }

        Node<T> FindNode(Node<T> node, T valueToCompare)
        {
            Node<T> result = null;
            if (comparer.Equals(node.Value, valueToCompare))
                result = node;
            else if (result == null && node.Next != head)
                result = FindNode(node.Next, valueToCompare);
            return result;
        }

        /// <summary>
        /// Gets a forward enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            Node<T> current = head;
            if (current != null)
            {
                do
                {
                    yield return current.Value;
                    current = current.Next;
                } while (current != head);
            }
        }

        /// <summary>
        /// Gets a reverse enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetReverseEnumerator()
        {
            Node<T> current = tail;
            if (current != null)
            {
                do
                {
                    yield return current.Value;
                    current = current.Previous;
                } while (current != tail);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Determines whether a value is in the list.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>TRUE if item exist, else FALSE</returns>
        public bool Contains(T item)
        {
            return Find(item) != null;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex");

            Node<T> node = this.head;
            do
            {
                array[arrayIndex++] = node.Value;
                node = node.Next;
            } while (node != head);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<T>.Add(T item)
        {
            this.AddLast(item);
        }
    }
}
