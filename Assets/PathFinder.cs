using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour {

    private Grid grid = new Grid();

    Dictionary<string, List<Tile>> occupiedTiles = new Dictionary<string, List<Tile>>();
    Dictionary<string, Tile> targetTiles = new Dictionary<string, Tile>();

    public Vector3 GetNextGrid(string barrelId, Vector3 position, Vector3 target)
    {

        var currentTile = grid.getTile(position);

        var targetTile = grid.getTile(target);


        if (!occupiedTiles.ContainsKey(barrelId))
        {
            occupiedTiles[barrelId] = new List<Tile>();
        }

        if (targetTile.xIndex == currentTile.xIndex)
        {
            bool zDirectionIsPositive = currentTile.zIndex < targetTile.zIndex;
            
            var nextZ = zDirectionIsPositive ? currentTile.zIndex + 1 : currentTile.zIndex - 1;

            var firstCandidate = grid.getTile(currentTile.xIndex, nextZ);

            return firstCandidate.midPoint;
        } else
        {
            bool xDirectionIsPositive = currentTile.xIndex < targetTile.xIndex;

            var nextX = xDirectionIsPositive ? currentTile.xIndex + 1 : currentTile.xIndex - 1;
            
            var firstCandidate = grid.getTile(nextX, currentTile.zIndex);

            return firstCandidate.midPoint;
        }


    }



    public class Grid
    {

        public float maxX = 4.5f;
        public float minX = -4.5f;

        public float maxZ = 9.5f;
        public float minZ = -9.5f;

        public float barrelXLength = 1f;
        public float barrelZLength = 1f;
        List<List<Tile>> tiles;

        public Grid()
        {
            tiles = new List<List<Tile>>();
            var zIndexMax = (maxZ - minZ) / barrelZLength;
            var xIndexMax = (maxX - minX) / barrelXLength;
            for (var xIndex = 0; xIndex < xIndexMax + 1; xIndex++)
            {
                var xPoint = minX + xIndex * barrelXLength;
                var currentList = new List<Tile>();
                tiles.Add(currentList);

                for (var zIndex = 0; zIndex < zIndexMax + 1; zIndex++)
                {
                    var zPoint = minZ + zIndex * barrelZLength;
                    var tile = new Tile()
                    {
                        midPoint = new Vector3(xPoint, 1, zPoint),
                        xIndex = xIndex,
                        zIndex = zIndex,
                        occupiedBy = null
                    };
                    currentList.Add(tile);
                }
            }
        }

        public Tile getTile(int x, int z)
        {
            x = x < 0 ? 0 : x;
            z = z < 0 ? 0 : z;

            x = x > 9 ? 9 : x;
            z = z > 18 ? 18 : z;
            try
            {
                return tiles[x][z];
            }
            catch (Exception e)
            {
                print("FAILINPUT: X: " + x + " Z:" + z);
                print(JsonUtility.ToJson(tiles));
                throw e;
            }
            
        }

        public Tile getTile(Vector3 pos)
        {
            int xIndex = (int)System.Math.Round(pos.x - minX, 0);
            int zIndex = (int)System.Math.Round(pos.z - minZ, 0);

            xIndex = xIndex < 0 ? 0 : xIndex;
            zIndex = zIndex < 0 ? 0 : zIndex;

            xIndex = xIndex > 9 ? 9 : xIndex;
            zIndex = zIndex > 18 ? 18 : zIndex;

            try
            {
                return tiles[xIndex][zIndex];
            }
            catch (Exception e)
            {
                print("FAILINPUT: X: " + xIndex + " Z:" + zIndex);
                print(JsonUtility.ToJson(tiles));
                throw e;
            }
        }
    }

    internal Vector3 AllocateEnd(string id, Vector3 target)
    {
        var tile = grid.getTile(target);
        if (id == tile.reservedAsTargetBy) return tile.midPoint;

        if (targetTiles.ContainsKey(id))
        {
            targetTiles[id].occupiedBy = null;
            targetTiles.Remove(id);
        }
        

        if (tile.reservedAsTargetBy != null && tile.reservedAsTargetBy != id)
        {
            var newTile = grid.getTile(tile.xIndex, tile.zIndex + 1);
            if (newTile.reservedAsTargetBy != null && tile.reservedAsTargetBy != id)
            {
                newTile = grid.getTile(tile.xIndex, tile.zIndex - 1);
            }
            if (newTile.reservedAsTargetBy != null && tile.reservedAsTargetBy != id)
            {
                newTile = grid.getTile(tile.xIndex + 1, tile.zIndex);
            }
            if (newTile.reservedAsTargetBy != null && tile.reservedAsTargetBy != id)
            {
                newTile = grid.getTile(tile.xIndex - 1, tile.zIndex);
            }

            tile = newTile;
        }
        tile.reservedAsTargetBy = id;
        return tile.midPoint;
    }

    public class Tile
    {
        public Vector3 midPoint;
        public string occupiedBy;
        public int xIndex;
        public int zIndex;
        public string reservedAsTargetBy;
    }

}
