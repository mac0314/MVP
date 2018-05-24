﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CellularAutomata : MonoBehaviour {

	public int width;
	public int height;

	public int seed;
	public bool useRandomSeed;
    private bool debugTest;

    [Range(0,100)]
	public int randomFillPercent;

	int[,] map; //생성된 2차원 배열맵 저장할 변수
    int[,] worldMap;
    List<Room> survivingRooms = new List<Room>();

    public static CellularAutomata Instance { get; private set; }

    const int _startHeightLevel = 2;
    public int MaxHeightLevel = 3;

    void Start() {
		//GenerateMap();
	}

    private void Awake()
    {
        Instance = this;
    }

    void Update() {
        //테스트용 코드. 왼쪽클릭시 맵 다시 생성
		//if (Input.GetMouseButtonDown(0)) {
		//	GenerateMap();
		//}
	}

	public void GenerateMap() {
		map = new int[width,height];
        worldMap = new int[width, height];
        ProcessMap();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                worldMap[x, y] = map[x, y] == 0? 1 : 0;
            }
        }
        
        for(int i = _startHeightLevel; i <= MaxHeightLevel; i++) { 
            makeDepth(i);
        }

        if (RandomMapGenerator.Instance)
        {
            RandomMapGenerator.Instance.GenerateMapByAlgorithm(worldMap, width, height);
        }

        //test
    }

    void ProcessMap()
    {
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        //최소 룸사이즈보다 작으면 버림
        List<List<Coord>> roomRegions = GetRegions(0);

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < RandomMapGenerator.Instance.roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true; // 가장 큰 방 메인룸
        survivingRooms[0].isAccessibleFromMainRoom = true;
        ConnectClosestRooms(survivingRooms);
    }

    void ProcessMapByDepth()
    {
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }
        
        List<List<Coord>> roomRegions = GetRegions(0);
        List<Room> survivingRoomsByDepth = new List<Room>();
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < RandomMapGenerator.Instance.roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRoomsByDepth.Add(new Room(roomRegion, map));
            }
        }
        survivingRoomsByDepth.Sort();
        survivingRoomsByDepth[0].isMainRoom = true; 
        survivingRoomsByDepth[0].isAccessibleFromMainRoom = true;
        ConnectClosestRooms(survivingRoomsByDepth);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> roomListA = new List<Room>(); // 메인룸에 연결안된 방
        List<Room> roomListB = new List<Room>(); // 메인룸에 연결된 방

        if (forceAccessibilityFromMainRoom) // 메인룸에 모든길이 연결
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true); // 아직도 메인에 연결안되면 
        }
    }

    //룸과 룸사이 연결선을 그림
    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        debugTest = RandomMapGenerator.Instance.debugTest;
        if (debugTest)
        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 5);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, -height / 2 + .5f + tile.tileY, -10);
    }

    List<Coord> GetLine(Coord from, Coord to) // 라인을 그리는 함수
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<List<Coord>> GetRegionsInWorldMap(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && worldMap[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }


    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }


    void RandomFillMap() {
        int newSeed;
        useRandomSeed = RandomMapGenerator.Instance.useRandomSeed;
        do
        {
            newSeed = UnityEngine.Random.Range(0,100);
        } while (seed == newSeed);
        seed = newSeed;

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				if (x == 0 || x == width-1 || y == 0 || y == height -1) {
					map[x,y] = 1; // 테두리는 1로 채운다
				}
				else {
					map[x,y] = (pseudoRandom.Next(0,100) < randomFillPercent)? 1: 0;
				}
			}
		}
	}

	void SmoothMap() {
		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				int neighbourWallTiles = GetSurroundingWallCount(x,y);

				if (neighbourWallTiles > 4)
					map[x,y] = 1;
				else if (neighbourWallTiles < 4)
					map[x,y] = 0;

			}
		}
	}

	int GetSurroundingWallCount(int gridX, int gridY) {
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX ++) {
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY ++) {
				if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height) {
					if (neighbourX != gridX || neighbourY != gridY) {
						wallCount += map[neighbourX,neighbourY];
					}
				}
				else {
					wallCount ++;
				}
			}
		}

		return wallCount;
	}

    void makeDepth(int height)
    {
        ProcessMapByDepth();
        /*for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x,y] + worldMap[x,y] == depth)
                {
                    worldMap[x, y] = depth;
                    
                }
            }
        }*/

        foreach (Room room in survivingRooms)
        {
            List<Coord> tilesDepth1 = new List<Coord>();
            List<Coord> tilesDepth2 = new List<Coord>();
            List<Coord> tilesDepth3 = new List<Coord>();

            for (int i = 0; i < room.roomSize; i++)
            {
                if (map[room.tiles[i].tileX, room.tiles[i].tileY] + worldMap[room.tiles[i].tileX, room.tiles[i].tileY] == height)
                {
                    worldMap[room.tiles[i].tileX, room.tiles[i].tileY] = height;
                    if (height == 2)
                    {
                        tilesDepth2.Add(room.tiles[i]);
                    }
                    if (height == 3)
                    {
                        room.tilesDepth2.Remove(room.tiles[i]);
                        tilesDepth3.Add(room.tiles[i]);
                    }
                } else
                {
                    if (height == 2)
                        tilesDepth1.Add(room.tiles[i]);
                }
            }

            if (height == 2)
            {
                room.tilesDepth1 = tilesDepth1;
                room.tilesDepth2 = tilesDepth2;
            }

            if (height == 3)
            {
                room.tilesDepth3 = tilesDepth3;
            }


        }

    }

    int GetRandomValue(int max) {
        return StageGenerator.Instance.ReadNextValue(max);
    }

    public List<Coord> GetRoomsCoord(int height, int num)
    {
        List<Coord> RoomsCoords = new List<Coord>();
        Room currentRoom;
        int currentPoint = 0;
        int MaxPoint = survivingRooms.Count;
        int randomTile = 0;
        

        for(int i=0; i< num; i++)
        {
            currentRoom = survivingRooms[currentPoint];
            switch (height)
            {
                case 1:
                    if (currentRoom.tilesDepth1.Count > 0)
                    {
                        randomTile = GetRandomValue(currentRoom.tilesDepth1.Count);
                        //Debug.Log("tiles1depth :" + currentRoom.tilesDepth1.Count);
                        //Debug.Log("randomTile :" + randomTile);
                        RoomsCoords.Add(currentRoom.tilesDepth1[randomTile]);

                        //Debug.Log("heightf :" + height);
                        //Debug.Log("tile :" + worldMap[currentRoom.tilesDepth1[randomTile].tileX, currentRoom.tilesDepth1[randomTile].tileY]);
                    }
                    else i--;

                    break;
                case 2:
                    if (currentRoom.tilesDepth2.Count > 0)
                    {
                        randomTile = GetRandomValue(currentRoom.tilesDepth2.Count);
                        //Debug.Log("tiles2depth :" + currentRoom.tilesDepth2.Count);
                        //Debug.Log("randomTile :" + randomTile);
                        RoomsCoords.Add(currentRoom.tilesDepth2[randomTile]);

                        //Debug.Log("heightf :" + height);
                        //Debug.Log("tile :" + worldMap[currentRoom.tilesDepth2[randomTile].tileX, currentRoom.tilesDepth2[randomTile].tileY]);

                    }
                    else i--;
                    break;
                case 3:
                    if (currentRoom.tilesDepth3.Count > 0)
                    {
                        randomTile = GetRandomValue(currentRoom.tilesDepth3.Count);
                        //Debug.Log("tiles3depth :" + currentRoom.tilesDepth3.Count);
                        //Debug.Log("randomTile :" + randomTile);
                        RoomsCoords.Add(currentRoom.tilesDepth3[randomTile]);

                        //Debug.Log("heightf :" + height);
                        //Debug.Log("tile :" + worldMap[currentRoom.tilesDepth3[randomTile].tileX, currentRoom.tilesDepth3[randomTile].tileY]);
                    }
                    else i--;
                    
                    break;
            }//.tileX, currentRoom.tiles[randomTile].tileY);
            //Debug.Log("randomTile :" + randomTile);
            currentPoint = currentPoint < MaxPoint - 1 ? currentPoint + 1 : 0;
        }

        return RoomsCoords;
    }

    public List<Coord> GetEnvironCoord(int height, int num)
    {
        List<Coord> RoomsCoords = new List<Coord>();
        Room currentRoom;
        int currentPoint = 0;
        int MaxPoint = survivingRooms.Count;
        int randomTile = 0;


        for (int i = 0; i < num; i++)
        {
            currentRoom = survivingRooms[currentPoint];
            switch (height)
            {
                case 1:
                    if (currentRoom.tilesDepth1.Count > 0)
                    {
                        randomTile = GetRandomValue(currentRoom.tilesDepth1.Count);
                        RoomsCoords.Add(currentRoom.tilesDepth1[randomTile]);
                        survivingRooms[currentPoint].tilesDepth1.Remove(currentRoom.tilesDepth1[randomTile]);
                    }
                    else i--;

                    break;
                case 2:
                    if (currentRoom.tilesDepth2.Count > 0)
                    {
                        randomTile = GetRandomValue(currentRoom.tilesDepth2.Count);
                        RoomsCoords.Add(currentRoom.tilesDepth2[randomTile]);
                        survivingRooms[currentPoint].tilesDepth1.Remove(currentRoom.tilesDepth2[randomTile]);
                    }
                    else i--;
                    break;
                case 3:
                    if (currentRoom.tilesDepth3.Count > 0)
                    {
                        randomTile = GetRandomValue(currentRoom.tilesDepth3.Count);
                        RoomsCoords.Add(currentRoom.tilesDepth3[randomTile]);
                        survivingRooms[currentPoint].tilesDepth1.Remove(currentRoom.tilesDepth1[randomTile]);
                    }
                    else i--;

                    break;
            }
            currentPoint = currentPoint < MaxPoint - 1 ? currentPoint + 1 : 0;
        }

        return RoomsCoords;
    }

    void OnDrawGizmos() {
        try
        {
            debugTest = RandomMapGenerator.Instance.debugTest;
            if (debugTest == false) return;
            else if (map != null)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                        Vector3 pos = new Vector3(-width / 2 + x + .5f, -height / 2 + y + .5f, 0);
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }
        }
        catch {

        }

    }

    public struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }

        public bool equalCoord(Coord coordA, Coord coordB)
        {
            if (coordA.tileX == coordB.tileX && coordA.tileY == coordB.tileY)
                return true;
            else return false;
        }
    }

    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> tilesDepth1;
        public List<Coord> tilesDepth2;
        public List<Coord> tilesDepth3;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room()
        {
        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            tilesDepth1 = null;
            tilesDepth2 = null;
            tilesDepth3 = null;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

}
