using System.IO;
using UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Assets;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UnityBuildTooling.Editor.build_tooling.Scripts.Runtime.Utils
{
    [InitializeOnLoad]
    internal static class UnityTesting
    {
        private const string GroupFileName = "group.dat";
        private const string BaseFileName = "base.dat";
        
        private static TestRunnerApi _testRunnerApi;
        private static readonly CallbackHandler _callbackHandler = new CallbackHandler();

        static UnityTesting()
        {
            InitTest();
        }

        private static void InitTest()
        {
            if (_testRunnerApi == null)
            {
                _testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
                _testRunnerApi.RegisterCallbacks(_callbackHandler);
            }
        }

        public static void RunTests(BuildingGroup @group)
        {
            Debug.Log("Start tests");

            StoreGroup(@group);
            InitTest();

            _testRunnerApi.Execute(new ExecutionSettings(new Filter { testMode = TestMode.PlayMode }));
        }
        
        public static void RunTests(UnityBuilding.BuildBehavior behavior, BuildingData data)
        {
            Debug.Log("Start tests");

            StoreBase(behavior, data);
            InitTest();

            _testRunnerApi.Execute(new ExecutionSettings(new Filter { testMode = TestMode.PlayMode }));
        }

        private static void StoreGroup(BuildingGroup @group)
        {
            var fileName = Path.GetTempPath() + "/" + GroupFileName;
            using var stream = new FileStream(fileName, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.WriteLine(JsonUtility.ToJson(@group));
        }

        private static bool LoadGroup(out BuildingGroup @group)
        {
            @group = null;
            var fileName = Path.GetTempPath() + "/" + GroupFileName;
            
            if (!File.Exists(fileName))
                return false;

            try
            {
                using var stream = new FileStream(fileName, FileMode.Open);
                using var reader = new StreamReader(stream);
                @group = JsonUtility.FromJson<BuildingGroup>(reader.ReadLine());
            }
            finally
            {
                File.Delete(fileName);
            }

            return true;
        }

        private static void StoreBase(UnityBuilding.BuildBehavior behavior, BuildingData overwriteData)
        {
            var fileName = Path.GetTempPath() + "/" + BaseFileName;
            using var stream = new FileStream(fileName, FileMode.Create);
            using var writer = new StreamWriter(stream);
            writer.WriteLine(JsonUtility.ToJson(behavior));
            writer.WriteLine(JsonUtility.ToJson(overwriteData));
        }

        private static bool LoadBase(out UnityBuilding.BuildBehavior behavior, out BuildingData overwriteData)
        {
            behavior = UnityBuilding.BuildBehavior.BuildOnly;
            overwriteData = null;
            
            var fileName = Path.GetTempPath() + "/" + BaseFileName;
            
            if (!File.Exists(fileName))
                return false;

            try
            {
                using var stream = new FileStream(fileName, FileMode.Open);
                using var reader = new StreamReader(stream);
                behavior = JsonUtility.FromJson<UnityBuilding.BuildBehavior>(reader.ReadLine());
                overwriteData = JsonUtility.FromJson<BuildingData>(reader.ReadLine());
            }
            finally
            {
                File.Delete(fileName);
            }

            return true;
        }

        private sealed class CallbackHandler : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                EditorUtility.DisplayProgressBar("Run Tests", "Test is running now", -1f);
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                EditorUtility.ClearProgressBar();

                Debug.Log("Finished test with success: " + result.PassCount + ", skipped: " + result.SkipCount + ", failed: " + result.FailCount);
                if (result.TestStatus == TestStatus.Failed)
                {
                    EditorUtility.DisplayDialog("Test failures", "There are test failures: " + result.TestStatus, "OK");
                    return;
                }

                if (LoadGroup(out var @group))
                {
                    UnityBuilding.Build(@group, false);
                }
                else if (LoadBase(out var behavior, out var overwriteData))
                {
                    UnityBuilding.Build(behavior, overwriteData, false);
                }
                else
                {
                    Debug.LogError("Unable to find base or group file for build execution!");
                }
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }
        }
    }
}