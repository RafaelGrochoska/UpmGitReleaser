using System;
using System.Collections.Generic;
using System.Linq;


namespace Editor
{
    public class GitInstance : IDisposable
    {
        private readonly CommandLineInstance _process = null;

        public GitInstance()
        {
            _process = new CommandLineInstance("git");
        }

        //git subtree split --rejoin --prefix=Packages/com.crenix.draganddrop --branch upm
        public void CreateSubtreeBranch(string name, string branchName)
        {
            _process.RunCommand($"subtree split --rejoin --prefix=Packages/{name} --branch {branchName}");
        }

        public void PushTags(string branch)
        {
            _process.RunCommand($"push origin {branch} --tags");
        }

        public void Push()
        {
            _process.RunCommand("push origin");
        }

        public void AddFile(string path)
        {
            _process.RunCommand($"add {path}");
        }

        public void Commit(string message)
        {
            _process.RunCommand($"commit -m \"{message}\"");
        }

        public void CreateTag(string tag, string branch)
        {
            _process.RunCommand($"tag {tag} {branch}");
        }

        public IEnumerable<string> GetTags()
        {
            return _process.RunCommand("tag").Split('\n').Where(data => !string.IsNullOrEmpty(data));
        }

        public bool HaveUncommittedChanges()
        {
            return _process.RunCommand("status -s").Split('\n').Any(data => !string.IsNullOrEmpty(data));
        }

        public void Dispose()
        {
            _process.Dispose();
        }
    }
}