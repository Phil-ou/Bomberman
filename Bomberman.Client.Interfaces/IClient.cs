using System.Collections.Generic;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.Interfaces
{
    public delegate void LoginEventHandler(LoginResults result, int playerId, EntityTypes playerEntity, List<MapDescription> maps, bool isGameStarted);
    public delegate void UserConnectedEventHandler(string player, int playerId);
    public delegate void UserDisconnectedEventHandler(string player, int playerId);
    public delegate void GameStartedEventHandler(Map map);
    public delegate void ChatReceivedEventHandler(string player, string msg);
    public delegate void BonusPickedUpEventHandler(EntityTypes bonus);
    public delegate void EntityAddedEventHandler(EntityTypes entity, int locationX, int locationY);
    public delegate void EntityDeletedEventHandler(EntityTypes entity, int locationX, int locationY);
    public delegate void EntityMovedEventHandler(EntityTypes entity, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY);
    public delegate void EntityTransformedEventHandler(EntityTypes oldEntity, EntityTypes newEntity, int locationX, int locationY);
    public delegate void MultipleEntityModifiedEventHandler();
    public delegate void GameDrawEventHandler();
    public delegate void GameLostEventHandler();
    public delegate void GameWonEventHandler(bool won, string name);
    public delegate void KilledEventHandler(string name);
    public delegate void ConnectionLostEventHandler();

    public interface IClient
    {
        List<MapDescription> MapDescriptions { get; }
        List<IOpponent> Opponents { get; }

        Map GameMap { get; }
        string Name { get; }
        int Id { get; }
        EntityTypes Entity { get; }
        int LocationX { get; }
        int LocationY { get; }

        event LoginEventHandler LoggedOn;
        event UserConnectedEventHandler UserConnected;
        event UserDisconnectedEventHandler UserDisconnected;
        event GameStartedEventHandler GameStarted;
        event BonusPickedUpEventHandler BonusPickedUp;
        event ChatReceivedEventHandler ChatReceived;
        event EntityAddedEventHandler EntityAdded;
        event EntityDeletedEventHandler EntityDeleted;
        event EntityMovedEventHandler EntityMoved;
        event EntityTransformedEventHandler EntityTransformed;
        event MultipleEntityModifiedEventHandler MultipleEntityModified;
        event GameDrawEventHandler GameDraw;
        event GameLostEventHandler GameLost;
        event GameWonEventHandler GameWon;
        event KilledEventHandler Killed;
        event ConnectionLostEventHandler ConnectionLost;
        
        void Stop();

        void Login(IProxy proxy, string name);
        void Logout();
        void Chat(string msg);
        void StartGame(int mapId);
        void Move(Directions direction);
        void PlaceBomb();
    }
}
