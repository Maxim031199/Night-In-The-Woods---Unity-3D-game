
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    // Map your item numbers in the same order as your UI:
    // 0 = Flashlight, 1 = NightVision, 2 = Lighter, 3 = Rags, ... etc.
    public void UseItem(int itemNumber)
    {
        switch (itemNumber)
        {
            case 0:  
                Debug.Log("Use Flashlight");
               
                break;

            case 1:  
                Debug.Log("Use Night Vision Item");
                
                break;

            case 2: 
                Debug.Log("Use Lighter");
                break;

            case 3: 
                Debug.Log("Use Rags (craft/medkit?)");
                break;

            
            default:
                Debug.Log($"Use item index {itemNumber}");
                break;
        }
    }
}
