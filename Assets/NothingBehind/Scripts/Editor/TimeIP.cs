using UnityEditor;
using UnityEngine;

namespace NothingBehind.Scripts.Editor
{
    [InitializeOnLoad]
    public class TimeIP : MonoBehaviour
    {
        public static float time;
        public static string ProjectName;


        static TimeIP()
        {
            ProjectName = GetProjectName();
            EditorApplication.update += UpdateTime;
        }

        static void UpdateTime()
        {
            if (time < EditorApplication.timeSinceStartup)
            {
                time = float.Parse(EditorApplication.timeSinceStartup.ToString()) + 1;
                EditorPrefs.SetFloat("TimeIP_" + ProjectName,
                    EditorPrefs.GetFloat("TimeIP_" + ProjectName) + time -
                    float.Parse(EditorApplication.timeSinceStartup.ToString()));
            }
        }

        [MenuItem("Window/TimeIP _#T")]
        public static void Test()
        {
            string q = "";
            string w = "";
            if (Application.systemLanguage == SystemLanguage.Russian)
            {
                q = "Общее время: ";
                w = "Время сеанса: ";
            }
            else
            {
                q = "Total time: ";
                w = "Session time: ";
            }

            float z = EditorPrefs.GetFloat("TimeIP_" + ProjectName);
            int h = Mathf.FloorToInt(z / 3600);
            int m = Mathf.FloorToInt(z / 60) - 60 * h;
            int s = Mathf.FloorToInt(z) - (m * 60 + 3600 * h);
            string hms = string.Format("{0:00}:{1:00}:{2:00}", h, m, s);

            float zz = float.Parse(EditorApplication.timeSinceStartup.ToString());
            int hh = Mathf.FloorToInt(zz / 3600);
            int mm = Mathf.FloorToInt(zz / 60) - 60 * hh;
            int ss = Mathf.FloorToInt(zz) - (mm * 60 + 3600 * hh);
            string hms2 = string.Format("{0:00}:{1:00}:{2:00}", hh, mm, ss);

            print(q + hms + "\n" +
                  w + hms2);
        }


        public static string GetProjectName()
        {
            string[] s = Application.dataPath.Split('/');
            string projectName = s[s.Length - 2];
            return projectName;
        }
    }
}