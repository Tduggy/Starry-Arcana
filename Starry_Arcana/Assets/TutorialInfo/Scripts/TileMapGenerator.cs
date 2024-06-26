using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TileMapGenerator : MonoBehaviour
{
    public Tilemap tilemap; // 기본 타일맵
    public Tilemap fogTilemap; // 암흑 타일맵
    public TileBase floorTile; // 바닥 타일
    public TileBase wallTile; // 벽 타일
    public TileBase fogTile; // 암흑 타일 (검은색 타일)
    public GameObject character; // 캐릭터

    // generateMap 스크립트에 접근하기 위한 참조
    public GenerateMap mapGenerator;
    // TreasurePlacer 스크립트에 접근하기 위한 참조
    public TreasurePlacer treasurePlacer;
    // StairPlacer 스크립트에 접근하기 위한 참조
    public StairPlacer stairPlacer;

    public int fogRange = 20; // 타일맵 외부에 추가할 암흑 범위
    public int revealRadius = 3; // 시야 반경

    void Start()
    {
        GenerateTilemap();
        GenerateFog();
        treasurePlacer.PlaceTreasures(4); // 보물 4개 배치
        MoveCharacter();
        stairPlacer.PlaceStairsInCenter(); // 계단 배치
        UpdateFog(); // 초기 캐릭터 위치에서 시야 갱신
    }

    void GenerateTilemap()
    {
        // generateMap 스크립트를 통해 맵 생성
        bool[,] map = mapGenerator.CreateMap();

        // 생성된 맵을 기반으로 타일맵에 타일 배치
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                // 맵의 값이 참(true)인 경우 타일을 배치
                if (map[x, y])
                {
                    // 타일맵 좌표에 타일 배치
                    tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
                else
                {
                    // 타일맵 좌표에 타일 배치
                    tilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                }
            }
        }
    }

    public void GenerateFog()
    {
        // 기존 타일맵의 범위를 가져옴
        BoundsInt bounds = tilemap.cellBounds;
        int xMin = bounds.xMin - fogRange;
        int xMax = bounds.xMax + fogRange;
        int yMin = bounds.yMin - fogRange;
        int yMax = bounds.yMax + fogRange;

        // 타일맵 전체에 암흑 타일 배치
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                fogTilemap.SetTile(tilePosition, fogTile);
                fogTilemap.SetTileFlags(tilePosition, TileFlags.None); // 타일 플래그 초기화
                fogTilemap.SetColor(tilePosition, Color.black); // 타일 색상 검은색으로 설정
            }
        }

        // 암흑 타일맵의 Z 좌표 설정
        fogTilemap.transform.position = new Vector3(fogTilemap.transform.position.x, fogTilemap.transform.position.y, -1f);
    }

    void MoveCharacter()
    {
        bool[,] map = mapGenerator.cellmap;

        List<Vector3Int> potentialPositions = new List<Vector3Int>();
        int maxNeighbours = 0;

        // 맵 전체를 탐색하여 이웃 바닥 타일이 가장 많은 위치 찾기
        for (int x = 1; x < map.GetLength(0) - 1; x++)
        {
            for (int y = 1; y < map.GetLength(1) - 1; y++)
            {
                if (!map[x, y]) // 바닥 타일인 경우
                {
                    int neighbours = CountFloorNeighbours(map, x, y);
                    if (neighbours >= 4)
                    {
                        if (neighbours > maxNeighbours)
                        {
                            maxNeighbours = neighbours;
                            potentialPositions.Clear();
                            potentialPositions.Add(new Vector3Int(x, y, 0));
                        }
                        else if (neighbours == maxNeighbours)
                        {
                            potentialPositions.Add(new Vector3Int(x, y, 0));
                        }
                    }
                }
            }
        }

        // 가장 많은 이웃 바닥 타일을 가진 위치들 중 하나를 랜덤하게 선택하여 캐릭터 배치
        if (potentialPositions.Count > 0)
        {
            Vector3Int chosenPosition = potentialPositions[Random.Range(0, potentialPositions.Count)];
            Vector3 worldPosition = tilemap.CellToWorld(chosenPosition) + new Vector3(tilemap.cellSize.x / 2, tilemap.cellSize.y / 2, 0);
            character.transform.position = new Vector3(worldPosition.x, worldPosition.y, -1.0f); // 타일의 중심으로 이동
            UpdateFog(); // 캐릭터 위치 기준으로 시야 갱신
        }
    }

    public void UpdateFog()
    {
        Vector3Int characterPosition = tilemap.WorldToCell(character.transform.position);

        // 시야 범위 내 타일의 암흑 타일 제거
        for (int x = -revealRadius; x <= revealRadius; x++)
        {
            for (int y = -revealRadius; y <= revealRadius; y++)
            {
                Vector3Int tilePosition = new Vector3Int(characterPosition.x + x, characterPosition.y + y, 0);
                fogTilemap.SetTile(tilePosition, null); // 암흑 타일 제거
            }
        }
    }

    int CountFloorNeighbours(bool[,] map, int x, int y)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; // 현재 타일은 제외
                int nx = x + i;
                int ny = y + j;
                if (nx >= 0 && ny >= 0 && nx < map.GetLength(0) && ny < map.GetLength(1))
                {
                    if (!map[nx, ny]) count++;
                }
            }
        }
        return count;
    }
}
