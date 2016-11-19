using ES.Ds.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using SharpDX.DirectInput;
using System.IO;




namespace SNICE.GameCode
{
    public class Game
    {
        private Thread mLoop;

        public bool running = false;
        public static string status;
        // Get the current directory.
        public static string path = Directory.GetCurrentDirectory();
        public static string currentFolder;


        // Initialize DirectInput
        private static DirectInput directInput = new DirectInput();

        // Find a Joystick Guid
        public static Player[] players = new Player[8];
        private static Joystick[] controllers = new Joystick[8];
        //private static JoystickState[] state = new JoystickState[8];
        private static Guid[] controllerGuids = new Guid[8];
        public static List<Coordinates>[,] sections = new List<Coordinates>[32, 18];
        public MenuElement[] MenuElements = new MenuElement[4];
        public static Item[] items = new Item[5];
        public static int frame;
        public static int playersAlive;
        public static int numberOfControllers;
        public static int numberOfActivePlayers;
        public static int pointsTotal;
        public int pointsTotalFactor = 2;
        public int menuTimeOut = 10;
        public static int itemsOnscreen = 0;
        public static int[] winners = new int[8];
        public static int numberOfWinners;

        public static double[] itemXValues = new double[32];
        public static double[] itemYValues = new double[13];
        public double startingElevation = 2.5;
        public double startingAzimuth = -174.38;
        public int playerSelectTimeOut = 50;
        private int selectPlayersCountdown = 30;

        public static short trailClassId;
        public static short trailColAttrIndex;
        
        public static short solidModelClassId;
        public static short smcPosAttrIndex;
        public static short smcScaleAttrIndex;
        public static short smcIntAttrIndex;

        public static short text2ClassId;
        public static short t2cTextAttrIndex;
        public static short t2cPosAttrIndex;
        public static short t2cIntAttrIndex;
        public static short t2cColAttrIndex;

        public static short winnersObjId;
        public static short pointsTotalObjId;

        public static short[] playerObjId = new short[8];
        public static short[] pointsObjId = new short[8];
        public static short[] snakeObjId = new short[8];
        public static short[] trailObjId = new short[8];
        public static short[] startlabelObjId = new short[8];
        public static short[] counterObjId = new short[8];

        public static double offset;

        public static DsScripts Script;

        public static DsPositionAttribute positionValue = new DsPositionAttribute();
        public static DsScaleAttribute scaleValue = new DsScaleAttribute();
        public static DsColorAttribute colorValue = new DsColorAttribute();
        public static DsStringAttribute textValue = new DsStringAttribute();
        public static DsInt32Attribute intensityValue = new DsInt32Attribute();

        public static Sound menuLoop;
        public static Sound gameLoop;
        public static Sound deathSound;
        public static Sound menuSelectSound;
        public static Sound menuMoveSound;
        public static Sound winSound;
        public static Sound countdown;
        

        public void Start()
        {

            Init();



            if (mLoop != null) Stop();
            mLoop = new Thread(GameLoop); // Das Objekt mLoop ist ein Thread, d.h. dat läuft, der Thread hat den Namen GameLoop und führt die Funktion GameLoop durch            
            mLoop.Name = "GameLoop";
            mLoop.Start();
            running = true;
            status = "init";

        }

        public void Stop()
        {
            running = false;
            status = "init";
            if (mLoop != null) mLoop.Join();
            mLoop = null;

        }

