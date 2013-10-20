using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Game
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




}
