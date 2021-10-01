using SimpleReactionMachine;
using System;
using System.Data;

namespace SimpleReactionMachine
{
    public class EnhancedReactionController : IController
    {
        // Settings for the game times

        const double TicksPerSecond = 100.0;
        private const int EndingTime = 500;
        private const int MaxReadyTime = 1000;


        // Instance variables and properties
        private State _state;
        private IGui Gui { get; set; }
        private IRandom Rng { get; set; }
        private int Ticks { get; set; }
        private int Times { get; set; }
        private int TotalTime { get; set; }


        public void Connect(IGui gui, IRandom rng)
        {
            Gui = gui;
            Rng = rng;
            Init();
        }


        public void Init() => _state = new Begin(this);


        public void CoinInserted() => _state.CoinInserted();

        public void GoStopPressed() => _state.GoStopPressed();


        public void Tick() => _state.Tick();


        void SetState(State state) => _state = state;


        public abstract class State
        {
            protected EnhancedReactionController _controller;
            public State(EnhancedReactionController controller) => _controller = controller;
            public abstract void CoinInserted();
            public abstract void GoStopPressed();
            public abstract void Tick();
        }


        public class Begin : State
        {
            public Begin(EnhancedReactionController controller) : base(controller)
            {
                _controller.Times = 0;
                _controller.TotalTime = 0;
                _controller.Gui.SetDisplay("Insert coin");
            }
            public override void CoinInserted() => _controller.SetState(new Ready(_controller));
            public override void GoStopPressed() { }
            public override void Tick() { }
        }


        public class Ready : State
        {
            public Ready(EnhancedReactionController controller) : base(controller)
            {
                _controller.Gui.SetDisplay("Press Go!");
                _controller.Ticks = 0;
            }
            public override void GoStopPressed()
            {
                _controller.SetState(new Wait(_controller));
            }
            public override void Tick()
            {
                _controller.Ticks++;
                if (_controller.Ticks == MaxReadyTime)
                    _controller.SetState(new Begin(_controller));
            }
            public override void CoinInserted() { }
        }


        public class Wait : State
        {
            private int _waitTime;
            public Wait(EnhancedReactionController controller) : base(controller)
            {
                _controller.Gui.SetDisplay("Wait...");
                _controller.Ticks = 0;
                _waitTime = _controller.Rng.GetRandom(100,250);
            }
            public override void GoStopPressed() => _controller.SetState(new Begin(_controller));
            public override void Tick()
            {
                _controller.Ticks++;
                if (_controller.Ticks == _waitTime)
                {
                    _controller.Times++;
                    _controller.SetState(new Run(_controller));
                }
            }
            public override void CoinInserted() { }
        }

        public class Run : State
        {
            public Run(EnhancedReactionController con) : base(con)
            {
                _controller.Gui.SetDisplay("0.00");
                _controller.Ticks = 0;
            }
            
            public override void GoStopPressed()
            {
                _controller.TotalTime += _controller.Ticks;
                _controller.SetState(new End(_controller));
            }
            public override void Tick()
            {
                _controller.Ticks++;
                _controller.Gui.SetDisplay(
                    (_controller.Ticks / TicksPerSecond).ToString("0.00"));
                if (_controller.Ticks == 200)
                    _controller.SetState(new End(_controller));
            }
            public override void CoinInserted() { }
        }

        public class End : State
        {
            public End(EnhancedReactionController con) : base(con)
            {
                _controller.Ticks = 0;
            }
            public override void GoStopPressed() => CheckTimes();
            public override void Tick()
            {
                _controller.Ticks++;
                if (_controller.Ticks == 300)
                    CheckTimes();
            }
            private void CheckTimes()
            {
                if (_controller.Times == 3)
                {
                    _controller.SetState(new Result(_controller));
                    return;
                }
                _controller.SetState(new Wait(_controller));
            }
            public override void CoinInserted() { }
        }


        public class Result : State
        {
            public Result(EnhancedReactionController con) : base(con)
            {
                _controller.Gui.SetDisplay("Average: "
                    + (((double)_controller.TotalTime / _controller.Times) / TicksPerSecond)
                    .ToString("0.00"));
                _controller.Ticks = 0;
            }
            public override void Tick()
            {
                _controller.Ticks++;
                if (_controller.Ticks == EndingTime)
                    _controller.SetState(new Begin(_controller));
            }
            public override void GoStopPressed() => _controller.SetState(new Begin(_controller));
            public override void CoinInserted() { }
            
        }
    }
}