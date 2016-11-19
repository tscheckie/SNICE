using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ES.Ds.Common;

namespace SNICE.GameCode
{
    public class Sound
    {

        public int volume;
        public string name;
        public bool playing = false;
        public bool looping = false;
        public int duration;
        public int startFrame;

        public Sound(string name)
        {
            this.name = name;

            if (name == "menuLoop") duration = 4800;
            if (name == "gameLoop") duration = 4800;
            if (name == "death") duration = 180;
            if (name == "menuSelectSound") duration = 120;
            if (name == "menuMoveSound") duration = 120;
            if (name == "winSound") duration = 600;
            if (name == "countdown") duration = 360;
            if (name == "item1_collect") duration = 60;           
            if (name == "item3_collect") duration = 4800;
            if (name == "item4_collect") duration = 30;
            if (name == "item5_collect") duration = 360;
        }

        public void play()
        {
            if (playing) DsAPI.DsSendStringCommand(name + " stop");
            
            DsAPI.DsSendStringCommand(name + " play");
            playing = true;
            startFrame = Game.frame;
        }

        public void stop()
        {
            DsAPI.DsSendStringCommand(name + " stop");
            playing = false;
            looping = false;
        }

        public void loop()
        {
            DsAPI.DsSendStringCommand(name + " loop");
            looping = true;
        }



        public void setVolume(int newVol, int duration = 0)
        {
            if (duration == 0) DsAPI.DsSendStringCommand(name + " volume " + newVol);
            else DsAPI.DsSendStringCommand(name + " volume " + newVol + " duration " + duration);
        }

        public void timer()
        {
            // check if an active item needs to be turned off
            if (playing && Game.frame > startFrame + duration) stop();
        }


    }
}
