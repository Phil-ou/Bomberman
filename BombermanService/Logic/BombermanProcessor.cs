using Common.DataContract;

namespace BombermanService.Logic
{
    public class BombermanProcessor
    {
        public Game CreateNewGame(Player creatorPlayer, string gameName, int mapNumber)
        {
            Game newGameToCreate = new Game(gameName);
            //generate map 
            GenerateMap(newGameToCreate);
            //return the new game created
            return newGameToCreate;
        }

        private void GenerateMap(Game newGameToCreate)
        {

        }
    }
}