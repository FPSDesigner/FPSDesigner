﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Editor.Game
{
    class CWeapon
    {
        private int _weaponsAmount;
        private double _lastShotMs;
        public int _selectedWeapon;
        private bool _dryFirePlayed;

        private WeaponData[] _weaponsArray;
        private Dictionary<string, SoundEffect> _weaponsSounds;

        #region "WeaponData Class"
        class WeaponData
        {
            #region "Constructor"
            public WeaponData(Model weaponModel, object[] weaponInfo, string[] weaponsSound, string[] weapAnim)
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
             * 0: Hands weapons
             * 1: Static weapons (tripod, mortars, etc.)
             */
            public int _wepType;

            // Clips & Magazines
            public int _actualClip;
            public int _maxClip;
            public int _bulletsAvailable; // Not including clip
            public int _magazinesAvailables;

            // Other Weapons Infos
            public bool _isAutomatic;
            public int _shotPerSeconds; // 0 if non automatic
            public int _range; // 0 if unlimited range

            // Models
            public Model _wepModel;


            //Anim
            public String[] _weapAnim;
            // Sounds
            public string _shotSound;
            public string _dryShotSound;
            public string _reloadSound;

        }
        #endregion

        public CWeapon()
        {

        }

        public void LoadContent(ContentManager content, Model[] modelsList, object[][] weaponsInfo, string[][] weaponsSounds, string[][] weapAnim)
        {
            if ((modelsList.Length != weaponsInfo.Length || modelsList.Length != weaponsSounds.Length
                )&& weapAnim.Length != modelsList.Length)
                throw new Exception("Weapons Loading Error - Arrays of different lengths");

            _weaponsAmount = modelsList.Length;
            _weaponsArray = new WeaponData[_weaponsAmount];

            // Initializing sounds
            _weaponsSounds = new Dictionary<string, SoundEffect>();
            for (int i = 0; i < weaponsSounds.Length; i++)
            {
                for (int x = 0; x < weaponsSounds[i].Length; x++)
                {
                    if (!_weaponsSounds.ContainsKey(weaponsSounds[i][x]))
                        _weaponsSounds.Add(weaponsSounds[i][x], content.Load<SoundEffect>(weaponsSounds[i][x]));
                }
            }

            for (int i = 0; i < _weaponsAmount; i++)
            {
                _weaponsArray[i] = new WeaponData(modelsList[i], weaponsInfo[i], weaponsSounds[i], weapAnim[i]);
            }
        }

        public void ChangeWeapon(int newWeapon)
        {
            _selectedWeapon = newWeapon;
        }

        public void Shot(bool firstShot, bool isCutAnimPlaying,GameTime gameTime)
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
                if(!isCutAnimPlaying)
                _weaponsSounds[_weaponsArray[_selectedWeapon]._shotSound].Play();
            }
        }

        private void InternFire()
        {
            if (_weaponsArray[_selectedWeapon]._actualClip > 0)
            {
                    _weaponsArray[_selectedWeapon]._actualClip--;
                    _weaponsSounds[_weaponsArray[_selectedWeapon]._shotSound].Play();
            }
            else
            {
                if (!_dryFirePlayed)
                {
                    _weaponsSounds[_weaponsArray[_selectedWeapon]._dryShotSound].Play();
                    _dryFirePlayed = true;
                }
            }
        }

        public string GetAnims(int weaponSelected, int animNumber)
        {
            return _weaponsArray[weaponSelected]._weapAnim[animNumber];
        }

        public Model GetModel(int modelNumber)
        {
            return _weaponsArray[modelNumber]._wepModel;
        }
        
    }
}
