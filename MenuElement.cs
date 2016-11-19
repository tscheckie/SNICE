using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNICE.GameCode
{
    public class MenuElement
    {
        public string OBJ_NAME;
        public bool selected;
        public string color;
        public string name;


        public MenuElement (int number)
        {
            OBJ_NAME = "menuItem" + number;

            if (number == 0)
            {
                selected = true;
                color = "100 100 100";
            }
            else
            {
                selected = false;
                color = "66 66 66";
            }

            switch (number)
            {
                case 0:
                    name = "play";
                    break;
                case 1:
                    name = "instr";
                    break;
                case 2:
                    name = "credits";
                    break;
                case 3:
                    name = "quit";
                    break;
            }

             
        }
    }
}
