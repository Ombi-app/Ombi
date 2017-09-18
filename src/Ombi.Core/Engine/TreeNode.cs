using System.Collections.Generic;

namespace Ombi.Core.Engine
{

    public class TreeNode<T>
    {
        public string Label { get; set; }
        public T Data { get; set; }
        public List<TreeNode<T>> Children { get; set; }
        public bool Leaf { get; set; }
        public bool Expanded { get; set; }
    }
}