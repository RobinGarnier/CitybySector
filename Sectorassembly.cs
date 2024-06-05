using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using City;
public class Sectorassembly : MonoBehaviour
{
    [Header("Constructor")]
    public Vector2Int spaceForEachSector;
    public GameObject sector;
    public string letterDenomination;
    public bool chargedAtCorner;
    private Vector3 rotationIndex;
    private Vector3 rotationIndexArchive;
    private bool sectorConstructed;
    private bool awakePossible = false;
    private bool awakeDone = false;

    [Header("City Organisation")]
    public organizationType organisation = organizationType.City;
    public bool needArchiveupdate = true;
    public int probaForDivision = 0;
    public int nbDivisionAllowed = 0;
    private float probaNode = 1;
    private SectorDistributor cityCode;
    private Archive archive;

    [Header("Performance")]
    [Range(1,11)] public int renderDistence;
    [Range(0, 0.5f)]public float sensi;
    private int renderDistenceArchive;
    public Vector3 sectorHabitated;
    private List<GameObject> listObjectActive = new List<GameObject>();
    private bool nonUpdated = true;
    private GameObject pivotForSectorOrga;
    public bool waitForTheFirstGen = true;
    public bool usePIS = true;
    public int showerSensibility;

    [Header("Player")]
    public GameObject[] player;
    private Vector3 positionPlayer;
    private Matrix4x4 changeBasisMatrix; 
    private Vector3 positionPlayerNonOriented;
    public bool playerInSector;
    private bool playerSeeAlongZ;
    private bool playerSeeAlongX;
    private float coefSeeAlongZ;
    private float coefSeeAlongX;
    private float coefSeeAlongY;


