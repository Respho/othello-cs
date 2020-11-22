using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player {
  static void Main(string[] args) {
    int id = int.Parse(Console.ReadLine()); // id of your player.
    int boardSize = int.Parse(Console.ReadLine());

    Console.Error.WriteLine("ID " + id + " size " + boardSize);

    // game loop
    while (true) {
      // rows from top to bottom (viewer perspective)
      var lines = new List < string > ();
      for (int i = 0; i < boardSize; i++) {
        string line = Console.ReadLine();
        Console.Error.WriteLine(line);
        lines.Add(line);
      }
      // number of legal actions for this turn
      var actions = new List < string > ();
      int actionCount = int.Parse(Console.ReadLine());
      for (int i = 0; i < actionCount; i++) {
        // the action
        string action = Console.ReadLine();
        Console.Error.WriteLine(action);
        actions.Add(action);
      }
      Console.Error.WriteLine("-----");

      // a-h1-8
      //string bestAction = actions.OrderByDescending(a => evaluateMove(a)).First();
      //Console.WriteLine(bestAction);
      //
      Othello o = new Othello(lines.ToArray());
      string move = o.PlayNextMove(id);

      //
      Console.WriteLine(move);
    }
  }

  static int evaluateMove(string move) {
    int score = 0;
    foreach(char c in "ah18") {
      if (move.Contains(c)) score += 3;
    }
    foreach(char c in "27bg") {
      if (move.Contains(c)) score--;
    }
    return score;
  }

  public class Board {
    // These constants represent the contents of a board square.
    public static readonly int Black = -1;
    public static readonly int Empty = 0;
    public static readonly int White = 1;

    // These counts reflect the current board situation.
    public int BlackCount {
      get {
        return this.blackCount;
      }
    }
    public int WhiteCount {
      get {
        return this.whiteCount;
      }
    }
    public int EmptyCount {
      get {
        return this.emptyCount;
      }
    }
    public int BlackFrontierCount {
      get {
        return this.blackFrontierCount;
      }
    }
    public int WhiteFrontierCount {
      get {
        return this.whiteFrontierCount;
      }
    }
    public int BlackSafeCount {
      get {
        return this.blackSafeCount;
      }
    }
    public int WhiteSafeCount {
      get {
        return this.whiteSafeCount;
      }
    }

    // Internal counts.
    private int blackCount;
    private int whiteCount;
    private int emptyCount;
    private int blackFrontierCount;
    private int whiteFrontierCount;
    private int blackSafeCount;
    private int whiteSafeCount;

    // This two-dimensional array represents the squares on the board.
    private int[, ] squares;

    // This two-dimensional array tracks which discs are safe (i.e.,
    // discs that cannot be outflanked in any direction).
    private bool[, ] safeDiscs;

    //
    // Creates a new, empty Board object.
    //
    public Board() {
      // Create the squares and safe disc map.
      this.squares = new int[8, 8];
      this.safeDiscs = new bool[8, 8];

      // Clear the board and map.
      int i,
      j;
      for (i = 0; i < 8; i++)
      for (j = 0; j < 8; j++) {
        this.squares[i, j] = Board.Empty;
        this.safeDiscs[i, j] = false;
      }

      // Update the counts.
      this.UpdateCounts();
    }

    //
    // Creates a new Board object by copying an existing one.
    //
    public Board(Board board) {
      // Create the squares and map.
      this.squares = new int[8, 8];
      this.safeDiscs = new bool[8, 8];

      // Copy the given board.
      int i,
      j;
      for (i = 0; i < 8; i++)
      for (j = 0; j < 8; j++) {
        this.squares[i, j] = board.squares[i, j];
        this.safeDiscs[i, j] = board.safeDiscs[i, j];
      }

      // Copy the counts.
      this.blackCount = board.blackCount;
      this.whiteCount = board.whiteCount;
      this.emptyCount = board.emptyCount;
      this.blackSafeCount = board.blackSafeCount;
      this.whiteSafeCount = board.whiteSafeCount;
    }

    //
    // Sets a board with the initial game set-up.
    //
    public void SetForNewGame() {
      // Clear the board.
      int i,
      j;
      for (i = 0; i < 8; i++)
      for (j = 0; j < 8; j++) {
        this.squares[i, j] = Board.Empty;
        this.safeDiscs[i, j] = false;
      }

      // Set two black and two white discs in the center.
      this.squares[3, 3] = White;
      this.squares[3, 4] = Black;
      this.squares[4, 3] = Black;
      this.squares[4, 4] = White;

      // Update the counts.
      this.UpdateCounts();
    }

    public int SetForState(string[] state) {
      //White 0, Black 1, Empty .
      int i,
      j,
      count = 0;
      for (i = 0; i < 8; i++)
      for (j = 0; j < 8; j++) {
        char c = state[i][j];
        if (c == '0' || c == '1') count++;
        this.squares[i, j] = (c == '1') ? Board.Black: (c == '0') ? Board.White: Board.Empty;
        this.safeDiscs[i, j] = false;
      }

      // Update the counts.
      this.UpdateCounts();

      //
      int moveNumber = count - 4 + 1;
      return moveNumber;
    }

    //
    // Returns the contents of a given board square.
    //
    public int GetSquareContents(int row, int col) {
      return this.squares[row, col];
    }

    //
    // Places a disc for the player on the board and flips any outflanked opponents.
    // Note: For performance reasons, it does NOT check that the move is valid.
    //
    public void MakeMove(int color, int row, int col) {
      // Set the disc on the square.
      this.squares[row, col] = color;

      // Flip any flanked opponents.
      int dr,
      dc;
      int r,
      c;
      for (dr = -1; dr <= 1; dr++)
      for (dc = -1; dc <= 1; dc++)
      // Are there any outflanked opponents?
      if (! (dr == 0 && dc == 0) && IsOutflanking(color, row, col, dr, dc)) {
        r = row + dr;
        c = col + dc;
        // Flip 'em.
        while (this.squares[r, c] == -color) {
          this.squares[r, c] = color;
          r += dr;
          c += dc;
        }
      }

      // Update the counts.
      this.UpdateCounts();
    }

    //
    // Determines if the player can make any valid move on the board.
    //
    public bool HasAnyValidMove(int color) {
      // Check all board positions for a valid move.
      int r,
      c;
      for (r = 0; r < 8; r++)
      for (c = 0; c < 8; c++)
      if (this.IsValidMove(color, r, c)) return true;

      // None found.
      return false;
    }

    //
    // Determines if a specific move is valid for the player.
    //
    public bool IsValidMove(int color, int row, int col) {
      // The square must be empty.
      if (this.squares[row, col] != Board.Empty) return false;

      // Must be able to flip at least one opponent disc.
      int dr,
      dc;
      for (dr = -1; dr <= 1; dr++)
      for (dc = -1; dc <= 1; dc++)
      if (! (dr == 0 && dc == 0) && this.IsOutflanking(color, row, col, dr, dc)) return true;

      // No opponents could be flipped.
      return false;
    }

    //
    // Returns the number of valid moves a player can make on the board.
    //
    public int GetValidMoveCount(int color) {
      int n = 0;

      // Check all board positions.
      int i,
      j;
      for (i = 0; i < 8; i++)
      for (j = 0; j < 8; j++)
      // If the move is valid for the color, bump the count.
      if (this.IsValidMove(color, i, j)) n++;
      return n;
    }

    //
    // Given a player move and a specific direction, determines if any
    // opponent discs will be outflanked.
    // Note: For performance reasons the direction values are NOT checked
    // for validity (dr and dc may be one of -1, 0 or 1 but both should
    // not be zero).
    //
    private bool IsOutflanking(int color, int row, int col, int dr, int dc) {
      // Move in the given direction as long as we stay on the board and
      // land on a disc of the opposite color.
      int r = row + dr;
      int c = col + dc;
      while (r >= 0 && r < 8 && c >= 0 && c < 8 && this.squares[r, c] == -color) {
        r += dr;
        c += dc;
      }

      // If we ran off the board, only moved one space or didn't land on
      // a disc of the same color, return false.
      if (r < 0 || r > 7 || c < 0 || c > 7 || (r - dr == row && c - dc == col) || this.squares[r, c] != color) return false;

      // Otherwise, return true;
      return true;
    }

    //
    // Updates the board counts and safe disc map.
    // Note: MUST be called after any changes to the board contents.
    //
    private void UpdateCounts() {
      // Reset all counts.
      this.blackCount = 0;
      this.whiteCount = 0;
      this.emptyCount = 0;
      this.blackFrontierCount = 0;
      this.whiteFrontierCount = 0;
      this.whiteSafeCount = 0;
      this.blackSafeCount = 0;

      int i,
      j;

      // Update the safe disc map.
      //
      // All currently unsafe discs are checked to see if they are still
      // outflankable. Those that are not are marked as safe.
      // If any new safe discs were found, the process is repeated
      // because this change may have made other discs safe as well. The
      // loop exits when no new safe discs are found.
      bool statusChanged = true;
      while (statusChanged) {
        statusChanged = false;
        for (i = 0; i < 8; i++)
        for (j = 0; j < 8; j++)
        if (this.squares[i, j] != Board.Empty && !this.safeDiscs[i, j] && !this.IsOutflankable(i, j)) {
          this.safeDiscs[i, j] = true;
          statusChanged = true;
        }
      }

      // Tally the counts.
      int dr,
      dc;
      for (i = 0; i < 8; i++)
      for (j = 0; j < 8; j++) {
        // If there is a disc at this position, determine if it is
        // on the frontier (i.e., adjacent to an empty square).
        bool isFrontier = false;
        if (this.squares[i, j] != Board.Empty) {
          for (dr = -1; dr <= 1; dr++)
          for (dc = -1; dc <= 1; dc++)
          if (! (dr == 0 && dc == 0) && i + dr >= 0 && i + dr < 8 && j + dc >= 0 && j + dc < 8 && this.squares[i + dr, j + dc] == Board.Empty) isFrontier = true;
        }

        // Update the counts.
        if (this.squares[i, j] == Board.Black) {
          this.blackCount++;
          if (isFrontier) this.blackFrontierCount++;
          if (this.safeDiscs[i, j]) this.blackSafeCount++;
        }
        else if (this.squares[i, j] == Board.White) {
          this.whiteCount++;
          if (isFrontier) this.whiteFrontierCount++;
          if (this.safeDiscs[i, j]) this.whiteSafeCount++;
        }
        else this.emptyCount++;
      }
    }

    //
    // Returns true if the disc at the given position can be outflanked in
    // any direction.
    // Note: For performance reasons we do NOT check that the square has a
    // disc.
    //
    private bool IsOutflankable(int row, int col) {
      // Get the disc color.
      int color = this.squares[row, col];

      // Check each line through the disc.
      // NOTE: A disc is outflankable if there is an empty square on
      // both sides OR if there is an empty square on one side and an
      // opponent or unsafe (outflankable) disc of the same color on the
      // other side.
      int i,
      j;
      bool hasSpaceSide1,
      hasSpaceSide2;
      bool hasUnsafeSide1,
      hasUnsafeSide2;

      // Check the horizontal line through the disc.
      hasSpaceSide1 = false;
      hasUnsafeSide1 = false;
      hasSpaceSide2 = false;
      hasUnsafeSide2 = false;
      // West side.
      for (j = 0; j < col && !hasSpaceSide1; j++)
      if (this.squares[row, j] == Board.Empty) hasSpaceSide1 = true;
      else if (this.squares[row, j] != color || !this.safeDiscs[row, j]) hasUnsafeSide1 = true;
      // East side.
      for (j = col + 1; j < 8 && !hasSpaceSide2; j++)
      if (this.squares[row, j] == Board.Empty) hasSpaceSide2 = true;
      else if (this.squares[row, j] != color || !this.safeDiscs[row, j]) hasUnsafeSide2 = true;
      if ((hasSpaceSide1 && hasSpaceSide2) || (hasSpaceSide1 && hasUnsafeSide2) || (hasUnsafeSide1 && hasSpaceSide2)) return true;

      // Check the vertical line through the disc.
      hasSpaceSide1 = false;
      hasSpaceSide2 = false;
      hasUnsafeSide1 = false;
      hasUnsafeSide2 = false;
      // North side.
      for (i = 0; i < row && !hasSpaceSide1; i++)
      if (this.squares[i, col] == Board.Empty) hasSpaceSide1 = true;
      else if (this.squares[i, col] != color || !this.safeDiscs[i, col]) hasUnsafeSide1 = true;
      // South side.
      for (i = row + 1; i < 8 && !hasSpaceSide2; i++)
      if (this.squares[i, col] == Board.Empty) hasSpaceSide2 = true;
      else if (this.squares[i, col] != color || !this.safeDiscs[i, col]) hasUnsafeSide2 = true;
      if ((hasSpaceSide1 && hasSpaceSide2) || (hasSpaceSide1 && hasUnsafeSide2) || (hasUnsafeSide1 && hasSpaceSide2)) return true;

      // Check the Northwest-Southeast diagonal line through the disc.
      hasSpaceSide1 = false;
      hasSpaceSide2 = false;
      hasUnsafeSide1 = false;
      hasUnsafeSide2 = false;
      // Northwest side.
      i = row - 1;
      j = col - 1;
      while (i >= 0 && j >= 0 && !hasSpaceSide1) {
        if (this.squares[i, j] == Board.Empty) hasSpaceSide1 = true;
        else if (this.squares[i, j] != color || !this.safeDiscs[i, j]) hasUnsafeSide1 = true;
        i--;
        j--;
      }
      // Southeast side.
      i = row + 1;
      j = col + 1;
      while (i < 8 && j < 8 && !hasSpaceSide2) {
        if (this.squares[i, j] == Board.Empty) hasSpaceSide2 = true;
        else if (this.squares[i, j] != color || !this.safeDiscs[i, j]) hasUnsafeSide2 = true;
        i++;
        j++;
      }
      if ((hasSpaceSide1 && hasSpaceSide2) || (hasSpaceSide1 && hasUnsafeSide2) || (hasUnsafeSide1 && hasSpaceSide2)) return true;

      // Check the Northeast-Southwest diagonal line through the disc.
      hasSpaceSide1 = false;
      hasSpaceSide2 = false;
      hasUnsafeSide1 = false;
      hasUnsafeSide2 = false;
      // Northeast side.
      i = row - 1;
      j = col + 1;
      while (i >= 0 && j < 8 && !hasSpaceSide1) {
        if (this.squares[i, j] == Board.Empty) hasSpaceSide1 = true;
        else if (this.squares[i, j] != color || !this.safeDiscs[i, j]) hasUnsafeSide1 = true;
        i--;
        j++;
      }
      // Southwest side.
      i = row + 1;
      j = col - 1;
      while (i < 8 && j >= 0 && !hasSpaceSide2) {
        if (this.squares[i, j] == Board.Empty) hasSpaceSide2 = true;
        else if (this.squares[i, j] != color || !this.safeDiscs[i, j]) hasUnsafeSide2 = true;
        i++;
        j--;
      }
      if ((hasSpaceSide1 && hasSpaceSide2) || (hasSpaceSide1 && hasUnsafeSide2) || (hasUnsafeSide1 && hasSpaceSide2)) return true;

      // All lines are safe so the disc cannot be outflanked.
      return false;
    }
  }

  public class Othello {
    // For converting column numbers to letters and vice versa.
    private static String alpha = "abcdefgh";

    // Defines the difficulty settings.
    public enum Difficulty {
      Beginner,
      Intermediate,
      Advanced,
      Expert
    }

    // AI parameters.
    private int lookAheadDepth;
    private int forfeitWeight;
    private int frontierWeight;
    private int mobilityWeight;
    private int stabilityWeight;

    // The game board.
    private Board board;

    //
    public class Options {
      public int FirstMove;
      public bool ComputerPlaysBlack;
      public bool ComputerPlaysWhite;
      public Difficulty Difficulty = Difficulty.Expert;
    }

    Options options = new Options();

    // Game parameters.
    private int currentColor;
    private int moveNumber;

    // Defines a thread for running the computer move look ahead.
    private System.Threading.Thread calculateComputerMoveThread;

    // Defines a structure for holding a look ahead move and rank.
    private struct ComputerMove {
      public int row;
      public int col;
      public int rank;

      public ComputerMove(int row, int col) {
        this.row = row;
        this.col = col;
        this.rank = 0;
      }
    }

    private ComputerMove _resultMove;

    // Defines the maximum move rank value (used for ranking an end game).
    private static int maxRank = System.Int32.MaxValue - 64;

    // Defines a structure for holding move history data.
    private struct MoveRecord {
      public Board board;
      public int currentColor;
      public string move;

      public MoveRecord(Board board, int currentColor, string move) {
        this.board = new Board(board);
        this.currentColor = currentColor;
        this.move = move;
      }
    }

    // Defines an array for storing the move history.
    private ArrayList moveHistory;

    // Used to track which player made the last move.
    private int lastMoveColor;

    public Othello() {
      // Create the game board.
      this.board = new Board();

      //
      this.StartGame(false);
    }

    public Othello(string[] state) {
      // Create the game board.
      this.board = new Board();

      //
      this.moveNumber = board.SetForState(state);

      // Initialize the move history.
      this.moveHistory = new ArrayList(60);

      //
      //this.StartGame(false);
    }

    public string PlayNextMove(int player) {
      // Black 0, White 1
      this.currentColor = player == 1 ? Board.Black: Board.White;
      this.lastMoveColor = this.currentColor * -1;

      //
      CalculateComputerMove();

      string result = "" + alpha[_resultMove.col] + (_resultMove.row + 1);
      return result;
    }

    //
    // Starts a new game or, optionally, restarts an ended game.
    //
    private void StartGame(bool isRestart) {
      // Initialize the move list.
      this.moveNumber = 1;

      // Initialize the move history.
      this.moveHistory = new ArrayList(60);

      // Initialize the last move color.
      this.lastMoveColor = Board.Empty;

      // Initialize the board.
      this.board.SetForNewGame();

      // Set the first player.
      this.currentColor = this.options.FirstMove;

      // Start the first turn.
      this.StartTurn();
    }

    //
    // Sets up for the current player to make a move or ends the game if
    // neither player can make a valid move.
    //
    private void StartTurn() {
      // If the current player cannot make a valid move, forfeit the turn.
      if (!this.board.HasAnyValidMove(this.currentColor)) {
        // Switch back to the other player.
        this.currentColor *= -1;
      }

      // Set the player text for the status display.
      string playerText = String.Format("{0}'s", (this.currentColor == Board.Black ? "Black": "White"));

      // Start a separate thread to perform the computer's move.
      this.calculateComputerMoveThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.CalculateComputerMove));
      this.calculateComputerMoveThread.IsBackground = true;
      this.calculateComputerMoveThread.Priority = System.Threading.ThreadPriority.Lowest;
      this.calculateComputerMoveThread.Name = "Calculate Computer Move";
      this.calculateComputerMoveThread.Start();

      // Make the move.
      //this.MakeMove(row, col);
    }

    //
    // Makes a move for the current player.
    //
    private void MakeMove(int row, int col) {
      // Clean up the move history to ensure that it contains only the
      // moves made prior to this one.
      while (this.moveHistory.Count > this.moveNumber - 1)
      this.moveHistory.RemoveAt(this.moveHistory.Count - 1);

      // Add the move to the move list.
      string color = "Black";
      if (this.currentColor == Board.White) color = "White";
      string move = "[] " + this.moveNumber.ToString() + " " + color + " " + (alpha[col] + (row + 1).ToString());

      // Add this move to the move history.
      this.moveHistory.Add(new MoveRecord(this.board, this.currentColor, move));

      // Bump the move number.
      this.moveNumber++;

      // Make a copy of the board (for doing move animation).
      Board oldBoard = new Board(this.board);

      // Make the move on the board.
      this.board.MakeMove(this.currentColor, row, col);

      // Save the player color.
      this.lastMoveColor = this.currentColor;
    }

    //
    // Define delegates for callbacks from the worker thread.
    //
    public delegate void UpdateStatusProgressDelegate();
    public delegate void MakeComputerMoveDelegate(int row, int col);

    //
    // Makes a computer-controlled move for the current color.
    // Note: Called from the worker thread.
    //
    private void MakeComputerMove(int row, int col) {
      // Lock the board to prevent a race condition while performing the
      // move.
      lock(this.board) {
        // Make the move.
        this.MakeMove(row, col);
      }
    }

    //
    // Calculates a computer move.
    // Note: Executed in the worker thread.
    //
    private void CalculateComputerMove() {
      // Load the AI parameters.
      this.SetAIParameters();

      // Find the best available move.
      ComputerMove move = this.GetBestMove(this.board);

      // Perform a callback to make the move.
      _resultMove = move;
    }

    // ===================================================================
    // Game AI code.
    // Note: These are executed in the worker thread.
    // ===================================================================
    //
    // This function starts the look ahead process to find the best move
    // for the current player color.
    //
    private ComputerMove GetBestMove(Board board) {
      // Initialize the alpha-beta cutoff values.
      int alpha = maxRank + 64;
      int beta = -alpha;

      // Kick off the look ahead.
      return this.GetBestMove(board, this.currentColor, 1, alpha, beta);
    }

    //
    // This function uses look ahead to evaluate all valid moves for a
    // given player color and returns the best move it can find.
    //
    private ComputerMove GetBestMove(Board board, int color, int depth, int alpha, int beta) {
      // Initialize the best move.
      ComputerMove bestMove = new ComputerMove( - 1, -1);
      bestMove.rank = -color * maxRank;

      // Find out how many valid moves we have so we can initialize the
      // mobility score.
      int validMoves = board.GetValidMoveCount(color);

      // Start at a random position on the board. This way, if two or
      // more moves are equally good, we'll take one of them at random.
      Random random = new Random();
      int rowStart = random.Next(8);
      int colStart = random.Next(8);

      // Check all valid moves.
      int i,
      j;
      for (i = 0; i < 8; i++)
      for (j = 0; j < 8; j++) {
        // Get the row and column.
        int row = (rowStart + i) % 8;
        int col = (colStart + j) % 8;

        if (board.IsValidMove(color, row, col)) {
          // Make the move.
          ComputerMove testMove = new ComputerMove(row, col);
          Board testBoard = new Board(board);
          testBoard.MakeMove(color, testMove.row, testMove.col);
          int score = testBoard.WhiteCount - testBoard.BlackCount;

          // Check the board.
          int nextColor = -color;
          int forfeit = 0;
          bool isEndGame = false;
          int opponentValidMoves = testBoard.GetValidMoveCount(nextColor);
          if (opponentValidMoves == 0) {
            // The opponent cannot move, count the forfeit.
            forfeit = color;

            // Switch back to the original color.
            nextColor = -nextColor;

            // If that player cannot make a move either, the
            // game is over.
            if (!testBoard.HasAnyValidMove(nextColor)) isEndGame = true;
          }

          // If we reached the end of the look ahead (end game or
          // max depth), evaluate the board and set the move
          // rank.
          if (isEndGame || depth == this.lookAheadDepth) {
            // For an end game, max the ranking and add on the
            // final score.
            if (isEndGame) {
              // Negative value for black win.
              if (score < 0) testMove.rank = -maxRank + score;

              // Positive value for white win.
              else if (score > 0) testMove.rank = maxRank + score;

              // Zero for a draw.
              else testMove.rank = 0;
            }

            // It's not an end game so calculate the move rank.
            else testMove.rank = this.forfeitWeight * forfeit + this.frontierWeight * (testBoard.BlackFrontierCount - testBoard.WhiteFrontierCount) + this.mobilityWeight * color * (validMoves - opponentValidMoves) + this.stabilityWeight * (testBoard.WhiteSafeCount - testBoard.BlackSafeCount) + score;
          }

          // Otherwise, perform a look ahead.
          else {
            ComputerMove nextMove = this.GetBestMove(testBoard, nextColor, depth + 1, alpha, beta);

            // Pull up the rank.
            testMove.rank = nextMove.rank;

            // Forfeits are cumulative, so if the move did not
            // result in an end game, add any current forfeit
            // value to the rank.
            if (forfeit != 0 && Math.Abs(testMove.rank) < maxRank) testMove.rank += forfeitWeight * forfeit;

            // Adjust the alpha and beta values, if necessary.
            if (color == Board.White && testMove.rank > beta) beta = testMove.rank;
            if (color == Board.Black && testMove.rank < alpha) alpha = testMove.rank;
          }

          // Perform a cutoff if the rank is outside tha alpha-beta range.
          if (color == Board.White && testMove.rank > alpha) {
            testMove.rank = alpha;
            return testMove;
          }
          if (color == Board.Black && testMove.rank < beta) {
            testMove.rank = beta;
            return testMove;
          }

          // If this is the first move tested, assume it is the
          // best for now.
          if (bestMove.row < 0) bestMove = testMove;

          // Otherwise, compare the test move to the current
          // best move and take the one that is better for this
          // color.
          else if (color * testMove.rank > color * bestMove.rank) bestMove = testMove;
        }
      }

      // Return the best move found.
      return bestMove;
    }

    //
    // Sets the AI parameters based on the current difficulty setting.
    //
    private void SetAIParameters() {
      // Set the AI parameter weights.
      switch (this.options.Difficulty) {
      case Difficulty.Beginner:
        this.forfeitWeight = 2;
        this.frontierWeight = 1;
        this.mobilityWeight = 0;
        this.stabilityWeight = 3;
        break;
      case Difficulty.Intermediate:
        this.forfeitWeight = 3;
        this.frontierWeight = 1;
        this.mobilityWeight = 0;
        this.stabilityWeight = 5;
        break;
      case Difficulty.Advanced:
        this.forfeitWeight = 7;
        this.frontierWeight = 2;
        this.mobilityWeight = 1;
        this.stabilityWeight = 10;
        break;
      case Difficulty.Expert:
        this.forfeitWeight = 35;
        this.frontierWeight = 10;
        this.mobilityWeight = 5;
        this.stabilityWeight = 50;
        break;
      default:
        this.forfeitWeight = 0;
        this.frontierWeight = 0;
        this.mobilityWeight = 0;
        this.stabilityWeight = 0;
        break;
      }

      // Set the look-ahead depth.
      //this.lookAheadDepth = (int) this.options.Difficulty + 3;
      this.lookAheadDepth = 4;

      // Near the end of the game, when there are relatively few moves
      // left, set the look-ahead depth to do an exhaustive search.
      //if (this.moveNumber >= 55 - (int) this.options.Difficulty) this.lookAheadDepth = this.board.EmptyCount;
      if (this.moveNumber >= 58 - (int) this.options.Difficulty) this.lookAheadDepth = this.board.EmptyCount;
    }
  }
}