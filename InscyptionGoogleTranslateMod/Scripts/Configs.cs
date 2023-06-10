using System;
using BepInEx.Configuration;

namespace GoogleTranslateMod;

public class Configs
{
	public enum Service
	{
		GoogleTranslate,
		ChatGPT
	}
	
	public readonly Service ServiceType = Bind("All", "Translation Service", Service.GoogleTranslate, "Which service do you want translating text?");
	public readonly string ChatGPTAPIKey = Bind("Chat GPT", "API Key", "get from openapi.com", "Get your key here https://platform.openai.com/account/api-keys");
        
	private static T Bind<T>(string section, string key, T defaultValue, string description)
	{
		return Plugin.Instance.Config.Bind(section, key, defaultValue, new ConfigDescription(description, null, Array.Empty<object>())).Value;
	}
	
}