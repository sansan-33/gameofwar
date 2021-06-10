using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayMeta : MonoBehaviour
{
    public static Dictionary<string, string> ArenaLevelTextDict = new Dictionary<string, string>()
    {
        {"1-1" , "Not saving any 3 star cards" },
        {"1-2" , "Save 2  star Cavalary"  },
        {"1-3" , "Save 3 Star Cavalery"  },
        {"1-4" , "Save 3 Star Cavalery + Attack, Defend HP Up"  },


        {"2-1" , "Not saving any 3 star cards + wall" },
        {"2-2" , "Save 2  star Magic and Tank + wall"  },
        {"2-3" , "Save 3 Star Magic and Tank + wall"  },
        {"2-4" , "Save 3 Star Magic and Tank + Attack, Defend HP Up + wall"  },



        {"3-1" , "Not saving any 3 star cards + wall + Spin Attack" },
        {"3-2" , "Save 2  star Cavalar + wally + Spin Attack"  },
        {"3-3" , "Save 3 Star Cavalery + wall + Spin Attack"  },
        {"3-4" , "Save 3 Star Cavalery + Attack, Defend HP Up + wall + Spin Attack"  },



        {"4-1" , "Not saving any 3 star cards + wall + Arrow Rain" },
        {"4-2" , "Save 2  star Cavalary + wall + Absolute Defense"  },
        {"4-3" , "Save 3 Star Cavalery + wall + Calalry Charges"  },
        {"4-4" , "Save 3 Star Cavalery + Attack, Defend HP Up + wall + Double Calalry Charges"  },

    };
}
