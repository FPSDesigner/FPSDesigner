using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Engine.Game
{
    class CWeapon
    {
        private int _weaponsAmount;
        private double _lastShotMs;
        public int _selectedWeapon;
        private bool _dryFirePlayed;

        public WeaponData[] _weaponsArray;

        #region "WeaponData Class"
        public class WeaponData
        {
            #region "Constructor"
            public WeaponData(Model weaponModel, object[] weaponInfo, string[] weaponsSound, string[] weapAnim, float[] animVelocity, Texture2D weapTexture)
            {
                // Model Assignement
                this._wepModel = weaponModel;

                // Integers Assignement
                this._wepType = (int)weaponInfo[0];
                this._actualClip = (int)weaponInfo[1];
                this._maxClip = (int)weaponInfo[2];
                this._bulletsAvailable = (int)weaponInfo[3];
                this._magazinesAvailables = (int)weaponInfo[4];
                this._isAutomatic = (bool)weaponInfo[5];
                this._shotPerSeconds = (int)(1000.0f / (float)weaponInfo[6]);
                this._range = (int)weaponInfo[7];

                this._rotation = (Matrix)weaponInfo[8];
                this._offset = (Vector3)weaponInfo[9];
                this._scale = (float)weaponInfo[10];

                this._delay = (float)weaponInfo[11];

                this._animVelocity = animVelocity;

                this._weapTexture = weapTexture;

                // SoundEffect Assignement
                this._shotSound = weaponsSound[0];
                if (_wepType != 2)
                {
                    this._dryShotSound = weaponsSound[1];
                    this._reloadSound = weaponsSound[2];
                }

                //Anim
                this._weapAnim = weapAnim;
            }
            #endregion

            /* WepType:
             * 0: HandGun
             * 1: Static weapons (tripod, mortars, etc.)
             * 2: Knife
             */
            public int _wepType;

            // Clips & Magazines
            public int _actualClip; // Bullets available in one magazine
            public int _maxClip; //Max bullets per magazine
            public int _bulletsAvailable; // Bullets left
            public int _magazinesAvailables;

            // Other Weapons Infos
            public bool _isAutomatic;
            public int _shotPerSeconds; // 0 if non automatic
            public int _range; // 0 if unlimited range
            public float _delay; // the delay used to play the sound

            // Models
            public Model _wepModel;

            // The baked texture
            public Texture2D _weapTexture;

            // Anim
            public String[] _weapAnim;

            // Velocity Anim
            public float[] _animVelocity;

            // Sounds
            public string _shotSound;
            public string _dryShotSound;
            public string _reloadSound;

            // Display
            public Matrix _rotation;
            public Vector3 _offset;
            public float _scale;

        }
        #endregion

        public CWeapon()
        {

        }

        public void LoadContent(ContentManager content, Model[] modelsList, Texture2D[] weapTexture, object[][] weaponsInfo, string[][] weaponsSounds, string[][] weapAnim,
            float[][] animVelocity)
        {
            if ((modelsList.Length != weaponsInfo.Length || modelsList.Length != weaponsSounds.Length
                ) && weapAnim.Length != modelsList.Length)
                throw new Exception("Weapons Loading Error - Arrays of different lengths");

            _weaponsAmount = modelsList.Length;
            _weaponsArray = new WeaponData[_weaponsAmount];

            // Initializing sounds
            for (int i = 0; i < weaponsSounds.Length; i++)
                for (int x = 0; x < weaponsSounds[i].Length; x++)
                    CSoundManager.AddSound("WEP." + weaponsSounds[i][x], content.Load<SoundEffect>(weaponsSounds[i][x]), (bool)weaponsInfo[i][5], (float)weaponsInfo[i][11]);

            for (int i = 0; i < _weaponsAmount; i++)
            {
                _weaponsArray[i] = new WeaponData(modelsList[i], weaponsInfo[i], weaponsSounds[i], weapAnim[i], animVelocity[i], weapTexture[i]);
            }
        }

        public void ChangeWeapon(int newWeapon)
        {
            _selectedWeapon = newWeapon;
        }

        public void Shot(bool firstShot, bool isCutAnimPlaying, GameTime gameTime)
        {
            if (_weaponsArray[_selectedWeapon]._wepType != 2)
            {
                if (firstShot)
                    _dryFirePlayed = false;
                if (firstShot && !_weaponsArray[_selectedWeapon]._isAutomatic)
                    InternFire();
                else if (_weaponsArray[_selectedWeapon]._isAutomatic)
                {

                    if (gameTime.TotalGameTime.TotalMilliseconds - _lastShotMs >= _weaponsArray[_selectedWeapon]._shotPerSeconds)
                    {
                        InternFire();
                        _lastShotMs = gameTime.TotalGameTime.TotalMilliseconds;
                    }

                }
            }
            else
            {

                if (isCutAnimPlaying)
                {
                    CSoundManager.PlayInstance("WEP." + _weaponsArray[_selectedWeapon]._shotSound);
                }
            }
        }

        private void InternFire()
        {
            if (_weaponsArray[_selectedWeapon]._actualClip > 0)
            {
                _weaponsArray[_selectedWeapon]._actualClip--;
                CSoundManager.PlaySound("WEP." + _weaponsArray[_selectedWeapon]._shotSound);
            }
            else
            {
                if (!_dryFirePlayed)
                {
                    CSoundManager.PlaySound("WEP." + _weaponsArray[_selectedWeapon]._dryShotSound);
                    _dryFirePlayed = true;
                }
            }
        }

        public bool Reloading()
        {
            bool isRealoadingDone = false;
            if (_weaponsArray[_selectedWeapon]._wepType != 2)
            {
                if (_weaponsArray[_selectedWeapon]._actualClip != _weaponsArray[_selectedWeapon]._maxClip)
                {
                    // If he has bullets available
                    if (_weaponsArray[_selectedWeapon]._bulletsAvailable >=
                            (_weaponsArray[_selectedWeapon]._maxClip - _weaponsArray[_selectedWeapon]._actualClip))
                    {
                        _weaponsArray[_selectedWeapon]._bulletsAvailable -= (_weaponsArray[_selectedWeapon]._maxClip - _weaponsArray[_selectedWeapon]._actualClip);
                        _weaponsArray[_selectedWeapon]._actualClip = _weaponsArray[_selectedWeapon]._maxClip;
                        isRealoadingDone = true;
                    }

                    else if (_weaponsArray[_selectedWeapon]._bulletsAvailable > 0
                        && (_weaponsArray[_selectedWeapon]._actualClip + _weaponsArray[_selectedWeapon]._bulletsAvailable) <= _weaponsArray[_selectedWeapon]._maxClip)
                    {
                        _weaponsArray[_selectedWeapon]._actualClip += _weaponsArray[_selectedWeapon]._bulletsAvailable;
                    }
                }

                //Console.WriteLine(" Bullet avaible : " + _weaponsArray[_selectedWeapon]._bulletsAvailable + " \n ActualClip : " + _weaponsArray[_selectedWeapon]._actualClip);
            }

            return isRealoadingDone;
        }


    }
}
