using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using City;
using NaughtyAttributes;

public class SectorDistributor : MonoBehaviour
{
    [Header("Sectors")]
    public List<SectorRequest> listRequest;
    public List<SectorVariation> listVariation;
    private List<List<SectorRequest>> archivedListsRequest = new List<List<SectorRequest>>();
    private List<List<SectorVariation>> archivedListsVariation = new List<List<SectorVariation>>();
    public List<Archive> listArchive = new List<Archive>();

    [Header("City Organization")]
    public List<GameObject> listStrucutre;

    [Header("Perf for Player")]
    public Playermovment playerCode;

    [Button("Archive")]
    public void ArchiveRequestsAndVariations()
    {

        //reset the system
        archivedListsRequest.Clear();
        archivedListsVariation.Clear();
        listArchive.Clear();

        //create archiveLists fror each structure
        for (int i = 0; i < 6; i++)
        {
            archivedListsRequest.Add(new List<SectorRequest>());
            archivedListsVariation.Add(new List<SectorVariation>());
        }

        List<string> listLetterAppearence = new List<string> { "A", "B", "C", "D", "E", "F" };

        //fill the archiveList with the good Request
        foreach (SectorRequest request in listRequest)
        {
            for (int j = 0; j < 6; j++)
            {
                if (request.StructureDependency == listLetterAppearence[j])
                {
                    archivedListsRequest[j].Add(request);
                    break;
                }
            }
        }

        //fill the archiveList with the good variation
        foreach (SectorVariation variation in listVariation)
        {
            foreach (string letter in variation.listStrucutrePresent)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (letter == listLetterAppearence[j])
                    {
                        archivedListsVariation[j].Add(variation);
                        break;
                    }
                }
            }
        }

        //create the correct archive
        for (int i = 0; i < 6; i++)
        {
            listArchive.Add(new Archive(listLetterAppearence[i], archivedListsRequest[i], archivedListsVariation[i]));
        }

        //warn the strucutre about the change
        foreach(GameObject structure in listStrucutre)
        {
            structure.GetComponent<Sectorassembly>().needArchiveupdate = true;
        }
    }

    private void Start()
    {
        archivedListsRequest.Clear();
        archivedListsVariation.Clear();
        listArchive.Clear();

        ArchiveRequestsAndVariations();

        foreach (GameObject structure in listStrucutre)
        {
            structure.SetActive(true);
        }
    }

    private void Update()
    {
        
    }


    [System.Serializable]
    public class SectorRequest
    {
        public GameObject sectorPrefab;
        public string StructureDependency;
        public Vector3 structureCoordinate;
        public Vector3 requestSize;
        public bool useSolConstruct;

        public SectorRequest(GameObject sectorPrefab, string structureDependency, Vector3 position, Vector3 size)
        {
            this.sectorPrefab = sectorPrefab;
            StructureDependency = structureDependency;
            structureCoordinate = position;
            requestSize = size;
            try
            {
                sectorPrefab.GetComponent<SolConstruct>();
                useSolConstruct = true;
            }
            catch { useSolConstruct = false; }
        }

        public void CheckPosAndSize()
        {
            while(structureCoordinate.x > Mathf.FloorToInt(Mathf.Abs(structureCoordinate.y + 1) / 4) + 1)
            {
                structureCoordinate.y -= 1;
            }
            while(structureCoordinate.z > Mathf.FloorToInt(Mathf.Abs(structureCoordinate.y + 1) / 4) + 1)
            {
                structureCoordinate.y -= 1;
            }
        }
    }

    [System.Serializable]
    public class SectorVariation
    {

        public GameObject sector;
        [Min(1)]public int spawnProbaDenom;
        public List<string> listStrucutrePresent;
        [Range(0, 2)]public int deapthPosition;
        public bool useSolConstruct;
        public bool replaceDefaultForPerf;
        public tagSect tag;

        public SectorVariation(GameObject sector, int spawnProbaOn100, List<string> listStrucutrePresent, bool useConstruct, bool replaceDefault, int deapth, tagSect tagSec)
        {
            this.sector = sector;
            this.spawnProbaDenom = spawnProbaOn100;
            this.listStrucutrePresent = listStrucutrePresent;
            deapthPosition = deapth;
            useSolConstruct = useConstruct;
            replaceDefaultForPerf = replaceDefault;
            tag = tagSec;
        }

        public SectorVariation(GameObject sector, int spawnProbaOn100)
        {
            this.sector = sector;
            spawnProbaDenom = spawnProbaOn100;
            listStrucutrePresent = new List<string> { "A", "B", "C", "D", "E", "F" };
            deapthPosition = 0;
            useSolConstruct = false;
            replaceDefaultForPerf = false;
            tag = tagSect.Standart;
        }

        public enum tagSect
        {
            EmptySpace, 
            Standart,
        }
    }
}

public class Archive
{
    public string letterDenomination;
    public List<SectorDistributor.SectorRequest> listRequest;
    public List<SectorDistributor.SectorVariation> listVariation;

    public Archive(string letterDenomination, List<SectorDistributor.SectorRequest> listRequest, List<SectorDistributor.SectorVariation> listVariation)
    {
        this.letterDenomination = letterDenomination;
        this.listRequest = listRequest;
        this.listVariation = listVariation;
    }
}




