using UnityEngine;

namespace SoftRenderer
{
    internal class Debug
    {
        public static void Log(bool condition, params object[] args)
        {
            if (condition)
            {
                string msg = "";
                for (int i = 0; i < args.Length; i++)
                {
                    msg += args[i].ToString() + "  ";
                }
                UnityEngine.Debug.Log(msg);
            }
        }

        public static void Log(params object[] args)
        {
            string msg = "";
            for (int i = 0; i < args.Length - 1; i++)
            {
                msg += args[i].ToString() + "  ";
            }
            msg += args[args.Length - 1].ToString();
            UnityEngine.Debug.Log(msg);
        }
    }
}
