using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Protection : MonoBehaviour
{
    protected Fighter mProtected, mProtector;

    public Fighter GetProtected() { return mProtected; }
    public Fighter GetProtector() { return mProtector; }
    public void SetProtected(Fighter fighter) { mProtected = fighter; }
    public void SetProtector(Fighter fighter) { mProtector = fighter; }
}
