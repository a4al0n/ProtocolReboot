using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Weapon : Colliderable
{
    [Header("------ Damage ------")]
    public int[] damagePoint = { 1, 2, 3, 4, 5, 6, 7 };
    public float[] pushForce = { 2.0f, 2.2f, 2.5f, 3.0f, 3.3f, 3.6f, 4.0f };

    [Header("------ WeaponLevel ------")]
    public int weaponLevel = 0;
    private SpriteRenderer SpriteRenderer;

    [Header("------ Swing ------")]
    public Animator animator;
    private float swingCoolDown = 0.4f;
    private float lastSwing;

    [Header("------ Rage ------")]
    public GameObject flamingSword;
    public GameObject rageState;
    public bool CanRageSkill = false;
    public bool raging = false;
    public float ragingTime = 4f;

    private Player player;
    private PhotonView playerView;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        if (rageState != null)
            rageState.SetActive(false);

        InitializePlayerComponents();
    }

    private void InitializePlayerComponents()
    {
        if (player == null)
        {
            player = GetComponentInParent<Player>();
            if (player != null)
                playerView = player.GetComponent<PhotonView>();
        }
    }

    protected override void Update()
    {
        InitializePlayerComponents();

        // base.Update() вызываем ВСЕГДА — коллизии должны работать независимо
        base.Update();

        // Дальше только логика оружия — только для локального живого игрока
        if (GameManager.instance == null || player == null || !player.isAlive)
            return;

        if (playerView != null && !playerView.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time - lastSwing > swingCoolDown)
            {
                lastSwing = Time.time;
                Swing();

                if (raging)
                    CreateFlamingSword();
                else if (rageState != null)
                    rageState.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !raging)
        {
            if (CanRageSkill)
            {
                raging = true;
                if (rageState != null)
                    rageState.SetActive(true);
                StartCoroutine("WaitingForRestRageSkill");
            }
        }
    }

    protected override void OnCollide(Collider2D coll)
    {
        if (playerView != null && !playerView.IsMine) return;

        // Пропускаем самого игрока — владельца оружия
        if (coll.GetComponent<Player>() != null ||
            coll.GetComponentInParent<Player>() != null) return;

        // Бьём только объекты с компонентом Fighter (враги, боссы)
        Fighter fighter = coll.GetComponent<Fighter>();
        if (fighter == null) fighter = coll.GetComponentInParent<Fighter>();
        if (fighter == null) return;

        Damag dmg = new Damag
        {
            damageAmount = damagePoint[weaponLevel],
            origin = transform.position,
            pushForce = pushForce[weaponLevel]
        };

        PhotonView targetPv = coll.GetComponent<PhotonView>();
        if (targetPv == null) targetPv = coll.GetComponentInParent<PhotonView>();

        if (targetPv != null && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            targetPv.RPC("RPC_NetworkTakeDamage", RpcTarget.All,
                dmg.damageAmount, dmg.origin, dmg.pushForce);
        }
        else
        {
            coll.SendMessage("ReceiveDamage", dmg, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void Swing()
    {
        if (animator != null)
            animator.SetTrigger("Swing");
    }

    private void CreateFlamingSword()
    {
        if (flamingSword != null)
            Instantiate(flamingSword);
    }

    public void UpgradeWeapon()
    {
        weaponLevel++;
        if (SpriteRenderer != null && GameManager.instance != null &&
            GameManager.instance.weaponSprites != null &&
            weaponLevel < GameManager.instance.weaponSprites.Count)
        {
            SpriteRenderer.sprite = GameManager.instance.weaponSprites[weaponLevel];
        }

        if (playerView != null && playerView.IsMine)
            playerView.RPC("RPC_SyncWeaponLevel", RpcTarget.OthersBuffered, weaponLevel);
    }

    public void SetWeaponLevel(int level)
    {
        weaponLevel = level;
        if (SpriteRenderer != null && GameManager.instance != null &&
            GameManager.instance.weaponSprites != null &&
            level < GameManager.instance.weaponSprites.Count)
        {
            SpriteRenderer.sprite = GameManager.instance.weaponSprites[weaponLevel];
        }

        if (playerView != null && playerView.IsMine)
            playerView.RPC("RPC_SyncWeaponLevel", RpcTarget.OthersBuffered, weaponLevel);
    }

    IEnumerator WaitingForRestRageSkill()
    {
        yield return new WaitForSeconds(ragingTime);
        raging = false;
        CanRageSkill = false;
        if (player != null)
            player.rage = 0;
        if (GameManager.instance != null)
            GameManager.instance.OnUIChange();
    }

    public void EnableWeaponCollider()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = true;
    }

    public void DisableWeaponCollider()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = false;
    }
}