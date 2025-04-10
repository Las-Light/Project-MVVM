using System.Collections;
using NothingBehind.Scripts.Game.Gameplay.Logic.WeaponSystem;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;
using UnityEngine.Pool;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Weapons
{
    public class WeaponView : MonoBehaviour
    {
        public WeaponType WeaponType;
        public int CurrentAmmo;

        private AudioSource ShootingAudioSource;
        private GameObject Model;
        private float LastShootTime;
        private float InitialClickTime;
        private float StopShootingTime;

        private ParticleSystem ShootSystem;
        private ObjectPool<TrailRenderer> TrailPool;

        private ObjectPool<Bullet> BulletPool;

        private bool LastFrameWantedToShoot;

        private WeaponViewModel _viewModel;


        public void Bind(WeaponViewModel viewModel)
        {
            _viewModel = viewModel;
            WeaponType = viewModel.WeaponType;
            CurrentAmmo = viewModel.FeedSystem.MagazinesItem.Value.Magazines.CurrentAmmo.Value;
        }

        /// <summary>
        /// Spawns the Gun Model into the scene
        /// </summary>
        /// <param name="parent">Parent for the gun model</param>
        public void Spawn()
        {
            TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);
            if (!_viewModel.Shoot.IsHitscan)
            {
                BulletPool = new ObjectPool<Bullet>(CreateBullet);
            }

            Model = Instantiate(_viewModel.ModelPrefab, transform);
            Model.transform.localPosition = _viewModel.SpawnPoint;
            Model.transform.localRotation = Quaternion.Euler(_viewModel.SpawnRotation);

            ShootingAudioSource = Model.GetComponent<AudioSource>();
            ShootSystem = Model.GetComponentInChildren<ParticleSystem>();

            if (_viewModel.Knockback.KnockbackStrength.Value > 0)
            {
                // ICollisionHandler[] currentHandlers = BulletImpactEffects;
                // BulletImpactEffects = new ICollisionHandler[currentHandlers.Length + 1];
                // Array.Copy(currentHandlers, BulletImpactEffects, currentHandlers.Length);
                // BulletImpactEffects[^1] = new Knockback();
            }
        }

        /// <summary>
        /// Despawns the active gameobjects and cleans up pools.
        /// </summary>
        public void Despawn()
        {
            // We do a bunch of other stuff on the same frame, so we really want it to be immediately destroyed, not at Unity's convenience.
            Model.SetActive(false);
            Destroy(Model);
            TrailPool.Clear();
            if (BulletPool != null)
            {
                BulletPool.Clear();
            }

            ShootingAudioSource = null;
            ShootSystem = null;
        }

        /// <summary>
        /// Expected to be called every frame
        /// </summary>
        /// <param name="WantsToShoot">Whether or not the player is trying to shoot</param>
        public void Tick(bool WantsToShoot)
        {
            Model.transform.localRotation = Quaternion.Lerp(
                Model.transform.localRotation,
                Quaternion.Euler(_viewModel.SpawnRotation),
                Time.deltaTime * _viewModel.Shoot.RecoilRecoverySpeed.Value
            );

            if (WantsToShoot)
            {
                LastFrameWantedToShoot = true;
                TryToShoot();
            }

            if (!WantsToShoot && LastFrameWantedToShoot)
            {
                StopShootingTime = Time.time;
                LastFrameWantedToShoot = false;
            }
        }

        /// <summary>
        /// Plays the reloading audio clip if assigned.
        /// Expected to be called on the first frame that reloading begins
        /// </summary>
        public void StartReloading()
        {
            _viewModel.AudioWeapon.PlayReloadClip(ShootingAudioSource);
        }

        //TODO: сделать AmmoView???
        /// <summary>
        /// Handle ammo after a reload animation.
        /// ScriptableObjects can't catch Animation Events, which is how we're determining when the
        /// reload has completed, instead of using a timer
        /// </summary>
        public void EndReload()
        {
            _viewModel.Reload();
        }

        /// <summary>
        /// Whether or not this gun can be reloaded
        /// </summary>
        /// <returns>Whether or not this gun can be reloaded</returns>
        public bool CanReload()
        {
            return _viewModel.CanReload();
        }

        /// <summary>
        /// Performs the shooting raycast if possible based on gun rate of fire. Also applies bullet spread and plays sound effects based on the AudioConfig.
        /// </summary>
        private void TryToShoot()
        {
            if (Time.time - LastShootTime - _viewModel.Shoot.FireRate.Value > Time.deltaTime)
            {
                float lastDuration = Mathf.Clamp(
                    0,
                    (StopShootingTime - InitialClickTime),
                    _viewModel.Shoot.MaxSpreadTime.Value
                );
                float lerpTime = (_viewModel.Shoot.RecoilRecoverySpeed.Value - (Time.time - StopShootingTime))
                                 / _viewModel.Shoot.RecoilRecoverySpeed.Value;

                InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
            }

            if (Time.time > _viewModel.Shoot.FireRate.Value + LastShootTime)
            {
                LastShootTime = Time.time;
                if (_viewModel.FeedSystem.MagazinesItem.Value.Magazines.CurrentAmmo.Value == 0)
                {
                    _viewModel.AudioWeapon.PlayOutOfAmmoClip(ShootingAudioSource);
                    return;
                }

                ShootSystem.Play();
                _viewModel.AudioWeapon.PlayShootingClip(ShootingAudioSource,
                    _viewModel.FeedSystem.MagazinesItem.Value.Magazines.CurrentAmmo.Value == 1);

                _viewModel.FeedSystem.MagazinesItem.Value.Magazines.CurrentAmmo.Value--;

                for (int i = 0; i < _viewModel.Shoot.BulletsPerShot.Value; i++)
                {
                    Vector3 spreadAmount = _viewModel.Shoot.GetSpread(Time.time - InitialClickTime);

                    Vector3 shootDirection = Vector3.zero;

                    if (Time.time - InitialClickTime <= 0.05f)
                    {
                        Model.transform.localRotation = Quaternion.Euler(_viewModel.SpawnRotation);
                    }
                    else
                    {
                        Model.transform.forward += Model.transform.TransformDirection(spreadAmount);
                    }

                    if (_viewModel.Shoot.ShootType == ShootType.FromGun)
                    {
                        shootDirection = ShootSystem.transform.forward;
                    }

                    if (_viewModel.Shoot.IsHitscan)
                    {
                        DoHitscanShoot(shootDirection, GetRaycastOrigin(), ShootSystem.transform.position);
                    }
                    else
                    {
                        DoProjectileShoot(shootDirection);
                    }
                }
            }
        }

        /// <summary>
        /// Generates a live Bullet instance that is launched in the <paramref name="ShootDirection"/> direction
        /// with velocity from <see cref="ShootConfigScriptableObject.BulletSpawnForce"/>.
        /// </summary>
        /// <param name="ShootDirection"></param>
        private void DoProjectileShoot(Vector3 ShootDirection)
        {
            Bullet bullet = BulletPool.Get();
            bullet.gameObject.SetActive(true);
            bullet.OnCollision += HandleBulletCollision;

            bullet.transform.position = ShootSystem.transform.position;
            bullet.Spawn(ShootDirection * _viewModel.Shoot.BulletSpawnForce.Value);

            TrailRenderer trail = TrailPool.Get();
            if (trail != null)
            {
                trail.transform.SetParent(bullet.transform, false);
                trail.transform.localPosition = Vector3.zero;
                trail.emitting = true;
                trail.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Performs a Raycast to determine if a shot hits something. Spawns a TrailRenderer
        /// and will apply impact effects and damage after the TrailRenderer simulates moving to the
        /// hit point. 
        /// See <see cref="PlayTrail(Vector3, Vector3, RaycastHit)"/> for impact logic.
        /// </summary>
        /// <param name="ShootDirection"></param>
        private void DoHitscanShoot(Vector3 ShootDirection, Vector3 Origin, Vector3 TrailOrigin, int Iteration = 0)
        {
            if (Physics.Raycast(
                    Origin,
                    ShootDirection,
                    out RaycastHit hit,
                    float.MaxValue,
                    _viewModel.Shoot.HitMask
                ))
            {
                StartCoroutine(
                    PlayTrail(TrailOrigin, hit.point, hit, Iteration));
            }
            else
            {
                StartCoroutine(
                    PlayTrail(
                        TrailOrigin,
                        TrailOrigin + (ShootDirection * _viewModel.Trail.MissDistance),
                        new RaycastHit(),
                        Iteration
                    )
                );
            }
        }

        /// <summary>
        /// Returns the proper Origin point for raycasting based on <see cref="ShootConfigScriptableObject.ShootType"/>
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRaycastOrigin()
        {
            Vector3 origin = ShootSystem.transform.position;
            return origin;
        }

        /// <summary>
        /// Returns the forward of the spawned gun model
        /// </summary>
        /// <returns></returns>
        public Vector3 GetGunForward()
        {
            return Model.transform.forward;
        }

        /// <summary>
        /// Plays a bullet trail/tracer from start/end point. 
        /// If <paramref name="Hit"/> is not an empty hit, it will also play an impact using the <see cref="SurfaceManager"/>.
        /// </summary>
        /// <param name="StartPoint">Starting point for the trail</param>
        /// <param name="EndPoint">Ending point for the trail</param>
        /// <param name="Hit">The hit object. If nothing is hit, simply pass new RaycastHit()</param>
        /// <returns>Coroutine</returns>
        private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit, int Iteration = 0)
        {
            TrailRenderer instance = TrailPool.Get();
            instance.gameObject.SetActive(true);
            instance.transform.position = StartPoint;
            yield return null; // avoid position carry-over from last frame if reused

            instance.emitting = true;

            float distance = Vector3.Distance(StartPoint, EndPoint);
            float remainingDistance = distance;
            while (remainingDistance > 0)
            {
                instance.transform.position = Vector3.Lerp(
                    StartPoint,
                    EndPoint,
                    Mathf.Clamp01(1 - (remainingDistance / distance))
                );
                remainingDistance -= _viewModel.Trail.SimulationSpeed * Time.deltaTime;

                yield return null;
            }

            instance.transform.position = EndPoint;

            if (Hit.collider != null)
            {
                //HandleBulletImpact(distance, EndPoint, Hit.normal, Hit.collider, Iteration);
            }

            yield return new WaitForSeconds(_viewModel.Trail.Duration);
            yield return null;
            instance.emitting = false;
            instance.gameObject.SetActive(false);
            TrailPool.Release(instance);

            if (_viewModel.BulletPenetration != null &&
                _viewModel.BulletPenetration.MaxObjectsToPenetrate.Value > Iteration)
            {
                yield return null;
                Vector3 direction = (EndPoint - StartPoint).normalized;
                Vector3 backCastOrigin =
                    Hit.point + direction * _viewModel.BulletPenetration.MaxPenetrationDepth.Value;

                if (Physics.Raycast(
                        backCastOrigin,
                        -direction,
                        out RaycastHit hit,
                        _viewModel.BulletPenetration.MaxPenetrationDepth.Value,
                        _viewModel.Shoot.HitMask
                    ))
                {
                    Vector3 penetrationOrigin = hit.point;
                    direction += new Vector3(
                        Random.Range(-_viewModel.BulletPenetration.AccuracyLoss.Value.x,
                            _viewModel.BulletPenetration.AccuracyLoss.Value.x),
                        Random.Range(-_viewModel.BulletPenetration.AccuracyLoss.Value.y,
                            _viewModel.BulletPenetration.AccuracyLoss.Value.y),
                        Random.Range(-_viewModel.BulletPenetration.AccuracyLoss.Value.z,
                            _viewModel.BulletPenetration.AccuracyLoss.Value.z)
                    );

                    DoHitscanShoot(direction, penetrationOrigin, penetrationOrigin, Iteration + 1);
                }
            }
        }

        /// <summary>
        /// Callback handler for <see cref="Bullet.OnCollision"/>. Disables TrailRenderer, releases the 
        /// Bullet from the BulletPool, and applies impact effects if <paramref name="collision"/> is not null.
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="collision"></param>
        /// <param name="objectsPenetrated"></param>
        private void HandleBulletCollision(Bullet bullet, Collision collision, int objectsPenetrated)
        {
            TrailRenderer trail = bullet.GetComponentInChildren<TrailRenderer>();

            if (collision != null && _viewModel.BulletPenetration != null &&
                _viewModel.BulletPenetration.MaxObjectsToPenetrate.Value > objectsPenetrated)
            {
                Vector3 direction = (bullet.transform.position - bullet.SpawnLocation).normalized;
                ContactPoint contact = collision.GetContact(0);
                Vector3 backCastOrigin = contact.point +
                                         direction * _viewModel.BulletPenetration.MaxPenetrationDepth.Value;

                if (Physics.Raycast(
                        backCastOrigin,
                        -direction,
                        out RaycastHit hit,
                        _viewModel.BulletPenetration.MaxPenetrationDepth.Value,
                        _viewModel.Shoot.HitMask
                    ))
                {
                    direction += new Vector3(
                        Random.Range(-_viewModel.BulletPenetration.AccuracyLoss.Value.x,
                            _viewModel.BulletPenetration.AccuracyLoss.Value.x),
                        Random.Range(-_viewModel.BulletPenetration.AccuracyLoss.Value.y,
                            _viewModel.BulletPenetration.AccuracyLoss.Value.y),
                        Random.Range(-_viewModel.BulletPenetration.AccuracyLoss.Value.z,
                            _viewModel.BulletPenetration.AccuracyLoss.Value.z)
                    );
                    bullet.transform.position = hit.point + direction * 0.01f;

                    bullet.Rigidbody.linearVelocity = bullet.SpawnVelocity - direction;
                }
                else
                {
                    DisableTrailAndBullet(trail, bullet);
                }
            }
            else
            {
                DisableTrailAndBullet(trail, bullet);
            }

            if (collision != null)
            {
                ContactPoint contactPoint = collision.GetContact(0);

                // HandleBulletImpact(
                //     Vector3.Distance(contactPoint.point, Bullet.SpawnLocation),
                //     contactPoint.point,
                //     contactPoint.normal,
                //     contactPoint.otherCollider,
                //     ObjectsPenetrated
                // );
            }
        }

        private void DisableTrailAndBullet(TrailRenderer trail, Bullet bullet)
        {
            if (trail != null)
            {
                trail.transform.SetParent(null, true);
                StartCoroutine(DelayedDisableTrail(trail));
            }

            bullet.gameObject.SetActive(false);
            BulletPool.Release(bullet);
        }

        /// <summary>
        /// Disables the trail renderer based on the <see cref="TrailConfigScriptableObject.Duration"/> provided
        ///and releases it from the<see cref="TrailPool"/>
        /// </summary>
        /// <param name="trail"></param>
        /// <returns></returns>
        private IEnumerator DelayedDisableTrail(TrailRenderer trail)
        {
            yield return new WaitForSeconds(_viewModel.Trail.Duration);
            yield return null;
            trail.emitting = false;
            trail.gameObject.SetActive(false);
            TrailPool.Release(trail);
        }


        /// <summary>
        /// Calls <see cref="SurfaceManager.HandleImpact(GameObject, Vector3, Vector3, ImpactType, int)"/> and applies damage
        /// if a damagable object was hit
        /// </summary>
        /// <param name="DistanceTravelled"></param>
        /// <param name="HitLocation"></param>
        /// <param name="HitNormal"></param>
        /// <param name="HitCollider"></param>
        // private void HandleBulletImpact(
        //     float DistanceTravelled,
        //     Vector3 HitLocation,
        //     Vector3 HitNormal,
        //     Collider HitCollider,
        //     int ObjectsPenetrated = 0)
        // {
        //     SurfaceManager.Instance.HandleImpact(
        //         HitCollider.gameObject,
        //         HitLocation,
        //         HitNormal,
        //         ImpactType,
        //         0
        //     );
        //
        //     if (HitCollider.TryGetComponent(out IDamageable damageable))
        //     {
        //         float maxPercentDamage = 1;
        //         if (BulletPenConfig != null && ObjectsPenetrated > 0)
        //         {
        //             for (int i = 0; i < ObjectsPenetrated; i++)
        //             {
        //                 maxPercentDamage *= BulletPenConfig.DamageRetentionPercentage;
        //             }
        //         }
        //
        //         damageable.TakeDamage(DamageConfig.GetDamage(DistanceTravelled, maxPercentDamage));
        //     }
        //
        //     foreach (ICollisionHandler collisionHandler in BulletImpactEffects)
        //     {
        //         collisionHandler.HandleImpact(HitCollider, HitLocation, HitNormal, DistanceTravelled, this);
        //     }
        // }

        /// <summary>
        /// Creates a trail Renderer for use in the object pool.
        /// </summary>
        /// <returns>A live TrailRenderer GameObject</returns>
        private TrailRenderer CreateTrail()
        {
            GameObject instance = new GameObject("Bullet Trail");
            TrailRenderer trail = instance.AddComponent<TrailRenderer>();
            trail.colorGradient = _viewModel.Trail.Color;
            trail.material = _viewModel.Trail.Material;
            trail.widthCurve = _viewModel.Trail.WidthCurve;
            trail.time = _viewModel.Trail.Duration;
            trail.minVertexDistance = _viewModel.Trail.MinVertexDistance;

            trail.emitting = false;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return trail;
        }

        //TODO: Может вес брать из Аммо???


        /// <summary>
        /// Creates a Bullet for use in the object pool.
        /// </summary>
        /// <returns>A live Bullet GameObject</returns>
        private Bullet CreateBullet()
        {
            Bullet bullet = Instantiate(_viewModel.Shoot.BulletPrefab);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.mass = _viewModel.Shoot.BulletWeight.Value;

            return bullet;
        }
    }
}