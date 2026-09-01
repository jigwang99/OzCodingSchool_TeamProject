using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace StudioNAP
{
    public class ExampleScript : MonoBehaviour
    {
        public List<UnitController> UnitList;
        float _timer = 0;
        public int interval = 3;
        AnimationTypeEnum _lastAni;
        public AnimationTypeEnum CurrentAni;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= interval)
            {
                _timer -= interval;
                CurrentAni++;
                print("ani: " + CurrentAni);
                if (CurrentAni > AnimationTypeEnum.Dead)
                {
                    CurrentAni = AnimationTypeEnum.Idle;
                }
            }
            if (_lastAni != CurrentAni)
            {
                _lastAni = CurrentAni;
                foreach (var unit in UnitList)
                {
                    unit.RunAnimation(CurrentAni);
                }
            }
        }
    }

}
