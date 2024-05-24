using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Xiaobo.Parameter
{
    public class ParameterController : MonoBehaviour
    {

        //List<IAdjustableParameter> parameters = new List<IAdjustableParameter>();

        Dictionary<string, IAdjustableParameter> parameters2 = new Dictionary<string, IAdjustableParameter>();

        [field: SerializeField]
        public bool Verbose { get; set; } = false;

        public string ConfigPath { get; } = "Parameters.xml";

        public float Speed { get; set; } = 1;

        public void RegisterParameter(IAdjustableParameter param, string param_name)
        {
            IAdjustableParameter stored_param = GetParameterByName(param_name);

            // If LoadSettings() is executed before registering process, there will be a param stored in the list
            if (stored_param != null)
            {
                // if type doesn't match, redirect mapping relation to use param that user provide in replace of the one loaded
                if (stored_param.GetType() != param.GetType())
                {
                    Debug.LogWarning(string.Format("RegisterParameter({0}): Parameter of a different type already exists and has been replaced.", param_name));
                    parameters2[param_name] = param;
                    return;
                }

                // if type matches, redirect mapping relation to use param that user provide first, then copy value from existing param
                parameters2[param_name] = param;
                param.CopyFrom(stored_param);
            }
            else
            {
                // Insert Paramter
                //parameters.Add(param);
                parameters2[param_name] = param;
            }
        }
        void RegisterParameterWhileLoading(IAdjustableParameter param, string param_name)
        {
            IAdjustableParameter stored_param = GetParameterByName(param_name);
            // If registering process is executed before LoadSettings(), for example user choose to RestoreSettings()
            if (stored_param != null)
            {
                // if type doesn't match, use stored parameter that user provided
                if (stored_param.GetType() != param.GetType())
                {
                    Debug.LogWarning(string.Format("RegisterParameterWhileLoading({0}): Can't load parameter because a parameter type doesn't match.", param_name));
                    return;
                }

                // if type matches, update values of stored param with loaded param 
                stored_param.CopyFrom(param);
            }
            else
            {
                // Insert Paramter
                //parameters.Add(param);
                parameters2[param_name] = param;
            }
        }

        IAdjustableParameter GetParameterByName(string param_name)
        {
            if (parameters2.ContainsKey(param_name))
                return parameters2[param_name];

            //foreach (IAdjustableParameter param in parameters)
            //{
            //    if(param.Name != null && param.Name == param_name)
            //    {
            //        return param;
            //    }
            //}
            return null;
        }

        public void Update()
        {

            //foreach (IAdjustableParameter param in parameters)
            foreach (IAdjustableParameter param in parameters2.Values)
            {
                param.Update(Time.deltaTime * Speed);
            }

            if (Input.GetKeyDown("s"))
            {
                SaveSettings();
            }

            if (Input.GetKeyDown("l"))
            {
                LoadSettings();
            }
        }
        #region Settings
        public void LoadSettings()
        {
            if (!File.Exists(FileAbsolutePath()))
            {
                Debug.Log(string.Format("LoadSettings({0}): File doesn't exist.{1}", ConfigPath, FileAbsolutePath()));
                return;
            }


            XmlDocument doc = new XmlDocument();
            using (FileStream fs = new FileStream(FileAbsolutePath(), FileMode.Open))
            {
                try
                {
                    doc.Load(fs);
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("LoadSettings({0}): Failed. \n{1}", ConfigPath, e.Message));
                    return;
                }
            }

            XmlNodeList element_list = doc.GetElementsByTagName("Parameters");
            if (element_list == null || element_list.Count != 1)
            {
                Debug.LogWarning(string.Format("LoadSettings({0}): Failed. Can't find or find multiple <Parameters>.", ConfigPath));
                return;
            }

            XmlElement root = (XmlElement)element_list.Item(0);
            element_list = root.GetElementsByTagName("param");
            try
            {
                foreach (XmlElement ele in element_list)
                {
                    Type type = Type.GetType(ele.GetAttribute("type"));
                    IXmlSettings param = (IXmlSettings)Activator.CreateInstance(type);

                    string param_name = ele.GetAttribute("name");
                    param.LoadFromXmlElement(ele);

                    RegisterParameterWhileLoading((IAdjustableParameter)param, param_name);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("LoadSettings({0}): Failed when reading and parsing data from file. \n{1}", ConfigPath, e.Message));
            }

            if (Verbose)
                Debug.Log(string.Format("LoadSettings({0}): Succeed).", ConfigPath));
        }
        public void SaveSettings()
        {
            XmlDocument doc = new XmlDocument();
            XmlComment info = doc.CreateComment("Modified at " + DateTime.Now.ToString());
            doc.AppendChild(info);

            XmlElement root = doc.CreateElement("Parameters");
            //foreach (IXmlSettings param in parameters)
            foreach (var param in parameters2)
            {
                XmlElement ele = doc.CreateElement("param");
                ele.SetAttribute("name", param.Key);
                ((IXmlSettings)param.Value).WriteToXmlElement(ele);
                root.AppendChild(ele);
            }
            doc.AppendChild(root);

            using (FileStream fs = new FileStream(FileAbsolutePath(), FileMode.Create))
            {
                doc.Save(fs);
            }

            if (Verbose)
                Debug.Log(string.Format("SaveSettings({0}): Succeed).", ConfigPath));
        }

        string FileAbsolutePath()
        {
            //return Application.dataPath + "/" + ConfigPath;
            return ConfigPath;
        }
        #endregion

        #region Instance
        private static ParameterController _Instance;
        public static ParameterController Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = GameObject.FindObjectOfType<ParameterController>();
                    if (_Instance == null)
                    {
                        GameObject go = new GameObject();
                        _Instance = go.AddComponent<ParameterController>();
                    }
                }
                return _Instance;
            }
        }
        private void Awake()
        {
            if (_Instance != null)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            _Instance = null;
        }
        #endregion
    }
}