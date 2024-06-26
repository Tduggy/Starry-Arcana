using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    int width = 50; // 40 + 테두리 10
    int height = 50; // 40 + 테두리 10
    public bool[,] cellmap;

    float chanceToStartAlive = 0.55f;
    int starvationLimit = 2;
    int overpopLimit = 3;
    int birthLimit = 3;
    int numberOfSteps = 4;

    // 초기 맵 설정 메서드
    public bool[,] InitialiseMap(bool[,] map)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (Random.value < chanceToStartAlive)
                {
                    map[x, y] = true;
                }
            }
        }
        return map;
    }

    // 맵에 규칙을 적용하여 재생성
    public bool[,] DoSimulationStep(bool[,] oldMap)
    {
        bool[,] newMap = new bool[width, height];
        // 각 셀에 대해 규칙을 적용해 새로운 맵을 생성
        for (int x = 0; x < oldMap.GetLength(0); x++)
        {
            for (int y = 0; y < oldMap.GetLength(1); y++)
            {
                int nbs = CountAliveNeighbours(oldMap, x, y);
                // 현재 셀이 살아있는 경우 
                if (oldMap[x, y])
                {
                    if (nbs < starvationLimit || nbs > overpopLimit)
                    {
                        newMap[x, y] = false;
                    }
                    else
                    {
                        newMap[x, y] = true;
                    }
                }
                // 현재 셀이 죽어있는 경우
                else
                {
                    if (nbs > birthLimit)
                    {
                        newMap[x, y] = true;
                    }
                    else
                    {
                        newMap[x, y] = false;
                    }
                }
            }
        }
        return newMap; // 새로운 맵 반환
    }

    // 살아있는 이웃 셀 수를 계산하는 메서드
    public int CountAliveNeighbours(bool[,] map, int x, int y)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int neighbour_x = x + i;
                int neighbour_y = y + j;
                // 중앙의 셀은 제외
                if (i == 0 && j == 0)
                {
                    continue;
                }
                // 맵의 범위를 벗어난 경우 살아있는 이웃 셀 수 증가
                else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= map.GetLength(0) || neighbour_y >= map.GetLength(1))
                {
                    count = count + 1;
                }
                // 맵 안의 이웃 셀 검사
                else if (map[neighbour_x, neighbour_y])
                {
                    count = count + 1;
                }
            }
        }
        return count;
    }
    // 테두리를 살아있는 셀로 바꾸는 메서드
    private void SetBordersAlive(bool[,] map)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x < 5 || y < 5 || x >= width - 5 || y >= height - 5)
                {
                    map[x, y] = true; // 가장 바깥 셀 5줄을 살아있는 셀로 설정
                }
            }
        }
    }

    // 맵을 생성하는 메서드
    public bool[,] CreateMap()
    {
        cellmap = new bool[width, height]; // 맵 초기화
        cellmap = InitialiseMap(cellmap); // 초기 맵 생성
        // numberOfSteps만큼 시뮬레이션 진행
        for (int i = 0; i < numberOfSteps; i++)
        {
            cellmap = DoSimulationStep(cellmap);
        }
        SetBordersAlive(cellmap); // 테두리를 살아있는 셀로 설정
        return cellmap;
    }
}