using System;
using System.Collections.Generic;

namespace AsystentZOOM.VM.Common
{
    public class UndoRedoManager<T>
        where T : BaseVM, ICloneable
    {
        public class Snapshot<T>
        {
            public Snapshot(T obj, string description)
            {
                Value = obj;
                Description = description;
            }
            public DateTime Added { get; set; } = DateTime.Now;
            public string Description { get; set; }
            public T Value { get; set; }
        }

        public void ClearSnapshots()
        {
            _snapshots.Clear();
            _currObj = -1;
        }

        public bool AddSnapshot(T obj, string description)
        {
            obj.IsDataReady = false;
            try
            {
                // Jeśli aktualny zrzut nie jest ostanio dodanym => Usuń kolejne
                if (_currObj < _lastObj)
                    _snapshots.RemoveRange(_currObj + 1, _lastObj - _currObj);

                // Dodaj zrzut i wskaźnik na niego
                var clonedObj = (T)obj.Clone();

                if (CanUndo)
                {
                    var prevObj = _snapshots[_currObj].Value;
                    if (SingletonVMFactory.EqualsVM(clonedObj, prevObj))
                        return false;
                }
                _snapshots.Add(new Snapshot<T>(clonedObj, description));
                _currObj++;
                return true;
            }
            finally
            {
                obj.IsDataReady = true;
            }
        }

        public Snapshot<T> GetUndo()
        {
            if (!CanUndo)
                throw new Exception($"Nie można przesunąć się przed pozycję {_currObj}.");
            return _snapshots[--_currObj];
        }

        public Snapshot<T> GetRedo()
        {
            if (!CanRedo)
                throw new Exception($"Nie można przesunąć się za pozycję {_currObj}.");
            return _snapshots[++_currObj];
        }

        public bool CanUndo => _currObj > 0;
        public bool CanRedo => _currObj < _lastObj;
        private int _lastObj => _snapshots.Count - 1;

        private int _currObj = -1;
        private readonly List<Snapshot<T>> _snapshots = new();
    }
}
