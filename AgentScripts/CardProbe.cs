using System.Collections;
using Clover.Player;
using Clover.World;
using UnityEngine;

// Play Mode probe for the cards over the test yard's stones and shrines, and for the Fists and Katana rack stands.
// Stands the player by a form stone, the element stone and both shrines, logs what each card says, changes the
// element and checks the shrine's card follows, and photographs each. Run in the yard:
//   bash AgentScripts/runprobe.sh CombatTest CardProbe
public static class CardProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Card Probe").AddComponent<CardProbeRunner>();
        return "probe started";
    }
}

public class CardProbeRunner : MonoBehaviour
{
    private static void Say(string text) => Debug.Log("[Probe] " + text.Replace("\n", " / "));

    private IEnumerator Shoot(string name)
    {
        yield return new WaitForSeconds(0.7f);
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
    }

    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        var interaction = player.GetComponent<PlayerInteraction>();
        yield return new WaitForSeconds(0.5f);
        int cards = FindObjectsByType<StoneCard>(FindObjectsSortMode.None).Length;
        Say($"cards in the yard: {cards}");

        FormStone dragon = null, turn = null;
        foreach (var stone in FindObjectsByType<FormStone>(FindObjectsSortMode.None))
        {
            if (stone.Form == SpellForms.Dragon) dragon = stone;
            if (string.IsNullOrEmpty(stone.Form)) turn = stone;
        }
        GiftShrine gift = null, consume = null;
        foreach (var shrine in FindObjectsByType<GiftShrine>(FindObjectsSortMode.None))
        {
            if (shrine.Consumes) consume = shrine;
            else gift = shrine;
        }

        // Far from everything no card shows.
        player.Teleport(new Vector3(0f, 0.05f, 0f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        int up = 0;
        foreach (var card in FindObjectsByType<StoneCard>(FindObjectsSortMode.None))
            if (card.Visible) up++;
        Say($"cards showing in the middle of the yard: {up} (wanted 0)");

        player.Teleport(dragon.transform.position + new Vector3(0f, 0.05f, 0.9f), Quaternion.Euler(0f, 180f, 0f));
        yield return Shoot("card_form_stone");
        Say("form stone: " + dragon.GetComponent<StoneCard>().Showing);
        up = 0;
        foreach (var card in FindObjectsByType<StoneCard>(FindObjectsSortMode.None))
            if (card.Visible) up++;
        Say($"cards showing beside one form stone: {up}");

        player.Teleport(turn.transform.position + new Vector3(0f, 0.05f, 1.3f), Quaternion.Euler(0f, 180f, 0f));
        yield return Shoot("card_element_stone");
        Say("element stone: " + turn.GetComponent<StoneCard>().Showing);

        player.Teleport(gift.transform.position + new Vector3(0f, 0.05f, 1.2f), Quaternion.Euler(0f, 180f, 0f));
        yield return Shoot("card_gift_shrine");
        Say("gift shrine: " + gift.GetComponent<StoneCard>().Showing);
        string before = gift.GetComponent<StoneCard>().Showing;
        turn.Use(interaction);
        yield return new WaitForSeconds(0.6f);
        string after = gift.GetComponent<StoneCard>().Showing;
        Say($"gift shrine after the element changed: {(after != before ? "new words" : "SAME WORDS")}: " + after);

        player.Teleport(consume.transform.position + new Vector3(0f, 0.05f, 1.2f), Quaternion.Euler(0f, 180f, 0f));
        yield return Shoot("card_consume_shrine");
        Say("consume shrine: " + consume.GetComponent<StoneCard>().Showing);

        // The whole row of form stones with their names, and the weapon rack with something on every stand.
        player.Teleport(new Vector3(0f, 0.05f, -13.8f), Quaternion.Euler(0f, 180f, 0f));
        yield return Shoot("card_form_row");
        player.Teleport(new Vector3(-6f, 0.05f, 10.2f), Quaternion.identity);
        yield return Shoot("card_weapon_rack");
        foreach (var choice in FindObjectsByType<LoadoutChoice>(FindObjectsSortMode.None))
        {
            var shown = choice.transform.Find("Shown");
            int parts = shown != null ? shown.GetComponentsInChildren<Renderer>(true).Length : 0;
            if (parts == 0) Say($"EMPTY STAND: {choice.Choice}");
        }
        Say("done: cards");
    }
}
