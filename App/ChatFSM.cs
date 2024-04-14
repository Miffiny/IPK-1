using App.Enums;

namespace App
{
    public class ChatFSM
    {
        // Private field to store the current state
        private ChatState _currentState;

        // Public property to access the current state
        public ChatState CurrentState
        {
            get => _currentState;
            set => _currentState = value;
        }

        // Constructor to initialize the FSM with the Start state
        public ChatFSM()
        {
            _currentState = ChatState.Start;
        }

        // Method to transition to a new state
        public void TransitionToState(ChatState newState)
        {
            // If the new state is Error, automatically switch to End
            _currentState = newState == ChatState.Error ? ChatState.End : newState;
        }
    }
}