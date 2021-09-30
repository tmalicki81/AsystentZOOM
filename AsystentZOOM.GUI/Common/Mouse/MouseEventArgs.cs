using System;
using System.Windows;

namespace AsystentZOOM.GUI.Common.Mouse
{
    [Serializable]
    public class MouseEventArgs
    {
        public DateTime EventTime { get; set; }

        public MouseEventArgs() { }

        public MouseEventArgs(MouseEventEnum mouseEvent, int x, int y)
            : this()
        {
            EventTime = DateTime.Now;
            MouseEvent = mouseEvent;
            X = x;
            Y = y;
        }

        public MouseEventEnum MouseEvent { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Point Location => new(X, Y);

        public override string ToString()
            => $"Zdarzenie: {MouseEvent}, Pozycja: {X}, {Y}";
    }
}