    public void CreateSector(Vector3 dimLocal, int divisionDeapth, int positionInDivision) //default divisionDeapth=0, no division=>positionInDivision=0
    {
        //Condition de fabrication
        sectorConstructed = true;
        bool processInGoodWay = true;
        bool sectorNeeded = true;
        bool useSolConstruct = true;
        bool isReplaceVersion = false;
        string tagSector = "Standart";
        GameObject sectorOrder = sector;

        //Define the need to put a sector at dimLocal
        if (organisation == organizationType.City)
        {
            if (dimLocal.y >= 0) { sectorConstructed = false; }
            else if (Mathf.Abs(dimLocal.x) > Mathf.FloorToInt(AdaptCoordYToMath(dimLocal.y) / 4) + 1) { sectorConstructed = false; }
            else if (Mathf.Abs(dimLocal.z) > Mathf.FloorToInt(AdaptCoordYToMath(dimLocal.y) / 4) + 1) { sectorConstructed = false; }
        }

        //Determine the creation of a node
        else if (organisation == organizationType.Network)
        {
            useSolConstruct = false;
        }

        //determine the sectorOrder
        bool noSectorRequested = true;
        if (sectorConstructed)
        {
            //request
            bool SectorInTheRequestArea(SectorDistributor.SectorRequest request)
            {
                bool answer = false;
                if ((dimLocal.x <= request.structureCoordinate.x + request.requestSize.x && dimLocal.x >= request.structureCoordinate.x) && (dimLocal.z <= request.structureCoordinate.z + request.requestSize.z && dimLocal.z >= request.structureCoordinate.z)) { answer = true; }
                if (answer && dimLocal.y >= request.structureCoordinate.y - request.requestSize.y && dimLocal.y <= request.structureCoordinate.y) { answer = true; }//WARNING : y always negativ value)
                else { answer = false; }
                return answer;
            }
            foreach (SectorDistributor.SectorRequest request in archive.listRequest)
            {
                if (request.structureCoordinate == dimLocal)
                {
                    sectorOrder = request.sectorPrefab;
                    useSolConstruct = request.useSolConstruct;
                    noSectorRequested = false;
                    break;
                }
                else if (SectorInTheRequestArea(request))
                {
                    sectorNeeded = false;
                    noSectorRequested = false;
                    break;
                }
            }

            //variation
            if (noSectorRequested)
            {
                foreach (SectorDistributor.SectorVariation variation in archive.listVariation)
                {
                    if (Random.Range(0, variation.spawnProbaDenom) == 0 && variation.deapthPosition==divisionDeapth)
                    {
                        sectorOrder = variation.sector;
                        useSolConstruct = variation.useSolConstruct;
                        isReplaceVersion = variation.replaceDefaultForPerf;
                        tagSector = variation.tag.ToString();
                        break;
                    }
                }
            }
        }

        //Sector/Node Creating according to demand
        if (sectorNeeded && sectorConstructed)
        {
            //definie the possibility of a sector division
            bool divided = false;
            if (divisionDeapth < nbDivisionAllowed && noSectorRequested && probaForDivision > 0)
            {
                if (organisation == organizationType.Ruin)
                {
                    int division = Random.Range(0, probaForDivision);
                    _ = division == 0 ? divided = true : divided = false;
                }
            }

            //divide the sector into 8 smaller
            if (divided)
            {
                if (useSolConstruct)
                {
                    SolConstruct sectorScript = sectorOrder.GetComponent<SolConstruct>();
                    sectorScript.dimensionSector = sectorScript.dimensionSector * 1 / 2;
                    sectorScript.sizeSol /= 2;
                }

                try
                {
                    for (int i = 1; i < 9; i++)
                    {
                        CreateSector(dimLocal, divisionDeapth + 1, positionInDivision * 10 + i);
                    }
                }
                catch { Debug.LogWarning("division Failed"); }

                if (useSolConstruct)
                {
                    SolConstruct sectorScript = sectorOrder.GetComponent<SolConstruct>();
                    sectorScript.dimensionSector *= 2;
                    sectorScript.sizeSol *= 2;
                }
            }

            else
            {
                //Construct the sector
                GameObject sectorCreated = Instantiate(sectorOrder);
                sectorCreated.name = SectorNameCoordViaDim(dimLocal, positionInDivision);
                sectorCreated.tag = tagSector;

                //construction process via SolConstruct
                if (useSolConstruct)
                {
                    SolConstruct sectorScript = sectorCreated.GetComponent<SolConstruct>();
                    //sectorScript.dimensionSector[0] = (int)(sectorScript.dimensionSector[0] / Mathf.Pow(2, divisionDeapth));
                    //sectorScript.dimensionSector[1] = (int)(sectorScript.dimensionSector[1] / Mathf.Pow(2, divisionDeapth));
                    //sectorScript.sizeSol = sectorScript.sizeSol * Mathf.Pow(1 / 2, divisionDeapth);
                    //sectorScript.deapthHoles = (int)(sectorScript.deapthHoles / Mathf.Pow(2, divisionDeapth));

                    sectorScript.ResetSol();
                    try { sectorScript.Position(); } catch { Destroy(sectorCreated); processInGoodWay = false; Debug.LogWarning("Position Failed"); }
                    if (processInGoodWay)
                    {
                        if (organisation == organizationType.City || organisation == organizationType.StructureSingle)
                        {
                            //sectorScript.ResetCatWalk();
                            sectorScript.Naming();
                        }
                        sectorScript.subdivision = divisionDeapth;
                    }
                }

                //adapt the construction process for simplified version
                else
                {
                    if (isReplaceVersion)
                    {
                        try
                        {
                            SolConstruct sectorScript = sectorCreated.GetComponent<SolConstruct>();
                            sectorScript.Naming();
                            sectorScript.simplifiedVersion = true;
                        }
                        catch { Debug.LogError("ReplaceVersionWithoutSolConstruct"); }
                    }
                }

                //position the sector at the right place
                if (processInGoodWay)
                {
                    Vector3 finalPosition = Vector3.zero;
                    if (letterDenomination == "E") 
                    { 
                        finalPosition = transform.localToWorldMatrix.MultiplyVector(new Vector3(dimLocal.x * spaceForEachSector[0], dimLocal.y * spaceForEachSector[1] - 2.05f * spaceForEachSector[0], dimLocal.z * spaceForEachSector[0])); 
                    }
                    else 
                    { 
                        finalPosition = transform.localToWorldMatrix.MultiplyVector(new Vector3(dimLocal.x * spaceForEachSector[0], dimLocal.y * spaceForEachSector[1] , dimLocal.z * spaceForEachSector[0])); //- 1.25f * spaceForEachSector[0]
                    }

                    if (divisionDeapth > 0)
                    {
                        List<int> sequentialDiv = new List<int>();
                        for(int i = divisionDeapth; i > 0; i--)
                        {
                            sequentialDiv.Add((int)( Mathf.Floor(positionInDivision/(Mathf.Pow(10, i))) - 10 * Mathf.Floor(positionInDivision / (Mathf.Pow(10, i+1)))));
                        }
                        Vector2Int twoDiv = new(0, 0);
                        _ = positionInDivision > 9 ? twoDiv = new((int)(positionInDivision/10), 0) : twoDiv = new(positionInDivision, 0);
                        _ = positionInDivision > 9 ? twoDiv = new((int)(positionInDivision / 10), positionInDivision - twoDiv[0]*10) : twoDiv = new(positionInDivision, 0);
                        List<Vector3> listOffset = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 0) };
                        for (int i = 0; i < 2; i++) //sequentialDiv.Count
                        {
                            if (twoDiv[i] == 0) { break; } //sequentialDiv[i] == 0
                            bool up = false;
                            for (int j = 1; j < 9; j++)
                            {
                                if (twoDiv[i] == j) //sequentialDiv
                                {
                                    if (up) { finalPosition += listOffset[j - 4] * spaceForEachSector[0] / (2 * (i + 1)) + new Vector3(0, spaceForEachSector[1] / (2 * (i + 1)), 0); }
                                    else { finalPosition += listOffset[j] * spaceForEachSector[0] / (2 * (i + 1)); }
                                }
                                if (j == 4) { up = true; }
                            }
                        }
                    }
                    
                    sectorCreated.transform.position = finalPosition;

                    sectorCreated.transform.parent = transform;
                    sectorCreated.transform.localEulerAngles = Vector3.zero;
                    pivotForSectorOrga = sectorCreated;
                }
            }
        }
    }

    public void PlayerInSector(bool PISec)
    {
        if(player[0].GetComponent<Playermovment>().lineConnected == false && usePIS)
        {
            playerInSector = PISec;
            nonUpdated = true;

            _ = PISec ? renderDistence = 2 : renderDistence = renderDistenceArchive;
        }
    }

    private void ClearUnseenSector(List<GameObject> listActive)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (listActive.Count > 0)
            {
                bool actif = false;
                for (int j = 0; j < listActive.Count; j++)
                {
                    if (transform.GetChild(i).gameObject == listActive[j])
                    {
                        actif = true;
                        listActive.RemoveAt(j);
                        break;
                    }
                }
                if(transform.GetChild(i).gameObject.activeInHierarchy != actif) { transform.GetChild(i).gameObject.SetActive(actif); }
            }
            else { if (transform.GetChild(i).gameObject.name == "BaseStructure") { } else{ transform.GetChild(i).gameObject.SetActive(false); } break; }
        }
    }

    private void SelectSectorOrCreateIt(Vector3 coordAroundPlayer, bool underPlayer = false, int subdivi=0)
    {
        sectorConstructed = true;
        /*try
        { 
            pivotForSectorOrga = transform.Find(SectorNameCoordViaDim(positionPlayerCoordinate() + coordAroundPlayer)).gameObject; 
            listObjectActive.Add(pivotForSectorOrga);
        }
        catch 
        {
            try
            {
                pivotForSectorOrga = transform.Find(SectorNameCoordViaDim(positionPlayerCoordinate() + coordAroundPlayer, 1)).gameObject;
                for(int i=2; i < 9; i++)
                {
                    listObjectActive.Add(transform.Find(SectorNameCoordViaDim(positionPlayerCoordinate() + coordAroundPlayer, i)).gameObject);
                }
            }
            catch { CreateSector(positionPlayerCoordinate() + coordAroundPlayer, 0, 0); } // Create default sector
        }*/

        //Select existing Sector or create it
        List<GameObject> AllSectorFoundAtPosition(Vector3 position, int subdivPosition=0) 
        {
            List<GameObject> returnList = new List<GameObject>();
            try
            {
                returnList.Add(transform.Find(SectorNameCoordViaDim(position, subdivPosition)).gameObject);
            }
            catch
            {
                if (subdivPosition <= nbDivisionAllowed * 10)
                {
                    //returnList.Add(transform.Find(SectorNameCoordViaDim(position, subdivPosition * 10 + 1)).gameObject);
                    for (int i = 1; i < 9; i++)
                    {
                        foreach( GameObject elem in AllSectorFoundAtPosition(position, subdivPosition * 10 + i))
                        {
                            returnList.Add(elem);
                        }
                    }
                }
            }
            return returnList;
        }


        //Update its state if it uses SolConstruct

        List<GameObject> listSectorThere = new List<GameObject>(); 
        listSectorThere = AllSectorFoundAtPosition(positionPlayerCoordinate() + coordAroundPlayer);
        if(listSectorThere.Count == 0)
        {
            CreateSector(positionPlayerCoordinate() + coordAroundPlayer, 0, 0);
            listSectorThere = AllSectorFoundAtPosition(positionPlayerCoordinate() + coordAroundPlayer);
        }
        foreach(GameObject elem in listSectorThere)
        {
            listObjectActive.Add(elem);
        /*}
        if (listSectorThere.Count >= 1)//sectorConstructed
        {*/
            //listObjectActive.Add(pivotForSectorOrga);
            try
            {
                pivotForSectorOrga = elem;// listSectorThere[0];
                //pivotForSectorOrga.transform.SetAsFirstSibling();
                SolConstruct sectorScript = pivotForSectorOrga.GetComponent<SolConstruct>();

                if (organisation == organizationType.Ruin)
                {
                    sectorScript.insideSeen = true;
                }
                else { sectorScript.insideSeen = underPlayer; }

                if (sectorScript.simplifiedVersion && player[0].GetComponent<Playermovment>().lineConnected == false)
                {
                    sectorScript.ResetSol();
                    try { sectorScript.Position(); }
                    catch { Destroy(pivotForSectorOrga); }
                    sectorScript.simplifiedVersion = false;
                }

                //update the sector neighbours
                if (sectorScript.notUpdated && organisation==organizationType.Ruin)
                {
                    List<Vector3> suround = new List<Vector3> { new(1, 0, 0), new(-1, 0, 0), new(0, 0, 1), new(0, 0, -1), new(0, 1, 0), new(0, -1, 0) };
                    bool needWallCheck = false;
                    for (int i = 0; i < suround.Count; i++)
                    {
                        if (sectorScript.neighbors[i].type == Neighbors.neighborsTypes.None)
                        {
                            bool foundSmthg = false;
                            GameObject neighborTest;
                            List<GameObject> neighborList = new List<GameObject>();
                            SolConstruct neighborScript;
                            bool useSolConstruct = false;
                            bool isDivided = false;
                            /*try
                            {
                                neighborTest = transform.Find(SectorNameCoordViaDim(SectorCoord(pivotForSectorOrga) + suround[i], 0)).gameObject;
                                if(neighborTest != null) { needWallCheck = true; foundSmthg = true; }
                            }
                            catch 
                            {
                                neighborTest = transform.Find(SectorNameCoordViaDim(SectorCoord(pivotForSectorOrga) + suround[i], 1)).gameObject;
                                if (neighborTest != null) { needWallCheck = true; isDivided = true; foundSmthg = true; }
                            }*/
                            neighborList = AllSectorFoundAtPosition(SectorCoord(pivotForSectorOrga) + suround[i]);
                            if (neighborList.Count != 0)
                            {
                                foundSmthg = true;
                                needWallCheck = true;
                                switch (neighborList.Count)
                                {
                                    case 1:
                                        neighborTest = neighborList[0];
                                        break;
                                    default:

                                        break;

                                }
                            }

                            if (foundSmthg)
                            {
                                if (isDivided)
                                {
                                    //sectorScript.neighbors[i] = new Neighbors(Neighbors.neighborsTypes.Standart,i);
                                }
                                else
                                {
                                    try
                                    {
                                        neighborScript = transform.Find(SectorNameCoordViaDim(SectorCoord(pivotForSectorOrga) + suround[i], 0)).gameObject.GetComponent<SolConstruct>();
                                        //neighborScript = neighborTest.GetComponent<SolConstruct>();
                                        useSolConstruct = true;
                                        if (transform.Find(SectorNameCoordViaDim(SectorCoord(pivotForSectorOrga) + suround[i], 0)).tag == "OpenSpace")
                                        {
                                            sectorScript.neighbors[i] = new Neighbors(Neighbors.neighborsTypes.OpenSpace, 0, neighborScript, i);
                                        }
                                        else 
                                        {
                                            if (transform.Find(SectorNameCoordViaDim(SectorCoord(pivotForSectorOrga) + suround[i], 0)).childCount == 0)
                                            {
                                                sectorScript.neighbors[i] = new Neighbors(Neighbors.neighborsTypes.EmptySpace, 0, i);
                                                //Update its status according to its neighbor
                                                sectorScript.gameObject.tag = "OpenSpace";
                                            }
                                            else { sectorScript.neighbors[i] = new Neighbors(Neighbors.neighborsTypes.Standart, 0, neighborScript, i); }
                                        }

                                    }
                                    catch 
                                    { 
                                        if (transform.Find(SectorNameCoordViaDim(SectorCoord(pivotForSectorOrga) + suround[i], 0)).childCount == 0)
                                        {
                                            sectorScript.neighbors[i] = new Neighbors(Neighbors.neighborsTypes.EmptySpace, 0, i);
                                            //Update its status according to its neighbor
                                            sectorScript.gameObject.tag = "OpenSpace";
                                        }
                                        else if(transform.Find(SectorNameCoordViaDim(SectorCoord(pivotForSectorOrga) + suround[i], 0)).tag=="OpenSpace")
                                        {
                                            sectorScript.neighbors[i] = new Neighbors(Neighbors.neighborsTypes.OpenSpace, 0, i);
                                        }
                                        else { sectorScript.neighbors[i] = new Neighbors(Neighbors.neighborsTypes.Standart, 0, i); }
                                    }
                                }
                            }
                        }
                    }
                    if (needWallCheck)
                    {
                        sectorScript.WallBuilding();
                    }
                    sectorScript.notUpdated = false;
                }
            }
            catch { }
        }
    }

    private void ActivatorAlongAxis(float coef, Vector3 axis)
    {
        Vector3 oppositAxis = new();
        if (axis.x == 0) { oppositAxis.x = 1; }
        else { oppositAxis.z = 1; }
        Vector3 heightAxis = new(0, 1, 0);

        for (int i = -renderDistence - 2; i < renderDistence + 2; i++)
        {
            //Avec etage
            bool underPlayer = true;
            for (int j = -(renderDistence - Mathf.Abs(i)); j < renderDistence - Mathf.Abs(i); j++)// j = -(renderDistence + 1 - Mathf.Abs(i)); j < renderDistence + 2 - Mathf.Abs(i)
            {
                if (j > 0) { underPlayer = false; }
                if (coef < 0.2 / renderDistence * (renderDistence - Mathf.Abs(i)))
                {
                    SelectSectorOrCreateIt(i * axis - oppositAxis + j * heightAxis, underPlayer);
                    SelectSectorOrCreateIt(i * axis + j * heightAxis, underPlayer);
                }
                else if (coef > 0.4 + 0.6 / renderDistence * Mathf.Abs(i))
                {
                    SelectSectorOrCreateIt(i * axis + j * heightAxis, underPlayer);
                    SelectSectorOrCreateIt(i * axis + oppositAxis + j * heightAxis, underPlayer);
                }
            }
        }
    }

    private void ActivatorAlongAxisV2(Vector3Int coef)
    {
        Vector3 xAxis = new(1, 0, 0);
        Vector3 yAxis = new(0, 1, 0);
        Vector3 zAxis = new(0, 0, 1);

        //x
        for (int x = -renderDistence - 1 + coef[0]; x < renderDistence + coef[0] + 1; x++)
        {
            bool underPlayer = true;
            //z
            for (int z = -renderDistence - 1 + coef[2]; z < renderDistence + coef[2] + 1; z++)// j = -(renderDistence + 1 - Mathf.Abs(i)); j < renderDistence + 2 - Mathf.Abs(i)
            {
                //y
                for(int y = -renderDistence + (- 1 + coef[1]) + (int)Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)); y< renderDistence + (coef[1] + 1) - (int)Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) + 1; y++)
                {
                    if (y > 1) { underPlayer = false; }
                    SelectSectorOrCreateIt(z*xAxis + y* yAxis + x*zAxis, underPlayer);
                }
            }
        }
        
    }

    public string SectorNameCoordViaDim(Vector3 dimLocal, int subDivision = 0) => $"{letterDenomination}_{dimLocal.y},{dimLocal.x}.{dimLocal.z}={subDivision}";

    public Vector3 SectorCoord(GameObject sector) => new Vector3(sector.transform.position.x / spaceForEachSector[0], sector.transform.position.y / spaceForEachSector[1], sector.transform.position.z / spaceForEachSector[0]);

    private float NegativeValueUnderOne(float value) => value < 0 ? value - 1 : value;

    private float AdaptCoordYToMath(float value) => value <= -1 ? Mathf.Abs(value + 1) : value;

    public Vector3 positionPlayerCoordinate()
    {
        Vector3 answer = Vector3.zero;
        if (letterDenomination == "E") { answer = new(positionPlayer.x / spaceForEachSector[0], (positionPlayer.y + 2.05f * spaceForEachSector[0]) / spaceForEachSector[1], positionPlayer.z / spaceForEachSector[0]); }
        else { answer = new(positionPlayer.x / spaceForEachSector[0], positionPlayer.y / spaceForEachSector[1], positionPlayer.z / spaceForEachSector[0]); }//(positionPlayer.y + 1.25f * spaceForEachSector[1])
        //assure la cohérance en negatif
        for (int i = 0; i < 3; i++) { if (answer[i] < 0) { answer[i] += -1; } }
        return new((int)answer[0], (int)answer[1], (int)answer[2]);
    }


    private void Update()
    {
        //update the archive if necessary
        if (needArchiveupdate)
        {
            UpdateTheArchive();
        }

        //update Rotation Index
        rotationIndex = transform.eulerAngles - rotationIndexArchive;

        //Display the right sector around the player
        listObjectActive.Clear();

        for(int a = 0; a < player.Length; a++)
        {
            //position the player on the grid according to the nearest sector
            positionPlayerNonOriented = player[a].transform.position;
            positionPlayer = transform.worldToLocalMatrix.MultiplyVector(positionPlayerNonOriented);

            SelectSectorOrCreateIt(Vector3.zero);

            if (organisation == organizationType.City || organisation == organizationType.StructureSingle || organisation == organizationType.Ruin)
            {
                if (chargedAtCorner)
                {
                    //define the right axis onto sectors will be set active
                    if (Mathf.Abs(positionPlayer.z - spaceForEachSector[0] * positionPlayerCoordinate()[2]) > 0.75 * spaceForEachSector[0])
                    { playerSeeAlongX = true; }
                    else { playerSeeAlongX = false; }
                    if (Mathf.Abs(positionPlayer.x - spaceForEachSector[0] * positionPlayerCoordinate()[0]) > 0.75 * spaceForEachSector[0])
                    { playerSeeAlongZ = true; }
                    else { playerSeeAlongZ = false; }

                    //adapt the render distence for noncubic sectors

                    //create the corridor
                    for (int i = -(renderDistence + 2); i < renderDistence + 2; i++)
                    {
                        //Avec etage
                        for (int j = -(renderDistence - Mathf.Abs(i)); j < renderDistence - Mathf.Abs(i); j++)
                        {
                            bool underPlayer = true;
                            if (j > 0) { underPlayer = false; }
                            if (true)//(playerSeeAlongX)
                            {
                                SelectSectorOrCreateIt(new Vector3(i, j, 0), underPlayer);
                                SelectSectorOrCreateIt(new Vector3(i, j, 1), underPlayer);
                            }

                            if (true)//(playerSeeAlongZ)
                            {
                                SelectSectorOrCreateIt(new Vector3(0, j, i), underPlayer);
                                SelectSectorOrCreateIt(new Vector3(1, j, i), underPlayer);
                            }
                        }
                    }
                }
                else
                {
                    if (organisation == organizationType.Ruin)
                    {
                        float posZInSect = NegativeValueUnderOne(positionPlayerNonOriented.z) - spaceForEachSector[0] * positionPlayerCoordinate()[2];
                        float posXInSect = NegativeValueUnderOne(positionPlayerNonOriented.x) - spaceForEachSector[0] * positionPlayerCoordinate()[0];
                        float posYInSect = NegativeValueUnderOne(positionPlayerNonOriented.y) - spaceForEachSector[1] * positionPlayerCoordinate()[1];

                        coefSeeAlongX = posZInSect / spaceForEachSector[0];
                        coefSeeAlongZ = posXInSect / spaceForEachSector[0];
                        coefSeeAlongY = posYInSect / spaceForEachSector[1];
                        Vector3 coefSeeAlong = new Vector3(coefSeeAlongX, coefSeeAlongY, coefSeeAlongZ);
                        Vector3Int coefSensibilized = Vector3Int.zero;
                        for (int ind = 0; ind < 3; ind++)
                        {
                            if (coefSeeAlong[ind] < sensi) { coefSensibilized[ind] = -1; }
                            else if (coefSeeAlong[ind] > 1 - sensi) { coefSensibilized[ind] = 1; }
                        }
                        ActivatorAlongAxisV2(coefSensibilized);
                    }
                    else
                    {
                        float posZInSect = NegativeValueUnderOne(positionPlayer.z) - spaceForEachSector[0] * positionPlayerCoordinate()[2];
                        float posXInSect = NegativeValueUnderOne(positionPlayer.x) - spaceForEachSector[0] * positionPlayerCoordinate()[0];
                        float posYInSect = NegativeValueUnderOne(positionPlayer.y) - spaceForEachSector[1] * positionPlayerCoordinate()[1];

                        coefSeeAlongX = posZInSect / spaceForEachSector[0];
                        coefSeeAlongZ = posXInSect / spaceForEachSector[0];
                        coefSeeAlongY = posYInSect / spaceForEachSector[1];

                        ActivatorAlongAxis(coefSeeAlongX, new Vector3(1, 0, 0));
                        ActivatorAlongAxis(coefSeeAlongZ, new Vector3(0, 0, 1));
                    }
                }
                //ClearUnseenSector(listObjectActive);
            }

            //protect from Start problem
            else if (organisation == organizationType.Network)
            {
                if (awakePossible) { ActivateNodalSystem(); } //&& awakeDone == false
            }
        }

        ClearUnseenSector(listObjectActive);
        nonUpdated = true;
        waitForTheFirstGen = false;
    }

    private void Start()
    {
        renderDistenceArchive = renderDistence;

        UpdateTheArchive();

        awakePossible = true;
    }

    //link the structure with its archive
    public void UpdateTheArchive()
    {
        cityCode = gameObject.GetComponentInParent<SectorDistributor>();
        foreach (Archive archiveInList in cityCode.listArchive)
        {
            if (letterDenomination == archiveInList.letterDenomination) { archive = archiveInList; break; }
        }

        needArchiveupdate = false;
    }
    
    public void ActivateNodalSystem()
    {
        if (organisation == organizationType.Network && awakePossible)
        {
            listObjectActive.Clear();
            for (int x = -renderDistence; x < renderDistence + 1; x++)
            {
                for (int y = -renderDistence; y < renderDistence + 1; y++)
                {
                    for (int z = -renderDistence; z < renderDistence + 1; z++)
                    {
                        SelectSectorOrCreateIt(new(x, y, z));
                    }
                }
            }
            //ClearUnseenSector(listObjectActive);
            awakeDone = true;
        }
    }

    private void Awake()
    {
        Debug.Log("Awake");
        //ActivateNodalSystem();
    }


    [System.Serializable]
    public class Neighbors
    {
        //public List<neighborsTypes> type;
        public neighborsTypes type;
        public int subdivision;
        //public List<bool> useSolConstruct;
        //public List<SolConstruct> script;
        public bool useSolConstruct;
        public SolConstruct script;
        public int position;

        /*public Neighbors(neighborsTypes neiType, int subdiv, int positionAroundSect, int selfSubdiv=0)
        {
            type = new List<neighborsTypes>() { neiType };
            useSolConstruct = new List<bool>() { false };
            subdivision = subdiv;
            if (subdiv > selfSubdiv)
            {
                script= new List<SolConstruct>() { null };
            }
            position = positionAroundSect;
        }*/

        public Neighbors(neighborsTypes neiType, int subdiv, int positionAroundSect)
        {
            type = neiType;
            useSolConstruct = false;
            subdivision = subdiv;
            position = positionAroundSect;
        }

        public Neighbors(neighborsTypes neiType, int subdiv, SolConstruct neiScript, int positionAroundSect)
        {
            type = neiType ;
            useSolConstruct =  true;
            subdivision = subdiv;
            script = neiScript ;
            position = positionAroundSect;
        }

        public enum neighborsTypes
        {
            None,
            OpenSpace,
            EmptySpace,
            Standart,
        }
    }

}


