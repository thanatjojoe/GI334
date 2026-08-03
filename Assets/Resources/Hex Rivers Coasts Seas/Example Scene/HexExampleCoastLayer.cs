/*
	 * Hey, so I'm an artist, not a coder. Don't expect miracles.
	 * *
	 * This example code is provided to show one possible and definitely 
	 * inefficient approach to arranging coastlines and rivers using Unity's tilemap system.
	 * 
	 * You're certainly free to use this code as a base for your own projects, 
	 * but I recommend taking a hard look through the details and improving it.
	 * 
	 * - David
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Text.RegularExpressions;

namespace dgbExamples
{
	public class HexExampleCoastLayer : MonoBehaviour
	{
		// This tilemap is for rivers
		public Tilemap tilemapRivers;

		// This tilemap is for land
		public Tilemap tilemap;

		// This tilemap is for water
		public Tilemap tilemapWater;

		// This tilemap is for the "underground" pieces
		public Tilemap tilemapUnder;

		// Do you want the 'underground' side-view of tiles to be generated at all?
		public bool useUnderground = true;

		// Should we put coasts on sea tiles next to null cells/map edges? Generally no.
		public bool treatNullAsWater = true;

		// Do you want to use coasts at all? (In case you just want to generate rivers.)
		public bool useCoasts = true;

		// Do you want rivers to connect to the map edge?
		public bool riversConnectToMapEdge = false;

		// The side-view underground or underwater tiles.
		public TileBase below_water;
		public TileBase below_waterCoastEast;
		public TileBase below_waterCoastWest;
		public TileBase below_waterCoastBoth;
		public TileBase below_dirt;

		// All possible usable coast tiles go in here.
		// Does not include acute coast corners which are handled as sprites outside the Tilemap system.
		public TileBase[] coastHexes;

		// All possible usable river tiles go in here.
		public TileBase[] riverHexes;

		// River mouths are sprites that get stacked on the appropriate tile.
		public Sprite[] riverMouthSprites;

		// We'll instantiate sprites for river mouths and store them here.
		public List<GameObject> riverMouths;

		// Used for detecting if a tile's neighbours are adjacent to land or water.
		// If you're 1337, you might use bit operations to do all of this stuff in a way that is technically more efficient.
		// As a n00b, I shall use strings and arrays.
		private string landCode = "1";
		private string waterCode = "0";

		// The bounds of the map you've created will go here
		private BoundsInt bounds;

		// This is used to create an overlay sprite for terrain tiles that should draw overtop of coasts and rivers.
		// It's a bit of a hack because we can't layer sprites within a tile with respect to other tile rows
		// Eg. Mountains and tall castles
		public Sprite spriteSliceMask;

		// This is a whitelist of tile names that will create an overlay sprite to draw overtop of coasts and rivers.
		// This is basically a bunch of tall mountains. Does not include trailing numbers.
		public string[] overlayWhitelist;

		// tall tile sprite overlays are held here.
		private List<GameObject> overlaySprites;

		public void Start()
		{
			ProcessTileMap();
		}

		public void ProcessTileMap()
		{
			// Call this function to reset all coastlines, underground tiles, and rivers.

			//// If anything is on the water layer, put it back to land layer.
			//// This overwrites the land layer, so be mindful.
			CombineTilemap();

			//// Get the bounds and all tiles on base map layer.
			bounds = tilemap.cellBounds;

			//// Put underground/water tiles into their own tile layer.
			if (useUnderground) RefreshUndergroundHexes();

			//// Split land and water tiles. Water tiles are set to their own layer.
			SplitTilemap();

			//// Generate coastlines
			if (useCoasts) RefreshCoasts();

			//// Set up rivers tiles.
			RebuildRivers();
			RebuildRiverMouths();

			//// Create overlay sprites for any terrain tiles that poke out too much as defined by the whitelist.
			CheckAllTilesForOverlaySprites();
		}

		public void RebuildRiverMouths()
		{
			// iterate all water tiles. Check to see if they neighbour rivers. If so, place sprites.

			// First, clean out the old sprites if they exist.
			if (riverMouths != null) if (riverMouths.Count > 0) foreach (GameObject go in riverMouths) Destroy(go);
			riverMouths = new List<GameObject>();

			TileBase[] waterTiles = tilemapWater.GetTilesBlock(bounds);

			for (int x = 0; x < bounds.size.x; x++)
			{
				for (int y = 0; y < bounds.size.y; y++)
				{
					TileBase tile = waterTiles[x + y * bounds.size.x];
					if (tile != null && WaterNameCheck(tile.name))
					{
						Vector3Int gridPosition = new Vector3Int(x + bounds.position.x, y + bounds.position.y, bounds.position.z);
						string[] neighbours = GetRiverNeighbours(gridPosition);
						string river_neighbours = neighbours[0] + neighbours[1] + neighbours[2] + neighbours[3] + neighbours[4] + neighbours[5];

						if (river_neighbours.Contains("1"))
						{
							for (int i = 0; i < 6; i++)
							{
								if (neighbours[i] == "1")
								{
									GameObject rivermouth_go = new GameObject("riverMouth " + i + " @ " + gridPosition);
									rivermouth_go.transform.parent = transform;
									rivermouth_go.transform.position = tilemap.CellToWorld(gridPosition) - (Vector3.up * tilemap.layoutGrid.cellSize.x * 0.5f);

									SpriteRenderer corner_sr = rivermouth_go.AddComponent<SpriteRenderer>();
									string rivermouth_sprite_name = "100000";
									if (i == 1) rivermouth_sprite_name = "010000";
									if (i == 2) rivermouth_sprite_name = "001000";
									if (i == 3) rivermouth_sprite_name = "000100";
									if (i == 4) rivermouth_sprite_name = "000010";
									if (i == 5) rivermouth_sprite_name = "000001";

									corner_sr.sprite = GetRiverMouthSpriteByName(rivermouth_sprite_name);
									corner_sr.sortingLayerName = "Rivers";
									corner_sr.sortingOrder = 2; // If the sorting layer isn't set up, this ensures that they appear above the tilemap.
									riverMouths.Add(rivermouth_go);
								}
							}
						}
					}
				}
			}
		}

		private Sprite GetRiverMouthSpriteByName(string name)
		{
			List<Sprite> variations = new List<Sprite>();
			foreach (Sprite s in riverMouthSprites) if (s.name.Contains(name)) variations.Add(s);
			if (variations.Count == 0)
			{
				Debug.Log("Could not find sprite called '" + name + "'");
				return null;
			}
			return variations[Random.Range(0, variations.Count)];
		}

		public void RefreshCoasts()
		{
			Debug.Log("doing RefreshCoasts");

			TileBase[] allWaterTiles = tilemapWater.GetTilesBlock(bounds);

			for (int x = 0; x < bounds.size.x; x++)
			{
				for (int y = 0; y < bounds.size.y; y++)
				{
					TileBase tile = allWaterTiles[x + y * bounds.size.x];
					Vector3Int position = new Vector3Int(x + bounds.position.x, y + bounds.position.y, bounds.position.z);

					if (tile != null)
					{
						// Find hex neighbours.
						// This will return an array w/ direction stored by position: 
						// 0 is Northwest, 1 is Northeast, 2 is East, 3 is Southeast, 4 is Southwest, 5 is West 
						string[] neighboursArray = GetNeighbours(position);
						string neighbours = neighboursArray[0] + neighboursArray[1] + neighboursArray[2] +
											neighboursArray[3] + neighboursArray[4] + neighboursArray[5];

						if (neighbours.Contains("1")) tilemap.SetTile(position, GetCoastHexByCode(neighbours));
					}
				}
			}
		}

		public void RebuildRivers()
		{
			// Iterate over all rivers set in tilemap and change them to appropriate sprite based on neighbours.

			// Yes, a scriptable tile is the 'correct' way to do this.
			// (But it has an additional package dependency and I don't want to mess with it.)

			// While we're at it, if river is cardinally adjacent to water, then need to add a river->water connection.

			BoundsInt river_bounds = tilemapRivers.cellBounds;
			TileBase[] allRiverTiles = tilemapRivers.GetTilesBlock(bounds);

			for (int x = 0; x < bounds.size.x; x++)
			{
				for (int y = 0; y < bounds.size.y; y++)
				{
					TileBase tile = allRiverTiles[x + y * bounds.size.x];
					if (tile != null)
					{
						// Check if tile is a river. If not, something is on the wrong layer.
						if (!tile.name.Contains("hexRiver"))
						{
							Debug.LogWarning("Tile " + tile.name + " at " + x + " / " + y + " is not a river! Removing tile.");
							Vector3Int coords = new Vector3Int(x + bounds.position.x, y + bounds.position.y, bounds.position.z);
							tilemapRivers.SetTile(coords, null);
						}
						else
						{
							// Okay, you're a river. Let's set the sprite correctly.
							// First check neighbours tiles 
							Vector3Int gridPosition = new Vector3Int(x + bounds.position.x, y + bounds.position.y, bounds.position.z);
							string[] neighbour_rivers = GetRiverNeighbours(gridPosition);
							string rivers_code = neighbour_rivers[0] + neighbour_rivers[1] + neighbour_rivers[2] + neighbour_rivers[3] + neighbour_rivers[4] + neighbour_rivers[5];

							// Treat any adjacent water tile as a river connections.
							// The rivers themselves don't distinguish between water tiles and other rivers.
							// (this is handled in the river mouth building)
							if (rivers_code.Contains("2")) rivers_code = rivers_code.Replace('2', '1');

							tilemapRivers.SetTile(gridPosition, GetRiverHexByCode(rivers_code));
						}
					}
				}
			}
		}




		private TileBase GetCoastHexByCode(string code, bool noRiver = false)
		{
			// Pass in a six-character adjacency code, return a random coast tile that fits that code.
			// "noRiver" allows this to select coasts that don't work with the river overlay.

			List<TileBase> valid_tiles = new List<TileBase>();
			//foreach (TileBase tile in coastTiles) if (tile.name.Contains("hexCoast" + code) && !tile.name.Contains("river")) valid_tiles.Add(tile);
			foreach (TileBase tile in coastHexes) if (tile.name.Contains("hexCoast" + code)) valid_tiles.Add(tile);

			if (valid_tiles.Count == 0)
			{
				Debug.Log("GetCoastTileByCode couldn't find a tile for code: " + code);
				return null; // uh oh
			}

			if (valid_tiles.Count == 1) return valid_tiles[0];
			return valid_tiles[Random.Range(0, valid_tiles.Count)];
		}

		private TileBase GetRiverHexByCode(string code)
		{
			// Pass in a six-character adjacency code, return a random river tile that fits.
			List<TileBase> tiles = new List<TileBase>();
			foreach (TileBase tile in riverHexes) if (tile.name.Contains("hexRiver" + code)) tiles.Add(tile);

			if (tiles.Count == 0)
			{
				Debug.Log("GetRiverTileByCode couldn't find a tile for code: " + code);
				return null; // uh oh
			}

			if (tiles.Count == 1) return tiles[0];
			return tiles[Random.Range(0, tiles.Count)];
		}

		private string[] GetRiverNeighbours(Vector3Int position)
		{
			// 1 is river, 0 is no river. 
			// 2 is water!

			string[] neighbours = new string[6] { "0", "0", "0", "0", "0", "0" };

			TileBase east = tilemapRivers.GetTile(position + Vector3Int.right);
			TileBase west = tilemapRivers.GetTile(position + Vector3Int.left);
			TileBase nw;
			TileBase ne;
			TileBase sw;
			TileBase se;

			if (position.y % 2 == 0) // even		
			{
				nw = tilemapRivers.GetTile(position + Vector3Int.up + Vector3Int.left);
				ne = tilemapRivers.GetTile(position + Vector3Int.up);
				sw = tilemapRivers.GetTile(position + Vector3Int.down + Vector3Int.left);
				se = tilemapRivers.GetTile(position + Vector3Int.down);
			}
			else // odd
			{
				nw = tilemapRivers.GetTile(position + Vector3Int.up);
				ne = tilemapRivers.GetTile(position + Vector3Int.up + Vector3Int.right);
				sw = tilemapRivers.GetTile(position + Vector3Int.down);
				se = tilemapRivers.GetTile(position + Vector3Int.down + Vector3Int.right);
			}

			if (nw != null) neighbours[0] = "1";
			if (ne != null) neighbours[1] = "1";
			if (east != null) neighbours[2] = "1";
			if (se != null) neighbours[3] = "1";
			if (sw != null) neighbours[4] = "1";
			if (west != null) neighbours[5] = "1";

			// treat adjacent water tiles as a river connection.
			// The water tile will handle the river-sea transition.

			// need to override this setting temporarily.
			bool treatNullAsWaterOld = treatNullAsWater;
			if (riversConnectToMapEdge) treatNullAsWater = true;
			else treatNullAsWater = false;

			string[] waterNeighbours = GetNeighbours(position);

			for (int i = 0; i < neighbours.Length; i++)
			{
				if (waterNeighbours[i] == waterCode) neighbours[i] = "2"; // convert this to "1" if needed.
			}

			treatNullAsWater = treatNullAsWaterOld; // don't forget to set it back.

			return neighbours;
		}

		private string[] GetNeighbours(Vector3Int position)
		{
			TileBase east = GetTileWaterOrLand(position + Vector3Int.right);
			TileBase west = GetTileWaterOrLand(position + Vector3Int.left);
			TileBase nw;
			TileBase ne;
			TileBase sw;
			TileBase se;

			if (position.y % 2 == 0) // even		
			{
				nw = GetTileWaterOrLand(position + Vector3Int.up + Vector3Int.left);
				ne = GetTileWaterOrLand(position + Vector3Int.up);
				sw = GetTileWaterOrLand(position + Vector3Int.down + Vector3Int.left);
				se = GetTileWaterOrLand(position + Vector3Int.down);
			}
			else // odd
			{
				nw = GetTileWaterOrLand(position + Vector3Int.up);
				ne = GetTileWaterOrLand(position + Vector3Int.up + Vector3Int.right);
				sw = GetTileWaterOrLand(position + Vector3Int.down);
				se = GetTileWaterOrLand(position + Vector3Int.down + Vector3Int.right);
			}

			string null_value = waterCode;
			if (!treatNullAsWater) null_value = landCode;
			string[] neighbours = new string[6] { null_value, null_value, null_value, null_value, null_value, null_value };

			if (nw != null && WaterNameCheck(nw.name) == true) neighbours[0] = waterCode;
			else if (nw != null) neighbours[0] = landCode;

			if (ne != null && WaterNameCheck(ne.name) == true) neighbours[1] = waterCode;
			else if (ne != null) neighbours[1] = landCode;

			if (east != null && WaterNameCheck(east.name) == true) neighbours[2] = waterCode;
			else if (east != null) neighbours[2] = landCode;

			if (se != null && WaterNameCheck(se.name) == true) neighbours[3] = waterCode;
			else if (se != null) neighbours[3] = landCode;

			if (sw != null && WaterNameCheck(sw.name) == true) neighbours[4] = waterCode;
			else if (sw != null) neighbours[4] = landCode;

			if (west != null && WaterNameCheck(west.name) == true) neighbours[5] = waterCode;
			else if (west != null) neighbours[5] = landCode;

			return neighbours;
		}

		public void ClearUnderground()
		{
			// Remove all underground tiles. They will be reset via code, if desired.
			tilemapUnder.ClearAllTiles();

			// Also: we must remove all "below" type tiles from the land layer.
			// So: don't set these by hand because they'll just get erased.

			BoundsInt bounds = tilemap.cellBounds;
			TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

			for (int x = 0; x < bounds.size.x; x++)
			{
				for (int y = 0; y < bounds.size.y; y++)
				{
					TileBase tile = allTiles[x + y * bounds.size.x];
					if (tile != null)
					{
						if (tile.name.Contains("below"))
						{
							Vector3Int coords = new Vector3Int(x + bounds.position.x, y + bounds.position.y, bounds.position.z);
							tilemap.SetTile(coords, null);
						}
					}
				}
			}
		}

		public void CombineTilemap()
		{
			// If we process the tilemap again... well, this makes it easier.
			// It may be stupid, but it works.

			// Combines the tile types FROM their own maps.
			// Water tiles are sent to the base layer, land stays on the base layer.

			BoundsInt bounds = tilemapWater.cellBounds;
			TileBase[] allTiles = tilemapWater.GetTilesBlock(bounds);

			for (int x = 0; x < bounds.size.x; x++)
			{
				for (int y = 0; y < bounds.size.y; y++)
				{
					TileBase tile = allTiles[x + y * bounds.size.x];
					if (tile != null)
					{
						if (WaterNameCheck(tile.name))
						{
							Vector3Int coords = new Vector3Int(x + bounds.position.x, y + bounds.position.y, bounds.position.z);
							tilemap.SetTile(coords, tile); // add to land tilemap
							tilemapWater.SetTile(coords, null);      // clear from water tilemap
						}
					}
				}
			}

			BoundsInt riverBounds = tilemapRivers.cellBounds;
			TileBase[] riverTiles = tilemapRivers.GetTilesBlock(riverBounds);
			for (int x = 0; x < riverBounds.size.x; x++)
			{
				for (int y = 0; y < riverBounds.size.y; y++)
				{
					TileBase tile = riverTiles[x + y * riverBounds.size.x];
					if (tile != null)
					{
						if (!tile.name.Contains("hexRiver"))
						{
							Vector3Int coords = new Vector3Int(x + riverBounds.position.x, y + riverBounds.position.y, riverBounds.position.z);
							Debug.LogWarning("Found a non-river tile on the river layer at " + coords + "! Removing " + tile.name);
							tilemapRivers.SetTile(coords, null);
						}
					}
				}
			}
		}

		private bool WaterNameCheck(string tile_name)
		{
			// Does this tile's name imply that it is water terrain? 
			// Shockingly, my naming system is consistent enough that this works.
			// If you are adding more tiles, you might need to modify this.
			string nameLower = tile_name.ToLower();
			if (nameLower.Contains("ocean") ||
				 nameLower.Contains("lake") ||
				 nameLower.Contains("island") ||
				 nameLower.Contains("coast")) return true;
			else return false;
		}

		private TileBase GetTileWaterOrLand(Vector3Int position)
		{
			// find a tile on the ExampleTilemap OR ExampleTilemapWater layer.
			// Land layer overrules water.
			// We need this in case people want to re-process the map after the land/water layer split is done.

			TileBase tile = tilemap.GetTile(position);
			if (tile == null) tile = tilemapWater.GetTile(position);

			return tile;
		}

		public void SplitTilemap()
		{
			// Splits the tile types into their own maps.
			// Water tiles are sent to the water layer, land stays on the baser layer.
			// We do this because coast with transparency is drawn overtop of the water.

			TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

			for (int x = 0; x < bounds.size.x; x++)
			{
				for (int y = 0; y < bounds.size.y; y++)
				{
					TileBase tile = allTiles[x + y * bounds.size.x];
					if (tile != null)
					{
						if (WaterNameCheck(tile.name))
						{
							Vector3Int coords = new Vector3Int(x + bounds.position.x, y + bounds.position.y, bounds.position.z);
							tilemapWater.SetTile(coords, tile); // add to water tilemap
							tilemap.SetTile(coords, null);      // clear from land tilemap
						}
					}
				}
			}
		}

		public void RefreshUndergroundHexes()
		{
			// Clear underground first. They will be reset via code.
			tilemapUnder.ClearAllTiles();

			BoundsInt bounds = tilemap.cellBounds;
			TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

			// While we're doing this, remove all "under" type tiles from the land layer.
			// So don't set these by hand because they'll just get erased.
			for (int x = 0; x < bounds.size.x; x++)
			{
				for (int y = 0; y < bounds.size.y; y++)
				{
					TileBase tile = allTiles[x + y * bounds.size.x];
					Vector3Int coords = new Vector3Int(x + bounds.position.x, y + bounds.position.y, bounds.position.z);
					if (tile != null)
					{
						if (tile.name.Contains("hexUnder")) tilemap.SetTile(coords, null);
					}
				}
			}

			// Go through all tilemap positions and set an appropriate underground tile
			// Take bounds of land + water and iterate that combined set.
			// Be sure to check both land and water at each position.

			for (int x = 0; x < bounds.size.x; x++)
			{
				for (int y = 0; y < bounds.size.y; y++)
				{
					TileBase tile = allTiles[x + y * bounds.size.x];
					if (tile != null && !tile.name.Contains("hexUnder"))
					{
						Vector3Int coords = new Vector3Int(x + bounds.position.x, y + bounds.position.y, bounds.position.z);

						TileBase sw;
						TileBase se;

						// are tiles to the south empty? Then we'll need a below-tile.
						if (coords.y % 2 == 0) // even		
						{
							sw = tilemapRivers.GetTile(coords + Vector3Int.down + Vector3Int.left);
							se = tilemapRivers.GetTile(coords + Vector3Int.down);
						}
						else // odd
						{
							sw = tilemapRivers.GetTile(coords + Vector3Int.down);
							se = tilemapRivers.GetTile(coords + Vector3Int.down + Vector3Int.right);
						}

						TileBase west = GetTileWaterOrLand(coords + Vector3Int.left);
						TileBase east = GetTileWaterOrLand(coords + Vector3Int.right);

						if (se == null || sw == null)
						{
							if (WaterNameCheck(tile.name))
							{
								// Check if we need to use coastal under-tiles.
								if (west != null && east != null && !WaterNameCheck(west.name) && !WaterNameCheck(east.name))
								{
									tilemapUnder.SetTile(coords, below_waterCoastBoth);
								}
								else if (west != null && !WaterNameCheck(west.name))
								{
									tilemapUnder.SetTile(coords, below_waterCoastWest);
								}
								else if (east != null && !WaterNameCheck(east.name))
								{
									tilemapUnder.SetTile(coords, below_waterCoastEast);
								}
								else
								{
									tilemapUnder.SetTile(coords, below_water);
								}
							}
							// "Void" is a terrain type few people seem to use.
							// If you want to allow for it, here's where you'd do it.
							//else if (tile.name.Contains("void"))
							//{
							//	// set below to below_void00
							//}
							else
							{
								tilemapUnder.SetTile(coords, below_dirt);
							}
						}
					}
				}
			}
		}

		private void CheckAllTilesForOverlaySprites()
		{
			// First, clean out the old sprites if they exist.
			if (overlaySprites != null) if (overlaySprites.Count > 0) foreach (GameObject go in overlaySprites) Destroy(go);
			overlaySprites = new List<GameObject>();

			TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

			for (int x = 0; x < bounds.size.x; x++)
			{
				for (int y = 0; y < bounds.size.y; y++)
				{
					TileBase tile = allTiles[x + y * bounds.size.x];
					bool makeOverlay = false;

					if (tile != null)
					{
						foreach (string name in overlayWhitelist)
						{
							if (name != null && name != "")
							{
								if (Regex.IsMatch(tile.name, name + @"\d\d"))
								{
									makeOverlay = true;
									break;
								}
							}
						}
					}

					if (makeOverlay)
					{
						// To avoid unnecessary sprite creation, you could check if the tile north 
						// 1. exists
						// 2. has a sprite in it that might draw over this tile (river, road, or decor)
						// If both are true, then the overlay is needed.
						// ... but I'll leave that up to you.

						MakeOverlaySpriteForTileAtPosition(
							new Vector3Int(x + bounds.position.x, y + bounds.position.y, bounds.position.z),
							tilemap,
							tile.name);
					}
				}
			}
		}

		// draw sprite for cropped tile.
		private void MakeOverlaySpriteForTileAtPosition(Vector3Int position, Tilemap targetTileMap, string name)
		{
			GameObject parentGO = new GameObject("overlayParent " + name + " - " + position.ToString());
			parentGO.transform.parent = transform;

			SpriteMask parentSM = parentGO.AddComponent<SpriteMask>();
			parentSM.sprite = spriteSliceMask;

			GameObject spriteGO = new GameObject("overlaySprite " + name + " - " + position.ToString());
			spriteGO.transform.parent = parentGO.transform;

			SpriteRenderer spriteSR = spriteGO.AddComponent<SpriteRenderer>();
			spriteSR.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
			spriteSR.sortingLayerName = "Decor";
			spriteSR.sprite = targetTileMap.GetSprite(position);
			spriteSR.sortingOrder = 256 - position.y; // Make sure it draws over any other decor.

			overlaySprites.Add(parentGO);

			Vector3 worldPosition = targetTileMap.layoutGrid.CellToWorld(position);

			parentGO.transform.position = new Vector3(
				worldPosition.x,// + (targetTileMap.layoutGrid.cellSize.y * 0.5f),
				worldPosition.y + (targetTileMap.layoutGrid.cellSize.y * 0.5f),
				worldPosition.z);

			spriteGO.transform.position = new Vector3(
				worldPosition.x,// + (targetTileMap.layoutGrid.cellSize.y * 0.5f),
				worldPosition.y - (targetTileMap.layoutGrid.cellSize.y * 0.5f),
				worldPosition.z);
		}
	}
}
