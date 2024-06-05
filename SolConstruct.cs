using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;

namespace City
{
    public class SolConstruct : MonoBehaviour
    {

        [Header("Settings")]
        public Vector2Int dimensionSector;
        public bool random;
        [SerializeField, Range(1, 10)] private int nbLevel;
        [SerializeField] private bool[] needOfCubeType;
        public float sizeSol = 119f;
        private Vector3 pointOrBas;
        private Vector3 pointOrHaut;

        [Header("Look")]
        public Vector2Int minLengthLevel; //0 for center min size & 1 for border min size
        public bool wallSurrounding = false;
        public int deapthHoles;
        [SerializeField] private GameObject[] solElement;
        [SerializeField] private GameObject prefabHolder;
        [SerializeField] private bool heigthDisplay;
        [SerializeField] public List<int> levelCoordinate;
        [SerializeField] public List<int> levelHeigth;
        public int subdivision = 0;

        [Header("Plan")]
        [SerializeField] public GameObject prefabPlanSol;
        [SerializeField] public GameObject planSolHolder;
        [SerializeField] public GameObject grndHolder;
        [SerializeField] public GameObject escalierHolder;
        [SerializeField] public GameObject batCoreHolder;
        [SerializeField] public GameObject matrixHolder;
        private Vector3 positionIni;
        private bool needLvl = false;
        public bool planMode = true;
        private GameObject escModelHolder;
        private GameObject im;
        private List<Quaternion> surfaceConstruct;
        private List<List<List<bool>>> listMatrixConstructFloor;
        private List<List<bool>> matrixFreeSpace;
        private bool matrixPrinted;

        [Header("Performance")]
        public bool performanceActivated;
        public bool PIS = false;
        public bool insideSeen = false;
        private Sectorassembly structureScript;
        private bool noStructure = false;
        private bool nonUpdated = true;
        public sectorType sectorType;
        public Playermovment playerCode;
        public bool simplifiedVersion = false;

        [Header("Exterior")]
        public TMP_Text[] listNameHolder;
        public bool liftPlateform;
        List<GameObject> listCubeCatWalk = new List<GameObject>();
        List<int> listIndexPosition = new List<int> { 0, 17, 323, 306, 0 };
        List<Vector3> listCoordCorner = new List<Vector3> { new Vector3(-4.5f, 0, -4.5f), new Vector3(-4.5f, 0, 129.5f), new Vector3(129.5f, 0, 129.5f), new Vector3(129.5f, 0, -4.5f) };
        public bool notUpdated = true;
        public List<Sectorassembly.Neighbors> neighbors = new List<Sectorassembly.Neighbors>() { null, null, null, null, null, null };
        public List<bool> wallSide = new List<bool>() { false, false, false, false };
        List<bool> need = new List<bool>() { false, false, false, false };
        GameObject wallHolder;

         


        [Button("Generate Sol")]
        public void ResetSol()
        {
            //prepare the new generation
            needLvl = false;

            DeleteChildren(planSolHolder, "Volumic Sol");

            //Definie les coordonnées des points d'or
            void CoordGoldenPoint()
            {
                if (levelCoordinate[0] == Mathf.Min(levelCoordinate[0], levelCoordinate[2]))
                {
                    if (levelCoordinate[1] == Mathf.Min(levelCoordinate[1], levelCoordinate[3]))
                    {
                        pointOrBas = new Vector3(levelCoordinate[0], 0, levelCoordinate[1]);
                        pointOrHaut = new Vector3(levelCoordinate[2], 0, levelCoordinate[3]);
                    }
                    else
                    {
                        pointOrBas = new Vector3(levelCoordinate[0], 0, levelCoordinate[3]);
                        pointOrHaut = new Vector3(levelCoordinate[2], 0, levelCoordinate[1]);
                    }
                }
                else
                {
                    if (levelCoordinate[1] == Mathf.Min(levelCoordinate[1], levelCoordinate[3]))
                    {
                        pointOrBas = new Vector3(levelCoordinate[2], 0, levelCoordinate[1]);
                        pointOrHaut = new Vector3(levelCoordinate[0], 0, levelCoordinate[3]);
                    }
                    else
                    {
                        pointOrBas = new Vector3(levelCoordinate[2], 0, levelCoordinate[3]);
                        pointOrHaut = new Vector3(levelCoordinate[0], 0, levelCoordinate[1]);
                    }
                }
            }

            //create the Golden Points
            needLvl = true;
            if (random)
            {
                for (int i = 0; i < 4; i++)
                {
                    levelCoordinate[i] = (int)Random.Range(0, Mathf.Min((int)(sizeSol / (prefabPlanSol.transform.localScale.z)), (int)(sizeSol / (prefabPlanSol.transform.localScale.x))));
                    levelHeigth[i] = (int)Random.Range(0, nbLevel + 1);
                    if (levelCoordinate[i] > dimensionSector[0]-minLengthLevel[1] && levelCoordinate[i] < dimensionSector[0]) { levelCoordinate[i] = dimensionSector[0]; }
                    else if (levelCoordinate[i] > 0 && levelCoordinate[i] < minLengthLevel[1]) { levelCoordinate[i] = 0; }
                }

                //CoordGoldenPoint();
                //Debug.Log(pointOrBas);
                //Debug.Log(pointOrHaut);

                //verif pour centre de taille suffisante
                if (Mathf.Abs(levelCoordinate[0] - levelCoordinate[2]) < minLengthLevel[0])//if (Mathf.Abs(pointOrBas.x-pointOrHaut.x)<4)
                {
                    Debug.Log("petit x");
                    for (int i = 0; i < 4 - Mathf.Abs(levelCoordinate[0] - levelCoordinate[2]) + 1; i++)
                    {
                        if (levelCoordinate[2] < sizeSol / prefabPlanSol.transform.localScale.x && levelCoordinate[2] >= (sizeSol / prefabPlanSol.transform.localScale.x) - minLengthLevel[0])
                        { levelCoordinate[0] += -1; }
                        else { levelCoordinate[2] += 1; }
                    }
                    if (levelCoordinate[2] > dimensionSector[0]-minLengthLevel[1]) { levelCoordinate[2] = dimensionSector[0]; }
                }
                if (Mathf.Abs(levelCoordinate[1] - levelCoordinate[3]) < minLengthLevel[0])//if (Mathf.Abs(pointOrBas.z-pointOrHaut.z)<4)
                {
                    Debug.Log("petit z");
                    for (int i = 0; i < 4 - Mathf.Abs(levelCoordinate[1] - levelCoordinate[3]) + 2; i++)
                    {
                        if (levelCoordinate[3] < sizeSol / prefabPlanSol.transform.localScale.z && levelCoordinate[3] >= (sizeSol / prefabPlanSol.transform.localScale.z) - minLengthLevel[0])
                        { levelCoordinate[1] += -1; }
                        else { levelCoordinate[3] += 1; }
                    }
                    if (levelCoordinate[3] > dimensionSector[0]-minLengthLevel[1]) { levelCoordinate[3] = dimensionSector[0]; }
                }
            }
            else
            {
                pointOrBas.x = levelCoordinate[0];
                pointOrBas.z = levelCoordinate[1];
                pointOrHaut.x = levelCoordinate[2];
                pointOrHaut.z = levelCoordinate[3];
            }
            CoordGoldenPoint();
            levelCoordinate[0] = (int)pointOrBas.x;
            levelCoordinate[1] = (int)pointOrBas.z;
            levelCoordinate[2] = (int)pointOrHaut.x;
            levelCoordinate[3] = (int)pointOrHaut.z;

            //generate the new floor
            positionIni = planSolHolder.transform.position + new Vector3(prefabPlanSol.transform.localScale.x / 2, -prefabPlanSol.transform.localScale.y / 2, prefabPlanSol.transform.localScale.z / 2);
            Vector3 position = positionIni;
            for (int i = 0; i < sizeSol / (prefabPlanSol.transform.localScale.x); i++)
            {
                for (int j = 0; j < sizeSol / (prefabPlanSol.transform.localScale.z); j++)
                {
                    GameObject cube = Instantiate(prefabPlanSol);
                    cube.transform.position = position;
                    cube.transform.parent = planSolHolder.transform;

                    position += new Vector3(0, 0, prefabPlanSol.transform.localScale.z);

                    //level the floor
                    if (needLvl)
                    {
                        if ((i <= Mathf.Max(levelCoordinate[0], levelCoordinate[2]) && i >= Mathf.Min(levelCoordinate[0], levelCoordinate[2])) && (j <= Mathf.Max(levelCoordinate[1], levelCoordinate[3]) && j >= Mathf.Min(levelCoordinate[1], levelCoordinate[3])))
                        {
                            if (levelHeigth[0] <= deapthHoles) { Destroy(cube); }
                            else
                            {
                                cube.transform.position += new Vector3(0, levelHeigth[0] * prefabPlanSol.transform.localScale.y / 2, 0);
                                cube.transform.name = $"{i},{j}";// Etage{levelHeigth[0]}";
                                cube.transform.localScale += new Vector3(0, prefabPlanSol.transform.localScale.y * levelHeigth[0], 0);
                            }
                        }
                        else if ((i < Mathf.Min(levelCoordinate[0], levelCoordinate[2])) && (j <= Mathf.Max(levelCoordinate[1], levelCoordinate[3])))
                        {
                            if (levelHeigth[1] <= deapthHoles) { Destroy(cube); }
                            else
                            {
                                cube.transform.position += new Vector3(0, levelHeigth[1] * prefabPlanSol.transform.localScale.y / 2, 0);
                                cube.transform.name = $"{i},{j}";// Etage{levelHeigth[1]}";
                                cube.transform.localScale += new Vector3(0, prefabPlanSol.transform.localScale.y * levelHeigth[1], 0);
                            }
                        }
                        else if ((i <= Mathf.Max(levelCoordinate[0], levelCoordinate[2])) && (j > Mathf.Max(levelCoordinate[1], levelCoordinate[3])))
                        {
                            if (levelHeigth[2] <= deapthHoles) { Destroy(cube); }
                            else
                            {
                                cube.transform.position += new Vector3(0, levelHeigth[2] * prefabPlanSol.transform.localScale.y / 2, 0);
                                cube.transform.name = $"{i},{j}";// Etage{levelHeigth[2]}";
                                cube.transform.localScale += new Vector3(0, prefabPlanSol.transform.localScale.y * levelHeigth[2], 0);
                            }
                        }
                        else if ((i > Mathf.Max(levelCoordinate[0], levelCoordinate[2])) && (j >= Mathf.Min(levelCoordinate[1], levelCoordinate[3])))
                        {
                            if (levelHeigth[3] <= deapthHoles) { Destroy(cube); }
                            else
                            {
                                cube.transform.position += new Vector3(0, levelHeigth[3] * prefabPlanSol.transform.localScale.y / 2, 0);
                                cube.transform.name = $"{i},{j}";// Etage{levelHeigth[3]}";
                                cube.transform.localScale += new Vector3(0, prefabPlanSol.transform.localScale.y * levelHeigth[3], 0);
                            }
                        }
                        else if ((i >= Mathf.Min(levelCoordinate[0], levelCoordinate[2])) && (j < Mathf.Min(levelCoordinate[1], levelCoordinate[3])))
                        {
                            if (levelHeigth[4] <= deapthHoles) { Destroy(cube); }
                            else
                            {
                                cube.transform.position += new Vector3(0, levelHeigth[4] * prefabPlanSol.transform.localScale.y / 2, 0);
                                cube.transform.name = $"{i},{j}";// Etage{levelHeigth[4]}";
                                cube.transform.localScale += new Vector3(0, prefabPlanSol.transform.localScale.y * levelHeigth[4], 0);
                            }
                        }
                    }
                }
                position = new Vector3(positionIni[0] + (i + 1) * prefabPlanSol.transform.localScale.x, positionIni[1], positionIni[2]);
            }

            //position the gates accordingly
            if (wallSurrounding)
            {
                List<int> listIndexlvlCoord = new List<int> { 3, 2, 1, 0 };
                for (int i = 0; i < 4; i++)
                {
                    GameObject WallHolder = transform.GetChild(1).GetChild(i + 3).gameObject;
                    //WallHolder.transform.eulerAngles = new(0, 90 * i, 0);

                    int coordGate = 0;
                    if (levelCoordinate[listIndexlvlCoord[i]] > 0 && levelCoordinate[listIndexlvlCoord[i]] < dimensionSector[0])
                    {
                        coordGate = levelCoordinate[listIndexlvlCoord[i]];
                    }
                }
            }
        }

