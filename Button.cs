using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator
{
    public class Button
    {
        public enum Type { LeftScroll, RightScroll, UpArmor, DownArmor, UpShield,
                            DownShield, UpWeapon, DownWeapon, UpFood, DownFood,
                             UpPotion, DownPotion, UpKey, DownKey, NextLevel, CloseTotemHUD, Null};
        public Type type;

        public enum Group { TotemHUD, ItemScroll }

        public Group group;

        public Button(Type type, Group group)
        {
            this.type = type;
            this.group = group;
        }
    }
}
