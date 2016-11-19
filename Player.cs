using ES.Ds.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.DirectInput;



namespace SNICE.GameCode
{
    public class Player
    {
        public int NR; // This is the plain number of the player ... starting at 0
        public int NR_IN_DS; // This is the number the player has in digistar ... starting at 1 (always one higher than NR)
        public string NAME; // This is the name of the player related object in digistar
        public string TYPE = "snake"; // this is the type of obejct ... needed for collision detection ... item or snake
        public string dir = "up"; // direction the player is moving ... up, right, down, left
        public bool isAlive = true; // indicaties if a player is alive or dead
        public bool isActive = false; // indicaties if a player is taking part in this game or not
        //public string COLOR; // the color of the player in digistar
        public int RED;
        public int GREEN;
        public int BLUE;
        public int points = 0; // player score
        public double x; // position value
        public double y; // position value
        public Coordinates currentPosition;
        public int ring;
        public int segment;
        public double RANGE = 0.7;
        public double IGNORE_FRAMES =  25;
        public string collType = "none";
        public bool hasReachedTotalPoints = false;

        // Speed ----------------
        public double speed = 0.12; // this is tha snakes speed
        public double verticalSpeed; // euquals the speed atm
        public double horizontalSpeed; // speed needs to be adjusted depending on the y value

        // DS/Rendering ------------
        public short emptyClassId; // Class ID of emptyClass        
        public short positionAttrIndex; // Attribute Index of the position attribute             
        public short objId; // Object Id of the current player

        DsPositionAttribute positionValue = new DsPositionAttribute();
        
        // This method contains things that are set when a player is created ... meaning all CONSTANTS ...
        public Player (int number)
        {
            
            NR = number;
            NR_IN_DS = number + 1;
            NAME = "snake" + NR_IN_DS;

            switch (number)
            {
                case 0:
                    RED = 14;
                    GREEN = 74;
                    BLUE = 100;
                    break;
                case 1:
                    RED = 100;
                    GREEN = 89;
                    BLUE = 0;
                    break;
                case 2:
                    RED = 95;
                    GREEN = 24;
                    BLUE = 25;
                    break;
                case 3:
                    RED = 0;
                    GREEN = 95;
                    BLUE = 2;
                    break;
                case 4:
                    RED = 50;
                    GREEN = 0;
                    BLUE = 100;
                    break;
                case 5:
                    RED = 100;
                    GREEN = 55;
                    BLUE = 7;
                    break;
                case 6:
                    RED = 100;
                    GREEN = 65;
                    BLUE = 75;
                    break;
                case 7:
                    RED = 100;
                    GREEN = 100;
                    BLUE = 79;
                    break;

            }

            // Get Class ID
            DsAPI.DsGetClassID("emptyClass", out emptyClassId);

            // Get Attribute Index
            DsAPI.DsGetClassAttrIndex(emptyClassId, "position", out positionAttrIndex);

            // Get Object
            DsAPI.DsGetObjectID("snake"+NR_IN_DS, out objId);
            positionValue.PositionMode = 2; // Set the position mode to spherical

        }

