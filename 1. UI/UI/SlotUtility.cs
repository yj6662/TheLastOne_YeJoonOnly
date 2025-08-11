using _1.Scripts.Weapon.Scripts.Common;
using _1.Scripts.Weapon.Scripts.Grenade;
using _1.Scripts.Weapon.Scripts.Guns;
using _1.Scripts.Weapon.Scripts.Hack;
using _1.Scripts.Weapon.Scripts.Melee;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace _1.Scripts.Util
{
    /// <summary>
    /// UI 인벤토리나 무기 슬롯, 무기 개조 등에 쓰일 슬롯 헬퍼
    /// </summary>
    /// 
    public struct WeaponStatView
    {
        public int Damage;
        public float Rpm;
        public float Recoil;
        public float Weight;
        public int MaxAmmoCountInMagazine;

        public WeaponStatView(int damage, float rpm, float recoil, float weight, int maxAmmo)
        {
            Damage = damage;
            Rpm = rpm;
            Recoil = recoil;
            Weight = weight;
            MaxAmmoCountInMagazine = maxAmmo;
        }
    }

    public static class SlotUtility
    {
        public static bool IsMatchSlot(BaseWeapon weapon, WeaponType weaponType)
        {
            if (!weapon) return false;

            switch (weaponType)
            {
                case WeaponType.Rifle:
                    return weapon is Gun g1 && g1.GunData.GunStat.Type == WeaponType.Rifle;
                case WeaponType.Pistol:
                    return weapon is Gun g2 && g2.GunData.GunStat.Type == WeaponType.Pistol;
                case WeaponType.GrenadeLauncher:
                    return weapon is GrenadeLauncher;
                case WeaponType.HackGun:
                    return weapon is HackGun;
                default:
                    return false;
            }
        }
        
        public static bool TryGetWeaponType(BaseWeapon weapon, out WeaponType weaponType)
        {
            if (weapon is Punch)
            {
                weaponType = default;
                return false;
            }
            if (weapon is Gun g)
            {
                weaponType = g.GunData.GunStat.Type;
                return true;
            }
            if (weapon is GrenadeLauncher)
            {
                weaponType = WeaponType.GrenadeLauncher;
                return true;
            }
            if (weapon is HackGun)
            {
                weaponType = WeaponType.HackGun;
                return true;
            }
            weaponType = default;
            return false;
        }
        public static void GetWeaponName(BaseWeapon weapon, TextMeshProUGUI nameText)
        {
            if (!TryGetWeaponType(weapon, out var type)) { nameText.text = ""; return; }
            var localized = new LocalizedString("New Table", $"{type}_Title");
            localized.StringChanged += val => nameText.text = val;
        }
        
        public static (int mag, int total) GetWeaponAmmo(BaseWeapon weapon)
        {
            switch (weapon)
            {
                case Gun g:
                    return (g.CurrentAmmoCountInMagazine, g.CurrentAmmoCount);
                case GrenadeLauncher gl:
                    return (gl.CurrentAmmoCountInMagazine, gl.CurrentAmmoCount);
                case HackGun hg:
                    return (hg.CurrentAmmoCountInMagazine, hg.CurrentAmmoCount);
                default:
                    return (0, 0);
            }
        }
        public static WeaponStatView GetWeaponStat(BaseWeapon weapon)
        {
            switch (weapon)
            {
                case Gun g:
                {
                    var s = g.GunData.GunStat;
                    return new WeaponStatView(s.Damage, s.Rpm, s.Recoil, s.WeightPenalty, g.CurrentMaxAmmoCountInMagazine);
                }
                case GrenadeLauncher gl:
                {
                    var s = gl.GrenadeData.GrenadeStat;
                    return new WeaponStatView(s.Damage, s.Rpm, s.Recoil, s.WeightPenalty, gl.MaxAmmoCountInMagazine);
                }
                case HackGun hg:
                {
                    var s = hg.HackData.HackStat;
                    return new WeaponStatView(s.Damage, s.Rpm, s.Recoil, s.WeightPenalty, hg.CurrentMaxAmmoCountInMagazine);
                }
                default:
                    return new WeaponStatView();
            }
        }
        public static Sprite GetWeaponSprite(BaseWeapon weapon)
        {
            return weapon switch
            {
                Gun g => g.GunData.Icon,
                GrenadeLauncher gl => gl.GrenadeData.Icon,
                HackGun hg => hg.HackData.Icon,
                _ => null
            };
        }
        public static void GetWeaponDescription(BaseWeapon weapon, TextMeshProUGUI descText)
        {
            if (!TryGetWeaponType(weapon, out var type)) { descText.text = ""; return; }
            var localized = new LocalizedString("New Table", $"{type}_Description");
            localized.StringChanged += val => descText.text = val;
        }
    }
}