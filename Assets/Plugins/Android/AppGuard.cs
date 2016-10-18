#if UNITY_EDITOR
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

public class AppGuard : EditorWindow{
	private static string version = "1.1.4";
	// Needs : JDK, Android SDK
	private static string appGuardAppKey, productName;
	private static string appGuardCli, assetsPath, projectRootPath;
	private static string buildOutputFilePath;
	private static string sdk, jdk, unprotectedApk;
	private static string keystore, keystorePassword, keystoreAlias, keystoreAliasPassword;
	private static bool isSelectKestore = false;
	private static string unityData = EditorApplication.applicationContentsPath;
	private static Texture _logoTexture;
	private static bool protectOption = true;
	private static string developmentOption = "";
	private static string unityVersion = "";
	
	// Can modify this variable//
	public static BuildOptions buildOption = BuildOptions.None;

	// For Unity Editor
	[MenuItem ("AppGuard/Build and Protect")]
	public static void BuildSetting(MenuCommand menuCommand)
	{
		AppGuard window = (AppGuard)EditorWindow.GetWindow(typeof(AppGuard));
		window.Show();
	}

	// For Command Line Build.
	public static void Build()
	{		
		if (String.Compare(unityVersion, "") == 0)
		{
			unityVersion = getUnityVersion();
		}
		
		string[] arguments = Environment.GetCommandLineArgs();
		int argNum = arguments.GetLength(0);
		for (int i = 0; i < argNum; i++)
		{
			if (arguments[i] == "-output")
			{
				buildOutputFilePath = arguments[i+1];
			} 
			else if (arguments[i] == "-development")
			{
				developmentOption = " --development";
				buildOption |= BuildOptions.Development;
			}
		}
		if (IsNull(buildOutputFilePath))
		{
			return;
		}
		if (LoadData() == true)
		{
			UnityEditor.PlayerSettings.Android.keystoreName = keystore;
			UnityEditor.PlayerSettings.Android.keystorePass = keystorePassword;
			UnityEditor.PlayerSettings.Android.keyaliasName = keystoreAlias;
			UnityEditor.PlayerSettings.Android.keyaliasPass = keystoreAliasPassword;
			MakeApk();
		}
	}

    public static void PreCloudBuild()
    {
        //UnityEngine.Debug.Log("[AppGuard] PreCloudBuild!!!");
        //jdk = "/usr";
        //UnityEngine.Debug.Log("[AppGuard] jdk : " + jdk);
        //sdk = "/usr";
        //UnityEngine.Debug.Log("[AppGuard] sdk : " + sdk);
        //assetsPath = Application.dataPath;
        //UnityEngine.Debug.Log("[AppGuard] assetsPath : " + assetsPath);
        //projectRootPath = assetsPath + "/../";
        //UnityEngine.Debug.Log("[AppGuard] projectRootPath : " + projectRootPath);
        //unityVersion = Application.unityVersion;
        //UnityEngine.Debug.Log("[AppGuard] unityVersion : " + unityVersion);
        //appGuardAppKey = "zgOFnTikMpHhu7LJwjDvcVGpZS5RtRdLKw5RsXKtMICRm0ARQfeiJQLLOlcA2q3e";
        //InitSetting();
        //InjectAppGuardCode();
    }

    public static void PostCloudBuild()
    {
        RevertAppGuardCode();
    }

	public static void MakeApk()
	{
        EditorUtility.DisplayProgressBar("AppGuard initialize", "Wait few seconds...", (float)0.4);
		InitSetting();
        InjectAppGuardCode();
        EditorUtility.ClearProgressBar();
        BuildAndroidProject();
        EditorUtility.DisplayProgressBar("Protect Apk", "Wait few seconds...", (float)0.8);
        RevertAppGuardCode();
        ProtectAppGuard(Path.Combine(projectRootPath, productName + "_appguard.apk"));
        DeleteTempFile();
        EditorUtility.ClearProgressBar();
	}

