using UnityEngine;

public interface IGrapplable
{
    bool isBeingGrappled { get; set; }
    bool canBeGrappled { get; }
}