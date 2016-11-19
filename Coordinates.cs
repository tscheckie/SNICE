using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNICE.GameCode
{
    public class Coordinates
    {
        public double X;
        public double Y;
        public string TYPE;
        public int FRAME;
        public int NR;

        public int RING;
        public int SEGMENT;
        public int RING_UP;
        public int RING_DOWN;
        public int SEGMENT_LEFT;
        public int SEGMENT_RIGHT;

        

        public Coordinates (double x, double y, string type, int player_nr)
        {
            X = x;
            Y = y;
            TYPE = type;
            NR = player_nr;
            FRAME = Game.frame;


            // Get the RING the obj is in (elevation)		
            if (0 <= y && y < 5) RING = 0; // from 0 to 4.99
            if (5 <= y && y < 10) RING = 1; // from 5 to 9.99
            if (10 <= y && y < 15) RING = 2;
            if (15 <= y && y < 20) RING = 3;
            if (20 <= y && y < 25) RING = 4;
            if (25 <= y && y < 30) RING = 5;
            if (30 <= y && y < 35) RING = 6;
            if (35 <= y && y < 40) RING = 7;
            if (40 <= y && y < 45) RING = 8;
            if (45 <= y && y < 50) RING = 9;
            if (50 <= y && y < 55) RING = 10;
            if (55 <= y && y < 60) RING = 11;
            if (60 <= y && y < 65) RING = 12;
            if (65 <= y && y < 70) RING = 13;
            if (70 <= y && y < 75) RING = 14;
            if (75 <= y && y < 80) RING = 15; // from 75 to 79.99


            // Get the segment the obj is in (azimuth)
            if (180 >= x && x > 168.75) SEGMENT = 0;
            if (168.75 >= x && x > 157.5) SEGMENT = 1;
            if (157.5 >= x && x > 146.25) SEGMENT = 2;
            if (146.25 >= x && x > 135) SEGMENT = 3;
            if (135 >= x && x > 123.75) SEGMENT = 4;
            if (123.75 >= x && x > 112.5) SEGMENT = 5;
            if (112.5 >= x && x > 101.25) SEGMENT = 6;
            if (101.25 >= x && x > 90) SEGMENT = 7;
            if (90 >= x && x > 78.75) SEGMENT = 8;
            if (78.75 >= x && x > 67.5) SEGMENT = 9;
            if (67.5 >= x && x > 56.25) SEGMENT = 10;
            if (56.25 >= x && x > 45) SEGMENT = 11;
            if (45 >= x && x > 33.75) SEGMENT = 12;
            if (33.75 >= x && x > 22.5) SEGMENT = 13;
            if (22.5 >= x && x > 11.25) SEGMENT = 14;
            if (11.25 >= x && x > 0) SEGMENT = 15;
            if (0 >= x && x > -11.25) SEGMENT = 16;
            if (-11.25 >= x && x > -22.5) SEGMENT = 17;
            if (-22.5 >= x && x > -33.75) SEGMENT = 18;
            if (-33.75 >= x && x > -45) SEGMENT = 19;
            if (-45 >= x && x > -56.25) SEGMENT = 20;
            if (-56.25 >= x && x > -67.5) SEGMENT = 21;
            if (-67.5 >= x && x > -78.75) SEGMENT = 22;
            if (-78.75 >= x && x > -90) SEGMENT = 23;
            if (-90 >= x && x > -101.25) SEGMENT = 24;
            if (-101.25 >= x && x > -112.5) SEGMENT = 25;
            if (-112.5 >= x && x > -123.75) SEGMENT = 26;
            if (-123.75 >= x && x > -135) SEGMENT = 27;
            if (-135 >= x && x > -146.25) SEGMENT = 28;
            if (-146.25 >= x && x > -157.5) SEGMENT = 29;
            if (-157.5 >= x && x > -168.75) SEGMENT = 30;
            if (-168.75 >= x && x > -180) SEGMENT = 31;


            RING_UP = RING + 1;
            RING_DOWN = RING - 1;
            SEGMENT_LEFT = SEGMENT - 1;
            if (SEGMENT_LEFT == -1) SEGMENT_LEFT = 31;
            SEGMENT_RIGHT = SEGMENT + 1;
            if (SEGMENT_RIGHT == 32) SEGMENT_RIGHT = 0;


        }
    }
}
