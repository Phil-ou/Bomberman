using System.Collections.Generic;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.Interfaces
{
    public delegate void LoginDelegate(LoginResults result, int playerId, EntityTypes playerEntity, List<MapDescription> maps);
    public delegate void UserConnectedDelegate(string player, int playerId);
    public delegate void UserDisconnectedDelegate(string player, int playerId);
    public delegate void GameStartedDelegate(Map map);
    public delegate void ChatReceivedDelegate(string player, string msg);
    public delegate void BonusPickedUpDelegate(EntityTypes bonus);
    public delegate void EntityAddedDelegate(EntityTypes entity, int locationX, int locationY);
    public delegate void EntityDeletedDelegate(EntityTypes entity, int locationX, int locationY);
    public delegate void EntityMovedDelegate(EntityTypes entity, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY);
    public delegate void EntityTransformedDelegate(EntityTypes oldEntity, EntityTypes newEntity, int locationX, int locationY);
    public delegate void MultipleEntityModifiedDelegate();
    public delegate void GameDrawDelegate();
    public delegate void GameLostDelegate();
    public delegate void GameWonDelegate(bool won, string name);
    public delegate void KilledDelegate(string name);
    public delegate void ConnectionLostDelegate();

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

        event LoginDelegate LoginHandler;
        event UserConnectedDelegate UserConnectedHandler;
        event UserDisconnectedDelegate UserDisconnectedHandler;
        event GameStartedDelegate GameStartedHandler;
        event BonusPickedUpDelegate BonusPickedUpHandler;
        event ChatReceivedDelegate ChatReceivedHandler;
        event EntityAddedDelegate EntityAddedHandler;
        event EntityDeletedDelegate EntityDeletedHandler;
        event EntityMovedDelegate EntityMovedHandler;
        event EntityTransformedDelegate EntityTransformedHandler;
        event MultipleEntityModifiedDelegate MultipleEntityModifiedHandler;
        event GameDrawDelegate GameDrawHandler;
        event GameLostDelegate GameLostHandler;
        event GameWonDelegate GameWonHandler;
        event KilledDelegate KilledHandler;
        event ConnectionLostDelegate ConnectionLostHandler;
        
        void Stop();

        void Login(IProxy proxy, string name);
        void Logout();
        void Chat(string msg);
        void StartGame(int mapId);
        void Move(Directions direction);
        void PlaceBomb();
    }
}
