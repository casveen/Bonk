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


public static class Extender {
    public static Vector3 CInverse(this Vector3 v) {
        return new Vector3(1f/v.x, 1f/v.y, 1f/v.z);
    }
}

[NodeType(
    Id = "4dca52c8-58b8-40d7-b3e0-3bd5dd443752", // Must be unique. Generate one at https://guidgenerator.com/
    Title = "Bonk",
    Category ="Fnugus")]
public class BonkNode : Node {  
    [DataInput]
    public  CharacterAsset toBonk;
    [DataInput]
    public  HumanBodyBones boneToBonk;
    [DataInput]
    public string bonkWithSource;
    [DataInput]
    public Vector3 preparePosition = new Vector3(1,1,0);
    [DataInput]
    public Vector3 bonkPosition = new Vector3(0.5f,0.5f,0);
    [DataInput]
    public Vector3 flattening = new Vector3(1f,1f,0.1f); 
    [DataInput]
    public float startScale = 0.0f;
    [DataInput]
    public float finalScale = 1.0f;
    [DataInput]
    public float prepareTime = 1.0f;
    [DataInput]
    public float bonkTime = 0.25f;
    [DataInput]
    public float stayTime = 1.0f;
    [DataInput]
    public float flattenTime    = 8.0f;
    [DataInput]
    public float inflateTime    = 1.0f;
    [DataInput]
    public Vector3 prepareRotation = new Vector3(0,0,-60);
    [DataInput]
    public Vector3 bonkRotation    = new Vector3(0,0,90);
    [DataInput]
    public Easing.Function prepareEasing = Easing.Function.ExponentialOut;
    [DataInput]
    public Easing.Function bonkEasing    = Easing.Function.ExponentialOut;
    [DataInput]
    public Easing.Function flattenEasing = Easing.Function.Linear;
    [DataInput]
    public Easing.Function inflateEasing = Easing.Function.ElasticOut;
    [DataInput]
    public Easing.Function sizeEasing    = Easing.Function.CubicOut;
    public int multiplier = 0;

    [DataOutput]
    public int damageMultiplier() {return multiplier;} 

    

    class BonkBehaviour : MonoBehaviour {
        public BonkNode node = null; 
        public Transform bone = null;
        public GameObject bonkingWith = null;
        private bool collided = false;
        public BonkedBehaviour bonked = null;

        void OnCollisionEnter(Collision collision)
    {
        // Debug-draw all contact points and normals
        if (!collided) {
        foreach (ContactPoint contact in collision.contacts)
        {
            //Debug.DrawRay(contact.point, contact.normal, Color.white);'
            Object.Destroy( bonkingWith.GetComponent<Rigidbody>());
            collided = true;
            //var par =  bonkingWith.GetComponent<Transform>().parent;
            node.multiplier += 1;
            node.InvokeFlow(nameof(node.OnHit));
            //bonkingWith.GetComponent<Transform>().parent = null; //localScale
            bone.localScale = Vector3.Scale(bone.localScale,node.flattening); //multiplies component-wise
            Debug.Log("SETTING BONKED PARAMS");
            bonked.timeUntilInflate = node.flattenTime;
            
            Debug.Log("START BONKED");
            bonked.run();
            
            //Debug.Log(bonkingWith.GetComponent<Transform>().localScale);
            //bonkingWith.GetComponent<Transform>().localScale = Vector3.Scale(bonkingWith.GetComponent<Transform>().localScale,node.flattening.CInverse()); //multiplies component-wise
            //Debug.Log(node.flattening.CInverse());
            //bonkingWith.GetComponent<Transform>().parent = par;
            break;
        }}
    }
    }

    class BonkedBehaviour : MonoBehaviour {
        public BonkNode node = null; 
        public int multiplier = 0;
        public Transform boneToBeBonked = null;
        //public GameObject bonkingWith = null;
        public Vector3 initialScale = Vector3.one;
        //private bool collided = false;
        private bool notInflated = true;
        public float timeUntilInflate = 0;
        private bool isRunning = false;

        public IEnumerator toRun() {
            Debug.Log("RUNNING BONKED");
            var e = Easing.GetDelegate(node.inflateEasing);

            while (notInflated) {
                Debug.Log("NOT INFLATED!");
                while(timeUntilInflate > 0) {
                    Debug.Log("awaiting inflate in");
                    Debug.Log(timeUntilInflate);
                    timeUntilInflate -= Time.deltaTime;
                    yield return null;
                }
                //start inflating, but might be interrupted!
                /*tweener.Tween(
                    (e) => ,
                    node.inflateTime, 
                    node.inflateEasing
                );*/ 

                for (float t = 0f; t < 1f; t += Time.deltaTime / node.inflateTime) {
                    Debug.Log("inflating:");
                    Debug.Log(t);
                    if (timeUntilInflate>0) break;
                    boneToBeBonked.localScale = (1-e(t))*node.flattening + e(t)*initialScale;
                    yield return null;
                }
                Debug.Log("done inflating, time untial inflate is");
                Debug.Log(timeUntilInflate);
                if (timeUntilInflate<=0) notInflated = false;
            }
            node.multiplier = 0;
            Object.Destroy(this);
        }

        public void run() {
            Debug.Log("INIT RUNNING BONKED");
            if (!isRunning) {
                isRunning = true;
                StartCoroutine( toRun() );
            }
        }
    }

    class Tweener : MonoBehaviour {
        protected BonkNode node = null;
        public void run(System.Action<float> settingAction, float inTime, Easing.Function easing, string todo, BonkNode it) {
            StartCoroutine(Tween(settingAction, inTime, easing, todo, it)); 
        }

