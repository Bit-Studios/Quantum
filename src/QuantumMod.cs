using BepInEx;
using KSP.Modding;
using UnityEngine;
using System.Collections;
using HarmonyLib;
using KSP.Game;
using System.Reflection;

namespace Quantum;
[BepInPlugin("com.shadow.quantum", "Quantum", "0.0.1")]
public class QuantumMod : BaseUnityPlugin { 
    public static string ModId = "com.shadow.quantum";
    public static string ModName = "Quantum";
    public static string ModVersion = "0.0.1";
    private static string LocationFile = Assembly.GetExecutingAssembly().Location;
    private static string LocationDirectory = Path.GetDirectoryName(LocationFile);
    void Awake()
    {
        Harmony.CreateAndPatchAll(typeof(PatchLoader));
        Debug.Log("PatchLoader");
        if (Directory.Exists(Path.GetFullPath($@"{LocationDirectory}\..\..\GameData"))){}
        else
        {
            Directory.CreateDirectory(Path.GetFullPath($@"{LocationDirectory}\..\..\GameData"));
            Directory.CreateDirectory(Path.GetFullPath($@"{LocationDirectory}\..\..\GameData\Mods"));
        }
        if (Directory.Exists(Path.GetFullPath($@"{LocationDirectory}\..\..\GameData\Mods"))) { }
        else
        {
            Directory.CreateDirectory(Path.GetFullPath($@"{LocationDirectory}\..\..\GameData\Mods"));
        }
    }
}
public class PatchLoader
{
    [HarmonyPatch(typeof(KSP2Mod))]
    [HarmonyPatch("Load")]
    [HarmonyPostfix]
    public static bool KSP2Mod_Load(KSP2Mod __instance, ref bool __result)
    {
        __result = true;
        if (__instance.EntryPoint.EndsWith(".dll"))
        {
            __instance.GetType().GetField("modType", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, KSP2ModType.CSharp);
        }
        if (__instance.APIVersion < KSP2ModManager.MinAPISupported)
        {
            __instance.GetType().GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, KSP2ModState.Error);
            __result = false;
            return false;
        }
        if (__instance.APIVersion > KSP2ModManager.CurrentAPISupported)
        {
            __instance.GetType().GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, KSP2ModState.Error);
            __result = false;
            return false;
        }
        if (GameManager.Instance.Game.KSP2ModManager.InvalidAPIs.Contains(__instance.APIVersion))
        {
            __instance.GetType().GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, KSP2ModState.Error);
            __result = false;
            return false;
        }
        switch ((KSP2ModType)__instance.GetType().GetField("modType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance))
        {
            case KSP2ModType.CSharp:
                Assembly ModCore = Assembly.LoadFile(Path.Combine(__instance.ModRootPath, __instance.EntryPoint));
                GameObject ModObject = new GameObject(__instance.ModName);
                ModObject.name = __instance.ModName;
                foreach(var mctype in ModCore.GetTypes())
                {
                    if (mctype.IsSubclassOf(typeof(Mod)))
                    {
                        ModObject.AddComponent(mctype);
                    }
                }
                ModObject.transform.parent = GameManager.Instance.Game.transform;
                break;
            default:
                __instance.GetType().GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, KSP2ModState.Error);
                __result = false;
                return false;
        }
        __instance.GetType().GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, KSP2ModState.Active);
        return false;
    }
}