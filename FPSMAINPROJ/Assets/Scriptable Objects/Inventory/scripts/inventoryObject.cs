using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory/New Inventory")]
public class inventoryObject : ScriptableObject
{
    public List<inventorySlot> containerForInv = new List<inventorySlot>();
    public GameObject primary;
    public GameObject secondary;
    public void AddItem(pickupObject _pickup, int _amount)
    {
        bool hasPickup = false;
        for (int i = 0; i < containerForInv.Count; i++)
        {
            if (containerForInv[i].pickup == _pickup && containerForInv[i].pickup.amount < _pickup.maxAmount)
            {
                containerForInv[i].addAmount(_amount);
                hasPickup = true;
                
                break;
            }
        }
        if(!hasPickup)
        {
            containerForInv.Add(new inventorySlot(_pickup, _amount));

            if (_pickup.type == itemType.Primary || _pickup.type == itemType.Secondary)
            {
                if (_pickup.type == itemType.Primary)
                {
                    useItem(itemType.Primary);
                }
                else if (_pickup.type == itemType.Secondary)
                {
                    useItem(itemType.Secondary);
                }
            }
        }
    }

    public void useItem(itemType type)
    {
        for (int i = 0;i < containerForInv.Count;i++)
        {
            if (containerForInv[i].pickup.type == type && containerForInv[i].amount > 0)
            {
                IPickup pickupComponent = containerForInv[i].pickup as IPickup; //CANT USE GETCOMPONENT FOR scriptableObjects 
                if (pickupComponent != null)
                {
                    pickupComponent.useItem();
                    Debug.Log("in use item inventory");
                    if (containerForInv[i].pickup.destroyAfterUse)
                    {
                        containerForInv[i].addAmount(-1);

                        if (containerForInv[i].amount <= 0)
                        {
                            containerForInv.RemoveAt(i);
                        }
                    }
                    else
                    {
                        Debug.Log("Didnt destory");
                    }
                    break;
                }
                else
                {
                    Debug.Log("doesnt contain a IPICKUP");
                }
            }
        }
    }

    public bool hasItem(itemType type )
    {
        for(int i = 0; i < containerForInv.Count; i++)
        {
            if (containerForInv[i].pickup.type == type)
            {
                return true;
            }
            
        }
        return false;
    }

    public pickupObject getItem(itemType type)
    {
        for (int i = 0; i < containerForInv.Count; i++)
        {
            if (containerForInv[i].pickup.type == type)
            {
                return containerForInv[i].pickup;
            }

        }
        return null;
    }

    public void removeWeapon(itemType type)
    {
        for (int i = 0; i < containerForInv.Count; i++)
        {
            if (containerForInv[i].pickup.type == type)
            {
                containerForInv.RemoveAt(i);
            }
        }
    }
}

//SO WE CAN HAVE SEPERATE SLOTS FOR EACH ITEM TYPE AND HAVE DIFFRENET TYPES OF EACH ITEM
[System.Serializable]
public class inventorySlot
{
    public pickupObject pickup;
    public int amount;
    public int maxAmount;
    public inventorySlot(pickupObject _pickup, int _amount)
    {
        pickup = _pickup;
        amount = _amount;
    }

    public void addAmount(int value)
    {
        if(amount > maxAmount)
        {
            return;
        }    
        amount += value;
    }

}