	void OnGUI()
	{
		InitPath();
		_logoTexture = AssetDatabase.LoadAssetAtPath(@"Assets/Plugins/Android/AppGuard.png", typeof(Texture)) as Texture;
		GUI.DrawTexture(new Rect(190, 0, 290, 140), _logoTexture);
		protectOption = GUI.Toggle(new Rect(250, 140, 160, 20), protectOption, "Decompile Prevention");
		GUILayout.Space(170);
			
		appGuardCli = EditorGUILayout.TextField("AppGuard CLI path", appGuardCli);
		appGuardAppKey = EditorGUILayout.TextField("AppGuard AppKey", appGuardAppKey);
		if (IsNull(UnityEditor.PlayerSettings.Android.keystoreName) || IsNull(UnityEditor.PlayerSettings.Android.keyaliasName))
		{
			EditorGUILayout.LabelField("Keystore", "Please, input your keystore infomation.");
			EditorGUILayout.LabelField(" ", "Build Settings -> Player Settings -> Android Setting -> Publishing Settings");
			isSelectKestore = false;
		}
		else
		{
			EditorGUILayout.LabelField("Keystore", keystore);
			EditorGUILayout.LabelField("Keystore alias name", keystoreAlias);
			isSelectKestore = true;
		}
#if UNITY_EDITOR_WIN
		EditorGUILayout.LabelField("JDK Root", jdk);
		EditorGUILayout.LabelField("Android SDK Root", sdk);
#elif UNITY_EDITOR_OSX
		jdk = EditorGUILayout.TextField("JDK Root", jdk);
		sdk = EditorGUILayout.TextField("Android SDK Root", sdk);
#endif
		SaveData();

		if (GUILayout.Button("Build"))
		{
			if (CheckSettings())
			{
				buildOutputFilePath = EditorUtility.SaveFilePanel("Build Android", "", productName + ".apk", "apk");
				if (buildOutputFilePath.Length != 0) {
					MakeApk();
				}
			}
		}

		// For Personal Edition
		/*
		if (GUILayout.Button("Pre-Build"))
		{
			if (CheckSettings())
			{
				InitSetting();
				InjectAppGuardCode();
			}
		}
		if (GUILayout.Button("Protect"))
		{
			if (CheckSettings())
			{
				string unprotectedApk = EditorUtility.OpenFilePanel("Select Apk File", "", "apk");
				buildOutputFilePath = EditorUtility.SaveFilePanel("Protected(Output) Apk", "", productName + ".apk", "apk");
				if (buildOutputFilePath.Length != 0) {
					RevertAppGuardCode();
					ProtectAppGuard(unprotectedApk);
				}
			}
		}
		*/
		EditorGUILayout.LabelField("", version);
	}

	public static void InitPath()
	{
		LoadData();
		if (IsNull(UnityEditor.PlayerSettings.Android.keystoreName) == false)
		{
			keystore = UnityEditor.PlayerSettings.Android.keystoreName;
		}
		if (IsNull(UnityEditor.PlayerSettings.Android.keystorePass) == false)
		{
			keystorePassword = UnityEditor.PlayerSettings.Android.keystorePass;
		}
		if (IsNull(UnityEditor.PlayerSettings.Android.keyaliasName) == false)
		{
			keystoreAlias = UnityEditor.PlayerSettings.Android.keyaliasName;
		}
		if (IsNull(UnityEditor.PlayerSettings.Android.keyaliasPass) == false)
		{
			keystoreAliasPassword = UnityEditor.PlayerSettings.Android.keyaliasPass;
		}

		assetsPath = Application.dataPath;
		projectRootPath = assetsPath +"/../";

#if UNITY_EDITOR_WIN
		appGuardCli = assetsPath + "/AppGuard/windows/AppGuard.exe";
#elif UNITY_EDITOR_OSX
		appGuardCli = assetsPath + "/AppGuard/mac/AppGuard";
#endif

#if UNITY_EDITOR_WIN
		jdk = Environment.GetEnvironmentVariable("JAVA_HOME");
		sdk = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
		// 키스토어가 상대경로로 주어지는 경우가 있음
		if (IsNull(keystore) == false)
		{
			if (keystore[1] != ':')
			{
				keystore = projectRootPath + keystore;
			}
		}
#elif UNITY_EDITOR_OSX
		if (IsNull(keystore) == false)
		{
			if (keystore[0] != '/')
			{
				keystore = projectRootPath + keystore;
			}
		}
#endif
	}