        public void GameLoop()
        {

            while (running)
            {

                switch (status)
                {
                    case "init":
                        MenuInit();
                        break;
                    case "menu":
                        MenuCtrl();
                        break;
                    case "playerSelect":
                        PlayerSelect();
                        break;
                    case "running":
                        // 60 mal pro Secunde ausführen
                        if (playersAlive > 1)
                        {
                            ProcessUserInput();
                            Update();
                            Render();
                        }
                        else
                        {
                            RoundDone();
                        }

                        break;
                }
                frame++;
                Thread.Sleep(17); // 16 ms
                // ggf. hier noch genauer timen durch nachschauen der Systemclock
            }
            CleanUp();
        }
        // Initialization ------------------------------
        public void Init()
        {
            
            // Moving to the project folder 
            while (currentFolder != "SNICE")
            {                
                path = Path.GetDirectoryName(path);
                currentFolder = Path.GetFileName(path);
            }
            // Add Backslash once we're in the right path
            path += "\\";
            

            Script = new DsScripts();
            Script.PlaySkript(Properties.Resources.SystemReset);
            DsAPI.DsSendStringCommand("eye int 0");
            Script.PlaySkript(Properties.Resources.InitBG);
            Script.PlaySkript(Properties.Resources.InitCounter);
            Script.PlaySkript(Properties.Resources.InitItemEffect);
            Script.PlaySkript(Properties.Resources.InitPoints);
            Script.PlaySkript(Properties.Resources.InitSelectPlayer);
            Script.PlaySkript(Properties.Resources.InitWinners);
            Script.PlaySkript(Properties.Resources.InitLabel);
            Script.PlaySkript(Properties.Resources.InitSound);
            Script.PlaySkript(Properties.Resources.InitSnakes);
            Script.PlaySkript(Properties.Resources.InitTrails);
            Script.PlaySkript(Properties.Resources.InitItem);
            Script.PlaySkript(Properties.Resources.InitMenu);

            Thread.Sleep(2200);


            // Sound Refs .....

            menuLoop = new Sound("menuLoop");
            gameLoop = new Sound("gameLoop");
            deathSound = new Sound("death");
            menuSelectSound = new Sound("menuSelectSound");
            menuMoveSound = new Sound("menuMoveSound");
            winSound = new Sound("winSound");
            countdown = new Sound("countdown");

            DsAPI.DsSendStringCommand("eye int 100 duration 2");
            menuLoop.loop();
            menuLoop.setVolume(70, 2);
           


            // Initialize Controllers      

            numberOfControllers = 0;
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad,
                        DeviceEnumerationFlags.AllDevices))
            {
                controllerGuids[numberOfControllers] = deviceInstance.InstanceGuid; // Find a guid
                controllers[numberOfControllers] = new Joystick(directInput, controllerGuids[numberOfControllers]); // Instantiate Joystick
                controllers[numberOfControllers].Acquire(); // acquire joystick
                numberOfControllers++;
            }

            InitSections();

            for (int i = 0; i < MenuElements.Length; i++) MenuElements[i] = new MenuElement(i);

            for (int i = 0; i < items.Length; i++) items[i] = new Item(i);

            double startingElevation = 2.5;
            double startingAzimuth = -174.38;

            for (int i = 0; i < itemYValues.Length; i++)
            {
                itemYValues[i] = startingElevation;
                startingElevation += 5;
            }

            for (int i = 0; i < itemXValues.Length; i++)
            {
                itemXValues[i] = startingAzimuth;
                startingAzimuth += 11.25;
            }


            // Set Position mode to sph
            positionValue.PositionMode = 2; 
            // Get Ds Ids and stuff
            // SolidModelClass
            DsAPI.DsGetClassID("solidModelClass", out solidModelClassId);

            // Position
            DsAPI.DsGetClassAttrIndex(solidModelClassId, "position", out smcPosAttrIndex);
            // Scale
            DsAPI.DsGetClassAttrIndex(solidModelClassId, "scale", out smcScaleAttrIndex);
            // Intenstiy
            DsAPI.DsGetClassAttrIndex(solidModelClassId, "intensity", out smcIntAttrIndex);

            Thread.Sleep(1200);

            // Get Object IDs from DS
            DsAPI.DsGetObjectID("winners", out winnersObjId);
            DsAPI.DsGetObjectID("pointsTotal", out pointsTotalObjId);


