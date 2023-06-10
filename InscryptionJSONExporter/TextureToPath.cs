using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace JSONExporter
{
	[HarmonyPatch]
	public static class TextureToPath
	{
		private static Dictionary<Texture, string> TextureToPathLookup = new Dictionary<Texture, string>();
		private static Dictionary<byte[], string> ReadAllBytesToPath = new Dictionary<byte[], string>();

		public static string GetTexturePath(Texture texture2D)
		{
			if (TextureToPathLookup.TryGetValue(texture2D, out string path))
			{
				return path;
			}

			return null;
		}
		
		[HarmonyPostfix, HarmonyPatch(typeof(TextureHelper), nameof(TextureHelper.GetImageAsTexture), new Type[]{typeof(string), typeof(FilterMode)})]
		private static void TextureHelper_GetImageAsTexture_Postfix(ref Texture2D __result, string pathCardArt, FilterMode filterMode)
		{
			TextureToPathLookup[__result] = pathCardArt;
		}
		
		[HarmonyPostfix, HarmonyPatch(typeof(File), nameof(File.ReadAllBytes), new Type[]{typeof(string)})]
		private static void File_ReadAllBytes_Postfix(ref byte[] __result, string path)
		{
			ReadAllBytesToPath[__result] = path;
		}
		
		[HarmonyPostfix, HarmonyPatch(typeof(ImageConversion), nameof(ImageConversion.LoadImage), new Type[]{typeof(Texture2D), typeof(byte[])})]
		private static void TextureHelper_LoadImage_Postfix(ref bool __result, Texture2D tex, byte[] data)
		{
			if (ReadAllBytesToPath.TryGetValue(data, out string path))
			{
				if (TextureToPathLookup.ContainsKey(tex))
				{
					TextureToPathLookup[tex] = path;
				}
				else
				{
					TextureToPathLookup.Add(tex, path);
				}
			}
		}
	}
}