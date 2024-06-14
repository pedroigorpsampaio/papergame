using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator.AI
{
    public class Action
    {
        // types of action
        public enum Type { Attack, Run, Follow, Patrol, Reward };
        // current action
        public Type type;

        public void Initialize(Type type)
        {
            this.type = type;
        }

    }
}
