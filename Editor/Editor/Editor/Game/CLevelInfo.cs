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


namespace Editor.Game.LevelInfo
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
        public MapTerrain MapTerrain { get; set; }
        public MapModels MapModels { get; set; }
        public ContentFiles ContentFiles { get; set; }
    }



    #region "Node - Properties"

    // Properties
    public class Properties
    {
        public string Author { get; set; }
        public string lastEditionDate { get; set; }
        public string levelName { get; set; }
    }
    #endregion


    #region "Node - MapTerrain"

    // Terrain
    public class MapTerrain
    {
        public string heightmapFile { get; set; }
        public string textureFile { get; set; }
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
        public MapModels_Model_Info MapModels_Model_Info { get; set; }
        public MapModels_Model_Position MapModels_Model_Position { get; set; }
    }

    // 3D Models - Model - Info
    public class MapModels_Model_Info
    {
        public string ModelID { get; set; }
    }

    // 3D Models - Model - Position
    public class MapModels_Model_Position
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
    #endregion


    #region "Node - ContentFiles"

    // Content Files to load
    public class ContentFiles
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


    #region Examples
    /*Game.LevelInfo.LevelData dataToSerialize = new Game.LevelInfo.LevelData
    {
        Properties = new Game.LevelInfo.Properties
        {
            Author = "Author",
            levelName = "LevelName"
        },
        ContentFiles = new Game.LevelInfo.ContentFiles
        {
            Texture = new[] { "Content/Texture1.fbx", "Content/Texture2.fbx" },
            Texture2D = new[] { "Content/2D/Texture1.fbx", "Content/2D/Texture2.fbx" }
        }
    };
    */
    #endregion
}
