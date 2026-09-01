using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace StudioNAP
{
    public enum AnimationTypeEnum
    {
        Idle,
        Run,
        Attack0,
        Attack1,
        Dead
    }
    public class UnitController : MonoBehaviour
    {
        public int UnitIndex = 0;
        // Start is called before the first frame update
        void Start()
        {
            SetSword(UnityEngine.Random.Range(0, 23));
            SetShield(UnityEngine.Random.Range(0, 12));

        }
        public void RunAnimation(AnimationTypeEnum ani)
        {
            string strAni;
            if (ani == AnimationTypeEnum.Idle)
            {
                strAni = "cat0Idle";
            }
            else if (ani == AnimationTypeEnum.Attack0)
            {
                strAni = "cat0Shoot0";
            }
            else if (ani == AnimationTypeEnum.Attack1)
            {
                strAni = "cat0Shoot1";
            }
            else if (ani == AnimationTypeEnum.Dead)
            {
                strAni = "cat0Dead";
            }
            else
            {
                strAni = GetRunSpriteName(UnitIndex);
            }
            GetComponent<Animator>().Play(strAni);
        }
        public string GetRunSpriteName(int index)
        {
            if (index == 0)
            {
                return "cat0Run";
            }
            else if (index == 2 ||
                    index == 7 ||
                    index == 11 ||
                    index == 14 ||
                    index == 17 ||
                    index == 20)
            {
                return "cat1Run";
            }
            else if (index == 4 ||
                    index == 6 ||
                    index == 9 ||
                    index == 10 ||
                    index == 12 ||
                    index == 13 ||
                    index == 15 ||
                    index == 16 ||
                    index == 19)
            {
                return "cat5Run";
            }
            else
            {
                return string.Format("cat{0}Run", index);
            }
        }
        public void SetSword(int index)
        {
            print("SetSword: " + index);
            transform.Find("armL").Find("sword").GetComponent<SpriteRenderer>().sprite = GetSpriteFromMultiple("CatsAsset/Item/swords", string.Format("swords_{0}", index));
        }
        public void SetShield(int index)
        {
            print("SetShield: " + index);
            transform.Find("armR").Find("shield").GetComponent<SpriteRenderer>().sprite = GetSpriteFromMultiple("CatsAsset/Item/shields", string.Format("shields_{0}", index));
        }

        public Sprite GetSpriteFromMultiple(string path, string name)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            foreach (Sprite spt in sprites)
            {
                if (spt.name.Equals(name))
                {
                    return spt;
                }
            }
            return null;
        }
    }

}