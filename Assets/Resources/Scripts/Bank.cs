using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bank : Interactable
{
    Player player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
    {
        base.Interact();
        List<Relic> relics = player.getBackpack();
        for(int iter = 0;iter<relics.Count;iter++)
        {
            player.setFunds(player.getFunds() + relics[iter].value);
            player.removeBackpack(relics[iter]);
        }
        FindObjectOfType<Director>().refreshUI();
    }
}
