using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataContract;

namespace ClientWPF.Logic
{
    public class ClientProcessor
    {
        public Player Player { get; set; }

        public Map Map { get; set; }

        public void OnUserConnected(Player player, List<String> loginsList, bool canStartGame)
        {
            Player = player; // SinaC: !!!!!!!!! Big bad bug in multiplayer OnUserConnected should only be sent to connecting player, another callback should be used to signal a new player to already connected one (OnNewUserConnected for example)

            InitializeConsole();
            //Console.WriteLine("--------------------------------------");
            //Console.WriteLine("-------- Welcome to Bomberman --------");
            //Console.WriteLine("--------------------------------------\n\n");
            //Console.WriteLine("New User Joined the server : " + player.Username + "\n");
            //Console.WriteLine("List of players online :\n\n");
            foreach (string login in loginsList)
            {
                Console.WriteLine(login + "\n\n");
            }
            if (Player.IsCreator)
            {
                //todo don't allow user to click on s if canstartgame is false
                Console.WriteLine(canStartGame ? "Press S to start the game" : "Wait for other players.");
            }
            else 
                Console.WriteLine("Wait until the creator start the game.");
        }

        public void OnGameStarted(Game newGame)
        {
            InitializeConsole();
            DisplayMap(newGame);
        }

        private static void InitializeConsole()
        {
            Console.SetWindowSize(80, 30);
            Console.BufferWidth = 80;
            Console.BufferHeight = 30;
            Console.CursorVisible = false;
            Console.Clear();
        }

        public void OnMove(LivingObject objectToMoveBefore, LivingObject objectToMoveAfter)
        {
            if (Map == null) 
                return;
            //check if object to move does exists
            if (!Map.GridPositions.Any(livingObject => livingObject.ComparePosition(objectToMoveBefore))) 
                return;
            //if before is player and is "me" then update global player
            if (objectToMoveBefore is Player && Player.CompareId(objectToMoveBefore))
                Player = objectToMoveAfter as Player;
            //handle before
            Console.SetCursorPosition(objectToMoveBefore.ObjectPosition.PositionX, 10 + objectToMoveBefore.ObjectPosition.PositionY); // 10 should be replaced with map parameters
            Console.Write(' ');
            Map.GridPositions.Remove(objectToMoveBefore);
            //handle after
            char toDisplay = ObjectToChar(objectToMoveAfter);
            Console.SetCursorPosition(objectToMoveAfter.ObjectPosition.PositionX, 10 + objectToMoveAfter.ObjectPosition.PositionY); // 10 should be replaced with map parameters
            Console.Write(toDisplay);
            Map.GridPositions.Add(objectToMoveAfter);
        }

        private void DisplayMap(Game currentGame)
        {
            Map = currentGame.Map; // SinaC: Map should be saved when starting game, not while displaying map
            foreach (LivingObject item in currentGame.Map.GridPositions)
            {
                Console.SetCursorPosition(item.ObjectPosition.PositionX, 10 + item.ObjectPosition.PositionY); // 10 should be replaced with map parameters
                char toDisplay = ObjectToChar(item);
                
                Console.Write(toDisplay);
            }
        }

        private char ObjectToChar(LivingObject item)
        {
            if (item is Wall)
            {
                var wall = item as Wall;
                return wall.WallType == WallType.Undestructible ? '█' : '.';
            }
            if (item is Player)
            {
                var player = item as Player;
                return Player.CompareId(player) ? 'X' : '*';
            }
            return ' ';
        }
    }
}