        public void run(System.Action<float> settingAction, float inTime, Easing.Function easing, BonkNode it) {
            StartCoroutine(Tween(settingAction, inTime, easing)); 
        }

         public IEnumerator Tween(System.Action<float> settingAction, float inTime, Easing.Function easing, string conttt, BonkNode it) {
            //Debug.Log("IN OUTER TWEEN");
            yield return StartCoroutine( Tween(settingAction, inTime, easing));
            //Debug.Log("FINISHED OUTER TWEEN");
            //yield return null;
            it.InvokeFlow(conttt);
            //Debug.Log("FINISHED OUTER TWEEN AND CONT");
            yield break;
         }
 
        public IEnumerator Tween(System.Action<float> settingAction, float inTime, Easing.Function easing) {
            //Debug.Log("IN INNER TWEEN");
            var e = Easing.GetDelegate(easing);

            for (float t = 0f; t < 1f; t += Time.deltaTime / inTime) {
                settingAction(e(t));
                yield return null;
            }

            //Debug.Log("FINISHED INNER TWEEN");
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
    }


    [FlowInput]
    public Continuation Enter() {
        //create a Prop asset that loads the prop mod, then use PropAsset.GameObject to get reference to the gameobject 
        var go = Context.ResourceManager.ResolveResourceUri<GameObject>(bonkWithSource); // as PropAsset;
        Transform transform = go.GetComponent<Transform>();
        Animator animator = toBonk.Animator;
        var boneToBeBonked = animator.GetBoneTransform(boneToBonk);

        //make a target, the same Transform as the boneToBonk
        //make a new target oject

        GameObject target = new GameObject();
        Transform targetTransform = target.GetComponent<Transform>();
        targetTransform.parent = boneToBeBonked.parent;
        targetTransform.localPosition = Vector3.Scale(boneToBeBonked.localPosition, boneToBeBonked.localScale);
        Debug.Log(targetTransform.localPosition);
        targetTransform.localRotation = boneToBeBonked.localRotation;

        
        
        //(animator.GetBoneTransform(boneToBonk));
        //targetTransform.DetachChildren();
        //targetTransform.parent = boneToBeBonked.parent; //attach new bone to head parent
        transform.parent = targetTransform; //attach prop to new bone
        //set rigidbody to prop
        var rigidbody = go.GetComponent<Rigidbody>();
        if (rigidbody == null) {
            rigidbody = go.AddComponent<Rigidbody>();
        }
        rigidbody.detectCollisions = false;
        //set tweener and bonkbehaviour to prop        
        Tweener tweener = toBonk.GameObject.AddComponent<Tweener>();

        BonkedBehaviour bonked = toBonk.GameObject.GetComponent<BonkedBehaviour>();
        if (bonked==null) {
            bonked = toBonk.GameObject.AddComponent<BonkedBehaviour>();
            bonked.initialScale = boneToBeBonked.localScale;
            bonked.boneToBeBonked = boneToBeBonked;
            bonked.node = this;
        }
        
        //AddComponent<BonkedBehaviour>();
        BonkBehaviour bonk = go.AddComponent<BonkBehaviour>();
        bonk.bonkingWith = go;
        bonk.node = this;
        bonk.bone = animator.GetBoneTransform(boneToBonk);
        bonk.bonked = bonked;
        //bonk.tweener = tweener;
        
        //
        //animator.GetBoneTransform(boneToBonk);
        
        //transform.SetLocalPositionAndRotation(toBonk.Transform.Position, Quaternion.Euler(toBonk.Transform.Rotation));
 
        Vector3 initialPosition    = transform.localPosition;
        Quaternion initialRotation = transform.localRotation;
        Vector3 initialScale       = transform.localScale;
        Vector3  pos;
        Quaternion rot;
        transform.GetPositionAndRotation(out pos, out rot);
        //Debug.Log(pos);
        //Debug.Log(transform.localScale);

        //var scaledBonkPosition = Vector3.Scale(bonkPosition, boneToBeBonked.localScale);

        tweener.runInSequence(
            new IEnumerator[] {
                tweener.Tween( //prepare rotation
                    (e) => { 
                        transform.SetLocalPositionAndRotation(
                            e*preparePosition,
                            Quaternion.Lerp(initialRotation,Quaternion.Euler(prepareRotation),e));
                        transform.localScale = (startScale + e*(finalScale-startScale))*Vector3.one;
                    },
                    prepareTime, 
                    prepareEasing, 
                    nameof(OnPrepared),
                    this 
                ),
                tweener.Perform(
                    () => {
                        rigidbody.detectCollisions = true;
                    }
                ),
                tweener.Tween( //bonk rotation
                    (e) =>  {
                        targetTransform.localPosition = Vector3.Scale(boneToBeBonked.localPosition, boneToBeBonked.localScale);
                        transform.SetLocalPositionAndRotation(
                        (1-e)*preparePosition+e*bonkPosition,
                        Quaternion.Lerp(initialRotation*Quaternion.Euler(prepareRotation),Quaternion.Euler(bonkRotation),e)
                    );},
                    //(e) => bonkWith.Transform.Rotation = initialRotation + new Vector3(0,0,prepareDegrees) + e*new Vector3(0,0,bonkDegrees-prepareDegrees), 
                    bonkTime, 
                    bonkEasing
                ),
                tweener.Delay(stayTime),
                tweener.Tween( //prepare rotation
                    (e) => { 
                        transform.localScale = (1-e)*finalScale*Vector3.one;
                    },
                    prepareTime/2, 
                    prepareEasing
                ),
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