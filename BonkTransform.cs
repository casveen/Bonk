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
    [NodeType(
        Id = nameof(BonkTransformNode), // Must be unique. Generate one at https://guidgenerator.com/
        Title = "Bonk Transform",
        Category ="Fnugus")]
    public class BonkTransformNode : Node {  
        [DataInput]
        public  Transform ToBonk;
        [DataInput]
        public string BonkWithSource;
        private BonkingAsset.Bonkable bonkWithAsset;

        [DataInput]
        public Vector3 Position = new Vector3(0f,0f,0f);

        [DataInput]
        public float SizeFactor = 1.0f;

        [DataInput]
        public BonkingAsset Bonk;

        protected override void OnCreate() {
            Watch(nameof(Bonk), () => {
                Watch(Bonk, nameof(Bonk.Bonkables), () => {
                    ResolveBonkable();
                });
                ResolveBonkable();
            });
            return;
        }

        public void ResolveBonkable() {
            if (Bonk != null)
                foreach (BonkingAsset.Bonkable bonkable in Bonk.Bonkables) {
                    if (bonkable.BonkWithSource == BonkWithSource) {
                        bonkWithAsset = bonkable; 
                    }
                }
        }


        [FlowInput]
        public Continuation Enter() {
            //Create a gameobject from a propasset
            GameObject target = new GameObject();
            GameObject go       = Context.ResourceManager.ResolveResourceUri<GameObject>(BonkWithSource);
            Transform transform = go.GetComponent<Transform>();
            //Transform boneToBeBonked = ToBonk;

            target.GetComponent<Transform>().position = Position;

            transform.SetParent(target.GetComponent<Transform>());

            //attach prop to bone
            //transform.SetParent(ToBonk, false); 

            //set tweener and bonkedbehaviour to the asset that is bonked        
            Tweener tweener = go.AddComponent<Tweener>();

            tweener.runInSequence( 
                new IEnumerator[] {
                    Bonk.PrepareMotion(bonkWithAsset,transform,tweener,SizeFactor),
                    tweener.Perform(() => {InvokeFlow(nameof(OnPrepared));}),
                    Bonk.BonkMotion(bonkWithAsset,transform,tweener,SizeFactor),
                    tweener.Perform(() => {InvokeFlow(nameof(OnHit));}),
                    tweener.Delay(bonkWithAsset.StayTime),
                    tweener.Perform(
                        () => {
                            Object.Destroy(go);
                            Object.Destroy(tweener);
                        }
                    )
                }
            );
            return Exit;
        }

        [FlowOutput]
        public Continuation Exit;

        [FlowOutput]
        public Continuation OnPrepared;

        [FlowOutput]
        public Continuation OnHit;
    }
}