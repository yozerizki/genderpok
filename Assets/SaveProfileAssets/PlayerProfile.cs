using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfile
{
    public string playerName;
    public int charselected;
    public int levelactivated;
    public bool[] leveldone;
    public int nyawa;
    public int skor;
    public bool perisaion;
    public bool[] pernahtamat;
    public int[] soalke;
    public Vector2 playerstartpos;
    // FLATTENED isdone
    public List<bool> isdoneFlat;
    public int isdoneRows = 4;
    public int isdoneCols = 10;

    // FLATTENED idgameplayed
    public List<bool> idgameplayedFlat;
    public int idgameplayedRows = 4;
    public int idgameplayedCols = 10;

    public float persentaseprogres;

    public string[] jawabanMateri;

    public PlayerProfile(string name)
    {
        playerName = name;
        charselected = 0 ;
        levelactivated = 1;
        leveldone = new bool[] { false, false, false, false };
        nyawa = 3;
        skor = 0;
        perisaion = false;
        pernahtamat = new bool[4] { false, false, false, false };
        soalke = new int[4] { 0, 0, 0, 0 };
        playerstartpos = new Vector2(0,0);
        isdoneFlat = new List<bool>(new bool[isdoneRows * isdoneCols]);
        idgameplayedFlat = new List<bool>(new bool[idgameplayedRows * idgameplayedCols]);
        jawabanMateri = new string[0];
    }

    // ---------- Flatten Utilities ----------
    public void SetIsDoneFromJagged(bool[][] input)
    {
        isdoneFlat = new List<bool>();
        isdoneRows = input.Length;
        isdoneCols = input[0].Length;
        foreach (var row in input)
        {
            isdoneFlat.AddRange(row);
        }
    }

    public bool[][] GetIsDoneJagged()
    {
        bool[][] result = new bool[isdoneRows][];
        for (int i = 0; i < isdoneRows; i++)
        {
            result[i] = new bool[isdoneCols];
            for (int j = 0; j < isdoneCols; j++)
            {
                result[i][j] = isdoneFlat[i * isdoneCols + j];
            }
        }
        return result;
    }

    public void SetIdGamePlayedFromJagged(bool[][] input)
    {
        idgameplayedFlat = new List<bool>();
        idgameplayedRows = input.Length;
        idgameplayedCols = input[0].Length;
        foreach (var row in input)
        {
            idgameplayedFlat.AddRange(row);
        }
    }

    public bool[][] GetIdGamePlayedJagged()
    {
        bool[][] result = new bool[idgameplayedRows][];
        for (int i = 0; i < idgameplayedRows; i++)
        {
            result[i] = new bool[idgameplayedCols];
            for (int j = 0; j < idgameplayedCols; j++)
            {
                result[i][j] = idgameplayedFlat[i * idgameplayedCols + j];
            }
        }
        return result;
    }
}
