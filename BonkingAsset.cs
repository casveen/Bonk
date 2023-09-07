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
using Warudo.Plugins.Core.Mixins;
using Warudo.Plugins.Core.Assets.Mixins;

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
using Playground;
using Bonk;

namespace Bonk {
    [AssetType(
        Id = nameof(BonkingAsset), // Must be unique. Generate one at https://guidgenerator.com/
        Title = "BonkingAsset",
        Category ="Fnugus")]
    public class BonkingAsset : Asset {  

        [DataInput]
        public Bonkable[] Bonkables = new Bonkable[] {};

        public class Bonkable : StructuredData<BonkingAsset>, ICollapsibleStructuredData {
            [DataInput]
            public string Name;

            [DataInput]
            [AutoCompleteResource("Prop")]
            public string BonkWithSource;

            [SectionAttribute("Rotaion axis",0)]
            [DataInput]
            public Vector3 RotationAxis;

            [DataInput]
            public Vector3 RotationOffset;

            private bool inAxisMode = false;
            GameObject visualizedObject = null;
            [Trigger]
            //[HiddenIf(nameof(HideEnterAxisMode))]
            public async void EnterAxisMode() {
                if (!inAxisMode) {
                    inAxisMode=true;
                    //make a prop for visualization
                    visualizedObject    = Context.ResourceManager.ResolveResourceUri<GameObject>(BonkWithSource);
                    Transform transform = visualizedObject.GetComponent<Transform>();
                    Vector3 origin      = transform.position; 

                    visualizedObject.AddComponent<Tweener>(); 
                    Tweener tweener = visualizedObject.GetComponent<Tweener>();
                    tweener.Cycle(
                        (e) => {    
                            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(StartRotation)); 
                            transform.RotateAround(
                                RotationOffset, 
                                RotationAxis, 
                                10*360*e
                            );
                        },
                        10f
                    );
                } else {
                    inAxisMode=false;
                    Object.Destroy(visualizedObject);
                }

            }
            [SectionAttribute("Start",0)]
            [DataInput]
            public Vector3 StartPosition = new Vector3(0,0,0);
            [DataInput]
            public Vector3 StartRotation = new Vector3(0,0,90);
            [DataInput]
            public float StartScale = 0.0f;


            [SectionAttribute("Prepare",0)]
            [DataInput]
            public Vector3 PreparePosition = new Vector3(0.25f,0,0);
            [DataInput]
            public float PrepareRotateAmount = -495f;
            [DataInput]
            public float PrepareScale = 4.0f;
            [DataInput]
            public float PrepareTime = 2.0f;
        
            [SectionAttribute("Bonk",0)]
            [DataInput]
            public Vector3 BonkPosition = new Vector3(0.5f,0.5f,0);
            [DataInput]
            public float BonkRotateAmount = 135f;
            [DataInput]
            public float BonkScale = 4.0f;
            [DataInput]
            public float BonkTime = 0.3f;
            [DataInput]
            public float StayTime = 1.0f;

            [SectionAttribute("Effect",0)]
            [DataInput]
            public Vector3 Flattening = new Vector3(1f,1f,0.1f); 
            [DataInput]
            public float FlattenTime = 10.0f; 
            [DataInput]
            public float InflateTime = 2.0f; 
            
            [DataInput]
            public float finalScale = 1.0f;
            [SectionAttribute("Easing",0)]
            [DataInput]
            public Easing.Function PrepareEasing = Easing.Function.ExponentialOut;
            [DataInput]
            public Easing.Function BonkEasing    = Easing.Function.ExponentialOut;
            [DataInput]
            public Easing.Function FlattenEasing = Easing.Function.Linear;
            [DataInput]
            public Easing.Function InflateEasing = Easing.Function.ElasticOut;
            [DataInput]
            public Easing.Function SizeEasing    = Easing.Function.CircularIn;

            public string GetHeader() {
                return Name??"<Bonkable name not set>";
            }

