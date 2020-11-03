using System.Collections.Generic;

public interface IMatchPattern
{
    List<BoardTile> CheckMatches(BoardTile tile, BoardTile ignoreOther, PieceType type);
}
