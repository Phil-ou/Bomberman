namespace Bomberman.Server
{
    internal enum BombKickBehaviours
    {
        // Players with bonus are allowed to kick bomb only on own cell
        OnPlayerCellOnly,
        // Players with bonus are allowed to kick bomb only on move destination cell
        OnDestinationCellOnly,
        // Players with bonus are allowed to kick bomb on any cell
        OnAnyCell
    }
}
