namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Tree
    {
        public static IEnumerable<TResult> ExpandSubTree<TNode, TResult>(this TNode root, Func<TNode, IEnumerable<TNode>> getChildren, Func<TNode, TResult> valueSelector)
        {
            var children = getChildren(root) ?? new TNode[0];

            foreach (var child in children)
            {
                yield return valueSelector(child);

                foreach (var r in child.ExpandSubTree(getChildren, valueSelector))
                {
                    yield return r;
                }
            }
        }

        public static IEnumerable<TResult> ExpandTree<TNode, TResult>(this TNode root, Func<TNode, IEnumerable<TNode>> getChildren, Func<TNode, TResult> valueSelector)
        {
            yield return valueSelector(root);
	    
            var children = getChildren(root) ?? new TNode[0];

            foreach (var child in children) 
            {
                foreach (var r in child.ExpandTree(getChildren, valueSelector))
                {
                    yield return r;
                }
            }
        }
    }
}
