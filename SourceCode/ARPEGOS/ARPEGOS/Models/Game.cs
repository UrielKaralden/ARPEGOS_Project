﻿using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Semantics;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ARPEGOS.Models
{
    public class Game
    {
        #region Properties
        static readonly RDFModelEnums.RDFFormats RdfFormat = RDFModelEnums.RDFFormats.RdfXml;
        public string CurrentGameName { get; set; }
        public string CurrentGameContext { get; internal set; }
        public string GameDBFile { get; internal set; }
        public string CurrentCharacterFile { get; internal set; }
        public string CurrentCharacterName { get; set; }
        public string CurrentCharacterContext { get; internal set; }
        public RDFOntology GameOntology { get; internal set; }
        public RDFOntology CharacterOntology { get; set; }
        #endregion

        #region Constructor
        public Game(string GamePath, string GameVersion)
        {
            CurrentGameName = GamePath.Substring(GamePath.LastIndexOf('/') + 1);
            CurrentCharacterName = "TestCharacter";
            CurrentCharacterFile = Path.Combine("F:/Alejandro/Xamarin/OWL Project/characters", CurrentCharacterName + ".owl");
            CurrentCharacterContext = "http://ARPEGOS_Project/Games/" + CurrentGameName + "/characters/" + CurrentCharacterName + "#";
            string CurrentGamePrefix = string.Concat(Regex.Matches(CurrentGameName, "[A-Z]").OfType<Match>().Select(match => match.Value)).ToLower();

            GameDBFile = GamePath + GameVersion;
            RDFGraph GameGraph = RDFGraph.FromFile(RdfFormat, GameDBFile);
            CurrentGameContext = GameGraph.Context.ToString() + '#';
            GameGraph.SetContext(new Uri(CurrentGameContext));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(CurrentGamePrefix, GameGraph.Context.ToString()));
            GameOntology = RDFOntology.FromRDFGraph(GameGraph);

            RDFGraph CharacterGraph = new RDFGraph();
            CharacterGraph.SetContext(new Uri(CurrentCharacterContext));
            CharacterOntology = RDFOntology.FromRDFGraph(CharacterGraph);
            #endregion

            #region Methods
            #region Game_Methods
            #region SPARQL_Select_Methods
            public List<string> GetIndividualsOfClass(string className)
        {
            RDFVariable individual = new RDFVariable("individual");
            RDFResource classResource = new RDFResource(GameGraph.Context + className);
            RDFPattern individual_from_class = new RDFPattern(individual, RDFVocabulary.RDF.TYPE, classResource);
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetIndividualsOfClass");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(individual_from_class);
            selectQuery.AddPatternGroup(queryPatternGroup);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);

            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();

            foreach (DataRow row in rows)
                foreach (var item in row.ItemArray)
                {
                    resultList.Add(item.ToString());
                }

            Console.ReadLine();
            return (resultList.Count() != 0) ? resultList : null;
        }
        public List<ObjectPropertyElement> GetRelatedObjectProperties(Uri elementUri)
        {
            RDFVariable property = new RDFVariable("property");
            RDFVariable destinyElement = new RDFVariable("destinyElement");
            RDFResource elementResource = new RDFResource(elementUri.ToString());
            RDFPattern property_is_object_property = new RDFPattern(property, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.OBJECT_PROPERTY);
            RDFPattern element_has_property_value = new RDFPattern(elementResource, property, destinyElement);
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetRelatedObjectProperties");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(property_is_object_property);
            queryPatternGroup.AddPattern(element_has_property_value);
            selectQuery.AddPatternGroup(queryPatternGroup);
            selectQuery.AddProjectionVariable(property);
            selectQuery.AddProjectionVariable(destinyElement);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();

            foreach (DataRow row in rows)
                foreach (var item in row.ItemArray)
                {
                    resultList.Add(item.ToString());
                }

            Debug.WriteLine(resultList);

            List<ObjectPropertyElement> ResultPropertiesList = new List<ObjectPropertyElement>();
            string objectProperty = "";
            string destiny;
            foreach (var item in resultList)
            {
                if (resultList.IndexOf(item) % 2 == 0)
                    objectProperty = item;
                else
                {
                    destiny = item;
                    ResultPropertiesList.Add(new ObjectPropertyElement(new Uri(objectProperty), elementUri, new Uri(destiny)));
                }
            }
            return (ResultPropertiesList.Count() != 0) ? ResultPropertiesList : null;
        }
        public List<DatatypePropertyElement> GetRelatedDatatypeProperties(Uri elementUri)
        {
            RDFVariable property = new RDFVariable("property");
            RDFVariable value = new RDFVariable("value");
            RDFResource elementResource = new RDFResource(elementUri.ToString());
            RDFPattern property_is_object_property = new RDFPattern(property, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DATATYPE_PROPERTY);
            RDFPattern element_has_property_value = new RDFPattern(elementResource, property, value);
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetRelatedDatatypeProperties");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(property_is_object_property);
            queryPatternGroup.AddPattern(element_has_property_value);
            selectQuery.AddPatternGroup(queryPatternGroup);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();

            foreach (DataRow row in rows)
                foreach (var item in row.ItemArray)
                {
                    resultList.Add(item.ToString());
                }

            List<DatatypePropertyElement> ResultPropertiesList = new List<DatatypePropertyElement>();
            dynamic propertyValue;
            string datatypeProperty = "";
            foreach (var item in resultList)
            {
                if (resultList.IndexOf(item) % 2 == 0)
                    datatypeProperty = item;
                else
                {
                    propertyValue = item;
                    ResultPropertiesList.Add(new DatatypePropertyElement(new Uri(datatypeProperty), elementUri, propertyValue));
                }
            }
            return (ResultPropertiesList.Count() != 0) ? ResultPropertiesList : null;
        }
        public List<string> GetObjectPropertyForElementUpdate(string elementName)
        {
            RDFVariable property = new RDFVariable("property");
            RDFRegexFilter objectPropertyFilter = new RDFRegexFilter(property, new Regex("tiene" + elementName));
            RDFPattern property_related_to_element = new RDFPattern(property, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.OBJECT_PROPERTY);
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetRelatedObjectProperties");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(property_related_to_element);
            queryPatternGroup.AddFilter(objectPropertyFilter);
            selectQuery.AddPatternGroup(queryPatternGroup);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();

            foreach (DataRow row in rows)
                foreach (var item in row.ItemArray)
                {
                    resultList.Add(item.ToString());
                }
            return (resultList.Count() != 0) ? resultList : null;

        }
        public List<string> GetDatatypePropertyForElementUpdate(string elementName)
        {
            RDFVariable property = new RDFVariable("property");
            RDFRegexFilter datatypePropertyFilter = new RDFRegexFilter(property, new Regex("tiene" + elementName));
            RDFPattern property_related_to_element = new RDFPattern(property, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DATATYPE_PROPERTY);
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetRelatedDatatypeProperties");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(property_related_to_element);
            queryPatternGroup.AddFilter(datatypePropertyFilter);
            selectQuery.AddPatternGroup(queryPatternGroup);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);

            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();

            foreach (DataRow row in rows)
                foreach (var item in row.ItemArray)
                {
                    resultList.Add(item.ToString());
                }

            return (resultList.Count() != 0) ? resultList : null;
        }
        public RDFResource GetElementType(Uri elementUri)
        {
            RDFVariable type = new RDFVariable("type");
            RDFResource elementResource = new RDFResource(elementUri.ToString());
            RDFPattern element_has_type_type = new RDFPattern(elementResource, RDFVocabulary.RDF.TYPE, type);
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetElementType");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(element_has_type_type);
            selectQuery.AddPatternGroup(queryPatternGroup);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();

            foreach (DataRow row in rows)
                foreach (var item in row.ItemArray)
                {
                    resultList.Add(item.ToString());
                }

            // For class, check if individuals has class type also
            RDFResource result;
            switch (resultList.Count())
            {
                case 3: result = RDFVocabulary.OWL.CLASS; break;
                case 2: result = RDFVocabulary.OWL.INDIVIDUAL; break;
                default:
                    int index = resultList[0].LastIndexOf('#');
                    var value = resultList[0].Substring(index);
                    if (value.Contains("ObjectProperty"))
                        result = RDFVocabulary.OWL.OBJECT_PROPERTY;
                    else
                        result = RDFVocabulary.OWL.DATATYPE_PROPERTY;
                    break;
            }
            return result;
        }
        public string GetElementSuperclass(Uri elementUri)
        {
            RDFVariable superclass = new RDFVariable("superclass");
            RDFResource elementResource = new RDFResource(elementUri.ToString());
            RDFPattern element_subclassof_superclass = new RDFPattern(elementResource, RDFVocabulary.RDFS.SUB_CLASS_OF, superclass);
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetElementSuperclass");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(element_subclassof_superclass);
            selectQuery.AddPatternGroup(queryPatternGroup);
            selectQuery.AddProjectionVariable(superclass);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            string result = rows[0].ItemArray[0].ToString();
            return result;
        }
        public List<string> GetElementSubclasses(Uri elementUri)
        {
            RDFVariable subclass = new RDFVariable("subclass");
            RDFResource elementResource = new RDFResource(elementUri.ToString());
            RDFPattern subclass_subclassof_element = new RDFPattern(subclass, RDFVocabulary.RDFS.SUB_CLASS_OF, elementResource);
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetElementSubclasses");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(subclass_subclassof_element);
            selectQuery.AddPatternGroup(queryPatternGroup);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();

            foreach (DataRow row in rows)
                foreach (var item in row.ItemArray)
                {
                    resultList.Add(item.ToString());
                }

            return (resultList.Count() != 0) ? resultList : null;
        }
        public string GetElementClass(Uri elementUri)
        {
            RDFVariable Class = new RDFVariable("class");
            RDFResource elementResource = new RDFResource(elementUri.ToString());
            RDFPattern Class_is_class = new RDFPattern(Class, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS);
            RDFPattern element_is_type_Class = new RDFPattern(elementResource, RDFVocabulary.RDF.TYPE, Class);
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetElementClass");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(Class_is_class);
            queryPatternGroup.AddPattern(element_is_type_Class);
            selectQuery.AddPatternGroup(queryPatternGroup);
            selectQuery.AddProjectionVariable(Class);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            string result = rows[0].ItemArray[0].ToString();
            return result;
        }
        public string GetElementDescription(ClassElement element)
        {
            RDFVariable description = new RDFVariable("description");
            RDFResource elementResource = new RDFResource(element.URI.ToString());
            RDFPattern element_has_comment_description = new RDFPattern(elementResource, RDFVocabulary.RDFS.COMMENT, description);
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetElementDescription");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(element_has_comment_description);
            selectQuery.AddPatternGroup(queryPatternGroup);
            selectQuery.AddProjectionVariable(description);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            string result = rows[0].ItemArray[0].ToString();
            return result.Replace("^^http://www.w3.org/2001/XMLSchema#string", "");
        }
        public ClassElement GetSchemeRootElement()
        {
            RDFVariable property = new RDFVariable("property");
            RDFVariable classtype = new RDFVariable("classtype");
            RDFVariable individual = new RDFVariable("individual");
            RDFVariable value = new RDFVariable("value");
            RDFPattern property_is_datatype_property = new RDFPattern(property, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DATATYPE_PROPERTY);
            RDFPattern property_has_domain_class = new RDFPattern(property, RDFVocabulary.RDFS.DOMAIN, classtype);
            RDFPattern individual_is_from_class = new RDFPattern(individual, RDFVocabulary.RDF.TYPE, classtype);
            RDFPattern individual_has_property = new RDFPattern(individual, property, value);
            RDFRegexFilter property_name = new RDFRegexFilter(property, new Regex("ClassIsCreationSchemeRoot"));
            RDFPatternGroup queryPatternGroup = new RDFPatternGroup("GetSchemeRootElement");
            RDFSelectQuery selectQuery = new RDFSelectQuery();

            queryPatternGroup.AddPattern(property_is_datatype_property);
            queryPatternGroup.AddPattern(property_has_domain_class);
            queryPatternGroup.AddPattern(individual_is_from_class);
            queryPatternGroup.AddPattern(individual_has_property);
            queryPatternGroup.AddFilter(property_name);
            selectQuery.AddPatternGroup(queryPatternGroup);
            selectQuery.AddProjectionVariable(individual);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            string result = rows[0].ItemArray[0].ToString();
            return new ClassElement(result);
        }
        #endregion
        #endregion

        #region Character_Methods
        #region Info_Methods
        public void SaveStatement(Uri subjectUri, Uri predicateUri, Uri objectUri)
        {
            RDFResource subjectResource = new RDFResource(subjectUri.ToString());
            RDFResource predicateResource = new RDFResource(predicateUri.ToString());
            RDFResource objectResource = new RDFResource(objectUri.ToString());
            RDFTriple statement = new RDFTriple(subjectResource, predicateResource, objectResource);
            if (!CurrentStage.Contains(statement))
                CurrentStage.Add(statement);
        }

        public void RemoveLastStatement()
        {
            CurrentStage.RemoveAt(CurrentStage.Count() - 1);
        }

        public void SaveCurrentStage()
        {
            if (!CharacterCreationStagedData.Contains(CurrentStage))
            {
                CharacterCreationStagedData.Add(CurrentStage);
                CurrentStage.Clear();
                ++CurrentStageNumber;
            }
        }

        public void RemoveLastSavedStage()
        {
            CharacterCreationStagedData.RemoveAt(CurrentStageNumber);
            --CurrentStageNumber;
        }

        public void SaveCharacterData()
        {
            foreach (var stage in CharacterCreationStagedData)
                foreach (var statement in stage)
                    if (!CharacterGraph.ContainsTriple(statement))
                        CharacterGraph.AddTriple(statement);

            CharacterGraph.ToFile(RdfFormat, CharacterFile);
        }

        public void RemoveCharacterData()
        {
            File.Delete(CharacterFile);   
        }


        #endregion
        #endregion
        #endregion
    }
}
