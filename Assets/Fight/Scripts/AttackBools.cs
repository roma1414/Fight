using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBools
{
    protected bool mAbsorbed, mOccular, mTouchSuccess;

    public bool CheckAbsorbed() { return mAbsorbed; }
    public bool CheckOccular() { return mOccular; }
    public bool CheckTouchSuccess() { return mTouchSuccess; }
    public void SetAbsorbed(bool absorbed) { mAbsorbed = absorbed; }
    public void SetOccular(bool occular) { mOccular = occular; }
    public void SetTouchSuccess(bool touchSuccess) { mTouchSuccess = touchSuccess; }
}
