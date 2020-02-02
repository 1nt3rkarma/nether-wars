using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AI;

public class TerrainSystem : MonoBehaviour
{
    [Range(32,64)][Tooltip("Размер сетки (в блоках) по оси X (Север-Юг) и Z (Запад-Восток)")]
    public int size = 32;

    public bool smartRender = false;

    public const int blockSize = 2;

    [Tooltip("Ссылка на основную поверхность террейна")]
    public NavMeshSurface navMeshSurface;

    [Tooltip("Палитра стандартных блоков (тип террейна - камень)")]
    public BlockPalette blocksRock;

    [Tooltip("Террейн генерируется мгновенно?")]
    public bool instantsGeneration = true;

    [Tooltip("Загрузить существующие данные террейна?")]
    public bool loadTerrainData = false;


    public static TerrainSystem local;
    public static Block[,] blocks;
    public static Vector3[,] points;
    static GameObject blockGroup;

    static bool isReady = false;
    public static bool IsReady { get => isReady;}

    #region Инициализация

    void Start()
    {
        local = this;
        Random.seed = System.DateTime.Now.Millisecond;
        if (loadTerrainData)
        {
            TerrainData data = SaveSystem.LoadTerrain();
            if (data != null)
                StartCoroutine(Generate(data));
            else
            {
                Debug.LogWarning("Не удалось считать данные, террейн будет сгенерирован с нуля");
                StartCoroutine(Generate());
            }
        }
        else
            StartCoroutine(Generate());
    }

