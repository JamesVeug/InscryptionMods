using System;
using System.Collections;
using InscryptionAPI.Localizing;
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleTranslateMod.Services;

public class GoogleTranslate
{
	public static IEnumerator Translate(Language language, string originalText, Plugin.Result result)
	{
		string baseUrl = "https://translate.google.com/translate_a/single";

        // The parameters for the request
        string param = "?client=gtx&sl=auto&tl=" + Plugin.LanguageToCode(language) + "&dt=t&q=" + WWW.EscapeURL(originalText);

        // Create a new UnityWebRequest
        UnityWebRequest www = UnityWebRequest.Get(baseUrl + param);

        // Send the request
        //Log.LogInfo($"Translating {language} {originalText}");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
	        Plugin.Log.LogError("Something went wrong. Recommend restarting the game!");
	        Plugin.Log.LogError("Error while trying to translate: " + www.error);
	        result.englishTranslation = null;
	        result.translation = null;
	        result.succeeded = false;
	        yield break;
        }

        try
        {
	        string remoteTranslation = ExtractText(www.downloadHandler.text);
	        Plugin.TranslationsManager.CacheTranslation(language, originalText, remoteTranslation);
	        result.translation = remoteTranslation;
	        result.englishTranslation = Plugin.TranslationsManager.GetTranslationFromOriginal(Language.English, originalText);
	        result.succeeded = true;
        }
        catch (Exception e)
        {
	        Plugin.Log.LogError(language);
	        Plugin.Log.LogError(originalText);
	        Plugin.Log.LogError("Something went wrong. Recommend restarting the game!");
	        Plugin.Log.LogError(www.downloadHandler.text);
	        Plugin.Log.LogError(e);
	        result.englishTranslation = null;
	        result.translation = null;
	        result.succeeded = false;
        }
	}
	
	public static string ExtractText(string jsonString) {
		string japaneseText = "";
		jsonString = jsonString.Trim('[', ']');
		string[] parts = jsonString.Split(new string[] { "\",\"" }, StringSplitOptions.None);
		japaneseText = parts[0];
		japaneseText = japaneseText.Trim('\"');
		return japaneseText;
	}
}