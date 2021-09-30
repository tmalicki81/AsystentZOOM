namespace AsystentZOOM.GUI.Common.Mouse
{
    public struct MousePoint
    {
        public MousePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }
}
