using System;
using System.IO;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace SiegeSanity
{
    public sealed class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            try
            {
                new Harmony("shedoy23.siegesanity").PatchAll(typeof(SubModule).Assembly);
                Log.Write("Harmony PatchAll OK");
            }
            catch (Exception ex)
            {
                Log.Write("Harmony PatchAll FAILED: " + ex);
            }
        }
    }

    internal static class Log
    {
        private static readonly string Path_ = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "Mount and Blade II Bannerlord", "Configs", "SiegeSanity.log");

        public static void Write(string s)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path_));
                File.AppendAllText(Path_,
                    DateTime.Now.ToString("HH:mm:ss") + " " + s + Environment.NewLine);
            }
            catch { }
        }
    }
}
