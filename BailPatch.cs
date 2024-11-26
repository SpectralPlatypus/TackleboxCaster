using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TackleboxCharSwitch
{
    [HarmonyPatch]
    internal class BailPatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(NPC_TackleboxBail))
                .Where(method => method.Name.StartsWith("<OnTalk>g__SelectDialogue")).First();
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchForward(false,
                    new CodeMatch(OpCodes.Callvirt, typeof(PlayerMachine).GetMethod("isTackle"))
                )
                .ThrowIfInvalid("Could not find call to Player.isTackle")
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Pop))
                .SetOpcodeAndAdvance(OpCodes.Br_S);

            return codeMatcher.Instructions();
        }
    }
}
