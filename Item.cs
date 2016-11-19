using System;
using ES.Ds.Common;
using System.Threading;

namespace SNICE.GameCode
{
    public class Item
    {
        public int NR;
        public int NR_IN_DS;
        public string NAME;
        public string TYPE = "item";
        public int EFFECT_NR;
        public bool isActive;
        public int frame_collected;
        public bool onscreen;
        public double x;
        public double y;
        public int duration;
        public Random rnd = new Random();
        public double RANGE = 0.7;
        public Coordinates currentPosition;
        public int ring;
        public int segment;
        public bool justSpawned = false;
        public bool justCollected = false;
        public double itemX;
        public double itemY;
        public double borderY = 0.5;
        public int borderZ = 0;
        public int borderLvl = 0;

        public Sound collectSound;


        // DS/Rendering ------------
        public short solidModelClassId; // Class ID of emptyClass        
        public short positionAttrIndex; // Attribute Index of the position attribute        
        public short solidModelClassPositionId; // Class Attribut Id of position in emptyClass       
        public short objId; // Object Id of the current player
        public short objIdBorder; // Object ids of the border item borders
        public short objIdWarning; // Object ids of the border item borders

        DsPositionAttribute positionValueCar = new DsPositionAttribute();
        DsPositionAttribute positionValueSph = new DsPositionAttribute();

        public Item(int number)
        {
            NR = number;
            NR_IN_DS = number + 1;
            NAME = "item" + NR_IN_DS;
            EFFECT_NR = number;
            isActive = false;
            onscreen = false;
            x = -5;
            y = -5;

            switch (number)
            {
                case 0: // Turn
                    duration = 200;                   
                    break;
                case 1: // Move up border
                    duration = 90;
                    break;
                case 2: // Siren
                    duration = 200;
                    break;
                case 3: // Eraser
                    duration = 600;
                    break;
                case 4: // Flash
                    duration = 200;
                    break;

            }

            collectSound = new Sound("item" + NR + "_collect");
          
            // Get Class ID
            DsAPI.DsGetClassID("solidModelClass", out solidModelClassId);

            // Get Attribute Index
            DsAPI.DsGetClassAttrIndex(solidModelClassId, "position", out positionAttrIndex);

            // Get Class Attribut ID
            DsAPI.DsGetClassAttrEnumID(solidModelClassId, positionAttrIndex, out solidModelClassPositionId);

            // Get Object
            DsAPI.DsGetObjectID("item" + NR_IN_DS, out objId);
            positionValueSph.PositionMode = 2; // Set the position mode to spherical

            if (number == 1) // border item 
            {
                DsAPI.DsGetObjectID("item" + NR_IN_DS + "_warning", out objIdWarning);
                DsAPI.DsGetObjectID("item" + NR_IN_DS + "_border", out objIdBorder);
            }

        }

        public void spawn()
        {
            
            // Generate a new, free position onscreen for that item
            bool freePosFound = false;
            double itemX;
            double itemY;
            do
            {

                // Generate random position
                itemX = itemRndSeg();
                itemY = itemRndRing();

                // Go through all items and check if there is an item, that has the same x value
                // because we dont want two items to be in the same segment
                bool itemInThisSegment = false;

                for (int z = 0; z < Game.items.Length; z++)
                {
                    if (Game.items[z].x == itemX)
                    {
                        itemInThisSegment = true;
                        break;
                    }
                }
                // if no other item has the same x value, there's no item in the same segment
                if (!itemInThisSegment)
                {
                    // Collision detection
                    getSection(itemX, itemY); // retrieve the section the player is currently in

                    int len = Game.sections[segment, ring].Count; // check how many elements are in this list.
                    if (len > 0) // this means, that if there are elements in this list
                    {
                        for (int z = 0; z < len; z++) // loop through all the elements inside the list
                        {
                            if (!collisionDetection(Game.sections[segment, ring][z].X, Game.sections[segment, ring][z].Y, Game.sections[segment, ring][z].TYPE))
                            {
                                // if theres no item in this segment and no collision, then we found a free position for the item
                                freePosFound = true;
                                break;
                            }
                        }
                    }
                }
            } while (!freePosFound);

            // Set the random position as the item position
            x = itemX;
            y = itemY;
            // store the position
            storePos();
            // set justSpawned to true, so it will be rendered next loop
            justSpawned = true;
            
                          
        }

        // render method for digistar
        public void renderPos()
        {

            // Set Attribut usring Ref
            positionValueSph.Azimuth = x;
            positionValueSph.Elevation = y;
            positionValueSph.Distance = 11;
            DsAPI.DsSetObjectAttr(objId, positionAttrIndex, positionValueSph, true);
            DsAPI.DsSendStringCommand("scene add item"+NR_IN_DS);
            DsAPI.DsSendStringCommand("item"+NR_IN_DS+" intensity 100 duration 1");
            // after being rendered, justSpawned can bet set to false again, so this item wont be rendered every frame
            // also we need to increment itemsOnScreen and set this item onscreen
            justSpawned = false;
            Game.itemsOnscreen++;
            onscreen = true;

        }

