using UnityEngine;
using TMPro;
using System.Collections;

public class TMPActivator : MonoBehaviour
{
    [SerializeField] private TextMeshPro[] tmpObjects; // Array to hold 6 TMP objects

    void Start()
    {
        // Ensure all TMPs are inactive at the start
        foreach (var tmp in tmpObjects)
        {
            if (tmp != null)
                tmp.gameObject.SetActive(false);
        }

        // Start the activation sequence
        StartCoroutine(ActivateTMPs());
    }

    IEnumerator ActivateTMPs()
    {
        // Wait for 1 second before starting
        yield return new WaitForSeconds(1f);

        // Activate first 4 TMPs with 2-second intervals
        for (int i = 0; i < 4; i++)
        {
            if (tmpObjects[i] != null)
            {
                tmpObjects[i].gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(2f);
        }

        // Activate 5th and 6th TMPs together
        if (tmpObjects[4] != null)
        {
            tmpObjects[4].gameObject.SetActive(true);
        }
        if (tmpObjects[5] != null)
        {
            tmpObjects[5].gameObject.SetActive(true);
        }
    }
}