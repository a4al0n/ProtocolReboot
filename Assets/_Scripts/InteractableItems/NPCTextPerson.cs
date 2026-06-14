using UnityEngine;
using Photon.Pun;

public class NPCTextPerson : Colliderable
{
    public string[] messages;
    private int msgNow = 0;

    public float showTime;
    public float coolDown = 4.0f;
    private float lastShout;

    public bool canLookAtPlayer = false;
    private float posDelta;

    protected override void Start()
    {
        base.Start();
        lastShout = -coolDown;
    }

    protected override void Update()
    {
        base.Update();

        if (canLookAtPlayer)
        {
            if (GameManager.instance == null || GameManager.instance.player == null) return;

            posDelta = GameManager.instance.player.transform.position.x - transform.position.x;
            if (posDelta > 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (posDelta < 0)
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    protected override void OnCollide(Collider2D coll)
    {
        // Ищем Player по компоненту вместо тега
        Player playerComponent = coll.GetComponent<Player>();
        if (playerComponent == null)
            playerComponent = coll.GetComponentInParent<Player>();
        if (playerComponent == null) return;

        PhotonView pv = coll.GetComponent<PhotonView>();
        if (pv == null) pv = coll.GetComponentInParent<PhotonView>();

        bool isLocalPlayer = (pv != null && pv.IsMine) || !PhotonNetwork.IsConnected;
        if (!isLocalPlayer) return;

        if (Time.time - lastShout > coolDown)
        {
            lastShout = Time.time;

            if (GameManager.instance != null && messages.Length > 0)
            {
                GameManager.instance.ShowText(
                    messages[msgNow++], 20, Color.white,
                    transform.position + new Vector3(0, 0.18f, 0),
                    Vector3.zero, showTime);

                if (msgNow == messages.Length)
                    msgNow = 0;
            }
        }
    }
}