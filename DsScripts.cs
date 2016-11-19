using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ES.Ds.Common;
using System.Threading;

namespace SNICE.GameCode
{
    public class DsScripts
    {

        public double lastTime = -1;
        private bool add5;
        private double sleeptime;
        private string test = "das ist ein test";
        private char test_char;
        private double currenttime;


        public DsScripts ()
        {
            
        }


        public void PlaySkript(string skript)
        {
            lastTime = -1;
            
            skript = skript.Replace('\r', ' ');
            string[] lines = skript.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
            
                
                // Look for relative paths in the commands and replace them
                string relPath = "..\\";
                if (lines[i].Contains(relPath)) lines[i] = lines[i].Replace(relPath, Game.path);
                if (lines[i].StartsWith("+") )
                {
                    sleeptime = Convert.ToDouble(lines[i].Substring(1, 1));
                    if (lines[i].Length > 4) sleeptime += Convert.ToDouble(lines[i].Substring(3, 1)) / 10;
                    
                    Thread.Sleep(Convert.ToInt32(sleeptime*1000));

                }
                if (char.IsDigit(lines[i][0])) 
                {
                    

                    currenttime = Convert.ToDouble(lines[i].Substring(0, 1));

                    // Check if the line contains a coma, if yes, add .5 to the sleeptime
                    if (lines[i].Substring(1, 1).Equals(".")) currenttime += 0.5;


                    if (lastTime < 0) sleeptime = currenttime;

                    else sleeptime = currenttime - lastTime;

                    Thread.Sleep(Convert.ToInt32(sleeptime*1000));
                    lastTime = currenttime;

                    
                }

                 DsAPI.DsSendStringCommand(lines[i]);


                
            }

            
        }


    }

}
