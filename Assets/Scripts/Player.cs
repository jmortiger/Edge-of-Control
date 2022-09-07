using Assets.ScriptableObjects;
using Assets.Scripts.Utility;
using Cinemachine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// TODO: Trim inspector stuff, handled in UIDocument.
// TODO: Modify UIDocument to reflect new fields.
// TODO: Modify UIDocument for view data key.
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
		CapsuleCollider2D MyCapsule { get => (CapsuleCollider2D)collider; }
		public PlayerInput input;
		public Rigidbody2D rb;
		[SerializeField] CinemachineImpulseSource impulseSource;
		[SerializeField] SpriteRenderer spriteRenderer;
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
		#region Camera Stuff
		public CinemachineVirtualCamera myCam;
		public Camera cam;
		public CameraSettings cameraSettings;
		#endregion

		[ContextMenu("Assign Scene References")]
		void AssignSceneReferences()
		{
			speedText = GameObject.Find("Speed").GetComponent<TMP_Text>();
			messages = GameObject.Find("Messages").GetComponent<TMP_Text>();
			scoreText = GameObject.Find("Score").GetComponent<TMP_Text>();
			damageIndicator = FindObjectOfType<DamageIndicator>();
			boostMeterUI = FindObjectOfType<UnityEngine.UI.Slider>();
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
		public AudioClip sfx_Running;
		public AudioClip sfx_Jump;
		public AudioClip sfx_Hurt;
		// TODO: Refactor SFXs to use SFX Groups
		public SFX_Group sfx_group_Jump;
		public SFX_Group sfx_group_Hurt;
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
		private Vector2 colliderInitialDimensions;
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
		}
		#endregion

		public MovementSettings movementSettings;
		#region Jump & Boost Checks and Update
		#region Jump
		bool jumpPressedLastFrame = false;
		bool jumpPressedThisFrame = false;
		bool JumpPressedOnThisFrame { get => (jumpPressedThisFrame && (!jumpPressedLastFrame)); }
		#endregion
		#region Boost
		bool boostPressedLastFrame = false;
		bool boostPressedThisFrame = false;
		bool BoostPressedOnThisFrame { get => (boostPressedThisFrame && (!boostPressedLastFrame)); }
		#endregion
		void Update()
		{
			jumpPressedThisFrame = jumpPressedThisFrame || input.IsPressed("Jump");
			boostPressedThisFrame = boostPressedThisFrame || input.IsPressed("Boost");
			//Debug.Log($"{jumpPressedLastFrame}");
		}
		#endregion
		#region State
		#region Flags
		[Flags]
		public enum MovementState
		{
			None = 0x0000_0000,
			Grounded = 0x0000_0001,
			Jumping = 0x0000_0010,
			Wallrunning = 0x0000_0100,
			Falling = 0x0000_1000,
			Stumbling = 0x0001_0000,
			Invincible = 0x0010_0000,
			Rolling = 0x0100_0000,
		}
		[SerializeField]
		MovementState movementState = MovementState.None;
		[Flags]
		public enum CollisionState
		{
			None = 0x0000_0000,
			BGWall = 0x0000_0001,
			Ground = 0x0000_0010,
			EnemyCollider = 0x0000_0100,
			EnemyTrigger = 0x0000_1000,
		}
		[SerializeField]
		CollisionState collisionState = CollisionState.None;
		#endregion
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
		bool boostConsumable = false;
		#endregion
		readonly Collider2D[] enemyCollidersOverlapped = new Collider2D[3];
		[SerializeField] uint jumpPresses = 0;
		public int combo = 0;
		public uint boostMeter = 0;

		public RollAnimationInfo rollInfo;

		// TODO: Make aerial movement different from grounded movement.
		// TODO: Troubleshoot landing on enemies problem.
		// TODO: Add combo timer
		// TODO: Tweak wall run
		// TODO: Check for refactors from Velocity to Cached Velocity.
		// TODO: Add coil jump
		// TODO: Add boost run
		// TODO: Add auditory feedback for fatal/damaging falls (like Mirror's Edge).
		// TODO: Add 2-stage jump w/ 2nd stage having increased gravity (https://youtu.be/ep_9RtAbwog?t=154)
		// TODO: Switch from physics-based movement to scripted movement.
		void FixedUpdate()
		{
			#region Debug Jump Checks
			//if (JumpPressedOnThisFrame)
			//{
			//	jumpPresses++;
			//	Debug.Log($"JumpPressedOnThisFrame");
			//}
			//Debug.Log($"Presses: {jumpPresses}");
			#endregion
			#region Methods
			Vector2 BasicMovement()
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
					rb.AddForce(Vector2.Scale(moveVector, movementSettings.moveForce));
				return moveVector;
			}
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
				combo = 0;
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
			void EnterGrounded()
			{
				movementState |= MovementState.Grounded;
				boostConsumable = true;
				particleSystem.Play(true);
			}
			void EnterRolling()
			{
				movementState |= MovementState.Rolling;
				particleSystem.Play(true);
				rollTimer = movementSettings.rollTimerLength;
				rollRight = /*Velocity*/CachedVelocity.x >= 0;
				rollInitialVx = Mathf.Abs(CachedVelocity.x);
				AddUIMessage("Successful PK Roll");
			}
			void EnterWallrunning()
			{
				var vel = Velocity;
				hangTime = Mathf.Abs(vel.x * Mathf.Sin(movementSettings.wallRunAngle));
				wallrunStartDir = (vel.x > 0) ? 1 : -1;
				vel.x *= Mathf.Cos(movementSettings.wallRunAngle);
				rb.velocity = vel;
				rb.AddForce(new(0, hangTime));
				boostConsumable = true;
				movementState |= MovementState.Wallrunning;
			}
			bool TryBoostJumping(Vector2 moveVector)
			{
				if (JumpPressedOnThisFrame && input.IsPressed("Boost") && boostConsumable && boostMeter >= movementSettings.boostJumpCost)
				{
					//var moveVector = input.FindAction("Move").ReadValue<Vector2>();
					//moveVector.x = (moveVector.x == 0) ? 0 : ((moveVector.x > 0) ? 1 : -1);
					moveVector.y = 1;
					// If falling, zero out vertical velocity so the jump isn't fighting the downward momentum.
					if (Velocity.y < 0)
					{
						var vel = Velocity;
						vel.y = 0;
						rb.velocity = vel;
					}
					var fApplied = Vector2.Scale(moveVector, movementSettings.jumpForce);
					rb.AddForce(fApplied, ForceMode2D.Impulse);
					aSource.Stop();
					//particleSystem.Play();// Should I do this?
					particleSystem.Emit(30);
					particleSystem.Stop();
					aSource.PlayOneShot(sfx_group_Jump.GetRandomClip());
					UpdateBoostMeter(-movementSettings.boostJumpCost);
					boostConsumable = false;
					movementState |= MovementState.Jumping;
					return true;
				}
				return false;
			}
			#endregion
			Vector2 moveVector = Vector2.positiveInfinity;
			switch (movementState)
			{
				case var t when t.HasFlag(MovementState.Grounded):
				case MovementState.Grounded:
				{
					if (movementState.HasFlag(MovementState.Rolling))
						goto case MovementState.Rolling;
					moveVector = moveVector.IsFinite() ? moveVector : BasicMovement();
					aSource.clip = sfx_Running;
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
					if (!collisionState.HasFlag(CollisionState.Ground) && 
						!collisionState.HasFlag(CollisionState.EnemyCollider) && 
						!collisionState.HasFlag(CollisionState.EnemyTrigger))
					{
						aSource.Stop();
						particleSystem.Stop();
						movementState |= MovementState.Falling;
						movementState ^= MovementState.Grounded;
					}
					else if (collisionState.HasFlag(CollisionState.EnemyCollider) && !movementState.HasFlag(MovementState.Invincible))
					{
						aSource.Stop();
						particleSystem.Stop();
						movementState ^= MovementState.Grounded;
						EnterStumble();
					}
					else if (JumpPressedOnThisFrame)
					{
						//var moveVector = input.FindAction("Move").ReadValue<Vector2>();
						//moveVector.x = (moveVector.x == 0) ? 0 : ((moveVector.x > 0) ? 1 : -1);
						moveVector.y = 1;
						// TODO: If fall speed too high, force to roll?
						if (collisionState.HasFlag(CollisionState.EnemyTrigger) && Velocity.y <= 0)
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
							combo++;
							UpdateBoostMeter(20);
						}
						var fApplied = Vector2.Scale(moveVector, movementSettings.jumpForce);
						rb.AddForce(fApplied, ForceMode2D.Impulse);
						aSource.Stop();
						particleSystem.Emit(30);
						particleSystem.Stop();
						aSource.PlayOneShot(/*sfx_Jump*/sfx_group_Jump.GetRandomClip());
						movementState |= MovementState.Jumping;
						movementState ^= MovementState.Grounded;
					}
					if (movementState.HasFlag(MovementState.Invincible))
						goto case MovementState.Invincible;
					break;
				}
				case var t when t.HasFlag(MovementState.Jumping):
				case MovementState.Jumping:
				{
					moveVector = moveVector.IsFinite() ? moveVector : BasicMovement();
					if (collisionState.HasFlag(CollisionState.EnemyCollider))
					{
						movementState ^= MovementState.Jumping;
						EnterStumble();
					}
					else if (Velocity.y <= 0)
					{
						movementState |= MovementState.Falling;
						movementState ^= MovementState.Jumping;
					}
					else if (collisionState.HasFlag(CollisionState.BGWall) && JumpPressedOnThisFrame && Velocity.x != 0)
					{
						movementState ^= MovementState.Jumping;
						EnterWallrunning();
					}
					else
						TryBoostJumping(moveVector);
					if (movementState.HasFlag(MovementState.Invincible))
						goto case MovementState.Invincible;
					break;
				}
				case var t when t.HasFlag(MovementState.Wallrunning):
				case MovementState.Wallrunning:
				{
					// TODO: Add wall jump
					moveVector = moveVector.IsFinite() ? moveVector : BasicMovement();
					if (!collisionState.HasFlag(CollisionState.BGWall) ||
						hangTime <= 0 ||
						(Velocity.x >= 0 && wallrunStartDir < 0) ||
						(Velocity.x <= 0 && wallrunStartDir > 0))
					{
						hangTime = 0;
						wallrunStartDir = 0;
						movementState |= MovementState.Falling;
						movementState ^= MovementState.Wallrunning;
					}
					rb.AddForce(Physics2D.gravity * -1);
					rb.AddForce(new(0, hangTime));
					hangTime -= Time.fixedDeltaTime;
					if (movementState.HasFlag(MovementState.Invincible))
						goto case MovementState.Invincible;
					break;
				}
				case var t when t.HasFlag(MovementState.Falling):
				case MovementState.Falling:
				{
					moveVector = moveVector.IsFinite() ? moveVector : BasicMovement();
					if (collisionState == CollisionState.None || collisionState == CollisionState.BGWall)
					{
						if (TryBoostJumping(moveVector))
							movementState ^= MovementState.Falling;
					}
					else if (collisionState.HasFlag(CollisionState.EnemyTrigger))
					{
						movementState ^= MovementState.Falling;
						EnterGrounded();
						goto case MovementState.Grounded;
					}
					else if (((collisionState.HasFlag(CollisionState.Ground) && CachedVelocity.y < movementSettings.maxSafeFallSpeed) || collisionState.HasFlag(CollisionState.EnemyCollider)) && !movementState.HasFlag(MovementState.Invincible))
					{
						if (input.IsPressed("DownAction"))
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
					//else
					//	if (TryBoostJumping(moveVector))
					//		movementState ^= MovementState.Falling;
					if (movementState.HasFlag(MovementState.Invincible))
						goto case MovementState.Invincible;
					break;
				}
				case var t when t.HasFlag(MovementState.Stumbling):
				case MovementState.Stumbling:
				{
					stumbleTimer -= Time.fixedDeltaTime;
					if (stumbleTimer <= 0f)
					{
						AddUIMessage("Back up!");
						//Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), false);
						invincibleTimer = movementSettings.invincibleTimerLength;
						movementState |= MovementState.Invincible;
						EnterGrounded();
						movementState ^= MovementState.Stumbling;
					}
					break;
				}
				case var t when t.HasFlag(MovementState.Invincible):
				case MovementState.Invincible:
				{
					moveVector = moveVector.IsFinite() ? moveVector : BasicMovement();
					invincibleTimer -= Time.fixedDeltaTime;
					if (invincibleTimer <= 0f || (rb.OverlapCollider(GlobalConstants.EnemyLayer/*enemyLayer*/, enemyCollidersOverlapped) <= 0 && collisionState.HasFlag(CollisionState.Ground)))
					{
						var c = spriteRenderer.color;
						c.a = 1f;
						spriteRenderer.color = c;
						Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), false);
						movementState ^= MovementState.Invincible;
					}
					break;
				}
				case var t when t.HasFlag(MovementState.Rolling):
				case MovementState.Rolling:
				{
					//moveVector = moveVector.IsFinite() ? moveVector : BasicMovement();
					//transform.position += new Vector3(movementSettings.rollSpeed * Time.fixedDeltaTime * (rollRight ? 1 : -1), 0f, 0f);
					rollTimer -= Time.fixedDeltaTime;
					// TODO: Apply DRY to following 2 ifs
					if (collisionState.HasFlag(CollisionState.EnemyCollider))
					{
						MyCapsule.size = colliderInitialDimensions;

						// TODO: Remove when using animations
						//var srb = spriteRenderer.bounds;
						//srb.size = rendererInitialDimensions;//new Vector3(colliderInitialDimensions.x, colliderInitialDimensions.y, srb.size.z);
						//spriteRenderer.bounds = srb;

						movementState ^= MovementState.Rolling;
						EnterStumble();
						break;
					}
					else if (rollTimer <= 0f/* || (rb.OverlapCollider(enemyLayer, enemyCollidersOverlapped) <= 0 && collisionState.HasFlag(CollisionState.Ground))*/)
					{
						rb.velocity = new(movementSettings.rollExitSpeed * (rollRight ? 1 : -1), Velocity.y);

						MyCapsule.size = colliderInitialDimensions;
						
						// TODO: Remove when using animations
						//var srb = spriteRenderer.bounds;
						//srb.size = rendererInitialDimensions;//new Vector3(colliderInitialDimensions.x, colliderInitialDimensions.y, srb.size.z);
						//spriteRenderer.bounds = srb;
						
						//movementState |= MovementState.Grounded;
						movementState ^= MovementState.Rolling;
						break;
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
						
						MyCapsule.size = new(colliderInitialDimensions.x, h);

						// TODO: Remove when using animations
						//var srb = spriteRenderer.bounds;
						//srb.size = new Vector3(colliderInitialDimensions.x, h, rendererInitialDimensions.z);//srb.size.z);
						//spriteRenderer.bounds = srb;
					}
					break;
				}
				case MovementState.None:
				default:
				{
					Debug.LogWarning("Player Movement State Failure, entering Grounded State.");
					EnterGrounded();
					goto case MovementState.Grounded;
				}
			}

			#region After movement is handled
			// Change Cam Size
			myCam.m_Lens.OrthographicSize = cameraSettings.isOrthographicSizeFunctionActive ? 
				cameraSettings.GetNewOrthographicSize(Velocity, transform.position, GlobalConstants.GroundLayer/*groundLayer*/) :
				cameraSettings.defaultOrthographicSize;

			// Update speed display after application of forces.
			UpdateSpeedText();

			// Reset jump/boost inputs
			jumpPressedLastFrame = jumpPressedThisFrame;
			jumpPressedThisFrame = false;
			boostPressedLastFrame = boostPressedThisFrame;
			boostPressedThisFrame = false;

			cachedVelocity = preCollisionVelocity = Velocity;
			#endregion
		}

		#region UI Stuff
		#region Fields
		public TMP_Text speedText;
		public TMP_Text messages;
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

		// TODO: Fade out messages over time.
		public void AddUIMessage(string message)
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
		readonly List<Enemy> currentEnemyCollisions = new(3);
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
	}
}