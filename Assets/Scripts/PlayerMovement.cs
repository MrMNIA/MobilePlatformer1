using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Features")]   //Header, script alt�ndaki girdileri daha d�zenli tutmak i�in onlar� s�n�fland�rmay� sa�lar.
    [SerializeField] private float accelerationForce; //karaktere hareket etmesi i�in uygulayaca��m�z kuvvetin g�c�
    [SerializeField] private float maxSpeed;        //ula�abilece�i azami yatay h�z
    [SerializeField] private float jumpPower;       //z�plama kuvveti
    [SerializeField] private float jumpCooldown = 0.25f;    //�st �ste z�plamalar� dizginlemek i�in saya�

    private BoxCollider2D boxCollider;  
    private Rigidbody2D body;
    [SerializeField] private MovementJoystick movementJoystick; //joystick referans�
    [SerializeField] private AttackJoystick attackJoystick;
    private Animator anim; //Animator bileşenine erişim

    [Header("WallJump")]
    [SerializeField] private float wallJumpX;   //sonradan ekleyece�imiz duvar z�plamas� i�in kuvvet girdileri
    [SerializeField] private float wallJumpY;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;     //baz� katmanlar� karaktere referans g�sterece�iz.
    [SerializeField] private LayerMask wallLayer;

    private float horizontalInput;
    private float verticalInput;
    private float currentAcceleration;          //birtak�m gerekli de�i�kenler
    private float jumpTimer;
    private bool isRunning;     //karakterin yürümekte olup olmadığını kaydeden bool
    private void Awake()            //referans atamalar�
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        horizontalInput = movementJoystick.Horizontal;      //yatay ve dikey girdiler
        verticalInput = movementJoystick.Vertical;


        anim.SetBool("onGround", onGround());

        isRunning = (onGround() && Mathf.Abs(body.linearVelocity.x) >= 0.2f);
        anim.SetBool("isRunning", isRunning);

        if (jumpTimer > 0f)      //z�plama sayac�n� geri sayma
        {
            jumpTimer -= Time.deltaTime;        //Time.deltaTime ile bir de�eri ger�ek zamana ba�l� olarak de�i�tirebiliriz
        }

        if (verticalInput >= 0.6f && jumpTimer <= 0f)       //joystick yeterince yukar�daysa ve z�plama m�saitse z�plamas�na izin ver
        {
            Jump();
        }


        if (!onGround() && onWall() && body.linearVelocity.y < 0f)    //karakter duvara yap���ksa s�rt�nmesi i�in yer�ekimini azalt�yoruz
            body.gravityScale = 0.3f;
        else
            body.gravityScale = 2.0f;

        if(Mathf.Abs(horizontalInput) >= 0.2f)
        {
            if (horizontalInput >0)                            //joystickin y�n�ne g�re karakteri �evir
                transform.localScale = new Vector3(1, 1, 1);
            else if (horizontalInput <0)
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void FixedUpdate()
    {
        if (Mathf.Abs(horizontalInput) >= 0.25f)        //joystick i�in k���k bir s�r�kleme s�n�r�
        {
            currentAcceleration = onGround() ? accelerationForce : (accelerationForce / 2);   //karakter yerde de�ilse itme kuvveti daha az olsun.
            body.AddForce(new Vector2(horizontalInput * currentAcceleration, 0));       //karaktere yatay girdi y�n�ne g�re bir kuvvet uygula
        }

        //joystick b�rak�ld���nda karakterin h�zla durmas�n� sa�lamak i�in
        if (Mathf.Abs(body.linearVelocity.x) > 0.1f && Mathf.Abs(horizontalInput) < 0.25f) //girdi yoksa ve hareket halindeyse
            body.AddForce(new Vector2(-body.linearVelocity.x * 20f, 0));              //ters y�nde bir itme uygula

        //karakter azami h�z� y�r�yerek a�amamal�
        if (Mathf.Abs(body.linearVelocity.x) > maxSpeed)              //e�er h�z� azami h�z� ge�iyorsa
            body.linearVelocity = new Vector2(Mathf.Sign(body.linearVelocity.x) * maxSpeed, body.linearVelocity.y); //h�z� s�n�ra indirmeliyiz
    }

    private void Jump()
    {
        if (onWall() && !onGround()) //duvara yapisiksa ve yerde degilse
        {
            //Duvar Z�plamas�
            body.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpX * 50, wallJumpY * 50));
            anim.SetTrigger("jump");

            //karakterin bakt��� y�n�n tersine ve yukar� do�ru
        }
        else if (onGround())        //yerdeyse
        {
            body.AddForce(new Vector2(0, jumpPower * 50)); //yukari dogru jumpPower kadar kuvvet
            anim.SetTrigger("jump");
        }

        jumpTimer = jumpCooldown;

    }

    private bool onGround()
    {
        RaycastHit2D hit = Physics2D.BoxCast(   //bir ���n yerine bir kutu ate�leyerek daha tutarl� bir kontrol yapar�z.
            boxCollider.bounds.center,          //at�lacak kutunun ba�lang�c�
            boxCollider.bounds.size,            //at�lacak kutunun boyutu
            0,                                  //at�lacak kutunun a��s�
            Vector2.down,                       //at�lacak kutunun y�n�
            0.1f,                               //at�lacak kutunun gidece�i max mesafe
            groundLayer);                       //at�lacak kutunun arad��� Layer
        return hit.collider != null;            //e�er bir sonu� alamazsak false de�er d�ner. e�er al�rsak true d�ner.
    }
    private bool onWall()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center, 
            boxCollider.bounds.size, 
            0, 
            new Vector2(transform.localScale.x, 0), //karakterin bakt��� y�ne do�ru olmal�
            0.2f, 
            wallLayer);

        return hit.collider != null;
    }
}
