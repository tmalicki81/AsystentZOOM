using System;
using System.Collections.Generic;

namespace AsystentZOOM.VM.Common
{
    public class UndoRedoManager<T>
        where T : ICloneable
    {
        public class Snapshot<T>
        {
            public Snapshot(T obj) => Object = obj;
            public DateTime Added { get; set; } = DateTime.Now;
            public T Object { get; set; }
        }

        public void ClearSnapshots()
        {
            _snapshots.Clear();
            _currObj = -1;
        }

        public void AddSnapshot(T obj)
        {
            // Jeśli aktualny zrzut nie jest ostanio dodanym => Usuń kolejne
            if (_currObj < _lastObj)
                _snapshots.RemoveRange(_currObj + 1, _lastObj - _currObj);

            // Dodaj zrzut i wskaźnik na niego
            var clonedObj = (T)obj.Clone();
            _snapshots.Add(new Snapshot<T>(clonedObj));
            _currObj++;
        }

        public T GetUndo()
        {
            if (!CanUndo)
                throw new Exception($"Nie można przesunąć się przed pozycję {_currObj}.");
            return _snapshots[--_currObj].Object;
        }

        public T GetRedo()
        {
            if (!CanRedo)
                throw new Exception($"Nie można przesunąć się za pozycję {_currObj}.");
            return _snapshots[++_currObj].Object;
        }

        public bool CanUndo => _currObj > 0;
        public bool CanRedo => _currObj < _lastObj;
        private int _lastObj => _snapshots.Count - 1;

        private int _currObj = -1;
        private readonly List<Snapshot<T>> _snapshots = new();
    }
}
