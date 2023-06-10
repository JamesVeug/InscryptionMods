using System;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GoogleTranslateMod.Services;
using HarmonyLib;
using InscryptionAPI.Localizing;

namespace GoogleTranslateMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "_jamesgames.inscryption.googletranslatemod";
	    public const string PluginName = "Google translate mod";
	    public const string PluginVersion = "0.3.0.0";

        public static string Directory;
        public static TranslationManager TranslationsManager;
        public static ManualLogSource Log;
        public static Configs Configs;
        public static Plugin Instance;
        

        private void Awake()
        {
	        Instance = this;
	        Log = Logger;
	        Directory = this.Info.Location.Replace("GoogleTranslateMod.dll", "");
            Logger.LogInfo($"Loading {PluginName}...");
            Configs = new Configs();
            TranslationsManager = TranslationManager.Load();
            new Harmony(PluginGuid).PatchAll();
            StartCoroutine(ChatGPT.Translate(Language.English, String.Empty, null));
            
            Logger.LogInfo($"Loaded {PluginName}!");
        }

        private void OnApplicationQuit()
        {
	        TranslationsManager.Save();
        }

        public class Result
        {
	        public string englishTranslation;
	        public string translation;
	        public bool succeeded;
        }
        
        internal static IEnumerator Translate(string textToTranslate, Result result, Language language=Language.NUM_LANGUAGES)
        {
	        if (language == Language.NUM_LANGUAGES)
	        {
		        language = Localization.CurrentLanguage;
	        }

	        if (language != Language.English)
	        {
		        string englishTranslation =
			        TranslationsManager.GetTranslation(Language.English, textToTranslate, out string _);
		        if (string.IsNullOrEmpty(englishTranslation))
		        {
			        // We don't have the english version yet!
			        // This is required so do it first
			        yield return Translate(textToTranslate, result, Language.English);
		        }
	        }

	        string translation = TranslationsManager.GetTranslation(language, textToTranslate, out string originalText);
	        if (!string.IsNullOrEmpty(translation))
	        {
		        result.englishTranslation = TranslationsManager.GetTranslationFromOriginal(Language.English, originalText);
		        result.succeeded = true;
		        yield break;
	        }

	        if (string.IsNullOrEmpty(originalText))
	        {
		        originalText = textToTranslate;
	        }

	        switch (Configs.ServiceType)
	        {
		        case Configs.Service.GoogleTranslate:
			        yield return GoogleTranslate.Translate(language, originalText, result);
			        break;
		        case Configs.Service.ChatGPT:
			        yield return ChatGPT.Translate(language, originalText, result);
			        break;
		        default:
			        throw new ArgumentOutOfRangeException();
	        }

	        if (result.succeeded)
	        {
		        // This is a new translation
		        // If its not english we want to give it to the base game so it can translate it for us and show it on cards
		        if (language != Language.English)
		        {
			        string english = Plugin.TranslationsManager.GetTranslationFromOriginal(Language.English, originalText);
			        if (english != null)
			        {
				        LocalizationManager.New(Plugin.PluginGuid, null, result.englishTranslation, result.englishTranslation, language);
			        }
		        }
	        }
        }

        public static string LanguageToCode(Language language)
        {
	        switch (language)
	        {
		        case Language.French:
			        return "fr";
		        case Language.Italian:
			        return "it";
		        case Language.German:
			        return "de";
		        case Language.Spanish:
			        return "es";
		        case Language.BrazilianPortuguese:
			        return "pt";
		        case Language.Turkish:
			        return "tr";
		        case Language.Russian:
			        return "ru";
		        case Language.Japanese:
			        return "ja";
		        case Language.Korean:
			        return "ko";
		        case Language.ChineseSimplified:
			        return "zh-CN";
		        case Language.ChineseTraditional:
			        return "zh-TW";
		        default:
			        return "en";
	        }
        }
    }
}
