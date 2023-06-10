using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using InscryptionAPI.Localizing;

namespace GoogleTranslateMod
{
	[Serializable]
	public class TranslationManager
	{
		private static string FullPath => Path.Combine(Plugin.Directory, "AllTranslations.gttsv");
		
		// Card has (おねがいします) as its name, we translated that into English 'Zergling'
		// おねがいします is a key.
		// Zergling is the translation
		// Both point to the same Translations
		internal Dictionary<string, Translations> KeyToTranslations = new Dictionary<string, Translations>();
		
		// Stores from Zergling to おねがいします
		internal Dictionary<string, string> TranslationToKey = new Dictionary<string, string>();

		public static TranslationManager Load()
		{
			TranslationManager translationManager = new TranslationManager();


			int files = 0;
			IEnumerable<string> enumerable = Directory.EnumerateFiles(Paths.PluginPath, "*.gttsv", SearchOption.AllDirectories);
			foreach (string path in enumerable)
			{
				List<GoogleTranslateTSV> rows = GoogleTranslateTSV.Load(path);
				foreach (GoogleTranslateTSV row in rows)
				{
					foreach (Language language in Enum.GetValues(typeof(Language)))
					{
						string translation = row.GetString(language);
						if (!string.IsNullOrEmpty(translation))
						{
							translationManager.CacheTranslation(language, row.Original, translation);
						}
					}
				}

				files++;
			}

			foreach (Translations value in translationManager.KeyToTranslations.Values)
			{
				string englishString = value.TryGet(Language.English, "");
				foreach (KeyValuePair<Language,string> pair in value.LanguageToTranslation)
				{
					LocalizationManager.New(Plugin.PluginGuid, null, englishString, pair.Value, pair.Key);
				}
			}

			Plugin.Log.LogInfo($"Loaded {files} files!");
			return translationManager;
		}

		public void Save()
		{
			GoogleTranslateTSV.Export(this, FullPath);
			Plugin.Log.LogInfo($"Saved to {FullPath}");
		}

		public string GetTranslation(Language language, string translation, out string originalKey)
		{
			if (!TranslationToKey.TryGetValue(translation, out originalKey))
			{
				return null;
			}

			return GetTranslationFromOriginal(language, originalKey);
		}
		
		public string GetTranslationFromOriginal(Language language, string originalKey)
		{
			if (!KeyToTranslations.TryGetValue(originalKey, out Translations keyData))
			{
				return null;
			}

			string languageTranslation = keyData.TryGet(language, null);
			if (languageTranslation != null)
			{
				return languageTranslation;
			}

			return null;
		}
		
		public string GetOriginal(string key)
		{
			if (TranslationToKey.TryGetValue(key, out string originalKey))
			{
				if (KeyToTranslations.TryGetValue(originalKey, out Translations data))
				{
					return data.Original;
				}
			}

			return null;
		}

		public void CacheTranslation(Language language, string original, string resultTranslate)
		{
			Translations keyData = null;
			if (!KeyToTranslations.TryGetValue(original, out keyData))
			{
				keyData = new Translations();
				keyData.Original = original;
				KeyToTranslations[original] = keyData; // assuming everything is in english
				TranslationToKey[original] = original;
			}

			keyData.LanguageToTranslation[language] = resultTranslate;
			TranslationToKey[resultTranslate] = original;
		}
	}

	[Serializable]
	public class Translations
	{
		public string Original;
		public Dictionary<Language, string> LanguageToTranslation = new Dictionary<Language, string>();

		public string TryGet(Language language, string defaultText)
		{
			if (LanguageToTranslation.TryGetValue(language, out string translation))
			{
				return translation;
			}

			return defaultText;
		}
	}
}