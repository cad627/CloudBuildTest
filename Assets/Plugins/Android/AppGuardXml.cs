#if UNITY_EDITOR
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using System.Xml;

public static class AppGuardXml {

	const string ROOT_ANDROID_XML_PATH = "Plugins/Android/AndroidManifest.xml";

	[MenuItem ("AppGuard/Add Android Permission")]
	public static void ModifyManifest()
	{
		var xmlFile = Path.Combine(Application.dataPath, ROOT_ANDROID_XML_PATH);
		if (!File.Exists(xmlFile))
		{
			var sampleFile = "";
			if (String.Compare(Application.unityVersion, "5.2.0") < 0)
			{
				sampleFile = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/androidplayer/AndroidManifest.xml");
			} 
			else if (String.Compare(Application.unityVersion, "5.2.0") >= 0)
			{
				sampleFile = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/androidplayer/Apk/AndroidManifest.xml");	
			}
#if UNITY_EDITOR_OSX
if (String.Compare(Application.unityVersion, "5.3.0") >= 0)
			{
				sampleFile = Path.Combine(EditorApplication.applicationContentsPath, "../../PlaybackEngines/androidplayer/Apk/AndroidManifest.xml");	
			}
#endif
			File.Copy(sampleFile, xmlFile);
		}

		UpdateManifest(xmlFile);
		UnityEngine.Debug.Log("Success Updating Manifest");
	}

	public static void UpdateManifest(string filePath)
	{
		XmlDocument doc = new XmlDocument();
		XmlElement permission;
		doc.Load(filePath);
		if (doc == null)
		{
			UnityEngine.Debug.LogError("Couldn't load file. " + filePath);
			return;
		}

		XmlNode manifestNode = FindChildNode(doc, "manifest");
		string ns = manifestNode.GetNamespaceOfPrefix("android");

		permission = FindElementWithAndroidName(manifestNode, "uses-permission", "name", ns, "android.permission.INTERNET");
		if (permission == null)
		{
			permission = doc.CreateElement("uses-permission");
			permission.SetAttribute("name", ns, "android.permission.INTERNET");
			manifestNode.AppendChild(permission);
		}

		permission = FindElementWithAndroidName(manifestNode, "uses-permission", "name", ns, "android.permission.WRITE_EXTERNAL_STORAGE");
		if (permission == null)
		{
			permission = doc.CreateElement("uses-permission");
			permission.SetAttribute("name", ns, "android.permission.WRITE_EXTERNAL_STORAGE");
			manifestNode.AppendChild(permission);
		}	

        permission = FindElementWithAndroidName(manifestNode, "uses-permission", "name", ns, "android.permission.ACCESS_NETWORK_STATE");
		if (permission == null)
		{
			permission = doc.CreateElement("uses-permission");
			permission.SetAttribute("name", ns, "android.permission.ACCESS_NETWORK_STATE");
			manifestNode.AppendChild(permission);
		}	

		permission = FindElementWithAndroidName(manifestNode, "uses-permission", "name", ns, "android.permission.MOUNT_UNMOUNT_FILESYSTEMS");
		if (permission == null)
		{
			permission = doc.CreateElement("uses-permission");
			permission.SetAttribute("name", ns, "android.permission.MOUNT_UNMOUNT_FILESYSTEMS");
			manifestNode.AppendChild(permission);
		}
		doc.Save(filePath);
	}

	private static XmlNode FindChildNode(XmlNode parent, string name)
	{
		XmlNode curr = parent.FirstChild;
		while (curr != null)
		{
			if (curr.Name.Equals(name))
			{
				return curr;
			}
			curr = curr.NextSibling;
		}
		return null;
	}

	private static XmlElement FindElementWithAndroidName(XmlNode parent, string name, string androidName, string ns, string value)
	{
		var curr = parent.FirstChild;
		while (curr != null)
		{
			if (curr.Name.Equals(name) && 
			    curr is XmlElement && 
			    ((XmlElement)curr).GetAttribute(androidName, ns) == value)
				return curr as XmlElement;
			curr = curr.NextSibling;
		}
		return null;
	}
}
#endif