        [Button("Construct & Revert")]
        private void Construct()
        {
            //position the sector for a correct construction
            Vector3 positionArchive = transform.localPosition;
            transform.eulerAngles = Vector3.zero;
            transform.position = Vector3.zero;

            if (planMode)
            {
                //mise en place du sol
                grndHolder.SetActive(true);

                int lengthCubePlan = planSolHolder.transform.childCount;
                for (int i = 0; i < lengthCubePlan; i++)
                {
                    GameObject cube = planSolHolder.transform.GetChild(i).gameObject;
                    GameObject grnd = Instantiate(solElement[0]);

                    grnd.transform.position = planSolHolder.transform.GetChild(i).position + new Vector3(0, cube.transform.localScale.y / 2 + 0.001f, 0);
                    grnd.transform.parent = grndHolder.transform;
                }
                planMode = false;

                //mise en place des escaliers
                if (escalierHolder.transform.childCount > 0)
                {
                    GameObject holder = new GameObject();
                    holder.transform.position = Vector3.zero;
                    holder.transform.parent = transform;
                    holder.transform.name = "ModelEsc Holder";
                    escModelHolder = holder;

                    int lengthEscList = escalierHolder.transform.childCount;
                    for (int i = 0; i < lengthEscList; i++)
                    {
                        if (escalierHolder.transform.GetChild(i).gameObject.activeSelf == true)
                        {
                            for (int j = 0; j < (int)Mathf.Max(escalierHolder.transform.GetChild(i).localScale.x, escalierHolder.transform.GetChild(i).localScale.z) / 18; j++) // (dimensionSector[0]+1)
                            {
                                GameObject esc = Instantiate(solElement[1]);
                                int deplacement = j;
                                esc.transform.position = escalierHolder.transform.GetChild(i).position + new Vector3(0, ((int)(escalierHolder.transform.GetChild(i).localScale.y / 5) - j - 1) * 4.63f, 0);

                                //positioning according to the wall & ajust the orientation of the stairs
                                esc.transform.GetChild(0).position += new Vector3(0, 0, -1.6f);
                                if ((int)escalierHolder.transform.GetChild(i).rotation.eulerAngles.y == 0) { esc.transform.position += new Vector3(18 * deplacement, 0, 0); }
                                else if ((int)escalierHolder.transform.GetChild(i).rotation.eulerAngles.y == 90) { esc.transform.position += new Vector3(0, 0, -18 * deplacement); esc.transform.Rotate(0, 90, 0); }
                                else if ((int)escalierHolder.transform.GetChild(i).rotation.eulerAngles.y == 180) { esc.transform.position += new Vector3(-18 * deplacement, 0, 0); esc.transform.Rotate(0, 180, 0); }
                                else if ((int)escalierHolder.transform.GetChild(i).rotation.eulerAngles.y == 270) { esc.transform.position += new Vector3(0, 0, 18 * deplacement); esc.transform.Rotate(0, -90, 0); }

                                esc.transform.parent = holder.transform;
                                esc.transform.GetChild(0).gameObject.GetComponent<Escalier>().height = (int)escalierHolder.transform.GetChild(i).localScale.y / 5 - j - 1;
                                esc.transform.GetChild(0).gameObject.GetComponent<Escalier>().Updated();
                            }
                        }
                    }
                    escalierHolder.SetActive(false);
                }

                //mise en place des batiments
                if (batCoreHolder.transform.childCount > 0)
                {
                    int lengthBatList = batCoreHolder.transform.childCount;
                    for (int i = 0; i < lengthBatList; i++)
                    {
                        batCoreHolder.transform.GetChild(i).GetComponent<EchafConstructor>().Construct();
                    }
                }
            }
            else
            {
                DeleteChildren(grndHolder, "ground");

                try 
                {
                    DeleteChildren(escModelHolder);
                    DestroyImmediate(escModelHolder);
                }
                catch { Debug.LogError("Non destruction du porteur de model"); }

                //reversion du process de construction
                if (batCoreHolder.transform.childCount > 0)
                {
                    int lengthBatList = batCoreHolder.transform.childCount;
                    for (int i = 0; i < lengthBatList; i++)
                    {
                        batCoreHolder.transform.GetChild(i).GetComponent<EchafConstructor>().Construct();
                    }
                }

                grndHolder.SetActive(false);
                planSolHolder.SetActive(true);
                escalierHolder.SetActive(true);
                planMode = true;
            }

            //reposition the sector into the structure
            transform.localPosition = positionArchive;
            transform.localEulerAngles = Vector3.zero;
        }