public enum organizationType
{
    City,
    StructureSingle,
    Network,
    Ruin,
}

//TEST PASSERELLE InterSecteur
/*  Lien entre les secteurs pour un possible pont entre les deux en liaison directe
        //fix the needed ends
        List<Vector3> checkNeighbour = new List<Vector3> { new(0, 0, -1), new(1, 0, 0), new(0, 0, 1), new(-1, 0, 0),};
        List<int> heightFixed = new List<int> { 1, 4, 3, 2, 1 };
        List<int> listCoordinate = new List<int>();
        List<bool> fixedValue = new List<bool>();
        for (int i = 0; i < 4; i++)
        {
            try
            {
                sectorScript.levelCoordinate[i] = transform.Find(SectorNameCoordViaDim(dimLocal + checkNeighbour[i])).gameObject.GetComponent<SolConstruct>().levelCoordinate[i];
                listCoordinate.Add(sectorScript.levelCoordinate[i]);
                sectorScript.levelHeigth[heightFixed[i+1]] = transform.Find(SectorNameCoordViaDim(dimLocal + checkNeighbour[i])).gameObject.GetComponent<SolConstruct>().levelHeigth[heightFixed[i+1]];
                sectorScript.levelHeigth[heightFixed[i]] = transform.Find(SectorNameCoordViaDim(dimLocal + checkNeighbour[i])).gameObject.GetComponent<SolConstruct>().levelHeigth[heightFixed[i]];

                fixedValue.Add(true);
            }
            catch
            {
                sectorScript.levelCoordinate[i] = Random.Range(0, 18);
                if (i > 0)
                {
                    if (fixedValue[i - 1])
                    {
                        sectorScript.levelHeigth[heightFixed[i + 1]] = Random.Range(0, 4);
                    }
                    else
                    {
                        sectorScript.levelHeigth[heightFixed[i + 1]] = Random.Range(0, 4);
                        sectorScript.levelHeigth[heightFixed[i]] = Random.Range(0, 4);
                    }
                }
                else
                {
                    sectorScript.levelHeigth[heightFixed[i + 1]] = Random.Range(0, 4);
                    sectorScript.levelHeigth[heightFixed[i]] = Random.Range(0, 4);
                }
                sectorScript.levelHeigth[i] = Random.Range(0, 4);
                listCoordinate.Add(sectorScript.levelCoordinate[i]);
                fixedValue.Add(false);
            }
        }

        void CheckDistenceBetweenFloor(float coord1, float coord2, bool fixed1, bool fixed2)
        {
            if (Mathf.Abs(coord1 - coord2) < 4)
            {
                for (int j = 0; j < 4 - Mathf.Abs(coord1 - coord2) + 1; j++)
                {
                    if (fixed1) { coord2 += 1; }
                    else if (fixed2) { coord1 += -1; }
                }
            }
        }

        CheckDistenceBetweenFloor(listCoordinate[0], listCoordinate[2], fixedValue[0], fixedValue[2]);
        CheckDistenceBetweenFloor(listCoordinate[1], listCoordinate[3], fixedValue[1], fixedValue[3]);

        for (int i = 0; i < 4; i++)
        {
            if (listCoordinate[i] > 14 && listCoordinate[i] < 17 && fixedValue[i]==false) { listCoordinate[i] = 17; }
            else if (listCoordinate[i] > 0 && listCoordinate[i] < 4 && fixedValue[i]==false) { listCoordinate[i] = 0; }

            sectorScript.levelCoordinate[i] = listCoordinate[i];
        }
        sectorScript.random = false;
        */
