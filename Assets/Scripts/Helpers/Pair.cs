namespace Helpers
{
    public class Pair<T1, T2>
    {
        public T1 Left { get; set; }
        public T2 Right { get; set; }

        public Pair(T1 left, T2 right)
        {
            this.Left = left;
            this.Right = right;
        }
    }
}