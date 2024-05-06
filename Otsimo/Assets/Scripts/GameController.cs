using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public class GameController : MonoBehaviour
{
    [System.Serializable]
    public class Vector2Data
    {
        public float x, y;

        public Vector2Data(Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }

        public Vector2 ToVector()
        {
            return new Vector2(x, y);
        }
    }

    [System.Serializable]
    public class ShapeData
    {
        public Vector2Data position;
        public ColorData color;
        public Vector2Data size;
        public string spriteName;
        public string materialName; // Material ad�n� saklamak i�in yeni bir alan

        public ShapeData(Vector2 position, Color color, Vector2 size, string spriteName, string materialName)
        {
            this.position = new Vector2Data(position);
            this.color = new ColorData(color);
            this.size = new Vector2Data(size);
            this.spriteName = spriteName;
            this.materialName = materialName; // Material ad�n� sakla
        }
    }


    [System.Serializable]
    public class ColorData
    {
        public float r, g, b, a;

        public ColorData(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }
    }


    [System.Serializable]
    public class DrawingData
    {
        public List<ShapeData> shapes = new List<ShapeData>();

        public DrawingData(List<ShapeData> shapes)
        {
            this.shapes = shapes;
        }
    }

    public GameObject UyariPano;
    public TextMeshProUGUI UyariPanoText;
    public Image AnaNesneinspriteBilgisi;
    public Slider mySlider;
    private Image clickedImage;
    // G�r�nt� referans�
    public Image Nesne;
    public Color Renk;
    //public Image renkSecim;
    public GameObject renkPaleti;
    public GameObject sekilPaleti;
    bool KalemDurumu = false;
    bool SilgiDurumu = false;
    bool KovaDurumu = false;
    bool SekilDurumu = false;
    public GameObject DurumPanosu;
    public TMPro.TextMeshProUGUI DurumPanosuYazi;

    public float pozisyonMesafeS�n�r� = 10f;
    private List<Vector2> gecilenPozisyonlar = new List<Vector2>();
    private List<GameObject> olusturulanNesneler = new List<GameObject>();
    private Vector2 baslangicPozisyonu;
    private bool OyunSahneMi=false;
    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "OyunSahne")
        {
            OyunSahneMi = true;
        }
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => ShowTag(button.gameObject));
        }

        MousePozisyonBul();
        LoadDrawingData();
        YeniNesneOlusturVeBoya();
    }
    
    void Update()
    {
        if (OyunSahneMi == true)
        {
            if (Input.GetMouseButton(0) && KalemDurumu == true)
            {
                Nesne.transform.position = Input.mousePosition;
                // Nesnenin konumunu al
                Vector2 nesneKonumu = Nesne.transform.position;

                // Ge�ilen pozisyonlar� kaydet
                GecilenPozisyonlariEkle(nesneKonumu);

                YeniNesneOlusturVeBoya();
            }
            if (Input.GetMouseButtonDown(0) && SekilDurumu == true)
            {
                SekilOlustur();
            }

            Slider();
        }
        
    }

    public void MousePozisyonBul()
    {
        if (Input.GetMouseButtonDown(0))
        {
            baslangicPozisyonu = Input.mousePosition;
            Nesne.transform.position = baslangicPozisyonu;
            
        }

    }

    private void GecilenPozisyonlariEkle(Vector2 konum)
    {
        if (KalemDurumu == true)
        {
            // E�er ge�ilen pozisyonlar listesinde bu pozisyon daha �nce eklenmediyse, yeni pozisyonu ekle
            if (!gecilenPozisyonlar.Contains(konum))
            {
                gecilenPozisyonlar.Add(konum);
            }
        }
       
    }

    private void YeniNesneOlusturVeBoya()
    {
        if (KalemDurumu == true)
        {
            foreach (Vector2 pozisyon in gecilenPozisyonlar)
            {
                // E�er bu pozisyonda daha �nce nesne olu�turulmad�ysa
                if (!NesneVarMi(pozisyon) && CizimAlani(pozisyon))
                {
                    // Yeni nesne olu�tur
                    GameObject yeniNesne = new GameObject("RenkliNesne");
                    yeniNesne.tag = "RenkliNesne";
                    
                    RectTransform yeniNesneRT = yeniNesne.AddComponent<RectTransform>();
                    yeniNesneRT.SetParent(Nesne.rectTransform.parent); // Nesne ile ayn� parent'a sahip olacak
                    yeniNesneRT.position = pozisyon; // Ge�ilen pozisyona yerle�tir

                    // �maj bile�eni ekle ve rengini belirle
                    Image yeniNesneImage = yeniNesne.AddComponent<Image>();
                    yeniNesneImage.sprite = Nesne.sprite; // Yeni nesnenin sprite'�n� Nesne'nin sprite'�yla ayn� yap
                    yeniNesneImage.material = Nesne.material;
                    yeniNesneImage.color = Renk;

                    // Yeni nesnenin boyutunu Nesne'nin boyutuna ayarla
                    yeniNesneRT.sizeDelta = Nesne.rectTransform.sizeDelta;

                    olusturulanNesneler.Add(yeniNesne);
                }
            }
        }
        

    }
    
    private void SekilOlustur()
    {
        Vector2 SeklinKonumu = Input.mousePosition;
        if (SekilDurumu == true && CizimAlani(SeklinKonumu))
        {
            SeklinKonumu = Input.mousePosition;
            GameObject yeniSekil = new GameObject("YeniSekil");
            yeniSekil.tag = "YeniSekil";
            RectTransform yeniSekilRT = yeniSekil.AddComponent<RectTransform>();
            yeniSekilRT.SetParent(Nesne.rectTransform.parent);
            yeniSekilRT.position = SeklinKonumu;

            Image yeniSekilImage = yeniSekil.AddComponent<Image>();
            yeniSekilImage.sprite = Nesne.sprite;
            yeniSekilImage.material = Nesne.material;
            yeniSekilImage.color = Renk;

            Button yeniSekilButton = yeniSekil.AddComponent<Button>();
            yeniSekilButton.onClick.AddListener(() => Boyama(yeniSekilImage));

            yeniSekilRT.sizeDelta = Nesne.rectTransform.sizeDelta;
            olusturulanNesneler.Add(yeniSekil);
            olusturulanNesneler.Add(yeniSekil);
             // Fonksiyon atamas�

        }
    }

    private bool NesneVarMi(Vector2 pozisyon)
    {
        // Belirtilen pozisyonda daha �nce nesne olu�turulmu�sa true d�nd�r
        GameObject[] tumNesneler = GameObject.FindGameObjectsWithTag("RenkliNesne");
        foreach (GameObject nesne in tumNesneler)
        {
            if ((Vector2)nesne.transform.position == pozisyon)
            {
                return true;
            }
        }
        return false;
    }

    private bool CizimAlani(Vector2 cizimPozisyonu)
    {
        RectTransform cizimAlan�RT = GameObject.FindGameObjectWithTag("CizimAlani").GetComponent<RectTransform>();

        // �izim alan�n�n d�nya koordinatlar�ndaki s�n�rlar�n� hesapla
        Vector3[] alanK��eleri = new Vector3[4];
        cizimAlan�RT.GetWorldCorners(alanK��eleri);

        // Dikd�rtgen s�n�rlar� olu�tur
        Rect cizimAlan�Rect = new Rect(alanK��eleri[0], alanK��eleri[2] - alanK��eleri[0]);

        // �izim pozisyonunun �izim alan� i�inde olup olmad���n� kontrol et
        return cizimAlan�Rect.Contains(cizimPozisyonu);
    }

    private void SilTumNesneleri()
    {
        GameObject[] cizimler = GameObject.FindGameObjectsWithTag("RenkliNesne");
        GameObject[] cisimKare = GameObject.FindGameObjectsWithTag("Kare");
        GameObject[] cisimUcgen = GameObject.FindGameObjectsWithTag("Ucgen");
        GameObject[] cisimDaire = GameObject.FindGameObjectsWithTag("Daire");

        foreach (GameObject nesne in olusturulanNesneler)
        {
            gecilenPozisyonlar.Remove((Vector2)nesne.transform.position);
            Destroy(nesne);
        }
        foreach (GameObject nesne in cisimKare)
        {
            gecilenPozisyonlar.Remove((Vector2)nesne.transform.position);
            Destroy(nesne);
        }
        foreach (GameObject nesne in cisimUcgen)
        {
            gecilenPozisyonlar.Remove((Vector2)nesne.transform.position);
            Destroy(nesne);
        }
        foreach (GameObject nesne in cisimDaire)
        {
            gecilenPozisyonlar.Remove((Vector2)nesne.transform.position);
            Destroy(nesne);
        }
        foreach (GameObject nesne in cizimler)
        {
            gecilenPozisyonlar.Remove((Vector2)nesne.transform.position);
            Destroy(nesne);
        }
        
        olusturulanNesneler.Clear();
        KalemDurumu = false;
        SilgiDurumu = false;
        KovaDurumu = false;
        SekilDurumu = false;
        Nesne.sprite = AnaNesneinspriteBilgisi.sprite;
    }

    

    public void SaveDrawingData()
    {
        List<ShapeData> shapes = new List<ShapeData>();
        foreach (GameObject obj in olusturulanNesneler)
        {
            Image img = obj.GetComponent<Image>();
            RectTransform rt = obj.GetComponent<RectTransform>();
            string spriteName = img.sprite ? img.sprite.name : "defaultSprite";
            string materialName = img.material ? img.material.name : "defaultMaterial"; // Material ad�n� al ve kaydet
            shapes.Add(new ShapeData(obj.transform.position, img.color, rt.sizeDelta, spriteName, materialName));
        }
        DrawingData data = new DrawingData(shapes);
        string json = JsonUtility.ToJson(data);
        string path = Path.Combine(Application.persistentDataPath, "DrawingData.json");
        File.WriteAllText(path, json);
        Debug.Log("Data saved to " + path);
    }

    public void TestColorSerialization()
    {
        Color originalColor = new Color(1.0f, 0.5f, 0.25f, 1.0f); // K�rm�z�ms� renk
        ColorData colorData = new ColorData(originalColor);
        string json = JsonUtility.ToJson(colorData);
        Debug.Log("Serialized color: " + json);

        ColorData loadedColorData = JsonUtility.FromJson<ColorData>(json);
        Color loadedColor = loadedColorData.ToColor();
        Debug.Log("Loaded color: " + loadedColor);
    }

    public void LoadDrawingData()
    {
        if (OyunSahneMi==true)
        {
            string path = Path.Combine(Application.persistentDataPath, "DrawingData.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                DrawingData loadedData = JsonUtility.FromJson<DrawingData>(json);
                foreach (ShapeData shape in loadedData.shapes)
                {
                    GameObject obj = new GameObject("LoadedShape");
                    RectTransform rt = obj.AddComponent<RectTransform>();
                    rt.SetParent(Nesne.rectTransform.parent);
                    rt.position = shape.position.ToVector();
                    rt.sizeDelta = shape.size.ToVector();

                    Image img = obj.AddComponent<Image>();
                    img.color = shape.color.ToColor();
                    img.sprite = Resources.Load<Sprite>(shape.spriteName);
                    img.material = Resources.Load<Material>(shape.materialName); // Material ad�yla materiali y�kle

                    olusturulanNesneler.Add(obj);
                }
                Debug.Log("Data loaded from " + path);
            }
            else
            {
                Debug.Log("No save file found at " + path);
            }
        }
        
    }


    private IEnumerator DurumBilgilendirmePanosuIcinSayac(float delay)
    {
        // Belirtilen s�re kadar bekle
        yield return new WaitForSeconds(delay);

        // S�re doldu�unda paneli gizle
        DurumPanosu.SetActive(false);
    }

    public void Kalem()
    {
        
        Nesne.sprite = AnaNesneinspriteBilgisi.sprite;
        KalemDurumu = true;
        SilgiDurumu = false;
        KovaDurumu = false;
        SekilDurumu = false;
        renkPaleti.SetActive(true);
        sekilPaleti.SetActive(false);
        DurumPanosu.SetActive(true);
        DurumPanosuYazi.text = "KALEM SE��LD�";
        StartCoroutine(DurumBilgilendirmePanosuIcinSayac(3f));

    }

    public void Silgi()
    {
        Nesne.sprite = AnaNesneinspriteBilgisi.sprite;
        KalemDurumu = true;
        SilgiDurumu = true;
        KovaDurumu = false;
        SekilDurumu = false;
        Renk = Color.white;
        Nesne.color = Color.white;
        renkPaleti.SetActive(false);
        DurumPanosu.SetActive(true);
        DurumPanosuYazi.text = "S�LG� SE��LD�";
        StartCoroutine(DurumBilgilendirmePanosuIcinSayac(3f));
    }

    public void Kova()
    {
        Nesne.sprite = AnaNesneinspriteBilgisi.sprite;
        KalemDurumu = false;
        SilgiDurumu = false;
        KovaDurumu = true;
        SekilDurumu = false;
        renkPaleti.SetActive(true);
        DurumPanosu.SetActive(true);
        DurumPanosuYazi.text = "KOVA SE��LD�";
        StartCoroutine(DurumBilgilendirmePanosuIcinSayac(3f));
    }

    public void Sekil()
    {
        KalemDurumu = false;
        SilgiDurumu = false;
        KovaDurumu = false;
        SekilDurumu = true;
        renkPaleti.SetActive(false);
        sekilPaleti.SetActive(true);
        DurumPanosu.SetActive(true);
        DurumPanosuYazi.text = "SEK�L SE��LD�";
        StartCoroutine(DurumBilgilendirmePanosuIcinSayac(3f));
    }

    private void Boyama(Image BoyanacakSekil)
    {
        if (KovaDurumu==true || KalemDurumu == false)
        {
            
            BoyanacakSekil.color = Renk;
        }
    }

    public void ShowTag(GameObject imageObject)
    {
        if (OyunSahneMi == true)
        {
            Image image = imageObject.GetComponent<Image>();
            Renk = image.color;
            Nesne.color = image.color;


            string tagBilgisi = imageObject.tag.ToString();
            if (tagBilgisi != "Untagged")
            {
                DurumPanosu.SetActive(true);
                DurumPanosuYazi.text = tagBilgisi + " SE��LD�";
                StartCoroutine(DurumBilgilendirmePanosuIcinSayac(3f));
            }
        }
        
        
    }

    public void SekilSec(GameObject SekilObject)
    {
        Image image = SekilObject.GetComponent<Image>();
        Nesne.sprite = image.sprite;
     
        string tagBilgisi = image.tag.ToString();
        DurumPanosu.SetActive(true);
        DurumPanosuYazi.text = tagBilgisi + " SE��LD�";
        StartCoroutine(DurumBilgilendirmePanosuIcinSayac(3f));
        sekilPaleti.SetActive(false);
        renkPaleti.SetActive(true);


    }

    public void Slider()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "OyunSahne")
        {
            float nesneSize = mySlider.value;
            RectTransform nesneRT = Nesne.GetComponent<RectTransform>();
            nesneRT.sizeDelta = new Vector2(nesneSize * 10, nesneSize * 10);
        }
        
        
        
    }

    public void AnaMenuyeDon()
    {
        
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "OyunSahne")
        {
            UyariPano.SetActive(true);
            UyariPanoText.text = "Ana Men�ye D�nmeden �nce Oyununuzu Kay�t Etti�inizden Emin Olunuz!";
            
        }
        else
        {
            SceneManager.LoadScene("AnaSayfa");
        }

    }

    public void PanoUyariDevamEt()
    {
        SceneManager.LoadScene("AnaSayfa");
    }

    public void PanoUyariGeriDon()
    {
        UyariPano.SetActive(false);
    }

    public void OyunaBasla()
    {
        SceneManager.LoadScene("OyunSahne");
    }

    public void Cikis()
    {
        Application.Quit();
    }

    public void Hazirliyan()
    {
        SceneManager.LoadScene("Hazirliyan");
    }

}
