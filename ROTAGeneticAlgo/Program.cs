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

            //string b2 = "B___R____";
            //string b1 = "B________";
            //BoardState testboard1 = new BoardState(b1);
            //BoardState testboard2 = new BoardState(b2);
            //int key1 = testboard1.GetBoardKey();
            //int key2 = testboard2.GetBoardKey();
            //bool equal = testboard1.IsEquivalent(testboard2, true) || testboard1.IsEquivalent(testboard2, false);

            Dictionary<int, List<BoardState>> boards = new Dictionary<int, List<BoardState>>();
            boards = BoardState.GenerateAllBoardsForColors(boards, Color.Red, Color.Blue);
            boards = BoardState.GenerateAllBoardsForColors(boards, Color.Blue, Color.Red);
            //Console.WriteLine("{0}\n", boards.ToPrintString());

            int errorsCount = 0;
            foreach (int key in boards.Keys)
            {
                int idx = 1;
                BoardState firstBoard = null;
                foreach (BoardState board in boards[key])
                {
                    if (idx++ == 1)
                        firstBoard = board;
                    else
                        if (!firstBoard.IsEquivalent(board))
                        {
                            errorsCount++;
                            Console.WriteLine("Error {3} {2}: \n{0}\n{1}", firstBoard, board, key, errorsCount);
                        }
                }
                //Console.WriteLine("Verified key={0}, count={1}\n", key, boards[key].Count);
            }
            Console.WriteLine("Total keys={0}, Errors={1}\n", boards.Keys.Count, errorsCount);

            // [Start] Generate random population of n chromosomes (suitable solutions for the problem)
            int populationSize = 1000;
            List<Chromosome> populationRed = new List<Chromosome>(populationSize/2);
            for (int idx = 0; idx < populationRed.Capacity; idx++)
            {
                Chromosome chrom = new Chromosome(Color.Red);
                foreach (int key in boards.Keys)
                {
                    chrom.genes.Add(key, boards[key][0].GenerateRandomValidMove(chrom.PlayerColor));
                }
            }
            List<Chromosome> populationBlue = new List<Chromosome>(populationSize / 2);
            for (int idx = 0; idx < populationRed.Capacity; idx++)
            {
                Chromosome chrom = new Chromosome(Color.Blue);
                foreach (int key in boards.Keys)
                {
                    chrom.genes.Add(key, boards[key][0].GenerateRandomValidMove(chrom.PlayerColor));
                }
            }

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

    public enum Color : int
    {
        _Empty = 1,
        Red = 3,
        Blue = 5
    }

    public class Chromosome
    {
        public Chromosome (Color color)
        {
            this.PlayerColor = color;
        }
        public Color PlayerColor = Color._Empty;
        public Dictionary<int, PlayerMove> genes = new Dictionary<int,PlayerMove>();
    }

    /// <summary>
    /// If PositionEnd = 0 it means this is a piece placement
    /// If PositionEnd != it means this is a pieace movement from pos start to pos end
    /// </summary>
    public class PlayerMove
    {
        public PlayerMove()
        {
            this.Color = Color._Empty;
            this.PositionStart = 0;
            this.PositionEnd = 0;
        }
        public PlayerMove(Color color, int positionStart, int positionEnd)
        {
            this.Color = color;
            this.PositionStart = positionStart;
            this.PositionEnd = positionEnd;
        }
        public Color Color = Color._Empty;
        public int PositionStart = 0;
        public int PositionEnd = 0;
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
        private Color rotaCenter = Color._Empty;

        public BoardState()
        {
            this.rotaCenter = Color._Empty;
            this.rotaLine = new CircularLinkedList<Position>();
            // initialize the board as empty
            for (int count=1; count <= 8; count++)
            {
                this.rotaLine.AddLast(new Position(count, Color._Empty));
            }
        }

        // B___R____
        public BoardState(string boardStrRepresantation)
        {
            this.rotaLine = new CircularLinkedList<Position>();
            int delta = 0;
            for (int idx = 1; idx <= boardStrRepresantation.Count(); idx++)
            {
                Color currentColor = Color._Empty;
                switch (boardStrRepresantation[idx-1])
                {
                    case 'R': currentColor = Color.Red;
                        break;
                    case 'B': currentColor = Color.Blue;
                        break;
                    case '_': currentColor = Color._Empty;
                        break;
                    default:
                        throw new ApplicationException("Wrong board state representation");
                }
                if (idx == 5)
                {
                    this.rotaCenter = currentColor;
                    delta = 1;
                }
                else
                {
                    this.rotaLine.AddLast(new Position(idx-delta, currentColor));
                }
            }
        }
        /// <summary>
        /// constructs a board from a key
        /// </summary>
        /// <param name="key"></param>
        public BoardState(int key)
        {
            // 18 bits: pairs of two, values 00, 01, 10 for empty, red, blue
            int val = 3;
            switch (key & (val << 16))
            {
                case 0: this.rotaCenter = Color._Empty; break;
                case 1: this.rotaCenter = Color.Red; break;
                case 2: this.rotaCenter = Color.Blue; break;
            }
            for (int idx = 8; idx > 0; idx --)
            {
                val = 3;
                switch (key & (val << ((idx-1) * 2)))
                {
                    case 0: this.rotaLine.AddLast(new Position(9-idx, Color._Empty)); break;
                    case 1: this.rotaLine.AddLast(new Position(9-idx, Color.Red)); break;
                    case 2: this.rotaLine.AddLast(new Position(9-idx, Color.Blue)); break;
                }
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

        public string ToString2()
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

                node = node.Next;
                index++;
            }
            while (node != this.rotaLine.Head);
            return sb.ToString();
        }

        private int GetPiecesCount(List<int> openRed, List<int> openBlue, List<int> openPos)
        {
            openRed = new List<int>();
            openBlue = new List<int>();
            openPos = new List<int>();
            if (this.rotaCenter == Color.Red)
                openRed.Add(0);
            if (this.rotaCenter == Color.Blue)
                openBlue.Add(0);
            if (this.rotaCenter == Color._Empty)
                openPos.Add(0);
            Node<Position> node = this.rotaLine.Head;
            do
            {
                if (node.Value.Col == Color.Red)
                    openRed.Add(node.Value.Pos);
                if (node.Value.Col == Color.Blue)
                    openBlue.Add(node.Value.Pos);
                if (node.Value.Col == Color._Empty)
                    openPos.Add(node.Value.Pos);
            }
            while (node != this.rotaLine.Head);
            return openBlue.Count() + openRed.Count();
        }
        public PlayerMove GenerateRandomValidMove(Color playerColor)
        {
            PlayerMove pm = new PlayerMove();
            pm.Color = playerColor;
            
            List<int> openPos = null, redPos = null, bluePos = null;
            int countOfPieces = GetPiecesCount(redPos, bluePos, openPos);

            if (countOfPieces == 6)
            {
                Random rand = new Random(DateTime.UtcNow.Millisecond);
                int randNumber = 1 + rand.Next(9999) % 3;

                if (playerColor == Color.Red)
                {
                    pm.PositionStart = redPos[randNumber];
                }
                if (playerColor == Color.Blue)
                {
                    pm.PositionStart = bluePos[randNumber];
                }
                // TO DO: find a random valid the pm.PositionEnd from pm.PositionStart

            }
            else
            {

            }
            return pm;
        }

        public static Dictionary<int, List<BoardState>> GenerateAllBoardsForColors(Dictionary<int, List<BoardState>> allBoards, Color firstColor, Color secondColor)
        {
            BoardState board = new BoardState();

            // place the first piece
            board.rotaCenter = firstColor;
            List<BoardState> list = new List<BoardState>();
            BoardState firstBoard = new BoardState(board);
            list.Add(firstBoard);
            allBoards.Add(firstBoard.GetBoardKey(), list);

            Color[] pieces = new Color[5]{ secondColor, firstColor, secondColor, firstColor, secondColor };
            GenerateAllBoardsUniquelyForPieces(allBoards, board, pieces);

            pieces = new Color[6] { firstColor, secondColor, firstColor, secondColor, firstColor, secondColor };
            GenerateAllBoardsUniquelyForPieces(allBoards, new BoardState(), pieces);
            
            return allBoards;
        }

        private static void GenerateAllBoardsUniquelyForPieces(Dictionary<int, List<BoardState>> allBoards, BoardState board, Color[] pieces)
        {
            if (pieces.Count() <= 0)
                return;

            Color currentPiece = pieces[0];
            Color[] remainingPieces = new Color[pieces.Count() - 1];

            for (int idx = 1; idx < pieces.Count(); idx++)
                remainingPieces[idx - 1] = pieces[idx];

            Node<Position> node = board.rotaLine.Head;
            do
            {
                // if empty spot then ocupy it
                if (node.Value.Col == Color._Empty)
                {
                    node.Value.Col = currentPiece;
                    BoardState newBoard = new BoardState(board);
                    int key = newBoard.GetBoardKey();

                    // go through existing keys and see if any is IsEquivalent with the one we want to add
                    bool keyFound = false;
                    foreach (int existingKey in allBoards.Keys)
                    {
                        if (newBoard.IsEquivalent(allBoards[existingKey][0]))
                        {
                            keyFound = true;

                            // add it to the queue and watch the duplicates duplicates
                            bool foundBoard = false;
                            foreach (BoardState bs in allBoards[existingKey])
                            {
                                if (bs.rotaCenter == newBoard.rotaCenter && bs.ToString() == newBoard.ToString())
                                {
                                    foundBoard = true;
                                    break;
                                }
                            }
                            if (!foundBoard)
                                allBoards[existingKey].Add(newBoard);
                            break;
                        }
                    }
                    if (!keyFound)
                    {
                        List<BoardState> list = new List<BoardState>();
                        list.Add(newBoard);
                        allBoards.Add(key, list);
                    }
                    //GenerateAllBoardsUniquelyForPieces(allBoards, board, remainingPieces);
                    GenerateAllBoardsUniquelyForPieces(allBoards, newBoard, remainingPieces);
                    node.Value.Col = Color._Empty;
                }
                node = node.Next;
            }
            while (node != board.rotaLine.Head);
        }

        private static void __GenerateAllBoardsForPieces(Dictionary<int, List<BoardState>> allBoards, BoardState board, Color[] pieces)
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
                    int key = newBoard.GetBoardKey();
                    if (!allBoards.Keys.Contains(key))
                    {
                        List<BoardState> list = new List<BoardState>();
                        list.Add(newBoard);
                        allBoards.Add(key, list);
                    }
                    else
                    {
                        allBoards[key].Add(newBoard);
                    }
                    __GenerateAllBoardsForPieces(allBoards, newBoard, remainingPieces);
                    __GenerateAllBoardsForPieces(allBoards, board, remainingPieces);
                    node.Value.Col = Color._Empty;
                }
                node = node.Next;
            }
            while (node != board.rotaLine.Head);
        }

        public int GetBoardKey()
        {
            return GetUInt32Key();

            //return GetKey();
            //int keyCenter = 31 * this.rotaCenter; // center
            //return keyCenter * GetBoardUniqueKey() * GetBoardUniqueKeyPerColor(Color.Red) * GetBoardUniqueKeyPerColor(Color.Blue);
        }

        private int GetUInt32Key()
        {
            int key = 0;

            if (this.rotaCenter == Color.Red)
                key |= 1;
            if (this.rotaCenter == Color.Blue)
                key |= 2;
            key = key << 16;

            Node<Position> node = this.rotaLine.Head;
            do
            {
                int val = 0;
                if (node.Value.Col == Color.Red)
                    val |= 1;
                if (node.Value.Col == Color.Blue)
                    val |= 2;
                val = val << ((8-node.Value.Pos) * 2);
                key |= val;

                node = node.Next;
            }
            while (node != this.rotaLine.Head);

            return key;
        }

        private int GetKey()
        {
            int minKey = GetKey(true);
            int secondKey = GetKey(false);
            if (minKey > secondKey)
                minKey = secondKey;
            return minKey;
        }
        /// <summary>
        /// the rotaLine is circled 8 times starting from each position 
        /// each position has a unique prime number assigned
        /// each piece color has assigned a unique prime number
        /// after each circle a big number is computed, chose the smaller one as the key 
        /// center 31
        /// </summary>
        /// <returns></returns>
        private int GetKey(bool next)
        {
            int key = 1, keyCenter = 1;
            int position = 0, origBluePieceIndex = 0, origRedPieceIndex = 0;
            int minKey = int.MaxValue;
            Node<Position> node = this.rotaLine.Head;

            if (this.rotaCenter == Color.Red)
            {
                keyCenter *= GetPrimaryForColor(this.rotaCenter, ++origRedPieceIndex) * GetPrimaryForPosition(9);
            }
            if (this.rotaCenter == Color.Blue)
            {
                keyCenter *= GetPrimaryForColor(this.rotaCenter, ++origBluePieceIndex) * GetPrimaryForPosition(9);
            }
            
            do
            {
                Node<Position> startNode = node;
                int bluePieceIndex = origBluePieceIndex, redPieceIndex = origRedPieceIndex;
                position = 1;
                key = 1;
                do
                {
                    if (startNode.Value.Col == Color.Red)
                    {
                        key *= GetPrimaryForColor(startNode.Value.Col, ++redPieceIndex) * GetPrimaryForPosition(position);
                    }
                    if (startNode.Value.Col == Color.Blue)
                    {
                        key *= GetPrimaryForColor(startNode.Value.Col, ++bluePieceIndex) * GetPrimaryForPosition(position);
                    }
                    startNode = next ? startNode.Next : startNode.Previous;
                    ++position;
                }
                while (node != startNode);

                if (key < minKey)
                    minKey = key;

                node = node.Next;
            }
            while (node != this.rotaLine.Head);

            return minKey * keyCenter;
        }

        /// <summary>
        /// this key a is calculated based on the distance between each piece and the alternation of the colors, 
        /// always starting to rotate around the board line from a piece colored red
        /// center 31
        /// red = 2
        /// blue = 3
        /// empty = 1
        /// </summary>
        /// <returns></returns>
        private int GetBoardUniqueKey()
        {
            int key = 1;
            int distance = 0;
            Node<Position> node = this.rotaLine.Head;
            Color currentColor = Color._Empty;
            bool firstFound = false;
            do
            {
                if (node.Value.Col != Color._Empty)
                {
                    firstFound = true;
                    // chime in the starting piece color, 5 will be used as the stargin piece
                    currentColor = node.Value.Col;
                    key *= GetPrimaryForDistance(distance) * GetPrimaryForColor(currentColor, 1) * 5;
                    break;
                }
                node = node.Next;
            }
            while (node != this.rotaLine.Head);

            if (firstFound)
            {
                Node<Position> startNode = node;
                node = node.Next;
                distance = 1;
                do
                {
                    if (node.Value.Col != Color._Empty)
                    {
                        if (node.Value.Col != currentColor)
                        {
                            currentColor = node.Value.Col;
                        }
                        key *= GetPrimaryForDistance(distance) * GetPrimaryForColor(currentColor, 1);
                        distance = 0;
                    }
                    node = node.Next;
                    distance++;
                }
                while (node != startNode);

                key *= GetPrimaryForDistance(distance) * GetPrimaryForColor(currentColor, 1);
            }

            return key;
        }

        /// <summary>
        /// this key a is calculated based on the distance between each piece of the same color, 
        /// always starting to rotate around the board line from a piece colored red
        /// center 31
        /// red = 2
        /// blue = 3
        /// empty = 1
        /// </summary>
        /// <returns></returns>
        public int GetBoardUniqueKeyPerColor(Color color)
        {
            int key = 1; 
            int distance = 0;
            Node<Position> node = this.rotaLine.Head;
            Color currentColor = Color._Empty;
            bool firstFound = false;
            do
            {
                if (node.Value.Col == color)
                {
                    firstFound = true;
                    // chime in the starting piece color, 5 will be used as the stargin piece
                    currentColor = node.Value.Col;
                    key *= GetPrimaryForDistance(distance, 2) * GetPrimaryForColor(currentColor, 1) * 5;
                    break;
                }
                node = node.Next;
            }
            while (node != this.rotaLine.Head);

            if (firstFound)
            {
                Node<Position> startNode = node;
                node = node.Next;
                distance = 1;
                do
                {
                    if (node.Value.Col == color)
                    {
                        if (node.Value.Col != currentColor)
                        {
                            currentColor = node.Value.Col;
                        }
                        key *= GetPrimaryForDistance(distance, 2) * GetPrimaryForColor(currentColor, 2);
                        distance = 0;
                    }
                    node = node.Next;
                    distance++;
                }
                while (node != startNode);

                key *= GetPrimaryForDistance(distance, 2) * GetPrimaryForColor(currentColor, 2);
            }

            return key;
        }
        private int GetPrimaryForColor(Color color, int pieceIndex)
        {
            switch (color)
            {
                case Color.Blue:
                    switch (pieceIndex)
                    {
                        case 1: return 3;
                        case 2: return 5;
                        case 3: return 7;
                        default: return 1;
                    };
                case Color.Red:
                    switch (pieceIndex)
                    {
                        case 1: return 11;
                        case 2: return 13;
                        case 3: return 17;
                        default: return 1;
                    };
                default: return 1;
            }
        }
        private int GetPrimaryForPosition(int distance)
        {
            switch (distance)
            {
                case 1: return 17;
                case 2: return 19;
                case 3: return 23;
                case 4: return 29;
                case 5: return 31;
                case 6: return 37;
                case 7: return 41;
                case 8: return 43;
                case 9: return 47; // center
                default: return 0; // TO DO: signal thi bug in a nicer way
            }
        }
        private int GetPrimaryForDistance(int distance, int? variant = null)
        {
            if (!variant.HasValue)
                switch (distance)
                {
                    case 0: return 7;
                    case 1:
                    case 7: return 11;
                    case 2:
                    case 6: return 13;
                    case 3:
                    case 5: return 17;
                    case 4: return 19;
                    case 8: return 23;
                    default: return 0; // TO DO: signal thi bug in a nicer way
                }
            else
                switch (distance)
                {
                    case 0: return 37;
                    case 1:
                    case 7: return 41;
                    case 2:
                    case 6: return 43;
                    case 3:
                    case 5: return 47;
                    case 4: return 53;
                    case 8: return 59;
                    default: return 0; // TO DO: signal thi bug in a nicer way
                }
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
                state = ((node.Value.Col != Color._Empty) &&
                         (node.Value.Col == node.Next.Value.Col) && 
                         (node.Next.Value.Col == node.Next.Next.Value.Col)) ? (int)node.Value.Col : 0;
                if (state != 0)
                    return state;

                // 2. look at the current node, oposite node from rotaLine and center
                state = ((node.Value.Col != Color._Empty) &&
                         (node.Value.Col == node.Next.Next.Next.Next.Value.Col) &&
                         node.Value.Col == rotaCenter) ? (int)node.Value.Col : 0;
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
        public bool IsEquivalent(BoardState board)
        {
            return IsEquivalent(board, true) || IsEquivalent(board, false);
        }
        /// <summary>
        /// This function finds out if two boards are identical if one is kept fixed (board) and the other (this) is rotated up to a full cycle, 7 steps that is
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public bool IsEquivalent(BoardState board, bool next)
        {
            bool equal = false;

            if (this.rotaCenter != board.rotaCenter)
            {
                return false;
            }

            // start rotating this board with head position
            Node<Position> thisNode = this.rotaLine.Head;
            do
            {
                Node<Position> thisPivot = thisNode;
                Node<Position> boardPivot = board.rotaLine.Head;
                bool boardsEqual = true;
                do
                {
                    if (boardPivot.Value.Col != thisPivot.Value.Col)
                    {
                        boardsEqual = false;
                        break;
                    }
                    else
                    {
                        thisPivot = next ? thisPivot.Next : thisPivot.Previous;
                        boardPivot = boardPivot.Next;
                    }
                }
                while (boardPivot != board.rotaLine.Head);
                if (boardsEqual && boardPivot != board.rotaLine.Head)
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
