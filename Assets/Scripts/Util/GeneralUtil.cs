using UnityEngine;
using System.Linq;

public static class GeneralUtil  {

    public static string TEST_1 = "Tentative";

    /**
     * Throws error if the parameter is null
     */
    public static void Require<T>(params T[] variables)
    {
        int nonNull = variables.Count(v => v != null);
        int total = variables.Length;

        if(nonNull != total)
        {
            Debug.LogError("Expected to find " + total + " of type " + typeof(T) + ". Got " + nonNull);
        }

    }

}
