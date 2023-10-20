using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI;
using InscryptionAPI.Regions;
using UnityEngine;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
	public const string PluginGuid = "jamesgames.inscryption.morecabinnodes";
	public const string PluginName = "More Cabin Nodes";
	public const string PluginVersion = "1.0.0.0";

	public static Plugin Instance;

	public static ManualLogSource Log;

	private void Awake()
	{
		Logger.LogInfo($"Loading {PluginName}...");
		Instance = this;
		Log = Logger;

		new Harmony(PluginGuid).PatchAll();
	}

	private void Start()
	{
		PredefinedNodes[] nodesArray = Resources.LoadAll<PredefinedNodes>("data/authoredmapdata")
			.Where((a) => a.nodeRows.Count == 3 && a.name.ToLower().Contains("final")).ToArray();
		foreach (PredefinedNodes nodes in nodesArray)
		{
			// Create new nodes
			List<NodeData> secondRow = nodes.nodeRows[1];
			float y = secondRow[0].position.y;
			List<NodeData> newNodes = new List<NodeData>()
			{
				/*new CardMergeNodeData() {gridX = 0, gridY = 0, connectedNodes = nextNodes},
				new GainConsumablesNodeData() {gridX = 1, gridY = 0, connectedNodes = nextNodes},
				new TradePeltsNodeData() {gridX = 2, gridY = 2, connectedNodes = nextNodes},
				new BuildTotemNodeData() {gridX = 3, gridY = 2, connectedNodes = nextNodes},*/
				new DuplicateMergeNodeData() {id = 1337, gridX = 4, gridY = 1, position = new Vector2(0,y)},
				new CardRemoveNodeData() {id = 1338, gridX = 5, gridY = 1, position = new Vector2(1,y)},
			};
			secondRow.AddRange(newNodes);

			// Align nicely
			float spacing = 0.08f; // was 0.12f;
			float initialPos = nodes.nodeRows[0][0].position.x;
			float leftOffset = initialPos - ((secondRow.Count - 1) * spacing) / 2;
			for (var i = 0; i < secondRow.Count; i++)
			{
				var node = secondRow[i];
				node.position.x = leftOffset + i * spacing;
			}

			// Connect start to middle
			foreach (NodeData data in nodes.nodeRows[0])
			{
				data.connectedNodes.AddRange(newNodes);
			}
		}
	}
}
