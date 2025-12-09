using UnityEngine;
using UnityEngine.EventSystems; //dokunma olaylarýný kullanabilmek için gereken kütüphane

public class MovementJoystick : MonoBehaviour,IDragHandler, IPointerUpHandler, IPointerDownHandler//ekrana dokunulurken ve ekran býrakýldýðýnda durumlarý için
                                                                               //gerekli metotlarý kullanabilmek için eklediðimiz interfaceler
{
    [SerializeField] private float maxRange = 75f; //joystickin uzaklaþabileceði max mesafe

    private RectTransform joystickThumb;
    private RectTransform joystickBackground; //background ve thumbýn UI konum bilgileri
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; } //joystickten gelen yatay ve dikey verileri bu deðerlerden okuyacaðýz

    private void Awake() //script çaðrýldýðý anda ilk çalýþan metottur. burada referans atamalarý yapýlýr.
    {
        joystickBackground = GetComponent<RectTransform>(); //atandýðý objenin ReckTransform bileþenini bu nesneye ata
        joystickThumb = transform.GetChild(0).GetComponent<RectTransform>(); //transform.GetChild(0); bu objenin ÝLK CHÝLD'INDAN okur (yani thumb)
        ResetValues(); //x,y ve thumb konumunu sýfýrlayan yardýmcý metotumuz. aþaðýda tanýmlayacaðýz
    }
    public void OnDrag(PointerEventData eventData) //obje üzerinde dokunulan konumun bilgisi, PointerEventData ile eventData'ya akatrýlýr
    {
        Vector2 position; //dokunulan konumun joysticke göre yönünü ve boyunu belirlemek için kullandýðýmýz vektör

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle( //kontrol ve dokunulan konum için geri dönüþ aldýðýmýz metot
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out position))
        {
            position = position / (joystickBackground.sizeDelta * 0.5f); //gelen deðeri [-1,1] aralýðýna indirmek için joystick yarýçapýna bölüyoruz

            if (position.magnitude > 1f) //vektörün boyutu 1'den büyükse (þeklimizin sýnýrýný aþýyorsa)
            {
                position = position.normalized; //vektörün boyutunu 1'e indirmek için normalized olarak ayarlýyoruz
            }
            
            joystickThumb.anchoredPosition = position * maxRange; //thumbý konuma ata
            Horizontal = position.x; //vektörün x deðerini al
            Vertical = position.y;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        ResetValues();
    }
    private void ResetValues()
    {
        joystickThumb.anchoredPosition = Vector2.zero;
        Horizontal = 0f;
        Vertical = 0f;
    }
}