        public void moveUpBorder (bool isWarning)
        {
            short ID;
            if (isWarning) ID = objIdWarning;
            else ID = objIdBorder;

            if (borderY == 0.5) borderZ = 131;
            if (borderY == 10.5) borderZ = 262;
            if (borderY == 15.5) borderZ = 400;
            if (borderY == 20.5) borderZ = 545;
            if (borderY == 25.5) borderZ = 700;
            if (borderY == 30.5) borderZ = 865;
            if (borderY == 35.5) borderZ = 1050;
            if (borderY == 40.5) borderZ = 1260;
            if (borderY == 45.5) borderZ = 1500;
            if (borderY == 50.5) borderZ = 1780;
            if (borderY == 55.5) borderZ = 2150;
            if (borderY == 60.5) borderZ = 2600;
            if (borderY == 65.5) borderZ = 3200;
            if (borderY == 70.5) borderZ = 4120;
            if (borderY == 75.5) borderZ = 5560;
            if (borderY == 80.5) borderZ = 9000;

            positionValueCar.X = 0;
            positionValueCar.Y = 0;
            positionValueCar.Z = borderZ;
            DsAPI.DsSetObjectAttr(ID, positionAttrIndex, positionValueCar, true);

            if (isWarning) DsAPI.DsSendStringCommand("script include " + Game.path + "DsScripts/Border_Item_Fade.ds");

            else
            {
                // check if there is an item in this ring
                for (int i = 0; i < Game.items.Length; i++) if (Game.items[i].onscreen) Game.items[i].remove();

                borderY += 5;
                borderLvl += 1;
            }
        }

        public void resetBorder()
        {
            borderY = 0.5;
            borderZ = 0;
            borderLvl = 0;

            positionValueCar.X = 0;
            positionValueCar.Y = 0;
            positionValueCar.Z = borderZ;
            DsAPI.DsSetObjectAttr(objIdWarning, positionAttrIndex, positionValueCar, true);
            DsAPI.DsSetObjectAttr(objIdBorder, positionAttrIndex, positionValueCar, true);

        }

        public void start()
        {
            isActive = true;
            int effectDirection = rnd.Next(0, 1);
            collectSound.play();

            switch (EFFECT_NR)
            {
                case 0: // Turn
                    if (effectDirection == 0) DsAPI.DsSendStringCommand("eye attitude cartesian rate 10 0 0 duration 1");
                    else DsAPI.DsSendStringCommand("eye attitude cartesian rate -10 0 0 duration 1");
                    break;
                case 1: // Border
                    moveUpBorder(true);
                    break;
                case 2: // Siren
                    DsAPI.DsSendStringCommand("sirens_effect intensity 90 duration 2");
                    if (effectDirection == 0) DsAPI.DsSendStringCommand("sirens_effect attitude cartesian rate 200 0 0 duration 2 ");
                    else DsAPI.DsSendStringCommand("sirens_effect attitude cartesian rate -200 0 0 duration 2 ");
                    break;
                case 3: // Eraser
                    // delete trails
                    for (int i = 0; i < Game.numberOfControllers; i++) DsAPI.DsSendStringCommand("trail" + Game.players[i].NR_IN_DS + " delete");
                    // delete items
                    for (int i = 0; i < Game.items.Length; i++)
                    {
                        if (Game.items[i].onscreen) Game.items[i].justCollected = true; 
                    }

                    // add trails
                    Game.Script.PlaySkript(Properties.Resources.InitTrails);
                    Thread.Sleep(200);
                    for (int i = 0; i < Game.numberOfControllers; i++)
                    {
                        Game.colorValue.Red = Game.players[i].RED;
                        Game.colorValue.Green = Game.players[i].GREEN;
                        Game.colorValue.Blue = Game.players[i].BLUE;

                        DsAPI.DsSetObjectAttr(Game.trailObjId[i], Game.trailColAttrIndex, Game.colorValue, true);
                    }
                    // init sections
                    Game.InitSections();
                    break;
                case 4: // Flashbomb
                    break;
            }
        }

        public void stop()
        {
            isActive = false;
            collectSound.stop();
            switch (EFFECT_NR)
            {
                case 0: // turn
                    DsAPI.DsSendStringCommand("eye attitude cartesian rate 0 0 0 duration 1");
                    break;
                case 1: // Border
                    moveUpBorder(false);
                    break;
                case 2: // Siren
                    DsAPI.DsSendStringCommand("sirens_effect intensity 0 duration 1");
                    DsAPI.DsSendStringCommand("sirens_effect attitude cartesian rate 0 0 0 duration 3 ");
                    break;
                case 3: // Eraser
                    break;
                case 4: // Flashbomb
                    break;
            }
        }

        public void timer ()
        {
            // check if an active item needs to be turned off
            if (isActive && Game.frame > frame_collected + duration) stop();
        }

        public void remove ()
        {
            // Set Attribut usring Ref
            positionValueCar.Azimuth = -5;
            positionValueCar.Elevation = -5;
            positionValueCar.Distance = 11;
            DsAPI.DsSetObjectAttr(objId, positionAttrIndex, positionValueCar, true);
            DsAPI.DsSendStringCommand("scene remove item" + NR_IN_DS);
            DsAPI.DsSendStringCommand("item" + NR_IN_DS + " intensity 0 duration 1");
            // after being rendered, justSpawned can bet set to false again, so this item wont be rendered every frame
            // also we need to increment itemsOnScreen and set this item onscreen
            justCollected = false;
            Game.itemsOnscreen--;
            onscreen = false;
        }

        public bool collisionDetection(double xRetr, double yRetr, string typeRetr)
        {
            bool collision = false;

            if (typeRetr == "snake")
            {
                double dx = x - xRetr;
                double dy = y - yRetr;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                double rangeRetr = RANGE;

                if (distance < RANGE * 3 + rangeRetr) collision = true;
                
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
        public void getSection(double x, double y)
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

        public double itemRndSeg ()
        {
            int rndX = rnd.Next(0, 31);
            double rndSeg = Game.itemXValues[rndX];
            return rndSeg;
        }

        public double itemRndRing()
        {
            int rndY = rnd.Next(1, 12);
            double rndRing = Game.itemYValues[rndY];
            return rndRing;
        }
    }


}
