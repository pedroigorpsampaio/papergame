using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator
{
    [Serializable]
    public class SaveGameData
    {
        public int level;
        public int seed;
        public int n_keys;
        public int n_foods;
        public int n_treasures;
        public int n_shields;
        public int n_armors;
        public int n_potions;
        public int n_weapons;
        public float player_pos_x;
        public float player_pos_y;
    }
}