# othello-cs
This is a console app adaptation of an Othello AI in C#, which facilitates automated benchmarking, and submitting to competitions, etc.

# Usage
For each turn, the AI reads from standard input and outputs move to standard output.
Accepts a single digit then a 8x8 board state.

```csharp
	//White = 1, Black = 0, Empty = .
	//ID = 0 means you are black player
	/*
    	0
		........
		........
		........
		...10...
		...01...
		........
		........
		........
	*/
```

Outputs chosen move in standard notation, a1 - h8.

# AI Parameters

Several levels of AI are available, for instance:

```csharp
case Difficulty.Advanced:
    this.forfeitWeight = 7;
    this.frontierWeight = 2;
    this.mobilityWeight = 1;
    this.stabilityWeight = 10;

this.lookAheadDepth = n;
```


# Credits

BrainJar - Reversi in C#
https://www.codeproject.com/Articles/4672/Reversi-in-C
