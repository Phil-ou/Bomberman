namespace Bomberman.Server
{
    internal struct ServerOptions
    {
        internal const int HeartbeatDelay = 300; // in ms
        internal const int TimeoutDelay = 500; // in ms
        internal const int MaxTimeoutCountBeforeDisconnection = 3;
        internal const bool IsTimeoutDetectionActive = false;
        internal const BombKickBehaviours BombKickBehaviour = BombKickBehaviours.OnDestinationCellOnly;

        internal const int MinBomb = 1; // Min simultaneous bomb for a player
        internal const int MinExplosionRange = 1; // Min bomb explosion range
        internal const int MaxBomb = 8; // Max simultaneous bomb for a player
        internal const int MaxExplosionRange = 10; // Max bomb explosion range
        internal const int BombTimer = 3000; // in ms
        internal const int FlameTimer = 2000; // in ms
        internal const int BombMoveTimer = 500; // in ms
        internal const int BonusTimer = 5000; // in ms
    }
}
