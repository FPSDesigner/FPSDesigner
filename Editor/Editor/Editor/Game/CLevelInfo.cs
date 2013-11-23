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
        public GameFiles GameFiles { get; set; }
        public GameMenu GameMenu { get; set; }
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

    #region "Node - Menu"

    // 2D Menu
    public class GameMenu
    {
        public string Type { get; set; }
        public string BackgroundMusic { get; set; }
        public string SelectionSound { get; set; }
        public string BGImageFile { get; set; }
        public string CursorFile { get; set; }
        public int CursorClickX { get; set; }
        public int CursorClickY { get; set; }

        public ButtonsInfo ButtonsInfo { get; set; }
    }


    // 3D Models - Model - Info
    public class ButtonsInfo
    {
        public string ButtonsImages { get; set; }
        public MenuButton[] MenuButton { get; set; }
    }

    public class MenuButton
    {
        public int Action { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int ImgPosX { get; set; }
        public int ImgPosY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
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
        GameMenu = new Game.LevelInfo.GameMenu
        {
            Type = "Image",
            BackgroundMusic = "sound.wav",
            BGImageFile = "Background.jpg",
            ButtonsInfo = new Game.LevelInfo.ButtonsInfo
            {
                ButtonsImages = "Images/Buttons.png",
                MenuButton = new Game.LevelInfo.MenuButton[]
                {
                    new Game.LevelInfo.MenuButton {Action = 0, PosX = 10, PosY = 20},
                    new Game.LevelInfo.MenuButton {Action = 1, PosX = 100, PosY = 200},
                }
            }
        }
    };
    */
    #endregion
}
