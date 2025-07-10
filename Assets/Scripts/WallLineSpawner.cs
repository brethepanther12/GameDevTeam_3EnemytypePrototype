using UnityEngine;

public class WallLineSpawner : MonoBehaviour
{
    [SerializeField] GameObject wallPrefab;
    [SerializeField] int wallCount;
    void Start()
    {
        if (wallPrefab == null || wallCount < 1)
        {
            return;
        }

        GameObject previousWall = Instantiate(wallPrefab, transform.position, Quaternion.identity);

        for (int i = 1; i < wallCount; i++)
        {
            Transform prevSnap = previousWall.transform.Find("LRHC_Snap_Point");
            if (prevSnap == null)
            {
                break;
            }

            GameObject newWall = Instantiate(wallPrefab, prevSnap.position, prevSnap.rotation);

            Transform newSnap = newWall.transform.Find("LLHC_Snap_Point");
            if (newSnap == null)
            {
                break;
            }

            Vector3 correctionOffset = newWall.transform.position - newSnap.position;
            newWall.transform.position += correctionOffset;

            previousWall = newWall;
        }
    }

}
