using UnityEngine;
using System.Collections;
using Warudo.Core.Attributes;
using Warudo.Core.Graphs;
using Warudo.Core.Scenes;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Character;
using Warudo.Plugins.Core.Assets.Prop;
using Warudo.Core.Data.Models;
using Animancer;


using Warudo.Plugins.Core.Assets.Utility;
using Warudo.Plugins.Core.Assets.Cinematography;

using System;
//using Cysharp.Threading.Tasks;
using Warudo.Core;
using Warudo.Core.Data;
using RootMotion.Dynamics;
using Warudo.Core.Utils;
using Warudo.Core.Localization;
using Warudo.Plugins.Core;
using Warudo.Plugins.Core.Utils;
using Warudo.Plugins.Interactions.Mixins;
using Object = UnityEngine.Object;

namespace Bonk {
    public class Tweener : MonoBehaviour {
        //Node node;

        public void run(System.Action<float> settingAction, float inTime, Easing.Function easing) {
            StartCoroutine(Tween(settingAction, inTime, easing)); 
        }

        public IEnumerator Tween(System.Action<float> settingAction, float inTime, Easing.Function easing) {
            var e = Easing.GetDelegate(easing);
            for (float t = 0f; t < 1f; t += Time.deltaTime / inTime) {
                settingAction(e(t));
                yield return null;
            }
            settingAction(e(1f));    
            yield return null;         
        }

        public IEnumerator Delay(float inTime) {
            for (float t = 0f; t < 1f; t += Time.deltaTime / inTime) {
                yield return null;
            }
        }

        public IEnumerator Perform(System.Action todo) {
            todo();
            yield return null; 
        }

        public void runInSequence(IEnumerator[] actions) {
            StartCoroutine( sequence(actions));
        }

        public IEnumerator sequence(IEnumerator[] actions) {
            foreach (IEnumerator action in actions) {
                yield return StartCoroutine(action); 
            }
        }

        public void Cycle(System.Action<float> settingAction, float inTime) {
                StartCoroutine(TweenCycle(settingAction, inTime)); 
        }

        public IEnumerator TweenCycle(System.Action<float> settingAction, float inTime) {
            while(true) {
                for (float t = 0f; t < 1f; t += Time.deltaTime / inTime) {
                    settingAction(t);
                    yield return null;
                }
            }
        }
    }
}