    IEnumerator Generate()
    {
        blocks = new Block[size, size];
        points = new Vector3[size, size];

        Vector3 center = new Vector3(size, 0, size);
        blockGroup = new GameObject("Слой блоков");
        blockGroup.transform.SetParent(transform);

        //Debug.Log("Создание блоков");
        for (int x = 0; x < size; x++)
        {
            GameObject rowGroup = new GameObject("Ряд " + x);
            rowGroup.transform.SetParent(blockGroup.transform);

            for (int z = 0; z < size; z++)
            {
                points[x,z] = transform.position + new Vector3(blockSize * (x + 1f / 2), 0, blockSize * (z + 1f / 2));
                float distance = Vector3.Distance(center, points[x, z]);
                if (distance > 6)
                {
                    blocks[x, z] = Instantiate(blocksRock.blockCap, points[x, z], Quaternion.identity);
                    blocks[x, z].transform.SetParent(rowGroup.transform);
                    blocks[x, z].name = "Блок " + x + "-" + z;
                }
                else
                    blocks[x, z] = null;
            }
            if (!instantsGeneration)
                yield return null;

        }

        //Debug.Log("Обновление типов блоков");
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                if (blocks[x, z] != null)
                    UpdateBlockOption(x, z);
            }
            if (!instantsGeneration)
                yield return null;
        }

        navMeshSurface.BuildNavMesh();
        isReady = true;
    }

    IEnumerator Generate(TerrainData data)
    {
        int size = data.size;
        blocks = new Block[size, size];
        points = new Vector3[size, size];

        Vector3 center = new Vector3(size, 0, size);
        blockGroup = new GameObject("Слой блоков");
        blockGroup.transform.SetParent(transform);

        //Debug.Log("Создание блоков");
        for (int x = 0; x < size; x++)
        {
            GameObject rowGroup = new GameObject("Ряд " + x);
            rowGroup.transform.SetParent(blockGroup.transform);

            for (int z = 0; z < size; z++)
            {
                points[x, z] = transform.position + new Vector3(blockSize * (x + 1f / 2), 0, blockSize * (z + 1f / 2));
                if (data.cells[x,z] != 0)
                {
                    blocks[x, z] = Instantiate(blocksRock.blockCap, points[x, z], Quaternion.identity);
                    blocks[x, z].transform.SetParent(rowGroup.transform);
                    blocks[x, z].name = "Блок " + x + "-" + z;
                }
                else
                    blocks[x, z] = null;
            }
            if (!instantsGeneration)
                yield return null;

        }

        //Debug.Log("Обновление конфигурации блоков");
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                if (blocks[x, z] != null)
                    UpdateBlockOption(x, z);
            }
            if (!instantsGeneration)
                yield return null;
        }

        navMeshSurface.BuildNavMesh();
        isReady = true;
    }

    #endregion

    #region Основные методы

    void Update()
    {
        SmartRender();
    }

    static void SmartRender()
    {
        Vector3 point = UIManager.mainCameraFocusPoint;
        point.y = blockGroup.transform.position.y;
        for (int x = 0; x < local.size; x++)
        {
            for (int z = 0; z < local.size; z++)
            {
                if (blocks[x, z] != null)
                {
                    if (UIManager.activeMenu == Menues.generic || UIManager.activeMenu == Menues.pause)
                    {
                        if (local.smartRender)
                        {
                            float deltaX = Mathf.Abs(points[x, z].x - point.x);
                            float deltaZ = Mathf.Abs(points[x, z].z - point.z);

                            int thresholdX = 12, thresholdZ = 15;


                            if (deltaX > thresholdX || deltaZ > thresholdZ)
                                blocks[x, z].renderer.enabled = false;
                            else
                            {
                                blocks[x, z].renderer.enabled = true;
                                //if (blocks[x, z].option == BlockOptions.cap)
                                    //blocks[x, z].renderer.shadowCastingMode = ShadowCastingMode.Off;
                                //else
                                    //blocks[x, z].renderer.shadowCastingMode = ShadowCastingMode.On;

                            }
                        }
                        else
                            blocks[x, z].renderer.enabled = true;
                    }
                    else
                        blocks[x, z].renderer.enabled = false;
                }
            }
        }
    }

    public static void CreateBlock(Vector3 position)
    {
        GetPoint(position, out int x, out int z);
        CreateBlock(x, z);
    }

    public static void CreateBlock(int x, int z)
    {
        if (blocks[x, z] == null)
        {
            Transform rowGroup = blockGroup.transform.GetChild(x);

            blocks[x, z] = Instantiate(local.blocksRock.blockCap, points[x, z], Quaternion.identity);
            blocks[x, z].transform.SetParent(rowGroup);
            blocks[x, z].name = "Блок " + x + "-" + z;

            UpdateBlockOption(x, z);
            UpdateBlockGroup(x, z);

            local.StartCoroutine(local.UpdateNavMesh());
        }
    }

    public static void DestroyBlock(Block block)
    {
        for (int x = 0; x < local.size; x++)
            for (int z = 0; z < local.size; z++)
                if (block == blocks[x, z])
                {
                    DestroyBlock(x, z);
                    return;
                }
        Debug.LogError("Блок отсутствует в массиве!");
        return;
    }

    public static void DestroyBlock(int x, int z)
    {
        if (blocks[x, z] == null)
            return;

        Destroy(blocks[x, z].gameObject);
        blocks[x, z] = null;

        UpdateBlockGroup(x, z);

        local.StartCoroutine(local.UpdateNavMesh());
    }

    IEnumerator UpdateNavMesh()
    {
        yield return null;
        local.navMeshSurface.BuildNavMesh();
    }

    static void UpdateBlockOption(int x, int z)
    {
        if (blocks[x, z] == null)
            return;

        string name = blocks[x, z].name;
        Transform parent = blocks[x, z].transform.parent;
        Vector3 position = blocks[x, z].transform.position;

        float rotation;
        BlockOptions option = CheckBlockOption(x, z, out rotation);
        if (option == BlockOptions.empty)
            return;

        Block prefab;
        switch (option)
        {
            case BlockOptions.cap:
                prefab = local.blocksRock.blockCap;
                break;
            case BlockOptions.corner:
                prefab = local.blocksRock.blockCorner;
                break;
            case BlockOptions.front:
                prefab = local.blocksRock.blockFront;
                break;
            case BlockOptions.jut:
                prefab = local.blocksRock.blockJut;
                break;
            case BlockOptions.row:
                prefab = local.blocksRock.blockRow;
                break;
            default:
                prefab = local.blocksRock.blockSingle;
                break;
        }
        Destroy(blocks[x, z].gameObject);
        blocks[x, z] = Instantiate(prefab, position, Quaternion.identity);
        blocks[x, z].name = name;
        blocks[x, z].transform.SetParent(parent);
        blocks[x, z].transform.localEulerAngles = new Vector3(0, rotation, 0);
        blocks[x, z].option = option;
    }
    static void UpdateBlockGroup(int x, int z)
    {
        int n = x - 1;
        int s = x + 1;
        int w = z - 1;
        int e = z + 1;

        if (n > 0)
            UpdateBlockOption(n, z);
        if (s < local.size)
            UpdateBlockOption(s, z);
        if (w > 0)
            UpdateBlockOption(x, w);
        if (e < local.size)
            UpdateBlockOption(x, e);
    }

    public static BlockOptions CheckBlockOption(int x, int z, out float rotation)
    {
        if (blocks[x, z] == null)
        {
            rotation = 0;
            return BlockOptions.empty;
        }

        #region Проверка соседей
        // Флаги, указывающие, какие из соседних блоков 
        // отсутствуют (false) или присутствуют (true)
        bool n = false, s = false, w = false, e = false;
        if (x > 0)
            if (blocks[x - 1, z] != null)
                n = true;
        if (x < local.size - 1)
            if (blocks[x + 1, z] != null)
                s = true;
        if (z > 0)
            if (blocks[x, z - 1] != null)
                w = true;
        if (z < local.size - 1)
            if (blocks[x , z + 1] != null)
                e = true;
        #endregion

        // Блок закрыт со всех сторон
        if (n && s && w && e)
        {
            int r = Random.Range(0, 4);
            switch (r)
            {
                default:
                    rotation = 0;
                    break;
                case 1:
                    rotation = 90;
                    break;
                case 2:
                    rotation = 180;
                    break;
                case 3:
                    rotation = 270;
                    break;
            }
            return BlockOptions.cap;
        }

        // Блок закрыт с 2-ух противоположных сторон
        if (n && s && !w && !e)
        {
            rotation = 0;
            return BlockOptions.row;
        }
        if (!n && !s && w && e)
        {
            rotation = 90;
            return BlockOptions.row;
        }

        // Блок закрыт с 2-ух смежных сторон
        if (n && !s && w && !e)
        {
            rotation = 0;
            return BlockOptions.corner;
        }
        if (n && !s && !w && e)
        {
            rotation = 90;
            return BlockOptions.corner;
        }
        if (!n && s && !w && e)
        {
            rotation = 180;
            return BlockOptions.corner;
        }
        if (!n && s && w && !e)
        {
            rotation = 270;
            return BlockOptions.corner;
        }

        // Блок закрыт с 3-ех сторон
        if (n && s && w && !e)
        {
            rotation = 0;
            return BlockOptions.front;
        }
        if (n && !s && w && e)
        {
            rotation = 90;
            return BlockOptions.front;
        }
        if (n && s && !w && e)
        {
            rotation = 180;
            return BlockOptions.front;
        }
        if (!n && s && w && e)
        {
            rotation = 270;
            return BlockOptions.front;
        }

        // Блок открыт с 3-ех сторон
        if (n && !s && !w && !e)
        {
            rotation = 90;
            return BlockOptions.jut;
        }
        if (!n && !s && !w && e)
        {
            rotation = 180;
            return BlockOptions.jut;
        }
        if (!n && s && !w && !e)
        {
            rotation = 270;
            return BlockOptions.jut;
        }
        if (!n && !s && w && !e)
        {
            rotation = 0;
            return BlockOptions.jut;
        }

        // Блок открыт с 4-ех сторон
        rotation = 0;
        return BlockOptions.single;
    }
    #endregion

    #region Вспомогательные методы
    public static List<Vector3> GetFreePoints()
    {
        List<Vector3> freePoints = new List<Vector3>();
        for (int x = 0; x < local.size; x++)
        {
            for (int z = 0; z < local.size; z++)
            {
                if (blocks[x, z] == null)
                    freePoints.Add(points[x, z]);
            }
        }
        if (freePoints.Count > 0)
            return freePoints;
        else
            return null;
    }

    public static Vector3 GetRandomFreePoint(bool shift)
    {
        List<Vector3> freePoints = GetFreePoints();
        if (freePoints == null)
            return Vector3.zero;
        if (freePoints.Count == 0)
            return Vector3.zero;

        int randomIndex = Random.Range(0, freePoints.Count);
        Vector3 point = freePoints[randomIndex];

        if (shift)
        {
            float randomAngle = Random.Range(0f, 359f);
            float randomRange = Random.Range(0f, 1f);
            float x = randomRange * Mathf.Cos(randomAngle);
            float z = randomRange * Mathf.Sin(randomAngle);
            point += new Vector3(x, 0, z);
        }
        return point;
    }

    public static Vector3 GetPoint(Vector3 searchPosition, out int indexX, out int indexZ)
    {
        indexX = -1;
        indexZ = -1;
        if (!isReady)
            return new Vector3(-1, -1, -1);
        if (searchPosition.x < points[0, 0].x || searchPosition.x > points[local.size-1, 0].x ||
            searchPosition.z < points[0, 0].z || searchPosition.z > points[0, local.size - 1].z)
            return new Vector3(-1, -1, -1);


        Vector3 wanted = new Vector3(-1, -1, -1);
        int wantedX = -1, wantedZ = -1;
        float sqrDeltaMin = float.MaxValue;

        for (int x = 0; x < local.size; x++)
        {
            var deltaX = Mathf.Abs(points[x, 0].x - searchPosition.x);
            if (deltaX <= blockSize)
                for (int z = 0; z < local.size; z++)
                {
                    var deltaZ = Mathf.Abs(points[0, z].z - searchPosition.z);
                    if (deltaZ <= blockSize)
                    {
                        var sqrDelta = Vector3.SqrMagnitude(searchPosition - points[x, z]);
                        if (sqrDelta <= sqrDeltaMin)
                        {
                            sqrDeltaMin = sqrDelta;
                            wanted = points[x, z];
                            wantedX = x; wantedZ = z;
                        }
                    }
                }            
        }
        indexX = wantedX;
        indexZ = wantedZ;
        return wanted;
    }
    #endregion

    #region Доп. интерфейс (в окне Сцене)

    void OnDrawGizmosSelected()
    {
        if (isActiveAndEnabled)
        {
            if (blocks != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 size = new Vector3(blockSize * 0.9f, 0.4f, blockSize * 0.9f);
                for (int x = 0; x < this.size; x++)
                    for (int z = 0; z < this.size; z++)
                        if (blocks[x, z] == null)
                            Gizmos.DrawWireCube(points[x, z], size);
            }
        }
    }

    #endregion

}

public enum BlockOptions { empty, cap, corner, front, row, jut,single}

[System.Serializable]
public class BlockPalette : object
{
    public Block blockCap;
    public Block blockCorner;
    public Block blockFront;
    public Block blockRow;
    public Block blockJut;
    public Block blockSingle;
}

[System.Serializable]
public class TerrainData
{
    public int size;
    public int[,] cells;

    public TerrainData(TerrainSystem terrain)
    {
        size = terrain.size;
        cells = new int[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                if (TerrainSystem.blocks[x, z] == null)
                    cells[x, z] = 0;
                else
                    cells[x, z] = 1;
            }
        }
    }
}