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
    public static class Extender {
        public static Vector3 CInverse(this Vector3 v) {
            return new Vector3(1f/v.x, 1f/v.y, 1f/v.z);
        }
    }

    [NodeType(
        Id = nameof(BonkCharacterNode),
        Title = "Bonk Character",
        Category ="Fnugus")]
    public class BonkCharacterNode : Node {  
        [DataInput]
        public  CharacterAsset ToBonk;
        [DataInput]
        public  HumanBodyBones BoneToBonk;
        [DataInput]
        public string BonkWithSource;
        private BonkingAsset.Bonkable bonkWithAsset;

        [DataInput]
        public Vector3 Position;

        //[DataInput]
        //public float YRotation;

        [DataInput]
        public BonkingAsset Bonk;

        public int multiplier = 0;

        [DataOutput]
        public int damageMultiplier() {return multiplier;} 

        

        class BonkingBehaviour : MonoBehaviour { 
            public BonkCharacterNode Node = null; 
            private bool collided = false;

            void OnCollisionEnter(Collision collision) {
                if (!collided) {
                    Object.Destroy( gameObject.GetComponent<Rigidbody>());
                    Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!BONK!!!!");
                    collided = true;
                    Node.multiplier += 1;

                    Transform transform = 
                        Node?.ToBonk
                            ?.Animator
                            ?.GetBoneTransform(Node.BoneToBonk);
                    Debug.Log(transform);
                    if (transform != null) {
                        Debug.Log("IM IN");
                        transform.localScale = Vector3.Scale(
                            transform.localScale,
                            Node.bonkWithAsset.Flattening
                        );
                    }
                    BonkedBehaviour bonked = Node?.ToBonk?.GameObject?.GetComponent<BonkedBehaviour>();
                    if (bonked != null) {
                        bonked.TimeUntilInflate = Node.bonkWithAsset.FlattenTime;
                        bonked.Run();
                    }

                    Node.InvokeFlow(nameof(Node.OnHit));
                }
            }
        }

        class BonkedBehaviour : MonoBehaviour {
            public BonkCharacterNode Node = null; 
            public int multiplier = 0;
            public Transform BoneToBeBonked = null;
            public Vector3 InitialScale = Vector3.one;
            private bool notInflated = true;
            public float TimeUntilInflate = 0;
            private bool isRunning = false;

            private IEnumerator ToRun() {
                Debug.Log("RUNNING BONKED");
                if (Node != null) {
                    var e = Easing.GetDelegate(Node.bonkWithAsset.InflateEasing);

                    while (notInflated) {
                        Debug.Log("NOT INFLATED!");
                        while(TimeUntilInflate > 0) {
                            Debug.Log("awaiting inflate in");
                            Debug.Log(TimeUntilInflate);
                            TimeUntilInflate -= Time.deltaTime;
                            yield return null;
                        }
                        notInflated = false;

                        for (float t = 0f; t < 1f; t += Time.deltaTime / Node?.bonkWithAsset?.InflateTime??1) {
                            Debug.Log("inflating:");
                            Debug.Log(t);
                            if (TimeUntilInflate>0) {
                                notInflated = true;
                                break;
                            }
                            BoneToBeBonked.localScale = (1-e(t))*Node.bonkWithAsset.Flattening + e(t)*InitialScale;
                            yield return null;
                        }
                        Debug.Log("done inflating, time untial inflate is");
                        Debug.Log(TimeUntilInflate);
                    }
                    Node.multiplier = 0;
                }
                Object.Destroy(this);
            }

            public void Run() {
                Debug.Log("INIT RUNNING BONKED");
                if (!isRunning) {
                    isRunning = true;
                    StartCoroutine( ToRun() );
                }
            }
        }

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
            GameObject go       = Context.ResourceManager.ResolveResourceUri<GameObject>(BonkWithSource);
            Transform transform = go.GetComponent<Transform>();
            Animator animator   = ToBonk.Animator;
            Transform boneToBeBonked = animator.GetBoneTransform(BoneToBonk);

            //Make a target, the same Transform as the boneToBonk
            GameObject target             = new GameObject();
            Transform targetTransform     = target.GetComponent<Transform>();
            targetTransform.SetParent(boneToBeBonked.parent, false);
            targetTransform.localPosition = Position+Vector3.Scale(boneToBeBonked.localPosition, boneToBeBonked.localScale);

            //attach prop to new bone
            transform.SetParent(targetTransform, false); 

            //set rigidbody to prop
            Rigidbody rigidbody = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
            rigidbody.detectCollisions = false;

            //set tweener and bonkedbehaviour to the asset that is bonked        
            Tweener tweener        = ToBonk.GameObject.AddComponent<Tweener>();
            BonkedBehaviour bonked = ToBonk.GameObject.GetComponent<BonkedBehaviour>();
            if (bonked==null) {
                bonked = ToBonk.GameObject.AddComponent<BonkedBehaviour>();
                bonked.InitialScale = boneToBeBonked.localScale;
                bonked.BoneToBeBonked = boneToBeBonked;
                bonked.Node = this;
            }
            
            //add a bonkingBehaviour to the prop
            BonkingBehaviour bonk = go.AddComponent<BonkingBehaviour>();
            bonk.Node = this;

            tweener.runInSequence( 
                new IEnumerator[] {
                    Bonk.PrepareMotion(bonkWithAsset,transform,tweener),
                    tweener.Perform(() => { rigidbody.detectCollisions = true; }),
                    Bonk.BonkMotion(bonkWithAsset,transform,tweener),
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

        [FlowInput]
        public Continuation Reset() {
            Animator animator = ToBonk.Animator;
            var boneToBeBonked = animator.GetBoneTransform(BoneToBonk);
            boneToBeBonked.localScale = Vector3.one;
            return null;
        }

        [FlowOutput]
        public Continuation Exit;

        [FlowOutput]
        public Continuation OnPrepared;

        [FlowOutput]
        public Continuation OnHit;
    }
}