            for (int i = 1; i <= numberOfControllers; i++)
            {
                DsAPI.DsGetObjectID("player" + i, out playerObjId[i - 1]);
                DsAPI.DsGetObjectID("snake" + i, out snakeObjId[i - 1]);
                DsAPI.DsGetObjectID("points" + i, out pointsObjId[i - 1]);
                DsAPI.DsGetObjectID("startlabel" + i, out startlabelObjId[i - 1]);
                DsAPI.DsGetObjectID("counter" + i, out counterObjId[i - 1]);
                DsAPI.DsGetObjectID("trail" + i, out trailObjId[i - 1]);
            }

            // Text2Class
            DsAPI.DsGetClassID("text2Class", out text2ClassId);
            // Attr
            DsAPI.DsGetClassAttrIndex(text2ClassId, "text", out t2cTextAttrIndex);
            DsAPI.DsGetClassAttrIndex(text2ClassId, "intensity", out t2cIntAttrIndex);
            DsAPI.DsGetClassAttrIndex(text2ClassId, "position", out t2cPosAttrIndex);
            DsAPI.DsGetClassAttrIndex(text2ClassId, "color", out t2cColAttrIndex);

            
            // Trail Class
            DsAPI.DsGetClassID("trailClass", out trailClassId);
            DsAPI.DsGetClassAttrIndex(trailClassId, "color", out trailColAttrIndex);

        }

        public static void InitSections()
        {
            for (int i = 0; i < 32; i++)
            {
                for (int z = 0; z < 18; z++) sections[i, z] = new List<Coordinates>();
            }
        }

        // Menu ----------------------------------------

        public void MenuInit()
        {
            
            Script.PlaySkript(Properties.Resources.MenuFadeIn);
            DsAPI.DsSendStringCommand("eye attitude cartesian -90 0 0");
            DsAPI.DsSendStringCommand("eye attitude cartesian rate -1 0 0 duration 1");
            status = "menu";
            Thread.Sleep(2000);

        }

        public void MenuCtrl()
        {
            if (controllers[0] != null)
            {

                JoystickState state = controllers[0].GetCurrentState();

                // Check if MenuElement is being seleceted for color change
                // Got through all MenuElements and check if they're selected
                for (int i = 0; i < MenuElements.Length; i++)
                {
                    if (MenuElements[i].selected)
                    {
                        DsAPI.DsSendStringCommand("menuitem" + i + " color 100 100 100");

                        if (menuTimeOut == 12)
                        {
                            if (state.Y/100 < 100 && i != 0) // stir up
                            {
                                MenuElements[i].selected = false;
                                MenuElements[i - 1].selected = true;
                                menuMoveSound.play();
                                break;
                            }
                            if (state.Y / 100 > 500 && i < MenuElements.Length-1) // stir down
                            {
                                MenuElements[i].selected = false;
                                MenuElements[i + 1].selected = true;
                                menuMoveSound.play();

                                break;
                            }
                        }

                        // Check if MenuElement is being hit

                        if (state.Buttons[0])
                        {
                            status = "";
                            menuSelectSound.play();
                            MenuDone(MenuElements[i].name);                           
                        }
                        


                    }
                    else // if they're not selecetd, grey 'em out
                    {
                        DsAPI.DsSendStringCommand("menuitem" + i + " color 66 66 66");
                    }
                }

                

                menuTimeOut--;
                if (menuTimeOut == 0) menuTimeOut = 12;
            }


        }

        public void MenuDone(string command)
        {
            
            Script.PlaySkript(Properties.Resources.MenuFadeOut);
            if (command == "play") initPlayerSelect();                                     
            if (command == "instr") Instructions();
            if (command == "credits") Credits();
            if (command == "exit") Stop();
        }

        public void initPlayerSelect ()
        {
            status = "playerSelect";
            selectPlayersCountdown = 30;
            numberOfActivePlayers = 0;
            // Init character array 
            for (int i = 0; i < numberOfControllers; i++) players[i] = new Player(i); // create player 

            
            // Init text objects
            DsAPI.DsSendStringCommand("SelectPlayer_Text intensity 80 dur 1");
            DsAPI.DsSendStringCommand("SelectPlayer_Countdown text " + selectPlayersCountdown);
            DsAPI.DsSendStringCommand("SelectPlayer_Countdown intensity 80 dur 1");

            // Set label position
            int labelwidth = 10;
            double offset = -(labelwidth * (numberOfControllers - 2)) - 90;
            int offsetAdjustment = -15;

            offsetAdjustment = offsetAdjustment + numberOfControllers * 5;

            offset += offsetAdjustment;

            /*if (numberOfControllers == 2) offset -= 5;
            if (numberOfControllers == 4) offset += 5;
            if (numberOfControllers == 5) offset += 10;
            if (numberOfControllers == 6) offset += 15;
            if (numberOfControllers == 7) offset += 20;
            if (numberOfControllers == 8) offset += 25;*/

            for (int i = 0; i < numberOfControllers; i++)
            {
                positionValue.Azimuth = offset;
                positionValue.Elevation = 10;
                positionValue.Distance = 11;
                DsAPI.DsSetObjectAttr(startlabelObjId[i], smcPosAttrIndex, positionValue, true);
                DsAPI.DsSendStringCommand("startlabel" + players[i].NR_IN_DS + " intensity 60 dur 1");
                DsAPI.DsSendStringCommand("scene add startlabel" + players[i].NR_IN_DS);
                offset += labelwidth;
            }

            Thread.Sleep(600);
        }

        public void PlayerSelect()
        {
            
            for (int i = 0; i < numberOfControllers; i++)
            {

                if (controllers[i] != null && !players[i].isActive)
                {
                    JoystickState state = controllers[i].GetCurrentState();
                    if (state.Buttons[0])
                    {
                        players[i].activate();
                        menuSelectSound.play();
                    }
                    
                }
                
                
                
            }

            if (selectPlayersCountdown == 0 || numberOfActivePlayers == numberOfControllers)
            {
                DsAPI.DsSendStringCommand("SelectPlayer_Text intensity 0 dur 1"); 
                DsAPI.DsSendStringCommand("SelectPlayer_Countdown intensity 0 dur 1");



                if (numberOfActivePlayers < 2)
                {
                    for (int i = 0; i < numberOfControllers; i++)
                    {
                        DsAPI.DsSendStringCommand("scene remove startlabel" + players[i].NR_IN_DS);
                        players[i].isActive = false;
                    }
                    Thread.Sleep(1000);
                    status = "init";
                }
                else
                {
                    Thread.Sleep(1000);
                    GameInit();
                }
            } 

            playerSelectTimeOut--;
            if (playerSelectTimeOut == 0)
            {
                selectPlayersCountdown--;
                DsAPI.DsSendStringCommand("SelectPlayer_Countdown text " + selectPlayersCountdown);

                playerSelectTimeOut = 50;
            }

        }

        public void Credits()
        {
            status = "";
            Script.PlaySkript(Properties.Resources.Credits);
            Thread.Sleep(16500);
            status = "init";
        }

        public void Instructions()
        {
            Script.PlaySkript(Properties.Resources.InitIntstr);
            DsAPI.DsSendStringCommand("js play " + path + "/DsScripts/instructions.js");
            Thread.Sleep(75000);
            
            Script.PlaySkript(Properties.Resources.InstrDone);
            status = "init";
        }
        // Game Strucutre ---------------------------------

        public void GameInit()
        {
            // Stop the music (fade out)
            gameLoop.setVolume(0, 5);
            // Change the background image to the game grid background
            DsAPI.DsSendStringCommand("BG_ss frame 4");
            // Fade the background image in
            DsAPI.DsSendStringCommand("BG intensity 100 duration 1");

            // Calculate the position for the player related objects such as startlabel, score, ect.
            // the distance between the objects depends on the number of players --> calc the angle
            double angle = 360 / numberOfControllers;

            // Init points, counter and startlabel
            for (int i = 0; i < numberOfControllers; i++)
            {
                
                double horizontalPlayerPosition = angle * i - 180; // calculate the objects position depending on player number with a 180 degree shift because values must go from -180 to 180
                // Set the points (text obejct)
                positionValue.Azimuth = horizontalPlayerPosition;
                positionValue.Elevation = 83.2;
                positionValue.Distance = 9;
                DsAPI.DsSetObjectAttr(pointsObjId[i], t2cPosAttrIndex, positionValue, true);
                textValue.Value = Convert.ToString(players[i].points);
                DsAPI.DsSetObjectAttr(pointsObjId[i], t2cTextAttrIndex, textValue, true);

                colorValue.Red = players[i].RED;
                colorValue.Green = players[i].GREEN;
                colorValue.Blue = players[i].BLUE;

                DsAPI.DsSetObjectAttr(pointsObjId[i],t2cColAttrIndex, colorValue, true);            

                DsAPI.DsSendStringCommand("scene add points" + players[i].NR_IN_DS);
                // Set the counter (frame around points)
                DsAPI.DsSendStringCommand("points" + players[i].NR_IN_DS + " intensity 100 duration 2");
                DsAPI.DsSendStringCommand("counter" + players[i].NR_IN_DS + " modelTexture 0 " + path + "img/counter/Counter" + numberOfControllers + "-0" + i + ".png");
                DsAPI.DsSendStringCommand("counter" + players[i].NR_IN_DS + " attitude 0 -90 " + horizontalPlayerPosition);
                DsAPI.DsSendStringCommand("scene add counter" + players[i].NR_IN_DS);
                // Set the startlabel
                DsAPI.DsSendStringCommand("counter" + players[i].NR_IN_DS + " intensity 100 duration 2 ");
                DsAPI.DsSendStringCommand("startlabel" + players[i].NR_IN_DS + " position spherical " + horizontalPlayerPosition + " 5 11 dur 2");
                DsAPI.DsSendStringCommand("scene add startlabel" + players[i].NR_IN_DS);

            }

            // Calc and display points total (needed to win game)
            pointsTotal = numberOfControllers * pointsTotalFactor;

            textValue.Value = Convert.ToString(pointsTotal);
            if (pointsTotal == 6 || pointsTotal == 9) textValue.Value += "."; // append '.' so 6 and 9 are distinguishable
            DsAPI.DsSetObjectAttr(pointsTotalObjId, t2cTextAttrIndex, textValue, true);

            DsAPI.DsSendStringCommand("pointstotal intensity 100 dur 1");
            DsAPI.DsSendStringCommand("pointstotal attitude cartesian rate 5 0 0 dur 1");

            // Stop the rotation of the eye
            DsAPI.DsSendStringCommand("eye attitude cartesian rate 0 0 0 duration 1");

            for (int i = 0; i < winners.Length; i++) winners[i] = -1;
            numberOfWinners = 0;

            // Init first round
            RoundInit();
        }

        public void RoundInit()
        {

            // Set the startlabels
            // Set player varibles right
            playersAlive = 0;
            for (int i = 0; i < numberOfControllers; i++)
            {
                playersAlive++;
                players[i].isAlive = true;
                players[i].dir = "up";
                DsAPI.DsSendStringCommand("startlabel" + players[i].NR_IN_DS + " modelTexture 0 " + path + "/img/label/label.dds");
                DsAPI.DsSendStringCommand("startlabel" + players[i].NR_IN_DS + " int 100 dur 1");
            }


            // Inits Trails and Countdown

            Script.PlaySkript(Properties.Resources.InitTrails);
            
            double angle = 360 / numberOfControllers;

            //Display the snakes
            for (int i = 0; i < numberOfControllers; i++)
            {

                players[i].setPos(angle * i - 180, 5);
                positionValue.Azimuth = players[i].x;
                positionValue.Elevation = players[i].y;
                positionValue.Distance = 11;
                DsAPI.DsSetObjectAttr(snakeObjId[i], smcPosAttrIndex, positionValue, true);

                colorValue.Red = players[i].RED;
                colorValue.Green = players[i].GREEN;
                colorValue.Blue = players[i].BLUE;

                DsAPI.DsSetObjectAttr(trailObjId[i], trailColAttrIndex, colorValue, true);

                DsAPI.DsSendStringCommand("trail" + players[i].NR_IN_DS + " intensity 0");
                DsAPI.DsSendStringCommand("scene add snake" + players[i].NR_IN_DS);

            }

            Script.PlaySkript(Properties.Resources.Countdown);

            //Display the snakes
            for (int i = 0; i < numberOfControllers; i++) DsAPI.DsSendStringCommand("trail" + players[i].NR_IN_DS + " intensity 100 dur 1");

            // Empty the position arrays
            clearSections();
            // Wait for the countdown to end

            frame = 0;
            status = "running";

            // Fade out the startlabels
            for (int i = 0; i < numberOfControllers; i++) DsAPI.DsSendStringCommand("startlabel" + players[i].NR_IN_DS + " int 0 dur 1");

        }

        public void RoundDone()
        {

            for (int i = 0; i < numberOfControllers; i++)
            {
                DsAPI.DsSendStringCommand("trail" + players[i].NR_IN_DS + " delete");
                DsAPI.DsSendStringCommand("scene remove snake" + players[i].NR_IN_DS);
            }


            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].onscreen) items[i].remove();
                if (items[i].isActive) items[i].stop();
            }


            gameLoop.setVolume(0, 4);

            items[1].resetBorder();

            if (numberOfWinners > 0) GameDone(false); // false, because not an debug action
            else RoundInit();
        }

        public static void GameDone(bool debug)
        {
            // ----------------------------------------------------------------------------------------
            if (debug)
            {
                for (int i = 0; i < numberOfControllers; i++)
                {
                    DsAPI.DsSendStringCommand("trail" + +players[i].NR_IN_DS + " delete");
                    DsAPI.DsSendStringCommand("scene remove snake" + players[i].NR_IN_DS);
                }
            }

            // -----------------------------------------------------------------------------------------

            DsAPI.DsSendStringCommand("BG intensity 0 duration 2");
            DsAPI.DsSendStringCommand("BG_ss frame 0");
            DsAPI.DsSendStringCommand("pointstotal intensity 0 dur 1");

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                {
                    DsAPI.DsSendStringCommand("points" + players[i].NR_IN_DS + " intensity 0 duration 1");
                    DsAPI.DsSendStringCommand("counter" + players[i].NR_IN_DS + " intensity 0 duration 1");
                    DsAPI.DsSendStringCommand("startlabel" + players[i].NR_IN_DS + " intensity 0 duration 1");
                }
            }

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                {
                    DsAPI.DsSendStringCommand("startlabel" + players[i].NR_IN_DS + " modelTexture 0 " + path + "/img/label/label_" + players[i].NR_IN_DS + ".dds");
                    DsAPI.DsSendStringCommand("scene remove points" + players[i].NR_IN_DS);
                    DsAPI.DsSendStringCommand("scene remove counter" + players[i].NR_IN_DS);
                    DsAPI.DsSendStringCommand("scene remove startlabel" + players[i].NR_IN_DS);
                }
            }


            DsAPI.DsSendStringCommand("eye reset");
            DsAPI.DsSendStringCommand("pointstotal intensity 0 dur 1");
            DsAPI.DsSendStringCommand("pointstotal attitude cartesian rate 0 0 0 dur 1");

            Thread.Sleep(100);


            
            DsAPI.DsSendStringCommand("BG intensity 100 duration 1");

            // Hier muss noch die Gameloopmusic auf null gesetzt werden und anhalten
            gameLoop.setVolume(0, 2);
            gameLoop.stop();
            //  Hier muss noch der gewinner sound abgespielt werden
            winSound.play();
            // Hier muss noch kontrolleirt werden wer alles gewonnen hat
            for (int i = 0; i < winners.Length; i++)
            {
                if (i == numberOfWinners - 1)
                {
                    int z = i + 1;
                    positionValue.Azimuth = -90;
                    positionValue.Elevation = 5 * (z + 3);
                    positionValue.Distance = 11;
                    DsAPI.DsSetObjectAttr(winnersObjId, smcPosAttrIndex, positionValue, true);
                    DsAPI.DsSendStringCommand("winners intensity 100 dur 1");
                }


                if (winners[i] > -1)
                {
                    positionValue.Azimuth = -90;
                    positionValue.Elevation = 5 * (i + 3);
                    positionValue.Distance = 11;
                    DsAPI.DsSetObjectAttr(playerObjId[winners[i]], smcPosAttrIndex, positionValue, true);

                    scaleValue.X = scaleValue.Y = scaleValue.Z = 0.5;
                    DsAPI.DsSetObjectAttr(playerObjId[winners[i]], smcScaleAttrIndex, scaleValue, true); // duration 2 fehlt noch

                    DsAPI.DsSendStringCommand("player" + players[winners[i]].NR_IN_DS + " intensity 100 dur 1");
                }
            }

            Thread.Sleep(4000);

            for (int i = 0; i < winners.Length; i++)
            {
                if (i == numberOfWinners - 1)
                {
                    int z = i + 1;
                    DsAPI.DsSendStringCommand("winners intensity 0 dur 1");
                }

                if (winners[i] > -1)
                {
                    scaleValue.X = scaleValue.Y = scaleValue.Z = 0;
                    DsAPI.DsSetObjectAttr(playerObjId[winners[i]], smcScaleAttrIndex, scaleValue, true); // duration 2 fehlt noch
                    DsAPI.DsSendStringCommand("player" + players[winners[i]].NR_IN_DS + " intensity 0 dur 1");
                }
            }

            Thread.Sleep(2000);

            for (int i = 0; i < numberOfControllers; i++)
            {
                players[i] = null;
            }

            status = "init"; // Initialize the menu


        }

        public void clearSections()
        {
            for (int i = 0; i < 32; i++)
            {
                for (int z = 0; z < 18; z++)
                {
                    sections[i, z].Clear();
                }
            }
        }

        // In-Game ----------------------------------------

        public void ProcessUserInput()
        {


            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null && players[i].isAlive)
                {
                    players[i].setDir(controllers[i]);
#if DEBUG
                    players[i].panic(controllers[i]);
#endif
                }

                
            }


        }

        public void Update()
        {

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null && players[i].isAlive) players[i].move();
            }
            // spawn item every 15 seconds
            if (frame % 200 == 0)
            {
                // Check if there are less than all items onscreen
                if (itemsOnscreen < items.Length)
                {
                    // Got through items and check for the first item, that is not onscreen an inactive 
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (!items[i].onscreen && !items[i].isActive)
                        {
                            // Spwan this item
                            items[i].spawn();
                            break;
                        }
                    }
                }
            }

        }

        public void Render()
        {
            // an den Digistar senden
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null && players[i].isAlive) {
                    players[i].renderPos();
                    textValue.Value = Convert.ToString(players[i].points);
                    DsAPI.DsSetObjectAttr(pointsObjId[i], t2cTextAttrIndex, textValue, true);
                }
                
            }

            // an den Digistar senden
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].justSpawned) items[i].renderPos();
                if (items[i].justCollected)
                {
                    items[i].remove();
                    items[i].start();
                }

                if (items[i].isActive) items[i].timer();
            }

        }

        // End Game --------------------------------------

        public void CleanUp()
        {
            Script.PlaySkript(Properties.Resources.SystemReset);
        }

      
    }
}
