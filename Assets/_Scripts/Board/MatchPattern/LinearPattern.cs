using System.Collections.Generic;
using UnityEngine;

public class LinearPattern : ScriptableObject, IMatchPattern
{
    public List<BoardTile> CheckMatches(BoardTile tile, BoardTile ignoreOther, PieceType type)
    {
        List<BoardTile> tempMatches = new List<BoardTile>();
        List<BoardTile> tilesMatched = new List<BoardTile>();

        tempMatches.AddRange(GetTileMatchesOnDirection(tile, Direction.Right, type, ignoreOther));
        tempMatches.AddRange(GetTileMatchesOnDirection(tile, Direction.Left, type, ignoreOther));

        if (tempMatches.Count >= GameManager.Instance.MatchThreshold - 1)
        {
            tilesMatched.Add(tile);
            tilesMatched.AddRange(tempMatches);
        }

        tempMatches.Clear();
        tempMatches.AddRange(GetTileMatchesOnDirection(tile, Direction.Up, type, ignoreOther));
        tempMatches.AddRange(GetTileMatchesOnDirection(tile, Direction.Down, type, ignoreOther));

        if (tempMatches.Count >= GameManager.Instance.MatchThreshold - 1)
        {
            if (!tilesMatched.Contains(tile))
                tilesMatched.Add(tile);

            tilesMatched.AddRange(tempMatches);
        }

        return tilesMatched;
    }

    List<BoardTile> GetTileMatchesOnDirection(BoardTile tile, Direction direction, PieceType type, BoardTile ignoreOther = null)
    {
        List<BoardTile> tempMatches = new List<BoardTile>();

        BoardTile nextTile = tile.GetNeighbor(direction);
        while (nextTile != ignoreOther && nextTile != null && nextTile.MatchPiece.pieceType == type)
        {
            tempMatches.Add(nextTile);
            nextTile = nextTile.GetNeighbor(direction);
        }

        return tempMatches;
    }
}
