using BepInEx.Logging;
using Fleece;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TackleboxCharSwitch
{
    [HarmonyPatch]
    internal class Patches
    {
        static bool playAsCaster = false;
        static MethodInfo LoadGameSceneWithFadeOut = AccessTools.Method(typeof(MainMenu), "LoadGameSceneWithFadeOut");

        [HarmonyPatch(typeof(PlayerMachine), nameof(PlayerMachine.CutsceneMove), new Type[] { typeof(Vector3), typeof(float) })]
        [HarmonyPrefix]
        public static void CutsceneMove(PlayerMachine __instance)
        {
            if (playAsCaster)
                __instance.ToggleBetweenProtagAndTackle();
        }
        [HarmonyPatch(typeof(TackleboxIntroCutScene), "OnSkipCutscene")]
        [HarmonyPrefix]
        public static void OnSkipCutscene()
        {
            if(playAsCaster)
                Manager.GetPlayerMachine()?.ToggleBetweenProtagAndTackle();
        }

        [HarmonyPatch(typeof(MainMenu), "DoNewGamePlus")]
        [HarmonyPrefix]
        public static void DoNewGamePlus(MainMenu __instance, SaveData data)
        {
            data.Clear();
            data.SetDateStarted(DateTime.Now);
            SaveManager saveManager = Manager.GetSaveManager();
            saveManager.PrepareGameState(data);
            saveManager.SaveCurrentFile();
            data._newSave = true;

            playAsCaster = true;
            LoadGameSceneWithFadeOut.Invoke(__instance, new object[] { __instance._gameScene });
        }

        [HarmonyPatch(typeof(MainMenu), "DoNewGame")]
        [HarmonyPrefix]
        public static void DoNewGame(MainMenu __instance, SaveData data)
        {
            playAsCaster = false;
        }

        static ModalRequest customModal = null;
        [HarmonyPatch(typeof(Manager),"Awake")]
        [HarmonyPostfix]
        public static void Awake(Manager __instance)
        {
            if (customModal == null)
            {
                var req = __instance._modalRequests;
                Jumper jumper = new Jumper();
                int newId = 666;
                var pass = new Passage
                {
                    text = "Choose Your Character",
                    colorIndex = 0,
                    id = newId
                };

                Jumper jumper2 = new Jumper();
                newId += 50;
                var pass2 = new Passage
                {
                    text = "<c=#4b897a>Caster",
                    //text = "<c=#f7bf51>Caster",
                    colorIndex = 4,
                    id = newId
                };


                Jumper jumper3 = new Jumper();
                newId -= 20;
                var pass3 = new Passage
                {
                    text = "<c=#9b3950ff>Tackle",
                    colorIndex = 1,
                    id = newId
                };

                Story.active.passages.Add(pass);
                Story.active.passages.Add(pass2);
                Story.active.passages.Add(pass3);

                jumper.passage = pass;
                jumper2.passage = pass2;
                jumper3.passage = pass3;

                customModal = req.QueueRequest(jumper, true, true, jumper3);
                customModal._showButtons = true;
                customModal._threeButton = true;
                customModal._altButtonTextOverride = jumper2;
                customModal._overrideAltButtonText = true;

                req._overwriteNewGamePlus = customModal;
                req._overwriteGame = customModal;
            }
        }


        [HarmonyPatch(typeof(MenuButton), nameof(MenuButton.ManagedAwake))]
        [HarmonyPrefix]
        public static bool ManagedAwake(MenuButtonArt __instance)
        {
            return false;
        }

        [HarmonyPatch(typeof(MenuButtonArt), nameof(MenuButtonArt.ManagedAwake))]
        [HarmonyPostfix]
        public static void ManagedAwakePost(MenuButtonArt __instance)
        {
            __instance._textProvider._mesh.color = __instance._textProvider._jumper.passage.color;
            __instance._buttonFront.material.color = __instance._textProvider._jumper.passage.color;
        }
    }
}
