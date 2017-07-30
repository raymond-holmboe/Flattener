namespace Flattener
{
    public class FlattenerMapping
    {
        public FlattenerMapping(string source, int destination)
        {
            Source = source;
            Destination = destination;
        }
        public string Source { get; set; }
        public int Destination { get; set; }
        public string Static { get; set; }
    }
}
