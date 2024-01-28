using System;
using BepInEx.Configuration;

public static class Configs
{
    public static ConfigEntry<int> MaxRandomizations = Bind("General", "Max randomizations", 2,
        "Maximum amount of changes that can be made to a card");
    
    public static ConfigEntry<bool> SuperMode = Bind("General", "Crazy Mode", false,
        "Increases all values so Goobert will make lots of crazy changes.");

    public static ConfigEntry<int> ChanceOfCardReplacement = Bind("General", "Chance to card replaced", 25,
        "Chance for Goobert to do a bad job painting and give you a different card", new AcceptableValueRange<int>(0, 100));

    public static ConfigEntry<string> CardReplacements = Bind("General", "Card replacements", "RingWorm, Opossum",
        "Cards Goobert gives you if he does a bad job");
    
    public static ConfigEntry<bool> DisablePaintDecal = Bind("General", "Disable Paint Decal", false,
        "Stops Goobert from adding the green paint all over the cards");
    
    public static ConfigEntry<bool> CanShuffleAllSigils = Bind("Sigils", "Shuffle Random Amount of Sigils", true,
        "All sigils have a chance to be shuffled if set to true. Otherwise only 1 will.");

    public static ConfigEntry<int> ChanceOfSigilChange = Bind("Sigils", "Chance To Change Sigils", 25,
        "Weight of chance to change sigils. Higher the value the higher the chance for it to happen.");

    public static ConfigEntry<int> ChanceOfSigilAdd = Bind("Sigils", "Chance To Add a Sigil", 10,
        "Weight of chance to change sigils. Higher the value the higher the chance for it to happen.");

    public static ConfigEntry<int> ChanceOfAttackChange = Bind("Attack", "Chance to Change the Attack", 25,
        "Weight of chance to change attack. Higher the value the higher the chance for it to happen.");

    public static ConfigEntry<int> ChanceOfHealthChange = Bind("Health", "Chance to Change the Health", 25,
        "Weight of chance to change health. Higher the value the higher the chance for it to happen.");

    public static ConfigEntry<int> ChanceOfCostChange = Bind("Cost", "Chance to Change the cost", 10,
        "Weight of chance to change cost. Higher the value the higher the chance for it to happen.");

    public static ConfigEntry<int> ChanceOfTribesChange = Bind("Tribes", "Chance To Change Tribes", 10,
        "Weight of chance to add or change change a Tribe. Higher the value the higher the chance for it to happen.");

    public static ConfigEntry<int> ChanceOfNameChange = Bind("Cosmetic", "Chance to Misspell name", 50,
        "Chance for Goobert to mispell the name. 0% is never and 100% is always", new AcceptableValueRange<int>(0, 100));

    
    private static ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description, AcceptableValueBase values=null)
    {
        return Plugin.Instance.Config.Bind(section, key, defaultValue,
            new ConfigDescription(description, values, Array.Empty<object>()));
    }
}