using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSolution
{
    public static class Solutions
    {
        private static IEnumerable<Solution> solutions;

        public static void LoadSolutions(string path)
        {
            var files = Directory.GetFiles(path, "*.sln", SearchOption.AllDirectories);

            solutions = files.Select(f =>
                {
                    var info = new FileInfo(f);
                    return new Solution { Name = info.Name, Path = info.FullName };
                })
                .OrderBy(s => s.Name).ToList();
        }

        public static IEnumerable<Solution> Get(string filter = "")
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return solutions;
            }

            return solutions.Where(s => s.Name.IndexOf(filter, 0, StringComparison.CurrentCultureIgnoreCase) != -1);
        }
    }
}
