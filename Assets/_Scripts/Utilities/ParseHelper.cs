using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ParseHelper
{
    public static string CamelCaseToSpaced(string camelCaseString)
    {
        if (string.IsNullOrEmpty(camelCaseString))
            return string.Empty;

        return Regex.Replace(camelCaseString, "(\\B[A-Z])", " $1");
    }
}
