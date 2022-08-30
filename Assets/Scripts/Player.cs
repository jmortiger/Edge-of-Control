using Cinemachine;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

using Assets.ScriptableObjects;
using Assets.Scripts.Utility;
using System.Collections.Generic;
// TODO: Trim inspector stuff, handled in UIDocument.
// TODO: Modify UIDocument to reflect new fields.
namespace Assets.Scripts
{
	[RequireComponent(
		typeof(Collider2D),
		typeof(PlayerInput),
		typeof(Rigidbody2D)
		)]
	public class Player : MonoBehaviour
	{
		#region Component References
		[SerializeField]
		new Collider2D collider;
		CapsuleCollider2D MyCapsule { get => (CapsuleCollider2D)collider; }
		public PlayerInput input;
		public Rigidbody2D rb;
		[SerializeField]
		CinemachineImpulseSource impulseSource;
		[SerializeField]
		SpriteRenderer spriteRenderer;
		[SerializeField]
		AudioSource aSource;
		[SerializeField]
		new ParticleSystem particleSystem;
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
		#region UI
		[Header("UI")]
		public TMP_Text speedText;
		public TMP_Text messages;
		public TMP_Text scoreText;
		public DamageIndicator damageIndicator;
		#endregion

		public CinemachineVirtualCamera myCam;
		public Camera cam;
		public CameraSettings cameraSettings;

		[ContextMenu("Assign Scene References")]
		void AssignSceneReferences()
		{
			speedText = GameObject.Find("Speed").GetComponent<TMP_Text>();
			messages = GameObject.Find("Messages").GetComponent<TMP_Text>();
			scoreText = GameObject.Find("Score").GetComponent<TMP_Text>();
			damageIndicator = FindObjectOfType<DamageIndicator>();
			myCam = FindObjectOfType<CinemachineVirtualCamera>();
			cam = FindObjectOfType<Camera>();
		}
		public Vector2 Velocity { get => rb.velocity; }
		Vector2 cachedVelocity;
		public Vector2 CachedVelocity { get => cachedVelocity; }
		Vector2 preCollisionVelocity;
		/// <summary>
		/// Updated at the end of FixedUpdate (same as <see cref="CachedVelocity"/>), but also updated in <see cref="OnCollisionEnter2D(Collision2D)"/>.
		/// </summary>
		public Vector2 PreCollisionVelocity { get => preCollisionVelocity; }
		#region SFX
		public AudioClip sfx_Running;
		public AudioClip sfx_Jump;
		public AudioClip sfx_Hurt;
		public AudioClip sfx_Shotgun;
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
		void Start()
		{
			enemyAndGround.SetLayerMask(LayerMask.GetMask(new string[] { "Ground", "Enemy" }));
			enemyLayer.SetLayerMask(LayerMask.GetMask(new string[] { "Enemy" }));
			groundLayer.SetLayerMask(LayerMask.GetMask(new string[] { "Ground" }));

			aSource.clip = sfx_Running;
			aSource.loop = true;

			StartShotgun();

			colliderInitialDimensions = collider.bounds.size;
		}
		#endregion

		#region Movement Settings
		//[Tooltip("The force applied when using the movement actions.")]
		//[Tooltip("\"Endless Runner Mode\". Apply a constant displacement and use a different move force.")]
		//[Tooltip("The constant displacement applied under endless runner mode.")]
		//[Tooltip("The force applied when using the movement actions if endless runner mode is on.")]
		/// <summary>
		/// The force applied when using the movement actions.
		/// </summary>
		public Vector2 moveForce = new(1f, 1f);
		/// <summary>
		/// "Endless Runner Mode". Apply a constant displacement and use a different move force.
		/// </summary>
		public bool isConstantMovementEnabled = false;
		/// <summary>
		/// The constant displacement applied under endless runner mode.
		/// </summary>
		public Vector2 constantMovementDisplacement = new(5f, 0f);
		/// <summary>
		/// The force applied when using the movement actions if endless runner mode is on.
		/// </summary>
		public Vector2 constantMoveForce = new(5f, 1f);

