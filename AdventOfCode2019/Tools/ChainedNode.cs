using System.Diagnostics;
using System.Text;

namespace AdventOfCode2019.Tools;

public sealed class ChainedNode
{
    public int Value { get; init; }
    public ChainedNode Next { get; set; }
    public ChainedNode Previous { get; set; }

    public ChainedNode(int value)
    {
        Value = value;
        Next = this;
        Previous = this;
    }

    public void InsertBefore(ChainedNode newNode)
    {
        newNode.Previous = Previous;
        Previous.Next = newNode;
        Previous = newNode;
        newNode.Next = this;
    }

    public void InsertAfter(ChainedNode newNode)
    {
        newNode.Next = Next;
        Next.Previous = newNode;
        Next = newNode;
        newNode.Previous = this;
    }

    public void RemovePreviousNode()
    {
        ChainedNode marble = Previous;
        Previous = marble.Previous;
        marble.Previous.Next = this;
    }

    public ChainedNode MoveForward(int nbrOfStep)
    {
        return nbrOfStep == 0 ? this : Next.MoveForward(nbrOfStep - 1);
    }

    public ChainedNode MoveBackward(int nbrOfStep)
    {
        return nbrOfStep == 0 ? this : Previous.MoveBackward(nbrOfStep - 1);
    }

    public void DebugPrint(ChainedNode? highlight1, ChainedNode? highlight2 = null, ChainedNode? highlight3 = null)
    {
        Dictionary<ChainedNode, string> highlight = [];
        if (highlight1 != null)
        {
            highlight.Add(highlight1, "({0})");
        }
        if (highlight2 != null)
        {
            highlight.Add(highlight2, "[{0}]");
        }
        if (highlight3 != null)
        {
            highlight.Add(highlight3, "{{0}}");
        }

        // Debugging method to print the current state of the game
        ChainedNode tmp = this;
        StringBuilder stringBuilder = new();
        do
        {
            string formatedResult = highlight.TryGetValue(tmp, out string? value) ? string.Format(value, tmp.Value) : tmp.Value.ToString();
            stringBuilder.Append(formatedResult);
            stringBuilder.Append("  ");
            tmp = tmp.Next;
        } while (tmp != this);
        Debug.WriteLine(stringBuilder.ToString().Trim());
    }
}