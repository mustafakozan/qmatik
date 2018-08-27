using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour {

	public void bankayagir()
    {
        NewMethod();
    }

    private static void NewMethod()
    {
#pragma warning disable CS0618 // Tür veya üye eski
        Application.LoadLevel(1);
#pragma warning restore CS0618 // Tür veya üye eski
    }
}