		public float rollSpeed = 3f;
		public float rollTimerLength = 1f;
		public float stumbleTimerLength = 1f;
		public float invincibleTimerLength = 1f;
		#endregion
		#region Jump Checks and Update
		bool jumpPressedLastFrame = false;
		//bool? jumpPressedThisFrame = null;
		bool jumpPressedThisFrame = false;
		//bool JumpPressedOnThisFrame { get => (jumpPressedThisFrame ?? false) && !jumpPressedLastFrame; }
		bool JumpPressedOnThisFrame { get => (jumpPressedThisFrame && (!jumpPressedLastFrame)); }
		void Update()
		{
			//jumpPressedThisFrame = (jumpPressedThisFrame ?? false) || input.IsPressed("Jump");
			jumpPressedThisFrame = jumpPressedThisFrame || input.IsPressed("Jump");
			//Debug.Log($"{jumpPressedLastFrame}");
		}
		#endregion
		#region State
		[Flags]
		public enum MovementState
		{
			None		= 0x0000_0000,
			Grounded	= 0x0000_0001,
			Jumping		= 0x0000_0010,
			Wallrunning	= 0x0000_0100,
			Falling		= 0x0000_1000,
			Stumbling	= 0x0001_0000,
			Invincible	= 0x0010_0000,
			Rolling		= 0x0100_0000,
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
		bool rollRight = true;
		float rollTimer = 0;
		float stumbleTimer = 0;
		float invincibleTimer = 0;
		#endregion
		#region Collision Checking Members
		ContactFilter2D enemyAndGround = new();
		ContactFilter2D enemyLayer = new();
		ContactFilter2D groundLayer = new();
		Collider2D[] enemyCollidersOverlapped = new Collider2D[3];
		#endregion
		[SerializeField] uint jumpPresses = 0;
		public int combo = 0;

		#region RollAnim Fields - controls the collider during the roll based of anim frame timings and dimensions
		public float rollAnim_EntranceLengthRatio = 1f / 3f;
		public float rollAnim_ProperLengthRatio = 1f / 3f;
		public float rollAnim_ExitLengthRatio = 1f / 3f;
		public float rollAnim_MinHeightRatio = .5f;
		#endregion

