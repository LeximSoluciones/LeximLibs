using System;
using System.Reflection;

namespace Lexim.Data
{
    public class NhibernateConfig
    {
        public string ConnectionString { get; set; }
        public bool UpdateSchema { get; set; }
        public bool CommitUpdates { get; set; }
        public Assembly MappingsAssembly { get; set; }
        public bool AutoMap { get; set; }
        public Func<Type, bool> AutoMappingFilter { get; set; }
        public string ScriptsPath { get; set; }
        public bool UseNumericEnums { get; set; }
    }
}