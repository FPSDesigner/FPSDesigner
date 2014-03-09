using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Engine.Game.LevelInfo
{
    class CLevelInfo
    {
        LevelData _levelData;
        CXMLManager _xmlManager;

        public CLevelInfo()
        {
            _xmlManager = new CXMLManager();
        }

        public LevelData loadLevelData(string levelFile)
        {
            _levelData = _xmlManager.deserializeClass<LevelData>(levelFile);
            return _levelData;
        }
    }

    public class LevelData
    {
        public Properties Properties { get; set; }
        public SpawnInfo SpawnInfo { get; set; }
        public Terrain Terrain { get; set; }
        public Water Water { get; set; }
        public MapModels MapModels { get; set; }
        public GameFiles GameFiles { get; set; }
    }

    #region "Node - SpawnInfo"
    public class SpawnInfo
    {
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public float MoveSpeed { get; set; }
        public Coordinates SpawnCoordinates { get; set; }
    }
    #endregion

    #region "Node - Properties"

    // Properties
    public class Properties
    {
        public string Author { get; set; }
        public string LastEditionDate { get; set; }
        public string LevelName { get; set; }
    }
    #endregion


    #region "Node - MapTerrain"
    public class Terrain
    {
        public bool UseTerrain { get; set; }
        public float CellSize { get; set; }
        public float Height { get; set; }
        public float TextureTiling { get; set; }
        public TerrainTextures TerrainTextures { get; set; }
    }

    public class TerrainTextures
    {
        public string HeightmapFile { get; set; }
        public string TextureFile { get; set; }
        public string RTexture { get; set; }
        public string GTexture { get; set; }
        public string BTexture { get; set; }
        public string BaseTexture { get; set; }
    }
    #endregion

    #region "Node - Water"
    public class Water
    {
        public bool UseWater { get; set; }
        public float SizeX { get; set; }
        public float SizeY { get; set; }
        public float Alpha { get; set; }
        public Coordinates Coordinates { get; set; }
    }
    #endregion


    #region "Node - MapModels"
    // 3D Models
    public class MapModels
    {
        public MapModels_Model MapModels_Model { get; set; }
    }

    // 3D Models - Model
    public class MapModels_Model
    {
        public string ModelFile { get; set; }
        public Coordinates Coordinates { get; set; }
    }
    #endregion

    #region "Node - Coordinates"
    public class Coordinates
    {
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
    }
    #endregion


    #region "Node - GameFiles"
    public class GameFiles
    {
        [XmlElement("Texture")]
        public string[] Texture { get; set; }
        [XmlElement("Texture2D")]
        public string[] Texture2D { get; set; }
        [XmlElement("Texture3D")]
        public string[] Texture3D { get; set; }
        [XmlElement("Model")]
        public string[] Model { get; set; }
    }
    #endregion

    // TODO: Add all the nodes to the example
    #region Examples
    /*
    Game.LevelInfo.LevelData dataToSerialize = new Game.LevelInfo.LevelData
    {
        Properties = new Game.LevelInfo.Properties
        {
            Author = "Author",
            levelName = "LevelName",
            lastEditionDate = "07/11/2013, 21:44"
        },
        MapTerrain = new Game.LevelInfo.MapTerrain
        {
            heightmapFile = "Folder/Heightmap.bmp",
            textureFile = "Folder/TexturesMap.bmp"
        },
        MapModels = new Game.LevelInfo.MapModels
        {
            MapModels_Model = new Game.LevelInfo.MapModels_Model
            {
                MapModels_Model_Info = new Game.LevelInfo.MapModels_Model_Info
                {
                    ModelID = "building.fbx"
                },
                MapModels_Model_Position = new Game.LevelInfo.MapModels_Model_Position
                {
                    X = 0.54f,
                    Y = 21.1f,
                    Z = 32.0f,
                }
            }
        },
        GameFiles = new Game.LevelInfo.GameFiles
        {
            Texture = new[] { "Content/Texture1.fbx", "Content/Texture2.fbx" },
            Texture2D = new[] { "Content/2D/Texture1.fbx", "Content/2D/Texture2.fbx" }
        },
    };
    */
    #endregion
}
