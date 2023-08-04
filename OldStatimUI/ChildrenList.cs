using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public class ChildList : IList<Component>
    {
        private readonly List<Component> children = new();

        public event EventHandler<Component>? OnChildAdded;

        public Component this[int index] { get => children[index]; set => children[index] = value; }

        public int Count => children.Count;

        public bool IsReadOnly => false;

        public void Add(Component item)
        {
            children.Add(item);
            OnChildAdded?.Invoke(this, item);
        }

        public void Clear()
        {
            children.Clear();
        }

        public void AddRange(IEnumerable<Component> components)
        {
            foreach (var component in components)
            {
                children.Add(component);
                OnChildAdded?.Invoke(this, component);
            }
        }

        public bool Contains(Component item)
        {
            return children.Contains(item);
        }

        public void CopyTo(Component[] array, int arrayIndex)
        {
            children.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Component> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        public int IndexOf(Component item)
        {
            return children.IndexOf(item);
        }

        public void Insert(int index, Component item)
        {
            children.Insert(index, item);
            OnChildAdded?.Invoke(this, item);
        }

        public bool Remove(Component item)
        {
            return children.Remove(item);
        }

        public void RemoveAt(int index)
        {
            children.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return children.GetEnumerator();
        }
    }
}
