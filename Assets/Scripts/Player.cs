using Assets.ScriptableObjects;
using Assets.Scripts.PlayerStateMachine;
using Assets.Scripts.Utility;
using Cinemachine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// TODO: Fix inspector errors
namespace Assets.Scripts
{
	#region RequireComponent
	[RequireComponent(typeof(AudioSource))]
	[RequireComponent(typeof(CinemachineImpulseSource))]
	[RequireComponent(typeof(Collider2D))]
	[RequireComponent(typeof(ParticleSystem))]
	[RequireComponent(typeof(PlayerInput))]
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(SpriteRenderer))]
	#endregion
	public class Player : MonoBehaviour
	{
		#region Component References
		[SerializeField] new Collider2D collider;
		public CapsuleCollider2D MyCapsule { get => (CapsuleCollider2D)collider; }
		public PlayerInput input;
		public Rigidbody2D rb;
		[SerializeField] CinemachineImpulseSource impulseSource;
		public CinemachineImpulseSource ImpulseSource { get => impulseSource; }
		[SerializeField] SpriteRenderer spriteRenderer;
		public SpriteRenderer SRenderer { get => spriteRenderer; }
		[SerializeField] AudioSource aSource;
		[SerializeField] new ParticleSystem particleSystem;
		[ContextMenu("Assign Component References")]
		void AssignComponentReferences()
		{
			collider = GetComponent<Collider2D>();
			input = GetComponent<PlayerInput>();
			rb = GetComponent<Rigidbody2D>();
			impulseSource = GetComponent<CinemachineImpulseSource>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			aSource = GetComponent<AudioSource>();
			particleSystem = GetComponent<ParticleSystem>();
		}
		#endregion
		public AudioSource ASource { get => aSource; }
		public ParticleSystem PSystem { get => particleSystem; }
		#region Camera Stuff
		public CinemachineVirtualCamera myCam;
		public Camera cam;
		[Expandable] public CameraSettings cameraSettings;
		#endregion
		PlayerContext _ctx;
		[ContextMenu("Assign Scene References")]
		void AssignSceneReferences()
		{
			speedText = GameObject.Find("Speed").GetComponent<TMP_Text>();
			messages = GameObject.Find("Messages").GetComponent<TMP_Text>();
			scoreText = GameObject.Find("Score").GetComponent<TMP_Text>();
			damageIndicator = FindObjectOfType<DamageIndicator>();
			boostMeterUI = FindObjectOfType<Slider>();
			myCam = FindObjectOfType<CinemachineVirtualCamera>();
			cam = FindObjectOfType<Camera>();
		}
		#region Velocity
		public Vector2 Velocity { get => rb.velocity; }
		Vector2 cachedVelocity;
		public Vector2 CachedVelocity { get => cachedVelocity; }
		Vector2 preCollisionVelocity;
		/// <summary>
		/// Updated at the end of FixedUpdate (same as <see cref="CachedVelocity"/>), but also updated in <see cref="OnCollisionEnter2D(Collision2D)"/>.
		/// </summary>
		public Vector2 PreCollisionVelocity { get => preCollisionVelocity; }
		#endregion
		#region SFX
		// TODO: USE RB_Walk For running sfx
		public AudioClip sfx_Running;
		public AudioClip sfx_Jump;
		public AudioClip sfx_Hurt;
		// TODO: Refactor SFXs to use SFX Groups
		public SFX_Group sfx_group_Jump;
		public SFX_Group sfx_group_Hurt;
		public SFX_Group sfx_group_Wallrun;
		#endregion
		#region Initializers
		void Reset()
		{
			AssignComponentReferences();
			AssignSceneReferences();
		}
		/// <summary>
		/// Used to restore size after rolling.
		/// </summary>
		public Vector2 colliderInitialDimensions { get; private set; }
		///// <summary>
		///// Used to restore size after rolling.
		///// </summary>
		//private Vector3 rendererInitialDimensions;
		void Start()
		{
			aSource.clip = sfx_Running;
			aSource.loop = true;

			colliderInitialDimensions = collider.bounds.size;
			//rendererInitialDimensions = spriteRenderer.bounds.size;

			(_ctx, _) = PlayerContext.CreateStateMachine(this, true);
		}
		#endregion

		#region Jump, Boost, & DownAction Checks and Update
		public readonly ButtonInfo<InputNames> jumpButton		= new(InputNames.Jump);
		public readonly ButtonInfo<InputNames> boostButton		= new(InputNames.Boost);
		public readonly ButtonInfo<InputNames> downActionButton	= new(InputNames.DownAction);
		void Update()
		{
			// Poll input
			jumpButton		.Update/*_DEBUG*/(input);
			boostButton		.Update/*_DEBUG*/(input);
			downActionButton.Update/*_DEBUG*/(input);
		}
		#endregion
		public readonly Collider2D[] enemyCollidersOverlapped = new Collider2D[3];
		/// <summary>
		/// Keeps track of number of consecutive actions that have kept you aerial.
		/// </summary>
		public uint aerialChain = 0;
		public uint boostMeter = 0;

		[Expandable] public MovementSettings movementSettings;
		[Expandable] public RollAnimationInfo rollInfo;
		[Expandable] public RollAnimationInfo coilInfo;

		// TOOD: Test aerial and grounded movement.
		// TODO: Troubleshoot landing on enemies problem.
		// TODO: Tweak wall run
		// TODO: Check for refactors from Velocity to Cached Velocity.
		// TODO: Add boost run
		// TODO: Add auditory feedback for fatal/damaging falls (like Mirror's Edge).
		// TODO: Add 2-stage jump w/ 2nd stage having increased gravity (https://youtu.be/ep_9RtAbwog?t=154)
		// TODO: Switch from physics-based movement to scripted movement?
		// TODO: Add toggleable state machine testing.
		// TODO: Figure out immediate double jump taps.
		// TODO: Expand Aerial Chain updates.
		// TODO: Display aerial chain
		void FixedUpdate()
		{
			_ctx.UpdateStates_DEBUG();
			//DirectStateManagement();

			#region After state is handled
			// Change Cam Size
			//myCam.m_Lens.OrthographicSize = cameraSettings.isOrthographicSizeFunctionActive ? 
			//	cameraSettings.GetTargetOrthographicSize(Velocity, transform.position, GlobalConstants.GroundLayer) :
			//	cameraSettings.defaultOrthographicSize;
			cameraSettings.UpdateCamera(myCam, /*Velocity, */transform.position, GlobalConstants.GroundLayer);

			// Update speed display after application of forces.
			UpdateSpeedText();

			// Update messages for opacity.
			UpdateUIMessages(Time.fixedDeltaTime);

			// Reset jump/boost inputs
			jumpButton		.AdvanceToNextFrame/*_DEBUG*/();
			boostButton		.AdvanceToNextFrame/*_DEBUG*/();
			downActionButton.AdvanceToNextFrame/*_DEBUG*/();

			cachedVelocity = preCollisionVelocity = Velocity;
			#endregion
		}

		#region State
		#region Flags
		[SerializeField] MovementState[] movementStateBuffer = new MovementState[10];
		[SerializeField] MovementState movementState = MovementState.None;
		public MovementState MState { get => movementState; }
		public CollisionState[] CollisionStateBuffer { get => collisionStateBuffer; }
		[SerializeField] CollisionState[] collisionStateBuffer = new CollisionState[10];
		[SerializeField] CollisionState collisionState = CollisionState.None;
		public CollisionState CState { get => collisionState; }
		#endregion
		bool boostConsumable = false;
		bool coilConsumable = false;
		#region Bound to 1 state
		#region Roll State
		bool rollRight = true;
		float rollTimer = 0;
		float rollInitialVx = 0;
		#endregion
		#region Wallrun State
		float hangTime = 1f;
		float wallrunStartDir = 0;
		#endregion
		float stumbleTimer = 0;
		float invincibleTimer = 0;
		float coilTimer = 0;
		#endregion
		#endregion
		void DirectStateManagement()
		{
			#region Methods
			Vector2 BasicMovement(Vector2 moveForce)
			{
				var moveVector = input.FindAction("Move").ReadValue<Vector2>();
				moveVector.x = (moveVector.x == 0) ? 0 : ((moveVector.x > 0) ? 1 : -1);
				moveVector.y = (moveVector.y == 0) ? 0 : ((moveVector.y > 0) ? 1 : -1);

				if (movementSettings.isConstantMovementEnabled)
				{
					rb.AddForce(Vector2.Scale(moveVector, movementSettings.constantMoveForce));
					transform.position += (Vector3)movementSettings.constantMovementDisplacement * Time.fixedDeltaTime;
				}
				else
					rb.AddForce(Vector2.Scale(moveVector, moveForce));
				return moveVector;
			}
			#region Coil State
			void EnterCoiling()
			{
				Debug.Assert(!movementState.HasFlag(MovementState.Coiling));
				Debug.Assert(coilConsumable);
				movementState |= MovementState.Coiling;
				coilTimer = movementSettings.coilTimerLength;
				coilConsumable = false;
				AddUIMessage("Coil Jump");
				// TODO: Only have aerial chain increment if coiling avoided a collsion.
				UpdateAerialChain(1);
			}
			void UpdateCoilState()
			{
				if (!movementState.HasFlag(MovementState.Jumping) &&
					!movementState.HasFlag(MovementState.Falling))
				{
					ExitCoiling();
					return;
				}
				coilTimer -= Time.fixedDeltaTime;
				// TODO: Apply DRY to following 2 ifs
				if (collisionState.HasFlag(CollisionState.EnemyCollider) && !movementState.HasFlag(MovementState.Invincible))
				{
					ExitCoiling();
					EnterStumble();
				}
				else if (coilTimer <= 0f || (/*rb.OverlapCollider(GlobalConstants.EnemyLayer, enemyCollidersOverlapped) <= 0 && */collisionState.HasFlag(CollisionState.Ground)))
					ExitCoiling();
				else
				{
					coilTimer = (coilTimer < 0) ? 0 : coilTimer;

					float GetCoilHeightAtTime(float coilTimer)
					{
						if (coilTimer <= coilInfo.entranceLengthRatio * movementSettings.coilTimerLength)
							return Mathf.SmoothStep(colliderInitialDimensions.y, colliderInitialDimensions.y * coilInfo.minHeightRatio, coilTimer / (movementSettings.coilTimerLength / 2));
						else if (coilTimer <= (coilInfo.entranceLengthRatio + coilInfo.properLengthRatio) * movementSettings.coilTimerLength)
							return colliderInitialDimensions.y * coilInfo.minHeightRatio;
						else
							return Mathf.SmoothStep(colliderInitialDimensions.y * coilInfo.minHeightRatio, colliderInitialDimensions.y, (coilTimer / (movementSettings.coilTimerLength / 2)) - 1);
					}
					var h = GetCoilHeightAtTime(coilTimer);

					MyCapsule.size = new(colliderInitialDimensions.x, h);

					// TODO: Remove when using animations
					//var srb = spriteRenderer.bounds;
					//srb.size = new Vector3(colliderInitialDimensions.x, h, rendererInitialDimensions.z);//srb.size.z);
					//spriteRenderer.bounds = srb;
				}
			}
			void ExitCoiling()
			{
				MyCapsule.size = colliderInitialDimensions;

				// TODO: Remove when using animations
				//var srb = spriteRenderer.bounds;
				//srb.size = rendererInitialDimensions;//new Vector3(colliderInitialDimensions.x, colliderInitialDimensions.y, srb.size.z);
				//spriteRenderer.bounds = srb;

				movementState ^= MovementState.Coiling;
			}
			#endregion
			#region Invincible State
			void EnterInvincible()
			{
				//Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), false);
				invincibleTimer = movementSettings.invincibleTimerLength;
				movementState |= MovementState.Invincible;
			}
			void UpdateInvincible()
			{
				//moveVector = moveVector.IsFinite() ? moveVector : BasicMovement(moveForceAerial);
				invincibleTimer -= Time.fixedDeltaTime;
				if (invincibleTimer <= 0f ||
					(rb.OverlapCollider(GlobalConstants.EnemyLayer, enemyCollidersOverlapped) <= 0 &&
					collisionState.HasFlag(CollisionState.Ground)))
					ExitInvincible();
			}
			void ExitInvincible()
			{
				var c = spriteRenderer.color;
				c.a = 1f;
				spriteRenderer.color = c;
				Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), false);
				movementState ^= MovementState.Invincible;
			}
			#endregion
			#region Rolling State
			void EnterRolling()
			{
				movementState |= MovementState.Rolling;
				particleSystem.Play(true);
				rollTimer = movementSettings.rollTimerLength;
				rollRight = /*Velocity*/CachedVelocity.x >= 0;
				rollInitialVx = Mathf.Abs(CachedVelocity.x);
				AddUIMessage("Successful PK Roll");
			}
			void UpdateRolling()
			{
				//transform.position += new Vector3(movementSettings.rollSpeed * Time.fixedDeltaTime * (rollRight ? 1 : -1), 0f, 0f);
				rollTimer -= Time.fixedDeltaTime;
				// TODO: Apply DRY to following 2 ifs
				if (collisionState.HasFlag(CollisionState.EnemyCollider))
				{
					ExitRolling();
					EnterStumble();
					return;
				}
				else if (rollTimer <= 0f/* || (rb.OverlapCollider(enemyLayer, enemyCollidersOverlapped) <= 0 && collisionState.HasFlag(CollisionState.Ground))*/)
				{
					rb.velocity = new(movementSettings.rollExitSpeed * (rollRight ? 1 : -1), Velocity.y);

					ExitRolling();
					return;
				}
				else
				{
					rollTimer = (rollTimer < 0) ? 0 : rollTimer;

					rb.velocity = new(Mathf.Lerp(movementSettings.rollExitSpeed, rollInitialVx, rollTimer / movementSettings.rollTimerLength) * (rollRight ? 1 : -1), Velocity.y);

					float GetRollHeightAtTime(float rollTimer)
					{
						if (rollTimer <= rollInfo.entranceLengthRatio * movementSettings.rollTimerLength)
							return Mathf.SmoothStep(colliderInitialDimensions.y, colliderInitialDimensions.y * rollInfo.minHeightRatio, rollTimer / (movementSettings.rollTimerLength / 2));
						else if (rollTimer <= (rollInfo.entranceLengthRatio + rollInfo.properLengthRatio) * movementSettings.rollTimerLength)
							return colliderInitialDimensions.y * rollInfo.minHeightRatio;
						else
							return Mathf.SmoothStep(colliderInitialDimensions.y * rollInfo.minHeightRatio, colliderInitialDimensions.y, (rollTimer / (movementSettings.rollTimerLength / 2)) - 1);
					}
					var h = GetRollHeightAtTime(rollTimer);

					// TOOD: Roll displacement prevention is jittery, tweak
					// Stop roll shrink from making player airborne
					//transform.position = new(transform.position.x, transform.position.y - colliderInitialDimensions.y * .5f, transform.position.z);
					var delta = MyCapsule.size.y - h;
					MyCapsule.size = new(colliderInitialDimensions.x, h);
					transform.position = new(transform.position.x, transform.position.y - delta, transform.position.z);

					// TODO: Remove when using animations
					//var srb = spriteRenderer.bounds;
					//srb.size = new Vector3(colliderInitialDimensions.x, h, rendererInitialDimensions.z);//srb.size.z);
					//spriteRenderer.bounds = srb;
				}
			}
			void ExitRolling()
			{
				// TOOD: Roll displacement prevention is jittery, tweak
				// Stop roll shrink from making player airborne
				//transform.position = new(transform.position.x, transform.position.y - colliderInitialDimensions.y * .5f, transform.position.z);
				var delta = MyCapsule.size.y - colliderInitialDimensions.y;
				transform.position = new(transform.position.x, transform.position.y - delta, transform.position.z);
				MyCapsule.size = colliderInitialDimensions;

				// TODO: Remove when using animations
				//var srb = spriteRenderer.bounds;
				//srb.size = rendererInitialDimensions;//new Vector3(colliderInitialDimensions.x, colliderInitialDimensions.y, srb.size.z);
				//spriteRenderer.bounds = srb;

				movementState ^= MovementState.Rolling;
			}
			#endregion
			#region Stumble State
			void EnterStumble()
			{
				movementState |= MovementState.Stumbling;
				stumbleTimer = movementSettings.stumbleTimerLength;
				var knockback = new Vector2(-1 * /*Velocity*/CachedVelocity.x * .5f, 2);
				rb.velocity = Vector2.zero;
				//rb.velocity = knockback;
				rb.AddForce(knockback, ForceMode2D.Impulse);
				Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), true);
				damageIndicator.Show(movementSettings.stumbleTimerLength);
				impulseSource?.GenerateImpulseAt(transform.position, /*Velocity*/CachedVelocity);
				AddUIMessage("Stumbled...");
				ResetBoostMeter();
				UpdateScore(-100);
				var c = spriteRenderer.color;
				c.a = .5f;
				spriteRenderer.color = c;
				aSource.PlayOneShot(sfx_Hurt);
				// TODO: Figure out start direction for blood sprays.
				var m = particleSystem.main;
				m.startColor = Color.red;

				var startSpeed = m.startSpeed;
				var origStartSpeedConstant = startSpeed.constant;
				startSpeed.constant = 3f;//knockback.x * 2f;
				m.startSpeed = startSpeed;

				var startLifetime = m.startLifetime;
				var origStartLifetimeConstant = startLifetime.constant;
				startLifetime.constant = movementSettings.stumbleTimerLength;
				m.startLifetime = startLifetime;

				particleSystem.Emit(20);
				particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

				m.startColor = Color.white;

				startSpeed.constant = origStartSpeedConstant;
				m.startSpeed = startSpeed;

				startLifetime.constant = origStartLifetimeConstant;
				m.startLifetime = startLifetime;
			}
			void UpdateStumbling()
			{
				stumbleTimer -= Time.fixedDeltaTime;
				if (stumbleTimer <= 0f)
					ExitStumbling();
			}
			void ExitStumbling()
			{
				AddUIMessage("Back up!");
				EnterInvincible();
				EnterGrounded();
				movementState ^= MovementState.Stumbling;
			}
			#endregion
			void EnterWallrunning()
			{
				var vel = Velocity;
				hangTime = Mathf.Abs(vel.x * Mathf.Sin(movementSettings.wallRunAngle));
				wallrunStartDir = (vel.x > 0) ? 1 : -1;
				vel.x *= Mathf.Cos(movementSettings.wallRunAngle);
				rb.velocity = vel;
				rb.AddForce(new(0, hangTime));
				boostConsumable = true;
				coilConsumable = true;
				particleSystem.Play(true);
				// TODO: Add wallrun sfx;
				//aSource.PlayOneShot(sfx_group_Wallrun.GetRandomClip());
				movementState |= MovementState.Wallrunning;
			}
			void ExitWallrunning()
			{
				// TODO: Add wallrun sfx;
				particleSystem.Stop(/*false, ParticleSystemStopBehavior.StopEmitting*/);
				hangTime = 0;
				wallrunStartDir = 0;
				movementState ^= MovementState.Wallrunning;
			}
			void EnterGrounded()
			{
				movementState |= MovementState.Grounded;
				boostConsumable = true;
				coilConsumable = true;
				aSource.clip = sfx_Running;
				particleSystem.Play(true);
			}
			void ExitGrounded()
			{
				aSource.Stop();
				particleSystem.Stop(/*false, ParticleSystemStopBehavior.StopEmitting*/);
				movementState ^= MovementState.Grounded;
			}
			void EnterJumping(Vector2 moveVector)
			{
				moveVector.y = 1;
				var fApplied = Vector2.Scale(moveVector, movementSettings.jumpForce);
				rb.AddForce(fApplied, ForceMode2D.Impulse);
				particleSystem.Emit(30);
				aSource.PlayOneShot(sfx_group_Jump.GetRandomClip());
				movementState |= MovementState.Jumping;
			}
			bool TryBoostJumping(Vector2 moveVector)
			{
				if (jumpButton.InputPressedOnThisFrame &&
					input.IsPressed("Boost") &&
					boostConsumable &&
					boostMeter >= movementSettings.boostJumpCost)
				{
					// If falling, zero out vertical velocity so the jump isn't fighting the downward momentum.
					if (Velocity.y < 0)
					{
						var vel = Velocity;
						vel.y = 0;
						rb.velocity = vel;
					}
					//aSource.Stop();// Should I do this?
					//particleSystem.Play();// Should I do this?
					EnterJumping(moveVector);
					UpdateBoostMeter(-movementSettings.boostJumpCost);
					boostConsumable = false;
					return true;
				}
				return false;
			}
			#endregion
			Vector2 moveVector = Vector2.positiveInfinity;
			Vector2 moveForceGround = (movementSettings.isAerialAndGroundedMovementUnique) ? movementSettings.moveForceGround : movementSettings.moveForce;
			Vector2 moveForceAerial = (movementSettings.isAerialAndGroundedMovementUnique) ? movementSettings.moveForceAerial : movementSettings.moveForce;
			Vector2 moveForceWallrun = (movementSettings.isAerialAndGroundedMovementUnique) ? movementSettings.moveForceWallrun : movementSettings.moveForce;
			switch (movementState)
			{
				case var t when t.HasFlag(MovementState.Grounded):
				case MovementState.Grounded:
				{
					if (movementState.HasFlag(MovementState.Rolling))
						UpdateRolling();//goto case MovementState.Rolling;
										// Ground Specific
					else
					{
						moveVector = moveVector.IsFinite() ? moveVector : BasicMovement(moveForceGround);
						// TODO: Debug running sound & particle effect start and stop.
						// TODO: Combine this collisionState.HasFlag(CollisionState.Ground) check with the next one.
						if (collisionState.HasFlag(CollisionState.Ground))
						{
							if (Velocity.x != 0)
							{
								if (!aSource.isPlaying)
									aSource.Play();
								if (!particleSystem.isPlaying)
									particleSystem.Play();
							}
							else if (Velocity.x == 0)
							{
								if (aSource.isPlaying)
									aSource.Stop();
								if (particleSystem.isPlaying)
									particleSystem.Stop();
							}
						}
						//Debug.Log($"isPlaying:{aSource.isPlaying}");
					}
					if (!collisionState.HasFlag(CollisionState.Ground) &&
						!collisionState.HasFlag(CollisionState.EnemyCollider))// TODO: work on removing enemy collider as valid grounded state.
					{
						ExitGrounded();
						movementState |= MovementState.Falling;
					}
					else if (collisionState.HasFlag(CollisionState.EnemyCollider) && !movementState.HasFlag(MovementState.Invincible))
					{
						ExitGrounded();
						EnterStumble();
					}
					else if (jumpButton.InputPressedOnThisFrame && movementState.IsInJumpableState())
					{
						ExitGrounded();
						EnterJumping(moveVector);
					}
					break;
				}
				case var t when t.HasFlag(MovementState.Wallrunning):
				case MovementState.Wallrunning:
				{
					// TODO: Add wallrun sfx;
					// TODO: Add wall jump
					moveVector = moveVector.IsFinite() ? moveVector : BasicMovement(moveForceWallrun);
					if (!collisionState.HasFlag(CollisionState.BGWall) ||
						hangTime <= 0 ||
						(Velocity.x >= 0 && wallrunStartDir < 0) ||
						(Velocity.x <= 0 && wallrunStartDir > 0))
						ExitWallrunning();
					else
					{
						// Counter gravity
						rb.AddForce(Physics2D.gravity * -1);
						rb.AddForce(new(0, hangTime));
						hangTime -= Time.fixedDeltaTime;
					}
					if (movementState.HasFlag(MovementState.Jumping))
						goto case MovementState.Jumping;
					else if (movementState.HasFlag(MovementState.Falling))
						goto case MovementState.Falling;
					break;
				}
				case var t when t.HasFlag(MovementState.Jumping):
				case MovementState.Jumping:
				{
					moveVector = moveVector.IsFinite() ? moveVector : BasicMovement(moveForceAerial);
					if (collisionState.HasFlag(CollisionState.EnemyCollider))
					{
						movementState ^= MovementState.Jumping;
						EnterStumble();
					}
					else if (Velocity.y <= 0)
					{
						movementState ^= MovementState.Jumping;
						movementState |= MovementState.Falling;
					}
					else if (collisionState.HasFlag(CollisionState.BGWall) && jumpButton.InputPressedOnThisFrame && Velocity.x != 0)
						EnterWallrunning();
					if ((movementState.HasFlag(MovementState.Jumping) || movementState.HasFlag(MovementState.Falling)) &&        //If we're in the air...
						!(movementState.HasFlag(MovementState.Wallrunning) || movementState.HasFlag(MovementState.Stumbling)) && //...and not wallrunning nor stumbling...
						!TryBoostJumping(moveVector) &&                                                                          //...then try boost jumping. If that fails...
						input.IsPressed(InputNames.DownAction) &&                                                                //...and the coil button is pressed...
						coilConsumable &&                                                                                        //...and we can coil...
						PlayerDistanceFromGround() > collider.bounds.size.y)                                                     //...and we're farther than 1 bodylength from the ground...
						EnterCoiling();
					if (movementState.HasFlag(MovementState.Coiling))
						UpdateCoilState();
					break;
				}
				case var t when t.HasFlag(MovementState.Falling):
				case MovementState.Falling:
				{
					Debug.Assert(Velocity.y <= 0 || CachedVelocity.y <= 0);
					moveVector = moveVector.IsFinite() ? moveVector : BasicMovement(moveForceAerial);
					if (collisionState == CollisionState.None || collisionState == CollisionState.BGWall)
					{
						if (TryBoostJumping(moveVector))
							movementState ^= MovementState.Falling;
						else if (input.IsPressed(InputNames.DownAction) && coilConsumable)
							EnterCoiling();
					}
					// TODO: If fall speed too high, force to roll?
					else if (jumpButton.InputPressedOnThisFrame && collisionState.HasFlag(CollisionState.EnemyTrigger))
					{
						var vel = Velocity;
						vel.y *= -1;
						rb.velocity = vel;
						rb.velocity *= movementSettings.conservedVelocity;

						// This process seems to automatically clear currentEnemyCollisions due
						// to the deactivation of enemy colliders further down the chain. If
						// the following assert fails, check there.
						int lengthBefore = currentEnemyCollisions.Count;
						var temp = currentEnemyCollisions.ToArray();
						for (int i = 0; i < temp.Length; i++)
							temp[i]?.OnPlayerStomp();
						Debug.Assert(lengthBefore > currentEnemyCollisions.Count);
						UpdateScore((lengthBefore - currentEnemyCollisions.Count) * 200);

						AddUIMessage("Enemy Bounce");
						UpdateBoostMeter(20);

						movementState ^= MovementState.Falling;
						EnterJumping(moveVector);
					}
					else if (((collisionState.HasFlag(CollisionState.Ground) && CachedVelocity.y < movementSettings.maxSafeFallSpeed) || collisionState.HasFlag(CollisionState.EnemyCollider)) && !movementState.HasFlag(MovementState.Invincible))
					{
						if (input.IsPressed(InputNames.DownAction.GetName()))
						{
							movementState ^= MovementState.Falling;
							EnterGrounded();
							EnterRolling();
						}
						else
						{
							movementState ^= MovementState.Falling;
							EnterStumble();
						}
					}
					else if (collisionState.HasFlag(CollisionState.Ground))
					{
						movementState ^= MovementState.Falling;
						EnterGrounded();
					}
					if (movementState.HasFlag(MovementState.Coiling))
						UpdateCoilState();
					break;
				}
				case var t when t.HasFlag(MovementState.Stumbling):
				case MovementState.Stumbling:
				{
					UpdateStumbling();
					break;
				}
				case var t when t.HasFlag(MovementState.Rolling):
				case MovementState.Rolling:
				{
					UpdateRolling();
					break;
				}
				case var t when t.HasFlag(MovementState.Coiling):
				case MovementState.Coiling:
				{
					UpdateCoilState();
					break;
				}
				case var t when t.HasFlag(MovementState.Invincible):
				case MovementState.Invincible:
				{
					Debug.LogError("Should never get here");
					break;
				}
				case MovementState.None:
				default:
				{
					Debug.LogWarning("Player Movement State Failure, entering Falling State.");
					//EnterGrounded();
					//goto case MovementState.Grounded;
					movementState |= MovementState.Falling;
					goto case MovementState.Falling;
				}
			}
			if (movementState.HasFlag(MovementState.Invincible))
				UpdateInvincible();

			//Debug.Log(movementState);

			// Slide new state onto buffer
			movementStateBuffer.SlideElementsDown(movementState);
			collisionStateBuffer.SlideElementsDown(collisionState);
		}

		#region UI Stuff
		#region Fields
		public TMP_Text speedText;
		public TMP_Text scoreText;
		public DamageIndicator damageIndicator;
		public Slider boostMeterUI;
		#endregion
		const float WARNING_RANGE = .25f;
		//bool showY = true;
		//short flipShowYCounter = 0;
		void UpdateSpeedText()
		{
			speedText.text = $" X: {Velocity.x} u/s\r\n ";
			if (Velocity.y < movementSettings.maxSafeFallSpeed)
			{
				//flipShowYCounter++;
				//showY ^= (flipShowYCounter >= 8);
				//if (showY)
				speedText.text += $"<color=\"red\">Y: {Velocity.y} u/s";
			}
			// Transitions from white to red as you approach the max safe fall speed
			else if (Velocity.y < (movementSettings.maxSafeFallSpeed * (1f - WARNING_RANGE)))
			{
				var velAdjusted = Velocity.y - movementSettings.maxSafeFallSpeed * (1f - WARNING_RANGE);
				var maxAdjusted = movementSettings.maxSafeFallSpeed * WARNING_RANGE;
				var percentage = velAdjusted / maxAdjusted;
				var outOf256 = Mathf.FloorToInt(Mathf.Lerp(255f, 0f, percentage));
				speedText.text += $"<color=#FF{outOf256:X2}{outOf256:X2}>Y: {Velocity.y} u/s";
			}
			else
				speedText.text += $"Y: {Velocity.y} u/s";
		}

		// TODO: Refactor to separate object?
		#region Messages
		public TMP_Text messages;
		private string[] currentMessages = new string[6] { "", "", "", "", "", "" };
		private float[] currentMessageTimes = new float[6] { 5, 5, 5, 5, 5, 5 };
		private float messageTimerLength = 5f;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <remarks>Works with <see cref="UpdateUIMessages(float)"/> to fade messages over time; requires <see cref="messages"/>, 
		/// <see cref="currentMessages"/>, <see cref="currentMessageTimes"/>, and <see cref="messageTimerLength"/> to work.</remarks>
		public void AddUIMessage(string message)
		{
			if (currentMessages.Length < 6)
			{
				var t = currentMessages;
				currentMessages = new string[6] { "", "", "", "", "", "" };
				t.CopyTo(currentMessages, 1);
			}
			else
			{
				//var t = currentMessages;
				//currentMessages = new string[] { "", t[0], t[1], t[2], t[3], t[4] };
				currentMessages.SlideElementsDown("");
			}
			currentMessageTimes.SlideElementsDown(messageTimerLength);
			currentMessages[Array.IndexOf(currentMessages, "")] = message;
		}
		// TODO: Fire with coroutine to minimize ui updates and clear coroutine on player disabled.
		private void UpdateUIMessages(float deltaTime = -1)
		{
			if (deltaTime < 0)
				deltaTime = Time.fixedDeltaTime;
			for (int i = 0; i < currentMessageTimes.Length; i++)
			{
				currentMessageTimes[i] -= deltaTime;
				if (currentMessageTimes[i] < 0)
					currentMessageTimes[i] = 0;
			}

			if (currentMessages.Length < 6)
			{
				var t = currentMessages;
				currentMessages = new string[6] { "", "", "", "", "", "" };
				t.CopyTo(currentMessages, 0);
			}
			else if (currentMessages.Length != 6)
			{
				Debug.LogWarning("More than 6 UI Messages, remainder will be discarded.");
				var t = currentMessages;
				currentMessages = new string[] { t[0], t[1], t[2], t[3], t[4], t[5] };
			}

			// Opacity
			var opacities = new int[currentMessages.Length];
			for (int i = 0; i < opacities.Length; i++)
			{
				var percentage = currentMessageTimes[i] / messageTimerLength;
				opacities[i] = Mathf.FloorToInt(percentage * 255);
			}

			// Set
			messages.text = "";
			for (int i = currentMessages.Length - 1; i >= 0; i--)
				if (currentMessages[i] != "")
					messages.text = $"<alpha=#{opacities[i]:X2}>{currentMessages[i]}\r\n{messages.text}";
		}
		#region Deprecated
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <remarks>Standalone; doesn't fade messages over time, only needs <see cref="messages"/> to work.</remarks>
		public void Deprecated_AddUIMessage(string message)
		{
			var messagesStored = messages.GetParsedText().Split(
				new string[] { "\r\n", "\r", "\n" },
				StringSplitOptions.RemoveEmptyEntries);
			Debug.Log($"messagesStored.Length: {messagesStored.Length}");
			if (messagesStored.Length < 6)
			{
				var t = messagesStored;
				messagesStored = new string[6] { "", "", "", "", "", "" };
				t.CopyTo(messagesStored, 1);
			}
			else
			{
				var t = messagesStored;
				messagesStored = new string[] { "", t[0], t[1], t[2], t[3], t[4] };
			}
			messagesStored[Array.IndexOf(messagesStored, "")] = message;
			messages.text = "";
			for (int i = messagesStored.Length - 1; i >= 0; i--)
				if (messagesStored[i] != "")
					messages.text = $"<style=\"m{i + 1}\">{messagesStored[i]}</style>\r\n{messages.text}";
		}
		#endregion
		#endregion

		// TODO: boostMeter and score uint weirdness check
		#region Boost Meter
		public void UpdateBoostMeter(int delta)
		{
			if (delta < 0)
				boostMeter -= (boostMeter < -delta) ? 0 - boostMeter : (uint)-delta;
			else
				boostMeter += (uint)delta;
			boostMeterUI.value = (boostMeter > boostMeterUI.maxValue) ? boostMeterUI.maxValue : boostMeter;
			if (boostMeter >= movementSettings.boostJumpCost)
				boostMeterUI.fillRect.GetComponent<Image>().color = Color.green;
			else if (boostMeter >= movementSettings.boostRunCost)
				boostMeterUI.fillRect.GetComponent<Image>().color = Color.yellow;
			else
				boostMeterUI.fillRect.GetComponent<Image>().color = Color.red;
		}
		public void ResetBoostMeter()
		{
			boostMeter = 0;
			UpdateBoostMeter(0);
		}
		#endregion

		#region Score
		public uint score = 0;
		public void UpdateScore(int delta)
		{
			if (delta < 0)
				score -= (score < -delta) ? 0 - score : (uint)-delta;
			else
				score += (uint)delta;
			string sText;
			if (score >= 100000000) sText = "";
			else if (score >= 10000000) sText = "0";
			else if (score >= 1000000) sText = "00";
			else if (score >= 100000) sText = "000";
			else if (score >= 10000) sText = "0000";
			else if (score >= 1000) sText = "00000";
			else if (score >= 100) sText = "000000";
			else if (score >= 10) sText = "0000000";
			else if (score >= 1) sText = "00000000";
			else sText = "00000000";
			scoreText.text = sText + score;
			Debug.Assert(scoreText.text.Length == 9);
		}
		public void ResetScore()
		{
			score = 0;
			UpdateScore(0);
		}
		#endregion

		// TODO: Finish Aerial Chain Methods
		#region Aerial Chain

		public void UpdateAerialChain(int delta)
		{
			if (delta < 0)
				aerialChain -= (aerialChain < -delta) ? 0 - aerialChain : (uint)-delta;
			else
				aerialChain += (uint)delta;
			string cText = "x";
			if		(aerialChain >= 100000000) cText = "";
			else if (aerialChain >= 10000000) cText = "0";
			else if (aerialChain >= 1000000) cText = "00";
			else if (aerialChain >= 100000) cText = "000";
			else if (aerialChain >= 10000) cText = "0000";
			else if (aerialChain >= 1000) cText = "00000";
			else if (aerialChain >= 100) cText = "000000";
			else if (aerialChain >= 10) cText = "0000000";
			else if (aerialChain >= 1) cText = "00000000";
			else					   cText = "00000000";
			//scoreText.text = cText + aerialChain;
			//Debug.Assert(scoreText.text.Length == 9);
		}
		public void ResetAerialChain()
		{
			aerialChain = 0;
			UpdateAerialChain(0);
		}
		#endregion

		void DrawPositionIndicator()
		{
			var screenPos = cam.WorldToScreenPoint(transform.position);
			var pos = transform.position;
			var screenOrigin = cam.ScreenToWorldPoint(new(0, 0, screenPos.z));
			var indicatorHeight = cam.OrthographicBoundsByScreen().size.y / 5f;
			pos.y = screenOrigin.y;
			MyGizmos.DrawLine3D(pos, pos + new Vector3(0, indicatorHeight, 0), Color.red);
			var bounds = collider.bounds;
			bounds.size = new Vector3(bounds.size.x, indicatorHeight, bounds.size.z);
			pos.y += indicatorHeight / 2f;
			bounds.center = pos;
			MyGizmos.DrawFilledBox2D(bounds, color: new(1, 1, 1, .3f));
		}
		#endregion

		public void OnFirearmHit(int scoreValue, int boostValue)
		{
			UpdateScore(scoreValue);
			UpdateBoostMeter(boostValue);
		}

		void OnRenderObject() => DrawPositionIndicator();

		#region Collision Updates
		public readonly List<Enemy> currentEnemyCollisions = new(3);
		#region OnCollision
		// TODO: Resolve bad wall jumping (check collider.max against collided.min and vice versa)
		// TODO: Modify to use switches
		void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
			{
				collisionState |= CollisionState.EnemyCollider;
				currentEnemyCollisions.Add(collision.gameObject.GetComponent<Enemy>());
			}
			else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
				collisionState |= CollisionState.Ground;
		}
		void OnCollisionStay2D(Collision2D collision)
		{
			if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
				collisionState |= CollisionState.EnemyCollider;
			else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
				collisionState |= CollisionState.Ground;
		}
		void OnCollisionExit2D(Collision2D collision)
		{
			if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
			{
				collisionState |= CollisionState.EnemyCollider;
				collisionState ^= CollisionState.EnemyCollider;
				currentEnemyCollisions.Remove(collision.gameObject.GetComponent<Enemy>());
			}
			else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
			{
				collisionState |= CollisionState.Ground;
				collisionState ^= CollisionState.Ground;
			}
		}
		#endregion
		#region OnTrigger
		void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
			{
				collisionState |= CollisionState.EnemyTrigger;
				currentEnemyCollisions.Add(collision.GetComponent<Enemy>());
			}
			else if (collision.gameObject.layer == LayerMask.NameToLayer("BGInteractable"))
				collisionState |= CollisionState.BGWall;
			else if (collision.gameObject.layer == LayerMask.NameToLayer("Goal"))
				Debug.Log($"Finished with time of {Time.timeSinceLevelLoad}");
		}
		void OnTriggerStay2D(Collider2D collision)
		{
			if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
				collisionState |= CollisionState.EnemyTrigger;
			else if (collision.gameObject.layer == LayerMask.NameToLayer("BGInteractable"))
				collisionState |= CollisionState.BGWall;
		}
		void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
			{
				collisionState |= CollisionState.EnemyTrigger;
				collisionState ^= CollisionState.EnemyTrigger;
				currentEnemyCollisions.Remove(collision.GetComponent<Enemy>());
			}
			else if (collision.gameObject.layer == LayerMask.NameToLayer("BGInteractable"))
			{
				collisionState |= CollisionState.BGWall;
				collisionState ^= CollisionState.BGWall;
			}
		}
		#endregion
		#endregion

		#region PlayerDistanceFromGround
		private Vector3 _posAtLastCheck = new(float.NaN, float.NaN, float.NaN);
		private float _lastDist = float.NaN;
		public float PlayerDistanceFromGround(bool accountForCollider = true)
		{
			if (transform.position != _posAtLastCheck)
			{
				_posAtLastCheck = transform.position;
				var rcResults = new RaycastHit2D[1];
				var numResults = Physics2D.Raycast(_posAtLastCheck, Vector2.down, GlobalConstants.GroundLayer, rcResults);
				_lastDist = (numResults > 0) ? rcResults[0].distance : float.NaN;
				_lastDist -= (accountForCollider) ? collider.bounds.extents.y : 0; // Assumes collider not offset
			}
			return _lastDist;
		}
		#endregion
	}
}