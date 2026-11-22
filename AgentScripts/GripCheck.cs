using System.Linq;
using System.Text;
using Clover.EditorTools;
using Clover.EditorTools.Forge;
using Clover.Player;
using UnityEditor;
using UnityEngine;

// Edit mode: hangs the weapons on a Forge copy of the player and reports each hold (pose, the hand's turn, where the
// weapon sits in the hand) and the finger poses read from the grip file.
//   unity command script run -- --file AgentScripts/GripCheck.cs --entry GripCheck.Run
public static class GripCheck
{
    public static string Run()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.PlayerModelPath);
        var said = new StringBuilder();
        using (var rig = new ForgeRig(asset))
        {
            WeaponBuilder.Hang(rig.Model, asset);
            var grip = rig.Model.GetComponent<HandGrip>();
            if (grip == null) return "no HandGrip";
            foreach (var hold in grip.Holds)
            {
                var part = hold.Part.transform;
                var hand = part.parent.parent;
                Vector3 inHand = hand.InverseTransformPoint(part.position);
                Vector3 along = hand.InverseTransformDirection(part.forward);
                float turn = Quaternion.Angle(Quaternion.identity, hold.Turn);
                said.AppendLine($"{hold.Part.name} hand {hold.Hand} {hold.Pose}: turn {turn:0.0} deg, at {inHand.ToString("F3")} along {along.ToString("F3")}");
            }
            int middle2 = 7;
            said.AppendLine($"middle2.R open->closed {Quaternion.Angle(grip.FingerIn(HandGrip.Pose.Open, middle2), grip.FingerIn(HandGrip.Pose.Closed, middle2)):0.0} deg, ->fist {Quaternion.Angle(grip.FingerIn(HandGrip.Pose.Open, middle2), grip.FingerIn(HandGrip.Pose.Fist, middle2)):0.0}; middle2.L {Quaternion.Angle(grip.FingerIn(HandGrip.Pose.Open, 15 + middle2), grip.FingerIn(HandGrip.Pose.Closed, 15 + middle2)):0.0}");
            said.AppendLine($"open = rest? {Quaternion.Angle(grip.FingerIn(HandGrip.Pose.Open, middle2), rig["middle2.R"].RestLocalRotation):0.00} deg");
        }
        System.IO.File.WriteAllText("review/checks/grip_check.txt", said.ToString());
        return said.ToString();
    }
}
