using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using InscryptionAPI.Localizing;
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleTranslateMod.Services;

public class ChatGPT
{
	private static string baseUrl = "https://api.openai.com/v1/completions";
	private static string apiKey = "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";
	
	public static IEnumerator Translate(Language language, string originalText, Plugin.Result result)
	{
		string readAllText = File.ReadAllText(Path.Combine(Plugin.Directory, "chatgptprompt"));
		readAllText =
			"Translate the following sentence into Chinese\n.I like eating mince and cheese pies. They are delicious with tomato sauce.";
		Debug.Log("prompt: " + readAllText);

		var content = "{\"model\": \"text-davinci-001\", \"prompt\": \""+ readAllText +"\",\"temperature\": 1,\"max_tokens\": 100}";

		WWWForm form = new WWWForm();
		//form.headers["Content-Type"] = "application/json";
		//form.headers["authorization"] = "Bearer " + apiKey;
		form.AddField("model", "text-davinci-003");
		form.AddField("prompt", "Say this is a test");
		form.AddField("temperature", 0);
		form.AddField("max_tokens", 7);
		form.AddField("top_p", 7);
		form.AddField("n", 1);
		form.AddField("stream", "false");
		form.AddField("logprobs", "null");
		form.AddField("stop", "\n");
			
		// Send the POST request to the OpenAI API
		using (UnityWebRequest webRequest = UnityWebRequest.Post(baseUrl, form))
		{
			//webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(content));
			/*webRequest.downloadHandler = new DownloadHandlerBuffer();*/
			webRequest.SetRequestHeader("Content-Type", "application/json");
			webRequest.SetRequestHeader("authorization", "Bearer " + apiKey);

			yield return webRequest.SendWebRequest();

			if (webRequest.isNetworkError || webRequest.isHttpError)
			{
				Debug.LogError("Error: " + webRequest.error);
			}
			else
			{
				string responseBody = webRequest.downloadHandler.text;
				Debug.Log("Response: " + responseBody);
			}
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