        [Button("Cube Positioning")]
        public void Position()
        {
            //position the sector for a correct construction
            Vector3 positionArchive = transform.localPosition;
            transform.eulerAngles = Vector3.zero;
            transform.position = Vector3.zero;

            if (planMode)
            {
                //prepare les emptys pour le positionement
                DeleteChildren(escalierHolder, "Volumic Stairs");
                DeleteChildren(batCoreHolder, "Constructeur");

                //escalier
                //analyse le besoin d'etage
                List<int> besoinReelEtage = new List<int>();
                List<Vector3> floorBaseEsc = new List<Vector3>();
                for (int i = 0; i < levelHeigth.Count; i++)
                {
                    //vecteur escalier

                    if (levelHeigth[i] != levelHeigth[0])
                    {
                        if (Mathf.Min(levelHeigth[i], levelHeigth[0]) == levelHeigth[i])
                        { floorBaseEsc.Add(new Vector3(i, 0, Mathf.Abs(levelHeigth[i] - levelHeigth[0]))); }
                        else { floorBaseEsc.Add(new Vector3(0, i, Mathf.Abs(levelHeigth[i] - levelHeigth[0]))); }
                    }
                    else if (i < levelHeigth.Count - 1)
                    {
                        if (levelHeigth[i] != levelHeigth[i + 1] && i > 0)
                        {
                            if (Mathf.Min(levelHeigth[i], levelHeigth[i + 1]) == levelHeigth[i])
                            { floorBaseEsc.Add(new Vector3(i, i + 1, Mathf.Abs(levelHeigth[i] - levelHeigth[i + 1]))); }
                            else { floorBaseEsc.Add(new Vector3(i + 1, i, Mathf.Abs(levelHeigth[i] - levelHeigth[i + 1]))); }
                        }
                    }
                }

                //Definie les pointsBackup
                Vector3 pointBCB = pointOrBas + new Vector3(Mathf.Abs(levelCoordinate[0] - levelCoordinate[2]), 0, 0);
                Vector3 pointBCH = pointOrHaut + new Vector3(-Mathf.Abs(levelCoordinate[0] - levelCoordinate[2]), 0, 0);
                bool pointOrBasPris = false;
                bool pointOrHautPris = false;
                bool pointBCBPris = false;
                bool pointBCHPris = false;

                //Void de positionnement de la prefab & adaptation au terrain 
                void PoseCubeEsc(GameObject esc, Vector3 floorLink, Vector3 origin)
                {
                    esc.transform.localScale += new Vector3((floorLink.z - 1) * 18f, (floorLink.z - 1) * 5f, 0);
                    esc.transform.position = positionIni + 7f * origin + new Vector3(0, 2.5f + levelHeigth[(int)floorLink.x] * 5, 0);
                    esc.transform.parent = escalierHolder.transform;
                    esc.tag = "Volumic Stairs";
                }

                void CompactCubeEsc(GameObject escOrigin)
                {
                    GameObject escCompact = Instantiate(escalierHolder.transform.GetChild(0).gameObject);
                    escCompact.transform.position = escOrigin.transform.position;
                    escCompact.transform.Rotate(0, 180 + escOrigin.transform.eulerAngles.y, 0);
                    if ((int)escOrigin.transform.eulerAngles.y == 0) { escCompact.transform.position += new Vector3(14, 0, 0); }
                    else if ((int)escOrigin.transform.eulerAngles.y == 90) { escCompact.transform.position += new Vector3(0, 0, -14); }
                    else if ((int)escOrigin.transform.eulerAngles.y == 180) { escCompact.transform.position += new Vector3(-14, 0, 0); }
                    else if ((int)escOrigin.transform.eulerAngles.y == 270) { escCompact.transform.position += new Vector3(0, 0, 14); }

                    escCompact.transform.parent = escalierHolder.transform;
                    escCompact.tag = "Volumic Stairs";
                }

                //definie la surface constructibe par etage
                try { surfaceConstruct = new List<Quaternion>(); listMatrixConstructFloor = new List<List<List<bool>>>(); } catch { }
                surfaceConstruct.Add(new Quaternion(pointOrBas.x, pointOrBas.z, pointOrHaut.x + 1, pointOrHaut.z + 1));     //0     V
                surfaceConstruct.Add(new Quaternion(0, 0, pointBCH.x, pointBCH.z + 1));                                   //1       V
                surfaceConstruct.Add(new Quaternion(0, pointOrHaut.z, pointOrHaut.x + 1, dimensionSector[0]));                          //2         V
                surfaceConstruct.Add(new Quaternion(pointBCB.x, pointBCB.z, dimensionSector[0], dimensionSector[0] + 1));                             //3           V
                surfaceConstruct.Add(new Quaternion(pointOrBas.x, 0, dimensionSector[0] + 1, pointBCB.z));                          //4             V

                listMatrixConstructFloor = new List<List<List<bool>>>();
                for (int i = 0; i < 5; i++)
                { listMatrixConstructFloor.Add(matrixConstructFloor(surfaceConstruct[i])); }

                //position the lift plateform
                if (liftPlateform)
                {
                    //positionne la plateforme et ouvre le mur
                    if (pointOrBas.x > 0) { if (pointOrHaut.z > 5) { listMatrixConstructFloor[1][0][5] = false; listMatrixConstructFloor[1][1][5] = false; } else { listMatrixConstructFloor[2][0][(int)(5-pointOrHaut.z)] = false; listMatrixConstructFloor[2][1][(int)(5 - pointOrHaut.z)] = false; } }
                    else { if (pointOrBas.z > 5) { listMatrixConstructFloor[4][0][5] = false; listMatrixConstructFloor[4][1][5] = false; } else { if (pointOrHaut.z > 5) { listMatrixConstructFloor[0][0][5] = false; listMatrixConstructFloor[0][1][5] = false; } else { listMatrixConstructFloor[2][0][(int)(5-pointOrHaut.z)] = false; listMatrixConstructFloor[2][1][(int)(5 - pointOrHaut.z)] = false; } } }
                    prefabHolder.transform.GetChild(9).gameObject.SetActive(true);
                    if (wallSurrounding)
                    {
                        wallHolder.transform.GetChild(2).GetChild(0).localPosition = new(-81.9674f, 9.75f, -1.67f);
                        wallHolder.transform.GetChild(2).GetChild(0).localScale = new(79.62355f, 13.12504f, 3.435437f);
                    }

                    //prépare l'escalier pour entrer
                    for (int i = 0; i < 3; i++) { prefabHolder.transform.GetChild(9).Find("AccesSector").GetChild(i).gameObject.SetActive(false); }
                    for(int i=0;i< planSolHolder.transform.GetChild(5).localScale.y / 5; i++) { prefabHolder.transform.GetChild(9).Find("AccesSector").GetChild(i).gameObject.SetActive(true); }

                    if (planSolHolder.transform.GetChild(5).localScale.y == 15)
                    {
                        planSolHolder.transform.GetChild(5).localPosition = new(3.5f, 0, 38.5f);
                        planSolHolder.transform.GetChild(5).localScale = new(7, 10, 7);
                    }
                }
                else if(wallSurrounding)
                {
                    prefabHolder.transform.GetChild(9).gameObject.SetActive(false);
                    transform.Find("MurExtPrefab").GetChild(2).GetChild(0).localPosition = new(-78.4056f, 9.75f, -1.67f);
                    transform.Find("MurExtPrefab").GetChild(2).GetChild(0).localScale = new(86.74555f, 13.12504f, 3.435437f);
                }

                //application
                if (needOfCubeType[0])
                {
                    foreach (Vector3 floorLink in floorBaseEsc)
                    {
                        //position the stair on its floor
                        bool versCentre = false;
                        if (floorLink.y == 0) { versCentre = true; }

                        //center to Ext
                        if (floorLink.x == 0)
                        {
                            GameObject escCentre = Instantiate(escalierHolder.transform.GetChild(0).gameObject);

                            switch (floorLink.y)//if (floorLink.y == 1)
                            {
                                case 1:
                                    PoseCubeEsc(escCentre, floorLink, pointOrBas); pointOrBasPris = true;
                                    DePoseCubeAMatrice(0, new Quaternion(0, 0, 3, 1), true);   //DePoseCubeAMatrice(0, new Quaternion(pointOrBas.x, pointOrBas.z+1, 3*floorLink.z, 1), false);
                                    try { DePoseCubeAMatrice(1, new Quaternion(pointOrBas.x - 1, pointOrBas.z, 1, 1), true); } catch { }
                                    try { DePoseCubeAMatrice(0, new Quaternion(0, 0, floorLink.z * 3, 1), true); } catch { }//DePoseCubeAMatrice(1, new Quaternion(pointOrBas.x-1, pointOrBas.z, 1, 1), false);
                                    break;
                                case 2:
                                    PoseCubeEsc(escCentre, floorLink, pointBCH); pointBCHPris = true;
                                    DePoseCubeAMatrice(0, new Quaternion(0, pointBCH.z - pointOrBas.z - 3 + 1, 1, 3), true); //DePoseCubeAMatrice(0, new Quaternion(pointBCH.x, pointBCH.z - 3 * floorLink.z, 1, 3 * floorLink.z), false);
                                    try { DePoseCubeAMatrice(2, new Quaternion(pointBCH.x, 0, 1, 1), true); } catch { }
                                    try { DePoseCubeAMatrice(0, new Quaternion(0, pointBCH.z - pointOrBas.z - 3 + 1, 1, 3 * floorLink.z), true); } catch { }   //DePoseCubeAMatrice(2, new Quaternion(pointBCH.x, pointBCH.z+1, 1, 1), false);

                                    escCentre.transform.Rotate(0, 90, 0);
                                    break;
                                case 3:
                                    PoseCubeEsc(escCentre, floorLink, pointOrHaut); pointOrHautPris = true;
                                    DePoseCubeAMatrice(0, new Quaternion(pointOrHaut.x - pointOrBas.x - 2, pointOrHaut.z - pointOrBas.z, 3, 1), true);   //DePoseCubeAMatrice(0, new Quaternion(pointOrHaut.x - 3 * floorLink.z, pointOrHaut.z, 3 * floorLink.z, 1), false);
                                    try { DePoseCubeAMatrice(3, new Quaternion(0, pointOrHaut.z - pointOrBas.z, 1, 1), true); } catch { }
                                    try { DePoseCubeAMatrice(0, new Quaternion(pointOrHaut.x - pointOrBas.x - 2, pointOrHaut.z - pointOrBas.z, 3 * floorLink.z, 1), true); } catch { }  //DePoseCubeAMatrice(3, new Quaternion(pointOrHaut.x + 1, pointOrHaut.z, 1, 1), false);

                                    escCentre.transform.Rotate(0, 180, 0);
                                    break;
                                case 4:
                                    PoseCubeEsc(escCentre, floorLink, pointBCB); pointBCBPris = true;
                                    DePoseCubeAMatrice(0, new Quaternion(pointBCB.x - pointOrBas.x, 0, 1, 3), true);   //DePoseCubeAMatrice(0, new Quaternion(pointBCB.x, pointBCB.z, 1, 3 * floorLink.z), false);
                                    try { DePoseCubeAMatrice(4, new Quaternion(pointBCB.x - pointOrBas.x, pointBCB.z - 1, 1, 1), true); } catch { }
                                    try { DePoseCubeAMatrice(0, new Quaternion(pointBCB.x - pointOrBas.x, 0, 1, 3 * floorLink.z), true); } catch { }   //DePoseCubeAMatrice(4, new Quaternion(pointBCB.x - pointOrBas.x, pointBCB.z - 1, 1, 1), false);

                                    escCentre.transform.Rotate(0, -90, 0);
                                    break;
                            }

                            //verif (if useless)
                            if (levelCoordinate[0] == 0 && floorLink.y == 1) { DestroyImmediate(escCentre); }
                            else if (levelCoordinate[1] == 0 && floorLink.y == 4) { DestroyImmediate(escCentre); }
                            else if (levelCoordinate[2] == 17 && floorLink.y == 3) { DestroyImmediate(escCentre); }
                            else if (levelCoordinate[3] == 17 && floorLink.y == 2) { DestroyImmediate(escCentre); }

                            //verif (if too long)
                            else
                            {
                                if (((int)(escCentre.transform.localScale.x / 7) > Mathf.Abs(levelCoordinate[0] - levelCoordinate[2]) && (floorLink.y == 1 || floorLink.y == 3)) || ((int)(escCentre.transform.localScale.x / 7) > Mathf.Abs(levelCoordinate[1] - levelCoordinate[3]) && (floorLink.y == 4 || floorLink.y == 2)))
                                {
                                    if (floorLink.z > 2) { DestroyImmediate(escCentre); }
                                    else { CompactCubeEsc(escCentre); escCentre.transform.localScale += new Vector3(-18, 0, 0); }
                                }
                            }
                        }

                        //Ext to Center
                        else if (versCentre)
                        {
                            GameObject escVersCentre = Instantiate(escalierHolder.transform.GetChild(0).gameObject);

                            switch (floorLink.x)
                            {
                                case 1:
                                    if (pointBCHPris)
                                    {
                                        PoseCubeEsc(escVersCentre, floorLink, pointOrBas + new Vector3(-1, 0, 0));
                                        try
                                        {
                                            DePoseCubeAMatrice(1, new Quaternion(pointOrBas.x - 3, pointOrBas.z, 3, 1), false);
                                            DePoseCubeAMatrice(0, new Quaternion(pointOrBas.x, pointOrBas.z, 1, 1), false);
                                        }
                                        catch { }
                                        try { DePoseCubeAMatrice(1, new Quaternion(pointOrBas.x - 3, pointOrBas.z, 3 * floorLink.z, 1), false); } catch { }
                                    }
                                    else
                                    {
                                        PoseCubeEsc(escVersCentre, floorLink, pointBCH + new Vector3(-1, 0, 0));
                                        try
                                        {
                                            DePoseCubeAMatrice(1, new Quaternion(pointBCH.x - 3, pointBCH.z, 3, 1), false);
                                            DePoseCubeAMatrice(0, new Quaternion(pointBCH.x, pointBCH.z, 1, 1), false);
                                        }
                                        catch { }
                                        try { DePoseCubeAMatrice(1, new Quaternion(pointBCH.x - 3, pointBCH.z, 3 * floorLink.z, 1), false); } catch { }
                                    }
                                    break;
                                case 2:
                                    if (pointOrHautPris)
                                    {
                                        PoseCubeEsc(escVersCentre, floorLink, pointBCH + new Vector3(-1, 0, 0));
                                        try
                                        {
                                            DePoseCubeAMatrice(2, new Quaternion(pointBCH.x, pointBCH.z, 1, 3), false);
                                            DePoseCubeAMatrice(0, new Quaternion(pointBCH.x, pointBCH.z, 1, 1), false);
                                        }
                                        catch { }
                                        try { DePoseCubeAMatrice(2, new Quaternion(pointBCH.x, pointBCH.z, 1, 3 * floorLink.z), false); } catch { }
                                    }
                                    else
                                    {
                                        PoseCubeEsc(escVersCentre, floorLink, pointOrHaut + new Vector3(0, 0, 1)); escVersCentre.transform.Rotate(0, 90, 0);
                                        try
                                        {
                                            DePoseCubeAMatrice(2, new Quaternion(pointOrHaut.x, pointOrHaut.z, 1, 3), false);
                                            DePoseCubeAMatrice(0, new Quaternion(pointOrHaut.x, pointOrHaut.z, 1, 1), false);
                                        }
                                        catch { }
                                        try { DePoseCubeAMatrice(2, new Quaternion(pointOrHaut.x, pointOrHaut.z, 1, 3 * floorLink.z), false); } catch { }
                                    }
                                    break;
                                case 3:
                                    if (pointBCBPris)
                                    {
                                        PoseCubeEsc(escVersCentre, floorLink, pointOrHaut + new Vector3(-1, 0, 0));
                                        try
                                        {
                                            DePoseCubeAMatrice(3, new Quaternion(pointOrHaut.x, pointOrHaut.z, 3, 1), false);
                                            DePoseCubeAMatrice(0, new Quaternion(pointOrHaut.x, pointOrHaut.z, 1, 1), false);
                                        }
                                        catch { }
                                        try { DePoseCubeAMatrice(3, new Quaternion(pointOrHaut.x, pointOrHaut.z, 3 * floorLink.z, 1), false); } catch { }
                                    }
                                    else
                                    {
                                        PoseCubeEsc(escVersCentre, floorLink, pointBCB + new Vector3(1, 0, 0)); escVersCentre.transform.Rotate(0, 180, 0);
                                        try
                                        {
                                            DePoseCubeAMatrice(3, new Quaternion(pointBCB.x, pointBCB.z, 3, 1), false);
                                            DePoseCubeAMatrice(0, new Quaternion(pointBCB.x, pointBCB.z, 1, 1), false);
                                        }
                                        catch { }
                                        try { DePoseCubeAMatrice(3, new Quaternion(pointBCB.x, pointBCB.z, 3 * floorLink.z, 1), false); } catch { }
                                    }
                                    break;
                                case 4:
                                    if (pointOrBasPris)
                                    {
                                        PoseCubeEsc(escVersCentre, floorLink, pointBCB + new Vector3(-1, 0, 0));
                                        try
                                        {
                                            DePoseCubeAMatrice(4, new Quaternion(pointBCB.x, pointBCB.z - 1 - 3, 1, 3), false);
                                            DePoseCubeAMatrice(0, new Quaternion(pointBCB.x, pointBCB.z, 1, 1), false);
                                        }
                                        catch { }
                                        try { DePoseCubeAMatrice(4, new Quaternion(pointBCB.x, pointBCB.z - 1 - 3, 1, 3 * floorLink.z), false); } catch { }
                                    }
                                    else
                                    {
                                        PoseCubeEsc(escVersCentre, floorLink, pointOrBas + new Vector3(0, 0, -1)); escVersCentre.transform.Rotate(0, -90, 0);
                                        try
                                        {
                                            DePoseCubeAMatrice(4, new Quaternion(pointOrBas.x, pointOrBas.z - 3, 1, 3), false);
                                            DePoseCubeAMatrice(0, new Quaternion(pointOrBas.x, pointOrBas.z, 1, 1), false);
                                        }
                                        catch { }
                                        try { DePoseCubeAMatrice(4, new Quaternion(pointOrBas.x, pointOrBas.z - 3, 1, 3 * floorLink.z), false); } catch { }
                                    }
                                    break;
                            }

                            escVersCentre.transform.Rotate(0, 180, 0);

                            //verif (if useless)
                            if (escVersCentre.transform.position.x > sizeSol + 2.5 || escVersCentre.transform.position.x < 0) { DestroyImmediate(escVersCentre); }
                            else if (escVersCentre.transform.position.z > sizeSol + 2.5 || escVersCentre.transform.position.z < 0) { DestroyImmediate(escVersCentre); }

                            //verif (if too long)
                            else
                            {
                                if ((escVersCentre.transform.localScale.x / 7 > levelCoordinate[0] + 1 && floorLink.x == 1) || (escVersCentre.transform.localScale.x / 7 > dimensionSector[0] - levelCoordinate[3] && floorLink.x == 2) || (escVersCentre.transform.localScale.x / 7 > dimensionSector[0] - levelCoordinate[2] + 1 && floorLink.x == 3) || (escVersCentre.transform.localScale.x / 7 > levelCoordinate[1] && floorLink.x == 4))
                                {
                                    if (floorLink.z > 2) { DestroyImmediate(escVersCentre); }
                                    else { CompactCubeEsc(escVersCentre); escVersCentre.transform.localScale += new Vector3(-18, 0, 0); }
                                }
                            }
                        }
                    }
                }

                //batiments
                //define le nombre de batiment à positionner
                int prefabChildCount = prefabHolder.transform.childCount;
                int nbTypeBat = 0;
                for (int i = 0; i < prefabChildCount; i++)
                { if (prefabHolder.transform.GetChild(i).name.StartsWith("immeuble")) { nbTypeBat++; } }

                //positionne les cubes de bat type par type
                for (int a = 0; a < nbTypeBat; a++)
                {
                    if (needOfCubeType[a + 1])
                    {
                        //recupère les infos du constructeur pour selectionner les zones constructibles
                        GameObject constructBatType = prefabHolder.transform.GetChild(a).gameObject;
                        for (int i = 0; i < 5; i++)
                        {
                            int rotationBlocFloor = 0;
                            GameObject imConstructorSousSect = Instantiate(constructBatType);
                            imConstructorSousSect.transform.parent = batCoreHolder.transform;
                            imConstructorSousSect.transform.name += $" Floor {i}";
                            EchafConstructor constructor = imConstructorSousSect.GetComponent<EchafConstructor>();

                            GameObject cube = imConstructorSousSect.transform.GetChild(0).gameObject;
                            cube.transform.position = new Vector3(0, constructor.heightCoeff, 0);

                            if (listMatrixConstructFloor[i].Count > 0 && listMatrixConstructFloor[i][0].Count > 0)
                            {
                                for (int absX = 0; absX < listMatrixConstructFloor[i].Count; absX++)
                                {
                                    for (int ordZ = 0; ordZ < listMatrixConstructFloor[i][0].Count; ordZ++)
                                    {

                                        /*Regles
                                         * L1_DosAuxFrontières :> checkLongueurMatrix {alongZ -> x; alongX -> z;                            V
                                         * L2_Expension[Small_Bat]:a={1} :> While 'place' -> extension{. 1|3: alongZ; . 2|4: alongX;        
                                         */

                                        if (listMatrixConstructFloor[i][absX][ordZ])
                                        {
                                            int repeatIndex = 0;
                                            bool repeatAlongX = true;
                                            float cubeSolXSize = prefabPlanSol.transform.localScale.x;
                                            int facedOnRightAxis = 0; // 0 ->rien; 1 -> += 7x; 2 -> += 7z
                                            while (testDePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize), Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize) + 1)) && repeatAlongX || testDePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize))) && repeatAlongX == false)
                                            {

                                                //Position Option  
                                                void PositionEnRepeatAlongX()
                                                {
                                                    im.transform.GetChild(0).Rotate(0, -90, 0);
                                                    float xScale = constructor.lenghtCoeff / 2;
                                                    float zScale = constructor.thicknessCoeff / 2;
                                                    im.transform.GetChild(0).position += new Vector3(zScale - xScale, 0, xScale - zScale);
                                                }

                                                void FaceTheRightDirection()
                                                {
                                                    try
                                                    {
                                                        if (im.transform.GetChild(0).eulerAngles.y == 0 && testDePoseCubeAMatrice(i, new Quaternion(absX + Mathf.Ceil(constructor.lenghtCoeff / 7) + 1, ordZ, 1, (int)(constructor.thicknessCoeff / 7))) == false)
                                                        { im.transform.GetChild(0).Rotate(0, 180, 0); im.transform.GetChild(0).position += new Vector3(7, 0, 0); facedOnRightAxis = 1; return; }
                                                    }
                                                    catch { im.transform.GetChild(0).Rotate(0, 180, 0); im.transform.GetChild(0).position += new Vector3(7, 0, 0); facedOnRightAxis = 1; return; }

                                                    try
                                                    {
                                                        if (im.transform.GetChild(0).eulerAngles.y == -90 && testDePoseCubeAMatrice(i, new Quaternion(absX, ordZ + Mathf.Ceil(constructor.lenghtCoeff / 7) + 1, Mathf.Ceil(constructor.thicknessCoeff / 7), 1)) == false)
                                                        { im.transform.GetChild(0).Rotate(0, 180, 0); im.transform.GetChild(0).position += new Vector3(0, 0, 7); facedOnRightAxis = 2; }
                                                    }
                                                    catch { im.transform.GetChild(0).Rotate(0, 180, 0); im.transform.GetChild(0).position += new Vector3(0, 0, 7); facedOnRightAxis = 2; }
                                                }

                                                void Adosse(bool alongX)
                                                {
                                                    if (CheckAdossement(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize), Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize)), alongX))
                                                    {
                                                        if (alongX)
                                                        { im.transform.GetChild(0).Rotate(0, 180, 0); im.transform.GetChild(0).position += new Vector3(0, 0, cubeSolXSize); facedOnRightAxis = 1; return; }
                                                        else { im.transform.GetChild(0).Rotate(0, 180, 0); im.transform.GetChild(0).position += new Vector3(cubeSolXSize, 0, 0); facedOnRightAxis = 1; return; }
                                                    }

                                                }