        // method to set the position of a player
        public void setPos (double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        // method to set the direction of a player
        public void setDir (Joystick controller)
        {
            JoystickState state = controller.GetCurrentState();

            if (state.PointOfViewControllers[0] == 9000 && dir != "left") dir = "right";

            if (state.PointOfViewControllers[0] == 27000 && dir != "right") dir = "left";

            if (state.PointOfViewControllers[0] == 18000 && dir != "up") dir = "down";

            if (state.PointOfViewControllers[0] == 0 && dir != "down") dir = "up";

        }

        public void panic (Joystick controller)
        {
            JoystickState state = controller.GetCurrentState();

            if (state.Buttons[7])
            {
                isAlive = false;
                addPoints();
                Game.playersAlive--;
                Game.deathSound.play();
            }

            if (state.Buttons[6]) Game.GameDone(true);

        }

        // method that moves the player one position forward if the position is free
        public void move ()
        {
            verticalSpeed = speed;
            horizontalSpeed = speed / Math.Cos(y/52.9); // divide by a very high number and then it works ... dont ask why ... math ...
            horizontalSpeed = Math.Round(horizontalSpeed, 2); // Round the speed to 2 values after to comma, more precision is not needed, keep it simple

            if (dir == "right" && x < 179.91) x += horizontalSpeed;
            if (dir == "right" && x >= 179.91) x = -180;

            if (dir == "left" && x > -179.91) x -= horizontalSpeed;
            if (dir == "left" && x <= -179.91) x = 180;

            if (dir == "up") y += verticalSpeed;
            if (dir == "down") y -= verticalSpeed;

            getSection(x, y); // retrieve the section the player is currently in

            int len = Game.sections[segment, ring].Count; // check how many elements are in this list.
            if (len > 0) // this means, that if there are elements in this list
            {
                for (int i = 0; i < len; i++) // loop through all the elements inside the list
                {
                    if (collisionDetection(Game.sections[segment, ring][i].X, Game.sections[segment, ring][i].Y, Game.sections[segment, ring][i].TYPE, Game.sections[segment, ring][i].FRAME, Game.sections[segment, ring][i].NR, i)) break;
                }
            }

            
            storePos();

        }

        // render method for digistar
        public void renderPos ()
        {

            // Set Attribut usring Ref
            positionValue.Azimuth = x;
            positionValue.Elevation = y;
            positionValue.Distance = 11;
            DsAPI.DsSetObjectAttr(objId, positionAttrIndex, positionValue, true);


        }

        // method that checks if a position is free 
        public bool collisionDetection (double xRetr, double yRetr, string typeRetr, int frameRetr, int nrRetr, int loopCount)
        {
            double dx = x - xRetr;
            double dy = y - yRetr;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            bool collision = false;

            double rangeRetr;
            if (typeRetr == "item") rangeRetr = RANGE * 2.7;
            else rangeRetr = RANGE;

            // borders for out oy playground
            if (y <= 1 || y > 79.99)
            {
                isAlive = false;
                Game.playersAlive--;
                collType = "outOfPG";
                collision = true;
                addPoints();
                Game.deathSound.play();
            }
            else
            {
                if (distance < RANGE + rangeRetr) // Collision with an item or player
                {
                    if (typeRetr == TYPE && nrRetr == NR) // Player Collision with himself
                    {
                        if (frameRetr > Game.frame - IGNORE_FRAMES) // Collision happens withhin puffer time
                        {

                        }
                        else // real collision with himself
                        {
                            isAlive = false;
                            Game.playersAlive--;
                            collType = "self"+Game.frame;
                            collision = true;
                            addPoints();
                            Game.deathSound.play();

                        }
                    }
                    else // Collision with an item or another player
                    {
                        if (typeRetr == "item") // Collision with an item
                        {
                            // Remove item
                            Game.items[nrRetr].justCollected = true;
                            Game.items[nrRetr].frame_collected = Game.frame;                        
                            Game.sections[segment, ring].Remove(Game.sections[segment,ring][loopCount]);                        
                            collType = "item";
                            collision = true;
                        }
                        else // Collision with another player
                        {
                            isAlive = false;
                            Game.playersAlive--;
                            collType = "other";
                            collision = true;
                            addPoints();
                            Game.deathSound.play();

                        }
                    }
                }
            }

     
            return collision;

        }

        // This method stores the current position in the arrays
        public void storePos()
        {
            currentPosition = new Coordinates(x, y, TYPE, NR);
            // Store the position of the player in the according section
            Game.sections[currentPosition.SEGMENT, currentPosition.RING].Add(currentPosition);
            Game.sections[currentPosition.SEGMENT_LEFT, currentPosition.RING].Add(currentPosition);
            Game.sections[currentPosition.SEGMENT_RIGHT, currentPosition.RING].Add(currentPosition);
            if (currentPosition.RING_UP < 16) Game.sections[currentPosition.SEGMENT, currentPosition.RING_UP].Add(currentPosition);
            if (currentPosition.RING_DOWN > -1) Game.sections[currentPosition.SEGMENT, currentPosition.RING_DOWN].Add(currentPosition);
        }

        // This method returns the section the player is currently in
        public void getSection (double x, double y)
        {
            // Get the ring the obj is in (elevation)		
            if (0 <= y && y < 5) ring = 0; // from 0 to 4.99
            if (5 <= y && y < 10) ring = 1; // from 5 to 9.99
            if (10 <= y && y < 15) ring = 2;
            if (15 <= y && y < 20) ring = 3;
            if (20 <= y && y < 25) ring = 4;
            if (25 <= y && y < 30) ring = 5;
            if (30 <= y && y < 35) ring = 6;
            if (35 <= y && y < 40) ring = 7;
            if (40 <= y && y < 45) ring = 8;
            if (45 <= y && y < 50) ring = 9;
            if (50 <= y && y < 55) ring = 10;
            if (55 <= y && y < 60) ring = 11;
            if (60 <= y && y < 65) ring = 12;
            if (65 <= y && y < 70) ring = 13;
            if (70 <= y && y < 75) ring = 14;
            if (75 <= y && y < 80) ring = 15; // from 75 to 79.99


            // Get the segment the obj is in (azimuth)
            if (180 >= x && x > 168.75) segment = 0;
            if (168.75 >= x && x > 157.5) segment = 1;
            if (157.5 >= x && x > 146.25) segment = 2;
            if (146.25 >= x && x > 135) segment = 3;
            if (135 >= x && x > 123.75) segment = 4;
            if (123.75 >= x && x > 112.5) segment = 5;
            if (112.5 >= x && x > 101.25) segment = 6;
            if (101.25 >= x && x > 90) segment = 7;
            if (90 >= x && x > 78.75) segment = 8;
            if (78.75 >= x && x > 67.5) segment = 9;
            if (67.5 >= x && x > 56.25) segment = 10;
            if (56.25 >= x && x > 45) segment = 11;
            if (45 >= x && x > 33.75) segment = 12;
            if (33.75 >= x && x > 22.5) segment = 13;
            if (22.5 >= x && x > 11.25) segment = 14;
            if (11.25 >= x && x > 0) segment = 15;
            if (0 >= x && x > -11.25) segment = 16;
            if (-11.25 >= x && x > -22.5) segment = 17;
            if (-22.5 >= x && x > -33.75) segment = 18;
            if (-33.75 >= x && x > -45) segment = 19;
            if (-45 >= x && x > -56.25) segment = 20;
            if (-56.25 >= x && x > -67.5) segment = 21;
            if (-67.5 >= x && x > -78.75) segment = 22;
            if (-78.75 >= x && x > -90) segment = 23;
            if (-90 >= x && x > -101.25) segment = 24;
            if (-101.25 >= x && x > -112.5) segment = 25;
            if (-112.5 >= x && x > -123.75) segment = 26;
            if (-123.75 >= x && x > -135) segment = 27;
            if (-135 >= x && x > -146.25) segment = 28;
            if (-146.25 >= x && x > -157.5) segment = 29;
            if (-157.5 >= x && x > -168.75) segment = 30;
            if (-168.75 >= x && x > -180) segment = 31;
        }

        public void addPoints()
        {
            for (int i = 0; i < Game.numberOfControllers; i++)
            {
                if (Game.players[i].isAlive)
                {
                    Game.players[i].points++;
                    if (Game.players[i].points >= Game.pointsTotal)
                    {                       
                        Game.winners[Game.numberOfWinners] = Game.players[i].NR;
                        Game.numberOfWinners++;
                    }
                }
            }
        }

        public void activate ()
        {
            DsAPI.DsSendStringCommand("startlabel" + NR_IN_DS + " modelTexture 0 " + Game.path + "img/label/label_check.dds");
            DsAPI.DsSendStringCommand("startlabel" + NR_IN_DS + " intensity 100");
            isActive = true;
            Game.numberOfActivePlayers++;
        }
    }
}
