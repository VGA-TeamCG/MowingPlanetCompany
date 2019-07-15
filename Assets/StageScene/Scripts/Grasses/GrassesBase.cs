﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MowingPlanetCompany.StageScene
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">各草クラス.</typeparam>
    [RequireComponent(typeof(GizmoObject), typeof(Rigidbody))]
    public abstract class GrassesBase<T> : StatefulObjectBase<T, GrassState> where T : class
    {
        #region Properties
        public GrassStatus Status { get { return status; } set { status = value; } }
        public float HeightPower { get { return heightPower; } set { heightPower = value; } }
        public float SpherePower { get { return spherePower; } set { spherePower = value; } }
        public float DestoryDelayTime { get { return destroyDelayTime; } set { destroyDelayTime = value; } }
        #endregion

        #region Variables
        [SerializeField] protected GrassStatus status;
        /// <summary>上に飛ばす強さ</summary>
        [SerializeField] float heightPower;
        /// <summary>球体上のランダムな方角に飛ばす強さ</summary>
        [SerializeField] float spherePower;
        /// <summary>破壊迄の遅延時間</summary>
        [SerializeField] float destroyDelayTime = 1f;
        [SerializeField] string currentState;

        protected Rigidbody rb;
        protected Transform player;
        protected TimeManager timeManager;
        /// <summary>Gizmo of sensor range</summary>
        protected GizmoObject gizmoObj;
        /// <summary>
        /// Gizumoのwidthに毎フレーム渡す値.
        /// ステート毎にここに代入する変数をかえる.
        /// </summary>
        protected float gizmoRange;
        #endregion

        #region Methods
        protected virtual void Awake()
        {
            timeManager = TimeManager.Instance;
            gizmoObj = GetComponent<GizmoObject>();
            rb = GetComponent<Rigidbody>();
        }
        protected override void Update()
        {
            base.Update();
            //// ===============================================
            //// 転倒防止。　動くかは要確認
            //// ===============================================
            //var adjustRot = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);
            //transform.rotation = adjustRot;
            gizmoObj.SetGizmo(gizmoRange);
        }
        /// <summary>
        /// Colliderと衝突した時のコールバック
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.gameObject.name);
            if (other.gameObject.tag == "Weapon")
            {
                OnCollideWeapon();
            }
        }
        public virtual void Initialize()
        {
            player = GameObject.FindWithTag("Player").transform;
            status.Initialize();
        }
        /// <summary>
        /// on collide weapon of player
        /// </summary>
        public virtual void OnCollideWeapon()
        {
            var damage = player.GetComponent<Mowie>().MyStatus.Atk;
            status.Life -= damage;
            if (status.Life <= 0)
            {
                ChangeState(GrassState.ToDie);
            }
        }
        #endregion

        #region EachStateBehaviour
        /// <summary>
        /// StateWander
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected class StateWander<T> : State<T, GrassState> where T : GrassesBase<T>
        {
            public StateWander(T owner, GrassState identity) : base(owner, identity) { }
            Vector3 targetPosition;
            float changeDirectionTime = 5;

            public override void Enter()
            {
                base.Enter();
                Debug.Log(string.Format("owner is {0}. currentState is {1}.", owner.gameObject.name, owner.stateMachine.CurrentState));
                targetPosition = GetRandomPositionOnLevel();
            }
            public override void Execute()
            {
                if (ElapsedTimeSinseStateStart > owner.Status.ChangeStateLimitTime)
                {
                    owner.ChangeState(GrassState.Stop);
                }
                // プレイヤーとの距離が小さければ追跡ステートに遷移
                var distanceToPlayer = Vector3.Magnitude(owner.transform.position - owner.player.position);
                owner.gizmoRange = owner.status.PursuitDistance;
                if (distanceToPlayer < owner.status.PursuitDistance)
                {
                    //  change state.
                    owner.ChangeState(GrassState.Pursuit);
                }
                // 目標地点との距離が小さければ、次のランダムな目標地点を設定する
                var distanceToTarget = Vector3.Magnitude(owner.transform.position - targetPosition);
                owner.gizmoRange = owner.status.ChangeTargetDistance;
                if (distanceToTarget < owner.status.ChangeTargetDistance || ElapsedTimeSinseStateStart > changeDirectionTime)
                {
                    targetPosition = GetRandomPositionOnLevel();
                }

                // look at target position
                var targetRotation = Quaternion.LookRotation(targetPosition - owner.transform.position);
                owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRotation, Time.deltaTime * owner.status.RotationSmooth);
                owner.rb.velocity = Vector3.forward * owner.status.Speed;
            }
            public override void Exit() { }

            /// <summary>
            /// 指定半径内の座標をランダムに返す
            /// </summary>
            /// <returns></returns>
            public Vector3 GetRandomPositionOnLevel()
            {
                var radius = owner.status.ActivityRange;
                return new Vector3(Random.Range(-radius, radius), 0f, Random.Range(-radius, radius));
            }
        }
        /// <summary>
        /// StateAttack
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected class StateAttack<T> : State<T, GrassState> where T : GrassesBase<T>
        {
            public StateAttack(T owner, GrassState identity) : base(owner, identity) { }

            public override void Enter()
            {
                base.Enter();
                Debug.Log(string.Format("owner is {0}. currentState is {1}.", owner.gameObject.name, owner.stateMachine.CurrentState));
            }
            public override void Execute()
            {
                // Playerとの距離が大きければ追跡ステートに遷移
                var distanceToPlayer = Vector3.Magnitude(owner.transform.position - owner.player.position);
                owner.gizmoRange = distanceToPlayer;
                if (distanceToPlayer > owner.status.AttackDistance + owner.status.Margin)
                {
                    owner.ChangeState(GrassState.Pursuit);
                }
                Debug.Log("Attackしたよ");
            }
            public override void Exit() { }
        }
        /// <summary>
        /// StateEscape
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected class StateEscape<T> : State<T, GrassState> where T : GrassesBase<T>
        {
            public StateEscape(T owner, GrassState identity) : base(owner, identity) { }

            public override void Enter()
            {
                base.Enter();
                Debug.Log(string.Format("owner is {0}. currentState is {1}.", owner.gameObject.name, owner.stateMachine.CurrentState));
            }
            public override void Execute()
            {
                var sqrDistanceToPlayer = Vector3.SqrMagnitude(owner.player.position - owner.transform.position);
                if (sqrDistanceToPlayer > owner.status.EscapeDistance - owner.status.Margin)
                {
                    owner.ChangeState(GrassState.Wander);
                }

                // look at target position
                var targetRotation = Quaternion.LookRotation(owner.player.position - owner.transform.position);
                owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRotation, Time.deltaTime * owner.status.RotationSmooth);

                // go to straight
                owner.transform.Translate(Vector3.forward * owner.status.Speed * Time.deltaTime);
            }
            public override void Exit() { }
        }
        /// <summary>
        /// StateDestroy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected class StateDestroy<T> : State<T, GrassState> where T : GrassesBase<T>
        {
            public StateDestroy(T owner, GrassState identity) : base(owner, identity) { }

            public override void Enter()
            {
                base.Enter();
                Debug.Log(string.Format("owner is {0}. currentState is {1}.", owner.gameObject.name, owner.stateMachine.CurrentState));
                // 上方ベクトルとランダムなベクトル方向に飛ばす
                var force = Vector3.up * owner.HeightPower + Random.insideUnitSphere * owner.SpherePower;
                owner.GetComponent<Rigidbody>().AddForce(force);

                // 指定秒数遅延後オブジェクト破壊
                Destroy(owner.gameObject, owner.DestoryDelayTime);
            }
            public override void Execute() { }
            public override void Exit() { }
        }
        /// <summary>
        /// StatePursuit
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected class StatePursuit<T> : State<T, GrassState> where T : GrassesBase<T>
        {
            public StatePursuit(T owner, GrassState identity) : base(owner, identity) { }

            public override void Enter()
            {
                base.Enter();
                Debug.Log(string.Format("owner is {0}. currentState is {1}.", owner.gameObject.name, owner.stateMachine.CurrentState));
            }
            public override void Execute()
            {
                var sqrDistanceToPlayer = Vector3.SqrMagnitude(owner.player.position - owner.transform.position);
                if (sqrDistanceToPlayer > owner.status.PursuitDistance - owner.status.Margin)
                {
                    owner.ChangeState(GrassState.Wander);
                }

                // look at target position
                var targetRotation = Quaternion.LookRotation(owner.player.position - owner.transform.position);
                owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, targetRotation, Time.deltaTime * owner.status.RotationSmooth);

                // go to straight
                owner.transform.Translate(Vector3.forward * owner.status.Speed * Time.deltaTime);
            }
            public override void Exit() { }
        }
        /// <summary>
        /// StatePause
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected class StateStop<T> : State<T, GrassState> where T : GrassesBase<T>
        {
            public StateStop(T owner, GrassState identity) : base(owner, identity) { }

            public override void Enter()
            {
                base.Enter();
                Debug.Log(string.Format("owner is {0}. currentState is {1}.", owner.gameObject.name, owner.stateMachine.CurrentState));
            }
            public override void Execute() { }
            public override void Exit() { }
        }
        #endregion
    }
    public enum GrassState
    {
        Wander,
        Pursuit,
        Attack,
        Escape,
        ToDie,
        Stop,
    }

    public enum GrassID
    {
        Normal,
        Nigenige,
        Pakupaku,
        Tsukitsuki,
    }
}