                                                bool CheckAdossement(int floor, Quaternion posDim, bool alongX)
                                                {
                                                    float tailleMatrix = 0;
                                                    if (alongX)
                                                    { tailleMatrix = listMatrixConstructFloor[floor][0].Count; }
                                                    else { tailleMatrix = listMatrixConstructFloor[floor].Count; }

                                                    float coordTest = 0;
                                                    Quaternion posDimLocal = posDim;
                                                    if (alongX)
                                                    { coordTest = posDimLocal.y + posDimLocal.w + 1; }
                                                    else { coordTest = posDimLocal.x + posDimLocal.z + 1; }

                                                    if (coordTest >= tailleMatrix)
                                                    { return true; }
                                                    else { return false; }
                                                }

                                                //Position of buid pivot
                                                if (repeatIndex == 0)                   //default in repeatAlongX ; face up
                                                {
                                                    im = Instantiate(cube);
                                                    repeatAlongX = true;
                                                    Quaternion globalePosition = GlobalPositionFromLocal(i, new Quaternion(absX, ordZ, (int)(constructor.lenghtCoeff / cubeSolXSize), (int)(constructor.thicknessCoeff / cubeSolXSize)));
                                                    im.transform.position += new Vector3(globalePosition.x * cubeSolXSize + 3.5f, levelHeigth[i] * 5, globalePosition.y * cubeSolXSize + 3.5f);
                                                    im.transform.parent = imConstructorSousSect.transform;

                                                    //ATTENTION INVERSION DES ORIENTAIONS POUR EXPENSION : cubeDefaut en repeatAlongZ
                                                    if (i == 1 || i == 4)
                                                    {
                                                        //repeatAlongZ
                                                        if (testDePoseCubeAMatrice(i, new Quaternion(absX, ordZ + Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize), Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize))))
                                                        {
                                                            repeatAlongX = false;
                                                            DePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize)), true);
                                                        }
                                                        //repeatAlongX
                                                        else if (testDePoseCubeAMatrice(i, new Quaternion(absX + Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize), ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize), Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize) + 1)))
                                                        {
                                                            PositionEnRepeatAlongX();
                                                            DePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / 7), Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize) + 1), true);
                                                        }
                                                        //defaut
                                                        else
                                                        {
                                                            if (testDePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize))))
                                                            { DePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize)), true); repeatAlongX = false; }
                                                            else { DePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize), Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize) + 1), true); PositionEnRepeatAlongX(); }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //repeatAlongX
                                                        if (testDePoseCubeAMatrice(i, new Quaternion(absX + Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize), ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize), Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize) + 1)))
                                                        {
                                                            PositionEnRepeatAlongX();
                                                            DePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize), Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize) + 1), true);
                                                        }
                                                        //repeatAlongZ
                                                        else if (testDePoseCubeAMatrice(i, new Quaternion(absX, ordZ + Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize), Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize))))
                                                        {
                                                            repeatAlongX = false;
                                                            DePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize)), true);
                                                        }
                                                        //defaut
                                                        else
                                                        {
                                                            if (testDePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize))))
                                                            { DePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize)), true); repeatAlongX = false; }
                                                            else { DePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize), Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize) + 1), true); PositionEnRepeatAlongX(); }
                                                        }
                                                    }
                                                    Adosse(repeatAlongX);

                                                    if (heigthDisplay) { im.transform.GetChild(0).localScale += new Vector3(0, constructor.heightCoeff * Random.Range(3, 6 - i), 0); }

                                                    if (a == 0) { break; }
                                                }

                                                //length of final build
                                                else
                                                {
                                                    if (repeatAlongX)
                                                    {
                                                        im.transform.GetChild(0).localScale += new Vector3(0, 0, constructor.thicknessCoeff);
                                                        im.transform.GetChild(0).position += new Vector3(constructor.thicknessCoeff / 2, 0, 0);
                                                        if (a == 1)
                                                        { DePoseCubeAMatrice(i, new Quaternion(absX - repeatIndex + 1, ordZ, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize), Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1), true); }
                                                        else
                                                        { DePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize), Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1), true); }
                                                    }
                                                    else
                                                    {
                                                        im.transform.GetChild(0).localScale += new Vector3(0, 0, constructor.thicknessCoeff);
                                                        im.transform.GetChild(0).position += new Vector3(0, 0, constructor.thicknessCoeff / 2);
                                                        if (a == 1)
                                                        { DePoseCubeAMatrice(i, new Quaternion(absX, ordZ - repeatIndex + 1, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize)), true); }
                                                        else
                                                        { DePoseCubeAMatrice(i, new Quaternion(absX, ordZ, Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize) + 1, Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize)), true); }

                                                    }
                                                }

                                                im.transform.eulerAngles += new Vector3(0, rotationBlocFloor, 0);

                                                repeatIndex++;
                                                if (repeatAlongX) { absX += (int)(Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize)); }
                                                else { ordZ += (int)(Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize)); }
                                            }

                                            //verif de positionement

                                            if (repeatAlongX) { absX -= repeatIndex * ((int)(Mathf.Ceil(constructor.lenghtCoeff / cubeSolXSize))); }
                                            else { ordZ -= repeatIndex * ((int)(Mathf.Ceil(constructor.thicknessCoeff / cubeSolXSize))); }
                                        }

                                    }
                                }
                            }
                            cube.SetActive(false);

                            if (i > 0) { rotationBlocFloor += 90; }
                        }
                    }
                }
            }

            //ajust crossing stairs in constructMode
            else
            {
                try
                {
                    int childCount = escModelHolder.transform.childCount;
                    for (int i = 0; i < childCount; i++) { escModelHolder.transform.GetChild(i).GetChild(0).GetComponent<Escalier>().CheckNeighbours(); }
                }
                catch { }
            }

            //reposition the sector into the structure
            transform.localPosition = positionArchive;
            transform.localEulerAngles = Vector3.zero;
        }

        [Button("Afficher la matrice")]
        private void AfficheMatrix()
        {
            if (matrixPrinted)
            {
                DeleteChildren(matrixHolder, "Matrix Printed");
                matrixPrinted = false;
            }
            else
            {
                int height = 0;
                foreach(List<List<bool>> tableau in listMatrixConstructFloor)
                {
                    for(int i = 0; i < tableau.Count; i++)
                    {
                        for(int j = 0; j < tableau[0].Count; j++)
                        {
                            if (tableau[i][j])
                            {
                                GameObject cubeMatrix = Instantiate(prefabPlanSol);
                                Quaternion position = GlobalPositionFromLocal(height, new Quaternion(i, j, 0, 0));
                                cubeMatrix.transform.position = positionIni + new Vector3(7 * position[0], 50 + height, 7 * position[1]);
                                cubeMatrix.transform.parent = matrixHolder.transform;
                                cubeMatrix.transform.name = $"Floor:{height} dim:{i},{j}";
                                cubeMatrix.transform.tag = "Matrix Printed";
                            }
                        }
                    }
                    height++;
                }
                matrixPrinted = true;
            }
        }

        [Button("Reset Catwalk")]
        public void ResetCatWalk()
        {
            List<float> listPointCheck = new List<float> { pointOrHaut.z, pointOrHaut.x, pointOrHaut.z, pointOrBas.z };

            if (listCubeCatWalk.Count > 0)
            {
                foreach(GameObject elem in listCubeCatWalk)
                {
                    Destroy(elem);
                }
            }
            listCubeCatWalk.Clear();

            GameObject echafEsc = prefabHolder.transform.GetChild(2).gameObject;
            GameObject echafPlat = prefabHolder.transform.GetChild(7).gameObject;

            for (int i = 0; i < 4; i++)
            {
                GameObject cube = Instantiate(echafPlat.transform.GetChild(0).gameObject);
                cube.transform.position = listCoordCorner[i];
                float lengthcubePlat = 0;
                bool notAllLength = false;

                float levelDiff = (planSolHolder.transform.GetChild(listIndexPosition[i]).localScale.y - planSolHolder.transform.GetChild(listIndexPosition[i + 1]).localScale.y) / 5;
                if (levelDiff == 0)
                { lengthcubePlat = 138; notAllLength = false; }
                else
                {
                    float lengthPointCheck = 0;
                    if (i >= 2) { lengthPointCheck = listPointCheck[i]; }
                    else { lengthPointCheck = listPointCheck[i]; }
                    lengthcubePlat = 6 * lengthPointCheck + (int)(lengthPointCheck / 6); notAllLength = true;
                }
                cube.transform.GetChild(0).localScale = new Vector3(2, 5.5f, lengthcubePlat);
                if (i % 2 != 0)
                { cube.transform.eulerAngles = new Vector3(0, 90 * i, 0); }
                cube.transform.GetChild(0).localPosition = new Vector3(0, planSolHolder.transform.GetChild(listIndexPosition[i]).localScale.y, lengthcubePlat / 2);
                if (i == 2) { cube.transform.Rotate(0, 180, 0); }
                cube.transform.parent = echafPlat.transform;
                listCubeCatWalk.Add(cube);

                if (notAllLength)
                {
                    for (int j = 0; j < Mathf.Abs(levelDiff); j++)
                    {
                        GameObject cubEsc = Instantiate(echafEsc.transform.GetChild(0).gameObject);
                        cubEsc.transform.GetChild(0).localScale = new Vector3(6, 5.5f, 2);
                        cubEsc.transform.position = listCoordCorner[i];

                        if (levelDiff > 0)
                        {
                            cubEsc.transform.GetChild(0).localPosition = new Vector3(0, planSolHolder.transform.GetChild(listIndexPosition[i]).localScale.y - j * 5f, lengthcubePlat + j * 6.1f);
                            cubEsc.transform.Rotate(0, 90 * i, 0);

                        }
                        else if (levelDiff < 0)
                        {
                            cubEsc.transform.GetChild(0).localPosition = new Vector3(0, planSolHolder.transform.GetChild(listIndexPosition[i]).localScale.y + (j + 1) * 5f, -(lengthcubePlat + j * 6.1f));
                            cubEsc.transform.Rotate(0, 180 + 90 * i, 0);

                        }
                        if (j < 0) { cubEsc.transform.GetChild(0).localPosition += new Vector3(0, 0.5f, levelDiff / Mathf.Abs(levelDiff) * 1); }

                        cubEsc.transform.parent = echafEsc.transform;
                        listCubeCatWalk.Add(cubEsc);
                    }

                    GameObject cubeSuite = Instantiate(echafPlat.transform.GetChild(0).gameObject);
                    cubeSuite.transform.position = listCoordCorner[i];
                    cubeSuite.transform.GetChild(0).localScale = new Vector3(2, 5.5f, 138-lengthcubePlat - Mathf.Abs(levelDiff)*6.1f);
                    cubeSuite.transform.eulerAngles = new Vector3(0, 90 * i, 0);

                    float indexHeight = 0;
                    if(levelDiff > 0) { indexHeight = -levelDiff * 5.5f; }
                    else if(levelDiff < 0) { indexHeight = -levelDiff * 5.5f; }
                    cubeSuite.transform.GetChild(0).localPosition = new Vector3(0, planSolHolder.transform.GetChild(listIndexPosition[i]).localScale.y + indexHeight, lengthcubePlat + Mathf.Abs(levelDiff) * 6.1f + (138 - lengthcubePlat - Mathf.Abs(levelDiff) * 6.1f) /2);

                    cubeSuite.transform.parent = echafPlat.transform;
                    listCubeCatWalk.Add(cubeSuite);

                }
            }
        }

        public bool WallCheck()
        {
            /*wall rules: 
             *         V   1- neighbor(0-3) : connection to subdiv>itself
             *         V   2- all : top && neighbor biome == openspace 
             *         V   3- all : top || bottom == openspace + neighbor == empty
             * */

            //define the need of wall around it
            List<bool> faceInNeed = new List<bool>() { false, false, false, false };
            bool needConstruct = false;
            for(int i = 0; i < 4; i++)
            {
                // _1
                if (neighbors[i].subdivision > subdivision)
                {
                    faceInNeed[i] = true;
                }
                //_2
                else if(neighbors[i].type == Sectorassembly.Neighbors.neighborsTypes.OpenSpace)
                {
                    if (neighbors[4].type == Sectorassembly.Neighbors.neighborsTypes.OpenSpace)
                    {
                        faceInNeed[i] = true;
                    }
                }
                //_3
                else if(neighbors[i].type == Sectorassembly.Neighbors.neighborsTypes.EmptySpace)
                {
                    if (neighbors[4].type == Sectorassembly.Neighbors.neighborsTypes.OpenSpace || neighbors[5].type == Sectorassembly.Neighbors.neighborsTypes.OpenSpace)
                    {
                        faceInNeed[i] = true;
                    }
                }
            }
            //Update the need of wall
            need = faceInNeed;
            if (faceInNeed != wallSide) { return true; }
            else { return false; }
        }

        [Button("Build Walls")]
        public void WallBuilding(bool reset = false) 
        {
            //use it only on unwalled sector and check the need to recreate old walls
            if(wallSurrounding == false && WallCheck())
            {
                if (reset)
                {
                    //Deleate the old walls
                    DeleteChildren(wallHolder);
                    wallSide = new List<bool>() { false, false, false, false };
                }

                //Create the wall where it is needed
                bool wallSucced = false;
                void WallConstruct(int position)
                {
                    GameObject wallPrefab = transform.GetChild(1).GetChild(2).gameObject;
                    for(int i=0; i < dimensionSector[0]; i++)
                    {

                        GameObject newWall = Instantiate(wallPrefab);
                        newWall.transform.localScale = new Vector3(7, transform.parent.GetComponent<Sectorassembly>().spaceForEachSector[1], 7);
                        newWall.transform.parent = planSolHolder.transform;
                        newWall.tag = "Volumic Sol";
                        try
                        {
                            switch (position)
                            {
                                case 0:
                                    newWall.transform.localPosition = 7 * new Vector3(dimensionSector[0] - 1, -5 / 7, i);
                                    Destroy(planSolHolder.transform.Find($"{dimensionSector[0] - 1},{i}").gameObject);
                                    break;
                                case 1:
                                    newWall.transform.localPosition = 7 * new Vector3(0, -5 / 7, i);
                                    Destroy(planSolHolder.transform.Find($"0,{i}").gameObject);
                                    break;
                                case 2:
                                    newWall.transform.localPosition = 7 * new Vector3(i, -5 / 7, dimensionSector[0] - 1);
                                    Destroy(planSolHolder.transform.Find($"{i},{dimensionSector[0] - 1}").gameObject);
                                    break;
                                case 3:
                                    newWall.transform.localPosition = 7 * new Vector3(i, -5 / 7, 0);
                                    Destroy(planSolHolder.transform.Find($"{i},0").gameObject);
                                    break;
                            }
                        }
                        catch { Debug.LogWarning("No Sol Found"); }
                    }
                    wallSucced = true;
                }

                for (int i = 0; i < 4; i++)
                {
                    if (need[i] && reset)
                    {
                        WallConstruct(i);
                        if (wallSucced) { wallSide[i] = true; }
                    }
                    else if(need[i] && wallSide[i] == false)
                    {
                        WallConstruct(i);
                        if (wallSucced) { wallSide[i] = true; }
                    }
                }
            }
        }

        public void DeleteChildren(GameObject parent, string tag = "")
        {
            int childCount = parent.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                if (tag == "" || parent.transform.GetChild(i).CompareTag(tag))
                {
                    Destroy(parent.transform.GetChild(i).gameObject);
                    try { DestroyImmediate(parent.transform.GetChild(i).gameObject); } catch { }
                    childCount--;
                    i--;
                }
            }
        }
        
        private List<List<bool>> matrixConstructFloor(Quaternion dimension)
        {
            List<List<bool>> matrix = new List<List<bool>>();
            for(int i = 0; i < dimension.z - dimension.x; i++)
            {
                matrix.Add(new List<bool>());
                for(int j = 0; j < dimension.w - dimension.y; j++)
                {
                    matrix[i].Add(true);
                }
            }
            return matrix;
        }
    
        private bool testDePoseCubeAMatrice(int floor, Quaternion localPositionDimension)
        {
            //Quaternion localPositionDimension = localPositionFromGlobal(floor, globalePositionDimension);
            bool test = true;
            for (int i = (int)localPositionDimension.x; i < localPositionDimension.x + localPositionDimension.z; i++)
            {
                for (int j = (int)localPositionDimension.y; j < localPositionDimension.y + localPositionDimension.w; j++)
                {
                    try { if (listMatrixConstructFloor[floor][i][j] == false) { test = false; } } catch { test = false; }
                }
            }
            return test;
        }

        private void DePoseCubeAMatrice(int floor, Quaternion globalePositionDimension, bool local) 
        {
            //transform global position into local one usable by alone matrix
            Quaternion localPositionDimension = new Quaternion();
            if (local) { localPositionDimension = globalePositionDimension; }
            else { localPositionDimension = LocalPositionFromGlobal(floor, globalePositionDimension); }

            for (int i=(int)localPositionDimension.x; i < localPositionDimension.x + localPositionDimension.z; i++)
            {
                for(int j=(int)localPositionDimension.y; j < localPositionDimension.y + localPositionDimension.w; j++)
                {
                    listMatrixConstructFloor[floor][i][j] = false;
                }
            }
        }

        private Quaternion LocalPositionFromGlobal(int floor, Quaternion globalePositionDimension)
        {
            Quaternion localPositionDimension = new Quaternion(0, 0, globalePositionDimension.z, globalePositionDimension.w);
            switch (floor)
            {
                case 0:
                    localPositionDimension.x = globalePositionDimension.x - pointOrBas.x;
                    localPositionDimension.y = globalePositionDimension.y - pointOrBas.z;
                    break;
                case 1:
                    localPositionDimension.x = globalePositionDimension.x;
                    localPositionDimension.y = globalePositionDimension.y;
                    break;
                case 2:
                    localPositionDimension.x = globalePositionDimension.x;
                    localPositionDimension.y = globalePositionDimension.y - pointOrHaut.z;
                    break;
                case 3:
                    localPositionDimension.x = globalePositionDimension.x - pointOrHaut.x;
                    localPositionDimension.y = globalePositionDimension.y - pointOrBas.z;
                    break;
                case 4:
                    localPositionDimension.x = globalePositionDimension.x - pointOrBas.x;
                    localPositionDimension.y = globalePositionDimension.y;
                    break;
            }
            return localPositionDimension;
        }

        private Quaternion GlobalPositionFromLocal(int floor, Quaternion localPositionDimension)
        {
            Quaternion globalPositionDimension = new Quaternion(0, 0, localPositionDimension.z, localPositionDimension.w);
            switch (floor)
            {
                case 0:
                    globalPositionDimension.x = localPositionDimension.x + pointOrBas.x;
                    globalPositionDimension.y = localPositionDimension.y + pointOrBas.z;
                    break;
                case 1:
                    globalPositionDimension.x = localPositionDimension.x;
                    globalPositionDimension.y = localPositionDimension.y;
                    break;
                case 2:
                    globalPositionDimension.x = localPositionDimension.x;
                    globalPositionDimension.y = localPositionDimension.y + pointOrHaut.z+1;
                    break;
                case 3:
                    globalPositionDimension.x = localPositionDimension.x + pointOrHaut.x+1;
                    globalPositionDimension.y = localPositionDimension.y + pointOrBas.z;
                    break;
                case 4:
                    globalPositionDimension.x = localPositionDimension.x + pointOrBas.x;
                    globalPositionDimension.y = localPositionDimension.y;
                    break;
            }
            return globalPositionDimension;
        }

        public void ConstructCatWalk()
        {
            GameObject echafEsc = prefabHolder.transform.GetChild(2).gameObject;
            GameObject echafPlat = prefabHolder.transform.GetChild(7).gameObject;

            echafEsc.GetComponent<EchafConstructor>().Construct();
            echafPlat.GetComponent<EchafConstructor>().Construct();
        }

        public void Naming()
        {
            foreach(TMP_Text text in listNameHolder)
            {
                text.text = gameObject.name;
            }
        }


        public void Start()
        {
            //set up the inner variables 
            int wallOffset = 0;
            if (wallSurrounding) { wallOffset = 1; }
            wallHolder = transform.GetChild(1).gameObject;
            float cubeSolXSize = prefabPlanSol.transform.localScale.x;
            float cubeSolYSize = prefabPlanSol.transform.localScale.y;

            sizeSol = dimensionSector[0] * prefabPlanSol.transform.localScale.x;


            //set up of the player sensor
            BoxCollider boxPIBIn = gameObject.AddComponent<BoxCollider>();
            boxPIBIn.isTrigger = true;
            boxPIBIn.center = new Vector3((dimensionSector[0] + wallOffset) *  cubeSolXSize/ 2, dimensionSector[1]*cubeSolYSize, (dimensionSector[0]+ wallOffset) * cubeSolXSize / 2);
            boxPIBIn.size = new((dimensionSector[0] + wallOffset) * cubeSolXSize, dimensionSector[1] * cubeSolYSize, (dimensionSector[0] + wallOffset) * cubeSolXSize);
            try { structureScript = gameObject.GetComponentInParent<Sectorassembly>(); } catch { }
        }

        public void Update()
        {
            if (performanceActivated)
            {
                //active the inner part if the player is inside or can see it
                if (PIS == false)
                {
                    batCoreHolder.SetActive(insideSeen);
                    escalierHolder.SetActive(insideSeen);
                    planSolHolder.SetActive(insideSeen);
                    grndHolder.SetActive(insideSeen);

                    if (insideSeen && planMode == false)
                    {
                        if (nonUpdated)
                        {
                            //DeleteChildren(grndHolder, "ground");                                                 //Ground    V
                            int childCount = grndHolder.transform.childCount;
                            //Debug.Log(childCount);
                            for (int i = 0; i < childCount; i++)
                            { //if( grndHolder.transform.GetChild(i).CompareTag("ground"))
                                Destroy(grndHolder.transform.GetChild(i).gameObject);
                                childCount--;
                                i--;
                            }

                            Destroy(grndHolder);

                            GameObject grndReplace = new GameObject();
                            grndReplace.transform.parent = gameObject.transform;
                            grndReplace.transform.localPosition = Vector3.zero;
                            grndReplace.transform.name = "Ground Holder";
                            grndHolder = grndReplace;

                            //                                                                                          //Escalier  V
                            try
                            {
                                //DeleteChildren(escModelHolder);
                                childCount = escModelHolder.transform.childCount;
                                for (int i = 0; i < childCount; i++)
                                { //if( escModelHolder.transform.GetChild(i).CompareTag(""))
                                    Destroy(escModelHolder.transform.GetChild(i).gameObject); childCount--; i--;
                                }
                                Destroy(escModelHolder);
                            }
                            catch { }

                            //reversion du process de construction                                                      //Bat
                            if (batCoreHolder.transform.childCount > 0)
                            {
                                int lengthBatList = batCoreHolder.transform.childCount;
                                for (int i = 0; i < lengthBatList; i++)
                                {
                                    batCoreHolder.transform.GetChild(i).GetComponent<EchafConstructor>().Construct();
                                }
                            }
                            Destroy(batCoreHolder);

                            GameObject batCoreReplace = new GameObject();
                            batCoreReplace.transform.parent = gameObject.transform;
                            batCoreReplace.transform.localPosition = Vector3.zero;
                            batCoreReplace.transform.name = "BatCore Holder";
                            batCoreHolder = batCoreReplace;
                            if(transform.parent.GetComponent<Sectorassembly>().organisation == organizationType.City || transform.parent.GetComponent<Sectorassembly>().organisation == organizationType.StructureSingle)
                            {
                                bool isLiftStairsup = prefabHolder.transform.Find("ArrivéeLift").Find("AccesSector").GetChild(2).gameObject.activeSelf;
                                prefabHolder.transform.Find("ArrivéeLift").Find("AccesSector").GetChild(2).gameObject.SetActive(isLiftStairsup);
                            }
                            //                                                                                         //Reset Flag
                            planMode = true;
                            Position();

                            grndHolder.SetActive(false);
                            planSolHolder.SetActive(true);
                            escalierHolder.SetActive(true);

                            nonUpdated = false;
                        }
                    }
                }
                else
                {
                    batCoreHolder.SetActive(true);
                    //escalierHolder.SetActive(true);
                    planSolHolder.SetActive(true);
                    grndHolder.SetActive(true);

                    try
                    {
                        structureScript.playerInSector = true;
                        structureScript.sectorHabitated = new Vector3(Mathf.Ceil(transform.localPosition.x / 150), Mathf.Ceil(transform.localPosition.y / 40), Mathf.Ceil(transform.localPosition.z / 150));
                    }
                    catch { }
                }
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.transform.tag == "Player" && planMode) 
            {
                if(other.gameObject.GetComponent<Playermovment>().lineConnected == false)
                {
                    Construct();
                    try
                    {
                        structureScript.PlayerInSector(true);
                        structureScript.sectorHabitated = new Vector3(Mathf.Ceil(transform.position.x / 150), Mathf.Ceil(transform.position.y / 40), Mathf.Ceil(transform.position.z / 150));
                    }
                    catch { }
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.GetComponent<Playermovment>().lineConnected == false && other.gameObject.transform.tag == "Player")
            {
                PIS = true;
                insideSeen = true;
                nonUpdated = true;
            }   
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.transform.tag == "Player")// && other.gameObject.GetComponent<Playermovment>().lineConnected == false) 
            {
                PIS = false;
                insideSeen = false;
                structureScript.PlayerInSector(false);
            }
        }
    }
}

