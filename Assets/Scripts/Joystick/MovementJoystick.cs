using UnityEngine;
using UnityEngine.EventSystems; //dokunma olaylar�n� kullanabilmek i�in gereken k�t�phane

public class MovementJoystick : MonoBehaviour,IDragHandler, IPointerUpHandler, IPointerDownHandler//ekrana dokunulurken ve ekran b�rak�ld���nda durumlar� i�in
                                                                               //gerekli metotlar� kullanabilmek i�in ekledi�imiz interfaceler
{
    [SerializeField] private float maxRange = 75f; //joystickin uzakla�abilece�i max mesafe

    private RectTransform joystickThumb;
    private RectTransform joystickBackground; //background ve thumb�n UI konum bilgileri
    private bool ableToMove = true; //joystickin hareket edip edemeyece�ini kontrol eden de�i�ken
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; } //joystickten gelen yatay ve dikey verileri bu de�erlerden okuyaca��z

    private void Awake() //script �a�r�ld��� anda ilk �al��an metottur. burada referans atamalar� yap�l�r.
    {
        joystickBackground = GetComponent<RectTransform>(); //atand��� objenin ReckTransform bile�enini bu nesneye ata
        joystickThumb = transform.GetChild(0).GetComponent<RectTransform>(); //transform.GetChild(0); bu objenin �LK CH�LD'INDAN okur (yani thumb)
        ResetValues(); //x,y ve thumb konumunu s�f�rlayan yard�mc� metotumuz. a�a��da tan�mlayaca��z
    }
    public void OnDrag(PointerEventData eventData) //obje �zerinde dokunulan konumun bilgisi, PointerEventData ile eventData'ya akatr�l�r
    {
        if (!ableToMove) { return; } //hareket engellendiyse dokunma olaylar�n� i�leme, sadece joystickin s�f�rlanmas�n� yap
        Vector2 position; //dokunulan konumun joysticke g�re y�n�n� ve boyunu belirlemek i�in kulland���m�z vekt�r

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle( //kontrol ve dokunulan konum i�in geri d�n�� ald���m�z metot
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out position))
        {
            position = position / (joystickBackground.sizeDelta * 0.5f); //gelen de�eri [-1,1] aral���na indirmek i�in joystick yar��ap�na b�l�yoruz

            if (position.magnitude > 1f) //vekt�r�n boyutu 1'den b�y�kse (�eklimizin s�n�r�n� a��yorsa)
            {
                position = position.normalized; //vekt�r�n boyutunu 1'e indirmek i�in normalized olarak ayarl�yoruz
            }
            
            joystickThumb.anchoredPosition = position * maxRange; //thumb� konuma ata
            Horizontal = position.x; //vekt�r�n x de�erini al
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

    public void ChangeAbleToMove()
    {
        ableToMove = !ableToMove;
        if (!ableToMove)
        {
            ResetValues(); //hareket engellendiinde joystick de sfrlanr
        }
    }
}