	public static void InitSetting()
	{
        UnityEngine.Debug.Log("[AppGuard] InitSetting()");
		string args = "--initUnity --sdk \"" + sdk + "\" --jdk \"" + jdk + "\" -u \"" + projectRootPath + "\" --unityData \"" + unityData + "\"" + developmentOption + " -v " + appGuardAppKey + " --unityVersion " + unityVersion;
		RunAppGuardCli(args);
	}
	
	public static void BuildAndroidProject () {
		// add scenes
		List<string> sceneList = new List<string>();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (scene.enabled)
				sceneList.Add(scene.path);
		}
		string[] sceneArray = sceneList.ToArray();
		
		// Build Android Project
		string tempApk = Path.Combine(projectRootPath, productName + "_appguard.apk");
		string res = BuildPipeline.BuildPlayer(sceneArray, tempApk, BuildTarget.Android, buildOption);

		if (res.Length > 0)
		{
			RevertAppGuardCode();
			throw new Exception("BuildPlayer failure: " + res);
		}
	}

	public static void InjectAppGuardCode()
	{
        UnityEngine.Debug.Log("[AppGuard] InjectAppGuardCode()");
		string args = "--inject --unityData \"" + unityData + "\"" + " -u \"" + projectRootPath + "\"" + developmentOption + " -v " + appGuardAppKey  + " --unityVersion " + unityVersion;
		RunAppGuardCli(args);
	}

	public static void RevertAppGuardCode()
	{
		string args = "--revert --unityData \"" + unityData + "\"" + " -u \"" + projectRootPath + "\"" + developmentOption + " -v " + appGuardAppKey + " --unityVersion " + unityVersion;
		RunAppGuardCli(args);
	}

	private static void ProtectAppGuard(string unprotectedApk)
	{
		string protectMode = "";
		if (protectOption == true)
		{
			protectMode = " --prevent";
		}
		string args = " -k \"" + keystore + "\" -a \"" + keystoreAlias + "\" -p \"" + keystorePassword + "\" -n \"" + unprotectedApk + "\" -o \"" + buildOutputFilePath + "\" -v " + appGuardAppKey + protectMode;
		RunAppGuardCli(args);
	}

	private static void RunAppGuardCli(string args)
	{
		UnityEngine.Debug.Log(appGuardCli + " " + args);
		Process p = new Process();
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.RedirectStandardError = true;
        p.StartInfo.FileName = appGuardCli;
        p.StartInfo.Arguments = args;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        string error = p.StandardError.ReadToEnd();
        p.WaitForInputIdle();
        p.WaitForExit();
        p.Close();
        if (output != "")
            UnityEngine.Debug.Log(output);
        if (error != "")
            UnityEngine.Debug.LogError(error);
	}

	private static void DeleteTempFile()
	{
		string tempApk = Path.Combine(projectRootPath, productName + "_appguard.apk");
		if(File.Exists(tempApk))
		{
			try
			{
				UnityEngine.Debug.Log("AppGuard Protection Complete.");
				File.Delete(tempApk);
			}
			catch (IOException e)
			{
				UnityEngine.Debug.Log(e.Message);
				return;
			}
		}
		else
		{
			UnityEngine.Debug.LogError("Build Fail");
		}
	}

	private bool CheckSettings()
	{
		if (String.Compare(developmentOption, "") == 0)
		{
			setDevelopmentParameter();
		}

		if (String.Compare(unityVersion, "") == 0)
		{
			unityVersion = getUnityVersion();
		}

		if (isSelectKestore == false)
		{
			UnityEngine.Debug.LogError("Keystore not found.");
			EditorUtility.DisplayDialog("Input your keystore infomation.", "Build Settings -> Player Settings -> Android Setting -> Publishing Settings", "OK");
			return false;
		}

		if (IsNull(UnityEditor.PlayerSettings.Android.keystorePass))
		{
			UnityEngine.Debug.LogError("Input your keystore Password.");
			EditorUtility.DisplayDialog("Input your keystore Password.", "Build Settings -> Player Settings -> Android Setting -> Publishing Settings", "OK");
			return false;
		}

		if (IsNull(UnityEditor.PlayerSettings.Android.keyaliasPass))
		{
			UnityEngine.Debug.LogError("Input your keystore alias Password.");
			EditorUtility.DisplayDialog("Input your keystore alias Password.", "Build Settings -> Player Settings -> Android Setting -> Publishing Settings", "OK");
			return false;
		}

		if (IsNull(appGuardAppKey))
		{
			UnityEngine.Debug.LogError("Input AppKey.");
			EditorUtility.DisplayDialog("Input AppKey.", "Input your \''Toast App Guard\'s AppKey\'", "OK");
			return false;
		}

		if (IsValidFilePath(appGuardCli)
		   && IsValidFilePath(keystore)
		   && IsValidFilePath(jdk)
		   && IsValidFilePath(sdk))
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	
	static private bool IsValidFilePath(string filePath)
	{
		if (File.Exists(filePath) || Directory.Exists(filePath))
		{
			return true;
		}
		else
		{
			UnityEngine.Debug.LogError("File not found " + filePath);
			EditorUtility.DisplayDialog("Please, Check path.", "File not found " + filePath, "OK");
			return false;
		}
	}
		
	public static bool IsNull(string input)
	{
		if ((input == null) || (input == ""))
			return true;
		else
			return false;
	}

	public static void SaveData()
	{
		EditorPrefs.SetString("appGuardCli", appGuardCli);
		EditorPrefs.SetBool("protectOption", protectOption);
		if (IsNull(appGuardAppKey) == false)
		{
			EditorPrefs.SetString("appGuardAppKey", appGuardAppKey);
		}
		if (IsNull(keystore) == false)
		{
			EditorPrefs.SetString("keystore", keystore);
		}
		if (IsNull(keystoreAlias) == false)
		{
			EditorPrefs.SetString("keystoreAlias", keystoreAlias);
		}
		if (IsNull(keystorePassword) == false)
		{
			EditorPrefs.SetString("keystorePassword", keystorePassword);
		}
		if (IsNull(keystoreAliasPassword) == false)
		{
			EditorPrefs.SetString("keystoreAliasPassword", keystoreAliasPassword);
		}
		if (IsNull(jdk) == false)
		{
			EditorPrefs.SetString("jdk", jdk);
		}
		if (IsNull(sdk) == false)
		{
			EditorPrefs.SetString("sdk", sdk);
		}
		if (IsNull(projectRootPath) == false)
		{
			EditorPrefs.SetString("projectRootPath", projectRootPath);
		}
	}

	public static bool LoadData()
	{
		productName = UnityEditor.PlayerSettings.productName;
		projectRootPath = EditorPrefs.GetString("projectRootPath");
		appGuardCli = EditorPrefs.GetString("appGuardCli");
		appGuardAppKey = EditorPrefs.GetString("appGuardAppKey");
		keystore = EditorPrefs.GetString("keystore");
		keystorePassword = EditorPrefs.GetString("keystorePassword");
		keystoreAlias = EditorPrefs.GetString("keystoreAlias");
		keystoreAliasPassword = EditorPrefs.GetString("keystoreAliasPassword");
		protectOption = EditorPrefs.GetBool("protectOption", true);
		jdk = EditorPrefs.GetString("jdk");
		sdk = EditorPrefs.GetString("sdk");
		if (IsNull(appGuardCli) || IsNull(appGuardAppKey) || IsNull(keystore)
		    || IsNull(keystorePassword) || IsNull(keystoreAlias) 
		    || IsNull(keystoreAliasPassword) || IsNull(jdk)|| IsNull(sdk)
		    || IsNull(projectRootPath))
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	public static bool setDevelopmentParameter()
	{
		if((buildOption & BuildOptions.Development) == BuildOptions.Development)
		{
			developmentOption = " --development";
			return true;
		}
		else
		{
			developmentOption = "";
			return false;
		}
	}

	public static string getUnityVersion()
	{
		return Application.unityVersion;
	}
}
#endif