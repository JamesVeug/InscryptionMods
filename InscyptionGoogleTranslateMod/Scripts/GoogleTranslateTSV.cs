using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GoogleTranslateMod
{
	public class GoogleTranslateTSV
	{
		public string Original;
		public string English;
		public string German;
		public string Japanese;
		public string Korean;
		public string French;
		public string Italian;
		public string Spanish;
		public string BrazilianPortuguese;
		public string Turkish;
		public string Russian;
		public string ChineseSimplified;
		public string ChineseTraditional;

		public string GetString(Language language)
		{
			switch (language)
			{
				case Language.English:
					return English;
				case Language.French:
					return French;
				case Language.Italian:
					return Italian;
				case Language.German:
					return German;
				case Language.Spanish:
					return Spanish;
				case Language.BrazilianPortuguese:
					return BrazilianPortuguese;
				case Language.Turkish:
					return Turkish;
				case Language.Russian:
					return Russian;
				case Language.Japanese:
					return Japanese;
				case Language.Korean:
					return Korean;
				case Language.ChineseSimplified:
					return ChineseSimplified;
				case Language.ChineseTraditional:
					return ChineseTraditional;
				default:
					return null;
			}
		}
		
		public static List<GoogleTranslateTSV> Load(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return new List<GoogleTranslateTSV>();
			}

			List<GoogleTranslateTSV> list = new List<GoogleTranslateTSV>();
			string readAllText = File.ReadAllText(filePath);
			List<string> rows = readAllText.Split('\n').ToList();
			for (int i = 1; i < rows.Count; i++)
			{
				string row = rows[i];
				List<string> columns = SplitByTabs(row);
				if (columns.Count < 13)
				{
					if (columns.Count > 1)
					{
						Plugin.Log.LogWarning($"Skipping: row {i}. It has less than 13 columns! [{filePath}]");
					}

					continue;
				}
				GoogleTranslateTSV tsv = new GoogleTranslateTSV();
				tsv.Original = columns[0];
				tsv.English = columns[1];
				tsv.German = columns[2];
				tsv.Japanese = columns[3];
				tsv.Korean = columns[4];
				tsv.French = columns[5];
				tsv.Italian = columns[6];
				tsv.Spanish = columns[7];
				tsv.BrazilianPortuguese = columns[8];
				tsv.Turkish = columns[9];
				tsv.Russian = columns[10];
				tsv.ChineseSimplified = columns[11];
				tsv.ChineseTraditional = columns[12];
					
				list.Add(tsv);
			}

			return list;
		}

		public static List<string> SplitByTabs(string s)
		{
			List<string> split = new List<string>();
			if (string.IsNullOrEmpty(s))
				return split; // return an empty list if input is null or empty
			split = s.Split('\t').ToList();
			return split;
		}
		
		public static void Save(string filePath, List<GoogleTranslateTSV> list)
		{
			string readAllText = "";

			List<string> headers = new List<string>();
			headers.Add(nameof(Original));
			headers.Add(nameof(English));
			headers.Add(nameof(German));
			headers.Add(nameof(Japanese));
			headers.Add(nameof(Korean));
			headers.Add(nameof(French));
			headers.Add(nameof(Italian));
			headers.Add(nameof(Spanish));
			headers.Add(nameof(BrazilianPortuguese));
			headers.Add(nameof(Turkish));
			headers.Add(nameof(Russian));
			headers.Add(nameof(ChineseSimplified));
			headers.Add(nameof(ChineseTraditional));
			readAllText += string.Join("\t", headers) + "\n";

			foreach (GoogleTranslateTSV csv in list)
			{
				readAllText += csv.Original + "\t";
				readAllText += csv.English + "\t";
				readAllText += csv.German + "\t";
				readAllText += csv.Japanese + "\t";
				readAllText += csv.Korean + "\t";
				readAllText += csv.French + "\t";
				readAllText += csv.Italian + "\t";
				readAllText += csv.Spanish + "\t";
				readAllText += csv.BrazilianPortuguese + "\t";
				readAllText += csv.Turkish + "\t";
				readAllText += csv.Russian + "\t";
				readAllText += csv.ChineseSimplified + "\t";
				readAllText += csv.ChineseTraditional + "\n";
			}
			
			File.WriteAllText(filePath, readAllText);
		}

		public static void Export(TranslationManager translations, string path)
		{
			List<GoogleTranslateTSV> list = new List<GoogleTranslateTSV>();
			foreach (KeyValuePair<string,Translations> translation in translations.KeyToTranslations)
			{
				GoogleTranslateTSV r = new GoogleTranslateTSV();
				r.Original = translation.Key;
				r.English = translation.Value.TryGet(Language.English, "");
				r.Japanese = translation.Value.TryGet(Language.Japanese, "");
				r.German = translation.Value.TryGet(Language.German, "");
				r.Korean = translation.Value.TryGet(Language.Korean, "");
				r.French = translation.Value.TryGet(Language.French, "");
				r.Italian = translation.Value.TryGet(Language.Italian, "");
				r.Spanish = translation.Value.TryGet(Language.Spanish, "");
				r.BrazilianPortuguese = translation.Value.TryGet(Language.BrazilianPortuguese, "");
				r.Turkish = translation.Value.TryGet(Language.Turkish, "");
				r.Russian = translation.Value.TryGet(Language.Russian, "");
				r.ChineseSimplified = translation.Value.TryGet(Language.ChineseSimplified, "");
				r.ChineseTraditional = translation.Value.TryGet(Language.ChineseTraditional, "");
				list.Add(r);
			}
			
			Save(path, list);
		}
	}
}