public enum sectorType
{
    Default,
    Replacement,
}

public enum sectorBiomes
{
    standart,
    ravine,
    openSpaces,
}

//Lift adapt to the city
//create the freeSpaceMatrix
/*matrixFreeSpace.Clear();
for (int x = 0; x < 18; x++)
{
    matrixFreeSpace.Add(new List<bool>());
    for (int z = 0; z < 18; z++)
    {
        bool elemMatrix = true;
        matrixFreeSpace[x].Add(elemMatrix);
    }
}
for (int i = 1; i < 5; i++)
{
    switch (i)
    {
        case 1:
            for (int z = 0; z < pointOrHaut.z + 1; z++) { matrixFreeSpace[0][z] = listMatrixConstructFloor[i][0][z]; }
            for (int x = 0; x < pointOrBas.x; x++) { matrixFreeSpace[x][0] = listMatrixConstructFloor[i][x][0]; }
            break;
        case 2:
            for (int z = (int)pointOrHaut.z + 1; z < 18; z++) { matrixFreeSpace[0][z] = listMatrixConstructFloor[i][0][z]; }
            for (int x = 0; x < pointOrHaut.x; x++) { matrixFreeSpace[x][(int)(17 - pointOrHaut.z)] = listMatrixConstructFloor[i][x][(int)(17 - pointOrHaut.z)]; }
            break;
        case 3:
            for (int z = (int)pointOrHaut.z + 1; z < 18; z++) { matrixFreeSpace[0][z] = listMatrixConstructFloor[i][0][z]; }
            for (int x = 0; x < pointOrHaut.x; x++) { matrixFreeSpace[x][(int)(17 - pointOrHaut.z)] = listMatrixConstructFloor[i][x][(int)(17 - pointOrHaut.z)]; }
            break;
    }

}

bool stillSearching = true;
void PositionTheLiftPlateform(Vector3 matrixCoordinate)
{

    stillSearching = false;
}
for (int i = 1; i < 5; i++)
{
    //check for free space
    if (stillSearching)
    {
        for (int j = 2; j < 15 + 1; j++)//Check only space free to put the palteforme
        {
            //alongZ
            if (listMatrixConstructFloor[i][0][j])
            {
                PositionTheLiftPlateform(new(i, 0, j));
            }
            else if (listMatrixConstructFloor[i][17][j])
            {
                PositionTheLiftPlateform(new(i, 17, j));
            }
            //alongX
            else if (listMatrixConstructFloor[i][j][0])
            {
                PositionTheLiftPlateform(new(i, j, 0));
            }
            else if (listMatrixConstructFloor[i][j][17])
            {
                PositionTheLiftPlateform(new(i, j, 17));
            }

        }
    }
}*/

