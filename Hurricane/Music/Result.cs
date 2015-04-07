namespace Hurricane.Music
{
    public class Result
    {
        public State State { get; set; }
        public object CustomState { get; set; }

        public Result(State state) : this(state, null)
        {
        }

        public Result(State state, object customState)
        {
            State = state;
            CustomState = customState;
        }
    }

    public enum State
    {
        True,
        False,
        Exception
    }
}
