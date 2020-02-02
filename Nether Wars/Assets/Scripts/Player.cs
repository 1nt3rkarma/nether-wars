using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Свойства

    #region Локальные

    [Tooltip("Цвет выделения (блоков)")]
    public Color selectionColor;
    [Tooltip("Префаб подсветки тайлов (при строительстве)")]
    public GameObject tempMarkerPref;
    public GameResources resourceFile;

    public Transform spawnPoint;
    [Range(1, 5)]
    public float spawnMinDistance;
    #endregion

    #region Статические
    public static Player local;
    public static int level = 1;
    public static bool mouseOverUI = false;
    public static bool controlEnabled = true;

    public static Camera mainCamera { get => UIManager.activeCamera; }
    public static InteractionModes interactionMode = InteractionModes.generic;

    public static GameObject tempMarker;
    public static Block selectedBlock;

    public static int limitMax { get => CalculateLimitMax(level); }
    public static int limitOccupied { get => CalculateLimitOccupied();  }
    public static List<Unit> units;

    public static bool renderHealthBars = false;

    static int markerX = -1, markerZ = -1;
    #endregion

    #endregion

    void Awake()
    {
        local = this;
        units = new List<Unit>();
        Random.seed = System.DateTime.Now.Millisecond;
    }

    void Start()
    {
        SwitchMode(InteractionModes.generic);
    }

    void Update()
    {
        if (!controlEnabled)
            return;

        if (UIManager.activeMenu == Menues.generic)
        {
            #region Горячие клавиши в обычном режиме

            if (Input.GetKey(KeyCode.LeftAlt))
                renderHealthBars = true;            
            else
                renderHealthBars = false;

            if (Input.GetKeyDown(KeyCode.B))
            {
                if (interactionMode == InteractionModes.building)
                    SwitchMode(InteractionModes.generic);
                else
                    SwitchMode(InteractionModes.building);
                return;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (UIManager.activeMenu == Menues.alchemy)
                    UIManager.SwitchMenu(Menues.generic);
                else
                    UIManager.SwitchMenu(Menues.alchemy);
                return;
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                CreateUnit(resourceFile.defaultUnit, Team.enemy, spawnPoint.position);
            }
            #endregion

            if (TerrainSystem.IsReady && !mouseOverUI)
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                Debug.DrawRay(ray.origin, ray.direction * 10, Color.green, 0.1f);

                if (Physics.Raycast(ray, out hit))
                {
                    switch (interactionMode)
                    {
                        default:
                            if (tempMarker)
                                Destroy(tempMarker);
                            Block block = hit.collider.GetComponent<Block>();
                            if (block)
                            {
                                SelectBlock(block);
                                if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
                                    TerrainSystem.DestroyBlock(block);
                            }
                            else
                            {
                                if (selectedBlock)
                                    DeselectBlock();
                            }

                            break;
                        case InteractionModes.building:
                            if (hit.collider.CompareTag("Terrain"))
                            {
                                if (selectedBlock)
                                    DeselectBlock();

                                Vector3 point = TerrainSystem.GetPoint(hit.point, out int X, out int Z);
                                
                                // Подсветка места строительства
                                int x = Mathf.RoundToInt(point.x);
                                int z = Mathf.RoundToInt(point.z);
                                if (x != markerX || z != markerZ)
                                {
                                    if (tempMarker)
                                        Destroy(tempMarker);
                                    tempMarker = Instantiate(tempMarkerPref, point, Quaternion.identity);
                                }
                                // --//

                                if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
                                    BuildBlock(X, Z);
                            }
                            else if (tempMarker)
                                Destroy(tempMarker);

                            break;
                    }
                }
            }
            else
            {
                if (tempMarker)
                    Destroy(tempMarker);
                if (selectedBlock)
                    DeselectBlock();
            }
        }
        else if (UIManager.activeMenu == Menues.alchemy)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Alchemy.ClearReceipt();
                UIManager.SwitchMenu(Menues.generic);
                return;
            }
        }
    }

    void BuildBlock(int indexX, int indexZ)
    {
        Vector3 point = TerrainSystem.points[indexX, indexZ];
        Vector3 center = point + Vector3.up * TerrainSystem.blockSize / 2;
        Vector3 halfExtence = new Vector3(TerrainSystem.blockSize / 2,
            TerrainSystem.blockSize / 2, TerrainSystem.blockSize / 2);

        Collider[] colliders = Physics.OverlapBox(center, halfExtence);
        foreach (var collider in colliders)
        {
            if (collider.GetComponent<Unit>())
            {
                Debug.Log("Здесь нельзя строить!");
                return;
            }
        }

        TerrainSystem.CreateBlock(indexX, indexZ);
    }

    #region Режимы взаимодействия

    public static void SwitchMode(InteractionModes newMode)
    {
        interactionMode = newMode;
    }

    #endregion

    #region Взаимодействие с юнитами

    public static int CalculateLimitMax(int level)
    {
        switch(level)
        {
            default:
                return 7;
            case 2:
                return 11;
            case 3:
                return 17;
        }
    }

    public static int CalculateLimitOccupied()
    {
        int sum = 0;
        foreach (var unit in units)
            if (unit.isAlive)
                sum += unit.level;
        return sum;
    }

    public static Unit SpawnUnit(Unit unitType, Perk perk1, Perk perk2)
    {
        float randomAngle = Random.Range(0f, 359f);
        float x = local.spawnMinDistance * Mathf.Cos(randomAngle);
        float z = local.spawnMinDistance * Mathf.Sin(randomAngle);
        Vector3 position = local.transform.position + new Vector3(x, 0, z);

        Unit unit = CreateUnit(unitType, Team.player, position);
        unit.renderer.material.color = local.resourceFile.GetElementColor(perk1.element);
        unit.damageType = perk1.element;
        if (perk1 != null)
            unit.AddPerk(perk1);
        if (perk2 != null)
            unit.AddPerk(perk2);
        //Debug.Log("Вы сотворили существо: " + unitType.unitName);
        return unit;
    }

    public static Unit CreateUnit(Unit unitType, Team forTeam, Vector3 position)
    {
        Unit unit = CreateUnit(unitType, position);
        unit.team = forTeam;
        if (forTeam == Team.player)
            units.Add(unit);
        return unit;
    }

    public static Unit CreateUnit(Unit unitType, Vector3 position)
    {
        return Instantiate(unitType.unitType, position, Quaternion.identity);
    }

    #endregion

    #region Взаимодействие с блоками террейна

    void SelectBlock(Block block)
    {
        if (selectedBlock != block)
        {
            if (selectedBlock != null)
                DeselectBlock();
            selectedBlock = block;
            selectedBlock.renderer.material.color = selectionColor;
        }
    }

    void DeselectBlock()
    {
        selectedBlock.renderer.material.color = selectedBlock.defaultColor;
        selectedBlock = null;
    }

    #endregion

    #region Доп. интерфейс в режиме Сцены

    void OnDrawGizmosSelected()
    {
        if (isActiveAndEnabled)
        {
            if (spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(spawnPoint.position, spawnMinDistance);
            }
        }
    }

    #endregion
}

public enum InteractionModes { generic, building}
