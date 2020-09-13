using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    private const int ZOOM = 50;
    private DrawableTexture2D x;
    private Camera cam;
    private RaycastHit hit;
    private StrokeStorage ss = new StrokeStorage();

    public SpriteRenderer sr;
    public Texture2D texture;
    public Button btn_clear;
    public Button btn_undo;
    public TMP_Text Text;

    private void Awake()
    {
        cam = Camera.main;

        btn_clear.onClick.AddListener(() =>
        {
            ss.Clear();
            ShowResult();
        });

        btn_undo.onClick.AddListener(() =>
        {
            ss.Undo();
            ShowResult();
        });
    }

    void Start()
    {
        int width = texture.width;
        int height = texture.height;
        Debug.Log($"{width} - {height}");
        var tex = new Texture2D(width * 2, height * 2, TextureFormat.RGBA32, mipChain: false, linear: true);
        for (int y = 0; y < tex.height; ++y)
        {
            for (int x = 0; x < tex.width; ++x)
            {
                tex.SetPixel(x, y, Color.white);
            }
        }
        tex.SetPixels32(texture.width / 2, texture.height / 2, texture.width, texture.height, texture.GetPixels32());

        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ZOOM);

        x = new DrawableTexture2D(tex);
        ss.DrawableTexture2D = x;
        tex.Apply();
        x.Apply();
        ss.Init();
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            return;
        }

        if (!ss.IsDragStarted())
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                var mousePos = Input.mousePosition;
                var ray = cam.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out hit))
                {
                    var p = (hit.point - sr.transform.position) * ZOOM;
                    var p2 = new Vector2(p.x + x._texture.width / 2f, p.y + x._texture.height / 2f);
                    ss.DragStarted(p2);
                }
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                var mousePos = Input.mousePosition;
                mousePos.z = -cam.transform.position.z;

                var sp = cam.ScreenToWorldPoint(mousePos);
                var p = (sp - sr.transform.position) * ZOOM;
                var p1 = new Vector2(p.x + x._texture.width / 2f, p.y + x._texture.height / 2f);

                if (ss.IsDragable(p1))
                {
                    ss.Drag(p1);
                }
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                ss.DragEnded();
                ShowResult();
            }
        }
    }

    public void ShowResult()
    {
        var lst = ss.Print();
        if (lst.Count() == 0)
        {
            return;
        }

        var str = string.Join(" ", lst.Select(x => x.Item2.Trim('\0')).ToArray());
        Debug.Log($"{str}");
        Text.SetText(str);
    }
}
