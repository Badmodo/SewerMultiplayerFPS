using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamMember : MonoBehaviour
{
    public int TeamID;
    public GameObject alligator;
    public GameObject rats;
    public GameObject alligatorText;
    public GameObject ratsText;


    private void Start()
    {
        TeamID = Random.Range(18, 20);

        if (TeamID == 18)
        {
            alligator.SetActive(true);
            rats.SetActive(false);
            alligatorText.SetActive(true);
            ratsText.SetActive(false);
        }
        else if (TeamID == 19)
        {
            alligator.SetActive(false);
            rats.SetActive(true);
            alligatorText.SetActive(false);
            ratsText.SetActive(true);
        }
    }
}
