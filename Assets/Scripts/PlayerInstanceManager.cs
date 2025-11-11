using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class PlayerInstanceManager
{
    private static Player player;

    public static Player Player
    {
        get => player;
        set
        {
            if (value != null)
                player = value;
            else
                throw new NullReferenceException("Player cannot be null");
        }
    }
}
