namespace UnityPackTemplate.Editor
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class PackageGenerator : EditorWindow
    {
        private static PackageGenerator instance;

        private string packagePath = "Assets/MyPackage";
        private string companyName = "YourCompanyName";
        private string frameworkName = "YourFrameworkName";
        private string packageName = "MyCustomPackage";
        private string packageAuthor = "Your Name";
        private string packageVersion = "0.0.1";
        private string packageDescription = "A custom package for Unity.";
        private string unityVersion = "XXXX.X";
        private string newChangelogEntry = string.Empty;

        /// <summary>
        /// パッケージジェネレータを表示します。
        /// </summary>
        [MenuItem("Package/Package Generator")]
        public static void ShowWindow()
        {
            if (instance == null)
            {
                instance = GetWindow<PackageGenerator>("Package Generator");
                instance.Show();
            }
            else
            {
                instance.Focus();
            }
        }

        private void OnEnable()
        {
            instance = this;
        }

        private void OnDisable()
        {
            instance = null;
        }

        /// <summary>
        /// パッケージを読み込む.
        /// </summary>
        private void ReadPackageJsonIfExists()
        {
            var packageJsonPath = Path.Combine(packagePath, "package.json");

            if (!File.Exists(packageJsonPath))
            {
                return;
            }

            var packageJsonContent = File.ReadAllText(packageJsonPath);
            var packageJson = JsonUtility.FromJson<PackageJson>(packageJsonContent);
            companyName = packageJson.name.Split('.')[1];
            frameworkName = packageJson.name.Split('.')[2];
            packageName = packageJson.name.Split('.')[3];
            packageAuthor = packageJson.author;
            packageVersion = packageJson.version;
            packageDescription = packageJson.description;
            unityVersion = packageJson.unity;
        }

        private void OnGUI()
        {
            GUILayout.Label("Package Path", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            packagePath = EditorGUILayout.TextField("Path", packagePath);
            if (GUILayout.Button("Select Folder", GUILayout.Width(100)))
            {
                var absoluteFolderPath = EditorUtility.OpenFolderPanel("Select Package Folder", Path.Combine(Application.dataPath, packagePath), "");
                packagePath = GetRelativePath(absoluteFolderPath, Application.dataPath);
                ReadPackageJsonIfExists();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Package Info", EditorStyles.boldLabel);
            companyName = EditorGUILayout.TextField("Company Name", companyName);
            frameworkName = EditorGUILayout.TextField("Framework Name", frameworkName);
            packageName = EditorGUILayout.TextField("Package Name", packageName);
            packageAuthor = EditorGUILayout.TextField("Author", packageAuthor);
            packageVersion = EditorGUILayout.TextField("Version", packageVersion);
            unityVersion = EditorGUILayout.TextField("UnityVersion", unityVersion);
            packageDescription = EditorGUILayout.TextField("Description", packageDescription);

            EditorGUILayout.EndHorizontal();


            if (GUILayout.Button("Generate Package Files"))
            {
                // 入力された新しいエントリを反映
                if (!string.IsNullOrEmpty(newChangelogEntry))
                {
                    var changelogPath1 = Path.Combine(packagePath, "CHANGELOG.md");
                    InsertNewVersionEntry(changelogPath1);
                }

                GeneratePackageFiles();
            }

            // 新しい入力フィールドを追加
            var changelogPath = Path.Combine(packagePath, "CHANGELOG.md");
            if (!File.Exists(changelogPath))
            {
                return;
            }

            GUILayout.Label("Changelog Entry", EditorStyles.boldLabel);
            newChangelogEntry = EditorGUILayout.TextArea(newChangelogEntry, GUILayout.Height(100));
        }

        /// <summary>
        /// パスを相対パスに変換します。
        /// </summary>
        /// <param name="absolutePath">絶対パス</param>
        /// <param name="basePath">基準パス</param>
        /// <returns>相対パス</returns>
        public static string GetRelativePath(string absolutePath, string basePath)
        {
            if (string.IsNullOrEmpty(absolutePath) || string.IsNullOrEmpty(basePath))
            {
                return string.Empty;
            }

            Uri absoluteUri = new Uri(absolutePath);
            Uri baseUri = new Uri(basePath);
            Uri relativeUri = baseUri.MakeRelativeUri(absoluteUri);

            return relativeUri.ToString();
        }

        /// <summary>
        /// パッケージファイルを生成します。
        /// </summary>
        private void GeneratePackageFiles()
        {
            if (!Directory.Exists(packagePath))
            {
                Directory.CreateDirectory(packagePath);
            }

            GenerateOrUpdatePackageJson();
            GenerateReadme();
            GenerateOrUpdateChangelog();

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// ChangeLogを生成します。
        /// </summary>
        private void GenerateOrUpdateChangelog()
        {
            var changelogPath = Path.Combine(packagePath, "CHANGELOG.md");
            if (File.Exists(changelogPath))
            {
                InsertNewVersionEntry(changelogPath);
            }
            else
            {
                CreateChangelog(changelogPath);
            }
        }

        private void GenerateOrUpdatePackageJson()
        {
            var packageJsonPath = Path.Combine(packagePath, "package.json");

            if (File.Exists(packageJsonPath))
            {
                UpdatePackageJson(packageJsonPath);
                return;
            }

            CreatePackageJson(packageJsonPath);
        }

        /// <summary>
        /// READMEを生成します。
        /// </summary>
        /// <param name="packageJsonPath">パッケージのパス</param>
        private void CreatePackageJson(string packageJsonPath)
        {
            var packageJsonContent = $@"{{
        ""name"": ""com.{companyName.ToLower()}.{frameworkName.ToLower()}.{packageName.ToLower()}"",
        ""version"": ""{packageVersion}"",
        ""displayName"": ""{frameworkName}.{packageName}"",
        ""author"": ""{packageAuthor}"",
        ""description"": ""{packageDescription}"",
        ""unity"": ""{unityVersion}""
}}";

            File.WriteAllText(packageJsonPath, packageJsonContent);
        }

        /// <summary>
        /// パッケージJsonを生成します。
        /// </summary>
        /// <param name="packageJsonPath">パッケージのパス</param>
        private void UpdatePackageJson(string packageJsonPath)
        {
            var packageJson = JsonUtility.FromJson<PackageJson>(File.ReadAllText(packageJsonPath));

            packageJson.name = $"com.{companyName.ToLower()}.{frameworkName.ToLower()}.{packageName.ToLower()}";
            packageJson.version = packageVersion;
            packageJson.displayName = $"{frameworkName}.{packageName}";
            packageJson.author = packageAuthor;
            packageJson.description = packageDescription;
            packageJson.unity = unityVersion;
            File.WriteAllText(packageJsonPath, JsonUtility.ToJson(packageJson, true));
        }

        /// <summary>
        /// READMEを生成します。
        /// </summary>
        private void GenerateReadme()
        {
            var readmePath = Path.Combine(packagePath, "README.md");

            var readmeContent = @"
# " + packageName + @"

" + packageDescription + @"
";

            File.WriteAllText(readmePath, readmeContent);
        }

        /// <summary>
        /// ChangeLogを生成します。
        /// </summary>
        /// <param name="changelogPath">ChangeLogのパス</param>
        private void CreateChangelog(string changelogPath)
        {
            var changelogContent = @"
# Changelog

## " + packageVersion + @" - " + System.DateTime.Now.ToString("yyyy-MM-dd") + @"

- Initial release
";

            File.WriteAllText(changelogPath, changelogContent);
        }

        /// <summary>
        /// ChangeLogに新しいバージョンのエントリを追加します。
        /// </summary>
        /// <param name="changelogPath">ChangeLogのパス</param>
        private void InsertNewVersionEntry(string changelogPath)
        {
            var newEntry = $"## {packageVersion} - {System.DateTime.Now:yyyy-MM-dd}\n\n";
            var currentContent = File.ReadAllText(changelogPath);
            File.WriteAllText(changelogPath, newEntry + currentContent);
        }
    }


    [Serializable]
    public class PackageJson
    {
        public string name;
        public string displayName;
        public string version;
        public string unity;
        public string description;
        public string author;
    }
}