            public void EnableGizmos(Transform target) {
                var tg = Context.PluginManager.GetPlugin<CorePlugin>().TransformGizmo;
                tg.AddTargets(new [] { target }, 
                    null, gizmo => {
                    gizmo.handleLength = .025f;
                    gizmo.handleWidth  = .0004f;
                    gizmo.planeSize    = .005f;
                    gizmo.triangleSize = .006f;
                    gizmo.circleDetail = 16;
                });
            }


        }

        protected override void OnCreate() {
                base.OnCreate();
                SetActive(true);
            }

        public IEnumerator PrepareMotion(Bonkable bonkable, Transform transform, Tweener tweener) {
            return tweener.Tween(
                (e) => {
                    transform.localPosition   = (1-e)*bonkable.StartPosition+e*bonkable.PreparePosition;
                    transform.localRotation   = Quaternion.Euler(bonkable.StartRotation);
                    transform.RotateAround(
                        transform.position+ Vector3.Scale(bonkable.RotationOffset, transform.localScale), 
                        bonkable.RotationAxis, 
                        e*bonkable.PrepareRotateAmount
                    );
                    transform.localScale = (bonkable.StartScale + e*(bonkable.PrepareScale-bonkable.StartScale))*Vector3.one;
                },
                bonkable.PrepareTime, 
                bonkable.PrepareEasing 
            );
        }

        public IEnumerator PrepareMotion(Bonkable bonkable, Transform transform, Tweener tweener, float sizeFactor) {
            return tweener.Tween(
                (e) => {
                    transform.localPosition   = (1-e)*bonkable.StartPosition+e*bonkable.PreparePosition;
                    transform.localRotation   = Quaternion.Euler(bonkable.StartRotation);
                    transform.RotateAround(
                        transform.position+ Vector3.Scale(bonkable.RotationOffset, transform.localScale), 
                        bonkable.RotationAxis, 
                        e*bonkable.PrepareRotateAmount
                    );
                    transform.localScale = (bonkable.StartScale + e*(bonkable.PrepareScale-bonkable.StartScale))*Vector3.one*sizeFactor;
                },
                bonkable.PrepareTime, 
                bonkable.PrepareEasing 
            );
        }

        public IEnumerator BonkMotion(Bonkable bonkable, Transform transform, Tweener tweener) {
            return tweener.Tween(
                (e) => {
                    transform.localPosition   = (1-e)*bonkable.PreparePosition+e*bonkable.BonkPosition;
                    transform.localRotation   = Quaternion.Euler(bonkable.StartRotation);
                    
                    transform.RotateAround(
                        transform.position+ Vector3.Scale(bonkable.RotationOffset, transform.localScale), 
                        bonkable.RotationAxis, 
                        bonkable.PrepareRotateAmount+e*bonkable.BonkRotateAmount
                    );
                    transform.localScale = (bonkable.PrepareScale + e*(bonkable.BonkScale-bonkable.PrepareScale))*Vector3.one;
                },
                bonkable.BonkTime, 
                bonkable.BonkEasing 
            );
        }

        public IEnumerator BonkMotion(Bonkable bonkable, Transform transform, Tweener tweener, float sizeFactor) {
            return tweener.Tween(
                (e) => {
                    transform.localPosition   = (1-e)*bonkable.PreparePosition+e*bonkable.BonkPosition;
                    transform.localRotation   = Quaternion.Euler(bonkable.StartRotation);
                    
                    transform.RotateAround(
                        transform.position+ Vector3.Scale(bonkable.RotationOffset, transform.localScale), 
                        bonkable.RotationAxis, 
                        bonkable.PrepareRotateAmount+e*bonkable.BonkRotateAmount
                    );
                    transform.localScale = (bonkable.PrepareScale + e*(bonkable.BonkScale-bonkable.PrepareScale))*Vector3.one*sizeFactor;
                },
                bonkable.BonkTime, 
                bonkable.BonkEasing 
            );
        }
 



    }
}