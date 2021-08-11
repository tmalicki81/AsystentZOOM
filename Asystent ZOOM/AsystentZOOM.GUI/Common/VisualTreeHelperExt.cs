using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace AsystentZOOM.GUI.Common
{
    public static class VisualTreeHelperExt
    {
        public static DependencyObject GetParent(this DependencyObject dependencyObject, Func<DependencyObject, bool> predicate)
        {
            DependencyObject tmpDependencyObject = dependencyObject;
            while (tmpDependencyObject != null)
            {
                if (predicate(tmpDependencyObject))
                    return tmpDependencyObject;
                tmpDependencyObject = VisualTreeHelper.GetParent(tmpDependencyObject);
            }
            return null;
        }

        public static TParent GetParent<TParent>(this DependencyObject dependencyObject)
            where TParent : DependencyObject
            => (TParent)GetParent(dependencyObject, obj => obj is TParent);

        private static void CollectDependencyObjectChilds(DependencyObject dependencyObject, List<DependencyObject> dependencyObjectList)
        {
            if (dependencyObject == null)
                return;

            dependencyObjectList.Add(dependencyObject);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                CollectDependencyObjectChilds(child, dependencyObjectList);
            }
        }

        public static DependencyObject GetChild(this DependencyObject dependencyObject, Func<DependencyObject, bool> predicate)
        {
            if (dependencyObject == null)
                return null;
            var dependencyObjectList = new List<DependencyObject>();
            CollectDependencyObjectChilds(dependencyObject, dependencyObjectList);
            return dependencyObjectList.FirstOrDefault(predicate);
        }

        public static TChild GetChild<TChild>(this DependencyObject dependencyObject)
            where TChild : DependencyObject
            => (TChild)GetChild(dependencyObject, obj => obj is TChild);
    }
}