		// TODO: Expand timer and goalpost
		// TODO: Make aerial movement different from grounded movement.
		// TODO: Troubleshoot landing on enemies problem.
		// TODO: Add combo timer
		// TODO: Tweak wall run
		// TODO: Check for refactors from Velocity to Cached Velocity.
		// TODO: Add coil jump
		void FixedUpdate()
		{
			if (JumpPressedOnThisFrame)
			{
				jumpPresses++;
				Debug.Log($"JumpPressedOnThisFrame");
			}
			//Debug.Log($"Presses: {jumpPresses}");
			#region Methods
			Vector2 BasicMovement()
			{
				var moveVector = input.FindAction("Move").ReadValue<Vector2>();
				moveVector.x = (moveVector.x == 0) ? 0 : ((moveVector.x > 0) ? 1 : -1);
				moveVector.y = (moveVector.y == 0) ? 0 : ((moveVector.y > 0) ? 1 : -1);

				if (isConstantMovementEnabled)
				{
					rb.AddForce(Vector2.Scale(moveVector, constantMoveForce));
					transform.position += (Vector3)constantMovementDisplacement * Time.fixedDeltaTime;
				}
				else
					rb.AddForce(Vector2.Scale(moveVector, moveForce));
				return moveVector;
			}
			void EnterStumble()
			{
				movementState |= MovementState.Stumbling;
				stumbleTimer = stumbleTimerLength;
				var knockback = new Vector2(-1 * /*Velocity*/CachedVelocity.x * .5f, 2);
				rb.velocity = Vector2.zero;
				//rb.velocity = knockback;
				rb.AddForce(knockback, ForceMode2D.Impulse);
				Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), true);
				damageIndicator.Show(stumbleTimerLength);
				impulseSource?.GenerateImpulseAt(transform.position, /*Velocity*/CachedVelocity);
				AddUIMessage("Stumbled...");
				combo = 0;
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
				startLifetime.constant = stumbleTimerLength;
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
				particleSystem.Play(true);
			}
			void EnterRolling()
			{
				movementState |= MovementState.Rolling;
				particleSystem.Play(true);
				rollTimer = rollTimerLength;
				rollRight = Velocity.x > 0;
				AddUIMessage("Successful PK Roll");
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
						// TODO: If fall speed too high, force to roll
						if (collisionState.HasFlag(CollisionState.EnemyTrigger) && Velocity.y <= 0)
						{
							var vel = Velocity;
							vel.y *= -1;
							rb.velocity = vel;
							rb.velocity *= conservedVelocity;

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
						}
						var fApplied = Vector2.Scale(moveVector, jumpForce);
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
						//movementState |= MovementState.Stumbling;
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
						var vel = Velocity;
						hangTime = Mathf.Abs(vel.x * Mathf.Sin(wallRunAngle));
						wallrunStartDir = (vel.x > 0) ? 1 : -1;
						vel.x *= Mathf.Cos(wallRunAngle);
						rb.velocity = vel;
						rb.AddForce(new(0, hangTime));
						movementState |= MovementState.Wallrunning;
						movementState ^= MovementState.Jumping;
					}
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
						break;
					else if (collisionState.HasFlag(CollisionState.EnemyTrigger))
					{
						//movementState |= MovementState.Grounded;
						movementState ^= MovementState.Falling;
						EnterGrounded();
						goto case MovementState.Grounded;
					}
					else if (((collisionState.HasFlag(CollisionState.Ground) && /*Velocity.y*/CachedVelocity.y < maxSafeFallSpeed) || collisionState.HasFlag(CollisionState.EnemyCollider)) && !movementState.HasFlag(MovementState.Invincible))
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
						//Debug.Log($"Velocity.y {Velocity.y}");
						//movementState |= MovementState.Grounded;
						movementState ^= MovementState.Falling;
						EnterGrounded();
					}
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
						//movementState |= MovementState.Grounded;
						invincibleTimer = invincibleTimerLength;
						movementState |= MovementState.Invincible;
						//movementState |= MovementState.Grounded;
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
					if (invincibleTimer <= 0f || (rb.OverlapCollider(enemyLayer, enemyCollidersOverlapped) <= 0 && collisionState.HasFlag(CollisionState.Ground)))
					{
						//AddUIMessage("Back up!");
						var c = spriteRenderer.color;
						c.a = 1f;
						spriteRenderer.color = c;
						Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Enemy"), false);
						//movementState |= MovementState.Grounded;
						movementState ^= MovementState.Invincible;
					}
					break;
				}
				case var t when t.HasFlag(MovementState.Rolling):
				case MovementState.Rolling:
				{
					//moveVector = moveVector.IsFinite() ? moveVector : BasicMovement();
					transform.position += new Vector3(rollSpeed * Time.fixedDeltaTime * (rollRight ? 1 : -1), 0f, 0f);
					rollTimer -= Time.fixedDeltaTime;
					if (collisionState.HasFlag(CollisionState.EnemyCollider))
					{
						MyCapsule.size = colliderInitialDimensions;

						// TODO: Remove when using animations
						var srb = spriteRenderer.bounds;
						srb.size = new Vector3(colliderInitialDimensions.x, colliderInitialDimensions.y, srb.size.z);
						spriteRenderer.bounds = srb;

						movementState ^= MovementState.Rolling;
						EnterStumble();
						break;
					}
					else if (rollTimer <= 0f/* || (rb.OverlapCollider(enemyLayer, enemyCollidersOverlapped) <= 0 && collisionState.HasFlag(CollisionState.Ground))*/)
					{
						MyCapsule.size = colliderInitialDimensions;
						
						// TODO: Remove when using animations
						var srb = spriteRenderer.bounds;
						srb.size = new Vector3(colliderInitialDimensions.x, colliderInitialDimensions.y, srb.size.z);
						spriteRenderer.bounds = srb;
						
						//movementState |= MovementState.Grounded;
						movementState ^= MovementState.Rolling;
						break;
					}
					else
					{
						rollTimer = (rollTimer < 0) ? 0 : rollTimer;
						var h = (rollTimer > rollTimerLength / 2) ? 
							Mathf.SmoothStep(colliderInitialDimensions.y * rollAnim_MinHeightRatio, colliderInitialDimensions.y, (rollTimer / (rollTimerLength / 2)) - 1) : 
							Mathf.SmoothStep(colliderInitialDimensions.y, colliderInitialDimensions.y * rollAnim_MinHeightRatio, rollTimer / (rollTimerLength / 2));
						
						MyCapsule.size = new(colliderInitialDimensions.x, h);

						// TODO: Remove when using animations
						var srb = spriteRenderer.bounds;
						srb.size = new Vector3(colliderInitialDimensions.x, h, srb.size.z);
						spriteRenderer.bounds = srb;
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
				cameraSettings.GetNewOrthographicSize(Velocity, transform.position, groundLayer) :
				cameraSettings.defaultOrthographicSize;

			// Update speed display after application of forces.
			UpdateSpeedText();

			// Reset jump inputs
			//jumpPressedLastFrame = jumpPressedThisFrame ?? false;
			//jumpPressedThisFrame = null;
			jumpPressedLastFrame = jumpPressedThisFrame;
			jumpPressedThisFrame = false;

			UpdateShotgun();

			cachedVelocity = preCollisionVelocity = Velocity;
			#endregion
		}

		#region UI Elements
		const float WARNING_RANGE = .25f;
		//bool showY = true;
		//short flipShowYCounter = 0;
		void UpdateSpeedText()
		{
			speedText.text = $" X: {Velocity.x} u/s\r\n ";
			if (Velocity.y < maxSafeFallSpeed)
			{
				//flipShowYCounter++;
				//showY ^= (flipShowYCounter >= 8);
				//if (showY)
				speedText.text += $"<color=\"red\">Y: {Velocity.y} u/s";
			}
			// Transitions from white to red as you approach the max safe fall speed
			else if (Velocity.y < (maxSafeFallSpeed * (1f - WARNING_RANGE)))
			{
				var velAdjusted = Velocity.y - maxSafeFallSpeed * (1f - WARNING_RANGE);
				var maxAdjusted = maxSafeFallSpeed * WARNING_RANGE;
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

		#region Score
		public uint score = 0;
		public void UpdateScore(int change)
		{
			if (change < 0)
				score -= (score < -change) ? 0 - score : (uint)-change;
			else
				score += (uint)change;
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
		#endregion
		#endregion

		#region Shotgun
		public float fireRateLength = 1f;
		float fireRate = 0f;
		public GameObject pellet;
		//public Vector2 projectileSpawnOffset = new(.5f, 0);
		public float sprayRangeDegrees = 15f;
		public int shotgunPelletNumber = 5;
		GameObject[] pellets;
		Vector3[] pelletsQueuedPositions;
		Material[] pelletTrailMats;
		// TODO: Extract Shotgun
		public void OnFire(InputAction.CallbackContext cc)
		{
			if (fireRate <= 0)
			{
				aSource.PlayOneShot(sfx_Shotgun);
				fireRate = fireRateLength;
				//var t = projectileSpawnOffset;
				//t.x *= (Velocity.x != 0) ? (Velocity.x > 0 ? 1 : -1) : 0;
				//t.y *= (Velocity.y != 0) ? (Velocity.y > 0 ? 1 : -1) : 0;
				var aimDirection = input.GetActionValueAsJoystick("Aim", transform.position);
				if (aimDirection.magnitude == 0)
					aimDirection = Vector2.right;
				//projectile.GetComponent<TrailRenderer>().time = fireRateLength;
				for (int i = 0; i < pellets.Length; i++)
				{
					pellets[i].transform.position = (Vector3)aimDirection + transform.position;
					var tr = pellets[i].GetComponent<TrailRenderer>();
					tr.time = fireRateLength;
					tr.emitting = tr.enabled = true;
					pellets[i].SetActive(true);
					var angleOffset = (Random.value * sprayRangeDegrees) - (sprayRangeDegrees / 2f);
					Debug.Log($"angleOffset[{i}]: {angleOffset}");
					var pelletDir = (Quaternion.Euler(0, 0, angleOffset) * aimDirection).normalized;
					var raycastHit = new RaycastHit2D[2];
					var raycastHitNum = Physics2D.Raycast(pellets[i].transform.position, pelletDir, enemyAndGround, raycastHit);
					if (raycastHitNum <= 0)
						/*pellets[i].transform.position*/pelletsQueuedPositions[i] = pellets[i].transform.position + pelletDir * cam.OrthographicBoundsByScreen().size.x;
					else
					{
						/*pellets[i].transform.position*/pelletsQueuedPositions[i] = raycastHit[0].point + (Vector2)pelletDir;
						if (raycastHit[0].collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
						{
							UpdateScore(50);
							raycastHit[0].collider.GetComponent<Enemy>().KillEnemy();
						}
					}
				}
			}
		}
		void StartShotgun()
		{
			pelletsQueuedPositions = new Vector3[shotgunPelletNumber];
			for (int i = 0; i < pelletsQueuedPositions.Length; i++)
				pelletsQueuedPositions[i] = Vector3.positiveInfinity;
			pelletTrailMats = new Material[shotgunPelletNumber];
			pellets = new GameObject[shotgunPelletNumber];
			for (int i = 0; i < pellets.Length; i++)
			{
				pellets[i] = Instantiate(pellet, Vector3.zero, Quaternion.identity);
				//UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(pellets[i], UnityEngine.SceneManagement.SceneManager.GetSceneByName("GameSceneAdditive"));
				FindObjectOfType<MySceneManager>().MoveToScene(pellets[i], "GameSceneAdditive");
				var tr = pellets[i].GetComponent<TrailRenderer>();
				tr.time = fireRateLength;
				tr.emitting = tr.enabled = false;
				tr.material.color = Color.white;
				pelletTrailMats[i] = tr.material;
				pellets[i].SetActive(false);
			}
		}
		void UpdateShotgun()
		{
			if (fireRate > 0)
			{
				for (int i = 0; i < pellets.Length; i++)
				{
					if (pelletsQueuedPositions[i].IsFinite())
					{
						pellets[i].transform.position = pelletsQueuedPositions[i];
						pelletsQueuedPositions[i] = Vector3.positiveInfinity;
					}
					//var c = pelletTrailMats[i].color;
					var c = pellets[i].GetComponent<TrailRenderer>().material.color;
					c.a = Mathf.SmoothStep(0, 1, fireRate / fireRateLength);
					//pelletTrailMats[i].color = c;
					pellets[i].GetComponent<TrailRenderer>().material.color = c;
					pellets[i].GetComponent<SpriteRenderer>().color = c;
				}
				fireRate -= Time.fixedDeltaTime;
				if (fireRate <= 0)
					ResetPellets();
			}
		}
		void ResetPellets()
		{
			for (int i = 0; i < pellets.Length; i++)
			{
				var tr = pellets[i].GetComponent<TrailRenderer>();
				tr.time = fireRateLength;
				tr.emitting = tr.enabled = false;
				tr.material.color = Color.white;
				pellets[i].SetActive(false);
			}
		}
		#endregion

		// TODO: Have health and refactor damage
		//public float health = 100f;
		//public void OnTakeDamage(float damage = 10f)
		//{
		//	health 
		//}

		void OnRenderObject()
		{
			DrawPositionIndicator();
			DrawAimingIndicators(true);
		}

		void DrawAimingIndicators(bool showSpreadRange)
		{
#if UNITY_EDITOR
			MyGizmos.DrawLine3D(transform.position, transform.position + (Vector3)input.GetRightStickOrMouseValueAsJoystickEditor(transform.position) * 5f, Color.red);
#else
			MyGizmos.DrawLine3D(transform.position, transform.position + (Vector3)input.GetActionValueAsJoystick("Aim", transform.position));
#endif

			if (showSpreadRange)
			{
#if UNITY_EDITOR
				var aimDirection = input.GetRightStickOrMouseValueAsJoystickEditor(transform.position);
				MyGizmos.DrawLine3D(transform.position, transform.position + Quaternion.Euler(0, 0, sprayRangeDegrees / 2f) * aimDirection * 10f);
				MyGizmos.DrawLine3D(transform.position, transform.position + Quaternion.Euler(0, 0, -sprayRangeDegrees / 2f) * aimDirection * 10f);
#else
				var aimDirection = input.GetActionValueAsJoystick("Aim", transform.position);
				MyGizmos.DrawLine3D(transform.position, transform.position + Quaternion.Euler(0, 0, sprayRangeDegrees / 2f) * aimDirection * 10f);
				MyGizmos.DrawLine3D(transform.position, transform.position + Quaternion.Euler(0, 0, -sprayRangeDegrees / 2f) * aimDirection * 10f);
#endif
			}
		}

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

		#region Collision Stuff
		List<Enemy> currentEnemyCollisions = new List<Enemy>(3);
		#endregion
		#region Jump Stuff
		public Vector2 jumpForce = new(.5f, 15f);
		public float maxSafeFallSpeed = -20f;
		[Tooltip("The amount of velocity conserved when jumping off enemies' heads.")]
		[Range(0, 1)]
		public float conservedVelocity = .8f;
		[Tooltip("The angle of attack, in degrees, for wall runs. Determines loss of horizontal speed and hang time.")]
		[Range(0, 90)]
		public float wallRunAngle = 5f;
		private float hangTime = 1f;
		private float wallrunStartDir = 0;
		#endregion

		// TODO: Modify to use switches
		#region OnCollision
		// TODO: Resolve bad wall jumping (check collider.max against collided.min and vice versa)
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
			//else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
			//	collisionState |= CollisionState.Ground;
			else if (collision.gameObject.layer == LayerMask.NameToLayer("Goal"))
				Debug.Log($"Finished with time of {Time.timeSinceLevelLoad}");
		}
		void OnTriggerStay2D(Collider2D collision)
		{
			if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
				collisionState |= CollisionState.EnemyTrigger;
			else if (collision.gameObject.layer == LayerMask.NameToLayer("BGInteractable"))
				collisionState |= CollisionState.BGWall;
			//else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
			//	collisionState |= CollisionState.Ground;
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
			//else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
			//{
			//	collisionState |= CollisionState.Ground;
			//	collisionState ^= CollisionState.Ground;
			//}
		}
		#endregion
	}
}