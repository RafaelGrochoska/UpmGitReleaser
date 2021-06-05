using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    internal class PackageReleaserWindow : EditorWindow
    {
        public struct Validation
        {
            public bool error;
            public List<string> message;
        }

        private ReleaseType _releaseType = ReleaseType.Release;
        private GitInstance _git = null;
        private string _selectedPackage = "";
        private bool _uncommittedChanges = false;
        private IEnumerable<string> _releasedVersions = null;

        private string _latestVersion = "";
        private string _desiredVersion = "";

        private Action<Validation> _onErrorUpdate = delegate { };
        private Action _onInfoUpdate = delegate { };

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Package Releaser", priority = 1500)]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow<PackageReleaserWindow>();
            window.titleContent = new GUIContent("UPM Git Releaser", null, "Release packages trough git");
            window.minSize = new Vector2(200, 300);
            window.Show();
        }

        private void OnEnable()
        {
            _git = new GitInstance();

            // Reference to the root of the window.
            var root = rootVisualElement;

            var choices = Packages.FetchLocal();
            _selectedPackage = choices[0];

            var packageSelector = new PopupField<string>("Package", choices, 0);
            var versionInfo = new HelpBox("Version info", HelpBoxMessageType.Info);
            var newVersion = new TextField("New version");
            var branchName = new TextField("Branch name") {value = "upm"};
            var releaseType = new EnumField("Release type", ReleaseType.Release);
            var releaseButton = new Button {text = "Release"};
            var error = new HelpBox("", HelpBoxMessageType.Error);
            releaseType.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                _releaseType = (ReleaseType) evt.newValue;
                _onInfoUpdate?.Invoke();
                UpdateErrors();
            });

            packageSelector.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                _selectedPackage = evt.newValue;
                GetUpdatedPackageInfo();
                UpdateErrors();
            });

            newVersion.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                _desiredVersion = evt.newValue;
                _onInfoUpdate?.Invoke();
                UpdateErrors();
            });

            releaseButton.clicked += () =>
            {
                Packages.UpdateVersion(_selectedPackage, GetFullReleaseVersion());
                if (_git.HaveUncommittedChanges())
                {
                    _git.AddFile(Path.Combine("Packages", _selectedPackage, "package.json"));
                    _git.Commit($"Bump version {GetFullReleaseVersion()}");
                }

                _git.CreateSubtreeBranch(_selectedPackage, branchName.value);
                _git.CreateTag(GetFullReleaseVersion(), branchName.value);
                _git.PushTags(branchName.value);
                _git.Push();
            };

            root.Add(packageSelector);
            root.Add(versionInfo);
            root.Add(releaseType);
            root.Add(branchName);
            root.Add(newVersion);
            root.Add(error);
            root.Add(releaseButton);

            _onInfoUpdate += () =>
            {
                versionInfo.text =
                    $"This package is currently into version {_latestVersion}, the new version will be {GetFullReleaseVersion()}";
            };

            _onErrorUpdate += validation =>
            {
                error.SetVisibility(validation.error);
                error.text = string.Join("\n", validation.message);
                releaseButton.SetEnabled(!validation.error);
            };

            GetUpdatedPackageInfo();
            UpdateErrors();
        }

        private void GetUpdatedPackageInfo()
        {
            var packageInfo = Packages.RetrieveInfo(_selectedPackage);

            _latestVersion = packageInfo.version;

            _releasedVersions = _git.GetTags();
            _uncommittedChanges = _git.HaveUncommittedChanges();
            _onInfoUpdate?.Invoke();
        }

        private void UpdateErrors()
        {
            var validation = new Validation
            {
                error = false,
                message = new List<string>()
            };

            if (_releaseType != ReleaseType.Release && _desiredVersion.Split('_').Length < 2)
            {
                validation.error = true;
                validation.message.Add("Non Release versions must have subversion MAJOR.MINOR.PATCH_X");
            }

            if (_releasedVersions.Contains(_desiredVersion))
            {
                validation.error = true;
                validation.message.Add("Version already exists.");
            }

            if (_uncommittedChanges)
            {
                validation.error = true;
                validation.message.Add("You have uncommitted changes.");
            }

            _onErrorUpdate?.Invoke(validation);
        }

        public string GetFullReleaseVersion()
        {
            var version = _desiredVersion.Split('_');
            if (_releaseType != ReleaseType.Release && version.Length < 2)
            {
                return _desiredVersion;
            }

            switch (_releaseType)
            {
                case ReleaseType.Release:
                    return _desiredVersion;
                case ReleaseType.Beta:
                case ReleaseType.Alpha:
                case ReleaseType.Experimental:
                    return $"{version[0]}-{_releaseType.ToString().ToLower()}.{version[1]}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}