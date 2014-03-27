using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using LTreesLibrary.Trees;

namespace Engine.Display3D
{
    class TreeManager
    {
        public static Dictionary<string, TreeProfile> _tProfiles = new Dictionary<string, TreeProfile>();
        public static List<Tree> _tTrees = new List<Tree>();

        public static void LoadTreeProfile(string profileName, TreeProfile profile)
        {
            _tProfiles.Add(profileName, profile);
        }

        public static void LoadXMLTrees(CCamera cam, ContentManager content, List<Game.LevelInfo.MapModels_Tree> trees)
        {
            foreach (Game.LevelInfo.MapModels_Tree tree in trees)
            {
                if (!_tProfiles.ContainsKey(tree.Profile))
                    _tProfiles.Add(tree.Profile, content.Load<TreeProfile>(tree.Profile));

                _tTrees.Add(new Tree(cam, tree.Profile, tree.Position.Vector3, tree.Rotation.Vector3, tree.Scale.Vector3, tree.Seed));
            }
        }

        public static void Draw(CCamera cam, GameTime gameTime)
        {
            for (int i = 0; i < _tTrees.Count; i++)
                _tTrees[i]._tree.DrawTrunk(_tTrees[i]._worldMatrix, cam._view, cam._projection);

            // We draw leaves at the end
            for (int i = 0; i < _tTrees.Count; i++)
                _tTrees[i]._tree.DrawLeaves(_tTrees[i]._worldMatrix, cam._view, cam._projection);
        }
    }

    class Tree
    {
        public string _profile;
        public Matrix _worldMatrix;
        public SimpleTree _tree;

        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _scale;

        public Vector3 Position
        {
            get { return _position; }
            set { GenerateWorldMatrix(); _position = value; }
        }
        public Vector3 Rotation
        {
            get { return _rotation; }
            set { GenerateWorldMatrix(); _position = value; }
        }
        public Vector3 Scale
        {
            get { return _scale; }
            set { GenerateWorldMatrix(); _position = value; }
        }

        public Tree(CCamera cam, string profile, Vector3 coordinates, Vector3 rotation, Vector3 scale, int seed = 0)
        {
            _profile = profile;
            _position = coordinates;
            _rotation = rotation;
            _scale = scale;

            if (seed == 0)
                GenerateTree();
            else
                GenerateTree(seed);

            GenerateWorldMatrix();

            GenerateCollisions(cam);
        }

        /// <summary>
        /// Reload the world matrix according to proper position, rotation and scale
        /// </summary>
        public void GenerateWorldMatrix()
        {
            _worldMatrix =
               Matrix.CreateScale(_scale) *
               Matrix.CreateRotationX(_rotation.X) * Matrix.CreateRotationY(_rotation.Y) * Matrix.CreateRotationZ(_rotation.Z) *
               Matrix.CreateTranslation(_position);
        }

        /// <summary>
        /// Generates collision box from the base trunk
        /// </summary>
        public void GenerateCollisions(CCamera cam)
        {
            Matrix[] transforms = new Matrix[_tree.Skeleton.Bones.Count];
            _tree.Skeleton.CopyAbsoluteBoneTranformsTo(transforms, _tree.AnimationState.BoneRotations);

            BoundingSphere b1 = new BoundingSphere(_position + transforms[0].Translation, _tree.Skeleton.Branches[0].StartRadius * _scale.X);
            BoundingSphere b2 = new BoundingSphere(_position + transforms[1].Translation + transforms[1].Up * _tree.Skeleton.Bones[1].Length * _scale.X, _tree.Skeleton.Branches[1].EndRadius * _scale.X);

            BoundingBox CollisionBox = BoundingBox.CreateMerged(BoundingBox.CreateFromSphere(b1), BoundingBox.CreateFromSphere(b2));

            Vector3[] bbox = CollisionBox.GetCorners();

            List<Triangle> triangleList = new List<Triangle>();
            List<Vector3> triangleNormal = new List<Vector3>();

            int[,] trianglesPos = 
            {
                {0, 1, 2}, {0, 3, 2},
                {4, 0, 3}, {4, 7, 3},
                {4, 7, 6}, {4, 5, 6},
                {5, 6, 2}, {5, 1, 2},
                {4, 0, 1}, {4, 5, 1},
                {7, 3, 2}, {7, 6, 2},
            };

            for (int i = 0; i < 12; i++)
            {
                Triangle tri = new Triangle(bbox[trianglesPos[i, 0]], bbox[trianglesPos[i, 1]], bbox[trianglesPos[i, 2]]);
                triangleList.Add(tri);

                // Compute normal
                Vector3 Normal = Vector3.Cross(tri.V0 - tri.V1, tri.V0 - tri.V2);
                Normal.Normalize();
                triangleNormal.Add(Normal);
            }


            cam._physicsMap._triangleList.AddRange(triangleList);
            cam._physicsMap._triangleNormalsList.AddRange(triangleNormal);
        }

        /// <summary>
        /// Generate a random tree
        /// </summary>
        public void GenerateTree()
        {
            _tree = TreeManager._tProfiles[_profile].GenerateSimpleTree();
        }

        /// <summary>
        /// Generate a tree from a seed
        /// </summary>
        /// <param name="seed">Seed id</param>
        public void GenerateTree(int seed)
        {
            _tree = TreeManager._tProfiles[_profile].GenerateSimpleTree(new Random(seed));
        }
    }
}
